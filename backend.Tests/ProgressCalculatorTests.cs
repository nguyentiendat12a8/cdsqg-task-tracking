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
        Assert.Equal(complete, result.Status == ExecutionStatusEnum.CompletedOnTime);
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
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, result.Status);
    }

    [Fact]
    public void PendingReportNeverChangesOfficialProgress()
    {
        var item = Item();
        item.ProgressLogs.Add(Log(item, 2026, 25));
        item.ProgressLogs.Add(Log(item, 2026, 100, 8, ApprovalStatusEnum.Pending));
        var result = ProgressCalculator.Evaluate(item, evaluatedAt: Now);
        Assert.Equal(25m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, result.Status);
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
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, result.Status);
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
        Assert.Equal(expected == 100, result.Status == ExecutionStatusEnum.CompletedOnTime);
    }

    [Theory]
    [InlineData(0, false, ExecutionStatusEnum.InProgressOverdue)]
    [InlineData(25, false, ExecutionStatusEnum.InProgressOverdue)]
    [InlineData(100, false, ExecutionStatusEnum.CompletedOverdue)]
    [InlineData(100, true, ExecutionStatusEnum.CompletedOnTime)]
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
        Assert.Equal(ExecutionStatusEnum.ExpiringSoon, ProgressCalculator.Evaluate(item, Log(item, 2026, 1), evaluatedAt: Now).Status);
    }

    [Fact]
    public void RoundingMustNotDeclareCompletionEarly()
    {
        var item = Item();
        var result = ProgressCalculator.Evaluate(item, Log(item, 2026, 99.999m), evaluatedAt: Now);
        Assert.Equal(99.99m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, result.Status);
    }

    [Fact]
    public void EmptyProductSnapshotCannotBypassConfiguredProducts()
    {
        var item = Item(); item.EvaluationType = EvaluationTypeEnum.Qualitative;
        item.Deliverables.Add(new() { CurrentStatus = "Drafting" });
        var report = Log(item, 2026, 0); report.QualitativeStatus = TextStatusEnum.Completed; report.Deliverables = new();
        var result = ProgressCalculator.Evaluate(item, report, evaluatedAt: Now);
        Assert.Equal(25m, result.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, result.Status);
    }

    [Fact]
    public void GeneralTaskNeedsEveryEligibleAgencyToComplete()
    {
        var item = Item(); item.IsGeneralTask = true;
        var first = new Agency { Id = item.LeadAgencyId, Type = AgencyTypeEnum.Ministry, Code = "FIRST" };
        var second = new Agency { Type = AgencyTypeEnum.Province, Code = "SECOND" };
        item.ProgressLogs.Add(Log(item, 2026, 100));
        var partial = ProgressCalculator.EvaluateOverall(item, new[] { first, second }, Now);
        Assert.Equal(50m, partial.Percentage);
        Assert.Equal(ExecutionStatusEnum.InProgressOnTime, partial.Status);
        var secondLog = Log(item, 2026, 100); secondLog.AgencyId = second.Id;
        item.ProgressLogs.Add(secondLog);
        Assert.Equal(ExecutionStatusEnum.CompletedOnTime, ProgressCalculator.EvaluateOverall(item, new[] { first, second }, Now).Status);
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
}
