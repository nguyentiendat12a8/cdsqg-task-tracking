using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/agency")]
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
            [FromQuery] Guid? parentId = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            var query = _context.Agencies.Include(a => a.ParentAgency).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                var matchingAgencyInfo = await _context.Agencies
                    .Where(a => (!string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(s)) || a.Name.ToLower().Contains(s))
                    .Select(a => new { a.Id, a.ParentId })
                    .ToListAsync();

                var matchedIds = new HashSet<Guid>(matchingAgencyInfo.Select(a => a.Id));

                // Add parent agency IDs if any matched agency is a sub-agency
                foreach (var item in matchingAgencyInfo)
                {
                    if (item.ParentId.HasValue && item.ParentId.Value != Guid.Empty)
                    {
                        matchedIds.Add(item.ParentId.Value);
                    }
                }

                // Add all sub-agency IDs if any matched agency is a parent agency
                var matchedParentIds = matchingAgencyInfo.Where(a => !a.ParentId.HasValue || a.ParentId.Value == Guid.Empty).Select(a => a.Id).ToList();
                if (matchedParentIds.Count > 0)
                {
                    var childIds = await _context.Agencies
                        .Where(a => a.ParentId.HasValue && matchedParentIds.Contains(a.ParentId.Value))
                        .Select(a => a.Id)
                        .ToListAsync();

                    foreach (var cid in childIds)
                    {
                        matchedIds.Add(cid);
                    }
                }

                query = query.Where(a => matchedIds.Contains(a.Id));
            }

            if (parentId.HasValue && parentId.Value != Guid.Empty)
            {
                query = query.Where(a => a.ParentId == parentId.Value);
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
                    .OrderByDescending(a => a.Code == "ALL_AGENCIES")
                    .ThenBy(a => a.Name)
                    .Skip((pNum - 1) * pSize)
                    .Take(pSize)
                    .ToListAsync();

                var pagedResult = pagedItems.Select(a => new
                {
                    a.Id,
                    a.Code,
                    a.Name,
                    a.Type,
                    a.ParentId,
                    ParentName = a.ParentAgency?.Name,
                    a.ContactPersons,
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

            var list = await query
                .OrderByDescending(a => a.Code == "ALL_AGENCIES")
                .ThenBy(a => a.Name)
                .ToListAsync();
            var resultList = list.Select(a => new
            {
                a.Id,
                a.Code,
                a.Name,
                a.Type,
                a.ParentId,
                ParentName = a.ParentAgency?.Name,
                a.ContactPersons,
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

            if (!string.IsNullOrWhiteSpace(agency.Code) && await _context.Agencies.AnyAsync(a => a.Code == agency.Code))
            {
                return BadRequest(new { error = $"Mã cơ quan '{agency.Code}' đã tồn tại trong hệ thống." });
            }

            agency.Id = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(agency.Code))
            {
                agency.Code = "AG-" + Guid.NewGuid().ToString("N")[..8];
            }
            if (agency.ParentId == Guid.Empty) agency.ParentId = null;
            agency.CreatedAt = DateTime.UtcNow;
            agency.ContactPersons ??= new List<AgencyContactPerson>();
            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            return Ok(agency);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] Agency dto)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            if (!string.IsNullOrWhiteSpace(dto.Code) && !string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                int leadCount = await _context.GoalTaskItems.CountAsync(g => g.LeadAgencyId == id);
                int urgeCount = await _context.TaskUrgeLogs.CountAsync(u => u.LeadAgencyId == id);
                int totalUsage = leadCount + urgeCount;

                if (totalUsage > 0)
                {
                    return BadRequest(new { error = $"Không thể thay đổi mã cơ quan vì đã có {totalUsage} mục tiêu / nhiệm vụ / đôn đốc đang sử dụng." });
                }
                existing.Code = dto.Code;
            }
            else if (string.IsNullOrWhiteSpace(existing.Code))
            {
                existing.Code = "AG-" + Guid.NewGuid().ToString("N")[..8];
            }

            existing.Name = dto.Name;
            existing.Type = dto.Type;
            existing.ParentId = (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty) ? dto.ParentId : null;
            existing.IsActive = dto.IsActive;
            existing.ContactPersons = dto.ContactPersons ?? new List<AgencyContactPerson>();

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
            int childAgencyCount = await _context.Agencies.CountAsync(a => a.ParentId == id);
            int totalUsage = leadCount + urgeCount + childAgencyCount;

            if (totalUsage > 0)
            {
                return BadRequest(new { error = $"Không thể xóa cơ quan này vì đang được sử dụng hoặc có cơ quan trực thuộc liên quan." });
            }

            _context.Agencies.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
