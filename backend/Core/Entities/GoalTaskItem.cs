using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 2 & 3: Level 1A (Goals) & Level 1B (Tasks) linked to Level 0 Document
    /// Includes PostgreSQL JSONB CustomBaseline column for flexible milestone overrides.
    /// </summary>
    public class GoalTaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }
        public Document? Document { get; set; }

        /// <summary>
        /// Phân loại Cấp: Goal (Level 1A: Mục tiêu) hoặc Task (Level 1B: Nhiệm vụ)
        /// </summary>
        public ItemTypeEnum ItemType { get; set; } = ItemTypeEnum.Task;

        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Đơn vị chủ trì
        /// </summary>
        public Guid LeadAgencyId { get; set; }
        public Agency? LeadAgency { get; set; }

        /// <summary>
        /// Mảng UUID các Cơ quan phối hợp (Mapped to PostgreSQL JSONB column)
        /// </summary>
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Đơn vị tính
        /// </summary>
        public Guid? UnitId { get; set; }
        public UnitDictionary? Unit { get; set; }

        /// <summary>
        /// Loại đánh giá: Quantitative (Định lượng số) hoặc Qualitative (Định tính văn bản)
        /// </summary>
        public EvaluationTypeEnum EvaluationType { get; set; } = EvaluationTypeEnum.Quantitative;

        /// <summary>
        /// Động cơ tính toán: Cumulative (Cộng dồn) hoặc LatestValue (Ghi đè giá trị mới nhất)
        /// </summary>
        public CalculationMethodEnum CalculationMethod { get; set; } = CalculationMethodEnum.LatestValue;

        /// <summary>
        /// BRD Module 3 CRUCIAL REQUIREMENT:
        /// Dynamic PostgreSQL JSONB column storing custom baseline milestone overrides.
        /// Example payload: {"Q1_2026": 10.0, "Q2_2026": 40.0, "Q3_2026": 70.0, "Q4_2026": 100.0}
        /// Prevents schema changes while enabling infinite milestone flexibility per quarter/month.
        /// </summary>
        public Dictionary<string, string> CustomBaseline { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Advanced dynamic KPI metadata stored in PostgreSQL JSONB
        /// </summary>
        public Dictionary<string, object> DynamicKPIs { get; set; } = new Dictionary<string, object>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Latest recorded progress values (Computed dynamically for API responses)
        /// </summary>
        [NotMapped]
        public decimal? LatestProgressValue { get; set; }

        [NotMapped]
        public string? LatestProgressStatus { get; set; }

        [NotMapped]
        public DateTime? LastUpdated { get; set; }

        // Navigation Collections
        public ICollection<TargetBaseline> Baselines { get; set; } = new List<TargetBaseline>();
        public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    }
}
