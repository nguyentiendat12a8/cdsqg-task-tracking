using System.Globalization;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.Services;

/// <summary>Default annual shares and cumulative milestones from a goal's final result.</summary>
public static class GoalResultPlan
{
    public static void Validate(GoalTaskItem item)
    {
        if (!item.ResultTarget.HasValue) return;
        if (item.ItemType != ItemTypeEnum.Goal || item.EvaluationType != EvaluationTypeEnum.Quantitative)
            throw new InvalidOperationException("Kết quả cần đạt chỉ áp dụng cho mục tiêu định lượng.");
        if (item.ResultTarget <= 0 || item.ResultTarget > 1_000_000_000_000m || decimal.Round(item.ResultTarget.Value, 6) != item.ResultTarget.Value)
            throw new InvalidOperationException("Kết quả cần đạt phải lớn hơn 0, không quá 1.000.000.000.000 và tối đa 6 chữ số thập phân.");
        if (!item.StartDate.HasValue || !item.DueDate.HasValue || item.StartDate > item.DueDate)
            throw new InvalidOperationException("Nhập thời gian bắt đầu và kết thúc hợp lệ để chia kế hoạch mục tiêu.");
    }

    public static Dictionary<int, decimal> AnnualAllocations(GoalTaskItem item)
    {
        var shares = new Dictionary<int, decimal>();
        if (!item.ResultTarget.HasValue) return shares;
        Validate(item);
        var start = item.StartDate!.Value.Year;
        var count = item.DueDate!.Value.Year - start + 1;
        // Preserve input precision; whole-number results are allocated as whole numbers.
        var text = item.ResultTarget.Value.ToString("0.######", CultureInfo.InvariantCulture);
        var scale = text.Contains('.') ? text.Length - text.IndexOf('.') - 1 : 0;
        decimal factor = 1;
        for (var i = 0; i < scale; i++) factor *= 10;
        var units = item.ResultTarget.Value * factor;
        var whole = decimal.Floor(units / count);
        var remainder = units - whole * count;
        for (var i = 0; i < count; i++)
            shares[start + i] = (whole + (i >= count - remainder ? 1 : 0)) / factor;
        return shares;
    }

    public static decimal? CumulativeTarget(GoalTaskItem item, int year)
    {
        var shares = AnnualAllocations(item);
        if (shares.Count == 0 || year < shares.Keys.Min()) return null;
        return shares.Where(p => p.Key <= year).Sum(p => p.Value);
    }
}
