using Cdsqg.Application.DTOs;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

// View scope and reporting permission deliberately have separate contracts.
public sealed record AgencyViewScope(HashSet<Guid> AgencyIds, bool IsParentAgency)
{
    public bool CanView(PlanningGridItemDto item)
    {
        return item.LeadAssignmentAgencyIds.Any(AgencyIds.Contains) || item.CoordinatingAssignmentAgencyIds.Any(AgencyIds.Contains);

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
        => db.AgencyTaskExecutions.AnyAsync(e => e.GoalTaskId == taskId && e.IsLeadAssignment
            && (e.AgencyId == agencyId || db.Agencies.Any(a => a.Id == e.AgencyId && a.ParentId == agencyId)));
}
