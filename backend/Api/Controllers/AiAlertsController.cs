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
    public static readonly HashSet<Guid> FixedFeedIds = [
        Guid.Parse("c4a989de-8c33-4cd0-9139-688eac54b524"), // EGDI -> MT-12, MT-25
        Guid.Parse("1fdab812-a0b6-498d-a342-2bcafb952f9c"), // Kinh tế số GDP -> MT-13, MT-26
        Guid.Parse("6799b9b0-d1b4-4a89-81b4-d8718dd7cc6b")  // TMĐT -> MT-16
    ];

    // Khóa theo số thứ tự gốc QĐ 1266 (OriginalCode); hiển thị bằng mã hiện tại theo cơ quan chủ trì.
    private static string[] ApplicableGoals(string name, IReadOnlyDictionary<string,string> codes) =>
        (name.Contains("EGDI") || name.Contains("chính phủ") ? new[]{"MT-12", "MT-25"} :
         name.Contains("kinh tế số") || name.Contains("GDP") ? new[]{"MT-13", "MT-26"} :
         name.Contains("thương mại điện tử") || name.Contains("TMĐT") ? new[]{"MT-16"} : Array.Empty<string>())
        .Select(original => codes.TryGetValue(original, out var code) && !string.IsNullOrWhiteSpace(code) ? code : original).ToArray();

    [HttpGet]
    public async Task<IActionResult> List(bool deleted=false,int page=1,CancellationToken ct=default)
    {
        const int pageSize=5;
        var query=db.AiAlertFeeds.AsNoTracking().Where(f=>deleted?f.DeletedAt!=null:f.DeletedAt==null);
        var total=await query.CountAsync(ct);
        page=Math.Clamp(page,1,Math.Max(1,(total+pageSize-1)/pageSize));
        var goalCodes=(await db.GoalTaskItems.AsNoTracking()
                .Where(g=>g.DocumentId==AiDocumentScope.PriorityDocument&&g.ItemType==Cdsqg.Core.Enums.ItemTypeEnum.Goal&&g.OriginalCode!="")
                .Select(g=>new{g.OriginalCode,g.Code}).ToListAsync(ct))
            .GroupBy(g=>g.OriginalCode).ToDictionary(g=>g.Key,g=>g.First().Code);
        var feeds=await query.OrderByDescending(f=>f.CreatedAt).ThenByDescending(f=>f.Id).Skip((page-1)*pageSize).Take(pageSize).Select(f=>new{
            f.Id,f.Name,f.FeedUrl,f.Enabled,
            scanTime=(f.ScanMinute/60).ToString().PadLeft(2,'0')+":"+(f.ScanMinute%60).ToString().PadLeft(2,'0'),
            f.NextScanAt,f.UpdatedBy,
            isFixed=FixedFeedIds.Contains(f.Id),
            applicableGoals=ApplicableGoals(f.Name,goalCodes),
            lastScan=db.AiAlertScans.Where(s=>s.FeedId==f.Id).OrderByDescending(s=>s.QueuedAt).ThenByDescending(s=>s.Id).Select(s=>new{s.Id,s.Status,s.QueuedAt,s.CompletedAt,s.Added,s.Duplicates,s.Blocked,s.Received,s.Error}).FirstOrDefault()
        }).ToListAsync(ct);
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
    public async Task<IActionResult> Update(Guid id,AlertFeedInput input,CancellationToken ct)
    {
        if(FixedFeedIds.Contains(id))
            return BadRequest(new{message="Đây là nguồn Google Alerts cố định hệ thống áp dụng cho 5 chỉ số, không thể chỉnh sửa."});
        return await Save(id,input,ct);
    }
    private async Task<IActionResult> Save(Guid? id,AlertFeedInput input,CancellationToken ct)
    {
        try{var feed=await service.SaveAsync(id,input,Actor,ct);return Ok(new{feed.Id,message="Đã lưu nguồn RSS."});}
        catch(KeyNotFoundException){return NotFound();}
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        catch(DbUpdateException){return Conflict(new{message="Nguồn hoặc lượt quét vừa thay đổi. Tải lại và thử lại."});}
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id,CancellationToken ct)
    {
        if(FixedFeedIds.Contains(id))
            return BadRequest(new{message="Đây là nguồn Google Alerts cố định hệ thống áp dụng cho 5 chỉ số, không thể xóa."});
        return await SetDeleted(id,true,ct);
    }
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
