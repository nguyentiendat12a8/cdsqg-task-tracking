using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Cdsqg.Application.Services;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/agencies")]
    [Route("api/agency")]
    public class AgencyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public AgencyController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgencies(
            [FromQuery] string? search = null,
            [FromQuery] Guid? parentId = null,
            [FromQuery] Guid? agencyId = null,
            [FromQuery] bool? excludeSpecial = null,
            [FromQuery] string? contactFilter = null,
            [FromQuery] string? planFileFilter = null,
            [FromQuery] string? typeFilter = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] Guid? userAgencyId = null,
            [FromQuery] bool? restrictForUser = null)
        {
            var query = _context.Agencies.Include(a => a.ParentAgency).AsQueryable();

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(a => a.Id == agencyId.Value);
            }

            if (parentId.HasValue && parentId.Value != Guid.Empty)
            {
                query = query.Where(a => a.ParentId == parentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "ALL" && Enum.TryParse<AgencyTypeEnum>(typeFilter, true, out var parsedType))
            {
                query = query.Where(a => a.Type == parsedType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                var matchingAgencyInfo = await _context.Agencies
                    .Where(a => (!string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(s)) || a.Name.ToLower().Contains(s))
                    .Select(a => new { a.Id, a.ParentId })
                    .ToListAsync();

                var matchedIds = new HashSet<Guid>(matchingAgencyInfo.Select(a => a.Id));

                foreach (var item in matchingAgencyInfo)
                {
                    if (item.ParentId.HasValue && item.ParentId.Value != Guid.Empty)
                    {
                        matchedIds.Add(item.ParentId.Value);
                    }
                }

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

            var list = await query
                .OrderByDescending(a => a.Code == "ALL_AGENCIES" || a.Code == "ALL_MINISTRIES" || a.Code == "ALL_PROVINCES" || a.Code == "ALL_PROVINCES_UBND" || a.Code == "ALL_MINISTRIES_DIRECT")
                .ThenBy(a => a.Name)
                .ToListAsync();

            if (excludeSpecial == true)
            {
                list = list.Where(a => a.Code != "ALL_AGENCIES" && a.Code != "ALL_MINISTRIES" && a.Code != "ALL_PROVINCES" && a.Code != "ALL_PROVINCES_UBND" && a.Code != "ALL_MINISTRIES_DIRECT").ToList();
            }

            if (restrictForUser == true && userAgencyId.HasValue && userAgencyId.Value != Guid.Empty)
            {
                var uId = userAgencyId.Value;
                var userAg = list.FirstOrDefault(a => a.Id == uId);
                bool isUserBkhcn = userAg != null && (
                    (!string.IsNullOrEmpty(userAg.Code) && userAg.Code.Equals("bkhcn", StringComparison.OrdinalIgnoreCase)) ||
                    userAg.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                    userAg.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)
                );

                if (!isUserBkhcn)
                {
                    var bkhcnAg = list.FirstOrDefault(a =>
                        (!string.IsNullOrEmpty(a.Code) && a.Code.Equals("bkhcn", StringComparison.OrdinalIgnoreCase)) ||
                        a.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase));

                    Guid? bkhcnId = bkhcnAg?.Id;
                    var bkhcnChildIds = new HashSet<Guid>(list
                        .Where(a => (bkhcnId.HasValue && a.ParentId == bkhcnId.Value) ||
                                    (a.ParentAgency != null && (
                                        (!string.IsNullOrEmpty(a.ParentAgency.Code) && a.ParentAgency.Code.Equals("bkhcn", StringComparison.OrdinalIgnoreCase)) ||
                                        a.ParentAgency.Name.Contains("Khoa học", StringComparison.OrdinalIgnoreCase)
                                    )))
                        .Select(a => a.Id));

                    list = list.Where(a =>
                        a.Id == uId ||
                        a.ParentId == uId ||
                        (userAg != null && userAg.ParentId.HasValue && a.Id == userAg.ParentId.Value) ||
                        (bkhcnId.HasValue && a.Id == bkhcnId.Value) ||
                        bkhcnChildIds.Contains(a.Id)
                    ).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(contactFilter) && contactFilter != "ALL")
            {
                if (contactFilter == "PROVIDED")
                {
                    list = list.Where(a => a.ContactPersons != null && a.ContactPersons.Count > 0).ToList();
                }
                else if (contactFilter == "NOT_PROVIDED")
                {
                    list = list.Where(a => a.ContactPersons == null || a.ContactPersons.Count == 0).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(planFileFilter) && planFileFilter != "ALL")
            {
                if (planFileFilter == "SENT")
                {
                    list = list.Where(a => a.PlanFiles != null && a.PlanFiles.Count > 0).ToList();
                }
                else if (planFileFilter == "NOT_SENT")
                {
                    list = list.Where(a => a.PlanFiles == null || a.PlanFiles.Count == 0).ToList();
                }
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
                int totalCount = list.Count;
                int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);

                var pagedItems = list
                    .Skip((pNum - 1) * pSize)
                    .Take(pSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.Code,
                        a.Name,
                        a.Type,
                        a.ParentId,
                        ParentName = a.ParentAgency?.Name,
                        a.ContactPersons,
                        a.PlanFiles,
                        a.IsActive,
                        a.CreatedAt,
                        usedCount = leadCounts.GetValueOrDefault(a.Id, 0) + urgeCounts.GetValueOrDefault(a.Id, 0)
                    });

                return Ok(new
                {
                    items = pagedItems,
                    totalCount,
                    pageNumber = pNum,
                    pageSize = pSize,
                    totalPages
                });
            }

            var resultList = list.Select(a => new
            {
                a.Id,
                a.Code,
                a.Name,
                a.Type,
                a.ParentId,
                ParentName = a.ParentAgency?.Name,
                a.ContactPersons,
                a.PlanFiles,
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

            if (agency.ParentId.HasValue && agency.ParentId.Value != Guid.Empty)
            {
                var parentAg = await _context.Agencies.FindAsync(agency.ParentId.Value);
                if (parentAg == null)
                {
                    return BadRequest(new { error = "Cơ quan cấp trên không hợp lệ." });
                }

                bool isParentBkhcn = string.Equals(parentAg.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                     parentAg.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                     parentAg.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase);

                if (!isParentBkhcn)
                {
                    return BadRequest(new { error = "Chỉ cho phép chọn Cơ quan cấp trên là 'Bộ Khoa học và Công nghệ'." });
                }
            }

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
            agency.PlanFiles ??= new List<AgencyPlanFile>();
            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            return Ok(agency);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] Agency dto)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            bool isFixedSystemAgency = string.Equals(existing.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_AGENCIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES_UBND", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES_DIRECT", StringComparison.OrdinalIgnoreCase) ||
                                       (!existing.ParentId.HasValue && existing.Name.StartsWith("Bộ", StringComparison.OrdinalIgnoreCase) &&
                                        (existing.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                         existing.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)));

            if (isFixedSystemAgency)
            {
                // Cho phép cập nhật danh sách đầu mối và file kế hoạch của cơ quan cố định, giữ nguyên thông tin cơ quan gốc
                existing.ContactPersons = dto.ContactPersons ?? new List<AgencyContactPerson>();
                if (dto.PlanFiles != null) existing.PlanFiles = dto.PlanFiles;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }

            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                var parentAg = await _context.Agencies.FindAsync(dto.ParentId.Value);
                if (parentAg == null)
                {
                    return BadRequest(new { error = "Cơ quan cấp trên không hợp lệ." });
                }

                bool isParentBkhcn = string.Equals(parentAg.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                     (!parentAg.ParentId.HasValue && parentAg.Name.StartsWith("Bộ", StringComparison.OrdinalIgnoreCase) &&
                                      (parentAg.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                       parentAg.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)));

                if (!isParentBkhcn)
                {
                    return BadRequest(new { error = "Chỉ cho phép chọn Cơ quan cấp trên là 'Bộ Khoa học và Công nghệ'." });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Code) && !string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                var codeDuplicate = await _context.Agencies.AnyAsync(a => a.Id != id && a.Code.ToLower() == dto.Code.ToLower());
                if (codeDuplicate)
                {
                    return BadRequest(new { error = $"Mã cơ quan '{dto.Code}' đã tồn tại trong hệ thống." });
                }
                existing.Code = dto.Code;
            }
            else if (string.IsNullOrWhiteSpace(existing.Code))
            {
                existing.Code = "AG-" + Guid.NewGuid().ToString("N")[..8];
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                existing.Name = dto.Name;
            }
            if (dto.Type != 0)
            {
                existing.Type = dto.Type;
            }
            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                existing.ParentId = dto.ParentId;
            }
            existing.IsActive = dto.IsActive;
            if (dto.ContactPersons != null) existing.ContactPersons = dto.ContactPersons;
            if (dto.PlanFiles != null) existing.PlanFiles = dto.PlanFiles;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("{id:guid}/plan-files")]
        public async Task<IActionResult> UploadPlanFile(Guid id, IFormFile file)
        {
            var agency = await _context.Agencies.FindAsync(id);
            if (agency == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Tệp đính kèm không hợp lệ." });
            }

            var fileUrl = await _fileStorageService.SaveDocumentFileAsync(file);
            var planFile = new AgencyPlanFile
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FileUrl = fileUrl,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            var list = (agency.PlanFiles ?? new List<AgencyPlanFile>()).ToList();
            list.Add(planFile);
            agency.PlanFiles = list;
            await _context.SaveChangesAsync();

            return Ok(planFile);
        }

        [HttpDelete("{id:guid}/plan-files/{fileId:guid}")]
        public async Task<IActionResult> DeletePlanFile(Guid id, Guid fileId)
        {
            var agency = await _context.Agencies.FindAsync(id);
            if (agency == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            var list = (agency.PlanFiles ?? new List<AgencyPlanFile>()).ToList();
            var fileToRemove = list.FirstOrDefault(f => f.Id == fileId);
            if (fileToRemove != null)
            {
                list.Remove(fileToRemove);
                agency.PlanFiles = list;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAgency(Guid id)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            bool isFixedSystemAgency = string.Equals(existing.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_AGENCIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES_UBND", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES_DIRECT", StringComparison.OrdinalIgnoreCase) ||
                                       (!existing.ParentId.HasValue && existing.Name.StartsWith("Bộ", StringComparison.OrdinalIgnoreCase) &&
                                        (existing.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                         existing.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)));

            if (isFixedSystemAgency)
            {
                return BadRequest(new { error = "Cơ quan / Đơn vị hệ thống cố định không thể xóa." });
            }

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

        [HttpPut("{id:guid}/plans-and-contacts")]
        public async Task<IActionResult> UpdatePlansAndContacts(Guid id, [FromBody] UpdatePlansAndContactsDto dto, [FromQuery] Guid? userAgencyId = null)
        {
            var existing = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == id);

            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            if (userAgencyId.HasValue && userAgencyId.Value != Guid.Empty && userAgencyId.Value != id && existing.ParentId != userAgencyId.Value)
            {
                var userAg = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == userAgencyId.Value);
                bool isUserBkhcn = userAg != null && (
                    (!string.IsNullOrEmpty(userAg.Code) && userAg.Code.Equals("bkhcn", StringComparison.OrdinalIgnoreCase)) ||
                    userAg.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                    userAg.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)
                );

                if (!isUserBkhcn)
                {
                    return BadRequest(new { error = "Bạn chỉ có quyền cập nhật thông tin cán bộ đầu mối và Kế hoạch CĐS của đơn vị mình." });
                }
            }

            existing.ContactPersons = dto.ContactPersons ?? new List<AgencyContactPerson>();

            if (dto.PlanFiles != null)
            {
                existing.PlanFiles = dto.PlanFiles;
            }
            else if (dto.PlanFileIds != null)
            {
                var existingFiles = existing.PlanFiles ?? new List<AgencyPlanFile>();
                existing.PlanFiles = existingFiles
                    .Where(f => dto.PlanFileIds.Contains(f.Id))
                    .ToList();
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("upload-plan-file")]
        public async Task<IActionResult> UploadPlanFileLegacy([FromForm] IFormFile file, [FromForm] Guid agencyId)
        {
            if (agencyId == Guid.Empty)
            {
                return BadRequest(new { error = "Mã cơ quan không hợp lệ." });
            }
            return await UploadPlanFile(agencyId, file);
        }

        [HttpDelete("plan-files/{fileId:guid}")]
        public async Task<IActionResult> DeletePlanFileDirect(Guid fileId)
        {
            var agencies = await _context.Agencies.ToListAsync();
            var agency = agencies.FirstOrDefault(a => a.PlanFiles != null && a.PlanFiles.Any(f => f.Id == fileId));

            if (agency != null && agency.PlanFiles != null)
            {
                var list = agency.PlanFiles.ToList();
                var fileToRemove = list.FirstOrDefault(f => f.Id == fileId);
                if (fileToRemove != null)
                {
                    list.Remove(fileToRemove);
                    agency.PlanFiles = list;
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { success = true });
        }
    }

    public class UpdatePlansAndContactsDto
    {
        public List<AgencyContactPerson>? ContactPersons { get; set; }
        public List<AgencyPlanFile>? PlanFiles { get; set; }
        public List<Guid>? PlanFileIds { get; set; }
    }
}
