using Cdsqg.Api.Controllers;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;
public class GoalResultPlanPostgresTests
{
    [PostgresFact]
    public async Task MigrationAndGoalWritesPreserveLegacyTargetsAndReturnAnnualPlan()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "goal_result_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            var agency = new Agency { Code = "RESULT", Name = "Result" };
            var document = new Document { DocumentNumber = "RESULT", StartYear = 2025, EndYear = 2030 };
            var legacy = new GoalTaskItem { DocumentId = document.Id, LeadAgencyId = agency.Id, Code = "LEGACY", Title = "Legacy" };
            legacy.Baselines.Add(new TargetBaseline { Year = 2027, TargetQuantity = 50 });
            db.AddRange(agency, document, legacy); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            await db.GetService<IMigrator>().MigrateAsync("20261022100000_AddAgencyDisplayNames");
            await db.Database.MigrateAsync();
            Assert.Null((await db.GoalTaskItems.FindAsync(legacy.Id))!.ResultTarget);
            Assert.Equal(50m, (await db.TargetBaselines.SingleAsync(b => b.GoalTaskId == legacy.Id)).TargetQuantity);
            var controller = new GoalTaskItemController(db);
            var response = await controller.CreateItem(new CreateGoalTaskItemRequestDto {
                DocumentId = document.Id, ItemType = "Goal", Code = "MT-01", Title = "Ten products", LeadAgencyId = agency.Id,
                StartDate = new(2026, 1, 1), DueDate = new(2028, 12, 31), ResultTarget = 10
            });
            Assert.IsType<OkObjectResult>(response);
            db.ChangeTracker.Clear();
            var created = await db.GoalTaskItems.SingleAsync(i => i.Code == "MT-01");
            Assert.Equal(10m, created.ResultTarget);
            var grid = await new PlanningService(db).GetDocumentPlanningGridAsync(document.Id);
            var row = Assert.Single(grid.Items, i => i.Code == "MT-01");
            Assert.Equal(new decimal[] { 3, 3, 4 }, row.YearlyAllocations.Values);
            Assert.Equal(new decimal[] { 3, 6, 10 }, row.YearlyTargets.Where(p => p.Value != null).Select(p => (decimal)p.Value!));
            Assert.Null(row.YearlyTargets[2025]);
            Assert.Null(row.YearlyTargets[2029]);
            Assert.Null(row.YearlyTargets[2030]);
            await Assert.ThrowsAsync<InvalidOperationException>(() => new PlanningService(db).UpdateYearlyTargetAsync(created.Id, new UpdateYearlyTargetDto { Year = 2025, TargetQuantity = 99 }));
            await Assert.ThrowsAsync<InvalidOperationException>(() => new PlanningService(db).UpdateYearlyTargetAsync(created.Id, new UpdateYearlyTargetDto { Year = 2029, TargetQuantity = 99 }));
            await Assert.ThrowsAsync<InvalidOperationException>(() => new PlanningService(db).UpdateTaskCustomBaselineAsync(created.Id, new UpdateCustomBaselineDto { Milestones = new() { ["2025"] = "99" } }));
            var update = await controller.UpdateItem(created.Id, new UpdateGoalTaskItemRequestDto {
                Code = created.Code, Title = created.Title, LeadAgencyId = agency.Id,
                StartDate = new(2026, 1, 1), DueDate = new(2028, 12, 31), ResultTarget = 12
            });
            Assert.IsType<OkObjectResult>(update);
            db.ChangeTracker.Clear();
            Assert.Equal(12m, (await db.GoalTaskItems.FindAsync(created.Id))!.ResultTarget);
            var invalid = await controller.CreateItem(new CreateGoalTaskItemRequestDto {
                DocumentId = document.Id, ItemType = "Goal", Code = "INVALID", Title = "Invalid", LeadAgencyId = agency.Id,
                StartDate = new(2026, 1, 1), DueDate = new(2028, 12, 31), ResultTarget = -1
            });
            Assert.IsType<BadRequestObjectResult>(invalid);
            Assert.False(await db.GoalTaskItems.AnyAsync(i => i.Code == "INVALID"));
        }
        finally { await using var cleanup = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin); await cleanup.ExecuteNonQueryAsync(); }
    }
}