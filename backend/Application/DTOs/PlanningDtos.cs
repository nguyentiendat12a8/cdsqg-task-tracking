using System;
using System.Collections.Generic;
using Cdsqg.Core.Entities;

namespace Cdsqg.Application.DTOs
{
    public class PlanningGridResponseDto
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int? PagedTotalCount { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string TimeResolution { get; set; } = "Range";
        public int StartYear { get; set; } = 2026;
        public int EndYear { get; set; } = 2030;
        public List<int> DynamicYears { get; set; } = new List<int>();
        public List<PlanningGridItemDto> Items { get; set; } = new List<PlanningGridItemDto>();
    }

    public class PlanningGridItemDto : IPlanningFilterItem
    {
        public List<Guid> LeadAssignmentAgencyIds { get; set; } = new();
        public List<Guid> CoordinatingAssignmentAgencyIds { get; set; } = new();
        public Guid? ProgressAgencyId { get; set; }
        public string? ProgressAgencyName { get; set; }
        public string? AssignmentKind { get; set; }
        public Guid TaskId { get; set; }
        public string ItemType { get; set; } = "Task"; // Goal (1A) or Task (1B)
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public bool IsOngoing { get; set; } = false;
        
        public bool IsGeneralTask { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid LeadAgencyId { get; set; }
        public string LeadAgencyCode { get; set; } = string.Empty;
        public string LeadAgencyName { get; set; } = string.Empty;
        public string? LeadAgencyDisplayText { get; set; }
        public Guid? AssignedAgencyId { get; set; }
        public string? AssignedAgencyCode { get; set; }
        public string? AssignedAgencyName { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public List<string> CoordinatingAgencyCodes { get; set; } = new List<string>();
        public List<string> CoordinatingAgencyNames { get; set; } = new List<string>();
        public string? CoordinatingAgenciesDisplayText { get; set; }

        public decimal? ResultTarget { get; set; }
        public Guid? UnitId { get; set; }
        public string EvaluationType { get; set; } = "Quantitative";
        public string UnitName { get; set; } = "%";
        public string CalculationMethod { get; set; } = "LatestValue";

        public decimal? CompletionPercentage { get; set; }
        public decimal? ExpectedBaselineTarget { get; set; }
        public decimal? LatestProgressValue { get; set; }
        public string? LatestProgressStatus { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? CreatedAt { get; set; }

        public bool HasPendingApproval { get; set; } = false;

        /// <summary>
        /// Execution state: NotStarted, InProgress, Completed
        /// </summary>
        public string CalculatedStatus { get; set; } = "NotStarted";
        public string DeadlineStatus { get; set; } = "OnTime";
        public string CalculatedAlert { get; set; } = "Red";

        public Dictionary<string, string> CustomBaseline { get; set; } = new Dictionary<string, string>();
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();
        public Dictionary<int, decimal> YearlyAllocations { get; set; } = new();
        public Dictionary<int, object?> YearlyTargets { get; set; } = new Dictionary<int, object?>();
        public Dictionary<int, object?> DefaultYearlyTargets { get; set; } = new();
        public Dictionary<int, decimal?> ConfiguredYearlyTargets { get; set; } = new();

    }

    [System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
    public class CreateGoalTaskItemRequestDto
    {
        public Guid DocumentId { get; set; }
        public string ItemType { get; set; } = "Task";
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public bool IsOngoing { get; set; } = false;

        public bool IsGeneralTask { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid LeadAgencyId { get; set; }
        public string? LeadAgencyDisplayText { get; set; }
        public Guid? AssignedAgencyId { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public string? CoordinatingAgenciesDisplayText { get; set; }

        public decimal? ResultTarget { get; set; }
        public Guid? UnitId { get; set; }
        public string? UnitName { get; set; }
        public string EvaluationType { get; set; } = "Quantitative";
        public string CalculationMethod { get; set; } = "LatestValue";
        public Dictionary<string, string>? CustomBaseline { get; set; }
        public Dictionary<int, decimal>? YearlyTargets { get; set; }
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();
    }

    public class UpdateCustomBaselineDto
    {
        public Dictionary<string, string> Milestones { get; set; } = new Dictionary<string, string>();
    }

    public class UpdateYearlyTargetDto
    {
        public int Year { get; set; }
        public decimal? TargetQuantity { get; set; }
        public string? TargetQualitativeStatus { get; set; }
    }

    /// <summary>Kế hoạch mục tiêu theo năm do đơn vị chủ trì nhập từ Excel (sheet "Kế hoạch mục tiêu").</summary>
    public class ImportYearlyTargetsRequestDto
    {
        public List<ImportYearlyTargetRowDto> Rows { get; set; } = new();
        /// <summary>false: chỉ kiểm tra và trả danh sách kế hoạch đã có; true: ghi đè sau khi người dùng xác nhận.</summary>
        public bool ConfirmOverwrite { get; set; }
    }

    public class ImportYearlyTargetRowDto
    {
        public Guid TaskId { get; set; }
        public int Year { get; set; }
        public decimal TargetQuantity { get; set; }
    }

    public class YearlyTargetConflictDto
    {
        public Guid TaskId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal? CurrentTarget { get; set; }
        public decimal NewTarget { get; set; }
    }

    public class ImportYearlyTargetsResultDto
    {
        public bool RequiresConfirmation { get; set; }
        public int SavedCount { get; set; }
        public int UnchangedCount { get; set; }
        public List<YearlyTargetConflictDto> Conflicts { get; set; } = new();
    }

    public class UpdateGoalTaskItemRequestDto
    {
        public string? Code { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? Group { get; set; }
        public bool IsOngoing { get; set; } = false;
        public bool IsGeneralTask { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid LeadAgencyId { get; set; }
        public string? LeadAgencyDisplayText { get; set; }
        public Guid? AssignedAgencyId { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public string? CoordinatingAgenciesDisplayText { get; set; }
        private decimal? _resultTarget;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ResultTargetSpecified { get; private set; }
        public decimal? ResultTarget
        {
            get => _resultTarget;
            set { _resultTarget = value; ResultTargetSpecified = true; }
        }
        public Guid? UnitId { get; set; }
        public string? UnitName { get; set; }
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();
    }

    public class AssignGoalTaskItemRequestDto
    {
        public Guid? AssignedAgencyId { get; set; }
    }

    public class AgencyProgressReportDto
    {
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string AgencyCode { get; set; } = string.Empty;
        public string AgencyType { get; set; } = string.Empty;
        public int ReportYear { get; set; } = 2026;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; } = 1;
        public int TotalGoals { get; set; }
        public int TotalTasks { get; set; }
        public string ActiveTab { get; set; } = "goals";
        public List<AgencyProgressGoalItemDto> Goals { get; set; } = new();
        public List<AgencyProgressTaskItemDto> Tasks { get; set; } = new();
    }

    public class AgencyProgressGoalItemDto
    {
        public int SequenceNumber { get; set; }
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public List<string> SubCodes { get; set; } = new();
        public List<Guid> SubGoalIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string UnitName { get; set; } = "%";
        public string? Section { get; set; }
        public string? TargetPhase1 { get; set; }
        public string? TargetPhase2 { get; set; }
        public decimal? Actual2025 { get; set; }
        public decimal? Estimate2026 { get; set; }
        public string? Plan2027 { get; set; }
        public string? Plan2028 { get; set; }
        public string? Plan2029 { get; set; }
        public string? Plan2030 { get; set; }
        public string? Notes { get; set; }
        public List<int> ApplicableYears { get; set; } = new();
    }

    public class AgencyProgressTaskItemDto
    {
        public int SequenceNumber { get; set; }
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CoordinatingAgencies { get; set; }
        public string? DueDate { get; set; }
        public string Status { get; set; } = "NotStarted";
        public string StatusLabel { get; set; } = "Chưa thực hiện";
        public decimal? CompletionPercentage { get; set; }
        public string? Deliverables { get; set; }
        public string? Notes { get; set; }
    }
}
