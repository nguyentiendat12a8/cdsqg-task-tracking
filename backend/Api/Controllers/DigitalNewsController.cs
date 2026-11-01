using System.Security.Claims;
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
    public async Task<IActionResult> List(string? search=null,string? agency=null,int page=1,int pageSize=10,CancellationToken ct=default,string sortBy="importedAt",string sortOrder="desc")
    {
        pageSize=Math.Clamp(pageSize,1,100);var query=db.DigitalNewsItems.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(search)){var term=search.Trim().ToLowerInvariant();query=query.Where(i=>i.Title.ToLower().Contains(term)||i.AgencyName.ToLower().Contains(term));}
        if(!string.IsNullOrWhiteSpace(agency))query=query.Where(i=>i.AgencyName==agency);
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
        var hashes=rawItems.Select(i=>i.UrlHash).Where(h=>!string.IsNullOrEmpty(h)).Distinct().ToList();
        var urls=rawItems.Select(i=>i.Url).Where(u=>!string.IsNullOrEmpty(u)).Distinct().ToList();
        var inboxItems=await db.AiInboxItems.AsNoTracking().Where(a=>hashes.Contains(a.UrlHash)||urls.Contains(a.Url)).Select(a=>new{a.Url,a.UrlHash,a.Status,a.Error,a.LeaseUntil}).ToListAsync(ct);
        var inboxByHash=inboxItems.Where(a=>!string.IsNullOrEmpty(a.UrlHash)).GroupBy(a=>a.UrlHash).ToDictionary(g=>g.Key,g=>g.First());
        var inboxByUrl=inboxItems.Where(a=>!string.IsNullOrEmpty(a.Url)).GroupBy(a=>a.Url).ToDictionary(g=>g.Key,g=>g.First());
        var now=DateTime.UtcNow;
        var items=rawItems.Select(i=>
        {
            var inbox=inboxByHash.GetValueOrDefault(i.UrlHash)??inboxByUrl.GetValueOrDefault(i.Url);
            string status;
            if(inbox==null)status="Unread";
            else if(inbox.Status is "Read" or "NeedsReview")status="Read";
            else if(inbox.Status=="ReadNoData")status="ReadNoData";
            else if(inbox.Status is "TitleQueued" or "ReadQueued")status=inbox.LeaseUntil>now?"Processing":"Queued";
            else if(inbox.Status is "Skipped" or "NotRelevant")status="Skipped";
            else if(inbox.Status=="Error")status="Error";
            else status=inbox.Status;
            return new{i.Id,i.AgencyName,i.Title,i.Url,i.ImportedAt,i.UpdatedAt,i.SourceRow,AiStatus=status,AiError=inbox?.Error??""};
        });
        return Ok(new{items,total,page,pageSize});
    }
    [HttpGet("agencies")]
    public async Task<IActionResult> Agencies(CancellationToken ct)=>Ok(await db.DigitalNewsItems.AsNoTracking().Select(i=>i.AgencyName).Distinct().OrderBy(n=>n).ToListAsync(ct));
    [HttpGet("settings"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Settings(CancellationToken ct)=>Ok(await db.DigitalNewsSyncSettings.AsNoTracking().Select(s=>new{s.SheetUrl,s.Enabled,s.IntervalHours,s.NextRunAt,s.LastStartedAt,s.LastCompletedAt,s.LastStatus,s.LastError,s.Added,s.Updated,s.Unchanged,s.Skipped,s.UpdatedBy,busy=s.LeaseUntil>DateTime.UtcNow}).SingleAsync(ct));
    public sealed record SyncInput(string SheetUrl,bool Enabled,int IntervalHours);
    [HttpPut("settings"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Save(SyncInput input,CancellationToken ct){try{await sync.SaveAsync(input.SheetUrl??"",input.Enabled,input.IntervalHours,User.FindFirstValue("FullName")??User.Identity?.Name??"Admin",ct);return Ok(new{message="Đã lưu lịch đồng bộ Google Sheets."});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpPost("sync"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Sync(CancellationToken ct){try{var result=await sync.RunAsync(true,ct);return result==null?Conflict(new{message="Một lượt đồng bộ đang chạy."}):Ok(new{message=$"Đồng bộ xong: {result.Added} mới, {result.Updated} cập nhật, {result.Unchanged} đã có, {result.Skipped} bỏ qua.",result});}catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}}
    [HttpPost("{id:guid}/read"),Authorize(Roles="Admin")]
    public async Task<IActionResult> Read(Guid id,[FromServices]Cdsqg.Application.AI.AiInboxService inbox,CancellationToken ct)
    {
        var item=await db.DigitalNewsItems.FindAsync([id],ct);
        if(item==null)return NotFound();
        try
        {
            var url=Cdsqg.Application.AI.AiInboxService.CanonicalUrl(item.Url);
            var hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url)));
            var existing=await db.AiInboxItems.FirstOrDefaultAsync(i=>i.UrlHash==hash,ct);
            if(existing!=null)
            {
                existing.Status="TitleQueued";
                existing.LeaseUntil=null;
                existing.Error="";
                await db.SaveChangesAsync(ct);
            }
            else
            {
                await inbox.ImportEntryAsync(new Cdsqg.Application.AI.AlertInput(item.Title,item.Url),null,item.ImportedAt,"Google Sheets: "+item.AgencyName,ct);
            }
            return Ok(new{message=$"Đã đưa bài viết “{item.Title}” vào hàng đợi AI đánh giá tiêu đề."});
        }
        catch(InvalidOperationException ex){return BadRequest(new{message=ex.Message});}
    }
}
