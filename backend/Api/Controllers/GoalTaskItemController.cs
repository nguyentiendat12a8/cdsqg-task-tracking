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

                Guid? unitIdToAssign = dto.UnitId;
                if (!unitIdToAssign.HasValue && !string.IsNullOrWhiteSpace(dto.UnitName))
                {
                    var targetUnit = await _context.Units.FirstOrDefaultAsync(u => u.Name == dto.UnitName || (dto.UnitName == "%" && u.Code == "PERCENT") || (dto.UnitName == "Số lượng" && u.Code == "QTY"));
                    if (targetUnit != null) unitIdToAssign = targetUnit.Id;
                }

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
                    StartDate = dto.IsOngoing ? (dto.StartDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) : dto.StartDate,
                    DueDate = dto.IsOngoing ? (dto.DueDate ?? new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc)) : dto.DueDate,
                    LeadAgencyId = dto.LeadAgencyId,
                    AssignedAgencyId = dto.AssignedAgencyId,
                    CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>(),
                    UnitId = unitIdToAssign,
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

                var assignedUnit = unitIdToAssign.HasValue ? await _context.Units.FirstOrDefaultAsync(u => u.Id == unitIdToAssign.Value) : null;

                return Ok(new
                {
                    id = newItem.Id,
                    documentId = newItem.DocumentId,
                    itemType = newItem.ItemType.ToString(),
                    code = newItem.Code,
                    title = newItem.Title,
                    category = newItem.Category,
                    leadAgencyId = newItem.LeadAgencyId,
                    assignedAgencyId = newItem.AssignedAgencyId,
                    coordinatingAgencyIds = newItem.CoordinatingAgencyIds,
                    unitId = newItem.UnitId,
                    unitName = assignedUnit?.Name ?? dto.UnitName ?? "%",
                    evaluationType = newItem.EvaluationType.ToString(),
                    calculationMethod = newItem.CalculationMethod.ToString(),
                    customBaseline = newItem.CustomBaseline,
                    createdAt = newItem.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Không thể tạo mới Mục tiêu / Nhiệm vụ", details = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/planning/items/{id}/assign
        /// Giao nhiệm vụ cho đơn vị trực thuộc (Cấp 3).
        /// </summary>
        [HttpPost("{id:guid}/assign")]
        public async Task<IActionResult> AssignItem(Guid id, [FromBody] AssignGoalTaskItemRequestDto dto)
        {
            try
            {
                var item = await _context.GoalTaskItems.FirstOrDefaultAsync(i => i.Id == id);
                if (item == null)
                {
                    return NotFound(new { error = "Không tìm thấy Mục tiêu / Nhiệm vụ để giao." });
                }

                if (dto.AssignedAgencyId.HasValue && dto.AssignedAgencyId.Value != Guid.Empty)
                {
                    var assignedAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == dto.AssignedAgencyId.Value);
                    if (assignedAgency == null)
                    {
                        return BadRequest(new { error = "Không tìm thấy đơn vị trực thuộc được chọn." });
                    }
                    if (!assignedAgency.ParentId.HasValue)
                    {
                        return BadRequest(new { error = "Đơn vị được chọn phải là đơn vị trực thuộc (Cấp 3)." });
                    }
                    item.AssignedAgencyId = dto.AssignedAgencyId.Value;
                }
                else
                {
                    item.AssignedAgencyId = null;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã giao cho đơn vị trực thuộc thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Không thể giao đơn vị trực thuộc", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/planning/items/{id}
        /// Xóa một Mục tiêu (1A), Nhiệm vụ (1B) hoặc Sub-Task.
        /// Chỉ cho phép xóa khi ở trạng thái Chưa bắt đầu (chưa cập nhật tiến độ).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            try
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

                // Clean up any related TaskUrgeLogs, TargetBaselines, or ProgressLogs for target item & children
                var urgeLogs = await _context.TaskUrgeLogs.Where(u => allTargetIds.Contains(u.GoalTaskId)).ToListAsync();
                if (urgeLogs.Count > 0)
                {
                    _context.TaskUrgeLogs.RemoveRange(urgeLogs);
                }

                var baselines = await _context.TargetBaselines.Where(b => allTargetIds.Contains(b.GoalTaskId)).ToListAsync();
                if (baselines.Count > 0)
                {
                    _context.TargetBaselines.RemoveRange(baselines);
                }

                var logs = await _context.ProgressLogs.Where(p => allTargetIds.Contains(p.GoalTaskId)).ToListAsync();
                if (logs.Count > 0)
                {
                    _context.ProgressLogs.RemoveRange(logs);
                }

                if (childSubTaskIds.Count > 0)
                {
                    var childItems = await _context.GoalTaskItems.Where(sub => sub.ParentId == id).ToListAsync();
                    _context.GoalTaskItems.RemoveRange(childItems);
                }

                _context.GoalTaskItems.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = $"Đã xóa thành công {item.Code}: {item.Title}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Không thể xóa Mục tiêu / Nhiệm vụ", details = ex.Message });
            }
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
                item.StartDate = dto.IsOngoing ? (dto.StartDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)) : dto.StartDate;
                item.DueDate = dto.IsOngoing ? (dto.DueDate ?? new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc)) : dto.DueDate;
                item.LeadAgencyId = dto.LeadAgencyId;
                item.AssignedAgencyId = dto.AssignedAgencyId;
                item.CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>();
                item.Deliverables = dto.Deliverables ?? new List<TaskDeliverable>();

                if (dto.UnitId.HasValue)
                {
                    item.UnitId = dto.UnitId;
                }
                else if (!string.IsNullOrWhiteSpace(dto.UnitName))
                {
                    var targetUnit = await _context.Units.FirstOrDefaultAsync(u => u.Name == dto.UnitName || (dto.UnitName == "%" && u.Code == "PERCENT"));
                    if (targetUnit != null) item.UnitId = targetUnit.Id;
                }

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
