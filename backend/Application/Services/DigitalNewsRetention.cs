using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Application.Services;

public static class DigitalNewsRetention
{
    public static IQueryable<DigitalNewsItem> Visible(IQueryable<DigitalNewsItem> items, AppDbContext db, DateTime nowUtc)
    {
        var cutoff = nowUtc.AddDays(-3);
        return items.Where(item => item.ImportedAt > cutoff
            || !db.DigitalPlanReads.Any(read => read.ItemId == item.Id && read.Status == "NotRelevant")
            || db.DigitalPlanReads.Any(read => read.ItemId == item.Id && read.Status != "NotRelevant"));
    }
}
