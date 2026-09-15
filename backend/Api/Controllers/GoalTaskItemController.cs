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
                var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == dto.DocumentId);
                if (doc == null)
                {
                    return BadRequest(new { error = $"Văn bản / Quyết định với ID '{dto.DocumentId}' không tồn tại." });
                }

                if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new { error = "Mã và Tên mục tiêu/nhiệm vụ không được để trống." });
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

                EvaluationTypeEnum evalType = EvaluationTypeEnum.Quantitative;
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

                var newItem = new GoalTaskItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = dto.DocumentId,
                    ItemType = itemType,
                    Code = finalCode,
                    Title = dto.Title.Trim(),
                    Category = dto.Category?.Trim() ?? string.Empty,
                    LeadAgencyId = dto.LeadAgencyId,
                    CoordinatingAgencyIds = dto.CoordinatingAgencyIds ?? new List<Guid>(),
                    UnitId = dto.UnitId,
                    EvaluationType = evalType,
                    CalculationMethod = calcMethod,
                    CustomBaseline = dto.CustomBaseline ?? new Dictionary<string, string>(),
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
        /// Xóa một Mục tiêu (1A) hoặc Nhiệm vụ (1B).
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var item = await _context.GoalTaskItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound(new { error = "Không tìm thấy Mục tiêu / Nhiệm vụ để xóa." });
            }

            _context.GoalTaskItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã xóa thành công {item.Code}: {item.Title}" });
        }
    }
}
