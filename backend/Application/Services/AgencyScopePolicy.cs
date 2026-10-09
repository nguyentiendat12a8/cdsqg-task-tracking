using Cdsqg.Application.DTOs;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

// View scope and reporting permission deliberately have separate contracts.
public sealed record AgencyViewScope(HashSet<Guid> AgencyIds, bool IsParentAgency)
{
    public bool CanView(PlanningGridItemDto item)
    {
        var code = item.LeadAgencyCode;
        var general = IsParentAgency && (item.IsGeneralTask || code?.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase) == true)
            && (code == null || code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase));
        return general || AgencyIds.Contains(item.LeadAgencyId)
            || item.AssignedAgencyId.HasValue && AgencyIds.Contains(item.AssignedAgencyId.Value)
            || item.CoordinatingAgencyIds?.Any(AgencyIds.Contains) == true;
    }
}
public static class AgencyScopePolicy
{
    public static async Task<AgencyViewScope> ResolveViewScopeAsync(AppDbContext db, Guid agencyId)
    {
        var agency = await db.Agencies.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agencyId);
        var isParent = agency == null || !agency.ParentId.HasValue || agency.ParentId == Guid.Empty;
        var ids = new HashSet<Guid> { agencyId };
        if (isParent) ids.UnionWith(await db.Agencies.Where(a => a.ParentId == agencyId).Select(a => a.Id).ToListAsync());
        return new(ids, isParent);
    }
    public static Task<bool> CanReportAsync(AppDbContext db, Guid taskId, Guid agencyId)
        => db.GoalTaskItems.AnyAsync(t => t.Id == taskId &&
            (t.LeadAgencyId == agencyId || t.AssignedAgencyId == agencyId
             || (t.IsGeneralTask && db.Agencies.Any(a => a.Id == agencyId && a.ParentId == null))
             || db.Agencies.Any(a => a.ParentId == agencyId && (a.Id == t.LeadAgencyId || a.Id == t.AssignedAgencyId))));
}
