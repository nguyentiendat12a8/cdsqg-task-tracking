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
            [FromQuery] string? itemType = null)
        {
            try
            {
                var query = _context.GoalTaskItems
                    .Include(i => i.LeadAgency)
                    .Include(i => i.Unit)
                    .Include(i => i.Baselines)
                    .Include(i => i.ProgressLogs)
                    .AsQueryable();

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
                        query = query.Where(i => allowedAgencyIds.Contains(i.LeadAgencyId));
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

                var agencySummaries = new Dictionary<Guid, AgencyStatusSummaryDto>();
                foreach (var agency in targetAgencies)
                {
                    var childIds = GetAgencyAndChildIds(agency.Id, allAgencies);
                    var agencyItems = items.Where(i => childIds.Contains(i.LeadAgencyId)).ToList();

                    int aNotStarted = 0, aInProgOnTime = 0, aInProgOverdue = 0, aCompOnTime = 0, aCompOverdue = 0, aExpSoon = 0;
                    int aGoals = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                    int aTasks = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Task);

                    foreach (var item in agencyItems)
                    {
                        var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                        var status = PlanningService.CalculateExecutionStatus(item, latestLog);

                        switch (status)
                        {
                            case ExecutionStatusEnum.NotStarted: aNotStarted++; break;
                            case ExecutionStatusEnum.InProgressOnTime: aInProgOnTime++; break;
                            case ExecutionStatusEnum.InProgressOverdue: aInProgOverdue++; break;
                            case ExecutionStatusEnum.CompletedOnTime: aCompOnTime++; break;
                            case ExecutionStatusEnum.CompletedOverdue: aCompOverdue++; break;
                            case ExecutionStatusEnum.ExpiringSoon: aExpSoon++; break;
                        }
                    }

                    var summaryDto = new AgencyStatusSummaryDto
                    {
                        AgencyId = agency.Id,
                        Code = agency.Code,
                        Name = agency.Name,
                        Type = agency.Type.ToString(),
                        HasChildAgencies = agency.ChildAgencies.Any(),
                        TotalItems = agencyItems.Count,
                        TotalGoals = aGoals,
                        TotalTasks = aTasks,
                        NotStarted = aNotStarted,
                        InProgressOnTime = aInProgOnTime,
                        InProgressOverdue = aInProgOverdue,
                        CompletedOnTime = aCompOnTime,
                        CompletedOverdue = aCompOverdue,
                        ExpiringSoon = aExpSoon
                    };

                    agencySummaries[agency.Id] = summaryDto;

                    if (agency.Type == AgencyTypeEnum.Ministry)
                    {
                        ministriesPerformance.Add(summaryDto);
                    }
                    else if (agency.Type == AgencyTypeEnum.Province)
                    {
                        provincesPerformance.Add(summaryDto);
                    }
                }

                // Global Status counts with Goal vs Task breakdown
                foreach (var item in items)
                {
                    var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                    var status = PlanningService.CalculateExecutionStatus(item, latestLog);
                    bool isGoal = item.ItemType == ItemTypeEnum.Goal;

                    switch (status)
                    {
                        case ExecutionStatusEnum.NotStarted:
                            notStarted++;
                            if (isGoal) gNotStarted++; else tNotStarted++;
                            break;
                        case ExecutionStatusEnum.InProgressOnTime:
                            inProgressOnTime++;
                            if (isGoal) gInProgressOnTime++; else tInProgressOnTime++;
                            break;
                        case ExecutionStatusEnum.InProgressOverdue:
                            inProgressOverdue++;
                            if (isGoal) gInProgressOverdue++; else tInProgressOverdue++;
                            break;
                        case ExecutionStatusEnum.CompletedOnTime:
                            completedOnTime++;
                            if (isGoal) { completedGoals++; gCompletedOnTime++; } else { completedTasks++; tCompletedOnTime++; }
                            break;
                        case ExecutionStatusEnum.CompletedOverdue:
                            completedOverdue++;
                            if (isGoal) { completedGoals++; gCompletedOverdue++; } else { completedTasks++; tCompletedOverdue++; }
                            break;
                        case ExecutionStatusEnum.ExpiringSoon:
                            expiringSoon++;
                            if (isGoal) gExpiringSoon++; else tExpiringSoon++;
                            break;
                    }
                }

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

        private HashSet<Guid> GetAgencyAndChildIds(Guid rootId, List<Agency> allAgencies)
        {
            var result = new HashSet<Guid> { rootId };
            var queue = new Queue<Guid>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                var children = allAgencies.Where(a => a.ParentId == curr).Select(a => a.Id);
                foreach (var childId in children)
                {
                    if (result.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }
            return result;
        }
    }

    public class AgencyStatusSummaryDto
    {
        public Guid AgencyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Ministry";
        public bool HasChildAgencies { get; set; } = false;
        public int TotalItems { get; set; }
        public int TotalGoals { get; set; }
        public int TotalTasks { get; set; }
        public int NotStarted { get; set; }
        public int InProgressOnTime { get; set; }
        public int InProgressOverdue { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedOverdue { get; set; }
        public int ExpiringSoon { get; set; }
    }
}
