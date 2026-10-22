using System.Text.Json;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Xunit;

namespace Cdsqg.Tests;

public class GoalResultPlanTests
{
    public static GoalTaskItem Goal(decimal result, int start = 2026, int end = 2028) => new()
    {
        ItemType = ItemTypeEnum.Goal, ResultTarget = result,
        StartDate = new(start, 12, 1), DueDate = new(end, 12, 31)
    };

    [Theory]
    [InlineData(10, 3, 3, 4)]
    [InlineData(11, 3, 4, 4)]
    [InlineData(12, 4, 4, 4)]
    [InlineData(2, 0, 1, 1)]
    [InlineData(1, 0, 0, 1)]
    public void IntegerRemaindersGoToLastYears(decimal result, decimal a, decimal b, decimal c)
    {
        var item = Goal(result);
        Assert.Equal(new[] { a, b, c }, GoalResultPlan.AnnualAllocations(item).Values);
        Assert.Equal(result, GoalResultPlan.AnnualAllocations(item).Values.Sum());
        Assert.Equal(a, ProgressCalculator.ResolveBaseline(item, 2026).Target);
        Assert.Equal(a + b, ProgressCalculator.ResolveBaseline(item, 2027).Target);
        Assert.Equal(result, ProgressCalculator.ResolveBaseline(item, 2028).Target);
    }

    [Fact]
    public void SmallTotalAllowsZeroYearsWithoutFallingBackTo100Percent()
    {
        var item = Goal(2, 2026, 2030);
        Assert.Equal(new decimal[] { 0, 0, 0, 1, 1 }, GoalResultPlan.AnnualAllocations(item).Values);
        Assert.Equal(0m, ProgressCalculator.ResolveBaseline(item, 2026).Target);
        Assert.Null(ProgressCalculator.ResolveBaseline(item, 2025).Target);
        Assert.Equal(2m, ProgressCalculator.ResolveBaseline(item, 2031).Target);
    }

    [Theory]
    [InlineData("10.5")]
    [InlineData("0.5")]
    [InlineData("33.33")]
    [InlineData("0.000001")]
    public void DecimalResultsPreserveExactTotalAndPrecision(string input)
    {
        var result = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        var shares = GoalResultPlan.AnnualAllocations(Goal(result));
        Assert.Equal(result, shares.Values.Sum());
        Assert.All(shares.Values, value => Assert.True(value >= 0));
        Assert.True(shares.Values.Max() - shares.Values.Min() <= (input.Split('.')[1].Length == 6 ? 0.000001m : input.Split('.')[1].Length == 2 ? 0.01m : 0.1m));
    }

    [Fact]
    public void ExplicitAnnualTargetsAndOverridesRemainAuthoritative()
    {
        var item = Goal(10);
        item.Baselines.Add(new() { Year = 2027, TargetQuantity = 8 });
        Assert.Equal(8m, ProgressCalculator.ResolveBaseline(item, 2027).Target);
        item.CustomBaseline["2027"] = "9";
        Assert.Equal((9m, true), ProgressCalculator.ResolveBaseline(item, 2027));
        item.ResultTarget = null;
        Assert.Equal(100m, ProgressCalculator.ResolveBaseline(item, 2026).Target);
    }

    [Fact]
    public void SinglePartialYearReceivesEntireTargetAndChangedDatesRecalculateDefaults()
    {
        var item = Goal(10, 2025, 2025);
        Assert.Equal(10m, GoalResultPlan.AnnualAllocations(item)[2025]);
        item.DueDate = new(2027, 12, 31);
        Assert.Equal(new decimal[] { 3, 3, 4 }, GoalResultPlan.AnnualAllocations(item).Values);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.0000001")]
    [InlineData("1000000000001")]
    public void InvalidResultsAreRejected(string value) => Assert.Throws<InvalidOperationException>(() =>
        GoalResultPlan.Validate(Goal(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture))));

    [Fact]
    public void MissingOrReversedDatesAndTasksAreRejected()
    {
        var item = Goal(10); item.StartDate = null;
        Assert.Throws<InvalidOperationException>(() => GoalResultPlan.Validate(item));
        item.StartDate = new(2030, 1, 1);
        Assert.Throws<InvalidOperationException>(() => GoalResultPlan.Validate(item));
        item.StartDate = new(2026, 1, 1); item.ItemType = ItemTypeEnum.Task;
        Assert.Throws<InvalidOperationException>(() => GoalResultPlan.Validate(item));
    }

    [Fact]
    public void UpdateDistinguishesOmittedResultFromExplicitClear()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.False(JsonSerializer.Deserialize<UpdateGoalTaskItemRequestDto>("{}", options)!.ResultTargetSpecified);
        Assert.True(JsonSerializer.Deserialize<UpdateGoalTaskItemRequestDto>("{\"resultTarget\":null}", options)!.ResultTargetSpecified);
    }
}
