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
        => role == "Any" ? query.Where(i => i.AgencyExecutions.Any(e => (e.IsLeadAssignment || e.IsCoordinatingAssignment) && ids.Contains(e.AgencyId))) : Coordinating(role)
            ? query.Where(i => i.AgencyExecutions.Any(e => e.IsCoordinatingAssignment && ids.Contains(e.AgencyId)))
            : query.Where(i => i.AgencyExecutions.Any(e => e.IsLeadAssignment && ids.Contains(e.AgencyId)));

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
