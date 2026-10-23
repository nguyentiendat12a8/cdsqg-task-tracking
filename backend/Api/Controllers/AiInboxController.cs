using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cdsqg.Application.AI;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Api.Controllers;
[ApiController,Route("api/ai-inbox"),Authorize(Roles="Admin")]
public sealed class AiInboxController(AppDbContext db,AiInboxService inbox):ControllerBase
{
    
    [HttpGet]
    public async Task<IActionResult> List(int page=1,bool uploadsOnly=false,bool alertsOnly=false,string? status=null,CancellationToken ct=default,int pageSize=5)
    {
        page=Math.Clamp(page,1,100000);pageSize=Math.Clamp(pageSize,1,100);var q=db.AiInboxItems.AsNoTracking();
        if(uploadsOnly)q=q.Where(i=>i.StorageKey!="");
        if(alertsOnly)q=q.Where(i=>i.AlertFeedId!=null);
        if(!string.IsNullOrEmpty(status))q=q.Where(i=>i.Status==status);
        return Ok(new{pageSize=pageSize,total=await q.CountAsync(ct),items=await q.OrderByDescending(i=>i.ReceivedAt).ThenByDescending(i=>i.SequenceNumber).Skip((page-1)*pageSize).Take(pageSize).Select(i=>new{i.Id,i.SequenceNumber,i.Title,i.Url,i.Origin,i.ReceivedAt,i.PublishedAt,i.AlertFeedId,i.Status,i.MetricKeysJson,i.Reason,i.Error,feedName=db.AiAlertFeeds.Where(f=>f.Id==i.AlertFeedId).Select(f=>f.Name).FirstOrDefault(),feedUrl=db.AiAlertFeeds.Where(f=>f.Id==i.AlertFeedId).Select(f=>f.FeedUrl).FirstOrDefault(),processingDetails=db.AiSourceDocuments.Where(d=>d.InboxItemId==i.Id&&d.Error!="").OrderByDescending(d=>d.RetrievedAt).Select(d=>d.Error).FirstOrDefault(),i.Model,isUpload=i.StorageKey!="",processing=(i.Status=="ReadQueued"||i.Status=="TitleQueued")&&i.LeaseUntil>DateTime.UtcNow}).ToListAsync(ct)});
    }
    [HttpPost("import"),RequestSizeLimit(262144)]
    public async Task<IActionResult> Import(List<AlertInput> rows,CancellationToken ct)
    {
        try{return Ok(new{added=await inbox.ImportAsync(rows,ct)});}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}catch(DbUpdateException){return Conflict(new{message="Danh sách vừa được đồng bộ; thử nhập lại để bỏ qua các dòng trùng."});}
    }
    [HttpPost("upload"),RequestSizeLimit(13000000)]
    public async Task<IActionResult> Upload(IFormFile file,[FromForm]string metricKey,CancellationToken ct)
    {
        try
        {
            var startedAt=DateTime.UtcNow;
            var item=await inbox.UploadAsync(file,metricKey,User.FindFirstValue("FullName")??User.Identity?.Name??"Admin",ct);
            var duplicate=item.ReceivedAt<startedAt;
            return Ok(new{item.Id,duplicate,message=duplicate
                ? "Tài liệu này đã được nạp cho chỉ số đã chọn. Đang hiển thị bản đã có; dùng Đọc lại nếu muốn xử lý lại."
                : "Đã nhận tài liệu vào hàng đợi. Trạng thái xử lý hiển thị trong chi tiết bên dưới; chưa tạo báo cáo.",
                item=new{item.Id,item.SequenceNumber,item.Title,item.Url,item.Origin,item.ReceivedAt,item.Status,item.MetricKeysJson,item.Reason,item.Error,item.Model,isUpload=true,processing=(item.Status=="ReadQueued"||item.Status=="TitleQueued")&&item.LeaseUntil>DateTime.UtcNow}});
        }
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        catch(InvalidDataException){return BadRequest(new{message="File không hợp lệ hoặc không đọc được. Chọn PDF có chữ, DOCX hoặc TXT UTF-8."});}
        catch(DecoderFallbackException){return BadRequest(new{message="TXT cần mã hóa UTF-8."});}
    }
    public sealed record QueueInput(string MetricKey="",bool TitleOnly=false);
    [HttpPost("{id:guid}/queue")]
    public async Task<IActionResult> Queue(Guid id,QueueInput input,CancellationToken ct)
    {
        var item=await db.AiInboxItems.FindAsync([id],ct);if(item==null)return NotFound();
        if(item.StorageKey.Length==0&&item.AlertFeedId==null)return Conflict(new{message="Link cần được tiếp nhận từ nguồn RSS đã cấu hình."});
        if(item.StorageKey.Length==0){try{AiInboxService.CanonicalUrl(item.Url);}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}}
        if(item.Status=="ReadQueued"||item.Status=="TitleQueued")return Conflict(new{message="Tài liệu đang chờ hoặc đang xử lý; không thể đọc lại lúc này."});
        if(input.TitleOnly&&item.StorageKey.Length==0)
        {
            item.Status="TitleQueued";item.LeaseUntil=null;item.Error="";
        }
        else
        {
            try{if(item.StorageKey.Length>0)await AiMetricCatalog.ResolveAsync(db,input.MetricKey,ct);else AiMetricCatalog.Get(input.MetricKey);}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
            // An explicit reread is a new input version; preserve the original and its reviews.
            item=new Cdsqg.Core.Entities.AiInboxItem{Title=item.Title,Url=item.Url,UrlHash="reread:"+Guid.NewGuid().ToString("N"),Origin="Đọc lại từ "+item.Id,
                StorageKey=item.StorageKey,AlertFeedId=item.AlertFeedId,PublishedAt=item.PublishedAt,
                MetricKeysJson=System.Text.Json.JsonSerializer.Serialize(new[]{input.MetricKey}),Status="ReadQueued"};
            db.AiInboxItems.Add(item);
        }
        await db.SaveChangesAsync(ct);return Accepted(new{item.Id,message=input.TitleOnly?"Đã xếp hàng đánh giá lại tiêu đề.":"Đã tạo bản đọc mới và xếp hàng xử lý. Dẫn chứng lần trước được giữ nguyên; chưa ghi báo cáo."});
    }
    [HttpGet("{id:guid}/file"), AllowAnonymous]
    public async Task<IActionResult> Download(Guid id,CancellationToken ct)
    {
        var item=await db.AiInboxItems.AsNoTracking().SingleOrDefaultAsync(i=>i.Id==id,ct);
        if(item==null||item.StorageKey.Length==0)return NotFound();
        var path=inbox.PrivatePath(item.StorageKey);if(!System.IO.File.Exists(path))return NotFound();
        var ext=System.IO.Path.GetExtension(item.Title).ToLowerInvariant();
        var contentType=ext==".pdf"?"application/pdf":ext is ".docx" or ".doc"?"application/vnd.openxmlformats-officedocument.wordprocessingml.document":"application/octet-stream";
        return PhysicalFile(path,contentType,item.Title);
    }
}
[ApiController,Route("api/ai-alerts-bridge"),AllowAnonymous]
public sealed class AiAlertsBridgeController(IConfiguration configuration,AiInboxService inbox):ControllerBase
{
    public static bool ValidSecret(string? expected,string? supplied)=>!string.IsNullOrWhiteSpace(expected)&&expected.Length>=32&&!string.IsNullOrWhiteSpace(supplied)&&supplied.Length<=256
        &&CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)),SHA256.HashData(Encoding.UTF8.GetBytes(supplied)));
    [HttpPost,RequestSizeLimit(262144)]
    public async Task<IActionResult> Receive(List<AlertInput> rows,CancellationToken ct)
    {
        if(!ValidSecret(configuration["AiData:AlertsBridgeKey"],Request.Headers["X-Alerts-Key"].ToString()))return Unauthorized();
        try{return Ok(new{added=await inbox.ImportAsync(rows,ct)});}catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}catch(DbUpdateException){return Conflict(new{message="Đang đồng bộ đồng thời; thử lại."});}
    }
}

