using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Application.Services;
public sealed record DigitalNewsSyncResult(int Added,int Updated,int Unchanged,int Skipped,int Filtered=0);
public sealed class DigitalNewsSyncService(AppDbContext db,IDigitalPlanDiscoveryReader reader)
{
    public async Task SaveAsync(bool enabled,int intervalMinutes,int lookbackDays,int batchSize,string actor,CancellationToken ct,DateOnly? publishedFrom=null,DateOnly? publishedTo=null)
    {
        if(intervalMinutes is <15 or >10080||lookbackDays is <1 or >3650||batchSize is <1 or >12)throw new InvalidOperationException("Lịch quét: 15–10080 phút, khoảng tìm 1–3650 ngày, mỗi lượt 1–12 truy vấn.");
        DigitalNewsPublicationWindow.Validate(publishedFrom??DigitalNewsPublicationWindow.DefaultFrom,publishedTo);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var settings=await LockSettingsAsync(db,ct);
        settings.PublishedFrom=publishedFrom;settings.PublishedTo=publishedTo;settings.Enabled=enabled;settings.IntervalMinutes=intervalMinutes;settings.LookbackDays=lookbackDays;settings.BatchSize=batchSize;settings.UpdatedBy=actor;
        settings.NextRunAt=enabled?DateTime.UtcNow:null;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public static async Task<DigitalNewsSyncSettings> LockSettingsAsync(AppDbContext db,CancellationToken ct){
        var settings=await db.DigitalNewsSyncSettings.FromSqlRaw("""SELECT * FROM "DigitalNewsSyncSettings" WHERE "Id"=1 FOR UPDATE""").SingleAsync(ct);
        await db.Entry(settings).ReloadAsync(ct);
        if(settings.LeaseUntil>DateTime.UtcNow)throw new InvalidOperationException("Đang quét tin. Chờ hoàn tất trước khi đổi cấu hình.");
        return settings;
    }
    public async Task<DigitalNewsSyncResult?> RunAsync(bool manual,CancellationToken ct)
    {
        var now=DateTime.UtcNow;var token=Guid.NewGuid();
        var claim=db.DigitalNewsSyncSettings.Where(s=>s.Id==1&&(s.LeaseUntil==null||s.LeaseUntil<now));
        if(!manual)claim=claim.Where(s=>s.Enabled&&(s.NextRunAt==null||s.NextRunAt<=now));
        if(await claim.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.LeaseToken,token).SetProperty(x=>x.LeaseUntil,now.AddMinutes(10)).SetProperty(x=>x.LastStartedAt,now).SetProperty(x=>x.LastStatus,"Running").SetProperty(x=>x.LastError,""),ct)==0)return null;
        var config=await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync(ct);
        var nextCursor=config.QueryCursor;
        try{
            var sources=await db.DigitalPlanSources.AsNoTracking().Where(x=>x.Enabled).OrderBy(x=>x.SortOrder).ThenBy(x=>x.Id).ToListAsync(ct);
            var keywords=await db.DigitalPlanKeywords.AsNoTracking().Where(x=>x.Enabled).OrderBy(x=>x.SortOrder).ThenBy(x=>x.Id).ToListAsync(ct);
            if(sources.Count==0||keywords.Count==0)throw new InvalidOperationException("Cần bật ít nhất một nguồn và một từ khóa.");
            var count=sources.Count*keywords.Count;var rows=new List<(DigitalPlanFound Row,DigitalPlanSource Source)>();var failures=0;var filtered=0;
            var queries=Math.Min(config.BatchSize,count);var cursor=config.QueryCursor%count;nextCursor=(cursor+queries)%count;
            for(var offset=0;offset<queries;offset++){
                var index=(cursor+offset)%count;var source=sources[index%sources.Count];var keyword=keywords[(index/sources.Count+index%sources.Count)%keywords.Count];
                try{foreach(var row in await reader.ReadAsync(source,keyword,config.PublishedFrom??DigitalNewsPublicationWindow.DefaultFrom,config.PublishedTo,ct))if(DigitalNewsPublicationWindow.Includes(row.PublishedAt,config.PublishedFrom??DigitalNewsPublicationWindow.DefaultFrom,config.PublishedTo)){if(DigitalNewsCollectionFilter.Matches(row.Title,keyword))rows.Add((row,source));else filtered++;}}
                catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}
                catch{failures++;}
            }
            if(failures==queries)throw new InvalidOperationException("Các nguồn tìm kiếm tạm thời không đáp ứng. Bài đã lưu được giữ nguyên.");
            await using var tx=await db.Database.BeginTransactionAsync(ct);
            // Lock only for the short commit phase; network IO never holds a database transaction.
            var settings=await db.DigitalNewsSyncSettings.FromSqlRaw("""SELECT * FROM "DigitalNewsSyncSettings" WHERE "Id"=1 FOR UPDATE""").SingleAsync(ct);
            await db.Entry(settings).ReloadAsync(ct);
            if(settings.LeaseToken!=token){await tx.RollbackAsync(ct);return null;}
            var existing=await db.DigitalNewsItems.ToListAsync(ct);
            var known=existing.ToDictionary(i=>i.UrlHash);var titleKeys=existing.Select(i=>DigitalPlanText.TitleKey(i.AgencyName,i.Title)).ToHashSet();
            var added=0;var unchanged=0;var completed=DateTime.UtcNow;
            foreach(var (row,source) in rows){
                var hash=DigitalPlanText.Hash(row.Url);var key=DigitalPlanText.TitleKey(source.AgencyName,row.Title);
                if(known.ContainsKey(hash)||!titleKeys.Add(key)){unchanged++;continue;}
                var item=new DigitalNewsItem{Url=row.Url,UrlHash=hash,TitleKey=key,AgencyName=source.AgencyName,Title=row.Title,PublishedAt=row.PublishedAt,OriginalUrlVerified=row.OriginalUrlVerified,ImportedAt=completed,UpdatedAt=completed};
                db.Add(item);known[hash]=item;added++;
            }
            settings.QueryCursor=(cursor+queries)%count;
            settings.Added=added;settings.Updated=0;settings.Unchanged=unchanged;settings.Skipped=failures+filtered;settings.LastStatus=failures>0?"Partial":"Completed";settings.LastError=failures>0?$"{failures}/{queries} truy vấn lỗi; sẽ tiếp tục ở lượt sau.":"";settings.LastCompletedAt=completed;settings.NextRunAt=settings.Enabled?completed.AddMinutes(settings.IntervalMinutes):null;settings.LeaseUntil=null;settings.LeaseToken=null;
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new(added,0,unchanged,failures,filtered);
        }catch(Exception ex){var error=ex is InvalidOperationException?ex.Message:ex is OperationCanceledException?"Lượt đồng bộ bị gián đoạn hoặc hết thời gian; dữ liệu cũ được giữ nguyên.":"Không kết nối được nguồn tìm kiếm hoặc không lưu được dữ liệu. Dữ liệu cũ được giữ nguyên.";
            await db.DigitalNewsSyncSettings.Where(s=>s.Id==1&&s.LeaseToken==token).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.LeaseToken,(Guid?)null).SetProperty(x=>x.LeaseUntil,(DateTime?)null).SetProperty(x=>x.QueryCursor,nextCursor).SetProperty(x=>x.LastStatus,"Error").SetProperty(x=>x.LastError,error).SetProperty(x=>x.NextRunAt,config.Enabled?(DateTime?)DateTime.UtcNow.AddMinutes(config.IntervalMinutes):null),CancellationToken.None);
            if(ex is OperationCanceledException&&ct.IsCancellationRequested)throw;
            throw new InvalidOperationException(error);
        }
    }
}
public sealed class DigitalNewsSyncWorker(IServiceScopeFactory scopes,ILogger<DigitalNewsSyncWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromMinutes(1));
        do{try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<DigitalNewsSyncService>().RunAsync(false,ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){break;}catch{logger.LogWarning("Digital plan discovery failed; details available to administrators.");}
            try{if(!await timer.WaitForNextTickAsync(ct))break;}catch(OperationCanceledException){break;}
        }while(!ct.IsCancellationRequested);
    }
}
