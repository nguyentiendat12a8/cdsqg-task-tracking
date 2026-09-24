using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using System.Linq;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/execution")]
    public class ExecutionController : ControllerBase
    {
        private readonly IExecutionService _executionService;
        private readonly Cdsqg.Infrastructure.Data.AppDbContext _context;

        public ExecutionController(IExecutionService executionService, Cdsqg.Infrastructure.Data.AppDbContext context)
        {
            _executionService = executionService;
            _context = context;
        }

        /// <summary>
        /// Endpoint: POST /api/execution/tasks/{taskId}/progress
        /// Ghi nhận báo cáo tiến độ thực tế, đính kèm file IFormFile minh chứng (không bắt buộc), 
        /// tự động đối soát với CustomBaseline JSONB để tính Traffic Light Alert (Green/Yellow/Red).
        /// </summary>
        [HttpPost("tasks/{taskId:guid}/progress")]
        public async Task<IActionResult> SubmitProgress(Guid taskId, [FromForm] SubmitProgressRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception?.Message)
                    .Where(e => !string.IsNullOrEmpty(e)));

                return BadRequest(new { message = $"Dữ liệu gửi lên không hợp lệ: {errors}" });
            }

            try
            {
                var result = await _executionService.SubmitProgressAsync(taskId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = ex.Message });
            }
            catch (Exception ex)
            {
                var fullErr = ex.ToString();
                Console.WriteLine("[ERROR SubmitProgress] " + fullErr);
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật tiến độ: " + (ex.InnerException?.Message ?? ex.Message), error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/execution/tasks/{taskId}/progress?year=2026&period=1
        /// Lấy báo cáo tiến độ đã tồn tại cho một kỳ báo cáo cụ thể (nếu có).
        /// </summary>
        [HttpGet("tasks/{taskId:guid}/progress")]
        public async Task<IActionResult> GetProgress(
            Guid taskId, 
            [FromQuery] int year = 2026, 
            [FromQuery] int? period = null, 
            [FromQuery] int? periodQuarter = null, 
            [FromQuery] int? quarter = null,
            [FromQuery] Guid? agencyId = null)
        {
            try
            {
                int q = periodQuarter ?? quarter ?? period ?? 1;
                var log = await _executionService.GetProgressLogAsync(taskId, year, q, agencyId);
                if (log == null)
                {
                    return Ok(null);
                }
                return Ok(log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy báo cáo tiến độ: " + ex.Message, error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/execution/tasks/{taskId}/progress-history
        /// Lấy toàn bộ lịch sử các lượt báo cáo tiến độ đã gửi của một nhiệm vụ.
        /// </summary>
        [HttpGet("tasks/{taskId:guid}/progress-history")]
        public async Task<IActionResult> GetTaskProgressHistory(Guid taskId, [FromQuery] Guid? agencyId = null)
        {
            try
            {
                var history = await _executionService.GetTaskProgressHistoryAsync(taskId, agencyId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải lịch sử báo cáo: " + ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: POST /api/execution/tasks/{taskId}/urge
        /// Tạo dự thảo và lưu nhật ký đôn đốc tiến độ (kèm chỉ số dự báo) vào CSDL
        /// </summary>
        [HttpPost("tasks/{taskId:guid}/urge")]
        public async Task<IActionResult> SubmitUrge(Guid taskId, [FromBody] CreateTaskUrgeLogDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                dto.GoalTaskId = taskId;
                var result = await _executionService.CreateUrgeLogAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lưu lịch sử đôn đốc: " + ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/execution/tasks/{taskId}/urge-history
        /// Xem lại lịch sử các lần đôn đốc nhiệm vụ cụ thể
        /// </summary>
        [HttpGet("tasks/{taskId:guid}/urge-history")]
        public async Task<IActionResult> GetTaskUrgeHistory(Guid taskId)
        {
            try
            {
                var history = await _executionService.GetTaskUrgeHistoryAsync(taskId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải lịch sử đôn đốc: " + ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/execution/urge-logs
        /// Danh sách toàn bộ lịch sử đôn đốc trên toàn hệ thống (Hỗ trợ Phân trang Server & Bộ lọc nâng cao: Cơ quan, Từ ngày - Đến ngày)
        /// </summary>
        [HttpGet("urge-logs")]
        public async Task<IActionResult> GetAllUrgeLogs(
            [FromQuery] string? search = null,
            [FromQuery] Guid? agencyId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                var logs = await _executionService.GetAllUrgeLogsAsync();

                // 1. Filter by search term
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.Trim().ToLower();
                    bool isNumericCode = q.All(char.IsDigit) && q.Length <= 3;

                    logs = logs.Where(l => 
                        (l.TaskCode != null && (l.TaskCode.ToLower().Contains(q) || (isNumericCode && l.TaskCode.ToLower().EndsWith("-" + q)))) ||
                        (l.TaskTitle != null && l.TaskTitle.ToLower().Contains(q)) ||
                        (l.LeadAgencyName != null && l.LeadAgencyName.ToLower().Contains(q)) ||
                        (l.LeadAgencyCode != null && l.LeadAgencyCode.ToLower().Contains(q)) ||
                        (!isNumericCode && l.UrgeContent != null && l.UrgeContent.ToLower().Contains(q))
                    ).ToList();
                }

                // 2. Filter by Lead Agency ID with hierarchy support
                if (agencyId.HasValue && agencyId.Value != Guid.Empty)
                {
                    var agencyObj = await _context.Agencies.FindAsync(agencyId.Value);
                    var scopedAgencyIds = new System.Collections.Generic.List<Guid> { agencyId.Value };
                    if (agencyObj != null && !agencyObj.ParentId.HasValue)
                    {
                        var childIds = await _context.Agencies.Where(a => a.ParentId == agencyId.Value && a.IsActive).Select(a => a.Id).ToListAsync();
                        scopedAgencyIds.AddRange(childIds);
                    }

                    logs = logs.Where(l => l.LeadAgencyId.HasValue && scopedAgencyIds.Contains(l.LeadAgencyId.Value)).ToList();
                }

                // 3. Filter by From Date (Từ ngày đôn đốc)
                if (fromDate.HasValue)
                {
                    var startDate = fromDate.Value.Date;
                    logs = logs.Where(l => l.CreatedAt.Date >= startDate).ToList();
                }

                // 4. Filter by To Date (Đến ngày đôn đốc)
                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.Date;
                    logs = logs.Where(l => l.CreatedAt.Date <= endDate).ToList();
                }

                if (pageNumber.HasValue && pageSize.HasValue && pageSize.Value > 0)
                {
                    int pNum = pageNumber.Value > 0 ? pageNumber.Value : 1;
                    int pSize = pageSize.Value;
                    int totalCount = logs.Count;
                    int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);
                    var pagedItems = logs.Skip((pNum - 1) * pSize).Take(pSize).ToList();

                    return Ok(new
                    {
                        items = pagedItems,
                        totalCount,
                        pageNumber = pNum,
                        pageSize = pSize,
                        totalPages = Math.Max(1, totalPages)
                    });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách đôn đốc: " + ex.Message });
            }
        }

        /// <summary>
        /// <summary>
        /// Endpoint: GET /api/execution/urge-logs/{id}
        /// Chi tiết 1 văn bản đôn đốc theo ID
        /// </summary>
        [HttpGet("urge-logs/{id:guid}")]
        public async Task<IActionResult> GetUrgeLogById(Guid id)
        {
            try
            {
                var log = await _executionService.GetUrgeLogByIdAsync(id);
                if (log == null) return NotFound(new { message = "Không tìm thấy văn bản đôn đốc." });
                return Ok(log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải chi tiết đôn đốc: " + ex.Message });
            }
        }

        /// <summary>
        /// GET /api/execution/pending-approvals
        /// Danh sách báo cáo tiến độ đang chờ Cấp 1 (Admin) hoặc Cấp 2 phê duyệt.
        /// </summary>
        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPendingApprovals([FromQuery] Guid? parentAgencyId = null)
        {
            try
            {
                var query = _context.ProgressLogs
                    .Include(p => p.GoalTaskItem)
                        .ThenInclude(t => t!.LeadAgency)
                    .Include(p => p.Agency)
                        .ThenInclude(a => a!.ParentAgency)
                    .Where(p => p.ApprovalStatus == Cdsqg.Core.Enums.ApprovalStatusEnum.Pending);

                if (parentAgencyId.HasValue && parentAgencyId.Value != Guid.Empty)
                {
                    var childAgencyIds = await _context.Agencies
                        .Where(a => a.ParentId == parentAgencyId.Value)
                        .Select(a => a.Id)
                        .ToListAsync();

                    query = query.Where(p => (p.AgencyId.HasValue && childAgencyIds.Contains(p.AgencyId.Value)) ||
                                             (p.GoalTaskItem != null && p.GoalTaskItem.LeadAgencyId == parentAgencyId.Value));
                }

                var pendingLogs = await query
                    .OrderByDescending(p => p.LogDate)
                    .Select(p => new GetProgressLogResponseDto
                    {
                        Id = p.Id,
                        TaskId = p.GoalTaskId,
                        TaskCode = p.GoalTaskItem != null ? p.GoalTaskItem.Code : string.Empty,
                        TaskTitle = p.GoalTaskItem != null ? p.GoalTaskItem.Title : string.Empty,
                        IsGeneralTask = p.GoalTaskItem != null && (p.GoalTaskItem.IsGeneralTask || (p.GoalTaskItem.LeadAgency != null && (p.GoalTaskItem.LeadAgency.Code == "ALL_AGENCIES" || p.GoalTaskItem.LeadAgency.Code == "ALL_MINISTRIES" || p.GoalTaskItem.LeadAgency.Code == "ALL_PROVINCES" || p.GoalTaskItem.LeadAgency.Code == "ALL_PROVINCES_UBND" || p.GoalTaskItem.LeadAgency.Code == "ALL_MINISTRIES_DIRECT"))),
                        PeriodYear = p.PeriodYear,
                        PeriodQuarter = p.PeriodQuarter,
                        ActualValue = p.QuantitativeValue,
                        Status = p.QualitativeStatus != null ? p.QualitativeStatus.ToString() : null,
                        CompletionPercentage = p.CalculatedProgressPercentage,
                        SummaryNotes = p.SummaryNotes,
                        AttachmentFileUrls = p.AttachmentFileUrls ?? new List<string>(),
                        Deliverables = p.Deliverables,
                        LogDate = p.LogDate,
                        CalculatedAlert = p.CalculatedAlert,
                        CreatedBy = p.CreatedBy,
                        AgencyId = p.AgencyId,
                        AgencyCode = p.Agency != null ? p.Agency.Code : string.Empty,
                        AgencyName = p.Agency != null ? p.Agency.Name : string.Empty,
                        ParentAgencyId = p.Agency != null ? p.Agency.ParentId : null,
                        ParentAgencyName = p.Agency != null && p.Agency.ParentAgency != null ? p.Agency.ParentAgency.Name : null,
                        ApprovalStatus = p.ApprovalStatus.ToString(),
                        RejectionReason = p.RejectionReason,
                        ApprovedBy = p.ApprovedBy,
                        ApprovedAt = p.ApprovedAt
                    })
                    .ToListAsync();

                return Ok(pendingLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách báo cáo chờ duyệt: " + ex.Message });
            }
        }

        /// <summary>
        /// GET /api/execution/tasks/{taskId}/agencies-execution
        /// Ma trận tiến độ & phê duyệt của tất cả các cơ quan / địa phương cho 1 nhiệm vụ (đặc biệt là Nhiệm vụ chung).
        /// </summary>
        [HttpGet("tasks/{taskId:guid}/agencies-execution")]
        public async Task<IActionResult> GetTaskAgenciesExecution(Guid taskId)
        {
            try
            {
                var task = await _context.GoalTaskItems
                    .Include(t => t.LeadAgency)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null) return NotFound(new { message = "Không tìm thấy nhiệm vụ." });

                var allAgencies = await _context.Agencies
                    .Include(a => a.ParentAgency)
                    .Where(a => a.IsActive && a.Code != "ALL_AGENCIES" && a.Code != "ALL_MINISTRIES" && a.Code != "ALL_PROVINCES" && a.Code != "ALL_PROVINCES_UBND" && a.Code != "ALL_MINISTRIES_DIRECT")
                    .ToListAsync();

                var executions = await _context.AgencyTaskExecutions
                    .Include(e => e.Agency)
                        .ThenInclude(a => a!.ParentAgency)
                    .Where(e => e.GoalTaskId == taskId)
                    .ToListAsync();

                var pendingLogs = await _context.ProgressLogs
                    .Where(p => p.GoalTaskId == taskId && p.ApprovalStatus == Cdsqg.Core.Enums.ApprovalStatusEnum.Pending)
                    .ToListAsync();

                string leadCode = task.LeadAgency?.Code ?? string.Empty;
                bool isGeneral = task.IsGeneralTask || leadCode == "ALL_AGENCIES" || leadCode == "ALL_MINISTRIES" || leadCode == "ALL_PROVINCES" || leadCode == "ALL_PROVINCES_UBND" || leadCode == "ALL_MINISTRIES_DIRECT";

                // If general task, include all primary Level 2 agencies (and child agencies if assigned/executing)
                // If specific task, include lead agency and assigned agency
                var relevantAgencies = new List<Agency>();
                if (isGeneral)
                {
                    if (leadCode == "ALL_MINISTRIES" || leadCode == "ALL_MINISTRIES_DIRECT")
                    {
                        relevantAgencies = allAgencies.Where(a => a.ParentId == null && a.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Ministry).ToList();
                    }
                    else if (leadCode == "ALL_PROVINCES" || leadCode == "ALL_PROVINCES_UBND")
                    {
                        relevantAgencies = allAgencies.Where(a => a.ParentId == null && a.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Province).ToList();
                    }
                    else
                    {
                        relevantAgencies = allAgencies.Where(a => a.ParentId == null && (a.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Ministry || a.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Province)).ToList();
                    }
                    // Add any sub-agencies that have execution records or pending logs
                    var extraAgencyIds = executions.Select(e => e.AgencyId)
                        .Union(pendingLogs.Where(p => p.AgencyId.HasValue).Select(p => p.AgencyId!.Value))
                        .Distinct()
                        .ToList();
                    foreach (var extraId in extraAgencyIds)
                    {
                        if (!relevantAgencies.Any(a => a.Id == extraId))
                        {
                            var extraAg = allAgencies.FirstOrDefault(a => a.Id == extraId);
                            if (extraAg != null) relevantAgencies.Add(extraAg);
                        }
                    }
                }
                else
                {
                    if (task.LeadAgencyId != Guid.Empty)
                    {
                        var leadAg = allAgencies.FirstOrDefault(a => a.Id == task.LeadAgencyId);
                        if (leadAg != null) relevantAgencies.Add(leadAg);
                    }
                    if (task.AssignedAgencyId.HasValue)
                    {
                        var assAg = allAgencies.FirstOrDefault(a => a.Id == task.AssignedAgencyId.Value);
                        if (assAg != null && !relevantAgencies.Any(a => a.Id == assAg.Id)) relevantAgencies.Add(assAg);
                    }
                }

                var matrixItems = new List<AgencyExecutionItemDto>();

                foreach (var ag in relevantAgencies)
                {
                    var exec = executions.FirstOrDefault(e => e.AgencyId == ag.Id);
                    var pendingLog = pendingLogs.FirstOrDefault(p => p.AgencyId == ag.Id);

                    var itemDto = new AgencyExecutionItemDto
                    {
                        AgencyId = ag.Id,
                        AgencyName = ag.Name,
                        AgencyCode = ag.Code,
                        AgencyType = ag.Type.ToString(),
                        ParentAgencyId = ag.ParentId,
                        ParentAgencyName = ag.ParentAgency?.Name,
                        CalculatedStatus = exec != null ? exec.CalculatedStatus.ToString() : "NotStarted",
                        ApprovalStatus = pendingLog != null ? "Pending" : (exec != null ? exec.ApprovalStatus.ToString() : "NotReported"),
                        RejectionReason = exec?.RejectionReason ?? pendingLog?.RejectionReason,
                        LatestProgressValue = pendingLog?.QuantitativeValue ?? exec?.LatestProgressValue,
                        LatestQualitativeStatus = pendingLog?.QualitativeStatus?.ToString() ?? exec?.LatestQualitativeStatus?.ToString(),
                        CompletionPercentage = pendingLog?.CalculatedProgressPercentage ?? exec?.CompletionPercentage ?? 0m,
                        SummaryNotes = pendingLog?.SummaryNotes ?? exec?.SummaryNotes,
                        AttachmentFileUrls = (pendingLog?.AttachmentFileUrls?.Count > 0 ? pendingLog.AttachmentFileUrls : exec?.AttachmentFileUrls) ?? new List<string>(),
                        Deliverables = (pendingLog?.Deliverables?.Count > 0 ? pendingLog.Deliverables : exec?.Deliverables),
                        LastReportedAt = pendingLog?.LogDate ?? exec?.LastReportedAt,
                        LastReportedBy = pendingLog?.CreatedBy ?? exec?.LastReportedBy,
                        PendingProgressLogId = pendingLog?.Id
                    };

                    matrixItems.Add(itemDto);
                }

                return Ok(new AgencyTaskExecutionMatrixDto
                {
                    TaskId = task.Id,
                    TaskCode = task.Code,
                    TaskTitle = task.Title,
                    IsGeneralTask = isGeneral,
                    AgencyExecutions = matrixItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy ma trận tiến độ các cơ quan: " + ex.Message });
            }
        }

        /// <summary>
        /// POST /api/execution/approve/{logId}
        /// Cấp 2 phê duyệt báo cáo tiến độ từ Cấp 3.
        /// </summary>
        [HttpPost("approve/{logId:guid}")]
        public async Task<IActionResult> ApproveProgress(Guid logId, [FromQuery] string? approvedBy = null)
        {
            try
            {
                var log = await _context.ProgressLogs
                    .Include(p => p.GoalTaskItem)
                    .FirstOrDefaultAsync(p => p.Id == logId);

                if (log == null)
                {
                    return NotFound(new { message = "Không tìm thấy báo cáo tiến độ." });
                }

                log.ApprovalStatus = Cdsqg.Core.Enums.ApprovalStatusEnum.Approved;
                log.ApprovedBy = approvedBy ?? "Cơ quan cấp trên";
                log.ApprovedAt = DateTime.UtcNow;

                if (log.GoalTaskItem != null)
                {
                    if (log.Deliverables != null && log.Deliverables.Count > 0)
                    {
                        if (log.GoalTaskItem.IsGeneralTask && log.AgencyId.HasValue && log.AgencyId.Value != Guid.Empty)
                        {
                            string key = log.AgencyId.Value.ToString().ToLower();
                            log.GoalTaskItem.AgencyDeliverables ??= new Dictionary<string, List<Cdsqg.Core.Entities.TaskDeliverable>>();
                            log.GoalTaskItem.AgencyDeliverables[key] = log.Deliverables;
                            _context.Entry(log.GoalTaskItem).Property(t => t.AgencyDeliverables).IsModified = true;
                        }
                        else
                        {
                            log.GoalTaskItem.Deliverables = log.Deliverables;
                            _context.Entry(log.GoalTaskItem).Property(t => t.Deliverables).IsModified = true;
                        }
                    }
                    log.GoalTaskItem.LastUpdated = DateTime.UtcNow;
                }

                // Sync with AgencyTaskExecution
                if (log.AgencyId.HasValue)
                {
                    var execution = await _context.AgencyTaskExecutions
                        .FirstOrDefaultAsync(e => e.GoalTaskId == log.GoalTaskId && e.AgencyId == log.AgencyId.Value);

                    if (execution != null)
                    {
                        execution.ApprovalStatus = Cdsqg.Core.Enums.ApprovalStatusEnum.Approved;
                        execution.RejectionReason = null;
                        execution.LatestProgressValue = log.QuantitativeValue;
                        execution.LatestQualitativeStatus = log.QualitativeStatus;
                        execution.CompletionPercentage = log.CalculatedProgressPercentage ?? 0m;
                        execution.SummaryNotes = log.SummaryNotes;
                        execution.AttachmentFileUrls = log.AttachmentFileUrls ?? new System.Collections.Generic.List<string>();
                        if (log.Deliverables != null && log.Deliverables.Count > 0)
                        {
                            execution.Deliverables = log.Deliverables;
                        }
                        if (log.GoalTaskItem != null)
                        {
                            execution.CalculatedStatus = Cdsqg.Application.Services.PlanningService.CalculateExecutionStatus(log.GoalTaskItem, log, execution.Deliverables);
                        }
                    }
                }

                // Create notification for reporting agency (Level 3)
                if (log.AgencyId.HasValue && log.AgencyId.Value != Guid.Empty)
                {
                    var taskItem = log.GoalTaskItem ?? await _context.GoalTaskItems.FirstOrDefaultAsync(t => t.Id == log.GoalTaskId);
                    var taskCode = taskItem?.Code ?? "Nhiệm vụ";
                    var taskTitle = taskItem?.Title ?? "";
                    var approverName = !string.IsNullOrWhiteSpace(log.ApprovedBy) ? log.ApprovedBy : "Cơ quan cấp trên";

                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        AgencyId = log.AgencyId.Value,
                        Title = "Báo cáo tiến độ đã được phê duyệt",
                        Message = $"Báo cáo tiến độ cho nhiệm vụ {taskCode}: {taskTitle} đã được {approverName} phê duyệt.",
                        Type = "PROGRESS_APPROVED",
                        IsRead = false,
                        LinkUrl = taskItem != null ? $"/document-detail?taskId={taskItem.Id}&tab=reports" : null,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notif);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã phê duyệt báo cáo tiến độ thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi phê duyệt báo cáo: " + ex.Message });
            }
        }

        /// <summary>
        /// POST /api/execution/reject/{logId}
        /// Cấp 2 từ chối báo cáo tiến độ từ Cấp 3 (có lý do).
        /// </summary>
        [HttpPost("reject/{logId:guid}")]
        public async Task<IActionResult> RejectProgress(Guid logId, [FromBody] RejectProgressRequestDto dto, [FromQuery] string? approvedBy = null)
        {
            try
            {
                var log = await _context.ProgressLogs.FirstOrDefaultAsync(p => p.Id == logId);
                if (log == null)
                {
                    return NotFound(new { message = "Không tìm thấy báo cáo tiến độ." });
                }

                log.ApprovalStatus = Cdsqg.Core.Enums.ApprovalStatusEnum.Rejected;
                log.RejectionReason = dto?.Reason ?? "Chưa đạt yêu cầu";
                log.ApprovedBy = approvedBy ?? "Cơ quan cấp trên";
                log.ApprovedAt = DateTime.UtcNow;

                // Sync with AgencyTaskExecution
                if (log.AgencyId.HasValue)
                {
                    var execution = await _context.AgencyTaskExecutions
                        .FirstOrDefaultAsync(e => e.GoalTaskId == log.GoalTaskId && e.AgencyId == log.AgencyId.Value);

                    if (execution != null)
                    {
                        execution.ApprovalStatus = Cdsqg.Core.Enums.ApprovalStatusEnum.Rejected;
                        execution.RejectionReason = log.RejectionReason;

                        // Revert execution metrics to latest APPROVED log (if any)
                        var lastApprovedLog = await _context.ProgressLogs
                            .Where(p => p.GoalTaskId == log.GoalTaskId && p.AgencyId == log.AgencyId.Value && p.ApprovalStatus == Cdsqg.Core.Enums.ApprovalStatusEnum.Approved)
                            .OrderByDescending(p => p.LogDate)
                            .FirstOrDefaultAsync();

                        var taskItemForRevert = await _context.GoalTaskItems.FirstOrDefaultAsync(t => t.Id == log.GoalTaskId);

                        if (lastApprovedLog != null)
                        {
                            execution.LatestProgressValue = lastApprovedLog.QuantitativeValue;
                            execution.LatestQualitativeStatus = lastApprovedLog.QualitativeStatus;
                            execution.CompletionPercentage = lastApprovedLog.CalculatedProgressPercentage ?? 0m;
                            execution.SummaryNotes = lastApprovedLog.SummaryNotes;
                            execution.AttachmentFileUrls = lastApprovedLog.AttachmentFileUrls ?? new System.Collections.Generic.List<string>();
                            if (lastApprovedLog.Deliverables != null && lastApprovedLog.Deliverables.Count > 0)
                            {
                                execution.Deliverables = lastApprovedLog.Deliverables;
                            }
                            if (taskItemForRevert != null)
                            {
                                execution.CalculatedStatus = Cdsqg.Application.Services.PlanningService.CalculateExecutionStatus(taskItemForRevert, lastApprovedLog, execution.Deliverables);
                            }
                        }
                        else
                        {
                            execution.LatestProgressValue = null;
                            execution.LatestQualitativeStatus = Cdsqg.Core.Enums.TextStatusEnum.NotStarted;
                            execution.CompletionPercentage = 0m;
                            execution.SummaryNotes = null;
                            execution.AttachmentFileUrls = new System.Collections.Generic.List<string>();
                            if (taskItemForRevert?.Deliverables != null)
                            {
                                execution.Deliverables = taskItemForRevert.Deliverables.Select(d => new Cdsqg.Core.Entities.TaskDeliverable
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
                            if (taskItemForRevert != null)
                            {
                                execution.CalculatedStatus = Cdsqg.Core.Enums.ExecutionStatusEnum.NotStarted;
                            }
                        }
                    }
                }

                // Create notification for reporting agency (Level 3)
                if (log.AgencyId.HasValue && log.AgencyId.Value != Guid.Empty)
                {
                    var taskItem = await _context.GoalTaskItems.FirstOrDefaultAsync(t => t.Id == log.GoalTaskId);
                    var taskCode = taskItem?.Code ?? "Nhiệm vụ";
                    var taskTitle = taskItem?.Title ?? "";
                    var approverName = !string.IsNullOrWhiteSpace(log.ApprovedBy) ? log.ApprovedBy : "Cơ quan cấp trên";
                    var reasonText = !string.IsNullOrWhiteSpace(log.RejectionReason) ? log.RejectionReason : "Chưa đạt yêu cầu";

                    var notif = new Notification
                    {
                        Id = Guid.NewGuid(),
                        AgencyId = log.AgencyId.Value,
                        Title = "Báo cáo tiến độ bị từ chối phê duyệt",
                        Message = $"Báo cáo tiến độ cho nhiệm vụ {taskCode}: {taskTitle} đã bị {approverName} từ chối. Lý do: {reasonText}.",
                        Type = "PROGRESS_REJECTED",
                        IsRead = false,
                        LinkUrl = taskItem != null ? $"/document-detail?taskId={taskItem.Id}&tab=reports" : null,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notif);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã từ chối báo cáo tiến độ." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi từ chối báo cáo: " + ex.Message });
            }
        }

        /// <summary>
        /// POST /api/execution/import-progress-bulk
        /// Nhập báo cáo tiến độ hàng loạt từ file Excel.
        /// </summary>
        [HttpPost("import-progress-bulk")]
        public async Task<IActionResult> ImportProgressBulk([FromBody] ImportProgressBulkRequestDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "Danh sách dữ liệu import trống." });
            }

            try
            {
                var result = await _executionService.ImportProgressBulkAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi import tiến độ hàng loạt: " + ex.Message });
            }
        }
    }
}
