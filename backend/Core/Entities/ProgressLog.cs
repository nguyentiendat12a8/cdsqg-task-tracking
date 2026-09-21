using System;
using System.Collections.Generic;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 4 & 5: Progress Log & Evidence Attachment Entity
    /// Stores periodic progress updates, file attachment URLs (JSONB), and calculated Traffic Light status.
    /// </summary>
    public class ProgressLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GoalTaskId { get; set; }
        public GoalTaskItem? GoalTaskItem { get; set; }

        public int PeriodYear { get; set; }
        public int PeriodQuarter { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kết quả thực tế đạt được (Định lượng số)
        /// </summary>
        public decimal? QuantitativeValue { get; set; }

        /// <summary>
        /// Trạng thái thực tế đạt được (Định tính: NotStarted, Drafting, Reviewing, Completed)
        /// </summary>
        public TextStatusEnum? QualitativeStatus { get; set; }

        /// <summary>
        /// Nội dung giải trình / Ghi chú chuyên viên
        /// </summary>
        public string SummaryNotes { get; set; } = string.Empty;

        public decimal? CalculatedProgressPercentage { get; set; }

        /// <summary>
        /// Mảng các đường dẫn/URL file minh chứng PDF/Word đính kèm (Mapped to PostgreSQL JSONB)
        /// </summary>
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();

        public List<TaskDeliverable>? Deliverables { get; set; }

        /// <summary>
        /// Trạng thái cảnh báo tự động: Green (On track), Yellow (Warning), Red (Overdue)
        /// </summary>
        public AlertStatusEnum CalculatedAlert { get; set; } = AlertStatusEnum.Green;

        public string CreatedBy { get; set; } = "System User";

        public Guid? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        /// <summary>
        /// Trạng thái phê duyệt báo cáo tiến độ (Approved = 1, Pending = 2, Rejected = 3)
        /// </summary>
        public ApprovalStatusEnum ApprovalStatus { get; set; } = ApprovalStatusEnum.Approved;

        /// <summary>
        /// Lý do từ chối (nếu có)
        /// </summary>
        public string? RejectionReason { get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
