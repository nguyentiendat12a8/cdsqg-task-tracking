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
            await AssignmentTestData.PrepareAsync(db);
            var service = new ExecutionService(db, null!);
            var adminAgency = new Agency { Code = "ADMIN_ACTOR", Name = "Cơ quan của người quản trị" };
            var demoTask = new GoalTaskItem { Code = "NV_ADMIN_IMPORT", DocumentId = document.Id, LeadAgencyId = agency.Id,
                ItemType = ItemTypeEnum.Task, EvaluationType = EvaluationTypeEnum.Qualitative,
                Deliverables = new() { new() { Id = "demo_product", Title = "Sản phẩm demo" } } };
            db.AddRange(adminAgency, demoTask); await db.SaveChangesAsync();
            await AssignmentTestData.PrepareAsync(db);
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
                Assert.Equal(state == "Completed" ? ExecutionStatusEnum.Completed : ExecutionStatusEnum.InProgress,
                    ProgressCalculator.Evaluate(demoTask, year: ReportingPeriod.CurrentYear).Status);
                var adminList = await new PlanningService(db).GetDocumentItemsPagedAsync(document.Id,
                    new() { UserRole = "Admin", AgencyId = adminAgency.Id });
                Assert.Equal(state == "Completed" ? "Completed" : "InProgress",
                    adminList.Items.Single(i => i.Code == demoTask.Code).CalculatedStatus);
                var adminHistory = await service.GetTaskProgressHistoryAsync(demoTask.Id);
                Assert.Equal(state == "Completed" ? "Completed" : "InProgress", adminHistory[0].CalculatedStatus);
            }
            db.ProgressLogs.RemoveRange(db.ProgressLogs.Where(p => p.GoalTaskId == demoTask.Id));
            await db.SaveChangesAsync();
            await AssignmentTestData.PrepareAsync(db);
            var group = new Agency { Code = "ALL_AGENCIES", Type = AgencyTypeEnum.Special };
            var provinceGroup = new Agency { Code = "ALL_PROVINCES", Type = AgencyTypeEnum.Special };
            var province = new Agency { Code = "GROUP_PROVINCE", Type = AgencyTypeEnum.Province };
            var inactive = new Agency { Code = "GROUP_INACTIVE", Type = AgencyTypeEnum.Province, IsActive = false };
            var other = new Agency { Code = "GROUP_OTHER", Type = AgencyTypeEnum.Other };
            var child = new Agency { Code = "GROUP_CHILD", ParentId = agency.Id };
            var groupTask = new GoalTaskItem { Code = "NV_GROUP", DocumentId = document.Id, LeadAgencyId = group.Id,
                IsGeneralTask = true, EvaluationType = EvaluationTypeEnum.Qualitative,
                Deliverables = new() { new() { Id = "gp", Title = "Sản phẩm nhóm" } } };
            db.AddRange(group, provinceGroup, province, inactive, other, child, groupTask); await db.SaveChangesAsync();
            await AssignmentTestData.PrepareAsync(db);
            var groupDto = new SubmitProgressRequestDto { AgencyId = group.Id, UserRole = "Admin", CreatedBy = "Admin thực hiện",
                PeriodYear = ReportingPeriod.CurrentYear, Deliverables = new() { new() { Id = "gp", Title = "Sản phẩm nhóm", CurrentStatus = "Completed" } } };
            var groupResult = await service.SubmitProgressAsync(groupTask.Id, groupDto);
            Assert.Equal(3, groupResult.ReportCount);
            Assert.Equal(3, await db.ProgressLogs.CountAsync(p => p.GoalTaskId == groupTask.Id));
            Assert.DoesNotContain(group.Id, groupResult.ReportingAgencyIds);
            Assert.DoesNotContain(other.Id, groupResult.ReportingAgencyIds);
            Assert.DoesNotContain(inactive.Id, groupResult.ReportingAgencyIds);
            foreach (var id in groupResult.ReportingAgencyIds)
            {
                var row = await service.GetProgressLogAsync(groupTask.Id, ReportingPeriod.CurrentYear, id);
                Assert.Equal("Completed", row!.CalculatedStatus);
                Assert.Equal("Completed", row.Deliverables![0].CurrentStatus);
            }
            groupDto.AgencyId = provinceGroup.Id;
            var provinceResult = await service.SubmitProgressAsync(groupTask.Id, groupDto);
            Assert.Equal(new[] { province.Id }, provinceResult.ReportingAgencyIds);
            groupDto.UserRole = "AgencyUser";
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitProgressAsync(groupTask.Id, groupDto));
            groupDto.UserRole = "Admin";
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitProgressAsync(demoTask.Id, groupDto));
            groupDto.AgencyId = group.Id;
            var previousGroupReports = await db.ProgressLogs.CountAsync(p => p.GoalTaskId == groupTask.Id);
            // The only interpolated value is a generated Guid from this fixture.
            var rejectionConstraintSql = $"ALTER TABLE \"ProgressLogs\" ADD CONSTRAINT group_test_reject CHECK (\"AgencyId\" <> '{province.Id}'::uuid) NOT VALID";
            await db.Database.ExecuteSqlRawAsync(rejectionConstraintSql);
            await Assert.ThrowsAsync<DbUpdateException>(() => service.SubmitProgressAsync(groupTask.Id, groupDto));
            db.ChangeTracker.Clear();
            Assert.Equal(previousGroupReports, await db.ProgressLogs.CountAsync(p => p.GoalTaskId == groupTask.Id));
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ProgressLogs\" DROP CONSTRAINT group_test_reject");
            db.ProgressLogs.RemoveRange(db.ProgressLogs.Where(p => p.GoalTaskId == groupTask.Id));
            await db.SaveChangesAsync();
            await AssignmentTestData.PrepareAsync(db);
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
            var pendingBefore = await db.ProgressLogs.SingleAsync(x => x.PeriodYear == (reportYear + 1));
            pendingBefore.AttachmentFileUrls = new() { "/evidence/keep.pdf" };
            db.Entry(pendingBefore).Property(x => x.AttachmentFileUrls).IsModified = true;
            await db.SaveChangesAsync();
            var duplicatePending = await service.ImportProgressBulkAsync(new() { UserRole = "AgencyUser", UserAgencyId = agency.Id, Items = new() { new() { Code = goal.Code, Value = 45, PeriodYear = (reportYear + 1) } } });
            Assert.Equal(1, duplicatePending.PendingCount);
            Assert.Equal(45m, (await db.ProgressLogs.SingleAsync(x => x.PeriodYear == (reportYear + 1))).QuantitativeValue);
            var pendingAfter = await db.ProgressLogs.SingleAsync(x => x.PeriodYear == (reportYear + 1));
            Assert.Equal(pendingBefore.Id, pendingAfter.Id);
            Assert.Contains("/evidence/keep.pdf", pendingAfter.AttachmentFileUrls);
            var edited = await service.SubmitProgressAsync(goal.Id, new() { UserRole="AgencyUser", AgencyId=agency.Id,
                PeriodYear=reportYear+1, Value=46, SummaryNotes="Updated pending", ReplaceExistingFiles=true, ExistingFiles=new() });
            Assert.Equal(pendingBefore.Id, edited.ProgressLogId);
            Assert.Equal("Pending", edited.ApprovalStatus);
            Assert.Empty(pendingAfter.AttachmentFileUrls);
            Assert.Equal("Updated pending", pendingAfter.SummaryNotes);
            Assert.Equal(3, await db.ProgressLogs.CountAsync());
            Assert.Equal("Pending", (await service.GetProgressLogAsync(goal.Id, (reportYear + 1), agency.Id))!.ApprovalStatus);
            var officialItem = await db.GoalTaskItems.Include(x => x.ProgressLogs).SingleAsync(x => x.Id == goal.Id);
            Assert.Null(ProgressCalculator.LatestApproved(officialItem, agency.Id, (reportYear + 1)));
            Assert.Equal(35.5m, ProgressCalculator.LatestApproved(officialItem, agency.Id)!.QuantitativeValue);
            Assert.Equal(35.5m, (await service.GetProgressLogAsync(goal.Id, reportYear, agency.Id))!.ActualValue);
            var sameYear = await service.SubmitProgressAsync(goal.Id, new() { UserRole="AgencyUser", AgencyId=agency.Id, PeriodYear=reportYear, Value=49 });
            Assert.Equal("Pending", sameYear.ApprovalStatus);
            Assert.Equal(49m, (await service.GetProgressLogAsync(goal.Id,reportYear,agency.Id,preferPending:true))!.ActualValue);
            Assert.Equal(35.5m, (await service.GetProgressLogAsync(goal.Id,reportYear,agency.Id))!.ActualValue);

            var scope = group;
            var general = new GoalTaskItem { Code = "MT_GENERAL", DocumentId = document.Id, LeadAgencyId = scope.Id, IsGeneralTask = true, EvaluationType = EvaluationTypeEnum.Quantitative };
            db.Add(general); await db.SaveChangesAsync();
            await AssignmentTestData.PrepareAsync(db);
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
            await AssignmentTestData.PrepareAsync(db);
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
            var sharedProducts = new GoalTaskItem { Code="NV_SHARED_PENDING", DocumentId=document.Id, LeadAgencyId=scope.Id,
                IsGeneralTask=true, EvaluationType=EvaluationTypeEnum.Qualitative,
                Deliverables=new() { new() { Id="shared-p",Title="Shared product" } } };
            db.Add(sharedProducts); await db.SaveChangesAsync(); await AssignmentTestData.PrepareAsync(db);
            var otherReport=await service.SubmitProgressAsync(sharedProducts.Id,new() { UserRole="Admin", AgencyId=province.Id,PeriodYear=2026,
                Deliverables=new() {new() {Id="shared-p",Title="Shared product",CurrentStatus="Completed"}} });
            var ownReport=await service.SubmitProgressAsync(sharedProducts.Id,new() { UserRole="AgencyUser",AgencyId=agency.Id,PeriodYear=2026,
                Deliverables=new() {new() {Id="shared-p",Title="Shared product",CurrentStatus="Drafting",DocumentNumber="PENDING-META"}} });
            var ownEdit=await service.ImportProgressBulkAsync(new() { UserRole="AgencyUser",UserAgencyId=agency.Id,Items=new() {
                new() {Code=sharedProducts.Code,PeriodYear=2026,Deliverables=new() {new() {Id="shared-p",CurrentStatus="Completed"}}} } });
            Assert.Equal(1,ownEdit.PendingCount);
            var ownPending=await db.ProgressLogs.SingleAsync(p=>p.Id==ownReport.ProgressLogId);
            Assert.Equal(ApprovalStatusEnum.Pending,ownPending.ApprovalStatus);
            Assert.Equal("Completed",ownPending.Deliverables!.Single().CurrentStatus);
            Assert.Equal("PENDING-META",ownPending.Deliverables.Single().DocumentNumber);
            Assert.Equal(2,await db.ProgressLogs.CountAsync(p=>p.GoalTaskId==sharedProducts.Id));
            Assert.Equal(ApprovalStatusEnum.Approved,(await db.ProgressLogs.SingleAsync(p=>p.Id==otherReport.ProgressLogId)).ApprovalStatus);
            Assert.Equal("NotStarted",sharedProducts.Deliverables.Single().CurrentStatus);
            // Completion does not lock an administrator out of corrections or evidence updates.
            var completedGoal=await service.SubmitProgressAsync(goal.Id,new() {UserRole="Admin",AgencyId=agency.Id,PeriodYear=2026,Value=100});
            var correctedGoal=await service.SubmitProgressAsync(goal.Id,new() {UserRole="Admin",AgencyId=agency.Id,PeriodYear=2026,Value=100,SummaryNotes="Admin corrected completed goal",ExistingFiles=new(){"/evidence/goal.pdf"}});
            Assert.Equal("Approved",correctedGoal.ApprovalStatus);
            Assert.NotEqual(completedGoal.ProgressLogId,correctedGoal.ProgressLogId);
            Assert.Equal("Admin corrected completed goal",(await db.ProgressLogs.SingleAsync(p=>p.Id==correctedGoal.ProgressLogId)).SummaryNotes);
            Assert.Contains("/evidence/goal.pdf",(await db.ProgressLogs.SingleAsync(p=>p.Id==correctedGoal.ProgressLogId)).AttachmentFileUrls);
            Assert.Equal(100m,(await db.ProgressLogs.SingleAsync(p=>p.Id==completedGoal.ProgressLogId)).QuantitativeValue);
            var completedTask=await service.SubmitProgressAsync(productTask.Id,new() {UserRole="Admin",AgencyId=agency.Id,PeriodYear=2026,Deliverables=new(){new(){Id="p1",Title="Sản phẩm 1",CurrentStatus="Completed"},new(){Id="p2",Title="Sản phẩm 2",CurrentStatus="Completed"}}});
            var correctedTask=await service.SubmitProgressAsync(productTask.Id,new() {UserRole="Admin",AgencyId=agency.Id,PeriodYear=2026,SummaryNotes="Admin corrected completed task",Deliverables=new(){new(){Id="p1",Title="Sản phẩm 1",CurrentStatus="Completed",DocumentNumber="NEW-01"},new(){Id="p2",Title="Sản phẩm 2",CurrentStatus="Completed"}}});
            Assert.Equal("Approved",correctedTask.ApprovalStatus);
            var correctedTaskLog=await db.ProgressLogs.SingleAsync(p=>p.Id==correctedTask.ProgressLogId);
            Assert.Equal("Admin corrected completed task",correctedTaskLog.SummaryNotes);
            Assert.Equal("NEW-01",correctedTaskLog.Deliverables!.First().DocumentNumber);
            Assert.NotEqual(completedTask.ProgressLogId,correctedTask.ProgressLogId);
            Assert.Equal(100m,correctedTaskLog.CalculatedProgressPercentage);


        }
        finally
        {
            // Generated test schema only; application data is never touched.
            await using var cmd = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
