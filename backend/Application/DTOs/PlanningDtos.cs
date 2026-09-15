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
        public string ItemType { get; set; } = "Task"; // Goal (1A) or Task (1B)
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        public string LeadAgencyCode { get; set; } = string.Empty;
        public string LeadAgencyName { get; set; } = string.Empty;
        public List<string> CoordinatingAgencyCodes { get; set; } = new List<string>();

        public string EvaluationType { get; set; } = "Quantitative";
        public string UnitName { get; set; } = "%";
        public string CalculationMethod { get; set; } = "LatestValue";

        public decimal? LatestProgressValue { get; set; }
        public string? LatestProgressStatus { get; set; }
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Custom baseline milestone overrides dictionary (e.g. {"Q1_2026": "10.0", "hasQuarter": "true"})
        /// </summary>
        public Dictionary<string, string> CustomBaseline { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Computed target breakdown by year (decimal for Quantitative, string for Qualitative)
        /// </summary>
        public Dictionary<int, object?> YearlyTargets { get; set; } = new Dictionary<int, object?>();
    }

    public class UpdateCustomBaselineDto
    {
        /// <summary>
        /// Dictionary of period milestone keys to expected target values or statuses
        /// </summary>
        public Dictionary<string, string> Milestones { get; set; } = new Dictionary<string, string>();
    }

    public class UpdateYearlyTargetDto
    {
        public int Year { get; set; }
        public decimal? TargetQuantity { get; set; }
        public string? TargetQualitativeStatus { get; set; }
    }
}
