using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Application.Services;
public sealed record DigitalNewsSyncResult(int Added,int Updated,int Unchanged,int Skipped);
public sealed class DigitalNewsSyncService(AppDbContext db,IDigitalNewsSheetReader reader)
{
    public async Task SaveAsync(string sheetUrl,bool enabled,int intervalHours,string actor,CancellationToken ct)
    {
        var (url,_)=DigitalNewsSheetReader.ValidateSheetUrl(sheetUrl);
        if(intervalHours is <1 or >168)throw new InvalidOperationException("Chu kỳ đồng bộ từ 1 đến 168 giờ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var settings=await db.DigitalNewsSyncSettings.FromSqlRaw("""SELECT * FROM "DigitalNewsSyncSettings" WHERE "Id"=1 FOR UPDATE""").SingleAsync(ct);
        await db.Entry(settings).ReloadAsync(ct);
        if(settings.LeaseUntil>DateTime.UtcNow)throw new InvalidOperationException("Đang đồng bộ. Chờ hoàn tất trước khi đổi cấu hình.");
        var changed=settings.SheetUrl!=url||settings.Enabled!=enabled||settings.IntervalHours!=intervalHours;
        settings.SheetUrl=url;settings.Enabled=enabled;settings.IntervalHours=intervalHours;settings.UpdatedBy=actor;
        if(changed)settings.NextRunAt=enabled?DateTime.UtcNow:null;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task<DigitalNewsSyncResult?> RunAsync(bool manual,CancellationToken ct)
    {
        var now=DateTime.UtcNow;var token=Guid.NewGuid();
        var claim=db.DigitalNewsSyncSettings.Where(s=>s.Id==1&&(s.LeaseUntil==null||s.LeaseUntil<now));
        if(!manual)claim=claim.Where(s=>s.Enabled&&(s.NextRunAt==null||s.NextRunAt<=now));
        if(await claim.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.LeaseToken,token).SetProperty(x=>x.LeaseUntil,now.AddMinutes(10)).SetProperty(x=>x.LastStartedAt,now).SetProperty(x=>x.LastStatus,"Running").SetProperty(x=>x.LastError,""),ct)==0)return null;
        var config=await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync(ct);
        try{
            var sheet=await reader.ReadAsync(config.SheetUrl,ct);
            await using var tx=await db.Database.BeginTransactionAsync(ct);
            // Lock only for the short commit phase; network IO never holds a database transaction.
            var settings=await db.DigitalNewsSyncSettings.FromSqlRaw("""SELECT * FROM "DigitalNewsSyncSettings" WHERE "Id"=1 FOR UPDATE""").SingleAsync(ct);
            await db.Entry(settings).ReloadAsync(ct);
            if(settings.LeaseToken!=token){await tx.RollbackAsync(ct);return null;}
            var hashes=sheet.Rows.Select(r=>r.Hash).ToArray();var known=await db.DigitalNewsItems.Where(i=>hashes.Contains(i.UrlHash)).ToDictionaryAsync(i=>i.UrlHash,ct);
            var added=0;var updated=0;var unchanged=0;var completed=DateTime.UtcNow;
            foreach(var row in sheet.Rows){if(!known.TryGetValue(row.Hash,out var item)){item=new DigitalNewsItem{Url=row.Url,UrlHash=row.Hash,AgencyName=row.AgencyName,Title=row.Title,SourceRow=row.Row,SourceSheetUrl=config.SheetUrl,ImportedAt=completed,UpdatedAt=completed};db.Add(item);added++;}
                else if(item.Title!=row.Title||item.AgencyName!=row.AgencyName){item.Title=row.Title;item.AgencyName=row.AgencyName;item.SourceRow=row.Row;item.UpdatedAt=completed;updated++;}else unchanged++;}
            settings.Added=added;settings.Updated=updated;settings.Unchanged=unchanged;settings.Skipped=sheet.Skipped;settings.LastStatus="Completed";settings.LastError="";settings.LastCompletedAt=completed;settings.NextRunAt=settings.Enabled?completed.AddHours(settings.IntervalHours):null;settings.LeaseUntil=null;settings.LeaseToken=null;
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return new(added,updated,unchanged,sheet.Skipped);
        }catch(Exception ex){var error=ex is InvalidOperationException?ex.Message:ex is OperationCanceledException?"Lượt đồng bộ bị gián đoạn hoặc hết thời gian; dữ liệu cũ được giữ nguyên.":"Không kết nối được Google Sheets hoặc không lưu được dữ liệu. Dữ liệu cũ được giữ nguyên.";
            await db.DigitalNewsSyncSettings.Where(s=>s.Id==1&&s.LeaseToken==token).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.LeaseToken,(Guid?)null).SetProperty(x=>x.LeaseUntil,(DateTime?)null).SetProperty(x=>x.LastStatus,"Error").SetProperty(x=>x.LastError,error).SetProperty(x=>x.NextRunAt,config.Enabled?(DateTime?)DateTime.UtcNow.AddHours(config.IntervalHours):null),CancellationToken.None);
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
        do{try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<DigitalNewsSyncService>().RunAsync(false,ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){break;}catch{logger.LogWarning("Digital news sheet synchronization failed; details available to administrators.");}
            try{if(!await timer.WaitForNextTickAsync(ct))break;}catch(OperationCanceledException){break;}
        }while(!ct.IsCancellationRequested);
    }
}
