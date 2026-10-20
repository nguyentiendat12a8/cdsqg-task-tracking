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
var collection=new AiCollectionService(db,null!,extractor,options,store);
var inbox=new AiInboxService(db,new Env(root),null!,collection,clients,options,store);
var item=await db.AiInboxItems.SingleAsync(i=>i.Title=="UN-E-Government-Survey-2024.pdf");
if(args.Contains("status")){Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{item.Status,item.Error,item.LeaseUntil}));return;}
await db.AiInboxItems.Where(i=>i.Id==item.Id&&i.Status=="Error").ExecuteUpdateAsync(s=>s.SetProperty(i=>i.LeaseUntil,(DateTime?)null));
var now=DateTime.UtcNow;
if(await db.AiInboxItems.Where(i=>i.Id==item.Id&&(i.LeaseUntil==null||i.LeaseUntil<now)).ExecuteUpdateAsync(s=>s.SetProperty(i=>i.LeaseUntil,now.AddMinutes(10)))==0)throw new Exception("Already processing");
await db.Entry(item).ReloadAsync();
try{await inbox.ReadAsync(item,default);Console.WriteLine("Inbox: "+item.Status);}
catch(Exception ex){Console.WriteLine("Failure type: "+ex.GetType().Name);if(ex is InvalidOperationException)Console.WriteLine(ex.Message);item.Status="Error";item.Error=AiProviderRequest.Error(ex);}
finally{item.LeaseUntil=null;await db.SaveChangesAsync();}
var url="/api/ai-inbox/"+item.Id+"/file";
var docs=await db.AiSourceDocuments.Where(d=>d.Url==url).ToListAsync();
foreach(var d in docs){Console.WriteLine("Document: "+d.Status+" "+d.Error);foreach(var v in await db.AiDataSuggestions.Where(v=>v.DocumentId==d.Id).ToListAsync())Console.WriteLine($"Suggestion: {v.DataYear}, {v.Value} {v.Unit}, PDF page {v.Page}, {v.Status}, quote={v.Quote}");}
sealed class Clients:IHttpClientFactory{public HttpClient CreateClient(string name)=>new(AiSourceReader.CreatePublicHandler()){Timeout=TimeSpan.FromSeconds(90)};}
sealed class Env(string root):IWebHostEnvironment{public string ContentRootPath{get;set;}=root;public string WebRootPath{get;set;}="";public string EnvironmentName{get;set;}="Development";public string ApplicationName{get;set;}="backend";public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();}
