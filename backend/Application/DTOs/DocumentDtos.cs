using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.DTOs
{
    public class CreateDocumentRequestDto
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Signer { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public TimeResolutionEnum TimeResolution { get; set; } = TimeResolutionEnum.Range;
        public int? StartYear { get; set; } = 2026;
        public int? EndYear { get; set; } = 2030;
        public IFormFile? AttachmentFile { get; set; }
        public List<IFormFile>? AttachmentFiles { get; set; }
    }

    public class UpdateDocumentRequestDto
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Signer { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public TimeResolutionEnum TimeResolution { get; set; } = TimeResolutionEnum.Range;
        public int? StartYear { get; set; } = 2026;
        public int? EndYear { get; set; } = 2030;
        public IFormFile? AttachmentFile { get; set; }
        public List<IFormFile>? AttachmentFiles { get; set; }
    }

    public class DocumentSummaryDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Signer { get; set; }
        public DateTime IssueDate { get; set; }
        public string TimeResolution { get; set; } = string.Empty;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? AttachmentPath { get; set; }
        public List<string> AttachmentFilePaths { get; set; } = new List<string>();
        public int TotalGoals { get; set; }
        public int TotalTasks { get; set; }
        public decimal OverallCompletionRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateGoalTaskItemRequestDto
    {
        public Guid DocumentId { get; set; }
        public string ItemType { get; set; } = "Task"; // "Goal" or "Task"
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid LeadAgencyId { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public Guid? UnitId { get; set; }
        public string EvaluationType { get; set; } = "Quantitative"; // "Quantitative" or "Qualitative"
        public string CalculationMethod { get; set; } = "LatestValue";
        public Dictionary<int, decimal>? YearlyTargets { get; set; }
        public Dictionary<string, string>? CustomBaseline { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
