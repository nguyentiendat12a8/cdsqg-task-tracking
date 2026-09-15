using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/agencies")]
    public class AgencyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgencyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgencies(
            [FromQuery] string? search = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            var query = _context.Agencies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a => 
                    a.Code.ToLower().Contains(s) || 
                    a.Name.ToLower().Contains(s));
            }

            var leadCounts = await _context.GoalTaskItems
                .GroupBy(g => g.LeadAgencyId)
                .Select(g => new { AgencyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AgencyId, x => x.Count);

            var urgeCounts = await _context.TaskUrgeLogs
                .Where(u => u.LeadAgencyId.HasValue)
                .GroupBy(u => u.LeadAgencyId!.Value)
                .Select(u => new { AgencyId = u.Key, Count = u.Count() })
                .ToDictionaryAsync(x => x.AgencyId, x => x.Count);

            if (pageNumber.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                int pNum = pageNumber.Value > 0 ? pageNumber.Value : 1;
                int pSize = pageSize.Value;
                int totalCount = await query.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);

                var pagedItems = await query
                    .OrderBy(a => a.Code)
                    .Skip((pNum - 1) * pSize)
                    .Take(pSize)
                    .ToListAsync();

                var pagedResult = pagedItems.Select(a => new
                {
                    a.Id,
                    a.Code,
                    a.Name,
                    a.Type,
                    a.IsActive,
                    a.CreatedAt,
                    usedCount = leadCounts.GetValueOrDefault(a.Id, 0) + urgeCounts.GetValueOrDefault(a.Id, 0)
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

            var list = await query.OrderBy(a => a.Code).ToListAsync();
            var resultList = list.Select(a => new
            {
                a.Id,
                a.Code,
                a.Name,
                a.Type,
                a.IsActive,
                a.CreatedAt,
                usedCount = leadCounts.GetValueOrDefault(a.Id, 0) + urgeCounts.GetValueOrDefault(a.Id, 0)
            });
            return Ok(resultList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromBody] Agency agency)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Agencies.AnyAsync(a => a.Code == agency.Code))
            {
                return BadRequest(new { error = $"Mã cơ quan '{agency.Code}' đã tồn tại trong hệ thống." });
            }

            agency.Id = Guid.NewGuid();
            agency.CreatedAt = DateTime.UtcNow;
            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            return Ok(agency);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] Agency dto)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            // Kiểm tra ràng buộc nếu có Mục tiêu/Nhiệm vụ/Đôn đốc đang sử dụng Cơ quan này và người dùng muốn đổi Mã Cơ quan
            if (!string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                int leadCount = await _context.GoalTaskItems.CountAsync(g => g.LeadAgencyId == id);
                int urgeCount = await _context.TaskUrgeLogs.CountAsync(u => u.LeadAgencyId == id);
                int totalUsage = leadCount + urgeCount;

                if (totalUsage > 0)
                {
                    return BadRequest(new { error = $"Không thể thay đổi mã cơ quan vì đã có {totalUsage} mục tiêu / nhiệm vụ / đôn đốc đang sử dụng." });
                }
            }

            existing.Code = dto.Code;
            existing.Name = dto.Name;
            existing.Type = dto.Type;
            existing.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAgency(Guid id)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            int leadCount = await _context.GoalTaskItems.CountAsync(g => g.LeadAgencyId == id);
            int urgeCount = await _context.TaskUrgeLogs.CountAsync(u => u.LeadAgencyId == id);
            int totalUsage = leadCount + urgeCount;

            if (totalUsage > 0)
            {
                return BadRequest(new { error = $"Không thể xóa cơ quan này vì đã có {totalUsage} mục tiêu / nhiệm vụ / đôn đốc đang sử dụng." });
            }

            _context.Agencies.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
