using System.Security.Cryptography;
using System.Text.Json;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cdsqg.Application.AI;
public sealed class AiCollectionService(AppDbContext db, IAiSourceReader reader, IAiMetricExtractor extractor, IOptions<AiDataOptions> settings,AiSettingsStore? savedSettings=null)
{
    public async Task RunAsync(Guid sourceId, CancellationToken ct)
    {
        var now=DateTime.UtcNow;
        // Atomic lease shared by all application instances. Expired leases recover after a restart.
        var claimed=await db.AiDataSources.Where(s=>s.Id==sourceId && (s.LeaseUntil==null || s.LeaseUntil<now))
            .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.LeaseUntil, now.AddHours(2)).SetProperty(x=>x.LastStatus,"Đang thu thập"),ct);
        if (claimed==0) return;
        var source=await db.AiDataSources.SingleAsync(s=>s.Id==sourceId,ct);
        try
        {
            var metric = await AiMetricCatalog.ResolveAsync(db, source.MetricKey, ct);
            var first=await reader.ReadAsync(source.Url,ct);
            await ProcessAsync(source,metric,first,ct);
            var links=first.Links.Where(l=>l.Contains(".pdf",StringComparison.OrdinalIgnoreCase) || l.Contains("/download",StringComparison.OrdinalIgnoreCase))
                .Take(Math.Clamp(settings.Value.MaxDocumentsPerRun,1,5)-1);
            foreach(var link in links) { var doc=await reader.ReadAsync(link,ct); await ProcessAsync(source,metric,doc,ct); }
            source.LastStatus="Hoàn tất"; source.LastError="";
        }
        catch (OperationCanceledException) when(ct.IsCancellationRequested) { source.LastStatus="Đã dừng; sẽ thử lại"; }
        catch (Exception ex)
        {
            source.LastStatus="Lỗi thu thập";
            source.LastError=ex is InvalidOperationException ? ex.Message : "Không đọc được nguồn hoặc AI. Kiểm tra URL, quyền truy cập và cấu hình nhà cung cấp rồi thử lại.";
        }
        finally
        {
            source.LeaseUntil=null;source.LastCheckedAt=DateTime.UtcNow;
            source.NextCheckAt=DateTime.UtcNow.AddHours(Math.Clamp(source.IntervalHours,24,8760));
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
    public async Task<int> ProcessAsync(AiDataSource source,AiMetric metric,CollectedDocument collected,CancellationToken ct,bool reprocess=false,Guid? inboxId=null)
    {
        var hash=Convert.ToHexString(SHA256.HashData(collected.Bytes));
        var document=reprocess?null:await db.AiSourceDocuments.FirstOrDefaultAsync(d=>d.SourceId==source.Id && d.Url==collected.Url && d.ContentHash==hash,ct);
        if(document?.Status=="Extracted"&&!reprocess) return await db.AiDataSuggestions.CountAsync(s=>s.DocumentId==document.Id,ct);
        if(document==null) {
            document=new AiSourceDocument {InboxItemId=inboxId,SourceId=source.Id,Url=collected.Url,Title=collected.Title,ContentHash=hash,PagesJson=JsonSerializer.Serialize(collected.Pages)};
            db.Add(document);await db.SaveChangesAsync(ct);
        }
        document.PagesJson=JsonSerializer.Serialize(collected.Pages);
        try {
            var values=await extractor.ExtractAsync(metric,collected.Pages,ct);
            var invalid=0;var invalidReasons=new List<string>();
            foreach(var value in values.Take(20)) {
                try {AiMetricExtractor.Validate(value,metric,collected.Pages,Cdsqg.Application.Services.ReportingPeriod.CurrentYear);}
                catch(InvalidOperationException ex) {invalid++;invalidReasons.Add($"Trang PDF AI trả {value.Page}: {ex.Message}");continue;}
                var targetAgencyId=metric.Scopes?.Single(s=>string.Equals(s.Name,value.Scope,StringComparison.OrdinalIgnoreCase)).AgencyId;
                var methodology=value.Methodology[..Math.Min(2000,value.Methodology.Length)];
                if(await db.AiDataSuggestions.AnyAsync(s=>s.DocumentId==document.Id && s.MetricKey==metric.Key && s.DataYear==value.DataYear && s.Value==value.Value && s.TargetAgencyId==targetAgencyId,ct))continue;
                var conflict=await db.AiDataSuggestions.AnyAsync(s=>s.MetricKey==metric.Key && s.DataYear==value.DataYear && s.TargetAgencyId==targetAgencyId && s.Status!="Rejected" && (s.AcceptedValue??s.Value)!=value.Value,ct);
                var warnings=new List<string>{"Admin phải kiểm tra kỳ cả năm, phạm vi và định nghĩa chỉ tiêu trước khi duyệt."};
                var sameValueQuotes=await db.AiDataSuggestions.Where(s=>s.MetricKey==metric.Key&&s.DataYear==value.DataYear&&s.TargetAgencyId==targetAgencyId&&s.Value==value.Value).Select(s=>s.Quote).ToListAsync(ct);
                if(sameValueQuotes.Any(q=>AiMetricExtractor.Normalize(q)==AiMetricExtractor.Normalize(value.Quote)))warnings.Add("Có dẫn chứng/số liệu trùng đã tiếp nhận; không coi các bài đăng lại là kết quả độc lập.");
                if(conflict)warnings.Add("Có số liệu khác cho cùng chỉ số và năm; cần đối chiếu phương pháp/phiên bản.");
                if(value.DataYear<2026&&!metric.Internet)warnings.Add("Số liệu trước giai đoạn theo dõi; chỉ dùng làm tham chiếu lịch sử.");
                if(!string.IsNullOrWhiteSpace(value.Warnings))warnings.Add(value.Warnings[..Math.Min(value.Warnings.Length,1000)]);
                var suggestion=new AiDataSuggestion{DocumentId=document.Id,MetricKey=metric.Key,DataYear=value.DataYear,PeriodLabel=value.PeriodLabel[..Math.Min(250,value.PeriodLabel.Length)],Value=value.Value,Unit=metric.Unit,Scope=targetAgencyId.HasValue?metric.Scopes!.Single(s=>s.AgencyId==targetAgencyId).Name:"Việt Nam",TargetAgencyId=targetAgencyId,Quote=value.Quote,Page=value.Page,Methodology=value.Methodology[..Math.Min(2000,value.Methodology.Length)],Warnings=string.Join(" ",warnings)};
                db.Add(suggestion);
                var goals=db.GoalTaskItems.Where(g=>g.ItemType==Cdsqg.Core.Enums.ItemTypeEnum.Goal&&(metric.Key.StartsWith("GOAL_")?g.Id==Guid.Parse(metric.Key.Substring(5)):metric.GoalCodes.Contains(g.Code)));
                if(metric.Internet) {
                    if(!metric.Key.StartsWith("GOAL_"))goals=goals.Where(g=>g.DocumentId==Guid.Parse("12660000-0000-0000-0000-000000001266")||g.Document!.DocumentNumber=="1266/QĐ-TTg");
                    goals=goals.Where(g=>(!g.StartDate.HasValue||g.StartDate.Value.Year<=value.DataYear)&&(!g.DueDate.HasValue||g.DueDate.Value.Year>=value.DataYear));
                    if(targetAgencyId.HasValue)goals=goals.Where(g=>g.AgencyExecutions.Any(e=>e.AgencyId==targetAgencyId.Value&&e.IsLeadAssignment));
                }
                var goalIds=await goals.Select(g=>g.Id).ToListAsync(ct);
                if(metric.Internet&&goalIds.Count==0)suggestion.Warnings+=" Năm số liệu hoặc phạm vi ngoài phân giao/thời gian thực hiện mục tiêu; chỉ tham khảo, chưa thể ghi báo cáo.";
                foreach(var goalId in goalIds)db.Add(new AiGoalDataLink{GoalTaskId=goalId,SuggestionId=suggestion.Id});
                await db.SaveChangesAsync(ct);
            }
            var accepted=await db.AiDataSuggestions.CountAsync(s=>s.DocumentId==document.Id,ct);
            document.Status=accepted>0?"Extracted":invalid>0?"EvidenceRejected":"NoData";document.Error=invalid>0?$"Đã bỏ qua {invalid} kết quả không khớp dẫn chứng hoặc không hợp lệ. "+string.Join(" ",invalidReasons.Distinct().Take(3)):"";document.Model=(savedSettings==null?settings.Value:await savedSettings.GetAsync(ct)).Model;
        }
        catch(Exception ex) {
            // A failed insert remains Added in EF. Do not retry that insert while recording the failure.
            var failedIds=db.ChangeTracker.Entries<AiDataSuggestion>().Where(e=>e.State==EntityState.Added&&e.Entity.DocumentId==document.Id).Select(e=>e.Entity.Id).ToHashSet();
            foreach(var entry in db.ChangeTracker.Entries<AiGoalDataLink>().Where(e=>e.State==EntityState.Added&&failedIds.Contains(e.Entity.SuggestionId)).ToList())entry.State=EntityState.Detached;
            foreach(var entry in db.ChangeTracker.Entries<AiDataSuggestion>().Where(e=>e.State==EntityState.Added&&failedIds.Contains(e.Entity.Id)).ToList())entry.State=EntityState.Detached;
            document.Status="Error";document.Error=AiProviderRequest.Error(ex);
            await db.AiSourceDocuments.Where(d=>d.Id==document.Id).ExecuteUpdateAsync(s=>s.SetProperty(d=>d.Status,"Error").SetProperty(d=>d.Error,document.Error),CancellationToken.None);
            throw;
        }
        await db.SaveChangesAsync(ct);
        return await db.AiDataSuggestions.CountAsync(s=>s.DocumentId==document.Id,ct);
    }
}
public sealed class AiCollectionWorker(IServiceScopeFactory scopes,IOptions<AiDataOptions> settings,ILogger<AiCollectionWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(30));
        while(!stoppingToken.IsCancellationRequested) {
            try {
                if(settings.Value.Ready) {
                    using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();var now=DateTime.UtcNow;
                    var ids=await db.AiDataSources.AsNoTracking().Where(s=>s.Enabled && s.NextCheckAt!=null && s.NextCheckAt<=now && (s.LeaseUntil==null || s.LeaseUntil<now)).OrderBy(s=>s.NextCheckAt).Select(s=>s.Id).Take(3).ToListAsync(stoppingToken);
                    foreach(var id in ids)await scope.ServiceProvider.GetRequiredService<AiCollectionService>().RunAsync(id,stoppingToken);
                }
            } catch(OperationCanceledException)when(stoppingToken.IsCancellationRequested){break;}
            catch {logger.LogWarning("AI source collector could not finish this cycle; retrying on next cycle.");}
            try {if(!await timer.WaitForNextTickAsync(stoppingToken))break;}catch(OperationCanceledException){break;}
        }
    }
}



