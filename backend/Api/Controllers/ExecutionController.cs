using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;

using System.Linq;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/execution")]
    public class ExecutionController : ControllerBase
    {
        private readonly IExecutionService _executionService;

        public ExecutionController(IExecutionService executionService)
        {
            _executionService = executionService;
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
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật tiến độ: " + ex.Message, error = ex.Message });
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
            [FromQuery] int? quarter = null)
        {
            try
            {
                int q = periodQuarter ?? quarter ?? period ?? 1;
                var log = await _executionService.GetProgressLogAsync(taskId, year, q);
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
        public async Task<IActionResult> GetTaskProgressHistory(Guid taskId)
        {
            try
            {
                var history = await _executionService.GetTaskProgressHistoryAsync(taskId);
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

                // 2. Filter by Lead Agency ID
                if (agencyId.HasValue && agencyId.Value != Guid.Empty)
                {
                    logs = logs.Where(l => l.LeadAgencyId == agencyId.Value).ToList();
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
    }
}
