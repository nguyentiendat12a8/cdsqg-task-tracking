using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;

public class DeliverableJsonPostgresTests
{
    [PostgresFact]
    public async Task LegacyAndCurrentProductJsonKeepIdsTitlesAndStatusesAcrossReads()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="productjson_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection); await admin.OpenAsync();
        await using(var command=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options); await db.Database.MigrateAsync();
            var agency=new Agency{Code="BCA",Name="Bộ Công an"}; var doc=new Document{DocumentNumber="2629"};
            var task=new GoalTaskItem{DocumentId=doc.Id,LeadAgencyId=agency.Id,Code="NV-2629-03"};
            var log=new ProgressLog{GoalTaskId=task.Id,AgencyId=agency.Id,PeriodYear=2026};
            var execution=new AgencyTaskExecution{GoalTaskId=task.Id,AgencyId=agency.Id,IsLeadAssignment=true};
            db.AddRange(agency,doc,task,log,execution); await db.SaveChangesAsync();
            foreach(var camel in new[]{true,false})
            {
                var products=camel ? """[{"id":"stable-product","title":"Trình Quốc hội dự thảo Luật","dueDate":"2026-12-31","currentStatus":"Completed","documentNumber":"01/QD"}]"""
                    : """[{"Id":"stable-product","Title":"Trình Quốc hội dự thảo Luật","DueDate":"2026-12-31","CurrentStatus":"Completed","DocumentNumber":"01/QD"}]""";
                var byAgency="{\""+agency.Id+"\":"+products+"}";
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"GoalTaskItems\" SET \"Deliverables\"={products}::jsonb, \"AgencyDeliverables\"={byAgency}::jsonb WHERE \"Id\"={task.Id}");
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"ProgressLogs\" SET \"Deliverables\"={products}::jsonb WHERE \"Id\"={log.Id}");
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"AgencyTaskExecutions\" SET \"Deliverables\"={products}::jsonb WHERE \"Id\"={execution.Id}");
                for(int read=0;read<2;read++)
                {
                    await using var reader=new AppDbContext(options);
                    var loaded=await reader.GoalTaskItems.AsNoTracking().SingleAsync(i=>i.Id==task.Id);
                    var report=await reader.ProgressLogs.AsNoTracking().SingleAsync(i=>i.Id==log.Id);
                    var row=await reader.AgencyTaskExecutions.AsNoTracking().SingleAsync(i=>i.Id==execution.Id);
                    foreach(var product in new[]{loaded.Deliverables.Single(),loaded.AgencyDeliverables[agency.Id.ToString()].Single(),report.Deliverables!.Single(),row.Deliverables.Single()})
                    {
                        Assert.Equal("stable-product",product.Id); Assert.Equal("Trình Quốc hội dự thảo Luật",product.Title);
                        Assert.Equal("Completed",product.CurrentStatus); Assert.Equal("01/QD",product.DocumentNumber);
                        Assert.Equal(new DateTime(2026,12,31),product.DueDate);
                    }
                }
            }
        }
        finally {await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await command.ExecuteNonQueryAsync();}
    }
}
