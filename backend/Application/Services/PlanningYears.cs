using Cdsqg.Core.Entities;
namespace Cdsqg.Application.Services;

public static class PlanningYears
{
    public static void ValidateTargets(GoalTaskItem item, IEnumerable<int> years)
    {
        if (years.Any(year => !Contains(item, year)))
            throw new InvalidOperationException("Chỉ được nhập kế hoạch trong thời gian thực hiện của mục tiêu/nhiệm vụ.");
    }

    public static bool Contains(GoalTaskItem item, int year) =>
        year >= (item.StartDate?.Year ?? item.Document?.StartYear ?? 1)
        && year <= (item.DueDate?.Year ?? item.Document?.EndYear ?? 9999);
}
