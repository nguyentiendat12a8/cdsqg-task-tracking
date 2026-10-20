using Cdsqg.Application.AI;
using Cdsqg.Application.Services;
using Cdsqg.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Npgsql;
var root=Path.GetFullPath("backend");
var config=new ConfigurationBuilder().SetBasePath(root).AddJsonFile("appsettings.json",true).AddJsonFile("appsettings.Development.json",true).AddUserSecrets(typeof(AiSettingsStore).Assembly,true).AddEnvironmentVariables().Build();
var connection=DatabaseConfiguration.ResolveConnectionString(config);
if(connection.StartsWith("postgres")){var u=new Uri(connection);var a=u.UserInfo.Split(':',2);connection=new NpgsqlConnectionStringBuilder{Host=u.Host,Port=u.Port>0?u.Port:5432,Database=u.AbsolutePath.Trim('/'),Username=Uri.UnescapeDataString(a[0]),Password=Uri.UnescapeDataString(a.ElementAtOrDefault(1)??""),SslMode=SslMode.Require}.ConnectionString;}
using var db=new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).Options);
var options=Options.Create(config.GetSection("AiData").Get<AiDataOptions>()??new());
var keyPath=config["AiData:ProtectionKeyDirectory"]??Path.Combine(root,"App_Data","ai-keys");
var protection=DataProtectionProvider.Create(new DirectoryInfo(keyPath),b=>b.SetApplicationName("CDSQG.AiCredentials"));
var store=new AiSettingsStore(db,protection,options);
var clients=new Clients();var extractor=new AiMetricExtractor(options,clients,store);
var reader=new AiSourceReader();
var collection=new AiCollectionService(db,reader,extractor,options,store);
var inbox=new AiInboxService(db,new Env(root),reader,collection,clients,options,store);
var feed=await db.AiAlertFeeds.SingleAsync(f=>f.FeedUrl=="https://www.google.com/alerts/feeds/17039773334331169358/10847235485381683809");
var url="https://moit.gov.vn/tin-tuc/xuc-tien-thuong-mai/xuc-tien-hang-viet-dong-luc-tu-chuyen-doi-so-logistics-va-thuong-mai-dien-tu.html";
if(!args.Contains("status")){
 await inbox.ImportEntryAsync(new("Xúc tiến hàng Việt: Động lực từ chuyển đổi số logistics và thương mại điện tử",url),feed.Id,null,"DEMO — nạp thủ công bài thật để thử luồng sau RSS",default);
 var queued=await db.AiInboxItems.SingleAsync(i=>i.Url==url);
 await inbox.ProcessQueuedAsync(queued.Id,default);
}
var item=await db.AiInboxItems.AsNoTracking().SingleAsync(i=>i.Url==url);
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{item.Id,item.SequenceNumber,item.Status,item.Reason,item.Error,item.MetricKeysJson}));
foreach(var d in await db.AiSourceDocuments.Where(d=>d.InboxItemId==item.Id).ToListAsync())foreach(var v in await db.AiDataSuggestions.Where(v=>v.DocumentId==d.Id).ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{v.Id,v.DataYear,v.Value,v.Unit,v.Status,v.Quote}));
sealed class Clients:IHttpClientFactory{public HttpClient CreateClient(string name)=>new(AiSourceReader.CreatePublicHandler()){Timeout=TimeSpan.FromSeconds(90)};}
sealed class Env(string root):IWebHostEnvironment{public string ContentRootPath{get;set;}=root;public string WebRootPath{get;set;}="";public string EnvironmentName{get;set;}="Development";public string ApplicationName{get;set;}="backend";public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();}
