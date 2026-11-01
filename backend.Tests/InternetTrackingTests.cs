using System.Text;
using System.Text.Json;
using Cdsqg.Application.AI;
using Cdsqg.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class InternetTrackingTests
{
    [Theory]
    [InlineData("https://dangcongsan.vn/tin-tuc/test.html",true)]
    [InlineData("https://nhandan.vn/rss/kinh-te.rss",true)]
    [InlineData("https://hanoi.gov.vn/rss",true)]
    [InlineData("https://vov.vn/rss",true)]
    [InlineData("https://vtv.vn/rss",true)]
    [InlineData("https://dangcongsan.vn.evil.test/rss",false)]
    [InlineData("https://hanoi.gov.vn.evil.test/rss",false)]
    [InlineData("https://evil.test/rss",false)]
    [InlineData("https://127.0.0.1/rss",false)]
    [InlineData("https://user:password@hanoi.gov.vn/rss",false)]
    public void ExpandedSourcesKeepHostBoundaryAndHttpsChecks(string url,bool allowed)
    {
        Assert.Equal(allowed,AiMetricCatalog.IsAllowedUrl(url));
        if(allowed)Assert.Equal(url,AiAlertFeedReader.ValidateUrl(url));
        else Assert.Throws<InvalidOperationException>(()=>AiAlertFeedReader.ValidateUrl(url));
    }
    [Fact]
    public void ScopeYearForecastAndNationalGrdpAreValidatedBeforeSuggestions()
    {
        var metric=new AiMetric("GOAL_test","Tỷ lệ hồ sơ","%","Tỷ lệ hồ sơ",[],Scopes:[new(Guid.NewGuid(),"Hà Nội",["Hà Nội"])],Internet:true);
        var text="Năm 2026 Hà Nội đã số hóa 80% hồ sơ.";
        var value=new ExtractedMetric{DataYear=2026,Value=80,Unit="%",Scope="Hà Nội",Quote=text,Page=1,PeriodLabel="Năm 2026"};
        var pages=new List<SourcePage>{new(1,text)};
        AiMetricExtractor.Validate(value,metric,pages,2026);
        value.Scope="Đà Nẵng";Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        value.Scope="Hà Nội";value.IsForecastOrTarget=true;Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        value.IsForecastOrTarget=false;value.DataYear=2025;Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        value.DataYear=2026;value.PeriodLabel="Quý 1/2026";Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,metric,pages,2026));
        var gdp=AiMetricCatalog.Get("DIGITAL_GDP_SHARE") with {Internet=true};
        value.Scope="Việt Nam";value.PeriodLabel="Năm 2026";value.Quote="Năm 2026 Hà Nội đạt 80% GRDP.";
        Assert.Throws<InvalidOperationException>(()=>AiMetricExtractor.Validate(value,gdp,[new(1,value.Quote)],2026));
    }
    [PostgresFact]
    public async Task FeedExtractionAndReviewKeepEachAgencySeparateAndPreserveEvidence()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="internet_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();
        await using(var command=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await command.ExecuteNonQueryAsync();
        try{
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);
            await db.GetService<IMigrator>().MigrateAsync("20261025143854_PersistAiProtectionKeys");
            var legacyFeed=new AiAlertFeed{Name="Nguồn cũ",FeedUrl="https://www.google.com/alerts/feeds/111/222"};await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"AiAlertFeeds\" (\"Id\",\"Name\",\"FeedUrl\",\"Enabled\",\"ScanMinute\",\"CreatedAt\",\"UpdatedBy\") VALUES ({legacyFeed.Id},{legacyFeed.Name},{legacyFeed.FeedUrl},false,480,{DateTime.UtcNow},'fixture')");
            await db.Database.MigrateAsync();legacyFeed=await db.AiAlertFeeds.SingleAsync(f=>f.Id==legacyFeed.Id);Assert.Equal("[]",legacyFeed.MetricKeysJson);Assert.Equal(3,AiInternetTracking.Keys(legacyFeed.MetricKeysJson).Length);
            var hanoi=new Agency{Name="Hà Nội",Code="HN",Type=AgencyTypeEnum.Province};var danang=new Agency{Name="Đà Nẵng",Code="DN",Type=AgencyTypeEnum.Province};
            var document=new Document{DocumentNumber="CUSTOM-TEST",StartYear=2026,EndYear=2030};var unit=new UnitDictionary{Name="%",Code="INTERNET_PERCENT"};
            var goal=new GoalTaskItem{DocumentId=document.Id,Code="MT-13",Title="Tỷ lệ hồ sơ được số hóa",LeadAgencyId=hanoi.Id,IsGeneralTask=true,ItemType=ItemTypeEnum.Goal,EvaluationType=EvaluationTypeEnum.Quantitative,UnitId=unit.Id,StartDate=new(2026,1,1),DueDate=new(2030,12,31),ResultTarget=100,CustomBaseline=new(){["2026"]="95"}};
            db.AddRange(hanoi,danang,document,unit,goal,new AgencyTaskExecution{GoalTaskId=goal.Id,AgencyId=hanoi.Id,IsLeadAssignment=true},new AgencyTaskExecution{GoalTaskId=goal.Id,AgencyId=danang.Id,IsLeadAssignment=true});await db.SaveChangesAsync();
            var key="GOAL_"+goal.Id;
            db.Add(new AiGoalKeyword{DocumentId=document.Id,GoalCode=goal.Code,GoalTitle=goal.Title,KeywordsVi="số hóa, hồ sơ",KeywordsEn="digitization",ExpectedUnit="%",ExtractionPrompt="Tỷ lệ số hóa thực tế"});await db.SaveChangesAsync();
            var feedService=new AiAlertService(db,null!,null!);
            var feed=await feedService.SaveAsync(null,new("Số hóa các tỉnh","https://dangcongsan.vn/rss",false,"08:00",[key],[hanoi.Id],document.Id),"Admin test",default);
            // SQL-backed document filters distinguish goal IDs and preserve unclassified RSS articles.
            var uploads=new[]{new AiInboxItem{Title="Scoped upload",StorageKey="fixture.txt",UrlHash="scope-upload",MetricKeysJson=JsonSerializer.Serialize(new[]{key})},new AiInboxItem{Title="National upload",StorageKey="national.txt",UrlHash="national-upload",MetricKeysJson="[\"EGDI_RANK\"]"},new AiInboxItem{Title="Queued RSS",AlertFeedId=feed.Id,UrlHash="scope-rss"}};
            db.AddRange(uploads);await db.SaveChangesAsync();
            Assert.Contains(feed.Id,await (await AiDocumentScope.FeedsAsync(db,document.Id,default)).Select(f=>f.Id).ToListAsync());
            Assert.DoesNotContain(feed.Id,await (await AiDocumentScope.FeedsAsync(db,AiDocumentScope.PriorityDocument,default)).Select(f=>f.Id).ToListAsync());
            var scopedInputs=await (await AiDocumentScope.InboxAsync(db,document.Id,default)).Select(i=>i.Id).ToListAsync();
            Assert.Contains(uploads[0].Id,scopedInputs);Assert.Contains(uploads[2].Id,scopedInputs);Assert.DoesNotContain(uploads[1].Id,scopedInputs);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>feedService.SaveAsync(feed.Id,new(feed.Name,feed.FeedUrl,false,"08:00",["EGDI_RANK"],[],AiDocumentScope.PriorityDocument),"Admin",default));
            var oldKeys=feed.MetricKeysJson;var oldAgencies=feed.AgencyIdsJson;
            await feedService.SaveAsync(feed.Id,new(feed.Name,feed.FeedUrl,false,"08:00"),"Admin legacy client",default);
            Assert.Equal(oldKeys,feed.MetricKeysJson);Assert.Equal(oldAgencies,feed.AgencyIdsJson);
            var metric=Assert.Single(await AiInternetTracking.ResolveAsync(db,feed,default));Assert.Equal(2,metric.Scopes!.Length);Assert.Equal("%",metric.Unit); // MT-13 from another decision must not inherit GDP keywords.
            Assert.DoesNotContain("GDP",metric.Definition);
            var extractor=new Observations();var settings=Options.Create(new AiDataOptions{Model="fixture"});
            var collection=new AiCollectionService(db,null!,extractor,settings);
            var source=new AiDataSource{Name="Bài báo",Url="https://dangcongsan.vn/tin-tuc/so-hoa",MetricKey=key};db.Add(source);await db.SaveChangesAsync();
            const string text="Năm 2026 Hà Nội đã số hóa 80% hồ sơ. Năm 2026 Đà Nẵng đã số hóa 90% hồ sơ.";
            CollectedDocument Article(string url)=>new(url,"Kết quả số hóa",Encoding.UTF8.GetBytes(text),[new(1,text)],[]);
            var count=await collection.ProcessAsync(source,metric,Article(source.Url),default);Assert.True(count==2,string.Join(";",await db.AiSourceDocuments.Select(d=>d.Error).ToListAsync()));Assert.Empty(await db.ProgressLogs.ToListAsync());
            var suggestions=await db.AiDataSuggestions.OrderBy(s=>s.Value).ToListAsync();Assert.Equal(hanoi.Id,suggestions[0].TargetAgencyId);Assert.Equal(danang.Id,suggestions[1].TargetAgencyId);
            Assert.Equal(2,await db.AiGoalDataLinks.CountAsync(l=>l.GoalTaskId==goal.Id));
            var scopedController=new AiDataController(db,null!,null!);
            var scopedResult=Assert.IsType<OkObjectResult>(await scopedController.Suggestions(documentId:document.Id));
            Assert.Equal(2,JsonSerializer.SerializeToElement(scopedResult.Value).GetProperty("total").GetInt32());
            var otherResult=Assert.IsType<OkObjectResult>(await scopedController.Suggestions(documentId:AiDocumentScope.PriorityDocument));
            Assert.Equal(0,JsonSerializer.SerializeToElement(otherResult.Value).GetProperty("total").GetInt32());
            var scopedAll=new AiInboxItem{Title="All goals in document",StorageKey="all.txt",UrlHash="all-scoped",MetricKeysJson=JsonSerializer.Serialize(new[]{"ALL_GOALS_"+document.Id})};db.Add(scopedAll);await db.SaveChangesAsync();
            Assert.True(await (await AiDocumentScope.InboxAsync(db,document.Id,default)).AnyAsync(i=>i.Id==scopedAll.Id));
            Assert.False(await (await AiDocumentScope.InboxAsync(db,AiDocumentScope.PriorityDocument,default)).AnyAsync(i=>i.Id==scopedAll.Id));
            var review=new AiReviewService(db,new ExecutionService(db,null!));
            await review.ReviewAsync(suggestions[0].Id,true,null,"Đã kiểm tra","Admin test",default);
            await review.ReviewAsync(suggestions[1].Id,true,null,"Đã kiểm tra","Admin test",default);
            var reports=await db.ProgressLogs.ToListAsync();Assert.Equal(2,reports.Count);Assert.Contains(reports,p=>p.AgencyId==hanoi.Id&&p.QuantitativeValue==80);Assert.Contains(reports,p=>p.AgencyId==danang.Id&&p.QuantitativeValue==90);
            Assert.Equal(2,await db.AiDataSuggestions.CountAsync(s=>s.IsCurrent));
            var verified=Assert.IsType<OkObjectResult>(await new AiDataController(db,null!,null!).Verified(default));
            var verifiedJson=JsonSerializer.SerializeToElement(verified.Value,new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Assert.All(verifiedJson.EnumerateArray(),row=>Assert.False(row.GetProperty("thresholdReached").GetBoolean()));
            var repeatSource=new AiDataSource{Name="Bài đăng lại",Url="https://nhandan.vn/tin-dang-lai",MetricKey=key};db.Add(repeatSource);await db.SaveChangesAsync();
            await collection.ProcessAsync(repeatSource,metric,Article(repeatSource.Url),default);
            Assert.Contains(await db.AiDataSuggestions.Where(s=>s.DocumentId!=suggestions[0].DocumentId).ToListAsync(),s=>s.Warnings.Contains("trùng"));
            // Different measurements for one agency must not alter the other agency's current result.
            extractor.HanoiValue=81;var updatedText=text.Replace("80%","81%");
            var updateSource=new AiDataSource{Name="Bài cập nhật",Url="https://hanoi.gov.vn/update",MetricKey=key};db.Add(updateSource);await db.SaveChangesAsync();
            await collection.ProcessAsync(updateSource,metric,new(updateSource.Url,"Cập nhật",Encoding.UTF8.GetBytes(updatedText),[new(1,updatedText)],[]),default);
            var latest=await db.AiDataSuggestions.SingleAsync(s=>s.MetricKey==key&&s.Value==81);
            Assert.Contains("Có số liệu khác",latest.Warnings);
            await review.ReviewAsync(latest.Id,true,null,"Đối chiếu bản mới","Admin test",default);
            Assert.False(suggestions[0].IsCurrent);Assert.True(suggestions[1].IsCurrent);Assert.Equal(2,await db.AiDataSuggestions.CountAsync(s=>s.IsCurrent));
            // Legacy source goal/agency selections no longer restrict document-wide context.
            var context=await AiInternetTracking.ContextAsync(db,feed,default);Assert.Single(context);Assert.Equal("số hóa, hồ sơ",context[0].KeywordsVi);
            var secondGoal=new GoalTaskItem{LeadAgencyId=hanoi.Id,DocumentId=document.Id,Code="MT-14",Title="Tỷ lệ dịch vụ trực tuyến",ItemType=ItemTypeEnum.Goal,EvaluationType=EvaluationTypeEnum.Quantitative};db.Add(secondGoal);db.Add(new AiGoalKeyword{DocumentId=document.Id,GoalCode=secondGoal.Code,GoalTitle=secondGoal.Title,KeywordsVi="dịch vụ trực tuyến",ExpectedUnit="%"});await db.SaveChangesAsync();
            Assert.Equal(2,(await AiInternetTracking.ContextAsync(db,feed,default)).Length);
            var updatedKeyword=await db.AiGoalKeywords.SingleAsync(k=>k.DocumentId==document.Id&&k.GoalCode==goal.Code);updatedKeyword.KeywordsVi="từ khóa mới ngay lần đọc sau";await db.SaveChangesAsync();
            Assert.Contains(await AiInternetTracking.ContextAsync(db,feed,default),m=>m.Key==key&&m.KeywordsVi==updatedKeyword.KeywordsVi);
            db.Add(new Document{Id=AiDocumentScope.PriorityDocument,DocumentNumber="1266/QĐ-TTg"});await db.SaveChangesAsync();
            var otherFeed=await feedService.SaveAsync(null,new("Cùng RSS khác văn bản",feed.FeedUrl,false,"08:00",DocumentId:AiDocumentScope.PriorityDocument),"Admin",default);
            var inboxService=new AiInboxService(db,null!,null!,null!,null!,Options.Create(new AiDataOptions()));
            Assert.True(await inboxService.ImportEntryAsync(new("Hồ sơ số hóa và dịch vụ trực tuyến","https://dangcongsan.vn/scope-article"),feed.Id,null,"fixture",default));
            Assert.True(await inboxService.ImportEntryAsync(new("Hồ sơ số hóa và dịch vụ trực tuyến","https://dangcongsan.vn/scope-article"),otherFeed.Id,null,"fixture",default));
            Assert.False(await inboxService.ImportEntryAsync(new("Hồ sơ số hóa và dịch vụ trực tuyến","https://dangcongsan.vn/scope-article"),feed.Id,null,"fixture",default));
            // National indicators remain national even when a feed includes agency filters.
            var national=await AiInternetTracking.ResolveAsync(db,null,default);Assert.All(national,m=>Assert.Null(m.Scopes));
        }finally{await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await command.ExecuteNonQueryAsync();}
    }
    private sealed class Observations:IAiMetricExtractor
    {
        public decimal HanoiValue=80;
        public Task<List<ExtractedMetric>> ExtractAsync(AiMetric metric,List<SourcePage> pages,CancellationToken ct)=>Task.FromResult(new List<ExtractedMetric>{new(){DataYear=2026,Value=HanoiValue,Unit="%",Scope=metric.Scopes!.Single(s=>s.Aliases.Contains("Hà Nội")).Name,Quote=pages[0].Text.Split(". ")[0],Page=1,PeriodLabel="Năm 2026"},new(){DataYear=2026,Value=90,Unit="%",Scope=metric.Scopes!.Single(s=>s.Aliases.Contains("Đà Nẵng")).Name,Quote=pages[0].Text.Split(". ")[1],Page=1,PeriodLabel="Năm 2026"}});
    }
}

