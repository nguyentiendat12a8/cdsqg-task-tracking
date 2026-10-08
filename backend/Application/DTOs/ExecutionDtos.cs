using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.DTOs
{
    [System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
    public class SubmitProgressRequestDto
    {
        public int PeriodYear { get; set; } = 2026;
        public string PeriodKey => PeriodYear.ToString(System.Globalization.CultureInfo.InvariantCulture);

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
        public string? UserRole { get; set; }
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
        public decimal? ExpectedBaselineTarget { get; set; }
        public bool IsCustomBaselineUsed { get; set; }

        public decimal? CompletionPercentage { get; set; }
        public AlertStatusEnum CalculatedAlert { get; set; } // Green, Yellow, Red
        
        public string EvidenceFileUrl { get; set; } = string.Empty;
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = string.Empty;

        public string ApprovalStatus { get; set; } = "Approved";
        public string? RejectionReason { get; set; }
    }

    public class GetProgressLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Task";
        public bool IsGeneralTask { get; set; }
        public int PeriodYear { get; set; }
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
        public string AgencyCode { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public Guid? ParentAgencyId { get; set; }
        public string? ParentAgencyName { get; set; }

        public string ApprovalStatus { get; set; } = "Approved";
        public string? RejectionReason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Before Update Snapshot fields
        public decimal? PreviousValue { get; set; }
        public string? PreviousStatus { get; set; }
        public decimal? PreviousCompletionPercentage { get; set; }
        public string? PreviousNotes { get; set; }
        public List<TaskDeliverable>? PreviousDeliverables { get; set; }
    }

    public class AgencyTaskExecutionMatrixDto
    {
        public Guid TaskId { get; set; }
        public string TaskCode { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public bool IsGeneralTask { get; set; }
        public List<AgencyExecutionItemDto> AgencyExecutions { get; set; } = new List<AgencyExecutionItemDto>();
    }

    public class AgencyExecutionItemDto
    {
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string AgencyCode { get; set; } = string.Empty;
        public string AgencyType { get; set; } = string.Empty;
        public Guid? ParentAgencyId { get; set; }
        public string? ParentAgencyName { get; set; }

        public string CalculatedStatus { get; set; } = "NotStarted";
        public string ApprovalStatus { get; set; } = "Approved";
        public string? RejectionReason { get; set; }
        public decimal? LatestProgressValue { get; set; }
        public string? LatestQualitativeStatus { get; set; }
        public decimal? CompletionPercentage { get; set; }
        public string? SummaryNotes { get; set; }
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
        public List<TaskDeliverable>? Deliverables { get; set; }
        public DateTime? LastReportedAt { get; set; }
        public string? LastReportedBy { get; set; }

        public Guid? PendingProgressLogId { get; set; }
    }

    public class RejectProgressRequestDto
    {
        public string? Reason { get; set; }
    }

    [System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
    public class ImportProgressBulkItemDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public TextStatusEnum? Status { get; set; }
        public string? SummaryNotes { get; set; }
        public int PeriodYear { get; set; } = 2026;
    }

    public class ImportProgressBulkRequestDto
    {
        public Guid? UserAgencyId { get; set; }
        public string? UserRole { get; set; } = string.Empty;
        public List<ImportProgressBulkItemDto> Items { get; set; } = new List<ImportProgressBulkItemDto>();
    }

    public class ImportProgressResultItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = "Approved";
    }

    public class ImportProgressBulkResponseDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int PendingCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportProgressResultItemDto> Results { get; set; } = new List<ImportProgressResultItemDto>();
    }
}
