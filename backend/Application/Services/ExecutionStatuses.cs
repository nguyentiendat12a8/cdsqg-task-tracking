using Cdsqg.Core.Enums;
namespace Cdsqg.Application.Services;

public static class ExecutionStatuses
{
    public static IReadOnlyList<ExecutionStatusEnum> All { get; } = Array.AsReadOnly(new[] { ExecutionStatusEnum.NotStarted, ExecutionStatusEnum.InProgress, ExecutionStatusEnum.Completed });
    public static ExecutionStatusEnum Normalize(ExecutionStatusEnum status) => status switch
    {
        ExecutionStatusEnum.InProgressOnTime or ExecutionStatusEnum.InProgressOverdue or ExecutionStatusEnum.ExpiringSoon => ExecutionStatusEnum.InProgress,
        ExecutionStatusEnum.CompletedOnTime or ExecutionStatusEnum.CompletedOverdue => ExecutionStatusEnum.Completed,
        _ => status
    };
    public static string Normalize(string status) => Enum.TryParse<ExecutionStatusEnum>(status, true, out var parsed) ? Normalize(parsed).ToString() : status;
}

public class ExecutionStatusCounts
{
    public static decimal? CompletionRate(int completed, int total) => total > 0 ? Math.Round(completed * 100m / total, 2) : null;
    public int NotStarted { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    // Deadline warnings overlap execution states; never add them to the state total.
    public int ExpiringSoon { get; set; }
    public int Overdue { get; set; }
    public int OnTimeReports { get; set; }
    public Dictionary<string, int> States => new() { ["notStarted"] = NotStarted, ["inProgress"] = InProgress, ["completed"] = Completed };
    public int ReportedCount => InProgress + Completed;
    public int WarningCount => ExpiringSoon + Overdue;
    public void Add(ProgressCalculator.Result result)
    {
        switch (result.Status)
        {
            case ExecutionStatusEnum.NotStarted: NotStarted++; break;
            case ExecutionStatusEnum.InProgress: InProgress++; break;
            case ExecutionStatusEnum.Completed: Completed++; break;
        }
        if (result.DeadlineStatus == "ExpiringSoon") ExpiringSoon++;
        if (result.DeadlineStatus == "Overdue") Overdue++;
        if (result.Status != ExecutionStatusEnum.NotStarted && result.DeadlineStatus is "OnTime" or "CompletedOnTime") OnTimeReports++;
    }
    public static ExecutionStatusCounts Sum(IEnumerable<ExecutionStatusCounts> counts)
    {
        var list = counts.ToList();
        return new() { NotStarted = list.Sum(c => c.NotStarted), InProgress = list.Sum(c => c.InProgress), Completed = list.Sum(c => c.Completed),
            ExpiringSoon = list.Sum(c => c.ExpiringSoon), Overdue = list.Sum(c => c.Overdue), OnTimeReports = list.Sum(c => c.OnTimeReports) };
    }
}
