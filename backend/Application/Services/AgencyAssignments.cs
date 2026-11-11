using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

// Persist recipients on writes. Reads never reconstruct group membership or create assignments.
public static class AgencyAssignments
{
    public static bool Coordinating(string? role) => string.Equals(role, "Coordinating", StringComparison.OrdinalIgnoreCase);
    public static bool HasRole(AgencyTaskExecution row, string? role = null) => role == "Any" ? row.IsLeadAssignment || row.IsCoordinatingAssignment : Coordinating(role) ? row.IsCoordinatingAssignment : row.IsLeadAssignment;
    public static bool Matches(GoalTaskItem item, IEnumerable<Guid> ids, string? role = null)
    {
        var set = ids as ISet<Guid> ?? ids.ToHashSet();
        return item.AgencyExecutions.Any(e => HasRole(e, role) && set.Contains(e.AgencyId));
    }
    public static IQueryable<GoalTaskItem> Filter(IQueryable<GoalTaskItem> query, ICollection<Guid> ids, string? role = null)
    {
        if (role == "Any")
            return query.Where(i => ids.Contains(i.LeadAgencyId) || i.AgencyExecutions.Any(e => (e.IsLeadAssignment || e.IsCoordinatingAssignment) && ids.Contains(e.AgencyId)));
        if (Coordinating(role))
            return query.Where(i => i.AgencyExecutions.Any(e => e.IsCoordinatingAssignment && ids.Contains(e.AgencyId)));
        return query.Where(i => ids.Contains(i.LeadAgencyId) || i.AgencyExecutions.Any(e => e.IsLeadAssignment && ids.Contains(e.AgencyId)));
    }

    public static List<Guid> ExpandSelection(IEnumerable<Guid> ids, IReadOnlyCollection<Agency> agencies, bool descendants = false)
    {
        var result = ids.ToHashSet();
        foreach (var agency in agencies.Where(a => result.Contains(a.Id)).ToList())
            if (ReportingAgencyGroups.IsGroup(agency)) foreach (var member in ReportingAgencyGroups.Members(agency, agencies)) result.Add(member.Id);
        if (descendants)
        {
            bool changed;
            do { changed = false; foreach (var agency in agencies) if (agency.ParentId.HasValue && result.Contains(agency.ParentId.Value)) changed |= result.Add(agency.Id); } while (changed);
        }
        return result.ToList();
    }

    /// <summary>
    /// Cơ quan có báo cáo đại diện cho tiến độ chung của mục riêng (không phải nhiệm vụ chung):
    /// cơ quan đồng chủ trì, nếu không có thì đơn vị trực thuộc đầu mối, cuối cùng là cơ quan chủ trì.
    /// </summary>
    public static Guid PrimaryAgencyId(GoalTaskItem item)
    {
        if (item.AssignedAgencyId.HasValue) return item.AssignedAgencyId.Value;
        // Nhóm đơn vị (ví dụ "Tất cả cơ quan, đơn vị thuộc Bộ") không có dòng phân giao riêng nên không đại diện báo cáo.
        var leadRows = item.AgencyExecutions.Where(e => e.IsLeadAssignment).Select(e => e.AgencyId).ToHashSet();
        foreach (var id in item.SubAgencyIds)
            if (id != Guid.Empty && (leadRows.Count == 0 || leadRows.Contains(id))) return id;
        return item.LeadAgencyId;
    }

    /// <summary>
    /// Ghi danh sách đơn vị trực thuộc (Cấp 3). Mỗi đơn vị phải tồn tại và có cơ quan cấp trên.
    /// Giá trị cũ trong AssignedAgencyId mà là đơn vị trực thuộc được chuyển sang danh sách mới.
    /// </summary>
    public static async Task SetSubAgenciesAsync(AppDbContext db, GoalTaskItem item, IEnumerable<Guid> ids)
    {
        var list = ids.Where(id => id != Guid.Empty).Distinct().ToList();
        var found = await db.Agencies.AsNoTracking().Where(a => list.Contains(a.Id)).ToListAsync();
        if (found.Count != list.Count) throw new InvalidOperationException("Không tìm thấy đơn vị trực thuộc được chọn.");
        if (found.Any(a => !a.ParentId.HasValue)) throw new InvalidOperationException("Đơn vị được chọn phải là đơn vị trực thuộc (Cấp 3).");
        if (item.AssignedAgencyId is Guid legacy && await db.Agencies.AnyAsync(a => a.Id == legacy && a.ParentId != null))
            item.AssignedAgencyId = null;
        // Đơn vị cụ thể đứng trước nhóm đơn vị để đơn vị đầu mối luôn là đơn vị có thể báo cáo.
        var groups = found.Where(ReportingAgencyGroups.IsGroup).Select(a => a.Id).ToHashSet();
        item.SubAgencyIds = list.Where(id => !groups.Contains(id)).Concat(list.Where(groups.Contains)).ToList();
    }

    /// <summary>Tách giá trị AssignedAgencyId gửi từ form: đơn vị trực thuộc thì đưa vào danh sách đơn vị trực thuộc.</summary>
    public static async Task<(Guid? CoLead, List<Guid> SubAgencies)> SplitAssignedAsync(AppDbContext db, Guid? assigned, IEnumerable<Guid>? subAgencyIds)
    {
        var subs = (subAgencyIds ?? Enumerable.Empty<Guid>()).ToList();
        if (assigned is Guid id && id != Guid.Empty && await db.Agencies.AnyAsync(a => a.Id == id && a.ParentId != null))
        {
            if (!subs.Contains(id)) subs.Insert(0, id);
            return (null, subs);
        }
        return (assigned == Guid.Empty ? null : assigned, subs);
    }

    // Seed/import repair only: never rebuild an existing assignment snapshot.
    public static async Task<int> InitializeMissingAsync(AppDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var missing = await db.GoalTaskItems.Where(i => !i.AgencyExecutions.Any()).ToListAsync();
        foreach (var item in missing) await SyncAsync(db, item);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return missing.Count;
    }

    public static async Task SyncAsync(AppDbContext db, GoalTaskItem item, bool refreshLead = true, bool refreshCoordination = true)
    {
        var agencies = await db.Agencies.ToListAsync();
        var rows = await db.AgencyTaskExecutions.Where(e => e.GoalTaskId == item.Id).ToListAsync();
        var leadIds = refreshLead ? new HashSet<Guid>() : rows.Where(e => e.IsLeadAssignment).Select(e => e.AgencyId).ToHashSet();
        var coordinationIds = refreshCoordination ? new HashSet<Guid>() : rows.Where(e => e.IsCoordinatingAssignment).Select(e => e.AgencyId).ToHashSet();
        var leadGroups = refreshLead ? new HashSet<Guid>() : rows.Where(e => e.IsLeadGroupAssignment).Select(e => e.AgencyId).ToHashSet();
        var coordinationGroups = refreshCoordination ? new HashSet<Guid>() : rows.Where(e => e.IsCoordinatingGroupAssignment).Select(e => e.AgencyId).ToHashSet();
        void Expand(Guid id, HashSet<Guid> target, HashSet<Guid> groups)
        {
            var source = agencies.FirstOrDefault(a => a.Id == id) ?? throw new InvalidOperationException("Không tìm thấy cơ quan được giao.");
            if (ReportingAgencyGroups.IsGroup(source))
            {
                foreach (var member in ReportingAgencyGroups.Members(source, agencies)) { target.Add(member.Id); groups.Add(member.Id); }
            }
            else target.Add(source.Id);
        }
        if (refreshLead)
        {
            item.IsGeneralTask = agencies.FirstOrDefault(a => a.Id == item.LeadAgencyId) is { } lead && ReportingAgencyGroups.IsGroup(lead);
            Expand(item.LeadAgencyId, leadIds, leadGroups);
            if (item.AssignedAgencyId.HasValue) Expand(item.AssignedAgencyId.Value, leadIds, leadGroups);
            foreach (var id in item.SubAgencyIds.Distinct()) Expand(id, leadIds, leadGroups);
        }
        if (refreshCoordination) foreach (var id in item.CoordinatingAgencyIds.Distinct()) Expand(id, coordinationIds, coordinationGroups);
        foreach (var id in leadIds.Union(coordinationIds))
        {
            if (rows.Any(e => e.AgencyId == id)) continue;
            var row = new AgencyTaskExecution { GoalTaskId = item.Id, AgencyId = id, AssignedAt = DateTime.UtcNow };
            rows.Add(row); db.AgencyTaskExecutions.Add(row);
        }
        foreach (var row in rows)
        {
            var lead = leadIds.Contains(row.AgencyId); var coordination = coordinationIds.Contains(row.AgencyId);
            if ((lead || coordination) && !row.IsLeadAssignment && !row.IsCoordinatingAssignment) row.AssignedAt = DateTime.UtcNow;
            row.IsLeadAssignment = lead; row.IsCoordinatingAssignment = coordination;
            row.IsLeadGroupAssignment = leadGroups.Contains(row.AgencyId);
            row.IsCoordinatingGroupAssignment = coordinationGroups.Contains(row.AgencyId);
            // Reports, approval decisions, attachments and cached progress are deliberately untouched.
        }
    }
}
