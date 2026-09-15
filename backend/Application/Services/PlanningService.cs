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
        Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId);
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

        public async Task<PlanningGridResponseDto> GetDocumentPlanningGridAsync(Guid documentId)
        {
            var doc = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Quyết định/Văn bản với ID: {documentId}");
            }

            // Build dynamic year range
            int startYear = doc.StartYear ?? 2026;
            int endYear = doc.EndYear ?? 2030;
            var dynamicYears = new List<int>();
            for (int y = startYear; y <= endYear; y++)
            {
                dynamicYears.Add(y);
            }

            var agenciesMap = await _context.Agencies.ToDictionaryAsync(a => a.Id);
            var items = await _context.GoalTaskItems
                .Include(i => i.LeadAgency)
                .Include(i => i.Unit)
                .Include(i => i.Baselines)
                .Include(i => i.ProgressLogs)
                .Where(i => i.DocumentId == documentId)
                .ToListAsync();

            int gCount = 1;
            int tCount = 1;

            var gridItems = items.OrderBy(i => i.CreatedAt).Select(item =>
            {
                string safeCode = item.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(safeCode) || safeCode == "MT-" || safeCode == "NV-" || safeCode.EndsWith("-") || safeCode.Length <= 3)
                {
                    safeCode = item.ItemType == ItemTypeEnum.Goal ? $"MT-{gCount:D2}" : $"NV-{tCount:D2}";
                }
                if (item.ItemType == ItemTypeEnum.Goal) gCount++;
                else tCount++;

                var coordAgencyCodes = item.CoordinatingAgencyIds
                    .Where(id => agenciesMap.ContainsKey(id))
                    .Select(id => agenciesMap[id].Code)
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

                var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();

                return new PlanningGridItemDto
                {
                    TaskId = item.Id,
                    ItemType = item.ItemType.ToString(),
                    Code = safeCode,
                    Title = item.Title,
                    Category = item.Category,
                    LeadAgencyCode = item.LeadAgency?.Code ?? string.Empty,
                    LeadAgencyName = item.LeadAgency?.Name ?? string.Empty,
                    CoordinatingAgencyCodes = coordAgencyCodes,
                    EvaluationType = item.EvaluationType.ToString(),
                    UnitName = item.Unit?.Name ?? "%",
                    CalculationMethod = item.CalculationMethod.ToString(),
                    LatestProgressValue = latestLog?.QuantitativeValue,
                    LatestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    LastUpdated = latestLog?.LogDate,
                    CustomBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    YearlyTargets = yearlyTargets
                };
            }).ToList();

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

        public async Task<bool> UpdateTaskCustomBaselineAsync(Guid taskId, UpdateCustomBaselineDto dto)
        {
            var task = await _context.GoalTaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ/Mục tiêu với ID: {taskId}");
            }

            // Update CustomBaseline JSONB Dictionary
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
    }
}
