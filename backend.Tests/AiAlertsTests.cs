using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Xml;
using Cdsqg.Application.AI;
using Cdsqg.Api.Controllers;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class AiAlertsTests
{
    [Theory]
    [InlineData("https://www.google.com/alerts/feeds/123/456",true)]
    [InlineData("https://www.google.com/alerts/feeds/123/456/",true)]
    [InlineData("https://news.google.com/rss",true)]
    [InlineData("https://news.google.com/rss/search?q=Kinh+t%E1%BA%BF+s%E1%BB%91&hl=vi&gl=VN&ceid=VN:vi",true)]
    [InlineData("https://news.google.com/rss/topics/CAAqJggKIiBDQkFTRWdvSUwyMHZNREpxYW5RU0FtVnVHZ0pWVXlnQVAB",true)]
    [InlineData("https://news.google.com/not-rss",false)]
    [InlineData("https://www.google.com.evil.test/alerts/feeds/123/456",false)]
    [InlineData("https://www.google.com/url?q=https://127.0.0.1",false)]
    [InlineData("https://user@www.google.com/alerts/feeds/123/456",false)]
    [InlineData("http://www.google.com/alerts/feeds/123/456",false)]
    public void FeedUrlIsGoogleAlertsOrNewsOnly(string url,bool allowed)
    {
        if(allowed)Assert.True(AiAlertFeedReader.ValidateUrl(url).StartsWith("https://www.google.com/alerts/feeds/") || AiAlertFeedReader.ValidateUrl(url).StartsWith("https://news.google.com/rss"));
        else Assert.Throws<InvalidOperationException>(()=>AiAlertFeedReader.ValidateUrl(url));
    }
    [Theory]
    [InlineData("https://www.nso.gov.vn/feed/",true)]
    [InlineData("https://www.nso.gov.vn/feed/?s=%22kinh+te+so%22",true)]
    [InlineData("https://www.nso.gov.vn.evil.test/feed/",false)]
    [InlineData("https://www.nso.gov.vn/comments/feed/",false)]
    [InlineData("https://www.nso.gov.vn/feed/?url=http://127.0.0.1",false)]
    [InlineData("https://www.nso.gov.vn/feed/?s=one&s=two",false)]
    public void OfficialNsoFeedIsExplicitlyBounded(string url,bool allowed)
    {
        if(allowed)Assert.Equal("www.nso.gov.vn",new Uri(AiAlertFeedReader.ValidateUrl(url)).Host);
        else Assert.Throws<InvalidOperationException>(()=>AiAlertFeedReader.ValidateUrl(url));
    }
    [Fact]
    public void FeedParserUnwrapsGoogleLinksButDoesNotTrustArticleHosts()
    {
        var xml="""
        <feed xmlns="http://www.w3.org/2005/Atom"><entry><title>&lt;b&gt;Kinh tế số&lt;/b&gt; Việt Nam</title><link rel="alternate" href="https://www.google.com/url?rct=j&amp;url=https%3A%2F%2Fwww.nso.gov.vn%2Freport%3Futm_source%3Dalerts"/><published>2026-10-19T07:00:00+07:00</published></entry><entry><title>Blocked target</title><link href="https://www.google.com/url?q=http%3A%2F%2F127.0.0.1%2Fsecret"/></entry></feed>
        """;
        var items=AiAlertFeedReader.Parse(Encoding.UTF8.GetBytes(xml));
        Assert.Equal("Kinh tế số Việt Nam",items[0].Title);Assert.Equal(new DateTime(2026,10,19,0,0,0,DateTimeKind.Utc),items[0].PublishedAt);
        Assert.Equal("https://www.nso.gov.vn/report",AiInboxService.CanonicalUrl(items[0].Url));
        Assert.False(AiMetricCatalog.IsAllowedUrl(items[1].Url));
        Assert.Throws<XmlException>(()=>AiAlertFeedReader.Parse(Encoding.UTF8.GetBytes("<!DOCTYPE foo [<!ENTITY x SYSTEM 'file:///secret'>]><feed>&x;</feed>")));
        Assert.Throws<InvalidOperationException>(()=>AiAlertFeedReader.Parse("<html/>"u8.ToArray()));
        Assert.Single(AiAlertFeedReader.Parse(Encoding.UTF8.GetBytes("<rss><channel><item><title>RSS title</title><link>https://www.nso.gov.vn/test</link></item></channel></rss>")));
    }
    [Fact]
    public void ScheduleUsesVietnamDayAndCatchesUpWithoutHourlyRuns()
    {
        Assert.Equal(480,AiAlertService.ParseMinute("08:00"));
        Assert.Throws<InvalidOperationException>(()=>AiAlertService.ParseMinute("24:00"));
        Assert.Equal(new DateTime(2026,10,19,1,0,0),AiAlertService.NextDaily(new DateTime(2026,10,19,0,59,0),480));
        Assert.Equal(new DateTime(2026,10,20,1,0,0),AiAlertService.NextDaily(new DateTime(2026,10,19,1,0,0),480));
        Assert.Equal(new DateTime(2026,10,19,17,0,0),AiAlertService.NextDaily(new DateTime(2026,10,19,16,59,0),0));
    }
    [Fact]
    public void AlertAdministrationRequiresAdmin()
    {
        var attribute=(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)Attribute.GetCustomAttribute(typeof(AiAlertsController),typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))!;
        Assert.Equal("Admin",attribute.Roles);
    }
    [PostgresFact]
    public async Task ScanAndTitlePipelineIsIdempotentAndNeverPublishes()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="alerts_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();
        await using(var command=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await command.ExecuteNonQueryAsync();
        try
        {
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
            var clients=new TitlesClient();var sourceReader=new SourceReader();var extractor=new Extractor();
            var aiOptions=Options.Create(new AiDataOptions{Enabled=true,ApiKey="unit-test-not-real",Model="fixture",BaseUrl="https://provider.test/v1"});
            var settings=new AiSettingsStore(db,new EphemeralDataProtectionProvider(),aiOptions);
            var collection=new AiCollectionService(db,sourceReader,extractor,aiOptions);
            var inbox=new AiInboxService(db,null!,sourceReader,collection,clients,aiOptions);
            var feedReader=new FeedReader();var service=new AiAlertService(db,feedReader,inbox);
            var feed=await service.SaveAsync(null,new("Báo cáo kinh tế số","https://www.google.com/alerts/feeds/123/456",true,"08:00"),"Admin test",default);
            var controller=new AiAlertsController(db,service,settings);
            Assert.IsType<OkObjectResult>(await controller.List(default)); // Includes correlated last scan + schedule projection.
            Assert.Null(await service.EnqueueAsync(feed.Id,"Scheduled","Lịch",default)); // Not yet due.
            await db.AiAlertFeeds.Where(f=>f.Id==feed.Id).ExecuteUpdateAsync(s=>s.SetProperty(f=>f.NextScanAt,DateTime.UtcNow.AddDays(-2)));
            db.ChangeTracker.Clear();
            var scan=await service.EnqueueAsync(feed.Id,"Scheduled","Lịch",default);Assert.NotNull(scan);
            Assert.Null(await service.EnqueueAsync(feed.Id,"Manual","Admin test",default));
            Assert.Null(await service.EnqueueAsync(feed.Id,"Scheduled","Lịch",default));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>service.SaveAsync(feed.Id,new("Rename","https://www.google.com/alerts/feeds/123/456",true,"09:00"),"Admin",default));
            await service.RunAsync(scan!.Id,default);
            await db.Entry(scan).ReloadAsync();Assert.Null(scan.LeaseUntil);
            Assert.Equal("Completed",scan.Status);Assert.Equal(5,scan.Received);Assert.Equal(3,scan.Added);Assert.Equal(1,scan.Duplicates);Assert.Equal(1,scan.Blocked);
            Assert.Equal(0,sourceReader.Calls);Assert.Equal(0,clients.Calls); // RSS only, no inference/content fetch.
            var titles=await db.AiInboxItems.OrderBy(i=>i.Title).ToListAsync();Assert.Equal(3,titles.Count);
            var pausedInbox=new AiInboxService(db,null!,sourceReader,collection,clients,Options.Create(new AiDataOptions()));
            foreach(var item in titles)await pausedInbox.ProcessQueuedAsync(item.Id,default);
            Assert.All(titles,i=>Assert.Equal("TitleQueued",i.Status));Assert.Equal(0,clients.Calls);Assert.Equal(0,sourceReader.Calls);
            foreach(var item in titles)
            {
                // This pipeline fixture advances the queue between items; cadence has separate coverage.
                await db.Set<AiInboxQueueState>().Where(s=>s.Id==1).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.NextReadAt,DateTime.UtcNow.AddMinutes(-2)));
                await inbox.ProcessQueuedAsync(item.Id,default);
            }
            Assert.Equal(3,clients.Calls);Assert.Equal(1,sourceReader.Calls);Assert.Equal(1,extractor.Calls);
            Assert.Equal("Read",titles.Single(i=>i.Title.StartsWith("Liên quan")).Status);
            Assert.Equal("NeedsReview",titles.Single(i=>i.Title.StartsWith("Chưa rõ")).Status);
            Assert.Equal("NotRelevant",titles.Single(i=>i.Title.StartsWith("Không liên quan")).Status);
            foreach(var item in titles)await db.Entry(item).ReloadAsync();
            Assert.All(titles,i=>Assert.Null(i.LeaseUntil));Assert.Equal(0,await db.ProgressLogs.CountAsync());
            Assert.Equal("Pending",(await db.AiDataSuggestions.SingleAsync()).Status);
            var nextScan=await service.EnqueueAsync(feed.Id,"Manual","Admin",default);await service.RunAsync(nextScan!.Id,default);
            Assert.Equal(0,nextScan.Added);Assert.Equal(4,nextScan.Duplicates);Assert.Equal(1,nextScan.Blocked);
            await service.RunAsync(nextScan.Id,default);Assert.Equal(2,feedReader.Calls); // Completed run cannot execute again.
            foreach(var item in titles)
            {
                // This pipeline fixture advances the queue between items; cadence has separate coverage.
                await db.Set<AiInboxQueueState>().Where(s=>s.Id==1).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.NextReadAt,DateTime.UtcNow.AddMinutes(-2)));
                await inbox.ProcessQueuedAsync(item.Id,default);
            }Assert.Equal(3,clients.Calls);
            feed=await service.SaveAsync(feed.Id,new("Nguồn đã tắt lịch","https://www.google.com/alerts/feeds/123/456",false,"09:00"),"Admin",default);
            Assert.Null(feed.NextScanAt);Assert.Null(await service.EnqueueAsync(feed.Id,"Scheduled","Lịch",default));
            await using var secondDb=new AppDbContext(options);
            var otherService=new AiAlertService(secondDb,feedReader,inbox);
            var parallel=await Task.WhenAll(service.EnqueueAsync(feed.Id,"Manual","Admin",default),otherService.EnqueueAsync(feed.Id,"Manual","Admin",default));
            Assert.Single(parallel,s=>s!=null); // Manual works with schedule off; concurrent requests produce one job.
            var pending=parallel.Single(s=>s!=null)!;
            await new AiAlertService(db,new BrokenFeedReader(),inbox).RunAsync(pending.Id,default);
            await db.Entry(pending).ReloadAsync();Assert.Equal("Error",pending.Status);Assert.Null(pending.LeaseUntil);
            Assert.Equal(0,await db.ProgressLogs.CountAsync());
            Assert.IsType<OkObjectResult>(await controller.Scans(ct:default));
            var baseTime=DateTime.UtcNow.AddDays(-1);
            for(var n=0;n<11;n++)db.AiAlertFeeds.Add(new AiAlertFeed{Name="Page "+n,FeedUrl=$"https://news.google.com/rss/search?q=page{n}",CreatedAt=baseTime.AddMinutes(n)});
            await db.SaveChangesAsync();
            var expected=await db.AiAlertFeeds.AsNoTracking().OrderByDescending(f=>f.CreatedAt).ThenByDescending(f=>f.Id).Select(f=>f.Id).ToListAsync();
            var seen=new List<Guid>();
            for(var p=1;p<=3;p++){
                var result=Assert.IsType<OkObjectResult>(await controller.List(page:p));
                using var json=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(result.Value));
                Assert.Equal(5,json.RootElement.GetProperty("pageSize").GetInt32());
                Assert.Equal(expected.Count,json.RootElement.GetProperty("total").GetInt32());
                var pageItems=json.RootElement.GetProperty("feeds");
                Assert.Equal(p==3?2:5,pageItems.GetArrayLength());
                seen.AddRange(pageItems.EnumerateArray().Select(x=>x.GetProperty("Id").GetGuid()));
            }
            Assert.Equal(expected,seen);
        }
        finally{await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await command.ExecuteNonQueryAsync();}
    }
    private sealed class BrokenFeedReader:IAiAlertFeedReader
    {
        public Task<List<AlertFeedEntry>> ReadAsync(string url,CancellationToken ct)=>throw new InvalidOperationException("RSS test failure");
    }
    private sealed class FeedReader:IAiAlertFeedReader
    {
        public int Calls;
        public Task<List<AlertFeedEntry>> ReadAsync(string url,CancellationToken ct){Calls++;return Task.FromResult(new List<AlertFeedEntry>{new("Liên quan kinh tế số Việt Nam","https://www.nso.gov.vn/report",null),new("Liên quan kinh tế số Việt Nam","https://www.nso.gov.vn/report?utm_source=alerts",null),new("Chưa rõ báo cáo","https://www.nso.gov.vn/uncertain",null),new("Không liên quan thời tiết","https://www.nso.gov.vn/weather",null),new("Nguồn không được phép","https://evil.test/article",null)});}
    }
    private sealed class SourceReader:IAiSourceReader
    {
        public int Calls;
        public Task<CollectedDocument> ReadAsync(string url,CancellationToken ct){Calls++;Assert.Equal("https://www.nso.gov.vn/report",url);return Task.FromResult(new CollectedDocument(url,"Báo cáo",[1,2,3],[new(1,"Năm 2025 kinh tế số Việt Nam chiếm 18,2% GDP.")],[]));}
    }
    private sealed class Extractor:IAiMetricExtractor
    {
        public int Calls;
        public Task<List<ExtractedMetric>> ExtractAsync(AiMetric metric,List<SourcePage> pages,CancellationToken ct){Calls++;return Task.FromResult(new List<ExtractedMetric>{new(){DataYear=2025,Value=18.2m,Unit="%",Scope="Việt Nam",Page=1,Quote=pages[0].Text}});}
    }
    private sealed class TitlesClient:HttpMessageHandler,IHttpClientFactory
    {
        public int Calls;
        public HttpClient CreateClient(string name)=>new(this,false);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Calls++;using var body=JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));var title=body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!;
            Assert.DoesNotContain("https://www.nso.gov.vn",title);
            var uncertain=title.StartsWith("Chưa rõ");var related=title.StartsWith("Liên quan");
            var content=JsonSerializer.Serialize(new{metricKeys=related?new[]{"DIGITAL_GDP_SHARE"}:Array.Empty<string>(),uncertain,reason="Phân loại kiểm thử"});
            return new(HttpStatusCode.OK){Content=JsonContent.Create(new{choices=new[]{new{finish_reason="stop",message=new{content}}}})};
        }
    }
}
