using System;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Stores urge logs (Văn bản & Chỉ số đôn đốc) sent to Ministries/Agencies for lagging tasks
    /// </summary>
    public class TaskUrgeLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid GoalTaskId { get; set; }
        public GoalTaskItem? GoalTaskItem { get; set; }

        public Guid? LeadAgencyId { get; set; }
        public Agency? LeadAgency { get; set; }

        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;

        public string UrgeContent { get; set; } = string.Empty;

        /// <summary>
        /// JSON payload containing progress forecast metrics (Current %, Expected Target %, Overdue Delta %, Estimated Completion Date)
        /// </summary>
        public string ForecastDataJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "Chuyên viên theo dõi CĐS";
    }
}
