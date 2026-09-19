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
            [FromQuery] string[]? section = null,
            [FromQuery] string[]? group = null,
            [FromQuery] int? fromYear = null,
            [FromQuery] int? toYear = null,
            [FromQuery] bool? isOngoing = null,
            [FromQuery] string? itemType = null,
            [FromQuery] string? scope = null)
        {
            try
            {
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Unit)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(scope) && !scope.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (scope.Equals("general", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => i.IsGeneralTask || (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES"));
                    }
                    else if (scope.Equals("specific", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => !i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code != "ALL_AGENCIES"));
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

                if (isOngoing.HasValue && isOngoing.Value)
                {
                    query = query.Where(i => i.IsOngoing);
                }
                else if (fromYear.HasValue || toYear.HasValue)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => i.IsOngoing || ((!i.StartDate.HasValue || i.StartDate.Value.Year <= tY) && (!i.DueDate.HasValue || i.DueDate.Value.Year >= fY)));
                }

                var allAgencies = await _context.Agencies.Include(a => a.ChildAgencies).ToListAsync();
                var allAgenciesEntity = allAgencies.FirstOrDefault(a => a.Code == "ALL_AGENCIES");
                Guid allAgenciesId = allAgenciesEntity?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000009999");

                // Scope to specific agency + child agencies if agencyId supplied
                HashSet<Guid>? allowedAgencyIds = null;
                bool isAgencyFilterActive = false;
                if (agencyId != null && agencyId.Length > 0)
                {
                    var validAgencyIds = agencyId.Where(id => id != Guid.Empty).ToList();
                    if (validAgencyIds.Count > 0)
                    {
                        isAgencyFilterActive = true;
                        allowedAgencyIds = new HashSet<Guid>();
                        foreach (var agId in validAgencyIds)
                        {
                            var childs = GetAgencyAndChildIds(agId, allAgencies);
                            foreach (var c in childs) allowedAgencyIds.Add(c);
                        }
                        query = query.Where(i => i.LeadAgencyId == allAgenciesId || i.IsGeneralTask || allowedAgencyIds.Contains(i.LeadAgencyId));
                    }
                }

                var baseItems = await query.ToListAsync();

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

                // 6 Execution Status counters for ALL, GOALS, and TASKS
                int notStarted = 0, inProgressOnTime = 0, inProgressOverdue = 0, completedOnTime = 0, completedOverdue = 0, expiringSoon = 0;
                int gNotStarted = 0, gInProgressOnTime = 0, gInProgressOverdue = 0, gCompletedOnTime = 0, gCompletedOverdue = 0, gExpiringSoon = 0;
                int tNotStarted = 0, tInProgressOnTime = 0, tInProgressOverdue = 0, tCompletedOnTime = 0, tCompletedOverdue = 0, tExpiringSoon = 0;

                int completedGoals = 0;
                int completedTasks = 0;

                var ministriesPerformance = new List<AgencyStatusSummaryDto>();
                var provincesPerformance = new List<AgencyStatusSummaryDto>();

                // Build performance map grouped by Ministry vs Province
                IEnumerable<Agency> targetAgencies;
                if (parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty)
                {
                    targetAgencies = allAgencies.Where(a => a.ParentId == parentAgencyId.Value);
                }
                else if (isAgencyFilterActive && allowedAgencyIds != null)
                {
                    var selectedSet = agencyId!.Where(id => id != Guid.Empty).ToHashSet();
                    targetAgencies = allAgencies.Where(a => selectedSet.Contains(a.Id) || (a.ParentId.HasValue && selectedSet.Contains(a.ParentId.Value)));
                }
                else
                {
                    targetAgencies = allAgencies.Where(a => !a.ParentId.HasValue);
                }

                // Exclude "ALL_AGENCIES" (Các bộ, ngành, địa phương) pseudo-agency from standalone card lists
                targetAgencies = targetAgencies.Where(a => a.Id != allAgenciesId && a.Code != "ALL_AGENCIES");

                var agencySummaries = new Dictionary<Guid, AgencyStatusSummaryDto>();
                foreach (var agency in targetAgencies)
                {
                    var childIds = GetAgencyAndChildIds(agency.Id, allAgencies);
                    
                    var agencyItems = parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty
                        ? items.Where(i => childIds.Contains(i.LeadAgencyId)).ToList()
                        : items.Where(i => 
                            childIds.Contains(i.LeadAgencyId) || 
                            i.LeadAgencyId == allAgenciesId || 
                            (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES") || 
                            i.IsGeneralTask
                          ).ToList();

                    int aNotStarted = 0, aInProgOnTime = 0, aInProgOverdue = 0, aCompOnTime = 0, aCompOverdue = 0, aExpSoon = 0;
                    int aGNotStarted = 0, aGInProgOnTime = 0, aGInProgOverdue = 0, aGCompOnTime = 0, aGCompOverdue = 0, aGExpSoon = 0;
                    int aTNotStarted = 0, aTInProgOnTime = 0, aTInProgOverdue = 0, aTCompOnTime = 0, aTCompOverdue = 0, aTExpSoon = 0;

                    int aGoals = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                    int aTasks = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Task);

                    foreach (var item in agencyItems)
                    {
                        var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                        var status = PlanningService.CalculateExecutionStatus(item, latestLog);
                        bool isGoal = item.ItemType == ItemTypeEnum.Goal;

                        switch (status)
                        {
                            case ExecutionStatusEnum.NotStarted:
                                aNotStarted++;
                                if (isGoal) aGNotStarted++; else aTNotStarted++;
                                break;
                            case ExecutionStatusEnum.InProgressOnTime:
                                aInProgOnTime++;
                                if (isGoal) aGInProgOnTime++; else aTInProgOnTime++;
                                break;
                            case ExecutionStatusEnum.InProgressOverdue:
                                aInProgOverdue++;
                                if (isGoal) aGInProgOverdue++; else aTInProgOverdue++;
                                break;
                            case ExecutionStatusEnum.CompletedOnTime:
                                aCompOnTime++;
                                if (isGoal) aGCompOnTime++; else aTCompOnTime++;
                                break;
                            case ExecutionStatusEnum.CompletedOverdue:
                                aCompOverdue++;
                                if (isGoal) aGCompOverdue++; else aTCompOverdue++;
                                break;
                            case ExecutionStatusEnum.ExpiringSoon:
                                aExpSoon++;
                                if (isGoal) aGExpSoon++; else aTExpSoon++;
                                break;
                        }
                    }

                    var summaryDto = new AgencyStatusSummaryDto
                    {
                        AgencyId = agency.Id,
                        Code = agency.Code,
                        Name = agency.Name,
                        Type = agency.Type.ToString(),
                        HasChildAgencies = agency.ChildAgencies.Any(),
                        ContactPersons = agency.ContactPersons ?? new List<AgencyContactPerson>(),
                        TotalItems = agencyItems.Count,
                        TotalGoals = aGoals,
                        TotalTasks = aTasks,
                        NotStarted = aNotStarted,
                        InProgressOnTime = aInProgOnTime,
                        InProgressOverdue = aInProgOverdue,
                        CompletedOnTime = aCompOnTime,
                        CompletedOverdue = aCompOverdue,
                        ExpiringSoon = aExpSoon,

                        GoalNotStarted = aGNotStarted,
                        GoalInProgressOnTime = aGInProgOnTime,
                        GoalInProgressOverdue = aGInProgOverdue,
                        GoalCompletedOnTime = aGCompOnTime,
                        GoalCompletedOverdue = aGCompOverdue,
                        GoalExpiringSoon = aGExpSoon,

                        TaskNotStarted = aTNotStarted,
                        TaskInProgressOnTime = aTInProgOnTime,
                        TaskInProgressOverdue = aTInProgOverdue,
                        TaskCompletedOnTime = aTCompOnTime,
                        TaskCompletedOverdue = aTCompOverdue,
                        TaskExpiringSoon = aTExpSoon
                    };

                    agencySummaries[agency.Id] = summaryDto;

                    if (parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty)
                    {
                        var parentAgency = allAgencies.FirstOrDefault(p => p.Id == parentAgencyId.Value);
                        if (parentAgency != null && parentAgency.Type == AgencyTypeEnum.Province)
                        {
                            provincesPerformance.Add(summaryDto);
                        }
                        else
                        {
                            ministriesPerformance.Add(summaryDto);
                        }
                    }
                    else if (agency.Type == AgencyTypeEnum.Ministry)
                    {
                        ministriesPerformance.Add(summaryDto);
                    }
                    else if (agency.Type == AgencyTypeEnum.Province)
                    {
                        provincesPerformance.Add(summaryDto);
                    }
                    else
                    {
                        ministriesPerformance.Add(summaryDto);
                    }
                }

                // Priority sort for Ministries: Bộ Khoa học và Công nghệ first, then alphabetical
                bool IsBkhcn(AgencyStatusSummaryDto m)
                {
                    if (string.IsNullOrWhiteSpace(m.Name) && string.IsNullOrWhiteSpace(m.Code)) return false;
                    var name = (m.Name ?? "").ToLower();
                    var code = (m.Code ?? "").ToLower();
                    return code == "bkhcn" || name.Contains("khoa học và công nghệ") || name.Contains("khoa học & công nghệ") || name.Contains("khoa học công nghệ");
                }

                ministriesPerformance = ministriesPerformance
                    .OrderByDescending(m => IsBkhcn(m))
                    .ThenBy(m => m.Name)
                    .ToList();

                // Priority sort for Provinces: Central-governed cities (Thành phố trực thuộc trung ương) first, then alphabetical
                bool IsCentralCity(AgencyStatusSummaryDto p)
                {
                    if (string.IsNullOrWhiteSpace(p.Name) && string.IsNullOrWhiteSpace(p.Code)) return false;
                    var name = (p.Name ?? "").ToLower();
                    var code = (p.Code ?? "").ToLower();
                    return name.Contains("hà nội") || 
                           name.Contains("hồ chí minh") || 
                           name.Contains("hải phòng") || 
                           name.Contains("đà nẵng") || 
                           name.Contains("cần thơ") || 
                           name.StartsWith("thành phố") || 
                           name.StartsWith("tp.") || 
                           name.StartsWith("ubnd tp") || 
                           name.StartsWith("ubnd thành phố") || 
                           code == "tphcm" || code == "hanoi" || code == "haiphong" || code == "danang" || code == "cantho";
                }

                provincesPerformance = provincesPerformance
                    .OrderByDescending(p => IsCentralCity(p))
                    .ThenBy(p => p.Name)
                    .ToList();

                // Global Status counts aggregated across all target agencies so that General Tasks/Goals assigned to ALL_AGENCIES
                // are counted for every agency assigned to them.
                totalGoals = agencySummaries.Values.Sum(s => s.TotalGoals);
                totalTasks = agencySummaries.Values.Sum(s => s.TotalTasks);

                gNotStarted = agencySummaries.Values.Sum(s => s.GoalNotStarted);
                gInProgressOnTime = agencySummaries.Values.Sum(s => s.GoalInProgressOnTime);
                gInProgressOverdue = agencySummaries.Values.Sum(s => s.GoalInProgressOverdue);
                gCompletedOnTime = agencySummaries.Values.Sum(s => s.GoalCompletedOnTime);
                gCompletedOverdue = agencySummaries.Values.Sum(s => s.GoalCompletedOverdue);
                gExpiringSoon = agencySummaries.Values.Sum(s => s.GoalExpiringSoon);

                tNotStarted = agencySummaries.Values.Sum(s => s.TaskNotStarted);
                tInProgressOnTime = agencySummaries.Values.Sum(s => s.TaskInProgressOnTime);
                tInProgressOverdue = agencySummaries.Values.Sum(s => s.TaskInProgressOverdue);
                tCompletedOnTime = agencySummaries.Values.Sum(s => s.TaskCompletedOnTime);
                tCompletedOverdue = agencySummaries.Values.Sum(s => s.TaskCompletedOverdue);
                tExpiringSoon = agencySummaries.Values.Sum(s => s.TaskExpiringSoon);

                completedGoals = gCompletedOnTime + gCompletedOverdue;
                completedTasks = tCompletedOnTime + tCompletedOverdue;

                notStarted = gNotStarted + tNotStarted;
                inProgressOnTime = gInProgressOnTime + tInProgressOnTime;
                inProgressOverdue = gInProgressOverdue + tInProgressOverdue;
                completedOnTime = gCompletedOnTime + tCompletedOnTime;
                completedOverdue = gCompletedOverdue + tCompletedOverdue;
                expiringSoon = gExpiringSoon + tExpiringSoon;

                return Ok(new
                {
                    totalGoals,
                    completedGoals,
                    totalTasks,
                    completedTasks,
                    statusSummary = new
                    {
                        notStarted,
                        inProgressOnTime,
                        inProgressOverdue,
                        completedOnTime,
                        completedOverdue,
                        expiringSoon
                    },
                    goalStatusSummary = new
                    {
                        notStarted = gNotStarted,
                        inProgressOnTime = gInProgressOnTime,
                        inProgressOverdue = gInProgressOverdue,
                        completedOnTime = gCompletedOnTime,
                        completedOverdue = gCompletedOverdue,
                        expiringSoon = gExpiringSoon
                    },
                    taskStatusSummary = new
                    {
                        notStarted = tNotStarted,
                        inProgressOnTime = tInProgressOnTime,
                        inProgressOverdue = tInProgressOverdue,
                        completedOnTime = tCompletedOnTime,
                        completedOverdue = tCompletedOverdue,
                        expiringSoon = tExpiringSoon
                    },
                    ministriesPerformance = ministriesPerformance.OrderByDescending(m => m.TotalItems).ToList(),
                    provincesPerformance = provincesPerformance.OrderByDescending(p => p.TotalItems).ToList()
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
            [FromQuery] int? fromYear = null,
            [FromQuery] int? toYear = null,
            [FromQuery] bool? isOngoing = null,
            [FromQuery] string? itemType = null,
            [FromQuery] string? scope = null,
            [FromQuery] string? q = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Unit)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(scope) && !scope.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (scope.Equals("general", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => i.IsGeneralTask || (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES"));
                    }
                    else if (scope.Equals("specific", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(i => !i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code != "ALL_AGENCIES"));
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

                if (isOngoing.HasValue && isOngoing.Value)
                {
                    query = query.Where(i => i.IsOngoing);
                }
                else if (fromYear.HasValue || toYear.HasValue)
                {
                    int fY = fromYear ?? 2026;
                    int tY = toYear ?? 2030;
                    query = query.Where(i => i.IsOngoing || ((!i.StartDate.HasValue || i.StartDate.Value.Year <= tY) && (!i.DueDate.HasValue || i.DueDate.Value.Year >= fY)));
                }

                var allAgencies = await _context.Agencies.Include(a => a.ChildAgencies).ToListAsync();
                var allAgenciesEntity = allAgencies.FirstOrDefault(a => a.Code == "ALL_AGENCIES");
                Guid allAgenciesId = allAgenciesEntity?.Id ?? Guid.Parse("00000000-0000-0000-0000-000000009999");

                var baseItems = await query.ToListAsync();

                if (!string.IsNullOrWhiteSpace(itemType) && !itemType.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<ItemTypeEnum>(itemType, true, out var parsedItemType))
                    {
                        baseItems = baseItems.Where(i => i.ItemType == parsedItemType).ToList();
                    }
                }

                List<GoalTaskItem> agencyItems;
                if (agencyId != Guid.Empty)
                {
                    var childIds = GetAgencyAndChildIds(agencyId, allAgencies);
                    agencyItems = baseItems.Where(i =>
                        childIds.Contains(i.LeadAgencyId) ||
                        i.LeadAgencyId == allAgenciesId ||
                        (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES") ||
                        i.IsGeneralTask
                    ).ToList();
                }
                else
                {
                    agencyItems = baseItems;
                }

                var resultList = agencyItems.Select(item =>
                {
                    var latestLog = item.ProgressLogs != null ? item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault() : null;
                    var execStatus = PlanningService.CalculateExecutionStatus(item, latestLog);

                    double? latestPercent = null;
                    double? latestValue = null;
                    if (latestLog != null)
                    {
                        latestPercent = (double?)latestLog.CalculatedProgressPercentage;
                        latestValue = (double?)latestLog.QuantitativeValue;
                    }

                    return new DashboardAgencyItemDto
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Title = item.Title,
                        ItemType = item.ItemType.ToString(),
                        IsGeneralTask = item.IsGeneralTask,
                        Category = item.Category,
                        Section = item.Section,
                        Group = item.Group,
                        LeadAgencyId = item.LeadAgencyId,
                        LeadAgencyName = item.LeadAgency?.Name ?? (item.IsGeneralTask ? "Tất cả đơn vị (Chung)" : "Bộ Khoa học và Công nghệ"),
                        StartDate = item.StartDate,
                        DueDate = item.DueDate,
                        IsOngoing = item.IsOngoing,
                        UnitName = item.Unit?.Name,
                        EvaluationType = item.EvaluationType.ToString(),
                        LatestProgressPercent = latestPercent,
                        LatestProgressValue = latestValue,
                        LatestProgressNote = latestLog?.SummaryNotes,
                        LatestProgressDate = latestLog?.LogDate,
                        Status = execStatus.ToString()
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

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy danh sách nhiệm vụ của đơn vị", details = ex.Message });
            }
        }

        private static List<Guid> GetAgencyAndChildIds(Guid rootAgencyId, List<Agency> allAgencies)
        {
            var result = new List<Guid> { rootAgencyId };
            var queue = new Queue<Guid>();
            queue.Enqueue(rootAgencyId);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                var children = allAgencies.Where(a => a.ParentId == curr).Select(a => a.Id);
                foreach (var childId in children)
                {
                    if (!result.Contains(childId))
                    {
                        result.Add(childId);
                        queue.Enqueue(childId);
                    }
                }
            }
            return result;
        }
    }

    public class DashboardAgencyItemDto
    {
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
        public int NotStarted { get; set; }
        public int InProgressOnTime { get; set; }
        public int InProgressOverdue { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedOverdue { get; set; }
        public int ExpiringSoon { get; set; }

        public int GoalNotStarted { get; set; }
        public int GoalInProgressOnTime { get; set; }
        public int GoalInProgressOverdue { get; set; }
        public int GoalCompletedOnTime { get; set; }
        public int GoalCompletedOverdue { get; set; }
        public int GoalExpiringSoon { get; set; }

        public int TaskNotStarted { get; set; }
        public int TaskInProgressOnTime { get; set; }
        public int TaskInProgressOverdue { get; set; }
        public int TaskCompletedOnTime { get; set; }
        public int TaskCompletedOverdue { get; set; }
        public int TaskExpiringSoon { get; set; }
    }
}
