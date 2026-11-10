using System.Security.Claims;
using Cdsqg.Api.Security;
using Cdsqg.Application.Services;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Api.Controllers;
[ApiController,Route("api/digital-news"),Authorize]
public sealed class DigitalNewsController(AppDbContext db,DigitalNewsSyncService sync):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string? search=null,string? agency=null,string? status=null,int page=1,int pageSize=10,CancellationToken ct=default,string sortBy="importedAt",string sortOrder="desc")
    {
        pageSize=Math.Clamp(pageSize,1,100);var query=DigitalNewsRetention.Visible(db.DigitalNewsItems.AsNoTracking(),db,DateTime.UtcNow);
        if(!string.IsNullOrWhiteSpace(search)){var term=search.Trim().ToLowerInvariant();query=query.Where(i=>i.Title.ToLower().Contains(term)||i.AgencyName.ToLower().Contains(term));}
        if(!string.IsNullOrWhiteSpace(agency))query=query.Where(i=>i.AgencyName==agency);
        // Recheck existing titles without deleting history; filter BEFORE count and pagination.
        var collectionKeywords=await db.DigitalPlanKeywords.AsNoTracking().Where(x=>x.Enabled).ToListAsync(ct);
        var candidates=await query.Select(i=>new{i.Id,i.Title}).ToListAsync(ct);
        var visibleIds=candidates.Where(i=>collectionKeywords.Any(keyword=>DigitalNewsCollectionFilter.Matches(i.Title,keyword))).Select(i=>i.Id).ToArray();
        query=query.Where(i=>visibleIds.Contains(i.Id));
        var criteriaHash=DigitalPlanCriteria.Parse((await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync(ct)).EvaluationCriteriaJson).Hash();
        if(!string.IsNullOrWhiteSpace(status))
        {
            if(status.Equals("Unread",StringComparison.OrdinalIgnoreCase))
            {
                var readItemIds=db.DigitalPlanReads.AsNoTracking().Where(x=>x.CriteriaHash==criteriaHash).Select(x=>x.ItemId);
                query=query.Where(i=>!readItemIds.Contains(i.Id));
            }
            else
            {
                var matchingItemIds=db.DigitalPlanReads.AsNoTracking().Where(x=>x.CriteriaHash==criteriaHash&&x.Status==status).Select(x=>x.ItemId);
                query=query.Where(i=>matchingItemIds.Contains(i.Id));
            }
        }
        var total=await query.CountAsync(ct);page=Math.Clamp(page,1,Math.Max(1,(total+pageSize-1)/pageSize));
        var ascending=sortOrder.Equals("asc",StringComparison.OrdinalIgnoreCase);
        var ordered=sortBy switch
        {
            "agencyName"=>ascending?query.OrderBy(i=>i.AgencyName):query.OrderByDescending(i=>i.AgencyName),
            "title"=>ascending?query.OrderBy(i=>i.Title):query.OrderByDescending(i=>i.Title),
            _=>ascending?query.OrderBy(i=>i.ImportedAt):query.OrderByDescending(i=>i.ImportedAt)
        };
        var stableOrder=sortBy is not ("agencyName" or "title")&&ascending
            ?ordered.ThenBy(i=>i.SourceRow).ThenBy(i=>i.Id)
            :ordered.ThenByDescending(i=>i.SourceRow).ThenByDescending(i=>i.Id);
        var rawItems=await stableOrder.Skip((page-1)*pageSize).Take(pageSize).Select(i=>new{i.Id,i.AgencyName,i.Title,i.Url,i.UrlHash,i.ImportedAt,i.UpdatedAt,i.SourceRow}).ToListAsync(ct);
        var ids=rawItems.Select(x=>x.Id).ToArray();
        // criteriaHash cached

        var reads=await db.DigitalPlanReads.AsNoTracking().Where(x=>ids.Contains(x.ItemId)).ToListAsync(ct);
        var items=rawItems.Select(i=>{
            var hash=DigitalPlanText.Hash(i.AgencyName+"\n"+i.Title);
            var read=reads.FirstOrDefault(x=>x.ItemId==i.Id&&x.TitleHash==hash&&x.CriteriaHash==criteriaHash);
            return new{i.Id,i.AgencyName,i.Title,i.Url,i.ImportedAt,i.UpdatedAt,i.SourceRow,AiStatus=read?.Status??"Unread",Processing=read?.Status=="Reading",AiError=read?.Reason??"",Assessment=read};
        });
        return Ok(new{items,total,page,pageSize});
    }
    [HttpGet("agencies")]
    public async Task<IActionResult> Agencies(CancellationToken ct)=>Ok(await DigitalNewsRetention.Visible(db.DigitalNewsItems.AsNoTracking(),db,DateTime.UtcNow).Select(i=>i.AgencyName).Distinct().OrderBy(n=>n).ToListAsync(ct));
    [HttpGet("settings"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Settings(CancellationToken ct){
        var settings=await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync(ct);
        return Ok(new{evaluationCriteria=DigitalPlanCriteria.Parse(settings.EvaluationCriteriaJson),settings.Enabled,settings.IntervalMinutes,publishedFrom=settings.PublishedFrom??DigitalNewsPublicationWindow.DefaultFrom,settings.PublishedTo,settings.LookbackDays,settings.BatchSize,settings.QueryCursor,settings.NextRunAt,settings.LastStatus,settings.LastError,busy=settings.LeaseUntil>DateTime.UtcNow,
            sources=await db.DigitalPlanSources.AsNoTracking().OrderByDescending(x=>x.Enabled).ThenBy(x=>x.SortOrder).ThenBy(x=>x.Id).ToListAsync(ct),keywords=await db.DigitalPlanKeywords.AsNoTracking().OrderByDescending(x=>x.Enabled).ThenBy(x=>x.SortOrder).ThenBy(x=>x.Id).ToListAsync(ct)});
    }
    [HttpPut("criteria"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Criteria(DigitalPlanCriteria input,CancellationToken ct){
        try{var criteria=input.Validate();await db.DigitalNewsSyncSettings.Where(x=>x.Id==1).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.EvaluationCriteriaJson,criteria.Json()),ct);return Ok(new{message="Đã lưu tiêu chí. Lần bấm Đọc tiếp theo sẽ dùng cấu hình mới; kết quả cũ được giữ lại."});}
        catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}
    }
    public sealed record SyncInput(bool Enabled,int IntervalMinutes,int BatchSize,DateOnly? PublishedFrom=null,DateOnly? PublishedTo=null,int LookbackDays=365);
    [HttpPut("settings"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Save(SyncInput input,CancellationToken ct){try{await sync.SaveAsync(input.Enabled,input.IntervalMinutes,input.LookbackDays,input.BatchSize,User.Identity?.Name??"Admin",ct,input.PublishedFrom,input.PublishedTo);return Ok(new{message="Đã lưu lịch quét tin."});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    public sealed record MonitoringInput(List<Cdsqg.Core.Entities.DigitalPlanSource> Sources,List<Cdsqg.Core.Entities.DigitalPlanKeyword> Keywords);
    [HttpPut("monitoring"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Monitoring(MonitoringInput input,CancellationToken ct){
        try{
            if(input.Sources==null||input.Keywords==null||input.Sources.Count is <1 or >500||input.Keywords.Count is <1 or >100||!input.Sources.Any(x=>x.Enabled)||!input.Keywords.Any(x=>x.Enabled))throw new InvalidOperationException("Cần 1–500 nguồn, 1–100 từ khóa; bật ít nhất một nguồn và một từ khóa.");
            var sortedSources = input.Sources.OrderByDescending(x=>x.Enabled).ThenBy(x=>x.SortOrder).ToList();
            for (int i = 0; i < sortedSources.Count; i++) sortedSources[i].SortOrder = i + 1;
            var sortedKeywords = input.Keywords.OrderByDescending(x=>x.Enabled).ThenBy(x=>x.SortOrder).ToList();
            for (int i = 0; i < sortedKeywords.Count; i++) sortedKeywords[i].SortOrder = i + 1;

            foreach(var source in sortedSources){source.Domain=DigitalPlanDiscoveryReader.ValidateDomain(source.Domain);if(string.IsNullOrWhiteSpace(source.Name)||source.Name.Length>300||string.IsNullOrWhiteSpace(source.AgencyName)||source.AgencyName.Length>300||source.RequiredTerms.Length>300)throw new InvalidOperationException("Nhập tên nguồn, bộ/tỉnh tối đa 300 ký tự và từ khóa bổ sung tối đa 300 ký tự.");}
            foreach(var keyword in sortedKeywords)if(string.IsNullOrWhiteSpace(keyword.Phrase)||keyword.Phrase.Length>300||keyword.Conditions.Length>500||keyword.Name.Length>300)throw new InvalidOperationException("Cụm tìm kiếm bắt buộc, tối đa 300 ký tự; điều kiện tối đa 500 ký tự.");
            if(sortedSources.Select(x=>x.Id).Distinct().Count()!=sortedSources.Count||sortedKeywords.Select(x=>x.Id).Distinct().Count()!=sortedKeywords.Count)throw new InvalidOperationException("Mã cấu hình bị trùng.");
            await using var tx=await db.Database.BeginTransactionAsync(ct);var settings=await DigitalNewsSyncService.LockSettingsAsync(db,ct);
            var sources=await db.DigitalPlanSources.ToListAsync(ct);var keywords=await db.DigitalPlanKeywords.ToListAsync(ct);
            db.RemoveRange(sources.Where(x=>!sortedSources.Any(v=>v.Id==x.Id)));db.RemoveRange(keywords.Where(x=>!sortedKeywords.Any(v=>v.Id==x.Id)));
            foreach(var source in sortedSources){var old=sources.FirstOrDefault(x=>x.Id==source.Id);if(old==null)db.Add(source);else db.Entry(old).CurrentValues.SetValues(source);}
            foreach(var keyword in sortedKeywords){var old=keywords.FirstOrDefault(x=>x.Id==keyword.Id);if(old==null)db.Add(keyword);else db.Entry(old).CurrentValues.SetValues(keyword);}
            settings.QueryCursor=0;await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return Ok(new{message="Đã lưu nguồn và từ khóa theo dõi."});
        }catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}
    }
    [HttpPost("sync"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Sync(CancellationToken ct){try{var result=await sync.RunAsync(true,ct);return result==null?Conflict(new{message="Một lượt quét đang chạy."}):Ok(new{message=$"Quét xong: {result.Added} bài mới, {result.Unchanged} đã có, {result.Filtered} lượt kết quả bị loại theo tiêu đề, {result.Skipped} truy vấn lỗi.",result});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpGet("{id:guid}/assessment")]
    public async Task<IActionResult> Assessment(Guid id,CancellationToken ct,Guid? result=null,Guid? notification=null){
        var item=await DigitalNewsRetention.Visible(db.DigitalNewsItems.AsNoTracking(),db,DateTime.UtcNow).SingleOrDefaultAsync(x=>x.Id==id,ct);if(item==null)return NotFound();
        var reads=db.DigitalPlanReads.AsNoTracking().Where(x=>x.ItemId==id);
        if(result.HasValue)reads=reads.Where(x=>x.Id==result.Value);
        else if(notification.HasValue)reads=reads.Where(x=>x.NotificationId==notification.Value);
        else{var hash=DigitalPlanText.ReadHash(item);var criteriaHash=DigitalPlanCriteria.Parse((await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync(ct)).EvaluationCriteriaJson).Hash();reads=reads.Where(x=>x.TitleHash==hash&&x.CriteriaHash==criteriaHash);}
        var assessment=await reads.SingleOrDefaultAsync(ct);
        if((result.HasValue||notification.HasValue)&&assessment==null)return NotFound(new{message="Không tìm thấy kết quả được nhắc trong thông báo."});
        return Ok(new{item=new{item.Id,Title=assessment?.Title??item.Title,AgencyName=assessment?.AgencyName??item.AgencyName,item.Url},assessment});
    }
    [HttpPost("{id:guid}/read"),Authorize(Roles=RolePermissions.UtilityEditorRoles)]
    public async Task<IActionResult> Read(Guid id,[FromServices]DigitalPlanReadService reader,CancellationToken ct){
        if(!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var userId))return Unauthorized();
        var read=await reader.QueueAsync(id,userId,ct);return read==null?NotFound():Ok(new{message=read.Status is "Queued" or "Reading"?"Đã xếp hàng đánh giá tiêu đề. Kết quả sẽ được lưu tại bài viết.":"Đang hiển thị kết quả đã lưu.",assessment=read});
    }
}
