using System.Security.Claims;
using Cdsqg.Application.AI;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Api.Controllers;
[ApiController,Route("api/ai-alerts"),Authorize(Roles="Admin")]
public sealed class AiAlertsController(AppDbContext db,AiAlertService service,AiSettingsStore settings):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(bool deleted=false,int page=1,CancellationToken ct=default)
    {
        const int pageSize=5;
        var query=db.AiAlertFeeds.AsNoTracking().Where(f=>deleted?f.DeletedAt!=null:f.DeletedAt==null);
        var total=await query.CountAsync(ct);
        page=Math.Clamp(page,1,Math.Max(1,(total+pageSize-1)/pageSize));
        var feeds=await query.OrderByDescending(f=>f.CreatedAt).ThenByDescending(f=>f.Id).Skip((page-1)*pageSize).Take(pageSize).Select(f=>new{f.Id,f.Name,f.FeedUrl,f.Enabled,scanTime=(f.ScanMinute/60).ToString().PadLeft(2,'0')+":"+(f.ScanMinute%60).ToString().PadLeft(2,'0'),f.NextScanAt,f.UpdatedBy,
            lastScan=db.AiAlertScans.Where(s=>s.FeedId==f.Id).OrderByDescending(s=>s.QueuedAt).ThenByDescending(s=>s.Id).Select(s=>new{s.Id,s.Status,s.QueuedAt,s.CompletedAt,s.Added,s.Duplicates,s.Blocked,s.Received,s.Error}).FirstOrDefault()}).ToListAsync(ct);
        return Ok(new{page,pageSize,total,feeds,configured=(await settings.GetAsync(ct)).Ready,allowedHosts=AiMetricCatalog.AllowedHosts});
    }
    [HttpGet("scans")]
    public async Task<IActionResult> Scans(Guid? feedId=null,int page=1,CancellationToken ct=default,int pageSize=5)
    {
        const int fixedPageSize=5;
        var query=db.AiAlertScans.AsNoTracking();if(feedId.HasValue)query=query.Where(s=>s.FeedId==feedId);
        var total=await query.CountAsync(ct);
        page=Math.Clamp(page,1,Math.Max(1,(total+fixedPageSize-1)/fixedPageSize));
        return Ok(new{page,pageSize=fixedPageSize,total,items=await (from scan in query join feed in db.AiAlertFeeds on scan.FeedId equals feed.Id orderby scan.QueuedAt descending,scan.Id descending select new{scan.Id,scan.FeedId,feedName=feed.Name,scan.Status,scan.Trigger,scan.RequestedBy,scan.QueuedAt,scan.StartedAt,scan.CompletedAt,scan.Received,scan.Added,scan.Duplicates,scan.Blocked,scan.Error}).Skip((page-1)*fixedPageSize).Take(fixedPageSize).ToListAsync(ct)});
    }
    private string Actor=>User.FindFirstValue("FullName")??User.Identity?.Name??"Admin";
    [HttpPost,RequestSizeLimit(8192)]
    public Task<IActionResult> Create(AlertFeedInput input,CancellationToken ct)=>Save(null,input,ct);
    [HttpPut("{id:guid}"),RequestSizeLimit(8192)]
    public Task<IActionResult> Update(Guid id,AlertFeedInput input,CancellationToken ct)=>Save(id,input,ct);
    private async Task<IActionResult> Save(Guid? id,AlertFeedInput input,CancellationToken ct)
    {
        try{var feed=await service.SaveAsync(id,input,Actor,ct);return Ok(new{feed.Id,message="Đã lưu nguồn RSS."});}
        catch(KeyNotFoundException){return NotFound();}
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        catch(DbUpdateException){return Conflict(new{message="Nguồn hoặc lượt quét vừa thay đổi. Tải lại và thử lại."});}
    }
    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id,CancellationToken ct)=>SetDeleted(id,true,ct);
    [HttpPost("{id:guid}/restore")]
    public Task<IActionResult> Restore(Guid id,CancellationToken ct)=>SetDeleted(id,false,ct);
    private async Task<IActionResult> SetDeleted(Guid id,bool deleted,CancellationToken ct)
    {
        try{await service.SetDeletedAsync(id,deleted,Actor,ct);return Ok(new{message=deleted?"Đã xóa nguồn; dữ liệu và lịch sử được giữ lại.":"Đã khôi phục nguồn. Lịch quét đang tắt."});}
        catch(KeyNotFoundException){return NotFound();}
        catch(InvalidOperationException e){return Conflict(new{message=e.Message});}
    }
    [HttpPost("{id:guid}/scan")]
    public async Task<IActionResult> Scan(Guid id,CancellationToken ct)
    {
        try{var job=await service.EnqueueAsync(id,"Manual",Actor,ct);return job==null?Conflict(new{message="Nguồn đang có lượt quét trong hàng đợi."}):Accepted(new{job.Id,message="Đã xếp lượt quét. Tiêu đề mới sẽ được AI đánh giá khi cấu hình sẵn sàng."});}
        catch(KeyNotFoundException){return NotFound();}
        catch(DbUpdateException){return Conflict(new{message="Đã có lượt quét khác. Tải lại để theo dõi."});}
    }
}
