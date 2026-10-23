using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.AI;
public sealed record AlertFeedInput(string Name,string FeedUrl,bool Enabled,string ScanTime);
public sealed class AiAlertService(AppDbContext db,IAiAlertFeedReader reader,AiInboxService inbox)
{
    public static int ParseMinute(string time)
    {
        if(!TimeOnly.TryParseExact(time,"HH:mm",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var value))throw new InvalidOperationException("Giờ quét cần theo định dạng HH:mm.");
        return value.Hour*60+value.Minute;
    }
    // Vietnam is UTC+7 year-round; persist UTC and display using the shared FE formatter.
    public static DateTime NextDaily(DateTime utc,int minute)
    {
        if(minute is <0 or >1439)throw new InvalidOperationException("Giờ quét không hợp lệ.");
        var next=utc.AddHours(7).Date.AddMinutes(minute).AddHours(-7);
        return next>utc?next:next.AddDays(1);
    }
    public async Task<AiAlertFeed> SaveAsync(Guid? id,AlertFeedInput input,string actor,CancellationToken ct)
    {
        var name=input.Name.Trim();if(name.Length is <3 or >150)throw new InvalidOperationException("Tên nguồn từ 3–150 ký tự.");
        var url=AiAlertFeedReader.ValidateUrl(input.FeedUrl.Trim());var minute=ParseMinute(input.ScanTime);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var feed=id.HasValue?await db.AiAlertFeeds.FromSqlInterpolated($"SELECT * FROM \"AiAlertFeeds\" WHERE \"Id\"={id.Value} FOR UPDATE").SingleOrDefaultAsync(ct)??throw new KeyNotFoundException():new AiAlertFeed();
        if(feed.DeletedAt!=null)throw new InvalidOperationException("Nguồn đã xóa; cần khôi phục trước khi sửa.");
        if(id.HasValue&&await db.AiAlertScans.AnyAsync(s=>s.FeedId==id&&(s.Status=="Queued"||s.Status=="Running"),ct))throw new InvalidOperationException("Nguồn đang có lượt quét. Chờ hoàn tất trước khi sửa.");
        if(await db.AiAlertFeeds.AnyAsync(f=>f.FeedUrl==url&&f.Id!=feed.Id,ct))throw new InvalidOperationException("URL RSS đã có trong danh sách.");
        var scheduleChanged=feed.Enabled!=input.Enabled||feed.ScanMinute!=minute;
        feed.Name=name;feed.FeedUrl=url;feed.Enabled=input.Enabled;feed.ScanMinute=minute;feed.UpdatedBy=actor;
        if(!id.HasValue||scheduleChanged)feed.NextScanAt=input.Enabled?NextDaily(DateTime.UtcNow,minute):null;
        if(!id.HasValue)db.Add(feed);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return feed;
    }
    public async Task<AiAlertScan?> EnqueueAsync(Guid id,string trigger,string actor,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        // A database row lock serializes manual and scheduled enqueue requests across instances.
        var feed=await db.AiAlertFeeds.FromSqlInterpolated($"SELECT * FROM \"AiAlertFeeds\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)??throw new KeyNotFoundException();
        if(feed.DeletedAt!=null)throw new KeyNotFoundException();
        var now=DateTime.UtcNow;
        if(trigger=="Scheduled"&&(!feed.Enabled||feed.NextScanAt==null||feed.NextScanAt>now)){await tx.CommitAsync(ct);return null;}
        if(await db.AiAlertScans.AnyAsync(s=>s.FeedId==id&&(s.Status=="Queued"||s.Status=="Running"),ct)){await tx.CommitAsync(ct);return null;}
        var scan=new AiAlertScan{FeedId=id,Trigger=trigger,RequestedBy=actor};db.Add(scan);
        if(trigger=="Scheduled")feed.NextScanAt=NextDaily(now,feed.ScanMinute);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return scan;
    }
    public async Task SetDeletedAsync(Guid id,bool deleted,string actor,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var feed=await db.AiAlertFeeds.FromSqlInterpolated($"SELECT * FROM \"AiAlertFeeds\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)??throw new KeyNotFoundException();
        if(await db.AiAlertScans.AnyAsync(s=>s.FeedId==id&&(s.Status=="Queued"||s.Status=="Running"),ct))throw new InvalidOperationException("Nguồn đang có lượt quét; chờ hoàn tất trước khi xóa.");
        feed.DeletedAt=deleted?DateTime.UtcNow:null;feed.Enabled=false;feed.NextScanAt=null;feed.UpdatedBy=actor;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task RunAsync(Guid id,CancellationToken ct)
    {
        var now=DateTime.UtcNow;
        var claimed=await db.AiAlertScans.Where(s=>s.Id==id&&(s.Status=="Queued"||s.Status=="Running")&&(s.LeaseUntil==null||s.LeaseUntil<now))
            .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Status,"Running").SetProperty(x=>x.StartedAt,now).SetProperty(x=>x.LeaseUntil,now.AddMinutes(5)),ct);
        if(claimed==0)return;
        var scan=await db.AiAlertScans.SingleAsync(s=>s.Id==id,ct);
        await db.Entry(scan).ReloadAsync(ct); // Refresh the lease snapshot after the atomic claim.
        try
        {
            var feed=await db.AiAlertFeeds.AsNoTracking().SingleAsync(f=>f.Id==scan.FeedId,ct);
            var entries=await reader.ReadAsync(feed.FeedUrl,ct);
            scan.Received=entries.Count;scan.Added=0;scan.Blocked=0;scan.Duplicates=0;
            foreach(var entry in entries)
            {
                // Validate targets before AI or HTTP access. Google wrapper URLs are never followed.
                if(entry.Title.Length is <3 or >1000||!AiMetricCatalog.IsAllowedUrl(entry.Url)){scan.Blocked++;continue;}
                var originPrefix = new Uri(feed.FeedUrl).Host switch {
                    "news.google.com" => "Google News · ",
                    "www.google.com" or "google.com" => "Google Alerts · ",
                    _ => "RSS trực tiếp · "
                };
                var added=await inbox.ImportEntryAsync(new(entry.Title,entry.Url),feed.Id,entry.PublishedAt,originPrefix+feed.Name,ct);
                if(added)scan.Added++;else scan.Duplicates++;
            }
            scan.Status="Completed";scan.Error="";
        }
        catch(OperationCanceledException)when(ct.IsCancellationRequested){scan.Status="Queued";throw;}
        catch(Exception ex){scan.Status="Error";scan.Error=ex is InvalidOperationException?ex.Message:ex is System.Xml.XmlException?"RSS không đọc được hoặc không hợp lệ.":"Không nhận được RSS. Kiểm tra URL và kết nối rồi quét lại.";}
        finally{scan.LeaseUntil=null;if(scan.Status!="Queued")scan.CompletedAt=DateTime.UtcNow;await db.SaveChangesAsync(CancellationToken.None);}
    }
}
public sealed class AiAlertWorker(IServiceScopeFactory scopes,ILogger<AiAlertWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();var service=scope.ServiceProvider.GetRequiredService<AiAlertService>();var now=DateTime.UtcNow;
                var due=await db.AiAlertFeeds.AsNoTracking().Where(f=>f.DeletedAt==null&&f.Enabled&&f.NextScanAt<=now).OrderBy(f=>f.NextScanAt).Select(f=>f.Id).Take(20).ToListAsync(ct);
                foreach(var id in due)await service.EnqueueAsync(id,"Scheduled","Lịch tự động",ct);
                var jobs=await db.AiAlertScans.AsNoTracking().Where(s=>(s.Status=="Queued"||s.Status=="Running")&&(s.LeaseUntil==null||s.LeaseUntil<now)).OrderBy(s=>s.QueuedAt).Select(s=>s.Id).Take(5).ToListAsync(ct);
                foreach(var id in jobs)await service.RunAsync(id,ct);
            }
            catch(OperationCanceledException)when(ct.IsCancellationRequested){break;}
            catch{logger.LogWarning("Google Alerts cycle could not finish; retrying next cycle.");}
            try{if(!await timer.WaitForNextTickAsync(ct))break;}catch(OperationCanceledException){break;}
        }while(!ct.IsCancellationRequested);
    }
}
