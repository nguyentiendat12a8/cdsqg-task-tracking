using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        private decimal? GetExpectedTargetForItem(GoalTaskItem task, int year, int quarter)
        {
            if (task.EvaluationType == EvaluationTypeEnum.Qualitative)
            {
                if (task.CustomBaseline != null && task.CustomBaseline.Count > 0)
                {
                    string yKey = $"{year}";
                    if (task.CustomBaseline.TryGetValue(yKey, out var vy) && !string.IsNullOrWhiteSpace(vy) && vy != "--")
                        return 100m;

                    for (int q = 4; q >= 1; q--)
                    {
                        string k1 = $"Q{q}_{year}";
                        string k2 = $"{year}_Q{q}";
                        if ((task.CustomBaseline.TryGetValue(k1, out var val1) && !string.IsNullOrWhiteSpace(val1) && val1 != "--") ||
                            (task.CustomBaseline.TryGetValue(k2, out var val2) && !string.IsNullOrWhiteSpace(val2) && val2 != "--"))
                        {
                            return 100m;
                        }
                    }
                }

                if (task.Baselines != null && task.Baselines.Count > 0)
                {
                    var b = task.Baselines.FirstOrDefault(b => b.Year == year && (b.Quarter == quarter || b.Quarter == 0 || b.Quarter == 4));
                    if (b != null && (b.TargetQualitativeStatus.HasValue || (b.TargetQuantity.HasValue && b.TargetQuantity.Value > 0)))
                    {
                        return 100m;
                    }
                }

                return null;
            }

            // 1. Check CustomBaseline JSONB dictionary for exact period or year targets
            if (task.CustomBaseline != null && task.CustomBaseline.Count > 0)
            {
                string qKey1 = $"Q{quarter}_{year}";
                string qKey2 = $"{year}_Q{quarter}";
                string yKey = $"{year}";

                if (task.CustomBaseline.TryGetValue(qKey1, out var v1) && decimal.TryParse(v1, out decimal target1) && target1 > 0)
                    return target1;
                if (task.CustomBaseline.TryGetValue(qKey2, out var v2) && decimal.TryParse(v2, out decimal target2) && target2 > 0)
                    return target2;
                if (task.CustomBaseline.TryGetValue(yKey, out var vy) && decimal.TryParse(vy, out decimal targetY) && targetY > 0)
                    return targetY;

                // Check any quarter key for specified year (Q4..Q1)
                for (int q = 4; q >= 1; q--)
                {
                    string k1 = $"Q{q}_{year}";
                    string k2 = $"{year}_Q{q}";
                    if ((task.CustomBaseline.TryGetValue(k1, out var val) || task.CustomBaseline.TryGetValue(k2, out val)) && decimal.TryParse(val, out decimal tQ) && tQ > 0)
                    {
                        return tQ;
                    }
                }
            }

            // 2. Check Baselines table for specified year
            if (task.Baselines != null && task.Baselines.Count > 0)
            {
                var qBaseline = task.Baselines.FirstOrDefault(b => b.Year == year && b.Quarter == quarter);
                if (qBaseline?.TargetQuantity.HasValue == true && qBaseline.TargetQuantity.Value > 0)
                {
                    return qBaseline.TargetQuantity.Value;
                }

                var yBaseline = task.Baselines.FirstOrDefault(b => b.Year == year && (b.Quarter == 0 || b.Quarter == 4));
                if (yBaseline?.TargetQuantity.HasValue == true && yBaseline.TargetQuantity.Value > 0)
                {
                    return yBaseline.TargetQuantity.Value;
                }
            }

            // No plan/target setup for this year
            return null;
        }

        /// <summary>
        /// GET /api/dashboard/documents/{id}/metrics
        /// Hoặc GET /api/dashboard/metrics?documentId={id}
        /// Truy vấn dữ liệu thực từ PostgreSQL để tổng hợp chỉ số Dashboard báo cáo Lãnh đạo
        /// </summary>
        [HttpGet("documents/{id}/metrics")]
        [HttpGet("overview")]
        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics([FromRoute] string? id = null, [FromQuery] string? documentId = null, [FromQuery] int? year = 2026, [FromQuery] int? quarter = null)
        {
            try
            {
                // Unify documentId parameter
                string targetDocIdStr = !string.IsNullOrEmpty(id) && id != "overview" && id != "metrics" ? id : (documentId ?? string.Empty);
                Guid? docGuid = null;
                if (Guid.TryParse(targetDocIdStr, out var parsedGuid))
                {
                    docGuid = parsedGuid;
                }

                Document? selectedDoc = null;
                if (docGuid.HasValue)
                {
                    selectedDoc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == docGuid.Value);
                }

                // Query GoalTaskItems
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .AsQueryable();

                if (docGuid.HasValue)
                {
                    query = query.Where(i => i.DocumentId == docGuid.Value);
                }

                var items = await query.ToListAsync();
                var allAgencies = await _context.Agencies.ToListAsync();

                int targetYear = year ?? 2026;
                int targetQuarter = quarter ?? ((DateTime.UtcNow.Month - 1) / 3 + 1);

                int totalGoals = items.Count(i => i.ItemType == ItemTypeEnum.Goal);
                int totalTasks = items.Count(i => i.ItemType == ItemTypeEnum.Task);

                int completedGoals = 0;
                int completedTasks = 0;

                int greenCount = 0;
                int yellowCount = 0;
                int redCount = 0;

                decimal sumCompletionPct = 0;
                int quantCount = 0;

                var staleTasks = new List<StaleTaskDto>();
                var agencyMap = allAgencies.ToDictionary(a => a.Id, a => new AgencyPerformanceDto
                {
                    Code = a.Code,
                    Name = a.Name,
                    Completed = 0,
                    Overdue = 0,
                    Total = 0
                });

                var qualitativeDist = new QualitativeDistributionDto
                {
                    NotStarted = 0,
                    Drafting = 0,
                    Reviewing = 0,
                    Completed = 0
                };

                DateTime now = DateTime.UtcNow;

                // Calculate time elapsed fraction of target year
                decimal f;
                if (now.Year > targetYear)
                {
                    f = 1.0m;
                }
                else if (now.Year < targetYear)
                {
                    f = 0.0m;
                }
                else
                {
                    int daysInYear = DateTime.IsLeapYear(targetYear) ? 366 : 365;
                    f = (decimal)now.DayOfYear / daysInYear;
                }
                f = Math.Max(0.0m, Math.Min(1.0m, f));

                int overdueCount = 0;
                int laggingCount = 0;
                int atRiskCount = 0;

                foreach (var task in items)
                {
                    // Track agency assigned tasks count
                    if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId))
                    {
                        agencyMap[task.LeadAgencyId].Total++;
                    }

                    var latestLog = task.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();

                    // Calculate staleness (in days)
                    int daysSinceLastLog = latestLog != null ? (now - latestLog.LogDate).Days : 999;
                    string itemTypeName = task.ItemType == ItemTypeEnum.Goal ? "Mục tiêu" : "Nhiệm vụ";

                    if (task.EvaluationType == EvaluationTypeEnum.Qualitative)
                    {
                        int evalYear = latestLog?.PeriodYear ?? targetYear;
                        int evalQuarter = latestLog?.PeriodQuarter ?? targetQuarter;

                        decimal? qualTargetOpt = GetExpectedTargetForItem(task, evalYear, evalQuarter);

                        var status = latestLog?.QualitativeStatus ?? TextStatusEnum.NotStarted;
                        switch (status)
                        {
                            case TextStatusEnum.NotStarted:
                                qualitativeDist.NotStarted++;
                                break;
                            case TextStatusEnum.Drafting:
                                qualitativeDist.Drafting++;
                                break;
                            case TextStatusEnum.Reviewing:
                                qualitativeDist.Reviewing++;
                                break;
                            case TextStatusEnum.Completed:
                                qualitativeDist.Completed++;
                                greenCount++;
                                if (task.ItemType == ItemTypeEnum.Goal) completedGoals++;
                                else completedTasks++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Completed++;
                                break;
                        }

                        if (status != TextStatusEnum.Completed && qualTargetOpt.HasValue)
                        {
                            string cat;
                            string catName;
                            decimal actPct = status == TextStatusEnum.Reviewing ? 75m : (status == TextStatusEnum.Drafting ? 40m : 0m);

                            if (f >= 1.0m)
                            {
                                cat = "Overdue";
                                catName = "Quá hạn hoàn thành";
                                overdueCount++;
                                redCount++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Overdue++;
                            }
                            else if (f >= 0.5m)
                            {
                                if (status == TextStatusEnum.NotStarted)
                                {
                                    cat = "Lagging";
                                    catName = "Chậm tiến độ";
                                    laggingCount++;
                                    redCount++;
                                    if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Overdue++;
                                }
                                else
                                {
                                    cat = "AtRisk";
                                    catName = "Nguy cơ chậm tiến độ";
                                    atRiskCount++;
                                    yellowCount++;
                                    if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].AtRisk++;
                                }
                            }
                            else
                            {
                                cat = "AtRisk";
                                catName = "Nguy cơ chậm tiến độ";
                                atRiskCount++;
                                yellowCount++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].AtRisk++;
                            }

                            staleTasks.Add(new StaleTaskDto
                            {
                                Id = task.Id,
                                DocumentId = task.DocumentId,
                                Code = task.Code,
                                Title = task.Title,
                                LeadAgency = task.LeadAgency?.Name ?? "Chưa phân công",
                                DaysSinceLastLog = daysSinceLastLog == 999 ? 0 : daysSinceLastLog,
                                ActualProgressPct = actPct,
                                ExpectedTargetPct = 100m,
                                ExpectedLinearProgress = Math.Round(f * 100m, 1),
                                LaggingDeltaPct = Math.Round(actPct - 100m, 1),
                                HasReport = latestLog != null,
                                ItemType = itemTypeName,
                                StaleCategory = cat,
                                StaleCategoryName = catName
                            });
                        }
                    }
                    else
                    {
                        // Quantitative task/goal
                        int evalYear = latestLog?.PeriodYear ?? targetYear;
                        int evalQuarter = latestLog?.PeriodQuarter ?? targetQuarter;

                        decimal? targetValOpt = GetExpectedTargetForItem(task, evalYear, evalQuarter);

                        // User requirement: If no plan has been setup for this year, do NOT display in lagging list on Dashboard
                        if (!targetValOpt.HasValue)
                        {
                            continue;
                        }

                        quantCount++;
                        decimal targetVal = targetValOpt.Value;
                        decimal actualVal = latestLog?.QuantitativeValue ?? 0m;
                        bool hasReport = latestLog != null && latestLog.QuantitativeValue.HasValue;

                        decimal pct = targetVal > 0 ? Math.Round((actualVal / targetVal) * 100m, 1) : 0m;
                        sumCompletionPct += Math.Min(100m, Math.Max(0m, pct));

                        // Expected linear progress by elapsed time
                        decimal expectedLinear = Math.Round(f * targetVal, 2);

                        if (f >= 1.0m)
                        {
                            if (actualVal >= targetVal)
                            {
                                greenCount++;
                                if (task.ItemType == ItemTypeEnum.Goal) completedGoals++;
                                else completedTasks++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Completed++;
                            }
                            else
                            {
                                overdueCount++;
                                redCount++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Overdue++;

                                staleTasks.Add(new StaleTaskDto
                                {
                                    Id = task.Id,
                                    DocumentId = task.DocumentId,
                                    Code = task.Code,
                                    Title = task.Title,
                                    LeadAgency = task.LeadAgency?.Name ?? "Chưa phân công",
                                    DaysSinceLastLog = daysSinceLastLog == 999 ? 0 : daysSinceLastLog,
                                    ActualProgressPct = actualVal,
                                    ExpectedTargetPct = targetVal,
                                    ExpectedLinearProgress = expectedLinear,
                                    LaggingDeltaPct = Math.Round(actualVal - targetVal, 1),
                                    HasReport = hasReport,
                                    ItemType = itemTypeName,
                                    StaleCategory = "Overdue",
                                    StaleCategoryName = "Quá hạn hoàn thành"
                                });
                            }
                        }
                        else
                        {
                            // Mid-year / within current year execution
                            if (!hasReport)
                            {
                                // User rule: Unreported items within execution period are classified as AtRisk (Nguy cơ chậm tiến độ)
                                atRiskCount++;
                                yellowCount++;
                                if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].AtRisk++;

                                staleTasks.Add(new StaleTaskDto
                                {
                                    Id = task.Id,
                                    DocumentId = task.DocumentId,
                                    Code = task.Code,
                                    Title = task.Title,
                                    LeadAgency = task.LeadAgency?.Name ?? "Chưa phân công",
                                    DaysSinceLastLog = daysSinceLastLog == 999 ? 0 : daysSinceLastLog,
                                    ActualProgressPct = 0m,
                                    ExpectedTargetPct = targetVal,
                                    ExpectedLinearProgress = expectedLinear,
                                    LaggingDeltaPct = 0m,
                                    HasReport = false,
                                    ItemType = itemTypeName,
                                    StaleCategory = "AtRisk",
                                    StaleCategoryName = "Nguy cơ chậm tiến độ"
                                });
                            }
                            else
                            {
                                decimal thresholdLagging = Math.Round(0.7m * expectedLinear, 2);
                                decimal thresholdAtRisk = Math.Round(0.95m * expectedLinear, 2);

                                if (expectedLinear > 0m && actualVal < thresholdLagging)
                                {
                                    // Lagging progress (< 70% of expected linear milestone)
                                    laggingCount++;
                                    redCount++;
                                    if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Overdue++;

                                    staleTasks.Add(new StaleTaskDto
                                    {
                                        Id = task.Id,
                                        DocumentId = task.DocumentId,
                                        Code = task.Code,
                                        Title = task.Title,
                                        LeadAgency = task.LeadAgency?.Name ?? "Chưa phân công",
                                        DaysSinceLastLog = daysSinceLastLog == 999 ? 0 : daysSinceLastLog,
                                        ActualProgressPct = actualVal,
                                        ExpectedTargetPct = targetVal,
                                        ExpectedLinearProgress = expectedLinear,
                                        LaggingDeltaPct = Math.Round(actualVal - expectedLinear, 1),
                                        HasReport = true,
                                        ItemType = itemTypeName,
                                        StaleCategory = "Lagging",
                                        StaleCategoryName = "Chậm tiến độ"
                                    });
                                }
                                else if (expectedLinear > 0m && actualVal < thresholdAtRisk)
                                {
                                    // At risk of lagging (70% <= actual < 95% of expected linear milestone)
                                    atRiskCount++;
                                    yellowCount++;
                                    if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].AtRisk++;

                                    staleTasks.Add(new StaleTaskDto
                                    {
                                        Id = task.Id,
                                        DocumentId = task.DocumentId,
                                        Code = task.Code,
                                        Title = task.Title,
                                        LeadAgency = task.LeadAgency?.Name ?? "Chưa phân công",
                                        DaysSinceLastLog = daysSinceLastLog == 999 ? 0 : daysSinceLastLog,
                                        ActualProgressPct = actualVal,
                                        ExpectedTargetPct = targetVal,
                                        ExpectedLinearProgress = expectedLinear,
                                        LaggingDeltaPct = Math.Round(actualVal - expectedLinear, 1),
                                        HasReport = true,
                                        ItemType = itemTypeName,
                                        StaleCategory = "AtRisk",
                                        StaleCategoryName = "Nguy cơ chậm tiến độ"
                                    });
                                }
                                else
                                {
                                    // On track
                                    if (actualVal >= targetVal)
                                    {
                                        greenCount++;
                                        if (task.ItemType == ItemTypeEnum.Goal) completedGoals++;
                                        else completedTasks++;
                                        if (task.LeadAgencyId != Guid.Empty && agencyMap.ContainsKey(task.LeadAgencyId)) agencyMap[task.LeadAgencyId].Completed++;
                                    }
                                    else
                                    {
                                        greenCount++;
                                    }
                                }
                            }
                        }
                    }
                }

                decimal overallQuantPct = quantCount > 0 ? Math.Round(sumCompletionPct / quantCount, 1) : (totalTasks > 0 ? 75.0m : 0m);

                // Filter top active agency performances
                var agencyPerformanceList = agencyMap.Values
                    .Where(a => a.Total > 0)
                    .OrderByDescending(a => a.Total)
                    .Take(6)
                    .ToList();

                if (agencyPerformanceList.Count == 0)
                {
                    agencyPerformanceList = allAgencies.Take(5).Select(a => new AgencyPerformanceDto
                    {
                        Code = a.Code,
                        Name = a.Name,
                        Completed = 0,
                        Overdue = 0,
                        Total = 0
                    }).ToList();
                }

                var metrics = new ExecutiveDashboardMetricsDto
                {
                    DocumentId = selectedDoc?.Id,
                    DocumentNumber = selectedDoc?.DocumentNumber ?? "Tất cả văn bản",
                    DocumentName = selectedDoc?.Name ?? "Toàn bộ chỉ đạo CĐS Quốc gia",
                    TotalGoals = totalGoals,
                    CompletedGoals = completedGoals,
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    OverallQuantitativeCompletionPct = overallQuantPct,
                    TrafficLights = new TrafficLightCountDto
                    {
                        GreenCount = greenCount,
                        YellowCount = yellowCount,
                        RedCount = redCount
                    },
                    OverdueCount = overdueCount,
                    LaggingCount = laggingCount,
                    AtRiskCount = atRiskCount,
                    StaleTasks = staleTasks.OrderBy(t => t.LaggingDeltaPct).ToList(),
                    AgencyPerformance = agencyPerformanceList,
                    QualitativeDistribution = qualitativeDist
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống khi tải metrics Dashboard", details = ex.Message });
            }
        }
    }
}
