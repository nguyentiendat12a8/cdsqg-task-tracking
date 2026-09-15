using System;
using System.Collections.Generic;

namespace Cdsqg.Application.DTOs
{
    public class ExecutiveDashboardMetricsDto
    {
        public Guid? DocumentId { get; set; }
        public string DocumentNumber { get; set; } = "Tất cả văn bản";
        public string DocumentName { get; set; } = "Toàn bộ chỉ đạo CĐS Quốc gia";

        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        
        public decimal OverallQuantitativeCompletionPct { get; set; }
        public decimal OverallRemainingPct => Math.Max(0, 100 - OverallQuantitativeCompletionPct);

        public TrafficLightCountDto TrafficLights { get; set; } = new TrafficLightCountDto();
        
        // 3 Alert Category Breakdown Counts
        public int OverdueCount { get; set; }
        public int LaggingCount { get; set; }
        public int AtRiskCount { get; set; }

        public List<StaleTaskDto> StaleTasks { get; set; } = new List<StaleTaskDto>();
        public List<AgencyPerformanceDto> AgencyPerformance { get; set; } = new List<AgencyPerformanceDto>();
        public QualitativeDistributionDto QualitativeDistribution { get; set; } = new QualitativeDistributionDto();
    }

    public class TrafficLightCountDto
    {
        public int GreenCount { get; set; }
        public int YellowCount { get; set; }
        public int RedCount { get; set; }
    }

    public class StaleTaskDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string LeadAgency { get; set; } = string.Empty;
        public int DaysSinceLastLog { get; set; }
        public decimal ActualProgressPct { get; set; }
        public decimal ExpectedTargetPct { get; set; }
        public decimal ExpectedLinearProgress { get; set; }
        public decimal LaggingDeltaPct { get; set; }
        public bool HasReport { get; set; } = true;
        
        public string ItemType { get; set; } = "Task"; // "Goal" or "Task"
        public string StaleCategory { get; set; } = "Lagging"; // "Overdue", "Lagging", "AtRisk"
        public string StaleCategoryName { get; set; } = "Chậm tiến độ";
    }

    public class AgencyPerformanceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int AtRisk { get; set; }
        public int Overdue { get; set; }
        public int Total { get; set; }
    }

    public class QualitativeDistributionDto
    {
        public int NotStarted { get; set; }
        public int Drafting { get; set; }
        public int Reviewing { get; set; }
        public int Completed { get; set; }
    }
}
