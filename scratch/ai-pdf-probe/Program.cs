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

await using var tx=await db.Database.BeginTransactionAsync();
var units=await db.Units.Where(u=>u.Code=="SL").ToListAsync();
if(units.Count!=1)throw new InvalidOperationException("Can dung mot don vi SL; dung cap nhat de tranh UnitId null.");
var affected=await db.Database.ExecuteSqlRawAsync("""
UPDATE "GoalTaskItems"
SET "UnitId" = (SELECT "Id" FROM "Units" WHERE "Code" = 'SL')
WHERE "Code" = 'MT-12';
""");
Console.WriteLine("Updated rows: "+affected);
foreach(var g in await db.GoalTaskItems.AsNoTracking().Include(g=>g.Unit).Where(g=>g.Code=="MT-12").ToListAsync())Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new{g.Code,UnitCode=g.Unit!.Code,UnitName=g.Unit.Name}));
await tx.CommitAsync();

