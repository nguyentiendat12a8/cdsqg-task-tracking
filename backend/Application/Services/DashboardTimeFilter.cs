using Cdsqg.Core.Entities;

namespace Cdsqg.Application.Services;

/// <summary>Shared dashboard execution overlap and annual report snapshot window.</summary>
public sealed record DashboardTimeFilter(int? ReportFromYear, int? ReportToYear, DateTime? ExecutionFrom, DateTime? ExecutionTo)
{
    public string? ValidationError()
    {
        if (ReportFromYear is < 1900 or > 9999 || ReportToYear is < 1900 or > 9999)
            return "Năm báo cáo phải từ 1900 đến 9999.";
        if (ReportFromYear > ReportToYear) return "Năm báo cáo từ không được lớn hơn năm đến.";
        if (ExecutionFrom?.Date > ExecutionTo?.Date) return "Ngày hoàn thành từ không được lớn hơn ngày đến.";
        return null;
    }

    public IQueryable<GoalTaskItem> ApplyExecutionWindow(IQueryable<GoalTaskItem> query)
    {
        if (ExecutionFrom.HasValue)
        {
            var from = ExecutionFrom.Value.Date;
            query = query.Where(i => i.DueDate.HasValue && i.DueDate.Value >= from);
        }
        if (ExecutionTo.HasValue)
        {
            var toExclusive = ExecutionTo.Value.Date.AddDays(1);
            query = query.Where(i => i.DueDate.HasValue && i.DueDate.Value < toExclusive);
        }
        return query;
    }

    public void RestrictReports(IEnumerable<GoalTaskItem> items)
    {
        if (!ReportFromYear.HasValue && !ReportToYear.HasValue) return;
        // Only mutate no-tracking snapshots. Reports outside the range cannot affect totals/statuses.
        foreach (var item in items)
        {
            item.Deliverables = ProgressCalculator.AnnualDeliverables(item, null);
            item.ProgressLogs = item.ProgressLogs.Where(log =>
                (!ReportFromYear.HasValue || log.PeriodYear >= ReportFromYear.Value)
                && (!ReportToYear.HasValue || log.PeriodYear <= ReportToYear.Value)).ToList();
        }
    }
}
