using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Application.AI;
// Document membership is resolved from goal IDs, never from overlapping MT codes.
public static class AiDocumentScope
{
    public static readonly Guid PriorityDocument = Guid.Parse("12660000-0000-0000-0000-000000001266");
    public static bool IsAll(string key)=>key=="ALL_GOALS"||(key.StartsWith("ALL_GOALS_")&&Guid.TryParse(key[10..],out _));
    public static Guid AllDocument(string key)=>key=="ALL_GOALS"?PriorityDocument:Guid.Parse(key[10..]);
    public static async Task<string[]> KeysAsync(AppDbContext db,Guid documentId,CancellationToken ct)
    {
        var ids=await db.GoalTaskItems.Where(g=>g.DocumentId==documentId&&g.ItemType==ItemTypeEnum.Goal).Select(g=>g.Id).ToListAsync(ct);
        return ids.Select(id=>"GOAL_"+id).Concat(documentId==PriorityDocument?AiMetricCatalog.Metrics.Select(m=>m.Key):[]).ToArray();
    }
    public static Task<IQueryable<AiAlertFeed>> FeedsAsync(AppDbContext db,Guid documentId,CancellationToken ct)
        =>Task.FromResult(db.AiAlertFeeds.Where(f=>f.DocumentId==documentId));
    public static async Task<IQueryable<AiInboxItem>> InboxAsync(AppDbContext db,Guid documentId,CancellationToken ct)
    {
        var keys=(await KeysAsync(db,documentId,ct)).Concat(new[]{"ALL_GOALS_"+documentId}).Concat(documentId==PriorityDocument?new[]{"ALL_GOALS"}:Array.Empty<string>()).Select(k=>"\""+k+"\"").ToArray();
        var feeds=await FeedsAsync(db,documentId,ct);
        return db.AiInboxItems.Where(i=>keys.Any(k=>i.MetricKeysJson.Contains(k))||(i.StorageKey==""&&feeds.Any(f=>f.Id==i.AlertFeedId)));
    }
}
