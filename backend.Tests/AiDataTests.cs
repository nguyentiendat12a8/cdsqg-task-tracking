using Cdsqg.Application.AI;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
using Cdsqg.Core.Enums;
using Cdsqg.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Cdsqg.Api.Controllers;
using Microsoft.AspNetCore.DataProtection;

namespace Cdsqg.Tests;
public class AiDataTests
{
    [Theory]
    [InlineData("https://moit.gov.vn/report.pdf", true)]
    [InlineData("https://moit.gov.vn.evil.test/report.pdf", false)]
    [InlineData("https://127.0.0.1/report.pdf", false)]
    [InlineData("http://moit.gov.vn/report.pdf", false)]
    [InlineData("https://user@moit.gov.vn/report.pdf", false)]
    public void SourceHostsAreExplicit(string url, bool allowed) => Assert.Equal(allowed, AiMetricCatalog.IsAllowedUrl(url));

    [Fact]
    public void EvidenceMustContainExactValueAndPage()
    {
        var metric=AiMetricCatalog.Get("DIGITAL_GDP_SHARE");
        var pages=new List<SourcePage>{new(2,"Năm 2025, kinh tế số Việt Nam chiếm 30,5% GDP.")};
        var value=new ExtractedMetric{DataYear=2025,Value=30.5m,Unit="%",Scope="Việt Nam",Page=2,Quote=pages[0].Text};
        AiMetricExtractor.Validate(value,metric,pages,2026);
        value.Value=30;
        Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        value.Value=30.5m;value.IsForecastOrTarget=true;
        Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        value.IsForecastOrTarget=false;value.Page=1;
        Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
    }

    [Fact]
    public void RankTableNearEndIsSelectedBeforeNarrativeMentions()
    {
        var metric=AiMetricCatalog.Get("EGDI_RANK");
        var pages=new List<SourcePage>{new(66,"Viet Nam improved in digital government."),new(69,"Viet Nam has high OSI."),new(73,"Viet Nam has very high TII."),new(204,"2024 Annex EGDI\nRank LOSI\nRank Viet Nam V1 71 53 0.7709")};
        var selected=AiMetricExtractor.SelectChunks(metric,pages,3);
        Assert.Equal(204,selected[0].Number);
        Assert.Equal(3,selected.Count);
        Assert.Equal(pages[3].Text,selected[0].Text);
        AiMetricExtractor.Validate(new ExtractedMetric{DataYear=2024,Value=71,Unit="rank",Scope="Viet Nam",Page=204,Quote="Viet Nam V1 71 53 0.7709"},metric,pages,2026);
    }
    [Fact]
    public async Task ExtractionConstrainsCountryAndPreservesEvidence()
    {
        var handler=new ExtractionHandler();
        var metric=AiMetricCatalog.Get("EGDI_RANK");
        var pages=new List<SourcePage>{new(204,"2024 Annex EGDI Rank LOSI Rank\nViet Nam V1 71 53 0.7709")};
        var extractor=new AiMetricExtractor(Options.Create(new AiDataOptions{Enabled=true,ApiKey="unit-key-not-real",Model="test-model",BaseUrl="https://provider.test/v1"}),handler);
        var values=await extractor.ExtractAsync(metric,pages,default);
        var value=Assert.Single(values);
        AiMetricExtractor.Validate(value,metric,pages,2026);
        using var body=System.Text.Json.JsonDocument.Parse(handler.Body);
        Assert.Equal("Việt Nam",body.RootElement.GetProperty("response_format").GetProperty("json_schema").GetProperty("schema").GetProperty("properties").GetProperty("observations").GetProperty("items").GetProperty("properties").GetProperty("scope").GetProperty("enum")[0].GetString());
        Assert.Equal(71,value.Value);Assert.Equal(204,value.Page);
        value.Scope="Toàn quốc";
        Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
    }
    private sealed class ExtractionHandler:HttpMessageHandler,IHttpClientFactory
    {
        public string Body="";
        public HttpClient CreateClient(string name)=>new(this,false);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Body=await request.Content!.ReadAsStringAsync(ct);
            var content=System.Text.Json.JsonSerializer.Serialize(new{observations=new[]{new{dataYear=2024,periodLabel="UN 2024",value=71,unit="rank",scope="Việt Nam",quote="Viet Nam V1 71 53 0.7709",page=178,methodology="UN survey",isForecastOrTarget=false,warnings=""}}});
            return new(System.Net.HttpStatusCode.OK){Content=System.Net.Http.Json.JsonContent.Create(new{choices=new[]{new{finish_reason="stop",message=new{content}}}})};
        }
    }
    [PostgresFact]
    public async Task ReviewPreservesRawEvidenceAndHistoryInSeparateTables()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="ai_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();
        await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
        try
        {
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
            Assert.Equal(3,await db.AiDataSources.CountAsync());
            Assert.False(await db.AiDataSources.AnyAsync(s=>s.Enabled));
            var agency=new Agency{Code="AI_TEST",Name="AI test",Type=AgencyTypeEnum.Ministry,IsActive=true};
            var doc=new Document{DocumentNumber="AI_TEST",StartYear=2026,EndYear=2030};
            var goal=new GoalTaskItem{DocumentId=doc.Id,LeadAgencyId=agency.Id,ItemType=ItemTypeEnum.Goal,Code="MT-13",Title="Kinh tế số"};
            db.AddRange(agency,doc,goal,new AgencyTaskExecution{GoalTaskId=goal.Id,AgencyId=agency.Id,IsLeadAssignment=true});
            await db.SaveChangesAsync();
            var goalCount=await db.GoalTaskItems.CountAsync();var progressCount=await db.ProgressLogs.CountAsync();
            var source=await db.AiDataSources.FirstAsync();
            var document=new AiSourceDocument{SourceId=source.Id,Url=source.Url,ContentHash="test",PagesJson="[]"};
            db.Add(document);
            var first=new AiDataSuggestion{DocumentId=document.Id,MetricKey="DIGITAL_GDP_SHARE",DataYear=2025,Value=18.2m,Unit="%",Quote="Original evidence"};
            var second=new AiDataSuggestion{DocumentId=document.Id,MetricKey=first.MetricKey,DataYear=2025,Value=19m,Unit="%",Page=2};
            db.AddRange(first,second,new AiGoalDataLink{GoalTaskId=goal.Id,SuggestionId=first.Id},new AiGoalDataLink{GoalTaskId=goal.Id,SuggestionId=second.Id});await db.SaveChangesAsync();
            var review=new AiReviewService(db,new ExecutionService(db,new NoFiles()));
            await review.ReviewAsync(first.Id,true,18.3m,"","Admin A",default);
            Assert.Equal("",first.ReviewNote);Assert.Equal(18.2m,first.Value);Assert.Equal("Original evidence",first.Quote);Assert.Equal(18.3m,first.AcceptedValue);
            await review.ReviewAsync(second.Id,true,null,"Phiên bản công bố mới","Admin B",default);
            Assert.False(first.IsCurrent);Assert.True(second.IsCurrent);Assert.Equal("Admin A",first.ReviewedBy);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>review.ReviewAsync(second.Id,false,null,"Không duyệt lại","Admin C",default));
            Assert.Equal(goalCount,await db.GoalTaskItems.CountAsync());Assert.Equal(progressCount+2,await db.ProgressLogs.CountAsync());
            Assert.Equal(19m,ProgressCalculator.LatestApproved(goal,agency.Id,2025)!.QuantitativeValue);
            Assert.All(await db.AiGoalDataLinks.ToListAsync(),l=>Assert.NotNull(l.ProgressLogId));
            var reports=await db.ProgressLogs.Where(p=>p.GoalTaskId==goal.Id).ToListAsync();
            Assert.All(reports,p=>Assert.Equal(ApprovalStatusEnum.Approved,p.ApprovalStatus));
            Assert.Contains(reports,p=>p.QuantitativeValue==18.3m&&p.ApprovedBy=="Admin A");
            Assert.Equal(1,await db.AiDataSuggestions.CountAsync(s=>s.IsCurrent));
            var rejected=new AiDataSuggestion{DocumentId=document.Id,MetricKey=first.MetricKey,DataYear=2025,Value=17m,Unit="%",Page=3};
            db.Add(rejected);await db.SaveChangesAsync();
            await review.ReviewAsync(rejected.Id,false,null,null,"Admin A",default);
            Assert.Equal("Rejected",rejected.Status);Assert.Equal("",rejected.ReviewNote);

            var inbox=new AiInboxService(db,new TestEnvironment(),null!,new AiCollectionService(db,null!,new FixtureExtractor(),Options.Create(new AiDataOptions())),null!,Options.Create(new AiDataOptions()));
            var alerts=new List<AlertInput>{new("Báo cáo kinh tế số","https://www.nso.gov.vn/test?utm_source=alerts")};
            Assert.Equal(1,await inbox.ImportAsync(alerts,default));Assert.Equal(0,await inbox.ImportAsync(alerts,default));
            Assert.Equal("TitleQueued",(await db.AiInboxItems.SingleAsync()).Status);
            var bytes=System.Text.Encoding.UTF8.GetBytes("Năm 2025 kinh tế số Việt Nam chiếm 18,2% GDP toàn quốc.");
            using var stream=new MemoryStream(bytes);var file=new FormFile(stream,0,bytes.Length,"file","bao-cao.txt");
            var uploaded=await inbox.UploadAsync(file,"DIGITAL_GDP_SHARE","Admin test",default);
            Assert.Equal("ReadQueued",uploaded.Status);Assert.Equal(2,await db.ProgressLogs.CountAsync());
            await inbox.ReadAsync(uploaded,default);await db.SaveChangesAsync();
            Assert.Equal("Read",uploaded.Status);Assert.Equal(2,await db.ProgressLogs.CountAsync()); // Reading never publishes.
            Assert.Contains(await db.AiDataSuggestions.ToListAsync(),s=>s.Status=="Pending"&&s.Value==18.2m);
            uploaded.LeaseUntil=DateTime.UtcNow.AddMinutes(10);await db.SaveChangesAsync();
            var controller=new AiInboxController(db,inbox);
            var response=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.List(uploadsOnly:true));
            using(var json=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(response.Value)))
                Assert.False(json.RootElement.GetProperty("items").EnumerateArray().Single(i=>i.GetProperty("Id").GetGuid()==uploaded.Id).GetProperty("processing").GetBoolean());
            Assert.IsType<Microsoft.AspNetCore.Mvc.AcceptedResult>(await controller.Queue(uploaded.Id,new AiInboxController.QueueInput("DIGITAL_GDP_SHARE"),default));
            Assert.Null(uploaded.LeaseUntil);
            var suggestionCount=await db.AiDataSuggestions.CountAsync();
            await inbox.ReadAsync(uploaded,default);await db.SaveChangesAsync();
            Assert.Equal(suggestionCount,await db.AiDataSuggestions.CountAsync()); // Re-read is not another report/evidence duplicate.
            foreach(var empty in new[]{false,true})
            {
                var invalidInbox=new AiInboxService(db,new TestEnvironment(),null!,new AiCollectionService(db,null!,new InvalidExtractor(empty),Options.Create(new AiDataOptions())),null!,Options.Create(new AiDataOptions()));
                var otherBytes=System.Text.Encoding.UTF8.GetBytes("Năm 2025 kinh tế số Việt Nam chiếm 18,2% GDP toàn quốc. "+empty);
                using var otherStream=new MemoryStream(otherBytes);
                var other=await invalidInbox.UploadAsync(new FormFile(otherStream,0,otherBytes.Length,"file","invalid.txt"),"DIGITAL_GDP_SHARE","Admin test",default);
                await invalidInbox.ReadAsync(other,default);await db.SaveChangesAsync();
                Assert.Equal("ReadNoData",other.Status);
                var otherUrl="/api/ai-inbox/"+other.Id+"/file";
                var otherDoc=await db.AiSourceDocuments.SingleAsync(d=>d.Url==otherUrl);
                Assert.Equal(empty?"NoData":"EvidenceRejected",otherDoc.Status);
                if(!empty)Assert.Contains("phạm vi",otherDoc.Error);
                Assert.Equal(suggestionCount,await db.AiDataSuggestions.CountAsync());
                Assert.Equal(2,await db.ProgressLogs.CountAsync());
            }
            Assert.Contains(await db.AiGoalDataLinks.ToListAsync(),l=>l.GoalTaskId==goal.Id&&l.ProgressLogId==null);
            Assert.Equal(uploaded.Id,(await inbox.UploadAsync(file,"DIGITAL_GDP_SHARE","Admin test",default)).Id);
            var store=new AiSettingsStore(db,new EphemeralDataProtectionProvider(),Options.Create(new AiDataOptions()));
            await store.SaveAsync(new(false,"https://provider.test/v1","model-test","unit-secret-not-real"),"Admin settings",default);
            var saved=await db.AiProviderConfigurations.SingleAsync();
            Assert.DoesNotContain("unit-secret-not-real",saved.ProtectedApiKey);
            Assert.Equal("unit-secret-not-real",(await store.GetAsync()).ApiKey);
            Assert.DoesNotContain("unit-secret-not-real",System.Text.Json.JsonSerializer.Serialize(await store.PublicAsync(default)));
            await store.SaveAsync(new(true,"https://provider.test/v1","model-test"),"Admin settings",default);
            Assert.True((await store.GetAsync()).Ready); // Runtime changes, no restart; blank key keeps prior secret.
            await store.SaveAsync(new(false,"https://provider.test/v1","model-test",RemoveKey:true),"Admin settings",default);
            Assert.False((await store.GetAsync()).Ready);Assert.Empty((await store.GetAsync()).ApiKey);
        }
        finally {await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
    }
    [Fact]
    public void RankingUsesLowerValueAsBetter()
    {
        var goal=new GoalTaskItem{Unit=new UnitDictionary{Code="RANK",Name="Hạng"},UnitId=Guid.NewGuid(),CustomBaseline=new(){["2026"]="50"}};
        var report=new ProgressLog{PeriodYear=2026,QuantitativeValue=71};
        Assert.True(ProgressCalculator.Evaluate(goal,report).Percentage<100);
        report.QuantitativeValue=30;
        Assert.True(ProgressCalculator.Evaluate(goal,report).Percentage>=100);
    }
    [Theory]
    [InlineData("MT-12")]
    [InlineData("MT-25")]
    public void EgdiNumericUnitKeepsRankSemantics(string code)
    {
        var goal=new GoalTaskItem{ItemType=ItemTypeEnum.Goal,Code=code,Title="Chính phủ điện tử theo báo cáo Liên hợp quốc",Unit=new UnitDictionary{Code="SL",Name="Số lượng"},UnitId=Guid.NewGuid(),CustomBaseline=new(){["2026"]="50"}};
        Assert.True(ProgressCalculator.IsLowerBetter(goal));
        var report=new ProgressLog{PeriodYear=2026,QuantitativeValue=71};
        Assert.True(ProgressCalculator.Evaluate(goal,report).Percentage<100);
        report.QuantitativeValue=30;Assert.True(ProgressCalculator.Evaluate(goal,report).Percentage>=100);
        goal.Title="Số lượng dịch vụ công";Assert.False(ProgressCalculator.IsLowerBetter(goal));
        goal.Title="Chính phủ điện tử theo báo cáo Liên hợp quốc";goal.Unit.Code="PERCENT";goal.Unit.Name="%";
        Assert.False(ProgressCalculator.IsLowerBetter(goal));
    }
    [PostgresFact]
    public async Task DirectGoalOutsideCatalogKeepsResultsSeparateAndSavesOnlyOnConfirmation()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="direct_goal_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();
        await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
        try{
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
            var agency=new Agency{Code="DIRECT",Name="Direct test",Type=AgencyTypeEnum.Ministry,IsActive=true};
            var plan=new Document{DocumentNumber="DIRECT",StartYear=2026,EndYear=2030};
            var unit=new UnitDictionary{Code="SL",Name="Số lượng"};
            var goal=new GoalTaskItem{DocumentId=plan.Id,LeadAgencyId=agency.Id,ItemType=ItemTypeEnum.Goal,Code="MT-99",Title="Số nền tảng hoạt động",UnitId=unit.Id};
            db.AddRange(agency,plan,unit,goal,new AgencyTaskExecution{GoalTaskId=goal.Id,AgencyId=agency.Id,IsLeadAssignment=true});await db.SaveChangesAsync();
            var metric=await AiMetricCatalog.ResolveAsync(db,"GOAL_"+goal.Id,default);
            Assert.Equal("số lượng",metric.Unit);Assert.Contains("MT-99",metric.Name);Assert.Throws<InvalidOperationException>(()=>AiMetricCatalog.Get(metric.Key));
            var pages=new List<SourcePage>{new(1,"Năm 2026 Việt Nam có 250 nền tảng hoạt động.")};
            var observation=new ExtractedMetric{DataYear=2026,Value=250,Unit=metric.Unit,Scope="Việt Nam",Page=1,Quote=pages[0].Text};
            AiMetricExtractor.Validate(observation,metric,pages,2026);
            Assert.Single(AiMetricExtractor.SelectChunks(metric,pages,3));
            var input=new AiInboxItem{StorageKey="direct-test",UrlHash="direct-test",Status="Read"};db.Add(input);await db.SaveChangesAsync();
            var source=new AiDataSource{MetricKey=metric.Key,Url="/api/ai-inbox/"+input.Id+"/file"};db.Add(source);await db.SaveChangesAsync();
            var collected=new CollectedDocument(source.Url,"direct.txt",[1],pages,[]);
            var collector=new AiCollectionService(db,null!,new DirectExtractor(observation),Options.Create(new AiDataOptions()));
            Assert.Equal(1,await collector.ProcessAsync(source,metric,collected,default,true,input.Id));
            var suggestion=await db.AiDataSuggestions.SingleAsync();Assert.Equal("Pending",suggestion.Status);Assert.Empty(await db.ProgressLogs.ToListAsync());
            Assert.Equal(goal.Id,(await db.AiGoalDataLinks.SingleAsync()).GoalTaskId);
            var controller=new Cdsqg.Api.Controllers.AiDataController(db,null!,null!);
            var internet=(Microsoft.AspNetCore.Mvc.OkObjectResult)await controller.Suggestions(internetOnly:true);
            using(var json=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(internet.Value)))Assert.Equal(0,json.RootElement.GetProperty("total").GetInt32());
            var direct=(Microsoft.AspNetCore.Mvc.OkObjectResult)await controller.Suggestions(inboxId:input.Id);
            using(var json=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(direct.Value)))Assert.Equal(1,json.RootElement.GetProperty("total").GetInt32());
            var internetHistory=(Microsoft.AspNetCore.Mvc.OkObjectResult)await controller.Documents(uploadsOnly:false);
            using(var json=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(internetHistory.Value)))Assert.Equal(0,json.RootElement.GetProperty("total").GetInt32());
            await new AiReviewService(db,new ExecutionService(db,new NoFiles())).ReviewAsync(suggestion.Id,true,null,"","Admin",default);
            Assert.Equal("Approved",suggestion.Status);Assert.Single(await db.ProgressLogs.ToListAsync());
            goal.EvaluationType=EvaluationTypeEnum.Qualitative;await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(()=>AiMetricCatalog.ResolveAsync(db,metric.Key,default));
        }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
    }
    private sealed class DirectExtractor(ExtractedMetric observation):IAiMetricExtractor
    {
        public Task<List<ExtractedMetric>> ExtractAsync(AiMetric metric,List<SourcePage> pages,CancellationToken ct)=>Task.FromResult(new List<ExtractedMetric>{observation});
    }
    private sealed class NoFiles:IFileStorageService
    {
        public Task<string> SaveEvidenceFileAsync(IFormFile file)=>throw new InvalidOperationException("No upload expected");
        public Task<string> SaveDocumentFileAsync(IFormFile file)=>throw new InvalidOperationException("No upload expected");
    }
    [Fact]
    public void InboxRejectsUnsafeUrlsAndMissingBridgeSecrets()
    {
        Assert.Throws<InvalidOperationException>(()=>AiInboxService.CanonicalUrl("https://moit.gov.vn.evil.test/a"));
        Assert.False(AiAlertsBridgeController.ValidSecret(null,"anything"));
        Assert.False(AiAlertsBridgeController.ValidSecret(new string('x',32),new string('y',32)));
        Assert.True(AiAlertsBridgeController.ValidSecret(new string('x',32),new string('x',32)));
        Assert.Throws<InvalidOperationException>(()=>AiInboxService.ParseFile("not pdf"u8.ToArray(),".pdf"));
        Assert.Throws<InvalidOperationException>(()=>AiInboxService.ParseFile("text"u8.ToArray(),".exe"));
        Assert.Throws<InvalidOperationException>(()=>AiSettingsStore.Validate(new(false,"https://127.0.0.1/v1","model")));
        Assert.Throws<InvalidOperationException>(()=>AiSettingsStore.Validate(new(false,"https://user@provider.test/v1","model")));
    }
    private sealed class InvalidExtractor(bool empty):IAiMetricExtractor
    {
        public Task<List<ExtractedMetric>> ExtractAsync(AiMetric metric,List<SourcePage> pages,CancellationToken ct)=>Task.FromResult(empty?new List<ExtractedMetric>():new List<ExtractedMetric>{new(){DataYear=2025,Value=18.2m,Unit="%",Scope="Toàn quốc",Page=1,Quote=pages[0].Text}});
    }    private sealed class FixtureExtractor:IAiMetricExtractor
    {
        public Task<List<ExtractedMetric>> ExtractAsync(AiMetric metric,List<SourcePage> pages,CancellationToken ct)=>Task.FromResult(new List<ExtractedMetric>{new(){DataYear=2025,Value=18.2m,Unit="%",Scope="Việt Nam",Page=1,Quote=pages[0].Text}});
    }
    [Fact]
    public async Task TitleClassificationDoesNotFetchTheArticle()
    {
        var handler=new ClassificationHandler();
        var service=new AiInboxService(null!,null!,null!,null!,handler,Options.Create(new AiDataOptions{Enabled=true,ApiKey="test-key-not-real",Model="test-model",BaseUrl="https://provider.test/v1"}));
        var item=new AiInboxItem{Title="Báo cáo kinh tế số Việt Nam 2025",Url="https://www.nso.gov.vn/not-fetched"};
        await service.ClassifyAsync(item,default);
        Assert.Equal("ReadQueued",item.Status);Assert.Contains("DIGITAL_GDP_SHARE",item.MetricKeysJson);
        Assert.DoesNotContain(item.Url,handler.Body);Assert.Contains("Báo cáo",System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(handler.Body).GetProperty("messages")[1].GetProperty("content").GetString());
    }
    private sealed class ClassificationHandler:HttpMessageHandler,IHttpClientFactory
    {
        public string Body="";
        public HttpClient CreateClient(string name)=>new(this,false);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Body=await request.Content!.ReadAsStringAsync(ct);
            return new(System.Net.HttpStatusCode.OK){Content=System.Net.Http.Json.JsonContent.Create(new{choices=new[]{new{finish_reason="stop",message=new{content="{\"metricKeys\":[\"DIGITAL_GDP_SHARE\"],\"uncertain\":false,\"reason\":\"Có thể có số liệu GDP\"}"}}}})};
        }
    }
    private sealed class TestEnvironment:IWebHostEnvironment
    {
        public string ContentRootPath{get;set;}=Path.Combine(Path.GetTempPath(),"cdsqg-ai-inbox-tests",Guid.NewGuid().ToString("N"));
        public string EnvironmentName{get;set;}="Development";
        public string ApplicationName{get;set;}="Tests";
        public string WebRootPath{get;set;}="";
        public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();
        public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();
    }
}






