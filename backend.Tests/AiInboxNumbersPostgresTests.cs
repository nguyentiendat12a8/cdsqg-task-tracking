using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class AiInboxNumbersPostgresTests
{
    [PostgresFact]
    public async Task MigrationBackfillsAndConcurrentInsertsKeepUniqueStableNumbers()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "ai_numbers_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await cmd.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.GetService<IMigrator>().MigrateAsync("20261019155617_AddAiAlertRssSources");
            var oldId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "AiInboxItems" ("Id","Url","UrlHash","Title","Origin","ReceivedAt","Status","MetricKeysJson","Reason","Error","Model","StorageKey")
                VALUES ({oldId},'','old','Old report','Upload',CURRENT_TIMESTAMP,'Read','[]','','','','file')
                """);
            await db.Database.MigrateAsync();
            var now=new DateTime(2026,10,20,12,0,0,DateTimeKind.Utc);
            var gates=await Task.WhenAll(Enumerable.Range(0,6).Select(async _=>{
                await using var peer=new AppDbContext(options);
                return await new Cdsqg.Application.AI.AiInboxQueueGate(peer).TryAcquireAsync(now,default);
            }));
            Assert.Single(gates,x=>x.HasValue);
            var owner=gates.Single(x=>x.HasValue)!.Value;
            var gate=new Cdsqg.Application.AI.AiInboxQueueGate(db);
            Assert.Null(await gate.TryAcquireAsync(now.AddMinutes(10),default));
            await gate.CompleteAsync(Guid.NewGuid(),now,default);
            Assert.Null(await gate.TryAcquireAsync(now.AddMinutes(10),default));
            await gate.CompleteAsync(owner,now.AddSeconds(15),default);
            await using(var restarted=new AppDbContext(options)){
                var restartedGate=new Cdsqg.Application.AI.AiInboxQueueGate(restarted);
                Assert.Null(await restartedGate.TryAcquireAsync(now.AddSeconds(74),default));
                var next=await restartedGate.TryAcquireAsync(now.AddSeconds(75),default);
                Assert.NotNull(next);
                await restartedGate.CompleteAsync(next.Value,now.AddSeconds(80),default);
            }
            var old = await db.AiInboxItems.SingleAsync();
            Assert.Equal(1, old.SequenceNumber); Assert.Equal(oldId, old.Id); Assert.Equal("Read", old.Status);
            var numbers = await Task.WhenAll(Enumerable.Range(0, 12).Select(async n => {
                await using var writer = new AppDbContext(options);
                var item = new AiInboxItem { UrlHash = "new" + n, Title = "New" };
                writer.Add(item); await writer.SaveChangesAsync(); return item.SequenceNumber;
            }));
            Assert.Equal(12, numbers.Distinct().Count()); Assert.All(numbers, n => Assert.True(n > 1));
            var listController=new Cdsqg.Api.Controllers.AiInboxController(db,null!);
            var expectedIds=await db.AiInboxItems.AsNoTracking().OrderByDescending(i=>i.ReceivedAt).ThenByDescending(i=>i.SequenceNumber).Select(i=>i.Id).ToListAsync();
            var actualIds=new List<Guid>();
            for(var page=1;page<=3;page++){
                var resultPage=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await listController.List(page:page));
                using var data=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(resultPage.Value));
                Assert.Equal(5,data.RootElement.GetProperty("pageSize").GetInt32());
                Assert.Equal(13,data.RootElement.GetProperty("total").GetInt32());
                var pageItems=data.RootElement.GetProperty("items");Assert.Equal(page==3?3:5,pageItems.GetArrayLength());
                actualIds.AddRange(pageItems.EnumerateArray().Select(x=>x.GetProperty("Id").GetGuid()));
            }
            Assert.Equal(expectedIds,actualIds);Assert.Equal(13,actualIds.Distinct().Count());
            foreach(var size in new[]{10,20,0,500}){
                var responseSize=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await listController.List(page:1,pageSize:size));
                using var sizeData=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(responseSize.Value));
                var effective=Math.Clamp(size,1,100);Assert.Equal(effective,sizeData.RootElement.GetProperty("pageSize").GetInt32());
                Assert.Equal(Math.Min(13,effective),sizeData.RootElement.GetProperty("items").GetArrayLength());
            }
            var uploads=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await listController.List(uploadsOnly:true));
            using(var data=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(uploads.Value))){Assert.Equal(1,data.RootElement.GetProperty("total").GetInt32());Assert.Equal(oldId,data.RootElement.GetProperty("items")[0].GetProperty("Id").GetGuid());}

            old.Status = "ReadQueued"; await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            Assert.Equal(1, (await db.AiInboxItems.SingleAsync(i => i.Id == oldId)).SequenceNumber);
            var controller=new Cdsqg.Api.Controllers.AiInboxController(db,null!);
            Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(await controller.Queue(oldId,new("EGDI_RANK"),default));
            var completed=await db.AiInboxItems.SingleAsync(i=>i.Id==oldId);completed.Status="Read";await db.SaveChangesAsync();
            var result=await controller.Queue(oldId,new("EGDI_RANK"),default);
            Assert.IsType<Microsoft.AspNetCore.Mvc.AcceptedResult>(result);
            var version=await db.AiInboxItems.SingleAsync(i=>i.UrlHash.StartsWith("reread:"));
            Assert.NotEqual(oldId,version.Id);Assert.Equal("file",version.StorageKey);Assert.Equal("ReadQueued",version.Status);
            Assert.Equal("Read",(await db.AiInboxItems.SingleAsync(i=>i.Id==oldId)).Status);
            var source=new AiDataSource{Url="https://www.nso.gov.vn/report",MetricKey="DIGITAL_GDP_SHARE"};db.Add(source);await db.SaveChangesAsync();
            var collection=new Cdsqg.Application.AI.AiCollectionService(db,null!,new EmptyExtractor(),Microsoft.Extensions.Options.Options.Create(new Cdsqg.Application.AI.AiDataOptions()));
            var metric=Cdsqg.Application.AI.AiMetricCatalog.Get("DIGITAL_GDP_SHARE");
            var document=new Cdsqg.Application.AI.CollectedDocument(source.Url,"Test",[1,2,3],[new(1,"Test report")],[]);
            await collection.ProcessAsync(source,metric,document,default,reprocess:true,inboxId:version.Id);
            var first=await db.AiSourceDocuments.SingleAsync();
            first.Error="Trang 1: dẫn chứng không hợp lệ.";await db.SaveChangesAsync();
            var explained=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await listController.List(pageSize:100));
            using(var reasonJson=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(explained.Value))){
                var entry=reasonJson.RootElement.GetProperty("items").EnumerateArray().Single(i=>i.GetProperty("Id").GetGuid()==version.Id);
                Assert.Equal(first.Error,entry.GetProperty("processingDetails").GetString());
            }
            first.Error="";await db.SaveChangesAsync();
            db.Add(new AiDataSuggestion{DocumentId=first.Id,MetricKey=metric.Key,Status="Approved",Value=10});await db.SaveChangesAsync();
            await collection.ProcessAsync(source,metric,document,default,reprocess:true,inboxId:version.Id);
            Assert.Equal(2,await db.AiSourceDocuments.CountAsync());
            Assert.All(await db.AiSourceDocuments.ToListAsync(),d=>Assert.Equal(version.Id,d.InboxItemId));
            var dataController=new Cdsqg.Api.Controllers.AiDataController(db,null!,null!);
            var response=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await dataController.Suggestions(inboxId:version.Id));
            var json=System.Text.Json.JsonSerializer.Serialize(response.Value);
            Assert.Contains("inputSequence",json);Assert.Contains("inputType",json);Assert.Contains("upload",json);

            Assert.Equal("Approved",(await db.AiDataSuggestions.SingleAsync()).Status);
            Assert.Equal(first.Id,(await db.AiDataSuggestions.SingleAsync()).DocumentId);
            var dedup=new Cdsqg.Application.AI.AiCollectionService(db,null!,new RepeatedExtractor(),Microsoft.Extensions.Options.Options.Create(new Cdsqg.Application.AI.AiDataOptions()));
            var repeated=new Cdsqg.Application.AI.CollectedDocument(source.Url,"Repeated",[4,5],[new(1,"Viet Nam GDP 10 percent, official 2024"),new(2,"Viet Nam GDP 10 percent, official 2024")],[]);
            Assert.Equal(1,await dedup.ProcessAsync(source,metric,repeated,default,reprocess:true,inboxId:version.Id));
            var feed=new AiAlertFeed{Name="Failure test",FeedUrl="https://www.google.com/alerts/feeds/1/2"};db.Add(feed);
            var failedInput=new AiInboxItem{AlertFeedId=feed.Id,Url="https://www.nso.gov.vn/report",UrlHash="failure-test",Status="ReadQueued",MetricKeysJson="[\"DIGITAL_GDP_SHARE\"]"};db.Add(failedInput);await db.SaveChangesAsync();
            var sourceRow=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await listController.List(alertsOnly:true));
            using(var sourceJson=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(sourceRow.Value))){
                var row=sourceJson.RootElement.GetProperty("items")[0];
                Assert.Equal(feed.Name,row.GetProperty("feedName").GetString());
                Assert.Equal(feed.FeedUrl,row.GetProperty("feedUrl").GetString());
            }
            var inbox=new Cdsqg.Application.AI.AiInboxService(db,null!,new FailingReader(db),null!,null!,Microsoft.Extensions.Options.Options.Create(new Cdsqg.Application.AI.AiDataOptions{Enabled=true,ApiKey="fixture",Model="fixture"}));
            await inbox.ProcessQueuedAsync(failedInput.Id,default);
            var terminal=await db.AiInboxItems.AsNoTracking().SingleAsync(i=>i.Id==failedInput.Id);
            Assert.Equal("Error",terminal.Status);Assert.Null(terminal.LeaseUntil);Assert.Contains("cơ sở dữ liệu",terminal.Error);
            Assert.Null((await db.Set<AiInboxQueueState>().AsNoTracking().SingleAsync()).Owner);
            foreach(var entry in db.ChangeTracker.Entries<AiDataSuggestion>().Where(e=>e.State==EntityState.Added).ToList())entry.State=EntityState.Detached;
            var feedsService=new Cdsqg.Application.AI.AiAlertService(db,null!,null!);
            var inputCount=await db.AiInboxItems.CountAsync();var evidenceCount=await db.AiDataSuggestions.CountAsync();
            await feedsService.SetDeletedAsync(feed.Id,true,"Admin",default);
            Assert.NotNull(feed.DeletedAt);Assert.False(feed.Enabled);Assert.Null(feed.NextScanAt);
            await Assert.ThrowsAsync<KeyNotFoundException>(()=>feedsService.EnqueueAsync(feed.Id,"Manual","Admin",default));
            Assert.Equal(inputCount,await db.AiInboxItems.CountAsync());Assert.Equal(evidenceCount,await db.AiDataSuggestions.CountAsync());
            await feedsService.SetDeletedAsync(feed.Id,false,"Admin",default);Assert.Null(feed.DeletedAt);Assert.False(feed.Enabled);
            var scan=await feedsService.EnqueueAsync(feed.Id,"Manual","Admin",default);Assert.NotNull(scan);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>feedsService.SetDeletedAsync(feed.Id,true,"Admin",default));
            for(var n=0;n<12;n++){
                db.Add(new AiSourceDocument{SourceId=source.Id,Title="History "+n,RetrievedAt=now.AddDays(n/2)});
                db.Add(new AiAlertScan{FeedId=feed.Id,Status="Completed",QueuedAt=now.AddDays(n/2)});
            }
            await db.SaveChangesAsync();
            var historyController=new Cdsqg.Api.Controllers.AiAlertsController(db,feedsService,null!);
            foreach(var documentsHistory in new[]{true,false}){
                var expected=documentsHistory?await db.AiSourceDocuments.OrderByDescending(d=>d.RetrievedAt).ThenByDescending(d=>d.Id).Select(d=>d.Id).ToListAsync():await db.AiAlertScans.Where(r=>r.FeedId==feed.Id).OrderByDescending(r=>r.QueuedAt).ThenByDescending(r=>r.Id).Select(r=>r.Id).ToListAsync();
                var seen=new List<Guid>();
                for(var p=1;p<=Math.Ceiling(expected.Count/5d);p++){
                    var responseHistory=documentsHistory?await dataController.Documents(page:p):await historyController.Scans(feedId:feed.Id,page:p);
                    var ok=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(responseHistory);
                    using var data=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(ok.Value));
                    Assert.Equal(expected.Count,data.RootElement.GetProperty("total").GetInt32());Assert.Equal(5,data.RootElement.GetProperty("pageSize").GetInt32());
                    Assert.Equal(Math.Min(5,expected.Count-(p-1)*5),data.RootElement.GetProperty("items").GetArrayLength());
                    seen.AddRange(data.RootElement.GetProperty("items").EnumerateArray().Select(i=>i.GetProperty("Id").GetGuid()));
                }
                Assert.Equal(expected,seen);
            }





        }
        finally { await using var cmd = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin); await cmd.ExecuteNonQueryAsync(); }
    }
    sealed class FailingReader(AppDbContext db):Cdsqg.Application.AI.IAiSourceReader
    {
        public async Task<Cdsqg.Application.AI.CollectedDocument> ReadAsync(string url,CancellationToken ct)
        {
            var existing=await db.AiDataSuggestions.AsNoTracking().FirstAsync(s=>s.Status=="Approved",ct);
            db.Add(new AiDataSuggestion{DocumentId=existing.DocumentId,MetricKey=existing.MetricKey,DataYear=existing.DataYear,Value=existing.Value,Page=existing.Page});
            throw new DbUpdateException("Injected failed save with pending duplicate");
        }
    }
    sealed class RepeatedExtractor:Cdsqg.Application.AI.IAiMetricExtractor
    {
        public Task<List<Cdsqg.Application.AI.ExtractedMetric>> ExtractAsync(Cdsqg.Application.AI.AiMetric m,List<Cdsqg.Application.AI.SourcePage> pages,CancellationToken ct)=>Task.FromResult(pages.SelectMany(p=>new[]{new Cdsqg.Application.AI.ExtractedMetric{DataYear=2024,Value=10,Unit="%",Scope="Việt Nam",Page=p.Number,Quote=p.Text,Methodology="National GDP"},new Cdsqg.Application.AI.ExtractedMetric{DataYear=2024,Value=10,Unit="%",Scope="Việt Nam",Page=p.Number,Quote=p.Text,Methodology="Different wording"}}).ToList());
    }
    sealed class EmptyExtractor:Cdsqg.Application.AI.IAiMetricExtractor
    {
        public Task<List<Cdsqg.Application.AI.ExtractedMetric>> ExtractAsync(Cdsqg.Application.AI.AiMetric m,List<Cdsqg.Application.AI.SourcePage> pages,CancellationToken ct)=>Task.FromResult(new List<Cdsqg.Application.AI.ExtractedMetric>());
    }
}

