using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Application.Services
{
    public interface IPlanningService
    {
        Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId, Guid? agencyId = null);
        Task<PagedResultDto<PlanningGridItemDto>> GetDocumentItemsPagedAsync(Guid documentId, DocumentItemsQueryDto queryDto);
        Task<GoalTaskItem> CreateGoalTaskItemAsync(CreateGoalTaskItemRequestDto dto);
        Task<bool> UpdateTaskCustomBaselineAsync(Guid taskId, UpdateCustomBaselineDto dto);
        Task<bool> UpdateYearlyTargetAsync(Guid taskId, UpdateYearlyTargetDto dto);
    }

    public class PlanningService : IPlanningService
    {
        private readonly AppDbContext _context;

        public PlanningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId, Guid? agencyId = null)
        {
            var doc = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId)
                ?? await _context.Documents.FirstOrDefaultAsync();

            if (doc == null)
            {
                doc = new Document
                {
                    Id = Guid.Parse("12660000-0000-0000-0000-000000001266"),
                    DocumentNumber = "1266/QĐ-TTg",
                    Name = "Quyết định số 1266/QĐ-TTg ngày 14/07/2026 của Thủ tướng Chính phủ",
                    StartYear = 2026,
                    EndYear = 2030,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Documents.Add(doc);
                await _context.SaveChangesAsync();
            }

            int startYear = doc.StartYear ?? 2026;
            int endYear = doc.EndYear ?? 2030;
            var dynamicYears = new List<int>();
            for (int y = startYear; y <= endYear; y++)
            {
                dynamicYears.Add(y);
            }

            var agenciesMap = await _context.Agencies.ToDictionaryAsync(a => a.Id);
            var query = _context.GoalTaskItems
                .Include(i => i.LeadAgency)
                .Include(i => i.Unit)
                .Include(i => i.Baselines)
                .Include(i => i.ProgressLogs)
                .Include(i => i.AgencyExecutions)
                .AsQueryable();

            var items = await query.Where(i => i.DocumentId == doc.Id).ToListAsync();
            if (!items.Any())
            {
                items = await query.ToListAsync();
            }

            int gCount = 1;
            int tCount = 1;

            var itemsMap = items.ToDictionary(i => i.Id);
            var rootItems = items
                .OrderByDescending(i => i.LastUpdated ?? i.CreatedAt)
                .ThenByDescending(i => i.CreatedAt)
                .ToList();

            PlanningGridItemDto MapItemDto(GoalTaskItem item)
            {
                string safeCode = item.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(safeCode) || safeCode == "MT-" || safeCode == "NV-" || safeCode.EndsWith("-") || safeCode.Length <= 3)
                {
                    safeCode = item.ItemType == ItemTypeEnum.Goal ? $"MT-{gCount:D2}" : $"NV-{tCount:D2}";
                }
                if (item.ItemType == ItemTypeEnum.Goal) gCount++;
                else tCount++;

                var coordAgencies = item.CoordinatingAgencyIds
                    .Where(id => agenciesMap.ContainsKey(id))
                    .Select(id => agenciesMap[id])
                    .ToList();

                var yearlyTargets = new Dictionary<int, object?>();
                foreach (var year in dynamicYears)
                {
                    var baseline = item.Baselines.FirstOrDefault(b => b.Year == year);
                    if (item.EvaluationType == EvaluationTypeEnum.Quantitative)
                    {
                        yearlyTargets[year] = ProgressCalculator.ResolveBaseline(item, year).Target;
                    }
                    else
                    {
                        yearlyTargets[year] = baseline?.TargetQualitativeStatus?.ToString();
                    }
                }

                var latestLog = GetLatestProgressLogForAgency(item, agencyId);
                var agencyDeliverables = GetDeliverablesForAgency(item, agencyId);

                var progress = item.IsGeneralTask && !agencyId.HasValue
                    ? ProgressCalculator.EvaluateOverall(item, agenciesMap.Values)
                    : ProgressCalculator.Evaluate(item, latestLog, agencyDeliverables, agencyId);

                return new PlanningGridItemDto
                {
                    TaskId = item.Id,
                    ItemType = item.ItemType.ToString(),
                    Code = safeCode,
                    Title = item.Title,
                    Category = item.Category,
                    Section = item.Section ?? string.Empty,
                    Group = item.Group ?? string.Empty,
                    IsOngoing = item.IsOngoing,
                    IsGeneralTask = item.IsGeneralTask,
                    StartDate = item.StartDate,
                    DueDate = item.DueDate,
                    LeadAgencyId = item.LeadAgencyId,
                    LeadAgencyCode = item.LeadAgency?.Code ?? string.Empty,
                    LeadAgencyName = item.LeadAgency?.Name ?? string.Empty,
                    AssignedAgencyId = item.AssignedAgencyId,
                    AssignedAgencyCode = item.AssignedAgencyId.HasValue && agenciesMap.ContainsKey(item.AssignedAgencyId.Value)
                        ? agenciesMap[item.AssignedAgencyId.Value].Code
                        : null,
                    AssignedAgencyName = item.AssignedAgencyId.HasValue && agenciesMap.ContainsKey(item.AssignedAgencyId.Value)
                        ? agenciesMap[item.AssignedAgencyId.Value].Name
                        : null,
                    CoordinatingAgencyIds = item.CoordinatingAgencyIds,
                    CoordinatingAgencyCodes = coordAgencies.Select(a => a.Code).ToList(),
                    CoordinatingAgencyNames = coordAgencies.Select(a => a.Name).ToList(),
                    UnitId = item.UnitId,
                    EvaluationType = item.EvaluationType.ToString(),
                    UnitName = item.Unit?.Name ?? "%",
                    CalculationMethod = item.CalculationMethod.ToString(),
                    LatestProgressValue = progress.ActualValue,
                    CompletionPercentage = progress.Percentage,
                    ExpectedBaselineTarget = progress.Target,
                    LatestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    LastUpdated = latestLog?.LogDate ?? item.LastUpdated ?? item.CreatedAt,
                    CreatedAt = item.CreatedAt,
                    HasPendingApproval = item.ProgressLogs != null && item.ProgressLogs.Any(l => l.ApprovalStatus == ApprovalStatusEnum.Pending),
                    CalculatedStatus = progress.Status.ToString(),
                    CalculatedAlert = progress.Alert.ToString(),
                    CustomBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    Deliverables = agencyDeliverables ?? new List<TaskDeliverable>(),
                    YearlyTargets = yearlyTargets
                };
            }

            var gridItems = rootItems.Select(MapItemDto).ToList();

            return new PlanningGridResponseDto
            {
                DocumentId = doc.Id,
                DocumentNumber = doc.DocumentNumber,
                DocumentName = doc.Name,
                TimeResolution = doc.TimeResolution.ToString(),
                StartYear = startYear,
                EndYear = endYear,
                DynamicYears = dynamicYears,
                Items = gridItems
            };
        }

        public async Task<PagedResultDto<PlanningGridItemDto>> GetDocumentItemsPagedAsync(Guid documentId, DocumentItemsQueryDto queryDto)
        {
            var gridResponse = await GetDocumentPlanningGridAsync(documentId, queryDto.AgencyId);
            var list = gridResponse.Items ?? new List<PlanningGridItemDto>();

            // 1. Scoping for non-admin user accounts
            if (!string.Equals(queryDto.UserRole, "Admin", StringComparison.OrdinalIgnoreCase) && queryDto.AgencyId.HasValue && queryDto.AgencyId.Value != Guid.Empty)
            {
                var userAgencyId = queryDto.AgencyId.Value;
                var userAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == userAgencyId);
                var isParentAgency = userAgency == null || !userAgency.ParentId.HasValue || userAgency.ParentId == Guid.Empty;

                var scopedAgencyIds = new List<Guid> { userAgencyId };
                if (isParentAgency)
                {
                    var childIds = await _context.Agencies
                        .Where(a => a.ParentId == userAgencyId)
                        .Select(a => a.Id)
                        .ToListAsync();
                    scopedAgencyIds.AddRange(childIds);
                }

                list = list.Where(i =>
                {
                    bool isGeneral = isParentAgency && (i.IsGeneralTask || (i.LeadAgencyCode != null && i.LeadAgencyCode.ToUpper().StartsWith("ALL_"))) && (i.LeadAgencyCode == null || i.LeadAgencyCode.ToUpper().StartsWith("ALL_"));
                    bool isLead = scopedAgencyIds.Contains(i.LeadAgencyId);
                    bool isAssigned = i.AssignedAgencyId.HasValue && scopedAgencyIds.Contains(i.AssignedAgencyId.Value);
                    bool isCoord = i.CoordinatingAgencyIds != null && i.CoordinatingAgencyIds.Any(id => scopedAgencyIds.Contains(id));
                    return isGeneral || isLead || isAssigned || isCoord;
                }).ToList();
            }

            // 2. ItemType filter (Goal vs Task)
            if (!string.IsNullOrWhiteSpace(queryDto.ItemType))
            {
                var targetType = queryDto.ItemType.Trim();
                if (targetType.Equals("Goal", StringComparison.OrdinalIgnoreCase) || targetType == "1")
                {
                    list = list.Where(i => string.Equals(i.ItemType, "Goal", StringComparison.OrdinalIgnoreCase) || i.ItemType == "1").ToList();
                }
                else if (targetType.Equals("Task", StringComparison.OrdinalIgnoreCase) || targetType == "2")
                {
                    list = list.Where(i => string.Equals(i.ItemType, "Task", StringComparison.OrdinalIgnoreCase) || i.ItemType == "2").ToList();
                }
            }

            // 3. Search query
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim().ToLower();
                list = list.Where(i =>
                    (i.Code != null && i.Code.ToLower().Contains(s)) ||
                    (i.Title != null && i.Title.ToLower().Contains(s)) ||
                    (i.LeadAgencyName != null && i.LeadAgencyName.ToLower().Contains(s)) ||
                    (i.AssignedAgencyName != null && i.AssignedAgencyName.ToLower().Contains(s)) ||
                    (i.CoordinatingAgencyNames != null && i.CoordinatingAgencyNames.Any(c => c.ToLower().Contains(s)))
                ).ToList();
            }

            // 4. Filters
            if (queryDto.SelectedAgencyIds != null && queryDto.SelectedAgencyIds.Count > 0)
            {
                var expandedAgencyIds = new HashSet<Guid>(queryDto.SelectedAgencyIds);
                var ministriesGuid1 = Guid.Parse("00000000-0000-0000-0000-000000009998");
                var ministriesGuid2 = Guid.Parse("00000000-0000-0000-0000-000000009995");
                var provincesGuid1  = Guid.Parse("00000000-0000-0000-0000-000000009997");
                var provincesGuid2  = Guid.Parse("00000000-0000-0000-0000-000000009996");

                if (expandedAgencyIds.Contains(ministriesGuid1) || expandedAgencyIds.Contains(ministriesGuid2))
                {
                    expandedAgencyIds.Add(ministriesGuid1);
                    expandedAgencyIds.Add(ministriesGuid2);
                }
                if (expandedAgencyIds.Contains(provincesGuid1) || expandedAgencyIds.Contains(provincesGuid2))
                {
                    expandedAgencyIds.Add(provincesGuid1);
                    expandedAgencyIds.Add(provincesGuid2);
                }

                list = list.Where(i => expandedAgencyIds.Contains(i.LeadAgencyId)).ToList();
            }

            if (queryDto.SelectedSubAgencyIds != null && queryDto.SelectedSubAgencyIds.Count > 0)
            {
                list = list.Where(i => i.AssignedAgencyId.HasValue && queryDto.SelectedSubAgencyIds.Contains(i.AssignedAgencyId.Value)).ToList();
            }

            if (queryDto.SelectedSections != null && queryDto.SelectedSections.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedSections.Contains(i.Section)).ToList();
            }

            if (queryDto.SelectedGroups != null && queryDto.SelectedGroups.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedGroups.Contains(i.Group)).ToList();
            }

            bool wantOngoing = queryDto.OnlyOngoing;
            bool hasYearFilter = queryDto.FromYear.HasValue || queryDto.ToYear.HasValue;

            if (wantOngoing && hasYearFilter)
            {
                int fYr = queryDto.FromYear ?? 2026;
                int tYr = queryDto.ToYear ?? 2030;
                list = list.Where(i =>
                {
                    if (i.IsOngoing) return true;
                    int targetYear = i.DueDate.HasValue ? i.DueDate.Value.Year : (i.StartDate.HasValue ? i.StartDate.Value.Year : 2026);
                    if (queryDto.FromYear.HasValue && targetYear < fYr) return false;
                    if (queryDto.ToYear.HasValue && targetYear > tYr) return false;
                    return true;
                }).ToList();
            }
            else if (wantOngoing)
            {
                list = list.Where(i => i.IsOngoing).ToList();
            }
            else if (hasYearFilter)
            {
                int fYr = queryDto.FromYear ?? 2026;
                int tYr = queryDto.ToYear ?? 2030;
                list = list.Where(i =>
                {
                    if (i.IsOngoing) return false;
                    int targetYear = i.DueDate.HasValue ? i.DueDate.Value.Year : (i.StartDate.HasValue ? i.StartDate.Value.Year : 2026);
                    if (queryDto.FromYear.HasValue && targetYear < fYr) return false;
                    if (queryDto.ToYear.HasValue && targetYear > tYr) return false;
                    return true;
                }).ToList();
            }

            if (queryDto.SelectedScopes != null && queryDto.SelectedScopes.Count > 0)
            {
                list = list.Where(i =>
                {
                    if (queryDto.SelectedScopes.Contains("general") && i.IsGeneralTask) return true;
                    if (queryDto.SelectedScopes.Contains("specific") && !i.IsGeneralTask) return true;
                    return false;
                }).ToList();
            }

            if (queryDto.SelectedStatuses != null && queryDto.SelectedStatuses.Count > 0)
            {
                list = list.Where(i => queryDto.SelectedStatuses.Contains(i.CalculatedStatus)).ToList();
            }

            // 5. Column Sorting
            bool isAsc = string.Equals(queryDto.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);
            string sortByKey = (queryDto.SortBy ?? "code").ToLower();

            int GetStatusRank(string? status) => status switch
            {
                "NotStarted" => 1,
                "InProgressOnTime" => 2,
                "InProgressOverdue" => 3,
                "ExpiringSoon" => 4,
                "CompletedOnTime" => 5,
                "CompletedOverdue" => 6,
                _ => 0
            };

            switch (sortByKey)
            {
                case "code":
                    list = isAsc
                        ? list.OrderBy(i => i.Code, StringComparer.OrdinalIgnoreCase).ToList()
                        : list.OrderByDescending(i => i.Code, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "title":
                    list = isAsc
                        ? list.OrderBy(i => i.Title).ToList()
                        : list.OrderByDescending(i => i.Title).ToList();
                    break;
                case "leadagency":
                case "leadagencyname":
                    list = isAsc
                        ? list.OrderBy(i => i.LeadAgencyName).ToList()
                        : list.OrderByDescending(i => i.LeadAgencyName).ToList();
                    break;
                case "assignedagency":
                case "assignedagencyname":
                    list = isAsc
                        ? list.OrderBy(i => i.AssignedAgencyName ?? "").ToList()
                        : list.OrderByDescending(i => i.AssignedAgencyName ?? "").ToList();
                    break;
                case "coordinatingagency":
                    list = isAsc
                        ? list.OrderBy(i => i.CoordinatingAgencyNames?.FirstOrDefault() ?? "").ToList()
                        : list.OrderByDescending(i => i.CoordinatingAgencyNames?.FirstOrDefault() ?? "").ToList();
                    break;
                case "period":
                    list = isAsc
                        ? list.OrderByDescending(i => i.IsOngoing).ThenBy(i => i.StartDate).ThenBy(i => i.DueDate).ToList()
                        : list.OrderBy(i => i.IsOngoing).ThenByDescending(i => i.DueDate).ThenByDescending(i => i.StartDate).ToList();
                    break;
                case "progress":
                    list = isAsc
                        ? list.OrderBy(i => i.LatestProgressValue ?? -1).ToList()
                        : list.OrderByDescending(i => i.LatestProgressValue ?? -1).ToList();
                    break;
                case "status":
                    list = isAsc
                        ? list.OrderBy(i => GetStatusRank(i.CalculatedStatus)).ToList()
                        : list.OrderByDescending(i => GetStatusRank(i.CalculatedStatus)).ToList();
                    break;
                case "lastupdated":
                case "createdat":
                default:
                    list = isAsc
                        ? list.OrderBy(i => i.LastUpdated ?? i.CreatedAt).ThenBy(i => i.Code).ToList()
                        : list.OrderByDescending(i => i.LastUpdated ?? i.CreatedAt).ThenBy(i => i.Code).ToList();
                    break;
            }

            // 6. Server-side Pagination
            int totalCount = list.Count;
            int pageNum = queryDto.PageNumber > 0 ? queryDto.PageNumber : 1;
            int pageSize = queryDto.PageSize > 0 ? queryDto.PageSize : 10;
            var pagedItems = list.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultDto<PlanningGridItemDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNum,
                PageSize = pageSize
            };
        }

        public async Task<GoalTaskItem> CreateGoalTaskItemAsync(CreateGoalTaskItemRequestDto dto)
        {
            Enum.TryParse<ItemTypeEnum>(dto.ItemType, true, out var itemType);
            Enum.TryParse<EvaluationTypeEnum>(dto.EvaluationType, true, out var evalType);
            Enum.TryParse<CalculationMethodEnum>(dto.CalculationMethod, true, out var calcMethod);

            // Auto-increment code generation logic
            string code = dto.Code?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code) || code.StartsWith("MT-") || code.StartsWith("NV-") || code.Length <= 4)
            {
                if (itemType == ItemTypeEnum.Goal)
                {
                    var existingCodes = await _context.GoalTaskItems
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal)
                        .Select(i => i.Code)
                        .ToListAsync();

                    int maxNum = 0;
                    foreach (var c in existingCodes)
                    {
                        if (!string.IsNullOrEmpty(c) && c.StartsWith("MT-"))
                        {
                            if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                        }
                    }
                    code = $"MT-{(maxNum + 1):D2}";
                }
                else
                {
                    var existingCodes = await _context.GoalTaskItems
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task)
                        .Select(i => i.Code)
                        .ToListAsync();

                    int maxNum = 0;
                    foreach (var c in existingCodes)
                    {
                        if (!string.IsNullOrEmpty(c) && c.StartsWith("NV-"))
                        {
                            if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                        }
                    }
                    code = $"NV-{(maxNum + 1):D2}";
                }
            }
            else
            {
                // Check if user provided code is already used by another item
                bool exists = await _context.GoalTaskItems.AnyAsync(i => i.DocumentId == dto.DocumentId && i.Code == code);
                if (exists)
                {
                    if (itemType == ItemTypeEnum.Goal)
                    {
                        var existingCodes = await _context.GoalTaskItems
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal)
                            .Select(i => i.Code)
                            .ToListAsync();
                        int maxNum = 0;
                        foreach (var c in existingCodes)
                        {
                            if (!string.IsNullOrEmpty(c) && c.StartsWith("MT-"))
                            {
                                if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                            }
                        }
                        code = $"MT-{(maxNum + 1):D2}";
                    }
                    else
                    {
                        var existingCodes = await _context.GoalTaskItems
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task)
                            .Select(i => i.Code)
                            .ToListAsync();
                        int maxNum = 0;
                        foreach (var c in existingCodes)
                        {
                            if (!string.IsNullOrEmpty(c) && c.StartsWith("NV-"))
                            {
                                if (int.TryParse(c.Substring(3), out int num) && num > maxNum) maxNum = num;
                            }
                        }
                        code = $"NV-{(maxNum + 1):D2}";
                    }
                }
            }

            Guid? unitIdToAssign = dto.UnitId;
            if (!unitIdToAssign.HasValue && !string.IsNullOrWhiteSpace(dto.UnitName))
            {
                var targetUnit = await _context.Units.FirstOrDefaultAsync(u => u.Name == dto.UnitName || (dto.UnitName == "%" && u.Code == "PERCENT"));
                if (targetUnit != null) unitIdToAssign = targetUnit.Id;
            }

            var item = new GoalTaskItem
            {
                Id = Guid.NewGuid(),
                DocumentId = dto.DocumentId,
                ItemType = itemType,
                Code = code,
                Title = dto.Title,
                Category = dto.Category ?? "Chính phủ số",
                Section = dto.Section ?? string.Empty,
                Group = dto.Group ?? string.Empty,
                IsOngoing = dto.IsOngoing,
                IsGeneralTask = dto.IsGeneralTask,
                StartDate = dto.IsOngoing ? (dto.StartDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) : dto.StartDate,
                DueDate = dto.IsOngoing ? (dto.DueDate ?? new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc)) : dto.DueDate,
                LeadAgencyId = dto.LeadAgencyId,
                CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>(),
                UnitId = unitIdToAssign,
                EvaluationType = evalType,
                CalculationMethod = calcMethod,
                CreatedAt = DateTime.UtcNow
            };

            _context.GoalTaskItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateTaskCustomBaselineAsync(Guid taskId, UpdateCustomBaselineDto dto)
        {
            var task = await _context.GoalTaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            ProgressCalculator.ValidateAnnualBaselines(dto.Milestones);
            task.CustomBaseline = dto.Milestones ?? new Dictionary<string, string>();
            await ProgressCalculator.RefreshCachesAsync(_context, task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateYearlyTargetAsync(Guid taskId, UpdateYearlyTargetDto dto)
        {
            if (dto.Year is < 1900 or > 9999 || dto.TargetQuantity <= 0)
                throw new InvalidOperationException("Năm không hợp lệ hoặc chỉ tiêu không lớn hơn 0.");
            var task = await _context.GoalTaskItems
                .Include(t => t.Baselines)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            var baseline = task.Baselines.FirstOrDefault(b => b.Year == dto.Year);
            if (baseline == null)
            {
                baseline = new TargetBaseline
                {
                    Id = Guid.NewGuid(),
                    GoalTaskId = taskId,
                    Year = dto.Year
                };
                _context.TargetBaselines.Add(baseline);
            }

            baseline.TargetQuantity = dto.TargetQuantity;
            // Editing the annual grid explicitly replaces that year's custom override.
            var overrides = new Dictionary<string, string>(task.CustomBaseline);
            overrides.Remove(dto.Year.ToString(System.Globalization.CultureInfo.InvariantCulture));
            task.CustomBaseline = overrides;

            if (!string.IsNullOrEmpty(dto.TargetQualitativeStatus))
            {
                if (Enum.TryParse<TextStatusEnum>(dto.TargetQualitativeStatus, true, out var parsedStatus))
                {
                    baseline.TargetQualitativeStatus = parsedStatus;
                }
            }

            await ProgressCalculator.RefreshCachesAsync(_context, task);
            await _context.SaveChangesAsync();
            return true;
        }

        public static List<TaskDeliverable> GetDeliverablesForAgency(GoalTaskItem item, Guid? agencyId)
        {
            if (item == null) return new List<TaskDeliverable>();
            var masterList = item.Deliverables ?? new List<TaskDeliverable>();

            var approved = ProgressCalculator.LatestApproved(item, agencyId);
            if (approved?.Deliverables is { Count: > 0 }) return approved.Deliverables;
            if (!item.IsGeneralTask) return masterList;

            return masterList.Select(d => new TaskDeliverable
            {
                Id = d.Id,
                Title = d.Title,
                DueDate = d.DueDate,
                CurrentStatus = "NotStarted",
                DocumentNumber = null,
                PromulgationDate = null,
                AttachmentUrl = null,
                AttachmentName = null
            }).ToList();
        }

        public static ProgressLog? GetLatestProgressLogForAgency(GoalTaskItem item, Guid? agencyId)
            => ProgressCalculator.LatestApproved(item, agencyId);

        public static ExecutionStatusEnum CalculateExecutionStatus(GoalTaskItem item, ProgressLog? latestLog, List<TaskDeliverable>? customDeliverables = null)
            => ProgressCalculator.Evaluate(item, latestLog, customDeliverables, latestLog?.AgencyId).Status;
    }
}
