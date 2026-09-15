using System;
using System.Collections.Generic;

namespace Cdsqg.Application.DTOs
{
    public class CreateTaskUrgeLogDto
    {
        public Guid GoalTaskId { get; set; }
        public string UrgeContent { get; set; } = string.Empty;

        public decimal ActualProgressPct { get; set; } = 0m;
        public decimal ExpectedTargetPct { get; set; } = 100m;
        public decimal LaggingDeltaPct { get; set; } = 0m;
        public string EstimatedCompletionDate { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = "Lãnh đạo chỉ đạo CĐS";
    }

    public class TaskUrgeLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid GoalTaskId { get; set; }
        public Guid? LeadAgencyId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string LeadAgencyCode { get; set; } = string.Empty;
        public string LeadAgencyName { get; set; } = string.Empty;
        public string UrgeContent { get; set; } = string.Empty;
        public string ForecastDataJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
