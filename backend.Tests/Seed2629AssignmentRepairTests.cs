using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;
public class Seed2629AssignmentRepairTests
{
    [PostgresFact]
    public async Task NamedRecipientsAreRepairedWithoutLosingHistoryAndRepairIsIdempotent()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "seedrepair_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection); await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath=schema }.ConnectionString).Options);
            await db.Database.MigrateAsync();
            var bca = new Agency { Code="BCA", Name="Bộ Công an" };
            var all = new Agency { Code="ALL_AGENCIES", Name="Các bộ, ngành, địa phương", Type=AgencyTypeEnum.Special };
            var province = new Agency { Code="HN", Name="Hà Nội", Type=AgencyTypeEnum.Province };
            var doc = new Document { Id=Guid.Parse("26290000-0000-0000-0000-000000002629"), DocumentNumber="2629" };
            db.AddRange(bca, all, province, doc);
            GoalTaskItem Item(string code, string display) => new() { DocumentId=doc.Id, Code=code, Title=code, LeadAgencyId=all.Id, LeadAgencyDisplayText=display };
            var named=Item("MT-2629-01", "Văn phòng Chính phủ");
            var group=Item("NV-2629-02", "Các bộ, ngành");
            var multi=Item("NV-2629-03", "Bộ Khoa học và Công nghệ; Bộ Nội vụ");
            var manual=Item("NV-2629-04", "Bộ Tài chính"); manual.LeadAgencyId=province.Id;
            db.AddRange(named,group,multi,manual); await db.SaveChangesAsync();
            await AgencyAssignments.InitializeMissingAsync(db);
            var oldRow=await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==named.Id && e.AgencyId==bca.Id);
            oldRow.CompletionPercentage=55;
            var log=new ProgressLog { GoalTaskId=named.Id, AgencyId=bca.Id, PeriodYear=2026, SummaryNotes="Keep evidence", ApprovalStatus=ApprovalStatusEnum.Approved };
            db.Add(log); await db.SaveChangesAsync();
            Assert.Equal(3,await Seed2629AssignmentRepair.RunAsync(db));
            Assert.False(oldRow.IsLeadAssignment); Assert.Equal(55,oldRow.CompletionPercentage);
            Assert.Equal("Keep evidence",(await db.ProgressLogs.FindAsync(log.Id))!.SummaryNotes);
            var target=await db.Agencies.SingleAsync(a=>a.Code=="VPCP");
            Assert.Equal(target.Id,named.LeadAgencyId);
            Assert.True(await db.AgencyTaskExecutions.AnyAsync(e=>e.GoalTaskId==named.Id && e.AgencyId==target.Id && e.IsLeadAssignment));
            Assert.False(await db.AgencyTaskExecutions.AnyAsync(e=>e.GoalTaskId==group.Id && e.AgencyId==province.Id && e.IsLeadAssignment));
            Assert.True(await db.AgencyTaskExecutions.AnyAsync(e=>e.GoalTaskId==group.Id && e.AgencyId==bca.Id && e.IsLeadAssignment));
            Assert.Equal(2,await db.AgencyTaskExecutions.CountAsync(e=>e.GoalTaskId==multi.Id && e.IsLeadAssignment));
            Assert.Equal(province.Id,manual.LeadAgencyId);
            var count=await db.AgencyTaskExecutions.CountAsync(); var agencyCount=await db.Agencies.CountAsync();
            await Seed2629AssignmentRepair.RunAsync(db);
            Assert.Equal(count,await db.AgencyTaskExecutions.CountAsync()); Assert.Equal(agencyCount,await db.Agencies.CountAsync());
            Assert.Equal(oldRow.Id,(await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==named.Id && e.AgencyId==bca.Id)).Id);
        }
        finally { await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin); await command.ExecuteNonQueryAsync(); }
    }
}
