using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Xunit;
namespace Cdsqg.Tests;
public class ExecutionStatusTests
{
    [Theory]
    [InlineData(ItemTypeEnum.Goal, EvaluationTypeEnum.Quantitative)]
    [InlineData(ItemTypeEnum.Task, EvaluationTypeEnum.Qualitative)]
    public void UnreportedOverdueItemRemainsNotStarted(ItemTypeEnum type, EvaluationTypeEnum evaluation)
    {
        var item = new GoalTaskItem { ItemType = type, EvaluationType = evaluation, DueDate = new(2026, 1, 1) };
        var result = ProgressCalculator.Evaluate(item, evaluatedAt: new DateTime(2026, 10, 22));
        Assert.Equal(ExecutionStatusEnum.NotStarted, result.Status);
        Assert.Equal("Overdue", result.DeadlineStatus);
        var counts = new ExecutionStatusCounts(); counts.Add(result);
        Assert.Equal(1, counts.NotStarted); Assert.Equal(1, counts.Overdue);
        Assert.Equal(1, counts.States.Values.Sum()); Assert.Equal(3, counts.States.Count);
    }
    [Theory]
    [InlineData(0, ExecutionStatusEnum.NotStarted)]
    [InlineData(25, ExecutionStatusEnum.InProgress)]
    [InlineData(100, ExecutionStatusEnum.Completed)]
    public void DeadlineDoesNotChangeExecutionState(decimal actual, ExecutionStatusEnum expected)
    {
        var agency = Guid.NewGuid();
        var item = new GoalTaskItem { LeadAgencyId = agency, DueDate = new(2026, 10, 25) };
        var report = new ProgressLog { AgencyId = agency, PeriodYear = 2026, QuantitativeValue = actual, ApprovalStatus = ApprovalStatusEnum.Approved, LogDate = new(2026,10,22) };
        var before = ProgressCalculator.Evaluate(item, report, evaluatedAt: new DateTime(2026,10,22));
        var after = ProgressCalculator.Evaluate(item, report, evaluatedAt: new DateTime(2026,11,22));
        Assert.Equal(expected, before.Status); Assert.Equal(expected, after.Status);
        Assert.Equal(expected == ExecutionStatusEnum.Completed ? "CompletedOnTime" : "ExpiringSoon", before.DeadlineStatus);
        Assert.Equal(expected == ExecutionStatusEnum.Completed ? "CompletedOnTime" : "Overdue", after.DeadlineStatus);
    }
    [Theory]
    [InlineData("InProgressOnTime", "InProgress")]
    [InlineData("InProgressOverdue", "InProgress")]
    [InlineData("ExpiringSoon", "InProgress")]
    [InlineData("CompletedOnTime", "Completed")]
    [InlineData("CompletedOverdue", "Completed")]
    public void CanonicalFiltersReadLegacyStates(string legacy, string state)
    {
        var items = new List<PlanningGridItemDto> { new() { Code = "A", CalculatedStatus = legacy }, new() { Code = "B", CalculatedStatus = "NotStarted" } };
        var filtered = DocumentItemQuery.Apply(items, new() { SelectedStatuses = new() { state } });
        Assert.Equal("A", Assert.Single(filtered).Code);
    }
}
