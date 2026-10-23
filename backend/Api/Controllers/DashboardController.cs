using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
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

        [HttpGet("metrics")]
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardMetrics(
            [FromQuery] Guid[]? agencyId = null,
            [FromQuery] Guid? parentAgencyId = null,
            [FromQuery] Guid? documentId = null,
            [FromQuery] Guid[]? documentIds = null,
            [FromQuery] string[]? section = null,
            [FromQuery] string[]? group = null,
            [FromQuery] int? reportYear = null,
            [FromQuery] int? fromYear = null,
            [FromQuery] int? toYear = null,
            [FromQuery] bool? isOngoing = null,
            [FromQuery] string? itemType = null,
            [FromQuery] string? scope = null,
            [FromQuery] string assignmentRole = "Lead",
            [FromQuery] string? detailMode = null,
            [FromQuery] string? detailStatus = null,
            [FromQuery] string detailScope = "specific",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? reportFromYear = null, [FromQuery] int? reportToYear = null,
            [FromQuery] DateTime? executionFrom = null, [FromQuery] DateTime? executionTo = null)
        {
            try
            {
                if (reportYear.HasValue && reportYear.Value is < 1900 or > 9999) return BadRequest("Năm báo cáo không hợp lệ.");
                var timeFilter = new DashboardTimeFilter(reportFromYear, reportToYear, executionFrom, executionTo);
                if (timeFilter.ValidationError() is { } timeError) return BadRequest(timeError);
                if (reportYear.HasValue && (reportYear < reportFromYear || reportYear > reportToYear)) return BadRequest("Năm báo cáo nằm ngoài khoảng đã chọn.");
                if (detailMode != null && detailMode is not ("items" or "assignments")) return BadRequest("Loại danh sách không hợp lệ.");
                if (detailMode != null && ControllerContext.HttpContext != null && !User.IsInRole("Admin")) return Forbid();
                ExecutionStatusEnum? requestedStatus = null;
                if (!string.IsNullOrWhiteSpace(detailStatus))
                {
                    if (!Enum.TryParse<ExecutionStatusEnum>(detailStatus, true, out var parsedStatus) || !Enum.IsDefined(parsedStatus)) return BadRequest("Trạng thái không hợp lệ.");
                    requestedStatus = ExecutionStatuses.Normalize(parsedStatus);
                }
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Unit)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .Include(i => i.AgencyExecutions)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .AsQueryable();

                if (documentIds is { Length: > 0 })
                {
                    query = query.Where(i => documentIds.Contains(i.DocumentId));
                }
                else if (documentId.HasValue && documentId.Value != Guid.Empty)
                {
                    query = query.Where(i => i.DocumentId == documentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(scope) && !scope.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (scope.Equals("general", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => i.IsGeneralTask || (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_AGENCIES" || i.LeadAgency.Code == "ALL_MINISTRIES" || i.LeadAgency.Code == "ALL_PROVINCES" || i.LeadAgency.Code == "ALL_PROVINCES_UBND" || i.LeadAgency.Code == "ALL_MINISTRIES_DIRECT")));
                    }
                    else if (scope.Equals("specific", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => !i.IsGeneralTask && (i.LeadAgency == null || (i.LeadAgency.Code != "ALL_AGENCIES" && i.LeadAgency.Code != "ALL_MINISTRIES" && i.LeadAgency.Code != "ALL_PROVINCES" && i.LeadAgency.Code != "ALL_PROVINCES_UBND" && i.LeadAgency.Code != "ALL_MINISTRIES_DIRECT")));
                    }
                }

                if (section != null && section.Length > 0)
                {
                    var validSections = section.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    if (validSections.Count > 0)
                    {
                        query = query.Where(i => i.Section != null && validSections.Contains(i.Section));
                    }
                }

                if (group != null && group.Length > 0)
                {
                    var validGroups = group.Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
                    if (validGroups.Count > 0)
                    {
                        query = query.Where(i => i.Group != null && validGroups.Contains(i.Group));
                    }
                }

                bool wantOngoing = isOngoing.HasValue && isOngoing.Value;
                bool hasYearFilter = fromYear.HasValue || toYear.HasValue;

                if (wantOngoing && hasYearFilter)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => i.IsOngoing ||
                        (!i.IsOngoing && (fromYear.HasValue && toYear.HasValue
                            ? (i.DueDate.HasValue ? (i.DueDate.Value.Year >= fY && i.DueDate.Value.Year <= tY) : (i.StartDate.HasValue ? (i.StartDate.Value.Year >= fY && i.StartDate.Value.Year <= tY) : true))
                            : (fromYear.HasValue
                                ? (i.DueDate.HasValue ? i.DueDate.Value.Year >= fY : (i.StartDate.HasValue ? i.StartDate.Value.Year >= fY : true))
                                : (i.DueDate.HasValue ? i.DueDate.Value.Year <= tY : (i.StartDate.HasValue ? i.StartDate.Value.Year <= tY : true))))));
                }
                else if (wantOngoing)
                {
                    query = query.Where(i => i.IsOngoing);
                }
                else if (hasYearFilter)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => !i.IsOngoing &&
                        (fromYear.HasValue && toYear.HasValue
                            ? (i.DueDate.HasValue ? (i.DueDate.Value.Year >= fY && i.DueDate.Value.Year <= tY) : (i.StartDate.HasValue ? (i.StartDate.Value.Year >= fY && i.StartDate.Value.Year <= tY) : true))
                            : (fromYear.HasValue
                                ? (i.DueDate.HasValue ? i.DueDate.Value.Year >= fY : (i.StartDate.HasValue ? i.StartDate.Value.Year >= fY : true))
                                : (i.DueDate.HasValue ? i.DueDate.Value.Year <= tY : (i.StartDate.HasValue ? i.StartDate.Value.Year <= tY : true)))));
                }

                var allAgencies = await _context.Agencies.AsNoTracking().ToListAsync();
                var agencyChildren = allAgencies.ToLookup(a => a.ParentId);
                var allAgenciesEntity = allAgencies.FirstOrDefault(a => a.Code == "ALL_AGENCIES");
                Guid allAgenciesId = allAgenciesEntity?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000009999");

                HashSet<Guid>? allowedAgencyIds = null;
                bool isAgencyFilterActive = agencyId?.Any(id => id != Guid.Empty) == true;
                if (isAgencyFilterActive)
                {
                    allowedAgencyIds = AgencyAssignments.ExpandSelection(agencyId!, allAgencies).ToHashSet();
                    query = AgencyAssignments.Filter(query, allowedAgencyIds, assignmentRole);
                }
                if (parentAgencyId.HasValue && parentAgencyId != Guid.Empty)
                {
                    var childIds = allAgencies.Where(a => a.ParentId == parentAgencyId).Select(a => a.Id).ToList();
                    query = AgencyAssignments.Filter(query, childIds, assignmentRole);
                }

                if (AgencyAssignments.Coordinating(assignmentRole)) query = query.Where(i => i.AgencyExecutions.Any(e => e.IsCoordinatingAssignment));
                query = timeFilter.ApplyExecutionWindow(query);
                var baseItems = await query.ToListAsync();
                timeFilter.RestrictReports(baseItems);

                int totalGoals = baseItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                int totalTasks = baseItems.Count(i => i.ItemType == ItemTypeEnum.Task);

                // Filter by itemType if supplied ('Goal' or 'Task')
                var items = baseItems;
                if (!string.IsNullOrWhiteSpace(itemType) && !itemType.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<ItemTypeEnum>(itemType, true, out var parsedItemType))
                    {
                        items = baseItems.Where(i => i.ItemType == parsedItemType).ToList();
                    }
                }

                var ministriesPerformance = new List<AgencyStatusSummaryDto>();
                var provincesPerformance = new List<AgencyStatusSummaryDto>();
                var othersPerformance = new List<AgencyStatusSummaryDto>();

                var itemsByAgency = items.SelectMany(item => item.AgencyExecutions.Where(e => AgencyAssignments.HasRole(e, assignmentRole)).Select(e => (e.AgencyId, Item: item)))
                    .ToLookup(pair => pair.AgencyId, pair => pair.Item);
                IEnumerable<Agency> targetAgencies = parentAgencyId.HasValue && parentAgencyId != Guid.Empty
                    ? allAgencies.Where(a => a.ParentId == parentAgencyId)
                    : isAgencyFilterActive && allowedAgencyIds != null ? allAgencies.Where(a => agencyId!.Contains(a.Id) || allowedAgencyIds.Contains(a.Id) && !a.ParentId.HasValue)
                    : allAgencies.Where(a => !a.ParentId.HasValue);
                targetAgencies = targetAgencies.Where(a => !ReportingAgencyGroups.IsGroup(a) && (a.IsActive || itemsByAgency.Contains(a.Id)));

                // Drilldown shares the exact filtered item/recipient sets and calculator
                // with the counters below. Group assignments remain separate rows.
                if (detailMode != null)
                {
                    DashboardAgencyItemDto DetailRow(GoalTaskItem item, Agency? recipient)
                    {
                        var recipients = recipient != null ? new List<Guid> { recipient.Id }
                            : allowedAgencyIds == null ? new List<Guid>() : item.AgencyExecutions.Where(e => AgencyAssignments.HasRole(e, assignmentRole) && allowedAgencyIds.Contains(e.AgencyId)).Select(e => e.AgencyId).Distinct().ToList();
                        var report = ProgressCalculator.AnnualReport(item, recipient?.Id, reportYear);
                        var progress = recipients.Count > 0 ? ProgressCalculator.EvaluateRecipients(item, recipients, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear))
                            : IsGeneralItem(item) ? ProgressCalculator.EvaluateOverall(item, allAgencies, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear), reportYear)
                            : ProgressCalculator.Evaluate(item, report, year: reportYear, evaluatedAt: ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear));
                        return new DashboardAgencyItemDto {
                            Id = item.Id, Code = item.Code, Title = item.Title, ItemType = item.ItemType.ToString(),
                            IsGeneralTask = IsGeneralItem(item), LeadAgencyId = recipient?.Id ?? item.LeadAgencyId,
                            LeadAgencyName = recipient != null ? ProvinceCatalog.DisplayName(recipient) : item.LeadAgency?.Name ?? "—",
                            Category = item.Category, Section = item.Section, Group = item.Group,
                            AssignedAgencyId = item.AssignedAgencyId,
                            CoordinatingAgencyIds = item.CoordinatingAgencyIds,
                            CoordinatingAgencyNames = allAgencies.Where(a => item.CoordinatingAgencyIds.Contains(a.Id)).Select(ProvinceCatalog.DisplayName).ToList(),
                            ProgressAgencyId = recipient?.Id, ProgressAgencyName = recipient == null ? null : ProvinceCatalog.DisplayName(recipient),
                            UnitName = item.Unit?.Name, EvaluationType = item.EvaluationType.ToString(),
                            LatestProgressPercent = (double?)progress.Percentage, LatestProgressValue = (double?)progress.ActualValue,
                            CalculatedStatus = progress.Status.ToString(),
                            DeadlineStatus = progress.DeadlineStatus, Status = progress.Status.ToString(),
                            StartDate = item.StartDate, DueDate = item.DueDate, IsOngoing = item.IsOngoing,
                            Deliverables = ProgressCalculator.AnnualDeliverables(item, report)
                        };
                    }
                    IEnumerable<DashboardAgencyItemDto> rows = detailMode == "assignments"
                        ? targetAgencies.SelectMany(a => itemsByAgency[a.Id].DistinctBy(i => i.Id).Select(i => DetailRow(i, a)))
                        : items.Where(i => detailScope == "all" || (detailScope == "general" ? IsGeneralItem(i) : !IsGeneralItem(i))).Select(i => DetailRow(i, null));
                    if (requestedStatus.HasValue) rows = rows.Where(r => r.CalculatedStatus == requestedStatus.Value.ToString());
                    if (!string.IsNullOrWhiteSpace(search)) rows = rows.Where(r => new[] { r.Code, r.Title, r.LeadAgencyName }.Any(t => t?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) == true));
                    var detailRows = rows.OrderBy(r => r.Code).ThenBy(r => r.LeadAgencyName).ThenBy(r => r.Id).ToList();
                    pageNumber = Math.Max(1, pageNumber); pageSize = Math.Clamp(pageSize, 1, 100);
                    return Ok(new PagedResultDto<DashboardAgencyItemDto> {
                        Items = detailRows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                        TotalCount = detailRows.Count, PageNumber = pageNumber, PageSize = pageSize
                    });
                }

                var agencySummaries = new Dictionary<Guid, AgencyStatusSummaryDto>();
                foreach (var agency in targetAgencies)
                {
                    var childIds = GetAgencyAndChildIds(agency.Id, agencyChildren);
                    
                    var agencyItems = itemsByAgency[agency.Id].DistinctBy(i => i.Id).ToList();

                    var goalCounts = new ExecutionStatusCounts();
                    var taskCounts = new ExecutionStatusCounts();
                    foreach (var item in agencyItems)
                    {
                        var result = ProgressCalculator.EvaluateRecipients(item, new[] { agency.Id }, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear));
                        (item.ItemType == ItemTypeEnum.Goal ? goalCounts : taskCounts).Add(result);
                    }
                    var summaryDto = new AgencyStatusSummaryDto
                    {
                        AgencyId = agency.Id, Code = agency.Code, Name = ProvinceCatalog.DisplayName(agency), Type = agency.Type.ToString(),
                        HasChildAgencies = agencyChildren[agency.Id].Any(), ContactPersons = agency.ContactPersons ?? new List<AgencyContactPerson>(),
                        TotalItems = agencyItems.Count,
                        TotalGoals = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Goal),
                        TotalTasks = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Task),
                        Goals = goalCounts, Tasks = taskCounts
                    };

                    agencySummaries[agency.Id] = summaryDto;

                    bool isSpecial = agency.Type == AgencyTypeEnum.Special ||
                                     agency.Code == "ALL_AGENCIES" || agency.Code == "ALL_MINISTRIES" || agency.Code == "ALL_PROVINCES" || agency.Code == "ALL_PROVINCES_UBND" || agency.Code == "ALL_MINISTRIES_DIRECT" ||
                                     (agency.Name != null && (agency.Name.StartsWith("Các bộ, ngành") || agency.Name.StartsWith("Các địa phương") || agency.Name.Contains("UBND tỉnh, thành phố trực thuộc trung ương")));

                    Guid? effectiveParentId = (parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty) ? parentAgencyId.Value : agency.ParentId;

                    if (isSpecial)
                    {
                        // Do not add special agencies to standalone performance lists
                    }
                    else if (effectiveParentId.HasValue && effectiveParentId.Value != Guid.Empty)
                    {
                        var parentAgency = allAgencies.FirstOrDefault(p => p.Id == effectiveParentId.Value);
                        if (parentAgency != null && IsProvinceAgency(parentAgency, allAgencies))
                        {
                            provincesPerformance.Add(summaryDto);
                        }
                        else if (parentAgency != null && IsMinistryAgency(parentAgency, allAgencies))
                        {
                            ministriesPerformance.Add(summaryDto);
                        }
                        else
                        {
                            if (summaryDto.TotalItems > 0 || isAgencyFilterActive)
                            {
                                othersPerformance.Add(summaryDto);
                            }
                        }
                    }
                    else if (IsProvinceAgency(agency, allAgencies))
                    {
                        provincesPerformance.Add(summaryDto);
                    }
                    else if (IsMinistryAgency(agency, allAgencies))
                    {
                        ministriesPerformance.Add(summaryDto);
                    }
                    else
                    {
                        if (summaryDto.TotalItems > 0 || isAgencyFilterActive)
                        {
                            othersPerformance.Add(summaryDto);
                        }
                    }
                }

                ministriesPerformance = ministriesPerformance
                    .OrderByDescending(m => m.TotalItems)
                    .ThenBy(m => m.Name)
                    .ToList();

                provincesPerformance = provincesPerformance
                    .OrderByDescending(p => p.TotalItems)
                    .ThenBy(p => p.Name)
                    .ToList();

                othersPerformance = othersPerformance
                    .OrderByDescending(o => o.TotalItems)
                    .ThenBy(o => o.Name)
                    .ToList();

                // Calculate Unique Created Items Metrics (not expanded by individual agencies/localities)
                bool IsGeneralItem(GoalTaskItem item)
                {
                    if (item.IsGeneralTask) return true;
                    if (item.LeadAgency != null)
                    {
                        string code = item.LeadAgency.Code ?? "";
                        if (code == "ALL_AGENCIES" || code == "ALL_MINISTRIES" || code == "ALL_PROVINCES" || code == "ALL_PROVINCES_UBND" || code == "ALL_MINISTRIES_DIRECT")
                            return true;
                        string name = (item.LeadAgency.Name ?? "").ToLower();
                        if (name.StartsWith("các bộ, ngành") || name.StartsWith("các địa phương") || name.Contains("ubnd tỉnh, thành phố"))
                            return true;
                    }
                    return false;
                }

                int goalsTotal = baseItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                int goalsGeneral = baseItems.Count(i => i.ItemType == ItemTypeEnum.Goal && IsGeneralItem(i));
                int goalsSpecific = goalsTotal - goalsGeneral;

                int tasksTotal = baseItems.Count(i => i.ItemType == ItemTypeEnum.Task);
                int tasksGeneral = baseItems.Count(i => i.ItemType == ItemTypeEnum.Task && IsGeneralItem(i));
                int tasksSpecific = tasksTotal - tasksGeneral;

                var createdGoals = new ExecutionStatusCounts();
                var createdTasks = new ExecutionStatusCounts();
                var overviewGoals = new ExecutionStatusCounts();
                var overviewTasks = new ExecutionStatusCounts();
                var groupCounts = new Dictionary<(ItemTypeEnum Type, string Name), ExecutionStatusCounts>();

                foreach (var item in baseItems)
                {
                    var latestLog = ProgressCalculator.AnnualReport(item, null, reportYear);
                    var selectedRecipients = allowedAgencyIds == null ? new List<Guid>() : item.AgencyExecutions.Where(e => AgencyAssignments.HasRole(e, assignmentRole) && allowedAgencyIds.Contains(e.AgencyId)).Select(e => e.AgencyId).Distinct().ToList();
                    var result = selectedRecipients.Count > 0 ? ProgressCalculator.EvaluateRecipients(item, selectedRecipients, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear))
                        : IsGeneralItem(item) ? ProgressCalculator.EvaluateOverall(item, allAgencies, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear), reportYear)
                        : ProgressCalculator.Evaluate(item, latestLog, year: reportYear, evaluatedAt: ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear));
                    bool isGoal = item.ItemType == ItemTypeEnum.Goal;
                    (isGoal ? overviewGoals : overviewTasks).Add(result);
                    var groupKey = (item.ItemType, item.Group?.Trim() ?? "");
                    if (!groupCounts.TryGetValue(groupKey, out var counts)) groupCounts[groupKey] = counts = new ExecutionStatusCounts();
                    counts.Add(result);
                    if (!IsGeneralItem(item)) (isGoal ? createdGoals : createdTasks).Add(result);
                }

                var createdMetrics = new {
                    totalGoals = goalsTotal, goalsGeneral, goalsSpecific,
                    totalTasks = tasksTotal, tasksGeneral, tasksSpecific,
                    statusSummary = ExecutionStatusCounts.Sum(new[] { createdGoals, createdTasks }).States,
                    goalStatusSummary = createdGoals.States, taskStatusSummary = createdTasks.States
                };

                var goalsSummary = ExecutionStatusCounts.Sum(agencySummaries.Values.Select(s => s.Goals));
                var tasksSummary = ExecutionStatusCounts.Sum(agencySummaries.Values.Select(s => s.Tasks));
                return Ok(new {
                    totalGoals = baseItems.Count(i => i.ItemType == ItemTypeEnum.Goal), completedGoals = goalsSummary.Completed,
                    totalTasks = baseItems.Count(i => i.ItemType == ItemTypeEnum.Task), completedTasks = tasksSummary.Completed,
                    createdMetrics,
                    groupStatistics = groupCounts.OrderBy(g => g.Key.Type).ThenBy(g => g.Key.Name).Select(g => new {
                        itemType = g.Key.Type.ToString(), group = g.Key.Name.Length == 0 ? "Chưa phân nhóm" : g.Key.Name,
                        total = g.Value.NotStarted + g.Value.InProgress + g.Value.Completed,
                        notStarted = g.Value.NotStarted, inProgress = g.Value.InProgress, completed = g.Value.Completed,
                        completionRate = ExecutionStatusCounts.CompletionRate(g.Value.Completed, g.Value.NotStarted + g.Value.InProgress + g.Value.Completed)
                    }).ToList(),
                    overviewMetrics = new {
                        totalGoals = goalsTotal, totalTasks = tasksTotal,
                        goalStatusSummary = overviewGoals.States, taskStatusSummary = overviewTasks.States,
                        statusSummary = ExecutionStatusCounts.Sum(new[] { overviewGoals, overviewTasks }).States
                    },
                    statusSummary = ExecutionStatusCounts.Sum(new[] { goalsSummary, tasksSummary }).States,
                    goalStatusSummary = goalsSummary.States, taskStatusSummary = tasksSummary.States,
                    ministriesPerformance = ministriesPerformance.OrderByDescending(m => m.TotalItems).ToList(),
                    provincesPerformance = provincesPerformance.OrderByDescending(p => p.TotalItems).ToList(),
                    othersPerformance = othersPerformance.OrderByDescending(o => o.TotalItems).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống khi lấy chỉ số Dashboard", details = ex.Message });
            }
        }

        [HttpGet("agency-items")]
        public async Task<IActionResult> GetAgencyItems(
            [FromQuery] Guid agencyId,
            [FromQuery] string[]? section = null,
            [FromQuery] string[]? group = null,
            [FromQuery] int? reportYear = null,
            [FromQuery] int? fromYear = null,
            [FromQuery] int? toYear = null,
            [FromQuery] bool? isOngoing = null,
            [FromQuery] string? itemType = null,
            [FromQuery] string? scope = null,
            [FromQuery] string? q = null,
            [FromQuery] string? status = null,
            [FromQuery] string assignmentRole = "Lead",
            [FromQuery] int? reportFromYear = null, [FromQuery] int? reportToYear = null,
            [FromQuery] DateTime? executionFrom = null, [FromQuery] DateTime? executionTo = null,
            [FromQuery] Guid? documentId = null,
            [FromQuery] Guid[]? documentIds = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                if (reportYear.HasValue && reportYear.Value is < 1900 or > 9999) return BadRequest("Năm báo cáo không hợp lệ.");
                var timeFilter = new DashboardTimeFilter(reportFromYear, reportToYear, executionFrom, executionTo);
                if (timeFilter.ValidationError() is { } timeError) return BadRequest(timeError);
                if (reportYear.HasValue && (reportYear < reportFromYear || reportYear > reportToYear)) return BadRequest("Năm báo cáo nằm ngoài khoảng đã chọn.");
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Unit)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .Include(i => i.AgencyExecutions)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .AsQueryable();

                if (documentIds is { Length: > 0 }) query = query.Where(i => documentIds.Contains(i.DocumentId));
                else if (documentId.HasValue) query = query.Where(i => i.DocumentId == documentId.Value);

                if (!string.IsNullOrWhiteSpace(scope) && !scope.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (scope.Equals("general", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => i.IsGeneralTask || (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_AGENCIES" || i.LeadAgency.Code == "ALL_MINISTRIES" || i.LeadAgency.Code == "ALL_PROVINCES" || i.LeadAgency.Code == "ALL_PROVINCES_UBND" || i.LeadAgency.Code == "ALL_MINISTRIES_DIRECT")));
                    }
                    else if (scope.Equals("specific", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => !i.IsGeneralTask && (i.LeadAgency == null || (i.LeadAgency.Code != "ALL_AGENCIES" && i.LeadAgency.Code != "ALL_MINISTRIES" && i.LeadAgency.Code != "ALL_PROVINCES" && i.LeadAgency.Code != "ALL_PROVINCES_UBND" && i.LeadAgency.Code != "ALL_MINISTRIES_DIRECT")));
                    }
                }

                if (section != null && section.Length > 0)
                {
                    var validSections = section.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    if (validSections.Count > 0)
                    {
                        query = query.Where(i => i.Section != null && validSections.Contains(i.Section));
                    }
                }

                if (group != null && group.Length > 0)
                {
                    var validGroups = group.Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
                    if (validGroups.Count > 0)
                    {
                        query = query.Where(i => i.Group != null && validGroups.Contains(i.Group));
                    }
                }

                bool wantOngoing = isOngoing.HasValue && isOngoing.Value;
                bool hasYearFilter = fromYear.HasValue || toYear.HasValue;

                if (wantOngoing && hasYearFilter)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => i.IsOngoing ||
                        (!i.IsOngoing && (fromYear.HasValue && toYear.HasValue
                            ? (i.DueDate.HasValue ? (i.DueDate.Value.Year >= fY && i.DueDate.Value.Year <= tY) : (i.StartDate.HasValue ? (i.StartDate.Value.Year >= fY && i.StartDate.Value.Year <= tY) : true))
                            : (fromYear.HasValue
                                ? (i.DueDate.HasValue ? i.DueDate.Value.Year >= fY : (i.StartDate.HasValue ? i.StartDate.Value.Year >= fY : true))
                                : (i.DueDate.HasValue ? i.DueDate.Value.Year <= tY : (i.StartDate.HasValue ? i.StartDate.Value.Year <= tY : true))))));
                }
                else if (wantOngoing)
                {
                    query = query.Where(i => i.IsOngoing);
                }
                else if (hasYearFilter)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => !i.IsOngoing &&
                        (fromYear.HasValue && toYear.HasValue
                            ? (i.DueDate.HasValue ? (i.DueDate.Value.Year >= fY && i.DueDate.Value.Year <= tY) : (i.StartDate.HasValue ? (i.StartDate.Value.Year >= fY && i.StartDate.Value.Year <= tY) : true))
                            : (fromYear.HasValue
                                ? (i.DueDate.HasValue ? i.DueDate.Value.Year >= fY : (i.StartDate.HasValue ? i.StartDate.Value.Year >= fY : true))
                                : (i.DueDate.HasValue ? i.DueDate.Value.Year <= tY : (i.StartDate.HasValue ? i.StartDate.Value.Year <= tY : true)))));
                }

                var allAgencies = await _context.Agencies.AsNoTracking().ToListAsync();
                var agencyChildren = allAgencies.ToLookup(a => a.ParentId);
                var allAgenciesEntity = allAgencies.FirstOrDefault(a => a.Code == "ALL_AGENCIES");
                Guid allAgenciesId = allAgenciesEntity?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000009999");

                var scopedIds = agencyId == Guid.Empty ? null : AgencyAssignments.ExpandSelection(new[] { agencyId }, allAgencies);
                if (scopedIds != null) query = AgencyAssignments.Filter(query, scopedIds, assignmentRole);
                if (AgencyAssignments.Coordinating(assignmentRole)) query = query.Where(i => i.AgencyExecutions.Any(e => e.IsCoordinatingAssignment));
                query = timeFilter.ApplyExecutionWindow(query);
                var baseItems = await query.ToListAsync();
                timeFilter.RestrictReports(baseItems);

                if (!string.IsNullOrWhiteSpace(itemType) && !itemType.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<ItemTypeEnum>(itemType, true, out var parsedItemType))
                    {
                        baseItems = baseItems.Where(i => i.ItemType == parsedItemType).ToList();
                    }
                }

                var agencyItems = baseItems;

                var resultList = agencyItems.Select(item =>
                {
                    var contextIds = scopedIds == null ? AgencyAssignments.Coordinating(assignmentRole) ? item.AgencyExecutions.Where(e => e.IsCoordinatingAssignment).Select(e => e.AgencyId).Distinct().ToList() : new List<Guid>() : item.AgencyExecutions.Where(e => AgencyAssignments.HasRole(e, assignmentRole) && scopedIds.Contains(e.AgencyId)).Select(e => e.AgencyId).Distinct().ToList();
                    if (contextIds.Contains(agencyId)) contextIds = new() { agencyId };
                    Guid? contextId = contextIds.Count == 1 ? contextIds[0] : null;
                    var latestLog = ProgressCalculator.AnnualReport(item, contextId, reportYear);
                    var agencyDeliverables = reportYear.HasValue ? ProgressCalculator.AnnualDeliverables(item, latestLog) : PlanningService.GetDeliverablesForAgency(item, contextId);
                    var progress = contextIds.Count > 0 ? ProgressCalculator.EvaluateRecipients(item, contextIds, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear))
                        : item.IsGeneralTask || item.AgencyExecutions.Any(e => e.IsLeadAssignment && e.IsLeadGroupAssignment) ? ProgressCalculator.EvaluateOverall(item, allAgencies, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear), reportYear)
                        : ProgressCalculator.Evaluate(item, latestLog, agencyDeliverables, contextId, reportYear, ProgressCalculator.AnnualEvaluationDate(reportYear ?? reportToYear));
                    var execStatus = progress.Status;
                    double? latestPercent = (double?)progress.Percentage;
                    double? latestValue = (double?)progress.ActualValue;

                    var coordAgencies = item.CoordinatingAgencyIds != null && item.CoordinatingAgencyIds.Count > 0
                        ? allAgencies.Where(a => item.CoordinatingAgencyIds.Contains(a.Id)).ToList()
                        : new List<Agency>();

                    var assignedAgency = item.AssignedAgencyId.HasValue
                        ? allAgencies.FirstOrDefault(a => a.Id == item.AssignedAgencyId.Value)
                        : null;

                    return new DashboardAgencyItemDto
                    {
                        LeadAssignmentAgencyIds = item.AgencyExecutions.Where(e => e.IsLeadAssignment).Select(e => e.AgencyId).ToList(),
                        CoordinatingAssignmentAgencyIds = item.AgencyExecutions.Where(e => e.IsCoordinatingAssignment).Select(e => e.AgencyId).ToList(),
                        ProgressAgencyId = contextId,
                        ProgressAgencyName = contextId.HasValue ? allAgencies.FirstOrDefault(a => a.Id == contextId)?.Name : null,
                        AssignmentKind = contextId.HasValue ? (AgencyAssignments.Coordinating(assignmentRole) ? item.AgencyExecutions.FirstOrDefault(e => e.AgencyId == contextId)?.IsCoordinatingGroupAssignment : item.AgencyExecutions.FirstOrDefault(e => e.AgencyId == contextId)?.IsLeadGroupAssignment) == true ? "Group" : "Direct" : null,
                        Id = item.Id,
                        Code = item.Code,
                        Title = item.Title,
                        ItemType = item.ItemType.ToString(),
                        IsGeneralTask = item.IsGeneralTask,
                        Category = item.Category,
                        Section = item.Section,
                        Group = item.Group,
                        LeadAgencyId = item.LeadAgencyId,
                        LeadAgencyName = (item.LeadAgency == null ? null : ProvinceCatalog.DisplayName(item.LeadAgency)) ?? (item.IsGeneralTask ? "Tất cả đơn vị (Chung)" : "Bộ Khoa học và Công nghệ"),
                        AssignedAgencyId = item.AssignedAgencyId,
                        AssignedAgencyCode = assignedAgency?.Code,
                        AssignedAgencyName = assignedAgency == null ? null : ProvinceCatalog.DisplayName(assignedAgency),
                        CoordinatingAgencyIds = item.CoordinatingAgencyIds ?? new List<Guid>(),
                        CoordinatingAgencyCodes = coordAgencies.Select(a => a.Code).ToList(),
                        CoordinatingAgencyNames = coordAgencies.Select(ProvinceCatalog.DisplayName).ToList(),
                        StartDate = item.StartDate,
                        DueDate = item.DueDate,
                        IsOngoing = item.IsOngoing,
                        UnitName = item.Unit?.Name,
                        EvaluationType = item.EvaluationType.ToString(),
                        LatestProgressPercent = latestPercent,
                        LatestProgressValue = latestValue,
                        LatestProgressNote = latestLog?.SummaryNotes,
                        LatestProgressDate = latestLog?.LogDate,
                        Status = execStatus.ToString(),
                        CalculatedStatus = execStatus.ToString(),
                        DeadlineStatus = progress.DeadlineStatus,
                        Deliverables = agencyDeliverables
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var searchLower = q.Trim().ToLower();
                    resultList = resultList.Where(i =>
                        (i.Code != null && i.Code.ToLower().Contains(searchLower)) ||
                        (i.Title != null && i.Title.ToLower().Contains(searchLower)) ||
                        (i.Category != null && i.Category.ToLower().Contains(searchLower)) ||
                        (i.Section != null && i.Section.ToLower().Contains(searchLower)) ||
                        (i.Group != null && i.Group.ToLower().Contains(searchLower)) ||
                        (i.LeadAgencyName != null && i.LeadAgencyName.ToLower().Contains(searchLower))
                    ).ToList();
                }

                if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    resultList = resultList.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (pageNumber.HasValue || pageSize.HasValue)
                {
                    int pNum = Math.Max(1, pageNumber ?? 1);
                    int pSize = Math.Max(1, pageSize ?? 10);
                    int totalCount = resultList.Count;
                    int totalPages = (int)Math.Ceiling((double)totalCount / pSize);
                    var pagedItems = resultList.Skip((pNum - 1) * pSize).Take(pSize).ToList();

                    return Ok(new
                    {
                        items = pagedItems,
                        totalCount = totalCount,
                        pageNumber = pNum,
                        pageSize = pSize,
                        totalPages = totalPages
                    });
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy danh sách nhiệm vụ của đơn vị", details = ex.Message });
            }
        }

        private static List<Guid> GetAgencyAndChildIds(Guid rootAgencyId, ILookup<Guid?, Agency> agencyChildren)
        {
            var result = new List<Guid> { rootAgencyId };
            var visited = new HashSet<Guid> { rootAgencyId };
            var queue = new Queue<Guid>();
            queue.Enqueue(rootAgencyId);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                var children = agencyChildren[curr].Select(a => a.Id);
                foreach (var childId in children)
                {
                    if (visited.Add(childId))
                    {
                        result.Add(childId);
                        queue.Enqueue(childId);
                    }
                }
            }
            return result;
        }

        private static Agency GetRootParentAgency(Agency ag, List<Agency> allAgencies)
        {
            if (ag == null) return null!;
            var current = ag;
            var visited = new HashSet<Guid> { current.Id };
            while (current.ParentId.HasValue && current.ParentId.Value != Guid.Empty)
            {
                var parent = allAgencies.FirstOrDefault(a => a.Id == current.ParentId.Value);
                if (parent == null || visited.Contains(parent.Id)) break;
                visited.Add(parent.Id);
                current = parent;
            }
            return current;
        }

        private static bool IsMinistryAgency(Agency ag, List<Agency>? allAgencies = null)
        {
            if (ag == null) return false;
            if (ag.Type == AgencyTypeEnum.Ministry) return true;
            if (ag.Type == AgencyTypeEnum.Province || ag.Type == AgencyTypeEnum.Special || ag.Type == AgencyTypeEnum.Other) return false;

            if (allAgencies != null && ag.ParentId.HasValue && ag.ParentId.Value != Guid.Empty)
            {
                var root = GetRootParentAgency(ag, allAgencies);
                if (root != null && root.Id != ag.Id)
                {
                    return IsMinistryAgency(root, null);
                }
            }

            string name = (ag.Name ?? "").Trim().ToLower();
            return name.StartsWith("bộ") || 
                   name.StartsWith("bảo hiểm") || 
                   name.StartsWith("ngân hàng") || 
                   name.StartsWith("văn phòng chính phủ") || 
                   name.StartsWith("thanh tra chính phủ");
        }

        private static bool IsProvinceAgency(Agency ag, List<Agency>? allAgencies = null)
        {
            if (ag == null) return false;
            if (ag.Type == AgencyTypeEnum.Province) return true;
            if (ag.Type == AgencyTypeEnum.Ministry || ag.Type == AgencyTypeEnum.Special || ag.Type == AgencyTypeEnum.Other) return false;

            if (allAgencies != null && ag.ParentId.HasValue && ag.ParentId.Value != Guid.Empty)
            {
                var root = GetRootParentAgency(ag, allAgencies);
                if (root != null && root.Id != ag.Id)
                {
                    return IsProvinceAgency(root, null);
                }
            }

            string name = (ag.Name ?? "").Trim().ToLower();
            return name.StartsWith("ubnd") || 
                   name.StartsWith("tỉnh") || 
                   name.StartsWith("thành phố") || 
                   name.StartsWith("tp.");
        }
    }

    public class DashboardAgencyItemDto
    {
        public List<Guid> LeadAssignmentAgencyIds { get; set; } = new();
        public List<Guid> CoordinatingAssignmentAgencyIds { get; set; } = new();
        public Guid? ProgressAgencyId { get; set; }
        public string? ProgressAgencyName { get; set; }
        public string? AssignmentKind { get; set; }
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Task";
        public bool IsGeneralTask { get; set; }
        public string? Category { get; set; }
        public string? Section { get; set; }
        public string? Group { get; set; }
        public Guid LeadAgencyId { get; set; }
        public string LeadAgencyName { get; set; } = string.Empty;
        public Guid? AssignedAgencyId { get; set; }
        public string? AssignedAgencyCode { get; set; }
        public string? AssignedAgencyName { get; set; }
        public List<Guid> CoordinatingAgencyIds { get; set; } = new List<Guid>();
        public List<string> CoordinatingAgencyCodes { get; set; } = new List<string>();
        public List<string> CoordinatingAgencyNames { get; set; } = new List<string>();
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOngoing { get; set; }
        public string? UnitName { get; set; }
        public string EvaluationType { get; set; } = "Quantitative";
        public double? LatestProgressPercent { get; set; }
        public double? LatestProgressValue { get; set; }
        public string? LatestProgressNote { get; set; }
        public DateTime? LatestProgressDate { get; set; }
        public string Status { get; set; } = "NotStarted";
        public string CalculatedStatus { get; set; } = "NotStarted";
        public string DeadlineStatus { get; set; } = "OnTime";
        public List<TaskDeliverable> Deliverables { get; set; } = new List<TaskDeliverable>();
    }

    public class AgencyStatusSummaryDto
    {
        public Guid AgencyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Ministry";
        public bool HasChildAgencies { get; set; } = false;
        public List<AgencyContactPerson> ContactPersons { get; set; } = new List<AgencyContactPerson>();
        public int TotalItems { get; set; }
        public int TotalGoals { get; set; }
        public int TotalTasks { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public ExecutionStatusCounts Goals { get; set; } = new();
        [System.Text.Json.Serialization.JsonIgnore]
        public ExecutionStatusCounts Tasks { get; set; } = new();
        public int NotStarted => Goals.NotStarted + Tasks.NotStarted;
        public int InProgress => Goals.InProgress + Tasks.InProgress;
        public int Completed => Goals.Completed + Tasks.Completed;
        public int ExpiringSoon => Goals.ExpiringSoon + Tasks.ExpiringSoon;
        public int Overdue => Goals.Overdue + Tasks.Overdue;
        public int OnTimeReports => Goals.OnTimeReports + Tasks.OnTimeReports;
        public int ReportedCount => Goals.ReportedCount + Tasks.ReportedCount;
        public int WarningCount => Goals.WarningCount + Tasks.WarningCount;
        public int GoalNotStarted => Goals.NotStarted;
        public int GoalInProgress => Goals.InProgress;
        public int GoalCompleted => Goals.Completed;
        public int TaskNotStarted => Tasks.NotStarted;
        public int TaskInProgress => Tasks.InProgress;
        public int TaskCompleted => Tasks.Completed;
        public decimal? GoalCompletionRate => CompletionRate(GoalCompleted, TotalGoals);
        public decimal? TaskCompletionRate => CompletionRate(TaskCompleted, TotalTasks);
        public decimal? CompletionRateOverall => CompletionRate(Completed, TotalItems);
        private static decimal? CompletionRate(int completed, int total) => ExecutionStatusCounts.CompletionRate(completed, total);
    }
}
