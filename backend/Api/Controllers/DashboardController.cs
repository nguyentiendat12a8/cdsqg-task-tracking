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
                    .Include(i => i.AgencyExecutions)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .AsQueryable();

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

                // Scope to specific agency + child agencies if agencyId supplied
                HashSet<Guid>? allowedAgencyIds = null;
                bool isAgencyFilterActive = false;
                bool anyMinistrySelected = false;
                bool anyProvinceSelected = false;
                bool anySpecialAllSelected = false;

                if (agencyId != null && agencyId.Length > 0)
                {
                    var validAgencyIds = agencyId.Where(id => id != Guid.Empty).ToList();
                    if (validAgencyIds.Count > 0)
                    {
                        isAgencyFilterActive = true;
                        allowedAgencyIds = new HashSet<Guid>();

                        foreach (var agId in validAgencyIds)
                        {
                            var childs = GetAgencyAndChildIds(agId, agencyChildren);
                            foreach (var c in childs) allowedAgencyIds.Add(c);

                            var selectedAg = allAgencies.FirstOrDefault(a => a.Id == agId);
                            if (selectedAg != null)
                            {
                                if (IsMinistryAgency(selectedAg, allAgencies) || selectedAg.Code == "ALL_MINISTRIES" || selectedAg.Code == "ALL_MINISTRIES_DIRECT")
                                    anyMinistrySelected = true;
                                if (IsProvinceAgency(selectedAg, allAgencies) || selectedAg.Code == "ALL_PROVINCES" || selectedAg.Code == "ALL_PROVINCES_UBND")
                                    anyProvinceSelected = true;
                                if (selectedAg.Code == "ALL_AGENCIES" || (selectedAg.Name != null && selectedAg.Name.Contains("Các bộ, ngành, địa phương")))
                                    anySpecialAllSelected = true;

                                if (selectedAg.Type == AgencyTypeEnum.Special || selectedAg.Code == "ALL_AGENCIES" || selectedAg.Code == "ALL_MINISTRIES" || selectedAg.Code == "ALL_PROVINCES" || selectedAg.Code == "ALL_PROVINCES_UBND" || selectedAg.Code == "ALL_MINISTRIES_DIRECT" || (selectedAg.Name != null && (selectedAg.Name.StartsWith("Các bộ, ngành") || selectedAg.Name.StartsWith("Các địa phương") || selectedAg.Name.Contains("UBND tỉnh, thành phố trực thuộc trung ương"))))
                                {
                                    if (selectedAg.Code == "ALL_MINISTRIES" || selectedAg.Code == "ALL_MINISTRIES_DIRECT" || (selectedAg.Name != null && selectedAg.Name.StartsWith("Các bộ, ngành") && !selectedAg.Name.Contains("địa phương")))
                                    {
                                        anyMinistrySelected = true;
                                        var ministryIds = allAgencies.Where(a => IsMinistryAgency(a, allAgencies)).Select(a => a.Id);
                                        foreach (var mId in ministryIds)
                                        {
                                            var cList = GetAgencyAndChildIds(mId, agencyChildren);
                                            foreach (var c in cList) allowedAgencyIds.Add(c);
                                        }
                                    }
                                    else if (selectedAg.Code == "ALL_PROVINCES" || selectedAg.Code == "ALL_PROVINCES_UBND" || (selectedAg.Name != null && (selectedAg.Name.StartsWith("Các địa phương") || selectedAg.Name.Contains("UBND tỉnh"))))
                                    {
                                        anyProvinceSelected = true;
                                        var provinceIds = allAgencies.Where(a => IsProvinceAgency(a, allAgencies)).Select(a => a.Id);
                                        foreach (var pId in provinceIds)
                                        {
                                            var cList = GetAgencyAndChildIds(pId, agencyChildren);
                                            foreach (var c in cList) allowedAgencyIds.Add(c);
                                        }
                                    }
                                    else if (selectedAg.Code == "ALL_AGENCIES" || (selectedAg.Name != null && selectedAg.Name.Contains("Các bộ, ngành, địa phương")))
                                    {
                                        anySpecialAllSelected = true;
                                        foreach (var a in allAgencies) allowedAgencyIds.Add(a.Id);
                                    }
                                }
                            }
                        }

                        query = query.Where(i => 
                            allowedAgencyIds.Contains(i.LeadAgencyId) || 
                            (i.AssignedAgencyId.HasValue && allowedAgencyIds.Contains(i.AssignedAgencyId.Value)) || 
                            (i.AgencyExecutions != null && i.AgencyExecutions.Any(e => allowedAgencyIds.Contains(e.AgencyId) || (e.AssignedAgencyId.HasValue && allowedAgencyIds.Contains(e.AssignedAgencyId.Value)))) ||
                            (anySpecialAllSelected) ||
                            (anyMinistrySelected && (
                                i.LeadAgencyId == allAgenciesId ||
                                (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_AGENCIES" || i.LeadAgency.Code == "ALL_MINISTRIES" || i.LeadAgency.Code == "ALL_MINISTRIES_DIRECT")) ||
                                (i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code == "ALL_AGENCIES"))
                            )) ||
                            (anyProvinceSelected && (
                                i.LeadAgencyId == allAgenciesId ||
                                (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_AGENCIES" || i.LeadAgency.Code == "ALL_PROVINCES" || i.LeadAgency.Code == "ALL_PROVINCES_UBND")) ||
                                (i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code == "ALL_AGENCIES"))
                            ))
                        );
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
                var othersPerformance = new List<AgencyStatusSummaryDto>();

                // Build performance map grouped by Ministry vs Province vs Other
                IEnumerable<Agency> targetAgencies;
                if (parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty)
                {
                    targetAgencies = allAgencies.Where(a => a.ParentId == parentAgencyId.Value);
                }
                else if (isAgencyFilterActive && allowedAgencyIds != null)
                {
                    var selectedSet = agencyId!.Where(id => id != Guid.Empty).ToHashSet();
                    var targetList = new List<Agency>();

                    foreach (var id in selectedSet)
                    {
                        var ag = allAgencies.FirstOrDefault(a => a.Id == id);
                        if (ag == null) continue;

                        bool isSpecial = ag.Type == AgencyTypeEnum.Special ||
                                         ag.Code == "ALL_AGENCIES" || ag.Code == "ALL_MINISTRIES" || ag.Code == "ALL_PROVINCES" || ag.Code == "ALL_PROVINCES_UBND" || ag.Code == "ALL_MINISTRIES_DIRECT" ||
                                         (ag.Name != null && (ag.Name.StartsWith("Các bộ, ngành") || ag.Name.StartsWith("Các địa phương") || ag.Name.Contains("UBND tỉnh, thành phố trực thuộc trung ương")));

                        if (isSpecial)
                        {
                            if (ag.Code == "ALL_MINISTRIES" || ag.Code == "ALL_MINISTRIES_DIRECT" || (ag.Name != null && ag.Name.StartsWith("Các bộ, ngành") && !ag.Name.Contains("địa phương")))
                            {
                                var ministries = allAgencies.Where(a => !a.ParentId.HasValue && IsMinistryAgency(a, allAgencies));
                                foreach (var m in ministries) if (!targetList.Any(t => t.Id == m.Id)) targetList.Add(m);
                            }
                            else if (ag.Code == "ALL_PROVINCES" || ag.Code == "ALL_PROVINCES_UBND" || (ag.Name != null && (ag.Name.StartsWith("Các địa phương") || ag.Name.Contains("UBND tỉnh"))))
                            {
                                var provinces = allAgencies.Where(a => !a.ParentId.HasValue && IsProvinceAgency(a, allAgencies));
                                foreach (var p in provinces) if (!targetList.Any(t => t.Id == p.Id)) targetList.Add(p);
                            }
                            else if (ag.Code == "ALL_AGENCIES" || (ag.Name != null && ag.Name.Contains("Các bộ, ngành, địa phương")))
                            {
                                var topAgencies = allAgencies.Where(a => !a.ParentId.HasValue && a.Type != AgencyTypeEnum.Internal && a.Type != AgencyTypeEnum.Special);
                                foreach (var ta in topAgencies) if (!targetList.Any(t => t.Id == ta.Id)) targetList.Add(ta);
                            }
                        }
                        else
                        {
                            if (!targetList.Any(t => t.Id == ag.Id)) targetList.Add(ag);
                        }
                    }

                    targetAgencies = targetList;
                }
                else
                {
                    targetAgencies = allAgencies.Where(a => !a.ParentId.HasValue && a.Type != AgencyTypeEnum.Internal);
                }

                // Exclude special agencies and pseudo-agencies from standalone performance card lists ONLY when not specifically filtered
                if (!isAgencyFilterActive)
                {
                    targetAgencies = targetAgencies.Where(a => 
                        a.Type != AgencyTypeEnum.Special && 
                        (parentAgencyId.HasValue || a.ParentId.HasValue || a.Type != AgencyTypeEnum.Internal) &&
                        a.Id != allAgenciesId && 
                        a.Code != "ALL_AGENCIES" && 
                        a.Code != "ALL_MINISTRIES" && 
                        a.Code != "ALL_PROVINCES" && 
                        a.Code != "ALL_PROVINCES_UBND" && 
                        a.Code != "ALL_MINISTRIES_DIRECT" &&
                        !(a.Name != null && (a.Name.StartsWith("Các bộ, ngành") || a.Name.StartsWith("Các địa phương") || a.Name.Contains("UBND tỉnh, thành phố trực thuộc trung ương")))
                    );
                }

                var agencySummaries = new Dictionary<Guid, AgencyStatusSummaryDto>();
                foreach (var agency in targetAgencies)
                {
                    var childIds = GetAgencyAndChildIds(agency.Id, agencyChildren);
                    
                    bool includeGeneral = !agency.ParentId.HasValue && (!parentAgencyId.HasValue || parentAgencyId.Value == Guid.Empty);

                    bool isMinistryOrProvince = IsMinistryAgency(agency, allAgencies) || IsProvinceAgency(agency, allAgencies);

                    var agencyItems = includeGeneral
                        ? items.Where(i => 
                            childIds.Contains(i.LeadAgencyId) || 
                            (i.AssignedAgencyId.HasValue && childIds.Contains(i.AssignedAgencyId.Value)) ||
                            (i.AgencyExecutions != null && i.AgencyExecutions.Any(e => childIds.Contains(e.AgencyId) || (e.AssignedAgencyId.HasValue && childIds.Contains(e.AssignedAgencyId.Value)))) ||
                            (isMinistryOrProvince && (
                                i.LeadAgencyId == allAgenciesId || 
                                (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES") ||
                                (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_MINISTRIES" || i.LeadAgency.Code == "ALL_MINISTRIES_DIRECT") && IsMinistryAgency(agency, allAgencies)) ||
                                (i.LeadAgency != null && (i.LeadAgency.Code == "ALL_PROVINCES" || i.LeadAgency.Code == "ALL_PROVINCES_UBND") && IsProvinceAgency(agency, allAgencies)) ||
                                (i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code == "ALL_AGENCIES"))
                            ))
                          ).ToList()
                        : items.Where(i => childIds.Contains(i.LeadAgencyId) || (i.AssignedAgencyId.HasValue && childIds.Contains(i.AssignedAgencyId.Value)) || (i.AgencyExecutions != null && i.AgencyExecutions.Any(e => childIds.Contains(e.AgencyId) || (e.AssignedAgencyId.HasValue && childIds.Contains(e.AssignedAgencyId.Value))))).ToList();

                    int aNotStarted = 0, aInProgOnTime = 0, aInProgOverdue = 0, aCompOnTime = 0, aCompOverdue = 0, aExpSoon = 0;
                    int aGNotStarted = 0, aGInProgOnTime = 0, aGInProgOverdue = 0, aGCompOnTime = 0, aGCompOverdue = 0, aGExpSoon = 0;
                    int aTNotStarted = 0, aTInProgOnTime = 0, aTInProgOverdue = 0, aTCompOnTime = 0, aTCompOverdue = 0, aTExpSoon = 0;

                    int aGoals = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                    int aTasks = agencyItems.Count(i => i.ItemType == ItemTypeEnum.Task);

                    foreach (var item in agencyItems)
                    {
                        var latestLog = PlanningService.GetLatestProgressLogForAgency(item, agency.Id);
                        var agencyDeliverables = PlanningService.GetDeliverablesForAgency(item, agency.Id);
                        var status = PlanningService.CalculateExecutionStatus(item, latestLog, agencyDeliverables);
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
                        HasChildAgencies = agencyChildren[agency.Id].Any(),
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

                if (isAgencyFilterActive)
                {
                    if (anyMinistrySelected && !anyProvinceSelected && !anySpecialAllSelected)
                    {
                        provincesPerformance.Clear();
                        othersPerformance.Clear();
                    }
                    else if (anyProvinceSelected && !anyMinistrySelected && !anySpecialAllSelected)
                    {
                        ministriesPerformance.Clear();
                        othersPerformance.Clear();
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

                int cgNotStarted = 0, cgInProgOnTime = 0, cgInProgOverdue = 0, cgCompOnTime = 0, cgCompOverdue = 0, cgExpSoon = 0;
                int ctNotStarted = 0, ctInProgOnTime = 0, ctInProgOverdue = 0, ctCompOnTime = 0, ctCompOverdue = 0, ctExpSoon = 0;

                foreach (var item in baseItems)
                {
                    if (IsGeneralItem(item)) continue;

                    var latestLog = item.ProgressLogs != null && item.ProgressLogs.Count > 0 
                        ? item.ProgressLogs.OrderByDescending(p => p.LogDate).FirstOrDefault() 
                        : null;
                    var status = PlanningService.CalculateExecutionStatus(item, latestLog, item.Deliverables);
                    bool isGoal = item.ItemType == ItemTypeEnum.Goal;

                    switch (status)
                    {
                        case ExecutionStatusEnum.NotStarted:
                            if (isGoal) cgNotStarted++; else ctNotStarted++;
                            break;
                        case ExecutionStatusEnum.InProgressOnTime:
                            if (isGoal) cgInProgOnTime++; else ctInProgOnTime++;
                            break;
                        case ExecutionStatusEnum.InProgressOverdue:
                            if (isGoal) cgInProgOverdue++; else ctInProgOverdue++;
                            break;
                        case ExecutionStatusEnum.CompletedOnTime:
                            if (isGoal) cgCompOnTime++; else ctCompOnTime++;
                            break;
                        case ExecutionStatusEnum.CompletedOverdue:
                            if (isGoal) cgCompOverdue++; else ctCompOverdue++;
                            break;
                        case ExecutionStatusEnum.ExpiringSoon:
                            if (isGoal) cgExpSoon++; else ctExpSoon++;
                            break;
                    }
                }

                var createdMetrics = new
                {
                    totalGoals = goalsTotal,
                    goalsGeneral,
                    goalsSpecific,
                    totalTasks = tasksTotal,
                    tasksGeneral,
                    tasksSpecific,
                    statusSummary = new
                    {
                        notStarted = cgNotStarted + ctNotStarted,
                        inProgressOnTime = cgInProgOnTime + ctInProgOnTime,
                        inProgressOverdue = cgInProgOverdue + ctInProgOverdue,
                        completedOnTime = cgCompOnTime + ctCompOnTime,
                        completedOverdue = cgCompOverdue + ctCompOverdue,
                        expiringSoon = cgExpSoon + ctExpSoon
                    },
                    goalStatusSummary = new
                    {
                        notStarted = cgNotStarted,
                        inProgressOnTime = cgInProgOnTime,
                        inProgressOverdue = cgInProgOverdue,
                        completedOnTime = cgCompOnTime,
                        completedOverdue = cgCompOverdue,
                        expiringSoon = cgExpSoon
                    },
                    taskStatusSummary = new
                    {
                        notStarted = ctNotStarted,
                        inProgressOnTime = ctInProgOnTime,
                        inProgressOverdue = ctInProgOverdue,
                        completedOnTime = ctCompOnTime,
                        completedOverdue = ctCompOverdue,
                        expiringSoon = ctExpSoon
                    }
                };

                // Global Status counts aggregated across all target agencies so that General Tasks/Goals assigned to ALL_AGENCIES
                // are counted for every agency assigned to them.
                totalGoals = baseItems.Count(i => i.ItemType == ItemTypeEnum.Goal);
                totalTasks = baseItems.Count(i => i.ItemType == ItemTypeEnum.Task);

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
                    createdMetrics,
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
                    .Include(i => i.AgencyExecutions)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .AsQueryable();

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
                    var currentAg = allAgencies.FirstOrDefault(a => a.Id == agencyId);
                    bool includeGeneral = currentAg == null || !currentAg.ParentId.HasValue;
                    var childIds = GetAgencyAndChildIds(agencyId, agencyChildren);

                    if (currentAg != null && (currentAg.Type == AgencyTypeEnum.Special || currentAg.Code == "ALL_AGENCIES" || currentAg.Code == "ALL_MINISTRIES" || currentAg.Code == "ALL_PROVINCES" || currentAg.Code == "ALL_PROVINCES_UBND" || currentAg.Code == "ALL_MINISTRIES_DIRECT"))
                    {
                        if (currentAg.Code == "ALL_MINISTRIES" || currentAg.Code == "ALL_MINISTRIES_DIRECT")
                        {
                            var ministryIds = allAgencies.Where(a => IsMinistryAgency(a, allAgencies)).Select(a => a.Id);
                            foreach (var mId in ministryIds)
                            {
                                var cList = GetAgencyAndChildIds(mId, agencyChildren);
                                foreach (var c in cList) if (!childIds.Contains(c)) childIds.Add(c);
                            }
                        }
                        else if (currentAg.Code == "ALL_PROVINCES" || currentAg.Code == "ALL_PROVINCES_UBND")
                        {
                            var provinceIds = allAgencies.Where(a => IsProvinceAgency(a, allAgencies)).Select(a => a.Id);
                            foreach (var pId in provinceIds)
                            {
                                var cList = GetAgencyAndChildIds(pId, agencyChildren);
                                foreach (var c in cList) if (!childIds.Contains(c)) childIds.Add(c);
                            }
                        }
                        else if (currentAg.Code == "ALL_AGENCIES")
                        {
                            foreach (var a in allAgencies) if (!childIds.Contains(a.Id)) childIds.Add(a.Id);
                        }
                    }

                    bool isMinistryOrProvince = currentAg != null && (IsMinistryAgency(currentAg, allAgencies) || IsProvinceAgency(currentAg, allAgencies) || currentAg.Type == AgencyTypeEnum.Special || currentAg.Code == "ALL_AGENCIES" || currentAg.Code == "ALL_MINISTRIES" || currentAg.Code == "ALL_PROVINCES" || currentAg.Code == "ALL_PROVINCES_UBND" || currentAg.Code == "ALL_MINISTRIES_DIRECT");

                    agencyItems = includeGeneral
                        ? baseItems.Where(i =>
                            childIds.Contains(i.LeadAgencyId) ||
                            (i.AssignedAgencyId.HasValue && childIds.Contains(i.AssignedAgencyId.Value)) ||
                            (i.AgencyExecutions != null && i.AgencyExecutions.Any(e => childIds.Contains(e.AgencyId) || (e.AssignedAgencyId.HasValue && childIds.Contains(e.AssignedAgencyId.Value)))) ||
                            (isMinistryOrProvince && (
                                i.LeadAgencyId == allAgenciesId ||
                                (i.LeadAgency != null && i.LeadAgency.Code == "ALL_AGENCIES") ||
                                (currentAg != null && i.LeadAgency != null && (i.LeadAgency.Code == "ALL_MINISTRIES" || i.LeadAgency.Code == "ALL_MINISTRIES_DIRECT") && IsMinistryAgency(currentAg, allAgencies)) ||
                                (currentAg != null && i.LeadAgency != null && (i.LeadAgency.Code == "ALL_PROVINCES" || i.LeadAgency.Code == "ALL_PROVINCES_UBND") && IsProvinceAgency(currentAg, allAgencies)) ||
                                (i.IsGeneralTask && (i.LeadAgency == null || i.LeadAgency.Code == "ALL_AGENCIES"))
                            ))
                        ).ToList()
                        : baseItems.Where(i => childIds.Contains(i.LeadAgencyId) || (i.AssignedAgencyId.HasValue && childIds.Contains(i.AssignedAgencyId.Value)) || (i.AgencyExecutions != null && i.AgencyExecutions.Any(e => childIds.Contains(e.AgencyId) || (e.AssignedAgencyId.HasValue && childIds.Contains(e.AssignedAgencyId.Value))))).ToList();
                }
                else
                {
                    agencyItems = baseItems;
                }

                var resultList = agencyItems.Select(item =>
                {
                    var latestLog = PlanningService.GetLatestProgressLogForAgency(item, agencyId);
                    var agencyDeliverables = PlanningService.GetDeliverablesForAgency(item, agencyId);
                    var execStatus = PlanningService.CalculateExecutionStatus(item, latestLog, agencyDeliverables);

                    double? latestPercent = null;
                    double? latestValue = null;
                    if (latestLog != null)
                    {
                        latestPercent = (double?)latestLog.CalculatedProgressPercentage;
                        latestValue = (double?)latestLog.QuantitativeValue;
                    }

                    var coordAgencies = item.CoordinatingAgencyIds != null && item.CoordinatingAgencyIds.Count > 0
                        ? allAgencies.Where(a => item.CoordinatingAgencyIds.Contains(a.Id)).ToList()
                        : new List<Agency>();

                    var assignedAgency = item.AssignedAgencyId.HasValue
                        ? allAgencies.FirstOrDefault(a => a.Id == item.AssignedAgencyId.Value)
                        : null;

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
                        AssignedAgencyId = item.AssignedAgencyId,
                        AssignedAgencyCode = assignedAgency?.Code,
                        AssignedAgencyName = assignedAgency?.Name,
                        CoordinatingAgencyIds = item.CoordinatingAgencyIds ?? new List<Guid>(),
                        CoordinatingAgencyCodes = coordAgencies.Select(a => a.Code).ToList(),
                        CoordinatingAgencyNames = coordAgencies.Select(a => a.Name).ToList(),
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
