using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.DTOs
{
    public class SubmitProgressRequestDto
    {
        public int PeriodYear { get; set; } = 2026;
        public int PeriodQuarter { get; set; } = 0;
        public int PeriodMonth { get; set; } = 0;
        public string PeriodType { get; set; } = "Yearly"; // "Yearly", "Quarterly", "Monthly"

        /// <summary>
        /// Period string formatted key (e.g., "M1_2026", "Q1_2026", "2026")
        /// </summary>
        public string PeriodKey
        {
            get
            {
                if (PeriodType == "Monthly" || PeriodMonth > 0)
                    return $"M{PeriodMonth}_{PeriodYear}";
                if (PeriodType == "Quarterly" || PeriodQuarter > 0)
                    return $"Q{PeriodQuarter}_{PeriodYear}";
                return $"{PeriodYear}";
            }
        }

        public decimal? Value { get; set; } // Quantitative
        public decimal? ActualValue { get => Value; set => Value = value; }

        public TextStatusEnum? Status { get; set; } // Qualitative (NotStarted, Drafting, Reviewing, Completed)
        public TextStatusEnum? QualitativeStatus { get => Status; set => Status = value; }
        
        public string? SummaryNotes { get; set; } = string.Empty;
        public List<TaskDeliverable>? Deliverables { get; set; }
        public IFormFile? EvidenceFile { get; set; }
        public List<IFormFile>? EvidenceFiles { get; set; }
        public List<string>? ExistingFiles { get; set; }
        public string? CreatedBy { get; set; } = "Chuyên viên theo dõi";
        public Guid? AgencyId { get; set; }
    }

    public class SubmitProgressResponseDto
    {
        public Guid ProgressLogId { get; set; }
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        
        public string PeriodKey { get; set; } = string.Empty;
        public decimal? ActualValue { get; set; }
        public decimal ExpectedBaselineTarget { get; set; }
        public bool IsCustomBaselineUsed { get; set; }

        public decimal CompletionPercentage { get; set; }
        public AlertStatusEnum CalculatedAlert { get; set; } // Green, Yellow, Red
        
        public string EvidenceFileUrl { get; set; } = string.Empty;
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;
    }

    public class GetProgressLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodQuarter { get; set; }
        public decimal? ActualValue { get; set; }
        public string? Status { get; set; }
        public decimal? CompletionPercentage { get; set; }
        public string? SummaryNotes { get; set; }
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
        public List<TaskDeliverable>? Deliverables { get; set; }
        public DateTime LogDate { get; set; }
        public AlertStatusEnum CalculatedAlert { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public Guid? AgencyId { get; set; }

        // Before Update Snapshot fields
        public decimal? PreviousValue { get; set; }
        public string? PreviousStatus { get; set; }
        public decimal? PreviousCompletionPercentage { get; set; }
        public string? PreviousNotes { get; set; }
        public List<TaskDeliverable>? PreviousDeliverables { get; set; }
    }
}
