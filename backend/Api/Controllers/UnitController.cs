using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUnits(
            [FromQuery] string? search = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            var query = _context.Units.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u => 
                    u.Code.ToLower().Contains(s) || 
                    u.Name.ToLower().Contains(s));
            }

            var goalTaskItemCounts = await _context.GoalTaskItems
                .Where(g => g.UnitId.HasValue)
                .GroupBy(g => g.UnitId!.Value)
                .Select(g => new { UnitId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UnitId, x => x.Count);

            if (pageNumber.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                int pNum = pageNumber.Value > 0 ? pageNumber.Value : 1;
                int pSize = pageSize.Value;
                int totalCount = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);

                var pagedItems = await query
                    .OrderBy(u => u.Code)
                    .Skip((pNum - 1) * pSize)
                    .Take(pSize)
                    .ToListAsync();

                var pagedResult = pagedItems.Select(u => new
                {
                    u.Id,
                    u.Code,
                    u.Name,
                    u.DataType,
                    u.IsActive,
                    usedCount = goalTaskItemCounts.GetValueOrDefault(u.Id, 0)
                });

                return Ok(new
                {
                    items = pagedResult,
                    totalCount,
                    pageNumber = pNum,
                    pageSize = pSize,
                    totalPages
                });
            }

            var list = await query.OrderBy(u => u.Code).ToListAsync();
            var resultList = list.Select(u => new
            {
                u.Id,
                u.Code,
                u.Name,
                u.DataType,
                u.IsActive,
                usedCount = goalTaskItemCounts.GetValueOrDefault(u.Id, 0)
            });
            return Ok(resultList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromBody] UnitDictionary unit)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Units.AnyAsync(u => u.Code == unit.Code))
            {
                return BadRequest(new { error = $"Mã đơn vị tính '{unit.Code}' đã tồn tại." });
            }

            unit.Id = Guid.NewGuid();
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return Ok(unit);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UnitDictionary dto)
        {
            var existing = await _context.Units.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Đơn vị tính." });

            // Kiểm tra ràng buộc nếu có Mục tiêu/Nhiệm vụ đang sử dụng Đơn vị tính này và người dùng muốn đổi Kiểu dữ liệu
            if (existing.DataType != dto.DataType)
            {
                int usedCount = await _context.GoalTaskItems.CountAsync(g => g.UnitId == id);
                if (usedCount > 0)
                {
                    return BadRequest(new { error = $"Không thể thay đổi kiểu dữ liệu của đơn vị tính này vì đã có {usedCount} mục tiêu / nhiệm vụ đang sử dụng." });
                }
            }

            existing.Code = dto.Code;
            existing.Name = dto.Name;
            existing.DataType = dto.DataType;
            existing.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUnit(Guid id)
        {
            var existing = await _context.Units.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Đơn vị tính." });

            int usedCount = await _context.GoalTaskItems.CountAsync(g => g.UnitId == id);
            if (usedCount > 0)
            {
                return BadRequest(new { error = $"Không thể xóa đơn vị tính này vì đã có {usedCount} mục tiêu / nhiệm vụ đang sử dụng." });
            }

            _context.Units.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
