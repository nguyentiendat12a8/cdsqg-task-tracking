using System.Security.Claims;
using System.Text.Json;
using Cdsqg.Application.AI;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cdsqg.Api.Controllers;
[ApiController,Route("api/ai-data"),Authorize(Roles="Admin")]
public sealed class AiDataController(AppDbContext db,AiReviewService reviews,AiSettingsStore savedSettings):ControllerBase
{
    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken ct)
    {
        var codes=AiMetricCatalog.Metrics.SelectMany(m=>m.GoalCodes).ToArray();
        var goals=await db.GoalTaskItems.AsNoTracking().Where(g=>g.ItemType==ItemTypeEnum.Goal && codes.Contains(g.Code)).Select(g=>new{g.Id,g.Code,g.Title}).ToListAsync(ct);
        var options=await savedSettings.GetAsync(ct);
        var metrics=AiMetricCatalog.Metrics.Select(m=>m with{GoalCodes=m.GoalCodes.Where(code=>goals.Any(g=>g.Code==code)).ToArray()});
        var uploadGoals=await db.GoalTaskItems.Include(g=>g.Unit).AsNoTracking().Where(g=>g.ItemType==ItemTypeEnum.Goal).OrderBy(g=>g.Code).ToListAsync(ct);
        var uploadMetrics=uploadGoals.Select(g=>new{key="GOAL_"+g.Id,name=g.Code+" · "+g.Title,unit=(!g.UnitId.HasValue||g.Unit?.Name=="%"||g.Unit?.Code=="PERCENT")?"%":"số lượng",goalCodes=new[]{g.Code}});
        return Ok(new{metrics,uploadMetrics,goals,allowedHosts=AiMetricCatalog.AllowedHosts,configured=options.Ready,enabled=options.Enabled,model=options.Model});
    }
    [HttpGet("sources")]
    public async Task<IActionResult> Sources(CancellationToken ct)=>Ok(await db.AiDataSources.AsNoTracking().OrderBy(s=>s.Name).ToListAsync(ct));
    public sealed record SourceInput(string Name,string MetricKey,string Url,bool Enabled,int IntervalHours=168);
    [HttpPost("sources")]
    public async Task<IActionResult> SaveSource(SourceInput input,CancellationToken ct)
    {
        try{AiMetricCatalog.Get(input.MetricKey);}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        if(string.IsNullOrWhiteSpace(input.Name)||input.Name.Length>250||input.Url.Length>1500||!AiMetricCatalog.IsAllowedUrl(input.Url)||input.IntervalHours is <24 or >8760)return BadRequest(new{message="Tên nguồn/URL chưa hợp lệ hoặc lịch kiểm tra ngoài 24–8.760 giờ."});
        if(await db.AiDataSources.AnyAsync(s=>s.Url==input.Url&&s.MetricKey==input.MetricKey,ct))return Conflict(new{message="Nguồn đã tồn tại cho chỉ số này."});
        var source=new AiDataSource{Name=input.Name.Trim(),Url=input.Url,MetricKey=input.MetricKey,Enabled=false,IntervalHours=input.IntervalHours,NextCheckAt=null};
        db.Add(source);await db.SaveChangesAsync(ct);return Ok(source);
    }
    [HttpPut("sources/{id:guid}/enabled")]
    public IActionResult Enable()=>Conflict(new{message="Đã tạm dừng tự quét Internet. Hãy dùng hộp tin Google Alerts hoặc tải tài liệu."});
    [HttpPost("sources/{id:guid}/collect")]
    public IActionResult Collect()=>Conflict(new{message="Đã tạm dừng tự quét Internet. Hãy dùng hộp tin Google Alerts hoặc tải tài liệu."});
    [HttpGet("documents")]
    public async Task<IActionResult> Documents(int page=1,CancellationToken ct=default,int pageSize=5,bool? uploadsOnly=null)
    {page=Math.Clamp(page,1,100000);pageSize=Math.Clamp(pageSize,1,100);var q=db.AiSourceDocuments.AsNoTracking();if(uploadsOnly.HasValue)q=q.Where(d=>db.AiInboxItems.Any(i=>i.Id==d.InboxItemId&&(uploadsOnly.Value?i.StorageKey!="":i.StorageKey=="")));return Ok(new{pageSize,total=await q.CountAsync(ct),items=await q.OrderByDescending(x=>x.RetrievedAt).ThenByDescending(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).Select(x=>new{x.Id,x.SourceId,x.Title,x.Url,x.ContentHash,x.RetrievedAt,x.Status,x.Error,x.Model}).ToListAsync(ct)});}
    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions(string? status=null,string? metricKey=null,int? year=null,int page=1,Guid? inboxId=null,CancellationToken ct=default,int pageSize=10,bool internetOnly=false)
    {
        page=Math.Clamp(page,1,100000);pageSize=Math.Clamp(pageSize,1,100);var q=db.AiDataSuggestions.AsNoTracking();if(!string.IsNullOrEmpty(status))q=q.Where(s=>s.Status==status);if(!string.IsNullOrEmpty(metricKey))q=q.Where(s=>s.MetricKey==metricKey);if(year.HasValue)q=q.Where(s=>s.DataYear==year);
        if(internetOnly)q=q.Where(s=>db.AiSourceDocuments.Any(d=>d.Id==s.DocumentId&&db.AiInboxItems.Any(i=>i.Id==d.InboxItemId&&i.StorageKey=="")));
        if(inboxId.HasValue)
        {
            var input=await db.AiInboxItems.AsNoTracking().SingleOrDefaultAsync(i=>i.Id==inboxId.Value,ct);
            if(input==null)return NotFound(new{message="Không tìm thấy đầu vào."});
            var url=input.StorageKey!=""?"/api/ai-inbox/"+input.Id+"/file":input.Url;
            q=q.Where(s=>db.AiSourceDocuments.Any(d=>d.Id==s.DocumentId&&(d.InboxItemId==input.Id||(d.InboxItemId==null&&d.Url==url))));
        }
        var total=await q.CountAsync(ct);
        var items=await (from s in q join d in db.AiSourceDocuments on s.DocumentId equals d.Id join input in db.AiInboxItems on d.InboxItemId equals input.Id into inputs from input in inputs.DefaultIfEmpty() orderby s.CreatedAt descending select new{s.Id,s.DocumentId,s.MetricKey,s.DataYear,s.PeriodLabel,s.Value,s.Unit,s.Scope,s.Quote,s.Page,s.Methodology,s.Warnings,s.Status,s.CreatedAt,s.ReviewedAt,s.ReviewedBy,s.ReviewNote,s.AcceptedValue,s.IsCurrent,sourceTitle=d.Title,sourceUrl=d.Url,contentHash=d.ContentHash,model=d.Model,readAt=d.RetrievedAt,inboxId=d.InboxItemId,inputSequence=input==null?(long?)null:input.SequenceNumber,inputReceivedAt=input==null?(DateTime?)null:input.ReceivedAt,inputType=input==null?"":input.StorageKey!=""?"upload":"rss",feedName=input==null?null:db.AiAlertFeeds.Where(f=>f.Id==input.AlertFeedId).Select(f=>f.Name).FirstOrDefault()}).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new{pageSize,total,items});
    }
    [HttpGet("documents/{id:guid}/page/{page:int}")]
    public async Task<IActionResult> PageText(Guid id,int page,CancellationToken ct)
    {var d=await db.AiSourceDocuments.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);if(d==null)return NotFound();var p=JsonSerializer.Deserialize<List<SourcePage>>(d.PagesJson)?.FirstOrDefault(x=>x.Number==page);return p==null?NotFound():Ok(new{text=p.Text,d.Url,d.ContentHash});}
    [HttpGet("suggestions/{id:guid}/goals")]
    public async Task<IActionResult> LinkedGoals(Guid id,CancellationToken ct)=>Ok(await (
        from link in db.AiGoalDataLinks.AsNoTracking()
        join goal in db.GoalTaskItems.AsNoTracking() on link.GoalTaskId equals goal.Id
        where link.SuggestionId==id
        select new {goal.Id,goal.Code,goal.Title,link.ProgressLogId}).ToListAsync(ct));
    public sealed record ReviewInput(bool Approve,decimal? AcceptedValue,string? Note=null,bool Verified=false);
    [HttpPost("suggestions/{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id,ReviewInput input,CancellationToken ct)
    {
        if(input.Approve&&!input.Verified)return BadRequest(new{message="Cần xác nhận đã kiểm tra phạm vi, kỳ dữ liệu, đơn vị và phương pháp."});
        try {await reviews.ReviewAsync(id,input.Approve,input.AcceptedValue,input.Note,User.FindFirstValue("FullName")??User.Identity?.Name??User.FindFirstValue(ClaimTypes.NameIdentifier)??"Admin",ct);return Ok();}
        catch(KeyNotFoundException){return NotFound();}
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        catch(DbUpdateException){return Conflict(new{message="Dữ liệu đã thay đổi đồng thời; tải lại trước khi duyệt."});}
        catch(Npgsql.PostgresException){return Conflict(new{message="Dữ liệu đã thay đổi đồng thời; tải lại trước khi duyệt."});}
    }
    [HttpGet("verified")]
    public async Task<IActionResult> Verified(CancellationToken ct)
    {
        var data=await db.AiDataSuggestions.AsNoTracking().Where(s=>s.IsCurrent&&s.Status=="Approved").OrderByDescending(s=>s.DataYear).ToListAsync(ct);
        var links=await (from link in db.AiGoalDataLinks.AsNoTracking() join goal in db.GoalTaskItems.AsNoTracking() on link.GoalTaskId equals goal.Id select new {link.SuggestionId,link.ProgressLogId,goal.Id,goal.Code,goal.Title}).ToListAsync(ct);
        return Ok(from s in data join link in links on s.Id equals link.SuggestionId select new{s.Id,s.MetricKey,s.DataYear,s.PeriodLabel,value=s.AcceptedValue??s.Value,s.Unit,goalCode=link.Code,goalTitle=link.Title,goalId=link.Id,link.ProgressLogId,thresholdReached=AiMetricCatalog.Reached(link.Code,s.AcceptedValue??s.Value),s.DocumentId,s.ReviewedAt,s.ReviewedBy,s.ReviewNote});
    }
}
