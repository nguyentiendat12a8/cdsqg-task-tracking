using System;
using System.Collections.Generic;

namespace Cdsqg.Application.DTOs
{
    public class PlanningGridResponseDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string TimeResolution { get; set; } = "Range";
        public int StartYear { get; set; } = 2026;
        public int EndYear { get; set; } = 2030;
        public List<int> DynamicYears { get; set; } = new List<int>();
        public List<PlanningGridItemDto> Items { get; set; } = new List<PlanningGridItemDto>();
    }

    public class PlanningGridItemDto
    {
        public Guid TaskId { get; set; }
        public Guid? ParentId { get; set; }
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
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public List<string> CoordinatingAgencyCodes { get; set; } = new List<string>();
        public List<string> CoordinatingAgencyNames { get; set; } = new List<string>();

        public Guid? UnitId { get; set; }
        public string EvaluationType { get; set; } = "Quantitative";
        public string UnitName { get; set; } = "%";
        public string CalculationMethod { get; set; } = "LatestValue";

        public decimal? LatestProgressValue { get; set; }
        public string? LatestProgressStatus { get; set; }
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// 6-status execution evaluation status: NotStarted, InProgressOnTime, InProgressOverdue, CompletedOnTime, CompletedOverdue, ExpiringSoon
        /// </summary>
        public string CalculatedStatus { get; set; } = "NotStarted";

        public Dictionary<string, string> CustomBaseline { get; set; } = new Dictionary<string, string>();
        public Dictionary<int, object?> YearlyTargets { get; set; } = new Dictionary<int, object?>();

        public List<PlanningGridItemDto> SubItems { get; set; } = new List<PlanningGridItemDto>();
    }

    public class CreateGoalTaskItemDto
    {
        public Guid DocumentId { get; set; }
        public Guid? ParentId { get; set; }
        public string ItemType { get; set; } = "Task";
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Chính phủ số";
        public string Section { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public bool IsOngoing { get; set; } = false;
        public bool IsGeneralTask { get; set; } = false;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public Guid LeadAgencyId { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();

        public Guid? UnitId { get; set; }
        public string EvaluationType { get; set; } = "Quantitative";
        public string CalculationMethod { get; set; } = "LatestValue";
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
}
