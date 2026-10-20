using Microsoft.EntityFrameworkCore.Storage;
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

var parsed=new NpgsqlConnectionStringBuilder(connection);
if(parsed.Host!="localhost" && parsed.Host!="127.0.0.1" && parsed.Host!="::1")throw new Exception("Refusing non-local database");
await db.Database.OpenConnectionAsync();
await using var tx=await db.Database.BeginTransactionAsync();
var tables=new[]{"AiGoalDataLinks","AiDataSuggestions","AiSourceDocuments","AiInboxItems","AiAlertScans","AiAlertFeeds"};
var backup=new Dictionary<string,System.Text.Json.JsonElement>();
foreach(var table in tables){
 await using var cmd=db.Database.GetDbConnection().CreateCommand();cmd.Transaction=tx.GetDbTransaction();cmd.CommandText="SELECT COALESCE(jsonb_agg(to_jsonb(t)), '[]'::jsonb)::text FROM \""+table+"\" t";
 var json=(string)(await cmd.ExecuteScalarAsync())!;backup[table]=System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
 Console.WriteLine(table+" before: "+backup[table].GetArrayLength());
}
var backupPath=Path.GetFullPath("scratch/ai-local-reset/backup-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".json");
await File.WriteAllTextAsync(backupPath,System.Text.Json.JsonSerializer.Serialize(backup));
foreach(var table in tables)await db.Database.ExecuteSqlRawAsync("DELETE FROM \""+table+"\"");
await db.Database.ExecuteSqlRawAsync("UPDATE \"AiInboxQueueState\" SET \"Owner\"=NULL, \"LeaseUntil\"=NULL, \"NextReadAt\"='2000-01-01'::timestamp");
await db.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"AiInboxNumbers\" RESTART WITH 1");
await db.Database.ExecuteSqlRawAsync("UPDATE \"AiDataSources\" SET \"Enabled\"=FALSE, \"LeaseUntil\"=NULL, \"LastCheckedAt\"=NULL, \"NextCheckAt\"=NULL, \"LastError\"='', \"LastStatus\"='Chưa thu thập'");
await tx.CommitAsync();
Console.WriteLine("Reset complete. Backup: "+backupPath);
foreach(var table in tables){await using var cmd=db.Database.GetDbConnection().CreateCommand();cmd.CommandText="SELECT count(*) FROM \""+table+"\"";Console.WriteLine(table+" after: "+await cmd.ExecuteScalarAsync());}
