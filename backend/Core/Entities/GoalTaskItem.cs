using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 2 & 3: Goals & Tasks under Decision 1266
    /// Supports Sub-tasks hierarchy, General vs Specific scope, and Date ranges.
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

        /// <summary>
        /// ID Mục tiêu / Nhiệm vụ cha (Nếu là Nhiệm vụ con / Sub-task)
        /// </summary>
        public Guid? ParentId { get; set; }
        public GoalTaskItem? ParentItem { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? Group { get; set; }
        public bool IsOngoing { get; set; } = false;

        /// <summary>
        /// Phân loại phạm vi: true = Nhiệm vụ chung, false = Nhiệm vụ riêng (Mặc định = false)
        /// </summary>
        public bool IsGeneralTask { get; set; } = false;

        /// <summary>
        /// Ngày bắt đầu thực hiện
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày hoàn thành / Hạn chót
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Đơn vị chủ trì
        /// </summary>
        public Guid LeadAgencyId { get; set; }
        public Agency? LeadAgency { get; set; }

        /// <summary>
        /// Đơn vị trực thuộc được giao nhiệm vụ (Optional)
        /// </summary>
        public Guid? AssignedAgencyId { get; set; }
        public Agency? AssignedAgency { get; set; }

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
        /// Dynamic PostgreSQL JSONB column storing custom baseline milestone overrides.
        /// </summary>
        public Dictionary<string, string> CustomBaseline { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Multi-Deliverables Checklist stored in PostgreSQL JSONB
        /// </summary>
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();

        /// <summary>
        /// Agency-isolated Multi-Deliverables Checklist for General Tasks (Key = agencyId string, Value = List<TaskDeliverable>)
        /// </summary>
        public Dictionary<string, List<TaskDeliverable>> AgencyDeliverables { get; set; } = new Dictionary<string, List<TaskDeliverable>>();

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

        /// <summary>
        /// Computed 6-status execution evaluation enum
        /// </summary>
        [NotMapped]
        public ExecutionStatusEnum CalculatedStatus { get; set; } = ExecutionStatusEnum.NotStarted;

        // Navigation Collections
        public ICollection<GoalTaskItem> SubItems { get; set; } = new List<GoalTaskItem>();
        public ICollection<TargetBaseline> Baselines { get; set; } = new List<TargetBaseline>();
        public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
        public ICollection<AgencyTaskExecution> AgencyExecutions { get; set; } = new List<AgencyTaskExecution>();
    }

    public class TaskDeliverable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string CurrentStatus { get; set; } = "NotStarted"; // NotStarted, Drafting, Reviewing, Submitted, Completed
        public string? DocumentNumber { get; set; }
        public DateTime? PromulgationDate { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
    }
}
