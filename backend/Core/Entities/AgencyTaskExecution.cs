using System;
using System.Collections.Generic;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Entity tracking task execution state per Agency.
    /// - For Specific Tasks (Nhiệm vụ riêng): 1-to-1 relationship with GoalTaskItem.
    /// - For General Tasks (Nhiệm vụ chung giao ALL_AGENCIES): 1-to-N relationship (1 row per executing Agency).
    /// </summary>
    public class AgencyTaskExecution
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GoalTaskId { get; set; }
        public GoalTaskItem? GoalTaskItem { get; set; }

        public Guid AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public Guid? AssignedAgencyId { get; set; }
        public Agency? AssignedAgency { get; set; }

        public ExecutionStatusEnum CalculatedStatus { get; set; } = ExecutionStatusEnum.NotStarted;
        public ApprovalStatusEnum ApprovalStatus { get; set; } = ApprovalStatusEnum.Approved;
        public string? RejectionReason { get; set; }

        public decimal? LatestProgressValue { get; set; }
        public TextStatusEnum? LatestQualitativeStatus { get; set; }
        public decimal CompletionPercentage { get; set; } = 0m;

        /// <summary>
        /// Agency-isolated Multi-Deliverables Checklist stored in PostgreSQL JSONB
        /// </summary>
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();

        /// <summary>
        /// Agency-isolated Summary Notes / Explanation
        /// </summary>
        public string? SummaryNotes { get; set; }

        /// <summary>
        /// Agency-isolated Evidence Attachment File URLs stored in PostgreSQL JSONB
        /// </summary>
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();

        public DateTime? LastReportedAt { get; set; }
        public string? LastReportedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
