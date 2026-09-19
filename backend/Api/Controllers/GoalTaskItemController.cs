using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/planning/items")]
    public class GoalTaskItemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoalTaskItemController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// POST /api/planning/items
        /// Thêm mới một Mục tiêu (Level 1A) hoặc Nhiệm vụ (Level 1B) vào Văn bản Level 0.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateGoalTaskItemRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == dto.DocumentId) 
                          ?? await _context.Documents.FirstOrDefaultAsync();

                if (doc == null)
                {
                    var defaultId = dto.DocumentId != Guid.Empty ? dto.DocumentId : Guid.Parse("12660000-0000-0000-0000-000000001266");
                    doc = new Document
                    {
                        Id = defaultId,
                        DocumentNumber = "1266/QĐ-TTg",
                        Name = "Quyết định số 1266/QĐ-TTg ngày 14/07/2026 của Thủ tướng Chính phủ",
                        Summary = "Hệ thống theo dõi nhiệm vụ",
                        StartYear = 2026,
                        EndYear = 2030,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Documents.Add(doc);
                    await _context.SaveChangesAsync();
                }

                dto.DocumentId = doc.Id;

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new { error = "Tên mục tiêu/nhiệm vụ không được để trống." });
                }

                ItemTypeEnum itemType = ItemTypeEnum.Task;
                if (!string.IsNullOrWhiteSpace(dto.ItemType))
                {
                    if (dto.ItemType.Equals("Goal", StringComparison.OrdinalIgnoreCase) || dto.ItemType == "1")
                        itemType = ItemTypeEnum.Goal;
                    else if (dto.ItemType.Equals("Task", StringComparison.OrdinalIgnoreCase) || dto.ItemType == "2")
                        itemType = ItemTypeEnum.Task;
                    else
                        Enum.TryParse<ItemTypeEnum>(dto.ItemType, true, out itemType);
                }

                EvaluationTypeEnum evalType = itemType == ItemTypeEnum.Goal ? EvaluationTypeEnum.Quantitative : EvaluationTypeEnum.Qualitative;
                if (!string.IsNullOrWhiteSpace(dto.EvaluationType))
                {
                    if (dto.EvaluationType.Equals("Quantitative", StringComparison.OrdinalIgnoreCase) || dto.EvaluationType == "1")
                        evalType = EvaluationTypeEnum.Quantitative;
                    else if (dto.EvaluationType.Equals("Qualitative", StringComparison.OrdinalIgnoreCase) || dto.EvaluationType == "2")
                        evalType = EvaluationTypeEnum.Qualitative;
                    else
                        Enum.TryParse<EvaluationTypeEnum>(dto.EvaluationType, true, out evalType);
                }

                CalculationMethodEnum calcMethod = CalculationMethodEnum.LatestValue;
                if (!string.IsNullOrWhiteSpace(dto.CalculationMethod))
                {
                    if (dto.CalculationMethod.Equals("Cumulative", StringComparison.OrdinalIgnoreCase) || dto.CalculationMethod == "1")
                        calcMethod = CalculationMethodEnum.Cumulative;
                    else if (dto.CalculationMethod.Equals("LatestValue", StringComparison.OrdinalIgnoreCase) || dto.CalculationMethod == "2")
                        calcMethod = CalculationMethodEnum.LatestValue;
                    else
                        Enum.TryParse<CalculationMethodEnum>(dto.CalculationMethod, true, out calcMethod);
                }

                string finalCode = dto.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(finalCode) || finalCode == "MT-" || finalCode == "NV-" || finalCode.EndsWith("-"))
                {
                    var existingCount = await _context.GoalTaskItems.CountAsync(i => i.DocumentId == dto.DocumentId && i.ItemType == itemType);
                    string prefix = itemType == ItemTypeEnum.Goal ? "MT" : "NV";
                    finalCode = $"{prefix}-{(existingCount + 1):D2}";
                }

                var leadAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == dto.LeadAgencyId);
                bool isGeneral = dto.IsGeneralTask || (leadAgency != null && leadAgency.Code == "ALL_AGENCIES");

                var newItem = new GoalTaskItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = dto.DocumentId,
                    ParentId = dto.ParentId,
                    ItemType = itemType,
                    Code = finalCode,
                    Title = dto.Title.Trim(),
                    Category = dto.Category?.Trim() ?? string.Empty,
                    Section = dto.Section,
                    Group = dto.Group,
                    IsOngoing = dto.IsOngoing,
                    IsGeneralTask = isGeneral,
                    StartDate = dto.StartDate,
                    DueDate = dto.DueDate,
                    LeadAgencyId = dto.LeadAgencyId,
                    CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>(),
                    UnitId = dto.UnitId,
                    EvaluationType = evalType,
                    CalculationMethod = calcMethod,
                    CustomBaseline = dto.CustomBaseline ?? new Dictionary<string, string>(),
                    Deliverables = dto.Deliverables ?? new List<TaskDeliverable>(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.GoalTaskItems.Add(newItem);

                // Add annual baseline targets if provided
                if (dto.YearlyTargets != null && dto.YearlyTargets.Count > 0)
                {
                    foreach (var kvp in dto.YearlyTargets)
                    {
                        var targetBaseline = new TargetBaseline
                        {
                            Id = Guid.NewGuid(),
                            GoalTaskId = newItem.Id,
                            Year = kvp.Key,
                            Quarter = 0,
                            TargetQuantity = kvp.Value
                        };
                        _context.TargetBaselines.Add(targetBaseline);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = newItem.Id,
                    documentId = newItem.DocumentId,
                    itemType = newItem.ItemType.ToString(),
                    code = newItem.Code,
                    title = newItem.Title,
                    category = newItem.Category,
                    leadAgencyId = newItem.LeadAgencyId,
                    coordinatingAgencyIds = newItem.CoordinatingAgencyIds,
                    unitId = newItem.UnitId,
                    evaluationType = newItem.EvaluationType.ToString(),
                    calculationMethod = newItem.CalculationMethod.ToString(),
                    customBaseline = newItem.CustomBaseline,
                    createdAt = newItem.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi tạo Mục tiêu / Nhiệm vụ", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/planning/items/{id}
        /// Xóa một Mục tiêu (1A) hoặc Nhiệm vụ (1B) hoặc Sub-Task.
        /// Chỉ cho phép xóa khi chưa có báo cáo tiến độ.
        /// <summary>
        /// DELETE /api/planning/items/{id}
        /// Xóa một Mục tiêu (1A) hoặc Nhiệm vụ (1B) hoặc Sub-Task.
        /// Chỉ cho phép xóa khi ở trạng thái Chưa bắt đầu (Chưa có báo cáo/cập nhật tiến độ).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var item = await _context.GoalTaskItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound(new { error = "Không tìm thấy Mục tiêu / Nhiệm vụ để xóa." });
            }

            var childSubTaskIds = await _context.GoalTaskItems
                .Where(sub => sub.ParentId == id)
                .Select(sub => sub.Id)
                .ToListAsync();

            var allTargetIds = new List<Guid> { id };
            allTargetIds.AddRange(childSubTaskIds);

            var hasProgressLogs = await _context.ProgressLogs.AnyAsync(p => allTargetIds.Contains(p.GoalTaskId));

            var latestLog = await _context.ProgressLogs
                .Where(p => allTargetIds.Contains(p.GoalTaskId))
                .OrderByDescending(p => p.LogDate)
                .FirstOrDefaultAsync();

            var currentStatus = Cdsqg.Application.Services.PlanningService.CalculateExecutionStatus(item, latestLog);

            if (hasProgressLogs || currentStatus != ExecutionStatusEnum.NotStarted)
            {
                return BadRequest(new { error = "Chỉ được phép xóa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu (Chưa cập nhật tiến độ)." });
            }

            if (childSubTaskIds.Any())
            {
                var childItems = await _context.GoalTaskItems.Where(sub => sub.ParentId == id).ToListAsync();
                _context.GoalTaskItems.RemoveRange(childItems);
            }

            _context.GoalTaskItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã xóa thành công {item.Code}: {item.Title}" });
        }

        /// <summary>
        /// PUT /api/planning/items/{id}
        /// Cập nhật thông tin một Mục tiêu (1A), Nhiệm vụ (1B) hoặc Sub-Task.
        /// Chỉ cho phép chỉnh sửa khi ở trạng thái Chưa bắt đầu.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateGoalTaskItemRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var item = await _context.GoalTaskItems.FirstOrDefaultAsync(i => i.Id == id);
                if (item == null)
                {
                    return NotFound(new { error = "Không tìm thấy Mục tiêu / Nhiệm vụ để cập nhật." });
                }

                var childSubTaskIds = await _context.GoalTaskItems
                    .Where(sub => sub.ParentId == id)
                    .Select(sub => sub.Id)
                    .ToListAsync();

                var allTargetIds = new List<Guid> { id };
                allTargetIds.AddRange(childSubTaskIds);

                var hasProgressLogs = await _context.ProgressLogs.AnyAsync(p => allTargetIds.Contains(p.GoalTaskId));

                var latestLog = await _context.ProgressLogs
                    .Where(p => allTargetIds.Contains(p.GoalTaskId))
                    .OrderByDescending(p => p.LogDate)
                    .FirstOrDefaultAsync();

                var currentStatus = Cdsqg.Application.Services.PlanningService.CalculateExecutionStatus(item, latestLog);

                if (hasProgressLogs || currentStatus != ExecutionStatusEnum.NotStarted)
                {
                    return BadRequest(new { error = "Chỉ được phép chỉnh sửa Mục tiêu / Nhiệm vụ khi ở trạng thái Chưa bắt đầu (Chưa cập nhật tiến độ)." });
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new { error = "Tên mục tiêu/nhiệm vụ không được để trống." });
                }

                var leadAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == dto.LeadAgencyId);
                bool isGeneral = dto.IsGeneralTask || (leadAgency != null && leadAgency.Code == "ALL_AGENCIES");

                item.Title = dto.Title.Trim();
                item.Section = dto.Section;
                item.Group = dto.Group;
                item.IsOngoing = dto.IsOngoing;
                item.IsGeneralTask = isGeneral;
                item.StartDate = dto.StartDate;
                item.DueDate = dto.DueDate;
                item.LeadAgencyId = dto.LeadAgencyId;
                item.CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>();
                item.Deliverables = dto.Deliverables ?? new List<TaskDeliverable>();

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = $"Cập nhật thành công {item.Code}: {item.Title}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi cập nhật Mục tiêu / Nhiệm vụ", details = ex.Message });
            }
        }
    }
}
