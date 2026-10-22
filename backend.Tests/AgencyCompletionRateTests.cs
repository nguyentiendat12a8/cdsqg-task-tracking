using Cdsqg.Api.Controllers;
using Xunit;
namespace Cdsqg.Tests;
public class AgencyCompletionRateTests
{
    [Fact]
    public void EmptyAssignmentsHaveNoRate()
    {
        var dto = new AgencyStatusSummaryDto();
        Assert.Null(dto.GoalCompletionRate);
        Assert.Null(dto.TaskCompletionRate);
        Assert.Null(dto.CompletionRateOverall);
    }
    [Fact]
    public void KeepsGoalAndTaskCompletionDenominatorsSeparate()
    {
        var dto = new AgencyStatusSummaryDto { TotalGoals = 3, TotalTasks = 4, TotalItems = 7,
            Goals = new() { Completed = 2 }, Tasks = new() { Completed = 1 } };
        Assert.Equal(66.67m, dto.GoalCompletionRate);
        Assert.Equal(25m, dto.TaskCompletionRate);
        Assert.Equal(42.86m, dto.CompletionRateOverall);
    }
}
