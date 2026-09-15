using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/planning")]
    public class PlanningController : ControllerBase
    {
        private readonly IPlanningService _planningService;

        public PlanningController(IPlanningService planningService)
        {
            _planningService = planningService;
        }

        /// <summary>
        /// Endpoint: GET /api/planning/documents/{id}/grid
        /// Trả về toàn bộ Goals (1A) & Tasks (1B) thuộc Quyết định để Vue frontend render bảng chỉ tiêu động (2026-2030)
        /// </summary>
        [HttpGet("documents/{id:guid}/grid")]
        public async Task<IActionResult> GetDocumentPlanningGrid(Guid id)
        {
            try
            {
                var gridData = await _planningService.GetDocumentPlanningGridAsync(id);
                return Ok(gridData);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống khi tải Planning Grid", details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: POST /api/planning/tasks/{taskId}/custom-baseline
        /// Cập nhật cột JSONB CustomBaseline lưu cấu hình ghi đè mốc chỉ tiêu theo Quý (e.g. {"Q1_2026": 10, "Q2_2026": 40})
        /// </summary>
        [HttpPost("tasks/{taskId:guid}/custom-baseline")]
        public async Task<IActionResult> UpdateTaskCustomBaseline(Guid taskId, [FromBody] UpdateCustomBaselineDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _planningService.UpdateTaskCustomBaselineAsync(taskId, dto);
                return Ok(new { success, message = "Đã cập nhật cấu hình Custom Baseline JSONB thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi cập nhật Custom Baseline", details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: PUT /api/planning/tasks/{taskId}/yearly-target
        /// Auto-save chỉ tiêu hằng năm của một Task/Goal (Quarter == 0)
        /// </summary>
        [HttpPut("tasks/{taskId:guid}/yearly-target")]
        public async Task<IActionResult> UpdateYearlyTarget(Guid taskId, [FromBody] UpdateYearlyTargetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _planningService.UpdateYearlyTargetAsync(taskId, dto);
                return Ok(new { success, message = $"Đã tự động lưu chỉ tiêu năm {dto.Year} thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi cập nhật chỉ tiêu hàng năm", details = ex.Message });
            }
        }
    }
}
