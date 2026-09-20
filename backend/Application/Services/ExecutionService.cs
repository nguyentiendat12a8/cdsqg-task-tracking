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
    public interface IExecutionService
    {
        Task<SubmitProgressResponseDto> SubmitProgressAsync(Guid taskId, SubmitProgressRequestDto dto);
        Task<GetProgressLogResponseDto?> GetProgressLogAsync(Guid taskId, int year, int quarter, Guid? agencyId = null);
        Task<List<GetProgressLogResponseDto>> GetTaskProgressHistoryAsync(Guid taskId, Guid? agencyId = null);
        Task<TaskUrgeLogResponseDto> CreateUrgeLogAsync(CreateTaskUrgeLogDto dto);
        Task<List<TaskUrgeLogResponseDto>> GetTaskUrgeHistoryAsync(Guid taskId);
        Task<List<TaskUrgeLogResponseDto>> GetAllUrgeLogsAsync();
        Task<TaskUrgeLogResponseDto?> GetUrgeLogByIdAsync(Guid logId);
    }

    public class ExecutionService : IExecutionService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public ExecutionService(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<SubmitProgressResponseDto> SubmitProgressAsync(Guid taskId, SubmitProgressRequestDto dto)
        {
            var task = await _context.GoalTaskItems
                .Include(t => t.LeadAgency)
                .Include(t => t.Baselines)
                .Include(t => t.ProgressLogs)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy Nhiệm vụ với ID: {taskId}");
            }

            // 1. Resolve Target Baseline (Custom JSONB Override vs Linear Target Baseline)
            string periodKey = dto.PeriodKey; // e.g. "Q1_2026" or "2026_Q1"
            decimal expectedTarget = 100m;
            bool isCustomUsed = false;

            string altKey1 = $"Q{dto.PeriodQuarter}_{dto.PeriodYear}";
            string altKey2 = $"{dto.PeriodYear}_Q{dto.PeriodQuarter}";

            if (task.CustomBaseline != null)
            {
                string? strVal = null;
                if ((task.CustomBaseline.TryGetValue(periodKey, out strVal) ||
                     task.CustomBaseline.TryGetValue(altKey1, out strVal) ||
                     task.CustomBaseline.TryGetValue(altKey2, out strVal)) &&
                    decimal.TryParse(strVal, out decimal customVal))
                {
                    expectedTarget = customVal;
                    isCustomUsed = true;
                }
            }
            else
            {
                var linearBaseline = task.Baselines
                    .FirstOrDefault(b => b.Year == dto.PeriodYear && b.Quarter == dto.PeriodQuarter);

                if (linearBaseline?.TargetQuantity.HasValue == true)
                {
                    expectedTarget = linearBaseline.TargetQuantity.Value;
                }
                else
                {
                    // Fallback to linear quarterly distribution of annual target
                    var yearlyBaseline = task.Baselines.FirstOrDefault(b => b.Year == dto.PeriodYear && b.Quarter == 0);
                    if (yearlyBaseline?.TargetQuantity.HasValue == true && yearlyBaseline.TargetQuantity.Value > 0)
                    {
                        decimal yearlyTarget = yearlyBaseline.TargetQuantity.Value;
                        expectedTarget = dto.PeriodQuarter > 0 ? (yearlyTarget / 4.0m) * dto.PeriodQuarter : yearlyTarget;
                    }
                }
            }

            // 2. Handle Calculation Method (Cumulative vs LatestValue)
            decimal actualCalculatedVal = dto.Value ?? 0m;
            if (task.EvaluationType == EvaluationTypeEnum.Quantitative && dto.Value.HasValue)
            {
                if (task.CalculationMethod == CalculationMethodEnum.Cumulative)
                {
                    decimal previousSum = task.ProgressLogs
                        .Where(l => l.PeriodYear == dto.PeriodYear && l.PeriodQuarter < dto.PeriodQuarter)
                        .Sum(l => l.QuantitativeValue ?? 0m);

                    actualCalculatedVal = previousSum + dto.Value.Value;
                }
                else
                {
                    actualCalculatedVal = dto.Value.Value;
                }
            }

            // 3. Traffic Light Alert Calculation Engine
            AlertStatusEnum alertStatus = AlertStatusEnum.Green;
            decimal completionPercentage = 0m;

            if (task.EvaluationType == EvaluationTypeEnum.Quantitative)
            {
                decimal divisor = expectedTarget <= 0 ? 100m : expectedTarget;
                completionPercentage = (actualCalculatedVal / divisor) * 100m;

                if (completionPercentage >= 95m)
                    alertStatus = AlertStatusEnum.Green;
                else if (completionPercentage >= 70m)
                    alertStatus = AlertStatusEnum.Yellow;
                else
                    alertStatus = AlertStatusEnum.Red;
            }
            else
            {
                // Qualitative Text Status
                TextStatusEnum statusVal = dto.Status ?? TextStatusEnum.NotStarted;
                completionPercentage = statusVal switch
                {
                    TextStatusEnum.NotStarted => 0m,
                    TextStatusEnum.Drafting => 35m,
                    TextStatusEnum.Reviewing => 70m,
                    TextStatusEnum.Completed => 100m,
                    _ => 0m
                };

                alertStatus = statusVal == TextStatusEnum.Completed ? AlertStatusEnum.Green :
                              (statusVal == TextStatusEnum.Reviewing || statusVal == TextStatusEnum.Drafting) ? AlertStatusEnum.Yellow : AlertStatusEnum.Red;
            }

            // 4. Evidence File Storage
            List<string> uploadedUrls = new List<string>();

            // Retain existing files passed from client
            if (dto.ExistingFiles != null && dto.ExistingFiles.Count > 0)
            {
                foreach (var existingUrl in dto.ExistingFiles)
                {
                    if (!string.IsNullOrWhiteSpace(existingUrl) && !uploadedUrls.Contains(existingUrl.Trim()))
                    {
                        uploadedUrls.Add(existingUrl.Trim());
                    }
                }
            }

            if (dto.EvidenceFiles != null && dto.EvidenceFiles.Count > 0)
            {
                foreach (var file in dto.EvidenceFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var url = await _fileStorageService.SaveEvidenceFileAsync(file);
                        if (!string.IsNullOrEmpty(url) && !uploadedUrls.Contains(url))
                        {
                            uploadedUrls.Add(url);
                        }
                    }
                }
            }
            else if (dto.EvidenceFile != null && dto.EvidenceFile.Length > 0)
            {
                var url = await _fileStorageService.SaveEvidenceFileAsync(dto.EvidenceFile);
                if (!string.IsNullOrEmpty(url) && !uploadedUrls.Contains(url))
                {
                    uploadedUrls.Add(url);
                }
            }

            string primaryFileUrl = uploadedUrls.FirstOrDefault() ?? string.Empty;

            Guid? reportingAgencyId = dto.AgencyId;
            if (!reportingAgencyId.HasValue || reportingAgencyId.Value == Guid.Empty)
            {
                reportingAgencyId = task.LeadAgencyId;
            }

            if (dto.Deliverables != null && dto.Deliverables.Count > 0)
            {
                if (task.IsGeneralTask && reportingAgencyId.HasValue && reportingAgencyId.Value != Guid.Empty)
                {
                    string key = reportingAgencyId.Value.ToString().ToLower();
                    task.AgencyDeliverables ??= new Dictionary<string, List<TaskDeliverable>>();
                    task.AgencyDeliverables[key] = dto.Deliverables;
                    _context.Entry(task).Property(t => t.AgencyDeliverables).IsModified = true;
                }
                else
                {
                    task.Deliverables = dto.Deliverables;
                    _context.Entry(task).Property(t => t.Deliverables).IsModified = true;
                }
            }

            string createdBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) && dto.CreatedBy != "Chuyên viên theo dõi" && dto.CreatedBy != "System User"
                ? dto.CreatedBy
                : (task.LeadAgency?.Name ?? "Đơn vị chủ trì");

            // 5. Save Progress Log to Database
            var progressLog = new ProgressLog
            {
                GoalTaskId = task.Id,
                AgencyId = reportingAgencyId,
                PeriodYear = dto.PeriodYear,
                PeriodQuarter = dto.PeriodQuarter,
                LogDate = DateTime.UtcNow,
                QuantitativeValue = dto.Value,
                QualitativeStatus = dto.Status,
                SummaryNotes = dto.SummaryNotes ?? string.Empty,
                CalculatedProgressPercentage = completionPercentage,
                AttachmentFileUrls = uploadedUrls ?? new List<string>(),
                Deliverables = dto.Deliverables ?? new List<TaskDeliverable>(),
                CalculatedAlert = alertStatus,
                CreatedBy = createdBy
            };

            _context.ProgressLogs.Add(progressLog);

            // 6. Update or create AgencyTaskExecution for clean 1-to-1 or 1-to-N tracking per agency
            if (reportingAgencyId.HasValue && reportingAgencyId.Value != Guid.Empty)
            {
                var execution = await _context.AgencyTaskExecutions
                    .FirstOrDefaultAsync(e => e.GoalTaskId == task.Id && e.AgencyId == reportingAgencyId.Value);

                bool isNewExecution = false;
                if (execution == null)
                {
                    execution = new AgencyTaskExecution
                    {
                        GoalTaskId = task.Id,
                        AgencyId = reportingAgencyId.Value,
                        Deliverables = dto.Deliverables ?? new List<TaskDeliverable>(),
                        AttachmentFileUrls = uploadedUrls ?? new List<string>()
                    };
                    _context.AgencyTaskExecutions.Add(execution);
                    isNewExecution = true;
                }

                if (dto.Deliverables != null && dto.Deliverables.Count > 0)
                {
                    execution.Deliverables = dto.Deliverables;
                    if (!isNewExecution)
                    {
                        _context.Entry(execution).Property(e => e.Deliverables).IsModified = true;
                    }
                }
                execution.CalculatedStatus = PlanningService.CalculateExecutionStatus(task, progressLog, execution.Deliverables);
                execution.LatestProgressValue = actualCalculatedVal;
                execution.LatestQualitativeStatus = dto.Status;
                execution.CompletionPercentage = completionPercentage;
                execution.SummaryNotes = dto.SummaryNotes;
                if (uploadedUrls.Count > 0)
                {
                    execution.AttachmentFileUrls = uploadedUrls;
                    if (!isNewExecution)
                    {
                        _context.Entry(execution).Property(e => e.AttachmentFileUrls).IsModified = true;
                    }
                }
                execution.LastReportedAt = DateTime.UtcNow;
                execution.LastReportedBy = createdBy;

                if (!isNewExecution)
                {
                    _context.Entry(execution).State = EntityState.Modified;
                }
            }

            if (uploadedUrls.Count > 0)
            {
                var importLog = new DataImportLog
                {
                    FileName = string.Join(", ", uploadedUrls.Select(u => System.IO.Path.GetFileName(u))),
                    FileType = "File Minh chứng tiến độ",
                    Category = "Minh chứng báo cáo tiến độ",
                    ImportedBy = dto.CreatedBy ?? "Chuyên viên báo cáo",
                    ImportedAt = DateTime.UtcNow,
                    TotalGoalsCreated = 0,
                    TotalTasksCreated = 0,
                    Status = "Thành công",
                    SummaryNotes = $"Minh chứng báo cáo tiến độ {task.Code}: {task.Title}",
                    AgencyId = task.LeadAgencyId
                };
                _context.DataImportLogs.Add(importLog);
            }

            await _context.SaveChangesAsync();

            return new SubmitProgressResponseDto
            {
                ProgressLogId = progressLog.Id,
                TaskId = task.Id,
                TaskCode = task.Code,
                TaskTitle = task.Title,
                PeriodKey = periodKey,
                ActualValue = actualCalculatedVal,
                ExpectedBaselineTarget = expectedTarget,
                IsCustomBaselineUsed = isCustomUsed,
                CompletionPercentage = completionPercentage,
                CalculatedAlert = alertStatus,
                EvidenceFileUrl = primaryFileUrl,
                AttachmentFileUrls = uploadedUrls,
                LogDate = progressLog.LogDate,
                Message = "Ghi nhận báo cáo tiến độ và lưu file minh chứng thành công."
            };
        }

        public async Task<GetProgressLogResponseDto?> GetProgressLogAsync(Guid taskId, int year, int quarter, Guid? agencyId = null)
        {
            var task = await _context.GoalTaskItems.FirstOrDefaultAsync(t => t.Id == taskId);

            var query = _context.ProgressLogs
                .Where(l => l.GoalTaskId == taskId && l.PeriodYear == year && l.PeriodQuarter == quarter);

            if (task != null && task.IsGeneralTask && agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(l => l.AgencyId == agencyId.Value);
            }
            else if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(l => l.AgencyId == agencyId.Value || l.AgencyId == null);
            }

            var log = await query
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefaultAsync();

            if (log == null) return null;

            return new GetProgressLogResponseDto
            {
                Id = log.Id,
                TaskId = log.GoalTaskId,
                PeriodYear = log.PeriodYear,
                PeriodQuarter = log.PeriodQuarter,
                ActualValue = log.QuantitativeValue,
                Status = log.QualitativeStatus?.ToString(),
                CompletionPercentage = log.CalculatedProgressPercentage,
                SummaryNotes = log.SummaryNotes,
                AttachmentFileUrls = log.AttachmentFileUrls ?? new List<string>(),
                Deliverables = log.Deliverables,
                LogDate = log.LogDate,
                CalculatedAlert = log.CalculatedAlert
            };
        }

        public async Task<List<GetProgressLogResponseDto>> GetTaskProgressHistoryAsync(Guid taskId, Guid? agencyId = null)
        {
            var task = await _context.GoalTaskItems
                .Include(t => t.LeadAgency)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            string defaultAgencyName = task?.LeadAgency?.Name ?? "Đơn vị chủ trì";

            var query = _context.ProgressLogs
                .Include(p => p.Agency)
                .Where(p => p.GoalTaskId == taskId)
                .AsQueryable();

            if (task != null && task.IsGeneralTask && agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(p => p.AgencyId == agencyId.Value || p.AgencyId == null);
            }
            else if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(p => p.AgencyId == agencyId.Value || p.AgencyId == null);
            }

            var logsAsc = await query.OrderBy(p => p.LogDate).ToListAsync();

            var dtos = new List<GetProgressLogResponseDto>();
            var lastLogPerAgency = new Dictionary<string, ProgressLog>();

            List<TaskDeliverable>? GetInitialDeliverablesForAgency(Guid? agId)
            {
                if (task == null) return null;
                if (task.IsGeneralTask && agId.HasValue && agId.Value != Guid.Empty && task.AgencyDeliverables != null)
                {
                    string key = agId.Value.ToString().ToLower();
                    if (task.AgencyDeliverables.TryGetValue(key, out var agDels) && agDels != null)
                    {
                        return agDels;
                    }
                }
                return task.Deliverables;
            }

            for (int i = 0; i < logsAsc.Count; i++)
            {
                var log = logsAsc[i];
                string agencyKey = log.AgencyId.HasValue ? log.AgencyId.Value.ToString().ToLower() : "default";

                string createdByStr = log.CreatedBy;
                if (log.Agency != null && !string.IsNullOrWhiteSpace(log.Agency.Name))
                {
                    if (string.IsNullOrWhiteSpace(createdByStr) ||
                        createdByStr == "System User" ||
                        createdByStr == "Chuyên viên theo dõi" ||
                        createdByStr == "Đơn vị chủ trì")
                    {
                        createdByStr = log.Agency.Name;
                    }
                    else if (!createdByStr.StartsWith(log.Agency.Name))
                    {
                        createdByStr = $"{log.Agency.Name} ({createdByStr})";
                    }
                }
                else if (string.IsNullOrWhiteSpace(createdByStr) ||
                    createdByStr == "System User" ||
                    createdByStr == "Chuyên viên theo dõi" ||
                    createdByStr == "Đơn vị chủ trì")
                {
                    createdByStr = defaultAgencyName;
                }

                decimal? prevValue = null;
                string? prevStatus = "NotStarted";
                decimal? prevPercentage = 0m;
                string? prevNotes = null;
                List<TaskDeliverable>? prevDeliverables = null;

                if (lastLogPerAgency.TryGetValue(agencyKey, out var prevLog))
                {
                    prevValue = prevLog.QuantitativeValue;
                    prevStatus = prevLog.QualitativeStatus?.ToString();
                    prevPercentage = prevLog.CalculatedProgressPercentage;
                    prevNotes = prevLog.SummaryNotes;
                    prevDeliverables = prevLog.Deliverables;
                }
                else
                {
                    var initialDeliverables = GetInitialDeliverablesForAgency(log.AgencyId);
                    if (initialDeliverables != null && initialDeliverables.Count > 0)
                    {
                        prevDeliverables = initialDeliverables.Select(d => new TaskDeliverable
                        {
                            Id = d.Id,
                            Title = d.Title,
                            DueDate = d.DueDate,
                            CurrentStatus = "NotStarted",
                            DocumentNumber = "",
                            PromulgationDate = null
                        }).ToList();
                    }
                }

                dtos.Add(new GetProgressLogResponseDto
                {
                    Id = log.Id,
                    TaskId = log.GoalTaskId,
                    AgencyId = log.AgencyId,
                    PeriodYear = log.PeriodYear,
                    PeriodQuarter = log.PeriodQuarter,
                    ActualValue = log.QuantitativeValue,
                    Status = log.QualitativeStatus?.ToString(),
                    CompletionPercentage = log.CalculatedProgressPercentage,
                    SummaryNotes = log.SummaryNotes,
                    AttachmentFileUrls = log.AttachmentFileUrls ?? new List<string>(),
                    Deliverables = log.Deliverables,
                    LogDate = log.LogDate,
                    CalculatedAlert = log.CalculatedAlert,
                    CreatedBy = createdByStr,

                    PreviousValue = prevValue,
                    PreviousStatus = prevStatus,
                    PreviousCompletionPercentage = prevPercentage,
                    PreviousNotes = prevNotes,
                    PreviousDeliverables = prevDeliverables
                });

                lastLogPerAgency[agencyKey] = log;
            }

            dtos.Reverse();
            return dtos;
        }

        public async Task<TaskUrgeLogResponseDto> CreateUrgeLogAsync(CreateTaskUrgeLogDto dto)
        {
            var task = await _context.GoalTaskItems
                .Include(t => t.LeadAgency)
                .FirstOrDefaultAsync(t => t.Id == dto.GoalTaskId);

            if (task == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhiệm vụ với ID: {dto.GoalTaskId}");
            }

            var forecastObj = new
            {
                actualProgressPct = dto.ActualProgressPct,
                expectedTargetPct = dto.ExpectedTargetPct,
                laggingDeltaPct = dto.LaggingDeltaPct,
                estimatedCompletionDate = dto.EstimatedCompletionDate
            };
            string forecastJson = System.Text.Json.JsonSerializer.Serialize(forecastObj);

            var urgeLog = new TaskUrgeLog
            {
                GoalTaskId = task.Id,
                LeadAgencyId = task.LeadAgencyId,
                Title = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title : $"[Đôn đốc] V/v Thực hiện nhiệm vụ: {task.Code} - {task.Title}",
                TaskCode = task.Code,
                TaskTitle = task.Title,
                UrgeContent = dto.UrgeContent,
                ForecastDataJson = forecastJson,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            _context.TaskUrgeLogs.Add(urgeLog);
            await _context.SaveChangesAsync();

            return new TaskUrgeLogResponseDto
            {
                Id = urgeLog.Id,
                GoalTaskId = task.Id,
                Title = urgeLog.Title ?? "",
                TaskCode = task.Code,
                TaskTitle = task.Title,
                LeadAgencyCode = task.LeadAgency?.Code ?? string.Empty,
                LeadAgencyName = task.LeadAgency?.Name ?? string.Empty,
                UrgeContent = urgeLog.UrgeContent,
                ForecastDataJson = urgeLog.ForecastDataJson,
                CreatedAt = urgeLog.CreatedAt,
                CreatedBy = urgeLog.CreatedBy,
                RecipientsSummary = urgeLog.RecipientsSummary
            };
        }

        public async Task<List<TaskUrgeLogResponseDto>> GetTaskUrgeHistoryAsync(Guid taskId)
        {
            var logs = await _context.TaskUrgeLogs
                .Include(l => l.LeadAgency)
                .Where(l => l.GoalTaskId == taskId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return logs.Select(l => new TaskUrgeLogResponseDto
            {
                Id = l.Id,
                GoalTaskId = l.GoalTaskId,
                LeadAgencyId = l.LeadAgencyId,
                Title = !string.IsNullOrWhiteSpace(l.Title) ? l.Title : (!string.IsNullOrWhiteSpace(l.TaskTitle) ? $"[Thông báo] V/v Thực hiện nhiệm vụ: {l.TaskCode} - {l.TaskTitle}" : "Thông báo đôn đốc nhiệm vụ"),
                TaskCode = l.TaskCode,
                TaskTitle = l.TaskTitle,
                LeadAgencyCode = l.LeadAgency?.Code ?? string.Empty,
                LeadAgencyName = l.LeadAgency?.Name ?? string.Empty,
                UrgeContent = l.UrgeContent,
                ForecastDataJson = l.ForecastDataJson,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                RecipientsSummary = l.RecipientsSummary ?? string.Empty
            }).ToList();
        }

        public async Task<List<TaskUrgeLogResponseDto>> GetAllUrgeLogsAsync()
        {
            var logs = await _context.TaskUrgeLogs
                .Include(l => l.LeadAgency)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return logs.Select(l => new TaskUrgeLogResponseDto
            {
                Id = l.Id,
                GoalTaskId = l.GoalTaskId,
                LeadAgencyId = l.LeadAgencyId,
                Title = !string.IsNullOrWhiteSpace(l.Title) ? l.Title : (!string.IsNullOrWhiteSpace(l.TaskTitle) ? $"[Thông báo] V/v Thực hiện nhiệm vụ: {l.TaskCode} - {l.TaskTitle}" : "Thông báo đôn đốc nhiệm vụ"),
                TaskCode = l.TaskCode,
                TaskTitle = l.TaskTitle,
                LeadAgencyCode = l.LeadAgency?.Code ?? string.Empty,
                LeadAgencyName = l.LeadAgency?.Name ?? string.Empty,
                UrgeContent = l.UrgeContent,
                ForecastDataJson = l.ForecastDataJson,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                RecipientsSummary = l.RecipientsSummary ?? string.Empty
            }).ToList();
        }

        public async Task<TaskUrgeLogResponseDto?> GetUrgeLogByIdAsync(Guid logId)
        {
            var l = await _context.TaskUrgeLogs
                .Include(l => l.LeadAgency)
                .FirstOrDefaultAsync(l => l.Id == logId);

            if (l == null) return null;

            return new TaskUrgeLogResponseDto
            {
                Id = l.Id,
                GoalTaskId = l.GoalTaskId,
                LeadAgencyId = l.LeadAgencyId,
                Title = !string.IsNullOrWhiteSpace(l.Title) ? l.Title : (!string.IsNullOrWhiteSpace(l.TaskTitle) ? $"[Thông báo] V/v Thực hiện nhiệm vụ: {l.TaskCode} - {l.TaskTitle}" : "Thông báo đôn đốc nhiệm vụ"),
                TaskCode = l.TaskCode,
                TaskTitle = l.TaskTitle,
                LeadAgencyCode = l.LeadAgency?.Code ?? string.Empty,
                LeadAgencyName = l.LeadAgency?.Name ?? string.Empty,
                UrgeContent = l.UrgeContent,
                ForecastDataJson = l.ForecastDataJson,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy
            };
        }
    }
}
