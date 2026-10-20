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
if(args.Contains("repair")){
 var gate=await db.Set<Cdsqg.Core.Entities.AiInboxQueueState>().AsNoTracking().SingleAsync();
 if(gate.Owner!=null)throw new InvalidOperationException("Worker owns queue; refusing stale repair.");
 var id=Guid.Parse("fceb64f9-8719-4825-88d2-4e9dd77f7d13");
 var changed=await db.AiInboxItems.Where(i=>i.Id==id&&i.SequenceNumber==12&&i.Status=="ReadQueued"&&i.LeaseUntil!=null).ExecuteUpdateAsync(s=>s.SetProperty(i=>i.Status,"Error").SetProperty(i=>i.Error,"Lần đọc đã dừng do lỗi lưu số liệu trùng. Hệ thống đã sửa lỗi; các dẫn chứng lưu được vẫn giữ nguyên. Có thể đọc lại.").SetProperty(i=>i.LeaseUntil,(DateTime?)null));
 if(changed==1)await db.AiSourceDocuments.Where(d=>d.InboxItemId==id&&d.Status=="Collected").ExecuteUpdateAsync(s=>s.SetProperty(d=>d.Status,"Error").SetProperty(d=>d.Error,"Lần đọc dừng do lỗi lưu số liệu trùng; kết quả đã lưu được giữ nguyên."));
 Console.WriteLine("Repaired stalled inputs: "+changed);
}
Console.WriteLine("UTC now: "+DateTime.UtcNow.ToString("O"));
foreach(var i in await db.AiInboxItems.AsNoTracking().Where(i=>i.SequenceNumber>=6).OrderBy(i=>i.SequenceNumber).ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{i.Id,i.SequenceNumber,i.Status,i.ReceivedAt,i.LeaseUntil,i.Error}));
foreach(var q in await db.Set<Cdsqg.Core.Entities.AiInboxQueueState>().AsNoTracking().ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(q));
