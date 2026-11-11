using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Cdsqg.Tests;

public class AgencyProgressReportTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAgencyProgressReport_ReturnsCorrectGoalsAndTasksForAgency()
    {
        var dbName = "AgencyReportTest_" + Guid.NewGuid().ToString("N");
        await using var db = CreateInMemoryDb(dbName);

        var doc2629Id = Guid.Parse("26290000-0000-0000-0000-000000002629");
        var doc1266Id = Guid.Parse("12660000-0000-0000-0000-000000001266");

        var doc2629 = new Document { Id = doc2629Id, DocumentNumber = "2629/QĐ-TTg", Name = "Chương trình 2629" };
        var doc1266 = new Document { Id = doc1266Id, DocumentNumber = "1266/QĐ-TTg", Name = "Chiến lược 1266" };

        var agency = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };
        var otherAgency = new Agency { Id = Guid.NewGuid(), Code = "BTTTT", Name = "Bộ Thông tin và Truyền thông", Type = AgencyTypeEnum.Ministry };

        // Goal 1: 2629 Phase 1 (2025-2027)
        var goal1 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "MT-08",
            Title = "Tỉ lệ trưởng thành số",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Section = "Giai đoạn 2025-2027",
            DueDate = new DateTime(2027, 12, 31),
            Baselines = new List<TargetBaseline>
            {
                new() { Year = 2027, TargetQuantity = 100, ConfiguredTargetQuantity = 60 }
            },
            ProgressLogs = new List<ProgressLog>
            {
                new() { AgencyId = agency.Id, PeriodYear = 2025, QuantitativeValue = 50, SummaryNotes = "Đạt mục tiêu 2025" }
            }
        };

        // Goal 2: 2629 Phase 2 (2028-2030)
        var goal2 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "MT-16",
            Title = "Cấp độ 4 dữ liệu",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Section = "Giai đoạn 2028-2030",
            DueDate = new DateTime(2030, 12, 31),
            Baselines = new List<TargetBaseline>
            {
                new() { Year = 2030, TargetQuantity = 100 }
            }
        };

        // Goal 3: 1266 Goal
        var goal3 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc1266Id,
            Document = doc1266,
            Code = "MT-04",
            Title = "Chia sẻ dữ liệu mở",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Baselines = new List<TargetBaseline>
            {
                new() { Year = 2030, TargetQuantity = 80 }
            }
        };

        // Task 1: 2629 Task for agency
        var task1 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "NV-01",
            Title = "Xây dựng Trung tâm dữ liệu quốc gia",
            ItemType = ItemTypeEnum.Task,
            EvaluationType = EvaluationTypeEnum.Qualitative,
            LeadAgencyId = agency.Id,
            CoordinatingAgencyIds = new List<Guid> { otherAgency.Id },
            DueDate = new DateTime(2026, 12, 31),
            Deliverables = new List<TaskDeliverable>
            {
                new() { Id = "DEL-01", Title = "Hệ thống CSDL" }
            }
        };

        // Task for other agency (should NOT appear)
        var taskOther = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "NV-OTHER",
            Title = "Nhiệm vụ của BTTTT",
            ItemType = ItemTypeEnum.Task,
            LeadAgencyId = otherAgency.Id
        };

        db.Agencies.AddRange(agency, otherAgency);
        db.Documents.AddRange(doc2629, doc1266);
        db.GoalTaskItems.AddRange(goal1, goal2, goal3, task1, taskOther);
        await db.SaveChangesAsync();

        var planningService = new PlanningService(db);

        // ACT
        var report = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin");

        // ASSERT
        Assert.NotNull(report);
        Assert.Equal(agency.Id, report.AgencyId);
        Assert.Equal("Bộ Công an", report.AgencyName);

        // Check Goals
        Assert.Equal(3, report.Goals.Count);
        var rGoal1 = report.Goals.Single(g => g.Code == "MT-08");
        Assert.Equal("2629/QĐ-TTg", rGoal1.DocumentNumber);
        Assert.Equal("100", rGoal1.TargetPhase1);
        Assert.Equal("60", rGoal1.Plan2027);
        Assert.Equal(50, rGoal1.Actual2025);
        Assert.Contains(2027, rGoal1.ApplicableYears);
        Assert.DoesNotContain(2028, rGoal1.ApplicableYears);
        Assert.Equal("—", rGoal1.Plan2028);

        var rGoal2 = report.Goals.Single(g => g.Code == "MT-16");
        Assert.Equal("100", rGoal2.TargetPhase2);
        Assert.Equal("", rGoal2.Plan2030);
        Assert.DoesNotContain(2025, rGoal2.ApplicableYears);
        Assert.Equal("—", rGoal2.Plan2027);

        var rGoal3 = report.Goals.Single(g => g.Code == "MT-04");
        Assert.Equal("1266/QĐ-TTg", rGoal3.DocumentNumber);
        Assert.Contains(2030, rGoal3.ApplicableYears);
        Assert.Equal("", rGoal3.Plan2030);

        // Check Tasks
        Assert.Single(report.Tasks);
        var rTask1 = report.Tasks[0];
        Assert.Equal("NV-01", rTask1.Code);
        Assert.Equal("2629/QĐ-TTg", rTask1.DocumentNumber);
        Assert.Contains("Bộ Thông tin và Truyền thông", rTask1.CoordinatingAgencies);
        Assert.Equal("Chưa thực hiện", rTask1.StatusLabel);
        Assert.Equal("Hệ thống CSDL", rTask1.Deliverables);
    }

    [Fact]
    public async Task AgencyProgressReport_Permissions_PreventUnauthorizedAccess()
    {
        var dbName = "AgencyReportPermTest_" + Guid.NewGuid().ToString("N");
        await using var db = CreateInMemoryDb(dbName);

        var agencyA = new Agency { Id = Guid.NewGuid(), Code = "AG_A", Name = "Cơ quan A", Type = AgencyTypeEnum.Ministry };
        var agencyB = new Agency { Id = Guid.NewGuid(), Code = "AG_B", Name = "Cơ quan B", Type = AgencyTypeEnum.Ministry };

        db.Agencies.AddRange(agencyA, agencyB);
        await db.SaveChangesAsync();

        var planningService = new PlanningService(db);

        // User from Agency A tries to access Agency B's report -> Should throw UnauthorizedAccessException
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await planningService.GetAgencyProgressReportAsync(agencyB.Id, 2026, agencyA.Id, "AgencyUser");
        });

        // User from Agency A accesses Agency A's report -> Should succeed
        var report = await planningService.GetAgencyProgressReportAsync(agencyA.Id, 2026, agencyA.Id, "AgencyUser");
        Assert.NotNull(report);
        Assert.Equal(agencyA.Id, report.AgencyId);
    }

    [Fact]
    public async Task ImportProgressAndPlans_ReflectsInGetAgencyProgressReport()
    {
        var dbName = "AgencyReportImportTest_" + Guid.NewGuid().ToString("N");
        await using var db = CreateInMemoryDb(dbName);

        var doc2629Id = Guid.Parse("26290000-0000-0000-0000-000000002629");
        var doc2629 = new Document { Id = doc2629Id, DocumentNumber = "2629/QĐ-TTg", Name = "QĐ 2629" };

        var agency = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };

        var goal = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "MT-08",
            Title = "Tỉ lệ trưởng thành số",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Section = "Giai đoạn 2025-2027",
            DueDate = new DateTime(2027, 12, 31)
        };

        var task = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "NV-01",
            Title = "Triển khai CSDL",
            ItemType = ItemTypeEnum.Task,
            EvaluationType = EvaluationTypeEnum.Qualitative,
            LeadAgencyId = agency.Id,
            DueDate = new DateTime(2026, 12, 31)
        };

        db.Agencies.Add(agency);
        db.Documents.Add(doc2629);
        db.GoalTaskItems.AddRange(goal, task);
        await db.SaveChangesAsync();
        await AssignmentTestData.PrepareAsync(db);

        var planningService = new PlanningService(db);
        var executionService = new ExecutionService(db, null!);

        // 1. Import plan target for MT-08 (Year 2027 = 95%)
        var planResult = await planningService.ImportYearlyTargetsAsync(
            doc2629Id,
            new ImportYearlyTargetsRequestDto
            {
                ConfirmOverwrite = true,
                Rows = new List<ImportYearlyTargetRowDto>
                {
                    new() { TaskId = goal.Id, Year = 2027, TargetQuantity = 95 }
                }
            },
            agency.Id,
            true
        );
        Assert.Equal(1, planResult.SavedCount);

        // 2. Import progress for MT-08 (Estimate 2026 = 75%) and NV-01 (Status = Completed, Value = 100%)
        var progressResult = await executionService.ImportProgressBulkAsync(new ImportProgressBulkRequestDto
        {
            UserAgencyId = agency.Id,
            ReportingAgencyId = agency.Id,
            UserRole = "Admin",
            Items = new List<ImportProgressBulkItemDto>
            {
                new()
                {
                    Code = goal.Code,
                    Value = 75,
                    PeriodYear = 2026,
                    SummaryNotes = "Ước tính đạt 75% năm 2026"
                },
                new()
                {
                    Code = task.Code,
                    Status = TextStatusEnum.Completed,
                    Value = 100,
                    PeriodYear = 2026,
                    SummaryNotes = "Đã hoàn thành sớm"
                }
            }
        });

        var msgs = string.Join(" | ", progressResult.Results.Select(r => $"{r.Code}: {r.Message} (Success={r.Success})"));
        Assert.True(progressResult.SuccessCount == 2, $"Failed with: {msgs}");

        // 3. Verify GetAgencyProgressReport reflects the imported progress and plans
        var updatedReport = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin");

        var uGoal = updatedReport.Goals.Single(g => g.Code == goal.Code);
        Assert.Equal(75, uGoal.Estimate2026);
        Assert.Equal("95", uGoal.Plan2027);
        Assert.Equal("Ước tính đạt 75% năm 2026", uGoal.Notes);

        var uTask = updatedReport.Tasks.Single(t => t.Code == task.Code);
        Assert.Equal("Completed", uTask.Status);
        Assert.Equal("Đã hoàn thành", uTask.StatusLabel);
        Assert.Equal(100, uTask.CompletionPercentage);
        Assert.Equal("Đã hoàn thành sớm", uTask.Notes);
    }

    [Fact]
    public async Task GetAgencyProgressReport_MergesMultiPhaseGoalsWithSameTitle()
    {
        var dbName = "AgencyReportMergeTest_" + Guid.NewGuid().ToString("N");
        await using var db = CreateInMemoryDb(dbName);

        var doc2629Id = Guid.Parse("26290000-0000-0000-0000-000000002629");
        var doc2629 = new Document { Id = doc2629Id, DocumentNumber = "2629/QĐ-TTg", Name = "Chương trình 2629" };
        var agency = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };

        // Phase 1 Goal: MT-10
        var goalPhase1 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "MT-10",
            Title = "Tỉ lệ cán bộ chuyên trách CNTT được đào tạo.",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Section = "Giai đoạn 2025-2027",
            DueDate = new DateTime(2027, 12, 31),
            Baselines = new List<TargetBaseline>
            {
                new() { Year = 2027, TargetQuantity = 50 }
            },
            ProgressLogs = new List<ProgressLog>
            {
                new() { AgencyId = agency.Id, PeriodYear = 2025, QuantitativeValue = 30 }
            }
        };

        // Phase 2 Goal: MT-21 (Same title)
        var goalPhase2 = new GoalTaskItem
        {
            Id = Guid.NewGuid(),
            DocumentId = doc2629Id,
            Document = doc2629,
            Code = "MT-21",
            Title = "Tỉ lệ cán bộ chuyên trách CNTT được đào tạo.",
            ItemType = ItemTypeEnum.Goal,
            EvaluationType = EvaluationTypeEnum.Quantitative,
            LeadAgencyId = agency.Id,
            Section = "Giai đoạn 2028-2030",
            DueDate = new DateTime(2030, 12, 31),
            Baselines = new List<TargetBaseline>
            {
                new() { Year = 2030, TargetQuantity = 100 }
            }
        };

        db.Agencies.Add(agency);
        db.Documents.Add(doc2629);
        db.GoalTaskItems.AddRange(goalPhase1, goalPhase2);
        await db.SaveChangesAsync();

        var planningService = new PlanningService(db);
        var report = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin");

        // ASSERT: 2 goals are merged into 1
        Assert.Single(report.Goals);
        var mergedGoal = report.Goals[0];
        Assert.Equal("MT-10\nMT-21", mergedGoal.Code);
        Assert.Equal(2, mergedGoal.SubCodes.Count);
        Assert.Contains("MT-10", mergedGoal.SubCodes);
        Assert.Contains("MT-21", mergedGoal.SubCodes);
        Assert.Equal("50", mergedGoal.TargetPhase1);
        Assert.Equal("100", mergedGoal.TargetPhase2);
        Assert.Equal(30, mergedGoal.Actual2025);
        Assert.Equal("", mergedGoal.Plan2027);
        Assert.Equal("", mergedGoal.Plan2030);
        Assert.Contains(2027, mergedGoal.ApplicableYears);
        Assert.Contains(2030, mergedGoal.ApplicableYears);
    }

    [Fact]
    public async Task GetAgencyProgressReport_TabLoadingAndPagination_WorksCorrectly()
    {
        var dbName = "AgencyReportTabPagingTest_" + Guid.NewGuid().ToString("N");
        await using var db = CreateInMemoryDb(dbName);

        var doc2629Id = Guid.Parse("26290000-0000-0000-0000-000000002629");
        var doc2629 = new Document { Id = doc2629Id, DocumentNumber = "2629/QĐ-TTg", Name = "Chương trình 2629" };
        var agency = new Agency { Id = Guid.NewGuid(), Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };

        db.Agencies.Add(agency);
        db.Documents.Add(doc2629);

        for (int i = 1; i <= 5; i++)
        {
            db.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                DocumentId = doc2629Id,
                Document = doc2629,
                Code = $"MT-{i:D2}",
                Title = $"Mục tiêu số {i}",
                ItemType = ItemTypeEnum.Goal,
                EvaluationType = EvaluationTypeEnum.Quantitative,
                LeadAgencyId = agency.Id,
                Section = "Giai đoạn 2025-2027",
                DueDate = new DateTime(2027, 12, 31)
            });

            db.GoalTaskItems.Add(new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                DocumentId = doc2629Id,
                Document = doc2629,
                Code = $"NV-{i:D2}",
                Title = $"Nhiệm vụ số {i}",
                ItemType = ItemTypeEnum.Task,
                EvaluationType = EvaluationTypeEnum.Qualitative,
                LeadAgencyId = agency.Id,
                DueDate = new DateTime(2026, 12, 31)
            });
        }
        await db.SaveChangesAsync();

        var planningService = new PlanningService(db);

        // 1. Load only goals tab, page 1, pageSize 2
        var goalsP1 = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin", tab: "goals", page: 1, pageSize: 2);
        Assert.Equal("goals", goalsP1.ActiveTab);
        Assert.Equal(5, goalsP1.TotalGoals);
        Assert.Equal(5, goalsP1.TotalTasks);
        Assert.Equal(5, goalsP1.TotalCount);
        Assert.Equal(3, goalsP1.TotalPages);
        Assert.Equal(2, goalsP1.Goals.Count);
        Assert.Empty(goalsP1.Tasks);
        Assert.Equal(1, goalsP1.Goals[0].SequenceNumber);
        Assert.Equal(2, goalsP1.Goals[1].SequenceNumber);

        // 2. Load goals tab, page 2, pageSize 2
        var goalsP2 = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin", tab: "goals", page: 2, pageSize: 2);
        Assert.Equal(2, goalsP2.Goals.Count);
        Assert.Equal(3, goalsP2.Goals[0].SequenceNumber);
        Assert.Equal(4, goalsP2.Goals[1].SequenceNumber);

        // 3. Load only tasks tab, page 1, pageSize 3
        var tasksP1 = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin", tab: "tasks", page: 1, pageSize: 3);
        Assert.Equal("tasks", tasksP1.ActiveTab);
        Assert.Equal(5, tasksP1.TotalGoals);
        Assert.Equal(5, tasksP1.TotalTasks);
        Assert.Equal(5, tasksP1.TotalCount);
        Assert.Equal(2, tasksP1.TotalPages);
        Assert.Equal(3, tasksP1.Tasks.Count);
        Assert.Empty(tasksP1.Goals);
        Assert.Equal(1, tasksP1.Tasks[0].SequenceNumber);
        Assert.Equal(2, tasksP1.Tasks[1].SequenceNumber);
        Assert.Equal(3, tasksP1.Tasks[2].SequenceNumber);

        // 4. Fetch all
        var allReport = await planningService.GetAgencyProgressReportAsync(agency.Id, 2026, agency.Id, "Admin", fetchAll: true);
        Assert.Equal(5, allReport.Goals.Count);
        Assert.Equal(5, allReport.Tasks.Count);
    }
    [Fact]
    public async Task Report_OnlyIncludesActualLeadAssignments_NotDisplayTextOrCoordination()
    {
        await using var db = CreateInMemoryDb("ReportScope_" + Guid.NewGuid());
        var doc = new Document { Id = Guid.Parse("26290000-0000-0000-0000-000000002629"), DocumentNumber = "2629/QĐ-TTg", Name = "Test" };
        var a = new Agency { Id = Guid.NewGuid(), Code = "A", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };
        var b = new Agency { Id = Guid.NewGuid(), Code = "B", Name = "Bộ khác", Type = AgencyTypeEnum.Ministry };
        GoalTaskItem Item(string code) => new() { Id = Guid.NewGuid(), DocumentId = doc.Id, Code = code, Title = code, ItemType = ItemTypeEnum.Task, LeadAgencyId = b.Id };
        var direct = Item("NV-01"); direct.LeadAgencyId = a.Id;
        var assigned = Item("NV-02"); assigned.AgencyExecutions.Add(new AgencyTaskExecution { AgencyId = a.Id, IsLeadAssignment = true });
        var display = Item("NV-03"); display.LeadAgencyDisplayText = "Đơn vị thuộc " + a.Name;
        var coordination = Item("NV-04"); coordination.AgencyExecutions.Add(new AgencyTaskExecution { AgencyId = a.Id, IsCoordinatingAssignment = true });
        db.Documents.Add(doc); db.Agencies.AddRange(a,b); db.GoalTaskItems.AddRange(direct,assigned,display,coordination); await db.SaveChangesAsync();
        var service = new PlanningService(db);
        foreach (var fetchAll in new[] { false, true })
        {
            var report = await service.GetAgencyProgressReportAsync(a.Id,2026,a.Id,"Admin",tab:"tasks",fetchAll:fetchAll);
            Assert.Equal(2,report.TotalTasks);
            Assert.Equal(new[] { "NV-01", "NV-02" },report.Tasks.Select(t=>t.Code));
        }
    }

}
