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
    public void IncludesBothCompletionStatusesAndKeepsGoalAndTaskDenominatorsSeparate()
    {
        var dto = new AgencyStatusSummaryDto { TotalGoals = 3, GoalCompletedOnTime = 1, GoalCompletedOverdue = 1, TotalTasks = 4, TaskCompletedOverdue = 1, TotalItems = 7, CompletedOnTime = 1, CompletedOverdue = 2 };
        Assert.Equal(66.67m, dto.GoalCompletionRate);
        Assert.Equal(25m, dto.TaskCompletionRate);
        Assert.Equal(42.86m, dto.CompletionRateOverall);
    }
}
