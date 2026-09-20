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
                .Include(i => i.SubItems)
                .AsQueryable();

            var items = await query.Where(i => i.DocumentId == doc.Id).ToListAsync();
            if (!items.Any())
            {
                items = await query.ToListAsync();
            }

            int gCount = 1;
            int tCount = 1;

            var itemsMap = items.ToDictionary(i => i.Id);
            var rootItems = items.Where(i => !i.ParentId.HasValue || !itemsMap.ContainsKey(i.ParentId.Value))
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
                    var baseline = item.Baselines.FirstOrDefault(b => b.Year == year && b.Quarter == 0);
                    if (item.EvaluationType == EvaluationTypeEnum.Quantitative)
                    {
                        yearlyTargets[year] = baseline?.TargetQuantity;
                    }
                    else
                    {
                        yearlyTargets[year] = baseline?.TargetQualitativeStatus?.ToString();
                    }
                }

                var latestLog = GetLatestProgressLogForAgency(item, agencyId);
                var agencyDeliverables = GetDeliverablesForAgency(item, agencyId);
                var status = CalculateExecutionStatus(item, latestLog, agencyDeliverables);

                var childDtos = item.SubItems != null && item.SubItems.Any()
                    ? item.SubItems.OrderBy(s => s.CreatedAt).Select(s => MapItemDto(s)).ToList()
                    : new List<PlanningGridItemDto>();

                return new PlanningGridItemDto
                {
                    TaskId = item.Id,
                    ParentId = item.ParentId,
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
                    CoordinatingAgencyIds = item.CoordinatingAgencyIds,
                    CoordinatingAgencyCodes = coordAgencies.Select(a => a.Code).ToList(),
                    CoordinatingAgencyNames = coordAgencies.Select(a => a.Name).ToList(),
                    UnitId = item.UnitId,
                    EvaluationType = item.EvaluationType.ToString(),
                    UnitName = item.Unit?.Name ?? "%",
                    CalculationMethod = item.CalculationMethod.ToString(),
                    LatestProgressValue = latestLog?.QuantitativeValue,
                    LatestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    LastUpdated = latestLog?.LogDate ?? item.LastUpdated ?? item.CreatedAt,
                    CreatedAt = item.CreatedAt,
                    CalculatedStatus = status.ToString(),
                    CustomBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    Deliverables = agencyDeliverables,
                    YearlyTargets = yearlyTargets,
                    SubItems = childDtos
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

        public async Task<GoalTaskItem> CreateGoalTaskItemAsync(CreateGoalTaskItemRequestDto dto)
        {
            // Sub-task date range validation
            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                var parent = await _context.GoalTaskItems.FirstOrDefaultAsync(p => p.Id == dto.ParentId.Value);
                if (parent == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy Mục tiêu / Nhiệm vụ cha.");
                }

                if (dto.StartDate.HasValue && parent.StartDate.HasValue && dto.StartDate.Value < parent.StartDate.Value)
                {
                    throw new InvalidOperationException($"Ngày bắt đầu của nhiệm vụ con ({dto.StartDate.Value:dd/MM/yyyy}) không được trước ngày bắt đầu của nhiệm vụ cha ({parent.StartDate.Value:dd/MM/yyyy}).");
                }

                if (dto.DueDate.HasValue && parent.DueDate.HasValue && dto.DueDate.Value > parent.DueDate.Value)
                {
                    throw new InvalidOperationException($"Ngày hoàn thành của nhiệm vụ con ({dto.DueDate.Value:dd/MM/yyyy}) không được sau ngày hạn chót của nhiệm vụ cha ({parent.DueDate.Value:dd/MM/yyyy}).");
                }
            }

            Enum.TryParse<ItemTypeEnum>(dto.ItemType, true, out var itemType);
            Enum.TryParse<EvaluationTypeEnum>(dto.EvaluationType, true, out var evalType);
            Enum.TryParse<CalculationMethodEnum>(dto.CalculationMethod, true, out var calcMethod);

            // Auto-increment code generation logic
            string code = dto.Code?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code) || code.StartsWith("MT-") || code.StartsWith("NV-") || code.Length <= 4)
            {
                if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
                {
                    var parent = await _context.GoalTaskItems.FirstOrDefaultAsync(p => p.Id == dto.ParentId.Value);
                    if (parent != null)
                    {
                        int subCount = await _context.GoalTaskItems.CountAsync(i => i.ParentId == parent.Id) + 1;
                        code = $"{parent.Code}.{subCount:D2}";
                    }
                }
                else if (itemType == ItemTypeEnum.Goal)
                {
                    var existingCodes = await _context.GoalTaskItems
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal && !i.ParentId.HasValue)
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
                        .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task && !i.ParentId.HasValue)
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
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Goal && !i.ParentId.HasValue)
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
                            .Where(i => i.DocumentId == dto.DocumentId && i.ItemType == ItemTypeEnum.Task && !i.ParentId.HasValue)
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
                ParentId = (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty) ? dto.ParentId : null,
                ItemType = itemType,
                Code = code,
                Title = dto.Title,
                Category = dto.Category ?? "Chính phủ số",
                Section = dto.Section ?? string.Empty,
                Group = dto.Group ?? string.Empty,
                IsOngoing = dto.IsOngoing,
                IsGeneralTask = dto.IsGeneralTask,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
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

            task.CustomBaseline = dto.Milestones ?? new Dictionary<string, string>();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateYearlyTargetAsync(Guid taskId, UpdateYearlyTargetDto dto)
        {
            var task = await _context.GoalTaskItems
                .Include(t => t.Baselines)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            var baseline = task.Baselines.FirstOrDefault(b => b.Year == dto.Year && b.Quarter == 0);
            if (baseline == null)
            {
                baseline = new TargetBaseline
                {
                    Id = Guid.NewGuid(),
                    GoalTaskId = taskId,
                    Year = dto.Year,
                    Quarter = 0
                };
                _context.TargetBaselines.Add(baseline);
            }

            if (dto.TargetQuantity.HasValue)
            {
                baseline.TargetQuantity = dto.TargetQuantity.Value;
            }

            if (!string.IsNullOrEmpty(dto.TargetQualitativeStatus))
            {
                if (Enum.TryParse<TextStatusEnum>(dto.TargetQualitativeStatus, true, out var parsedStatus))
                {
                    baseline.TargetQualitativeStatus = parsedStatus;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public static List<TaskDeliverable> GetDeliverablesForAgency(GoalTaskItem item, Guid? agencyId)
        {
            if (item == null) return new List<TaskDeliverable>();
            var masterList = item.Deliverables ?? new List<TaskDeliverable>();

            if (agencyId.HasValue && agencyId.Value != Guid.Empty && item.AgencyExecutions != null && item.AgencyExecutions.Any())
            {
                var exec = item.AgencyExecutions.FirstOrDefault(e => e.AgencyId == agencyId.Value);
                if (exec != null && exec.Deliverables != null && exec.Deliverables.Count > 0)
                {
                    return exec.Deliverables;
                }
            }

            if (!item.IsGeneralTask || !agencyId.HasValue || agencyId.Value == Guid.Empty)
            {
                return masterList;
            }

            string key = agencyId.Value.ToString().ToLower();
            if (item.AgencyDeliverables != null && item.AgencyDeliverables.TryGetValue(key, out var agencyList) && agencyList != null && agencyList.Count > 0)
            {
                return agencyList;
            }

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
        {
            if (item?.ProgressLogs == null || !item.ProgressLogs.Any()) return null;

            if (item.IsGeneralTask && agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                var agencyLog = item.ProgressLogs
                    .Where(l => l.AgencyId == agencyId.Value)
                    .OrderByDescending(l => l.LogDate)
                    .FirstOrDefault();

                return agencyLog;
            }

            return item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
        }

        public static ExecutionStatusEnum CalculateExecutionStatus(GoalTaskItem item, ProgressLog? latestLog, List<TaskDeliverable>? customDeliverables = null)
        {
            var now = DateTime.UtcNow;
            var deliverables = customDeliverables ?? item.Deliverables;

            if (item.IsOngoing)
            {
                bool isDeliverableCompleted = deliverables != null && deliverables.Any() &&
                    deliverables.All(d => 
                        string.Equals(d.CurrentStatus, "Completed", StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(d.CurrentStatus, "4", StringComparison.OrdinalIgnoreCase));

                if (isDeliverableCompleted || latestLog?.QualitativeStatus == TextStatusEnum.Completed)
                {
                    return ExecutionStatusEnum.CompletedOnTime;
                }
                if (latestLog != null)
                {
                    return ExecutionStatusEnum.InProgressOnTime;
                }
                return ExecutionStatusEnum.NotStarted;
            }

            bool isCompleted = false;
            if (deliverables != null && deliverables.Any())
            {
                isCompleted = deliverables.All(d => 
                    string.Equals(d.CurrentStatus, "Completed", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(d.CurrentStatus, "4", StringComparison.OrdinalIgnoreCase));

                if (!isCompleted)
                {
                    bool hasOverdueDeliverable = deliverables.Any(d => 
                        d.DueDate.HasValue && 
                        now.Date > d.DueDate.Value.Date && 
                        !string.Equals(d.CurrentStatus, "Completed", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(d.CurrentStatus, "4", StringComparison.OrdinalIgnoreCase));

                    if (hasOverdueDeliverable || (item.DueDate.HasValue && now.Date > item.DueDate.Value.Date))
                    {
                        return ExecutionStatusEnum.InProgressOverdue;
                    }
                }
            }
            else if (item.EvaluationType == EvaluationTypeEnum.Quantitative)
            {
                decimal targetVal = 100m;
                if (item.Baselines != null && item.Baselines.Any())
                {
                    var b = item.Baselines.OrderByDescending(x => x.Year).FirstOrDefault();
                    if (b?.TargetQuantity > 0) targetVal = b.TargetQuantity.Value;
                }
                if (latestLog?.QuantitativeValue >= targetVal) isCompleted = true;
            }
            else
            {
                if (latestLog?.QualitativeStatus == TextStatusEnum.Completed)
                {
                    isCompleted = true;
                }
            }

            if (isCompleted)
            {
                if (item.DueDate.HasValue && latestLog != null && latestLog.LogDate.Date > item.DueDate.Value.Date)
                {
                    return ExecutionStatusEnum.CompletedOverdue;
                }
                return ExecutionStatusEnum.CompletedOnTime;
            }

            if (deliverables != null && deliverables.Any())
            {
                bool hasStarted = deliverables.Any(d => 
                    !string.Equals(d.CurrentStatus, "NotStarted", StringComparison.OrdinalIgnoreCase) && 
                    !string.Equals(d.CurrentStatus, "1", StringComparison.OrdinalIgnoreCase));

                if (!hasStarted && (latestLog == null || latestLog.QualitativeStatus == TextStatusEnum.NotStarted))
                {
                    if (item.DueDate.HasValue && now.Date > item.DueDate.Value.Date) return ExecutionStatusEnum.InProgressOverdue;
                    return ExecutionStatusEnum.NotStarted;
                }
            }
            else if (latestLog == null || (latestLog.QuantitativeValue == 0 && (latestLog.QualitativeStatus == null || latestLog.QualitativeStatus == TextStatusEnum.NotStarted)))
            {
                if (item.DueDate.HasValue && now.Date > item.DueDate.Value.Date) return ExecutionStatusEnum.InProgressOverdue;
                return ExecutionStatusEnum.NotStarted;
            }

            if (item.DueDate.HasValue && now.Date > item.DueDate.Value.Date)
            {
                return ExecutionStatusEnum.InProgressOverdue;
            }

            if (item.DueDate.HasValue && now.Date <= item.DueDate.Value.Date)
            {
                var remainingDays = (item.DueDate.Value.Date - now.Date).TotalDays;
                if (item.ParentId.HasValue && item.StartDate.HasValue)
                {
                    var totalDuration = (item.DueDate.Value.Date - item.StartDate.Value.Date).TotalDays;
                    if (totalDuration > 0 && (remainingDays / totalDuration) <= 0.10)
                    {
                        return ExecutionStatusEnum.ExpiringSoon;
                    }
                }
                else
                {
                    if (remainingDays <= 30)
                    {
                        return ExecutionStatusEnum.ExpiringSoon;
                    }
                }
            }

            return ExecutionStatusEnum.InProgressOnTime;
        }
    }
}
