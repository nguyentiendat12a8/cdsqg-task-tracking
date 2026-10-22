using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Xunit;

namespace Cdsqg.Tests;

public class ProgressCalculatorTests
{
    private static readonly DateTime Now = new(2026, 10, 8);
    private static GoalTaskItem Item() => new() { LeadAgencyId = Guid.NewGuid(), DueDate = new(2026, 12, 31), CalculationMethod = CalculationMethodEnum.LatestValue };
    private static ProgressLog Log(GoalTaskItem item, int year, decimal value, int day = 1, ApprovalStatusEnum approval = ApprovalStatusEnum.Approved)
        => new() { GoalTaskId = item.Id, AgencyId = item.LeadAgencyId, PeriodYear = year, QuantitativeValue = value, LogDate = new(2026, 10, day), ApprovalStatus = approval };

    [Theory]
    [InlineData(49, 98, false)]
    [InlineData(50, 100, true)]
    [InlineData(60, 120, true)]
    public void CustomBaselineControlsBothPercentageAndCompletion(decimal value, decimal percentage, bool complete)
    {
        var item = Item();
        item.CustomBaseline = new() { ["2026"] = "50", ["2030"] = "1000" };
        item.Baselines.Add(new() { Year = 2026, TargetQuantity = 200 });
        var result = ProgressCalculator.Evaluate(item, Log(item, 2026, value), evaluatedAt: Now);
        Assert.Equal(50m, result.Target);
        Assert.Equal(percentage, result.Percentage);
        Assert.Equal(complete, result.Status == ExecutionStatusEnum.Completed);
        Assert.True(result.IsCustomBaseline);
    }

    [Fact]
    public void ReportUsesItsOwnYearAndAnnualTarget()
    {
        var item = Item();
        item.Baselines.Add(new() { Year = 2026, TargetQuantity = 20 });
        item.Baselines.Add(new() { Year = 2030, TargetQuantity = 500 });
        Assert.Equal(100m, ProgressCalculator.Evaluate(item, Log(item, 2026, 20), evaluatedAt: Now).Percentage);
    }

    [Fact]
    public void AnnualTotalDoesNotAddPriorYearsOrOtherAgencies()
    {
        var item = Item(); item.CalculationMethod = CalculationMethodEnum.Cumulative;
        item.CustomBaseline["2026"] = "35";
        item.ProgressLogs.Add(Log(item, 2025, 10));
        item.ProgressLogs.Add(Log(item, 2025, 15, 2));
        item.ProgressLogs.Add(Log(item, 2025, 800, 3, ApprovalStatusEnum.Pending));
        item.ProgressLogs.Add(Log(item, 2025, 900, 3, ApprovalStatusEnum.Rejected));
        item.ProgressLogs.Add(new() { AgencyId = Guid.NewGuid(), PeriodYear = 2025, QuantitativeValue = 1000, ApprovalStatus = ApprovalStatusEnum.Approved, LogDate = Now });
        var current = Log(item, 2026, 20, 8);
        item.ProgressLogs.Add(current);
        item.ProgressLogs.Add(Log(item, 2027, 500, 8));
        var result = ProgressCalculator.Evaluate(item, current, evaluatedAt: Now);
        Assert.Equal(20m, result.ActualValue);
        Assert.Equal(57.14m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
    }

    [Fact]
    public void PendingReportNeverChangesOfficialProgress()
    {
        var item = Item();
        item.ProgressLogs.Add(Log(item, 2026, 25));
        item.ProgressLogs.Add(Log(item, 2026, 100, 8, ApprovalStatusEnum.Pending));
        var result = ProgressCalculator.Evaluate(item, evaluatedAt: Now);
        Assert.Equal(25m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
    }

    [Fact]
    public void GeneralAgenciesNeverShareReports()
    {
        var item = Item(); item.IsGeneralTask = true;
        item.ProgressLogs.Add(Log(item, 2026, 100));
        var result = ProgressCalculator.Evaluate(item, agencyId: Guid.NewGuid(), evaluatedAt: Now);
        Assert.Null(result.ActualValue);
        Assert.Equal(ExecutionStatusEnum.NotStarted, result.Status);
    }

    [Fact]
    public void QuantityWithoutBaselineHasNoInventedPercentageOrCompletion()
    {
        var item = Item(); item.Unit = new() { Name = "Văn bản", Code = "DOC" }; item.UnitId = item.Unit.Id;
        var result = ProgressCalculator.Evaluate(item, Log(item, 2026, 150), evaluatedAt: Now);
        Assert.Null(result.Target); Assert.Null(result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
    }

    [Theory]
    [InlineData("NotStarted", 0)]
    [InlineData("Drafting", 25)]
    [InlineData("Reviewing", 60)]
    [InlineData("Submitted", 85)]
    [InlineData("Completed", 100)]
    public void DeliverableWeightsAreShared(string status, decimal expected)
    {
        var item = Item(); item.EvaluationType = EvaluationTypeEnum.Qualitative;
        var log = Log(item, 2026, 0); log.QualitativeStatus = TextStatusEnum.Completed;
        log.Deliverables = new() { new() { CurrentStatus = status } };
        var result = ProgressCalculator.Evaluate(item, log, evaluatedAt: Now);
        Assert.Equal(expected, result.Percentage);
        Assert.Equal(expected == 100, result.Status == ExecutionStatusEnum.Completed);
    }

    [Theory]
    [InlineData(0, false, ExecutionStatusEnum.NotStarted)]
    [InlineData(25, false, ExecutionStatusEnum.InProgress)]
    [InlineData(100, false, ExecutionStatusEnum.Completed)]
    [InlineData(100, true, ExecutionStatusEnum.Completed)]
    public void DeadlineAndCompletionUseTheSameEvaluation(decimal value, bool beforeDeadline, ExecutionStatusEnum expected)
    {
        var item = Item(); item.DueDate = new(2026, 10, 5);
        var log = Log(item, 2026, value, beforeDeadline ? 1 : 8);
        Assert.Equal(expected, ProgressCalculator.Evaluate(item, log, evaluatedAt: Now).Status);
    }

    [Fact]
    public void ExpiringSoonUsesThirtyDaysForEveryItem()
    {
        var item = Item(); item.DueDate = Now.AddDays(30);
        var result = ProgressCalculator.Evaluate(item, Log(item, 2026, 1), evaluatedAt: Now);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
        Assert.Equal("ExpiringSoon", result.DeadlineStatus);
    }

    [Fact]
    public void RoundingMustNotDeclareCompletionEarly()
    {
        var item = Item();
        var result = ProgressCalculator.Evaluate(item, Log(item, 2026, 99.999m), evaluatedAt: Now);
        Assert.Equal(99.99m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
    }

    [Fact]
    public void EmptyProductSnapshotCannotBypassConfiguredProducts()
    {
        var item = Item(); item.EvaluationType = EvaluationTypeEnum.Qualitative;
        item.Deliverables.Add(new() { CurrentStatus = "Drafting" });
        var report = Log(item, 2026, 0); report.QualitativeStatus = TextStatusEnum.Completed; report.Deliverables = new();
        var result = ProgressCalculator.Evaluate(item, report, evaluatedAt: Now);
        Assert.Equal(25m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, result.Status);
    }

    [Theory]
    [InlineData(ItemTypeEnum.Goal)]
    [InlineData(ItemTypeEnum.Task)]
    public void SharedItemStatusFollowsAllAssignedRecipients(ItemTypeEnum itemType)
    {
        var item = Item(); item.IsGeneralTask = true; item.ItemType = itemType;
        item.EvaluationType = itemType == ItemTypeEnum.Goal ? EvaluationTypeEnum.Quantitative : EvaluationTypeEnum.Qualitative;
        item.CustomBaseline["2026"] = "100";
        var first = new Agency { Id = item.LeadAgencyId, Code = "FIRST", Type = AgencyTypeEnum.Ministry };
        var second = new Agency { Code = "SECOND", Type = AgencyTypeEnum.Province };
        var unassigned = new Agency { Code = "UNASSIGNED", Type = AgencyTypeEnum.Ministry };
        foreach (var agency in new[] { first, second })
            item.AgencyExecutions.Add(new() { AgencyId = agency.Id, IsLeadAssignment = true, IsLeadGroupAssignment = true });
        var agencies = new[] { first, second, unassigned };
        ExecutionStatusEnum Overall() => ProgressCalculator.EvaluateOverall(item, agencies, Now, 2026).Status;
        Assert.Equal(ExecutionStatusEnum.NotStarted, Overall());
        var report = Log(item, 2026, 25, approval: ApprovalStatusEnum.Pending);
        report.QualitativeStatus = TextStatusEnum.Drafting; item.ProgressLogs.Add(report);
        Assert.Equal(ExecutionStatusEnum.NotStarted, Overall());
        report.ApprovalStatus = ApprovalStatusEnum.Approved;
        Assert.Equal(ExecutionStatusEnum.InProgress, Overall());
        report.QuantitativeValue = 100; report.QualitativeStatus = TextStatusEnum.Completed;
        Assert.Equal(ExecutionStatusEnum.InProgress, Overall()); // One completed, one not started.
        Assert.Equal(ExecutionStatusEnum.Completed, ProgressCalculator.Evaluate(item, agencyId:first.Id, year:2026, evaluatedAt:Now).Status);
        Assert.Equal(ExecutionStatusEnum.NotStarted, ProgressCalculator.Evaluate(item, agencyId:second.Id, year:2026, evaluatedAt:Now).Status);
        var remaining = Log(item, 2026, 25); remaining.AgencyId = second.Id;
        remaining.QualitativeStatus = TextStatusEnum.Drafting; item.ProgressLogs.Add(remaining);
        Assert.Equal(ExecutionStatusEnum.InProgress, Overall());
        remaining.QuantitativeValue = 100; remaining.QualitativeStatus = TextStatusEnum.Completed;
        Assert.Equal(ExecutionStatusEnum.Completed, Overall()); // Unassigned agencies do not block completion.
        remaining.ApprovalStatus = ApprovalStatusEnum.Rejected;
        Assert.Equal(ExecutionStatusEnum.InProgress, Overall());
    }

    [Fact]
    public void GeneralTaskNeedsEveryEligibleAgencyToComplete()
    {
        var item = Item(); item.IsGeneralTask = true;
        var first = new Agency { Id = item.LeadAgencyId, Type = AgencyTypeEnum.Ministry, Code = "FIRST" };
        var second = new Agency { Type = AgencyTypeEnum.Province, Code = "SECOND" };
        item.AgencyExecutions.Add(new() { AgencyId=first.Id, IsLeadAssignment=true });
        item.AgencyExecutions.Add(new() { AgencyId=second.Id, IsLeadAssignment=true });
        item.ProgressLogs.Add(Log(item, 2026, 100));
        var partial = ProgressCalculator.EvaluateOverall(item, new[] { first, second }, Now);
        Assert.Equal(50m, partial.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgress, partial.Status);
        var secondLog = Log(item, 2026, 100); secondLog.AgencyId = second.Id;
        item.ProgressLogs.Add(secondLog);
        Assert.Equal(ExecutionStatusEnum.Completed, ProgressCalculator.EvaluateOverall(item, new[] { first, second }, Now).Status);
    }

    [Fact]
    public void PriorYearCorrectionDoesNotChangeCurrentAnnualTotal()
    {
        var item = Item(); item.CalculationMethod = CalculationMethodEnum.Cumulative;
        item.CustomBaseline["2026"] = "35";
        item.ProgressLogs.Add(Log(item, 2025, 10));
        item.ProgressLogs.Add(Log(item, 2025, 15, 8));
        Assert.Equal(20m, ProgressCalculator.Evaluate(item, Log(item, 2026, 20, 2), evaluatedAt: Now).ActualValue);
    }

    [Theory]
    [InlineData(CalculationMethodEnum.Cumulative)]
    [InlineData(CalculationMethodEnum.LatestValue)]
    public void AnnualRevisionReplacesTotalAndPendingPreviewUsesSameRule(CalculationMethodEnum method)
    {
        var item = Item(); item.CalculationMethod = method;
        item.CustomBaseline["2026"] = "50";
        item.ProgressLogs.Add(Log(item, 2025, 15));
        item.ProgressLogs.Add(Log(item, 2026, 30, 2));
        var revision = Log(item, 2026, 35, 8, ApprovalStatusEnum.Pending);
        item.ProgressLogs.Add(revision);
        Assert.Equal(30m, ProgressCalculator.Evaluate(item, evaluatedAt: Now).ActualValue);
        Assert.Equal(70m, ProgressCalculator.Evaluate(item, revision, evaluatedAt: Now).Percentage);
        revision.ApprovalStatus = ApprovalStatusEnum.Approved;
        var result = ProgressCalculator.Evaluate(item, evaluatedAt: Now);
        Assert.Equal(35m, result.ActualValue);
        Assert.Equal(70m, result.Percentage);
        var execution = new AgencyTaskExecution { AgencyId = item.LeadAgencyId };
        ProgressCalculator.RefreshExecution(item, execution);
        Assert.Equal(35m, execution.LatestProgressValue);
        Assert.Equal(70m, execution.CompletionPercentage);
    }

    [Theory]
    [InlineData("Q1_2026", "10")]
    [InlineData("M1_2026", "10")]
    [InlineData("2026", "0")]
    [InlineData("2026", "-1")]
    [InlineData("2026", "1,5")]
    public void BaselineRejectsRetiredPeriodsAndInvalidTargets(string key, string value)
        => Assert.Throws<InvalidOperationException>(() => ProgressCalculator.ValidateAnnualBaselines(new() { [key] = value }));

    [Fact]
    public void GeneralSnapshotUsesSameSelectedYearForEveryAgencyWithoutCarryingPriorValues()
    {
        var first = new Agency { Type = AgencyTypeEnum.Ministry, IsActive = true, Code = "FIRST" };
        var second = new Agency { Type = AgencyTypeEnum.Province, IsActive = true, Code = "SECOND" };
        var excluded = new Agency { Type = AgencyTypeEnum.Other, IsActive = true, Code = "OTHER" };
        var item = Item(); item.IsGeneralTask = true;
        item.AgencyExecutions.Add(new() { AgencyId=first.Id, IsLeadAssignment=true });
        item.AgencyExecutions.Add(new() { AgencyId=second.Id, IsLeadAssignment=true });
        var firstLog = Log(item, 2026, 100); firstLog.AgencyId = first.Id;
        var secondLog = Log(item, 2027, 100); secondLog.AgencyId = second.Id;
        item.ProgressLogs.Add(firstLog); item.ProgressLogs.Add(secondLog);
        var agencies = new[] { first, second, excluded };
        Assert.Equal(50m, ProgressCalculator.EvaluateOverall(item, agencies, Now, 2026).Percentage);
        Assert.Equal(100m, ProgressCalculator.EvaluateOverall(item, agencies, Now, 2027).Percentage);
        Assert.Equal(100m, ProgressCalculator.EvaluateOverall(item, agencies, Now, 2028).Percentage);
    }

    [Fact]
    public void AnnualTargetAchievementDoesNotCarryButFinalAchievementDoes()
    {
        var item = Item(); item.ItemType = ItemTypeEnum.Goal; item.DueDate = new(2030,12,31);
        item.CustomBaseline["2026"] = "50"; item.CustomBaseline["2030"] = "100";
        item.ProgressLogs.Add(Log(item,2026,50));
        Assert.Null(ProgressCalculator.Evaluate(item, year:2027, evaluatedAt:Now).ActualValue);
        item.ProgressLogs.Add(Log(item,2027,100));
        Assert.Equal(100m,ProgressCalculator.Evaluate(item, year:2028, evaluatedAt:Now).Percentage);
        Assert.Equal(50m,ProgressCalculator.Evaluate(item, year:2026, evaluatedAt:Now).ActualValue);
        item.IsOngoing=true;
        Assert.Null(ProgressCalculator.Evaluate(item, year:2028, evaluatedAt:Now).ActualValue);
    }

    [Fact]
    public void ProductAndDirectTaskCompletionCarryOnlyApprovedLatestRevisions()
    {
        var item=Item(); item.ItemType=ItemTypeEnum.Task; item.EvaluationType=EvaluationTypeEnum.Qualitative;
        item.Deliverables.Add(new() {Id="p",Title="Product"});
        var completed=Log(item,2026,0);completed.Deliverables=new() {new() {Id="p",CurrentStatus="Completed"}};
        item.ProgressLogs.Add(completed);
        Assert.Equal(100m,ProgressCalculator.Evaluate(item,year:2027,evaluatedAt:Now).Percentage);
        var correction=Log(item,2026,0,2);correction.Deliverables=new() {new() {Id="p",CurrentStatus="Drafting"}};
        item.ProgressLogs.Add(correction);
        Assert.Equal(0m,ProgressCalculator.Evaluate(item,year:2027,evaluatedAt:Now).Percentage);
        item.Deliverables.Clear(); item.ProgressLogs.Clear();
        completed.Deliverables.Clear();completed.QualitativeStatus=TextStatusEnum.Completed;
        item.ProgressLogs.Add(completed);
        Assert.Equal(ExecutionStatusEnum.Completed,ProgressCalculator.Evaluate(item,year:2027,evaluatedAt:Now).Status);
        completed.ApprovalStatus=ApprovalStatusEnum.Pending;
        Assert.Equal(0m,ProgressCalculator.Evaluate(item,year:2027,evaluatedAt:Now).Percentage);
    }
}
