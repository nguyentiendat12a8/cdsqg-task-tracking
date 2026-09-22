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
        Task<ImportProgressBulkResponseDto> ImportProgressBulkAsync(ImportProgressBulkRequestDto dto);
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

        public static decimal CalculateDeliverablesCompletionPercentage(List<TaskDeliverable>? deliverables)
        {
            if (deliverables == null || deliverables.Count == 0) return 0m;
            decimal total = 0m;
            foreach (var d in deliverables)
            {
                string st = (d.CurrentStatus ?? "NotStarted").Trim();
                decimal p = 0m;
                if (string.Equals(st, "Completed", StringComparison.OrdinalIgnoreCase) || st == "4")
                {
                    p = 100m;
                }
                else if (string.Equals(st, "Submitted", StringComparison.OrdinalIgnoreCase))
                {
                    p = 85m;
                }
                else if (string.Equals(st, "Reviewing", StringComparison.OrdinalIgnoreCase) || st == "3")
                {
                    p = 60m;
                }
                else if (string.Equals(st, "Drafting", StringComparison.OrdinalIgnoreCase) || st == "2")
                {
                    p = 25m;
                }
                else
                {
                    p = 0m;
                }
                total += p;
            }
            return Math.Round(total / deliverables.Count, 2);
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
                var deliverablesList = (dto.Deliverables != null && dto.Deliverables.Count > 0)
                    ? dto.Deliverables
                    : task.Deliverables;

                if (deliverablesList != null && deliverablesList.Count > 0)
                {
                    completionPercentage = CalculateDeliverablesCompletionPercentage(deliverablesList);
                }
                else
                {
                    TextStatusEnum statusVal = dto.Status ?? TextStatusEnum.NotStarted;
                    completionPercentage = statusVal switch
                    {
                        TextStatusEnum.NotStarted => 0m,
                        TextStatusEnum.Drafting => 25m,
                        TextStatusEnum.Reviewing => 60m,
                        TextStatusEnum.Completed => 100m,
                        _ => 0m
                    };
                }

                if (completionPercentage >= 95m)
                    alertStatus = AlertStatusEnum.Green;
                else if (completionPercentage >= 50m)
                    alertStatus = AlertStatusEnum.Yellow;
                else
                    alertStatus = AlertStatusEnum.Red;
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
            var reportingAgencyObj = reportingAgencyId.HasValue ? await _context.Agencies.FirstOrDefaultAsync(a => a.Id == reportingAgencyId.Value) : null;
            
            bool isAdmin = dto.CreatedBy != null && (dto.CreatedBy.ToLower().Contains("admin") || dto.CreatedBy.ToLower().Contains("quản trị"));
            bool isLevel3Subordinate = false;
            if (!isAdmin)
            {
                if (reportingAgencyObj != null && reportingAgencyObj.ParentId.HasValue)
                {
                    isLevel3Subordinate = true;
                }
                else if (task.AssignedAgencyId.HasValue)
                {
                    var assignedAg = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == task.AssignedAgencyId.Value);
                    if (assignedAg != null && assignedAg.ParentId.HasValue)
                    {
                        isLevel3Subordinate = true;
                        if (!reportingAgencyId.HasValue || reportingAgencyId.Value == Guid.Empty)
                        {
                            reportingAgencyId = task.AssignedAgencyId;
                            reportingAgencyObj = assignedAg;
                        }
                    }
                }
            }

            if (!reportingAgencyId.HasValue || reportingAgencyId.Value == Guid.Empty)
            {
                reportingAgencyId = task.LeadAgencyId;
            }

            var initialApprovalStatus = isLevel3Subordinate ? ApprovalStatusEnum.Pending : ApprovalStatusEnum.Approved;

            if (isLevel3Subordinate && reportingAgencyId.HasValue)
            {
                bool hasPending = await _context.ProgressLogs
                    .AnyAsync(p => p.GoalTaskId == taskId && p.AgencyId == reportingAgencyId.Value && p.ApprovalStatus == ApprovalStatusEnum.Pending);

                if (hasPending)
                {
                    throw new InvalidOperationException("Nhiệm vụ này đang có báo cáo tiến độ ở trạng thái 'Chờ duyệt'. Vui lòng chờ Cấp 2 phê duyệt hoặc từ chối trước khi gửi báo cáo mới.");
                }
            }

            // ONLY mutate entity deliverables immediately if automatically Approved (e.g. submitted by Level 2 or Admin).
            // If Pending Level 2 approval, deliverables are saved only in the ProgressLog record until approved.
            if (initialApprovalStatus == ApprovalStatusEnum.Approved && dto.Deliverables != null && dto.Deliverables.Count > 0)
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
                : (reportingAgencyObj?.Name ?? task.LeadAgency?.Name ?? "Đơn vị chủ trì");

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
                CreatedBy = createdBy,
                ApprovalStatus = initialApprovalStatus
            };

            _context.ProgressLogs.Add(progressLog);

            // Create notification for parent agency if submitted by Level 3
            if (isLevel3Subordinate && reportingAgencyObj?.ParentId.HasValue == true)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    AgencyId = reportingAgencyObj.ParentId.Value,
                    Title = "Báo cáo tiến độ mới chờ duyệt",
                    Message = $"{reportingAgencyObj.Name} đã gửi báo cáo tiến độ cho nhiệm vụ {task.Code}: {task.Title}. Vui lòng xem xét và phê duyệt.",
                    Type = "PROGRESS_APPROVAL",
                    IsRead = false,
                    LinkUrl = $"/document-detail?taskId={task.Id}&tab=reports",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

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
                        AttachmentFileUrls = uploadedUrls ?? new List<string>(),
                        ApprovalStatus = initialApprovalStatus
                    };
                    _context.AgencyTaskExecutions.Add(execution);
                    isNewExecution = true;
                }

                execution.ApprovalStatus = initialApprovalStatus;
                execution.LastReportedAt = DateTime.UtcNow;
                execution.LastReportedBy = createdBy;

                if (initialApprovalStatus == ApprovalStatusEnum.Approved)
                {
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
                }

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

            var baseQuery = _context.ProgressLogs.Where(l => l.GoalTaskId == taskId).AsQueryable();

            if (task != null && task.IsGeneralTask && agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(l => l.AgencyId == agencyId.Value);
            }
            else if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(l => l.AgencyId == agencyId.Value || l.AgencyId == null);
            }

            // 1. Try finding approved log for exact year & quarter
            var log = await baseQuery
                .Where(l => l.PeriodYear == year && l.PeriodQuarter == quarter && l.ApprovalStatus == ApprovalStatusEnum.Approved)
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefaultAsync();

            // 2. If not found, try finding any log for exact year & quarter
            if (log == null)
            {
                log = await baseQuery
                    .Where(l => l.PeriodYear == year && l.PeriodQuarter == quarter)
                    .OrderByDescending(l => l.LogDate)
                    .FirstOrDefaultAsync();
            }

            // 3. Fallback to latest approved log overall for this task
            if (log == null)
            {
                log = await baseQuery
                    .Where(l => l.ApprovalStatus == ApprovalStatusEnum.Approved)
                    .OrderByDescending(l => l.LogDate)
                    .FirstOrDefaultAsync();
            }

            // 4. Fallback to latest log overall
            if (log == null)
            {
                log = await baseQuery
                    .OrderByDescending(l => l.LogDate)
                    .FirstOrDefaultAsync();
            }

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

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                var subAgencyIds = await _context.Agencies
                    .Where(a => a.ParentId == agencyId.Value)
                    .Select(a => a.Id)
                    .ToListAsync();

                bool isLeadOrAssigned = task != null && (
                    (task.LeadAgencyId == agencyId.Value) ||
                    (task.AssignedAgencyId.HasValue && task.AssignedAgencyId.Value == agencyId.Value)
                );

                if (isLeadOrAssigned || subAgencyIds.Count > 0 || (task != null && task.IsGeneralTask))
                {
                    var allowedAgencyIds = new HashSet<Guid>(subAgencyIds) { agencyId.Value };
                    if (task != null) allowedAgencyIds.Add(task.LeadAgencyId);
                    if (task?.AssignedAgencyId.HasValue == true) allowedAgencyIds.Add(task.AssignedAgencyId.Value);

                    query = query.Where(p => p.AgencyId == null || allowedAgencyIds.Contains(p.AgencyId.Value));
                }
                else
                {
                    query = query.Where(p => p.AgencyId == agencyId.Value || p.AgencyId == null);
                }
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

                decimal? logCompletionPercentage = log.CalculatedProgressPercentage;
                if (task != null && task.EvaluationType != EvaluationTypeEnum.Quantitative)
                {
                    if (log.Deliverables != null && log.Deliverables.Count > 0)
                    {
                        logCompletionPercentage = CalculateDeliverablesCompletionPercentage(log.Deliverables);
                    }
                    else if (log.QualitativeStatus.HasValue)
                    {
                        logCompletionPercentage = log.QualitativeStatus.Value switch
                        {
                            TextStatusEnum.NotStarted => 0m,
                            TextStatusEnum.Drafting => 25m,
                            TextStatusEnum.Reviewing => 60m,
                            TextStatusEnum.Completed => 100m,
                            _ => 0m
                        };
                    }
                }

                decimal? prevCompletionPercentage = prevPercentage;
                if (task != null && task.EvaluationType != EvaluationTypeEnum.Quantitative && prevDeliverables != null && prevDeliverables.Count > 0)
                {
                    prevCompletionPercentage = CalculateDeliverablesCompletionPercentage(prevDeliverables);
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
                    CompletionPercentage = logCompletionPercentage,
                    SummaryNotes = log.SummaryNotes,
                    AttachmentFileUrls = log.AttachmentFileUrls ?? new List<string>(),
                    Deliverables = log.Deliverables,
                    LogDate = log.LogDate,
                    CalculatedAlert = log.CalculatedAlert,
                    CreatedBy = createdByStr,
                    ApprovalStatus = log.ApprovalStatus.ToString(),
                    RejectionReason = log.RejectionReason,
                    ApprovedBy = log.ApprovedBy,
                    ApprovedAt = log.ApprovedAt,

                    PreviousValue = prevValue,
                    PreviousStatus = prevStatus,
                    PreviousCompletionPercentage = prevCompletionPercentage,
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

        public async Task<ImportProgressBulkResponseDto> ImportProgressBulkAsync(ImportProgressBulkRequestDto dto)
        {
            var response = new ImportProgressBulkResponseDto();
            if (dto?.Items == null || dto.Items.Count == 0)
            {
                return response;
            }

            var userAgency = dto.UserAgencyId.HasValue && dto.UserAgencyId.Value != Guid.Empty
                ? await _context.Agencies.FirstOrDefaultAsync(a => a.Id == dto.UserAgencyId.Value)
                : null;

            bool isAdmin = dto.UserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
            bool isLevel2 = userAgency != null && !userAgency.ParentId.HasValue;
            bool isLevel3 = userAgency != null && userAgency.ParentId.HasValue;

            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Code))
                    continue;

                var task = await _context.GoalTaskItems
                    .Include(t => t.LeadAgency)
                    .Include(t => t.Baselines)
                    .Include(t => t.ProgressLogs)
                    .FirstOrDefaultAsync(t => t.Code.ToLower() == item.Code.Trim().ToLower());

                if (task == null)
                {
                    response.Results.Add(new ImportProgressResultItemDto
                    {
                        Code = item.Code,
                        Title = "N/A",
                        Success = false,
                        Message = $"Không tìm thấy mục tiêu/nhiệm vụ với mã '{item.Code}'."
                    });
                    response.FailureCount++;
                    continue;
                }

                // Permission check
                bool hasPermission = false;
                if (isAdmin)
                {
                    hasPermission = true;
                }
                else if (isLevel2)
                {
                    if (task.LeadAgencyId == userAgency!.Id || task.IsGeneralTask || task.LeadAgency?.ParentId == userAgency.Id)
                    {
                        hasPermission = true;
                    }
                    else if (task.AssignedAgencyId.HasValue)
                    {
                        var childAgencyIds = await _context.Agencies
                            .Where(a => a.ParentId == userAgency.Id)
                            .Select(a => a.Id)
                            .ToListAsync();
                        if (childAgencyIds.Contains(task.AssignedAgencyId.Value))
                        {
                            hasPermission = true;
                        }
                    }
                }
                else if (isLevel3)
                {
                    if (task.AssignedAgencyId == userAgency!.Id || task.LeadAgencyId == userAgency.Id)
                    {
                        hasPermission = true;
                    }
                }

                if (!hasPermission)
                {
                    response.Results.Add(new ImportProgressResultItemDto
                    {
                        Code = task.Code,
                        Title = task.Title,
                        Success = false,
                        Message = "Tài khoản không có quyền cập nhật tiến độ cho mục tiêu/nhiệm vụ này."
                    });
                    response.FailureCount++;
                    continue;
                }

                var submitDto = new SubmitProgressRequestDto
                {
                    PeriodYear = item.PeriodYear > 0 ? item.PeriodYear : 2026,
                    PeriodQuarter = item.PeriodQuarter,
                    PeriodType = item.PeriodQuarter > 0 ? "Quarterly" : "Yearly",
                    Value = item.Value,
                    Status = item.Status,
                    SummaryNotes = item.SummaryNotes ?? string.Empty,
                    AgencyId = userAgency?.Id ?? task.LeadAgencyId,
                    CreatedBy = userAgency?.Name ?? "Import Excel"
                };

                try
                {
                    var submitResult = await SubmitProgressAsync(task.Id, submitDto);
                    bool isPending = submitResult.ApprovalStatus == "Pending";

                    response.Results.Add(new ImportProgressResultItemDto
                    {
                        Code = task.Code,
                        Title = task.Title,
                        Success = true,
                        Message = isPending
                            ? "Đã gửi báo cáo tiến độ, đang chờ Cấp 2 phê duyệt."
                            : "Cập nhật tiến độ thành công.",
                        ApprovalStatus = submitResult.ApprovalStatus
                    });

                    if (isPending)
                    {
                        response.PendingCount++;
                    }
                    else
                    {
                        response.SuccessCount++;
                    }
                }
                catch (Exception ex)
                {
                    response.Results.Add(new ImportProgressResultItemDto
                    {
                        Code = task.Code,
                        Title = task.Title,
                        Success = false,
                        Message = "Lỗi khi lưu tiến độ: " + ex.Message
                    });
                    response.FailureCount++;
                }
            }

            response.TotalProcessed = dto.Items.Count;
            return response;
        }
    }
}
