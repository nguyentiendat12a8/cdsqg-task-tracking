using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class ProgressImportPostgresTests
{
    [PostgresFact]
    public async Task AnnualBulkImportPersistsTotalsQualitativeAndPendingWithoutUpdatingOfficialProgress()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        int reportYear = ReportingPeriod.CurrentYear - 1;
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "import_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await cmd.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            var agency = new Agency { Code = "IMPORT", Name = "Đơn vị kiểm thử" };
            var document = new Document { DocumentNumber = "IMPORT-TEST" };
            var goal = new GoalTaskItem { Code = "MT_IMPORT", DocumentId = document.Id, LeadAgencyId = agency.Id, EvaluationType = EvaluationTypeEnum.Quantitative, ItemType = ItemTypeEnum.Goal, CustomBaseline = new() { [reportYear.ToString()] = "50" }, DueDate = new DateTime(2030, 12, 31) };
            var task = new GoalTaskItem { Code = "NV_IMPORT", DocumentId = document.Id, LeadAgencyId = agency.Id, EvaluationType = EvaluationTypeEnum.Qualitative };
            db.AddRange(agency, document, goal, task);
            await db.SaveChangesAsync();
            var service = new ExecutionService(db, null!);
            var adminAgency = new Agency { Code = "ADMIN_ACTOR", Name = "Cơ quan của người quản trị" };
            var demoTask = new GoalTaskItem { Code = "NV_ADMIN_IMPORT", DocumentId = document.Id, LeadAgencyId = agency.Id,
                ItemType = ItemTypeEnum.Task, EvaluationType = EvaluationTypeEnum.Qualitative,
                Deliverables = new() { new() { Id = "demo_product", Title = "Sản phẩm demo" } } };
            db.AddRange(adminAgency, demoTask); await db.SaveChangesAsync();
            foreach (var state in new[] { "Drafting", "Completed" })
            {
                var adminImported = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", CreatedBy = "Admin demo", UserAgencyId = adminAgency.Id,
                    Items = new() { new() { Code = demoTask.Code, PeriodYear = ReportingPeriod.CurrentYear,
                        Deliverables = new() { new() { Id = "demo_product", CurrentStatus = state } } } } });
                Assert.Equal(1, adminImported.SuccessCount);
                var recorded = ProgressCalculator.AnnualReport(demoTask, null, ReportingPeriod.CurrentYear);
                Assert.NotNull(recorded);
                Assert.Equal(agency.Id, recorded!.AgencyId);
                Assert.Equal("Admin demo", recorded.CreatedBy);
                Assert.Equal(state == "Completed" ? ExecutionStatusEnum.CompletedOnTime : ExecutionStatusEnum.InProgressOnTime,
                    ProgressCalculator.Evaluate(demoTask, year: ReportingPeriod.CurrentYear).Status);
            }
            db.ProgressLogs.RemoveRange(db.ProgressLogs.Where(p => p.GoalTaskId == demoTask.Id));
            await db.SaveChangesAsync();
            var futureError = await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitProgressAsync(goal.Id, new() { PeriodYear = ReportingPeriod.CurrentYear + 1, Value = 10, CreatedBy = "Admin", AgencyId = agency.Id }));
            Assert.Contains("không được lớn hơn", futureError.Message);
            var futureImport = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", UserAgencyId = agency.Id, CreatedBy = "Admin", Items = new() { new() { Code = goal.Code, Value = 10, PeriodYear = ReportingPeriod.CurrentYear + 1 } } });
            Assert.Equal(1, futureImport.FailureCount);
            Assert.Equal(0, await db.ProgressLogs.CountAsync());
            var result = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", UserAgencyId = agency.Id, CreatedBy = "Quản trị kiểm thử", Items = new() {
                new() { Code = goal.Code, Value = 35.5m, PeriodYear = reportYear, SummaryNotes = "Ghi chú tiếng Việt" },
                new() { Code = task.Code, Status = TextStatusEnum.Completed, PeriodYear = reportYear },
                new() { Code = "UNKNOWN", Value = 10, PeriodYear = reportYear },
                new() { Code = goal.Code, Value = -1, PeriodYear = (reportYear + 1) }
            }});
            Assert.Equal(2, result.SuccessCount); Assert.Equal(2, result.FailureCount);
            db.ChangeTracker.Clear();
            var logs = await db.ProgressLogs.ToListAsync();
            Assert.Equal(2, logs.Count);
            var numeric = Assert.Single(logs, x => x.GoalTaskId == goal.Id);
            Assert.Equal(35.5m, numeric.QuantitativeValue); Assert.Equal(reportYear, numeric.PeriodYear);
            Assert.Equal("Quản trị kiểm thử", numeric.CreatedBy);
            var history = await service.GetTaskProgressHistoryAsync(goal.Id);
            Assert.Contains(history, row => row.CreatedBy == "Quản trị kiểm thử" && row.AgencyName == agency.Name);
            Assert.Equal("Ghi chú tiếng Việt", numeric.SummaryNotes);
            Assert.Equal(ApprovalStatusEnum.Approved, numeric.ApprovalStatus);
            var text = Assert.Single(logs, x => x.GoalTaskId == task.Id);
            Assert.Equal(TextStatusEnum.Completed, text.QualitativeStatus);
            var pending = await service.ImportProgressBulkAsync(new() { UserRole = "AgencyUser", UserAgencyId = agency.Id, Items = new() { new() { Code = goal.Code, Value = 40, PeriodYear = (reportYear + 1) } } });
            Assert.Equal(1, pending.PendingCount); Assert.Equal(0, pending.SuccessCount);
            Assert.Equal(ApprovalStatusEnum.Pending, (await db.ProgressLogs.SingleAsync(x => x.PeriodYear == (reportYear + 1))).ApprovalStatus);
            var duplicatePending = await service.ImportProgressBulkAsync(new() { UserRole = "AgencyUser", UserAgencyId = agency.Id, Items = new() { new() { Code = goal.Code, Value = 45, PeriodYear = (reportYear + 1) } } });
            Assert.Equal(1, duplicatePending.FailureCount);
            Assert.Equal(3, await db.ProgressLogs.CountAsync());
            Assert.Equal("Pending", (await service.GetProgressLogAsync(goal.Id, (reportYear + 1), agency.Id))!.ApprovalStatus);
            var officialItem = await db.GoalTaskItems.Include(x => x.ProgressLogs).SingleAsync(x => x.Id == goal.Id);
            Assert.Null(ProgressCalculator.LatestApproved(officialItem, agency.Id, (reportYear + 1)));
            Assert.Equal(35.5m, ProgressCalculator.LatestApproved(officialItem, agency.Id)!.QuantitativeValue);
            Assert.Equal(35.5m, (await service.GetProgressLogAsync(goal.Id, reportYear, agency.Id))!.ActualValue);
            var scope = new Agency { Code = "ALL_AGENCIES", Name = "Các bộ, ngành, địa phương" };
            var general = new GoalTaskItem { Code = "MT_GENERAL", DocumentId = document.Id, LeadAgencyId = scope.Id, IsGeneralTask = true, EvaluationType = EvaluationTypeEnum.Quantitative };
            db.AddRange(scope, general); await db.SaveChangesAsync();
            var rejected = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", CreatedBy = "Admin", Items = new() { new() { Code = general.Code, Value = 20000, PeriodYear = 2026 } } });
            Assert.Equal(1, rejected.FailureCount);
            var imported = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", CreatedBy = "Admin", ReportingAgencyId = agency.Id, Items = new() { new() { Code = general.Code, Value = 20000, PeriodYear = 2026 } } });
            Assert.Equal(1, imported.SuccessCount);
            Assert.Equal(scope.Id, general.LeadAgencyId);
            var existing = await service.GetProgressLogAsync(general.Id, 2026, agency.Id);
            Assert.Equal(20000m, existing!.ActualValue); Assert.Equal(agency.Id, existing.AgencyId);
            Assert.Equal("Admin", (await db.ProgressLogs.SingleAsync(x => x.GoalTaskId == general.Id)).CreatedBy);
            Assert.Null(await service.GetProgressLogAsync(general.Id, 2027, agency.Id));
            var productTask = new GoalTaskItem { Code = "NV_PRODUCTS", DocumentId = document.Id, LeadAgencyId = agency.Id, EvaluationType = EvaluationTypeEnum.Qualitative,
                Deliverables = new() { new() { Id = "p1", Title = "Sản phẩm 1", DocumentNumber = "KEEP" }, new() { Id = "p2", Title = "Sản phẩm 2" } } };
            db.Add(productTask); await db.SaveChangesAsync();
            var invalidProducts = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", CreatedBy = "Admin", Items = new() { new() { Code = productTask.Code, PeriodYear = 2026, Deliverables = new() { new() { Id = "p1", CurrentStatus = "Completed" } } } } });
            Assert.Equal(1, invalidProducts.FailureCount);
            var productsResult = await service.ImportProgressBulkAsync(new() { UserRole = "Admin", CreatedBy = "Admin", Items = new() { new() { Code = productTask.Code, PeriodYear = 2026, Deliverables = new() { new() { Id = "p1", Title = "Tampered", CurrentStatus = "Completed" }, new() { Id = "p2", CurrentStatus = "Drafting" } } } } });
            Assert.Equal(1, productsResult.SuccessCount);
            var productsLog = await service.GetProgressLogAsync(productTask.Id, 2026, agency.Id);
            Assert.Equal(2, productsLog!.Deliverables!.Count);
            Assert.Equal("Sản phẩm 1", productsLog.Deliverables[0].Title);
            Assert.Equal("KEEP", productsLog.Deliverables[0].DocumentNumber);
            Assert.Equal("Completed", productsLog.Deliverables[0].CurrentStatus);
            Assert.Equal("Drafting", productsLog.Deliverables[1].CurrentStatus);
            Assert.Equal(62.5m, productsLog.CompletionPercentage);
        }
        finally
        {
            // Generated test schema only; application data is never touched.
            await using var cmd = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
