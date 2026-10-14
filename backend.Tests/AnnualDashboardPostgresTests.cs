using Cdsqg.Api.Controllers;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;
public class AnnualDashboardPostgresTests
{
    [PostgresFact]
    public async Task DashboardAndReportGridUseExactApprovedYearAndBaselineWithoutCachedProducts()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "year_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await cmd.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            var agency = new Agency { Code = "TEST_YEAR", Type = AgencyTypeEnum.Ministry, IsActive = true };
            var doc = new Document { DocumentNumber = "YEAR", StartYear = 2026, EndYear = 2030 };
            var goal = new GoalTaskItem { Code = "MT_YEAR", DocumentId = doc.Id, LeadAgencyId = agency.Id, ItemType = ItemTypeEnum.Goal, EvaluationType = EvaluationTypeEnum.Quantitative, DueDate = new(2030,12,31), CustomBaseline = new() { ["2026"] = "50", ["2027"] = "100" } };
            var task = new GoalTaskItem { Code = "NV_YEAR", DocumentId = doc.Id, LeadAgencyId = agency.Id, ItemType = ItemTypeEnum.Task, EvaluationType = EvaluationTypeEnum.Qualitative, DueDate = new(2030,12,31), Deliverables = new() { new() { Id="p", Title="Product", CurrentStatus="Completed" } } };
            db.AddRange(agency, doc, goal, task);
            foreach (var (year, value, day, approval) in new[] { (2026,20m,1,ApprovalStatusEnum.Approved), (2026,30m,2,ApprovalStatusEnum.Approved), (2027,100m,3,ApprovalStatusEnum.Approved), (2027,99m,4,ApprovalStatusEnum.Pending), (2027,1m,5,ApprovalStatusEnum.Rejected) })
                db.Add(new ProgressLog { GoalTaskId=goal.Id, AgencyId=agency.Id, PeriodYear=year, QuantitativeValue=value, LogDate=new(year,10,day), ApprovalStatus=approval });
            foreach (var (year, state) in new[] { (2026,"Drafting"), (2027,"Completed") })
                db.Add(new ProgressLog { GoalTaskId=task.Id, AgencyId=agency.Id, PeriodYear=year, LogDate=new(year,10,1), ApprovalStatus=ApprovalStatusEnum.Approved, Deliverables=new() { new() { Id="p", Title="Product", CurrentStatus=state } } });
            await db.SaveChangesAsync();
            var planning = new PlanningService(db);
            var dashboard = new DashboardController(db);
            foreach (var (year, percentage, productsPercentage) in new[] { (2026,60m,25m), (2027,100m,100m), (2028,(decimal?)100m,100m) })
            {
                var grid = await planning.GetDocumentPlanningGridAsync(doc.Id, reportYear: year);
                Assert.Equal(percentage, grid.Items.Single(i=>i.Code==goal.Code).CompletionPercentage);
                Assert.Equal(productsPercentage, grid.Items.Single(i=>i.Code==task.Code).CompletionPercentage);
                var response = Assert.IsType<OkObjectResult>(await dashboard.GetAgencyItems(agency.Id, reportYear: year));
                var json = System.Text.Json.JsonSerializer.SerializeToElement(response.Value, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy=System.Text.Json.JsonNamingPolicy.CamelCase });
                var row = json.EnumerateArray().Single(i=>i.GetProperty("code").GetString()==goal.Code);
                if (percentage.HasValue) Assert.Equal(percentage.Value, row.GetProperty("latestProgressPercent").GetDecimal());
                else Assert.Equal(System.Text.Json.JsonValueKind.Null, row.GetProperty("latestProgressPercent").ValueKind);
                var metrics = Assert.IsType<OkObjectResult>(await dashboard.GetDashboardMetrics(reportYear: year));
                var stats = System.Text.Json.JsonSerializer.SerializeToElement(metrics.Value);
                Assert.Equal(year>=2027 ? 1 : 0, stats.GetProperty("completedGoals").GetInt32());
                Assert.Equal(year>=2027 ? 1 : 0, stats.GetProperty("completedTasks").GetInt32());
            }
            // A historical execution recorded under a different agency does
            // not establish ownership of this ministry's specific goal.
            var province = new Agency { Code = "AN_GIANG_TEST", Name = "An Giang", Type = AgencyTypeEnum.Province, IsActive = true };
            var all = new Agency { Code = "ALL_AGENCIES", Type = AgencyTypeEnum.Special, IsActive = true };
            var general = new GoalTaskItem { Code = "MT_GENERAL", DocumentId = doc.Id, LeadAgencyId = all.Id,
                ItemType = ItemTypeEnum.Goal, EvaluationType = EvaluationTypeEnum.Quantitative, IsGeneralTask = true };
            db.AddRange(province, all, general, new AgencyTaskExecution { GoalTaskId = goal.Id, AgencyId = province.Id });
            await db.SaveChangesAsync();
            var provinceResponse = Assert.IsType<OkObjectResult>(await dashboard.GetAgencyItems(province.Id, reportYear: 2026));
            var provinceRows = System.Text.Json.JsonSerializer.SerializeToElement(provinceResponse.Value,
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            Assert.DoesNotContain(provinceRows.EnumerateArray(), row => row.GetProperty("code").GetString() == goal.Code);
            Assert.Contains(provinceRows.EnumerateArray(), row => row.GetProperty("code").GetString() == general.Code);
            var provinceMetrics = Assert.IsType<OkObjectResult>(await dashboard.GetDashboardMetrics(reportYear: 2026));
            var provinceStats = System.Text.Json.JsonSerializer.SerializeToElement(provinceMetrics.Value,
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            var provinceSummary = provinceStats.GetProperty("provincesPerformance").EnumerateArray()
                .Single(row => row.GetProperty("agencyId").GetGuid() == province.Id);
            Assert.Equal(1, provinceSummary.GetProperty("totalGoals").GetInt32());
            goal.AssignedAgencyId = province.Id;
            await db.SaveChangesAsync();
            var assignedResponse = Assert.IsType<OkObjectResult>(await dashboard.GetAgencyItems(province.Id, reportYear: 2026));
            var assignedRows = System.Text.Json.JsonSerializer.SerializeToElement(assignedResponse.Value,
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
            Assert.Contains(assignedRows.EnumerateArray(), row => row.GetProperty("code").GetString() == goal.Code);
            Assert.IsType<BadRequestObjectResult>(await dashboard.GetDashboardMetrics(reportYear: 10000));
        }
        finally
        {
            await using var cmd = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
