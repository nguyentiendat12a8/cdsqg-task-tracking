using System.Globalization;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

/// <summary>Annual business rules. Stored percentages/statuses are caches, never the source of truth.</summary>
public static class ProgressCalculator
{
    public static void ValidateAnnualBaselines(Dictionary<string, string>? baselines)
    {
        if (baselines == null) return;
        foreach (var (year, text) in baselines)
            if (year.Length != 4 || !int.TryParse(year, out var y) || y is < 1900 or > 9999
                || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) || value <= 0)
                throw new InvalidOperationException("Chỉ tiêu tùy chỉnh chỉ nhận năm (YYYY) và số lớn hơn 0.");
    }
    public sealed record Result(int Year, decimal? ActualValue, decimal? Target,
        bool IsCustomBaseline, decimal? Percentage, ExecutionStatusEnum Status, AlertStatusEnum Alert);

    public static (decimal? Target, bool IsCustom) ResolveBaseline(GoalTaskItem item, int year)
    {
        if (item.CustomBaseline.TryGetValue(year.ToString(CultureInfo.InvariantCulture), out var text)
            && decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var custom) && custom > 0)
            return (custom, true);
        var target = item.Baselines.FirstOrDefault(b => b.Year == year)?.TargetQuantity;
        if (target > 0) return (target, false);
        // Legacy records without a unit are displayed as percentages throughout the application.
        if (!item.UnitId.HasValue || item.Unit?.Name == "%" || item.Unit?.Code == "PERCENT") return (100m, false);
        return (null, false);
    }

    public static ProgressLog? LatestApproved(GoalTaskItem item, Guid? agencyId = null, int? year = null)
        => ScopedLogs(item, agencyId).Where(l => l.ApprovalStatus == ApprovalStatusEnum.Approved
                && (!year.HasValue || l.PeriodYear == year))
            .OrderByDescending(l => l.PeriodYear).ThenByDescending(l => l.LogDate).ThenByDescending(l => l.Id).FirstOrDefault();

    private static IEnumerable<ProgressLog> ScopedLogs(GoalTaskItem item, Guid? agencyId)
    {
        if (agencyId == Guid.Empty) agencyId = null;
        var id = agencyId ?? (item.IsGeneralTask ? (Guid?)null : item.AssignedAgencyId ?? item.LeadAgencyId);
        // A general task has independent reports per agency. No cross-agency accumulation.
        if (item.IsGeneralTask && !id.HasValue) return Enumerable.Empty<ProgressLog>();
        return item.ProgressLogs.Where(l => l.AgencyId == id || (!item.IsGeneralTask && l.AgencyId == null && id == item.LeadAgencyId));
    }

    public static decimal DeliverablePercentage(IReadOnlyCollection<TaskDeliverable>? deliverables)
        => deliverables is { Count: > 0 } ? Math.Round(deliverables.Average(d => d.CurrentStatus?.ToLowerInvariant() switch
        {
            "completed" or "4" => 100m,
            "submitted" => 85m,
            "reviewing" or "3" => 60m,
            "drafting" or "2" => 25m,
            _ => 0m
        }), 2) : 0m;

    public static Result Evaluate(GoalTaskItem item, ProgressLog? report = null,
        List<TaskDeliverable>? deliverables = null, Guid? agencyId = null, int? year = null, DateTime? evaluatedAt = null)
    {
        var now = evaluatedAt ?? DateTime.UtcNow;
        report ??= LatestApproved(item, agencyId, year);
        var reportYear = year ?? report?.PeriodYear ?? now.Year;
        var (target, custom) = ResolveBaseline(item, reportYear);
        decimal? actual = report?.QuantitativeValue;
        // Annual input is the total achieved through the report year, including for the
        // legacy Cumulative identifier. Adding previous reports would count results twice.
        var products = deliverables is { Count: > 0 } ? deliverables
            : report?.Deliverables is { Count: > 0 } ? report.Deliverables : item.Deliverables;
        decimal? percentage = item.EvaluationType == EvaluationTypeEnum.Quantitative
            ? (actual.HasValue && target > 0 ? Math.Round(actual.Value / target.Value * 100m, 2) : null)
            : products.Count > 0 ? DeliverablePercentage(products) : report?.QualitativeStatus switch
            {
                TextStatusEnum.Completed => 100m,
                TextStatusEnum.Reviewing => 60m,
                TextStatusEnum.Drafting => 25m,
                _ => 0m
            };
        var completed = report != null && (item.EvaluationType == EvaluationTypeEnum.Quantitative
            ? target > 0 && actual >= target
            : products.Count > 0 ? products.All(d => d.CurrentStatus?.ToLowerInvariant() is "completed" or "4")
                : report.QualitativeStatus == TextStatusEnum.Completed);
        if (!completed && percentage >= 100m) percentage = 99.99m;
        var started = report != null && (actual > 0 || percentage > 0);
        var deadline = item.IsOngoing ? new DateTime(reportYear, 12, 31) : item.DueDate?.Date;
        var overdue = deadline.HasValue && now.Date > deadline.Value
            || products.Any(d => d.DueDate.HasValue && now.Date > d.DueDate.Value.Date
                && d.CurrentStatus?.ToLowerInvariant() is not ("completed" or "4"));
        var status = completed
            ? deadline.HasValue && report!.LogDate.Date > deadline.Value ? ExecutionStatusEnum.CompletedOverdue : ExecutionStatusEnum.CompletedOnTime
            : overdue ? ExecutionStatusEnum.InProgressOverdue
            : !started ? ExecutionStatusEnum.NotStarted
            : deadline.HasValue && (deadline.Value - now.Date).TotalDays <= 30 ? ExecutionStatusEnum.ExpiringSoon
            : ExecutionStatusEnum.InProgressOnTime;
        var alert = percentage >= 95 ? AlertStatusEnum.Green
            : percentage >= (item.EvaluationType == EvaluationTypeEnum.Quantitative ? 70 : 50) ? AlertStatusEnum.Yellow : AlertStatusEnum.Red;
        return new(reportYear, actual, target, custom, percentage, status, alert);
    }

    public static Result EvaluateOverall(GoalTaskItem item, IEnumerable<Agency> agencies, DateTime? evaluatedAt = null)
    {
        if (!item.IsGeneralTask) return Evaluate(item, evaluatedAt: evaluatedAt);
        var code = item.LeadAgency?.Code ?? "ALL_AGENCIES";
        var eligible = agencies.Where(a => a.IsActive && a.ParentId == null
            && !a.Code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase)
            && a.Type is AgencyTypeEnum.Ministry or AgencyTypeEnum.Province
            && (!code.StartsWith("ALL_MINISTRIES") || a.Type == AgencyTypeEnum.Ministry)
            && (!code.StartsWith("ALL_PROVINCES") || a.Type == AgencyTypeEnum.Province)).ToList();
        var year = item.ProgressLogs.Where(l => l.ApprovalStatus == ApprovalStatusEnum.Approved && eligible.Any(a => a.Id == l.AgencyId))
            .Select(l => (int?)l.PeriodYear).Max() ?? (evaluatedAt ?? DateTime.UtcNow).Year;
        var results = eligible.Select(a => Evaluate(item, agencyId: a.Id, year: year, evaluatedAt: evaluatedAt)).ToList();
        var (target, custom) = ResolveBaseline(item, year);
        if (results.Count == 0) return new(year, null, target, custom, null, ExecutionStatusEnum.NotStarted, AlertStatusEnum.Red);
        var allComplete = results.All(r => r.Status is ExecutionStatusEnum.CompletedOnTime or ExecutionStatusEnum.CompletedOverdue);
        var percentage = item.EvaluationType == EvaluationTypeEnum.Quantitative && target == null ? (decimal?)null
            : Math.Round(results.Average(r => Math.Clamp(r.Percentage ?? 0, 0m, 100m)), 2);
        if (!allComplete && percentage >= 100m) percentage = 99.99m;
        var status = allComplete ? results.Any(r => r.Status == ExecutionStatusEnum.CompletedOverdue) ? ExecutionStatusEnum.CompletedOverdue : ExecutionStatusEnum.CompletedOnTime
            : results.Any(r => r.Status == ExecutionStatusEnum.InProgressOverdue) ? ExecutionStatusEnum.InProgressOverdue
            : results.Any(r => r.Status == ExecutionStatusEnum.ExpiringSoon) ? ExecutionStatusEnum.ExpiringSoon
            : results.All(r => r.Status == ExecutionStatusEnum.NotStarted) ? ExecutionStatusEnum.NotStarted : ExecutionStatusEnum.InProgressOnTime;
        return new(year, null, target, custom, percentage, status,
            percentage >= 95 ? AlertStatusEnum.Green : percentage >= 70 ? AlertStatusEnum.Yellow : AlertStatusEnum.Red);
    }

    public static void RefreshExecution(GoalTaskItem item, AgencyTaskExecution execution)
    {
        var latest = LatestApproved(item, execution.AgencyId);
        var result = Evaluate(item, latest, latest?.Deliverables, execution.AgencyId);
        execution.LatestProgressValue = result.ActualValue;
        execution.LatestQualitativeStatus = latest?.QualitativeStatus;
        execution.CompletionPercentage = result.Percentage ?? 0m;
        execution.CalculatedStatus = result.Status;
        execution.Deliverables = latest?.Deliverables ?? new();
        execution.SummaryNotes = latest?.SummaryNotes;
        execution.AttachmentFileUrls = latest?.AttachmentFileUrls ?? new();
    }

    public static async Task RefreshCachesAsync(AppDbContext db, GoalTaskItem item)
    {
        await db.Entry(item).Collection(i => i.Baselines).LoadAsync();
        await db.Entry(item).Collection(i => i.ProgressLogs).LoadAsync();
        await db.Entry(item).Collection(i => i.AgencyExecutions).LoadAsync();
        await db.Entry(item).Reference(i => i.Unit).LoadAsync();
        foreach (var log in item.ProgressLogs)
        {
            var result = Evaluate(item, log, log.Deliverables, log.AgencyId);
            log.CalculatedProgressPercentage = result.Percentage;
            log.CalculatedAlert = result.Alert;
        }
        foreach (var execution in item.AgencyExecutions) RefreshExecution(item, execution);
    }
}
