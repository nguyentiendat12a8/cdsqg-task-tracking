using System;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 3: Target Baseline Entity (Mốc chỉ tiêu định kỳ)
    /// </summary>
    public class TargetBaseline
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GoalTaskId { get; set; }
        public GoalTaskItem? GoalTaskItem { get; set; }

        public int Year { get; set; }

        public decimal? TargetQuantity { get; set; }
        // Kế hoạch Admin thiết lập; TargetQuantity là bản mặc định được giữ nguyên.
        public decimal? ConfiguredTargetQuantity { get; set; }
        public TextStatusEnum? TargetQualitativeStatus { get; set; }

        public bool IsCustomOverride { get; set; } = false;
        public string? OverrideNote { get; set; }
    }
}
