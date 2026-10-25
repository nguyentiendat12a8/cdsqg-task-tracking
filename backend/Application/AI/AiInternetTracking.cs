using System.Text.Json;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Cdsqg.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.AI;

public static class AiInternetTracking
{
    public static string[] Keys(string? json)
    {
        var keys = JsonSerializer.Deserialize<string[]>(string.IsNullOrWhiteSpace(json)?"[]":json) ?? [];
        return keys.Length == 0 ? AiMetricCatalog.Metrics.Select(m => m.Key).ToArray() : keys.Distinct().ToArray();
    }
    public static Guid[] AgencyIds(string? json) => (JsonSerializer.Deserialize<Guid[]>(string.IsNullOrWhiteSpace(json)?"[]":json) ?? []).Distinct().ToArray();

    // Keyword settings are the document-wide catalog. RSS sources never narrow goals or agencies.
    public static async Task<AiMetric[]> ContextAsync(AppDbContext db,AiAlertFeed? feed,CancellationToken ct)
    {
        if(feed==null)return AiMetricCatalog.Metrics.Select(m=>m with{Internet=true}).ToArray();
        var goals=await db.GoalTaskItems.AsNoTracking().Where(g=>g.DocumentId==feed.DocumentId&&g.ItemType==ItemTypeEnum.Goal&&g.EvaluationType==EvaluationTypeEnum.Quantitative&&db.AiGoalKeywords.Any(k=>k.DocumentId==g.DocumentId&&k.GoalCode==g.Code)).OrderBy(g=>g.Code).Select(g=>g.Id).ToListAsync(ct);
        var result=new List<AiMetric>();
        foreach(var id in goals)result.Add((await AiMetricCatalog.ResolveAsync(db,"GOAL_"+id,ct)) with{Internet=true});
        return result.ToArray();
    }
    public static async Task<AiMetric[]> ResolveAsync(AppDbContext db,AiAlertFeed? feed,CancellationToken ct,string[]? selectedKeys=null)
    {
        var context=await ContextAsync(db,feed,ct);
        var result=new List<AiMetric>();
        foreach(var metric in context.Where(m=>selectedKeys==null||selectedKeys.Contains(m.Key)||AiMetricCatalog.Metrics.Any(legacy=>selectedKeys.Contains(legacy.Key)&&legacy.GoalCodes.Intersect(m.GoalCodes).Any()&&feed?.DocumentId==AiDocumentScope.PriorityDocument)))
        {
            if(!metric.Key.StartsWith("GOAL_")){result.Add(metric);continue;}
            var goalId=Guid.Parse(metric.Key[5..]);
            var goal=await db.GoalTaskItems.AsNoTracking().SingleAsync(g=>g.Id==goalId,ct);
            var national=goal.DocumentId==AiDocumentScope.PriorityDocument&&AiMetricCatalog.Metrics.Any(m=>m.GoalCodes.Contains(goal.Code));
            if(national){var definition=AiMetricCatalog.Metrics.Single(m=>m.GoalCodes.Contains(goal.Code)).Definition;result.Add(metric with{Definition=definition,ExtractionPrompt=definition+" "+metric.ExtractionPrompt});continue;}
            var recipients=db.AgencyTaskExecutions.Where(e=>e.GoalTaskId==goalId&&e.IsLeadAssignment).Select(e=>e.AgencyId);
            var agencies=await db.Agencies.AsNoTracking().Where(a=>a.IsActive&&recipients.Contains(a.Id)&&a.Type!=AgencyTypeEnum.Special).ToListAsync(ct);
            var scopes=agencies.Where(a=>!ReportingAgencyGroups.IsGroup(a)).Select(a=>new AiMetricScope(a.Id,ProvinceCatalog.DisplayName(a),new[]{a.Name,ProvinceCatalog.DisplayName(a)}.Distinct().ToArray())).ToArray();
            if(scopes.Length==0)throw new InvalidOperationException($"{goal.Code}: chưa có đơn vị chủ trì được phân giao để đối chiếu số liệu.");
            result.Add(metric with{Scopes=scopes,Definition=metric.Definition.Replace("phạm vi Việt Nam","phạm vi đơn vị được nhắc trong bài"),ExtractionPrompt=metric.ExtractionPrompt.Replace("phạm vi Việt Nam","phạm vi đơn vị được nhắc trong bài")});
        }
        return result.ToArray();
    }
}
