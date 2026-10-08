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
        public async Task<IActionResult> UploadPlanFileLegacy(IFormFile file, [FromForm] Guid agencyId)
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

        [HttpPost("seed-contacts")]
        public async Task<IActionResult> SeedContactPersons()
        {
            var agencies = await _context.Agencies.ToListAsync();
            int updatedCount = 0;
            var updatedAgencies = new List<string>();

            var seedMap = GetSeedContactMap();

            foreach (var kvp in seedMap)
            {
                var keywords = kvp.Key;
                var contacts = kvp.Value;

                var matchedAgency = agencies.FirstOrDefault(a =>
                    keywords.Any(k =>
                        (!string.IsNullOrEmpty(a.Code) && a.Code.Equals(k, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(a.Name) && a.Name.ToLower().Contains(k.ToLower()))
                    )
                );

                if (matchedAgency != null)
                {
                    matchedAgency.ContactPersons = contacts;
                    updatedCount++;
                    updatedAgencies.Add($"{matchedAgency.Name} ({matchedAgency.Code})");
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã cập nhật thành công thông tin đầu mối cho {updatedCount} cơ quan/đơn vị!", updatedAgencies });
        }

        private static Dictionary<string[], List<AgencyContactPerson>> GetSeedContactMap()
        {
            return new Dictionary<string[], List<AgencyContactPerson>>
            {
                // 1. Bộ Công Thương
                {
                    new[] { "BCT", "Bộ Công Thương", "Công Thương" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Hoàng Ninh", Position = "Phó Cục trưởng", Department = "Cục TMĐT&KTS", Phone = "0912 524 948", Email = "NinhH@moit.gov.vn" },
                        new AgencyContactPerson { Name = "Đinh Anh Tuấn", Position = "Trưởng phòng CĐS", Department = "Cục TMĐT", Phone = "0934 595 333", Email = "AnhDTuan@moit.gov.vn" }
                    }
                },
                // 2. Bộ Khoa học và Công nghệ
                {
                    new[] { "BKHCN", "Bộ Khoa học và Công nghệ", "Khoa học và Công nghệ" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Cục CĐSQG - TTCNTT", Position = "Đơn vị đầu mối", Department = "Cục CĐSQG", Phone = "", Email = "" }
                    }
                },
                // 3. Bộ Nội vụ
                {
                    new[] { "BNV", "Bộ Nội vụ", "Nội vụ" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Đỗ Chi Dũng", Position = "Giám đốc", Department = "TTCNTT", Phone = "0903 239 068", Email = "" },
                        new AgencyContactPerson { Name = "Trịnh Tuấn Chung", Position = "Trưởng phòng", Department = "TT CNTT", Phone = "0903 007 068", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Thu Hương", Position = "Phó Trưởng phòng", Department = "TTCNTT", Phone = "0983 341 213", Email = "" }
                    }
                },
                // 4. Bộ Nông nghiệp và Môi trường (hoặc BNN&PTNT)
                {
                    new[] { "BNNPTNT", "Bộ Nông nghiệp", "Nông nghiệp và Phát triển nông thôn", "Nông nghiệp và Môi trường" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Lê Phú Hà", Position = "Cục trưởng", Department = "Cục CĐS", Phone = "0989 080 937", Email = "lpha@mae.gov.vn" },
                        new AgencyContactPerson { Name = "Bùi Mạnh Khôi", Position = "Trưởng P Tài chính - Thống kê", Department = "Cục CĐS", Phone = "0902 164 919", Email = "bmkhoi@mae.gov.vn" }
                    }
                },
                // 5. Bộ Quốc phòng
                {
                    new[] { "BQP", "Bộ Quốc phòng", "Quốc phòng" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Tùng Hưng", Position = "Thiếu tướng. Phó tư lệnh", Department = "Bộ Tư lệnh 86", Phone = "0983 043 325", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Kiên", Position = "Đại tá. Trưởng phòng Phần mềm và CSDL", Department = "Bộ Tư lệnh 86", Phone = "0985 101 126", Email = "" },
                        new AgencyContactPerson { Name = "Phạm Trọng Linh", Position = "Thượng úy. Trợ lý Phòng phần mềm và CSDL", Department = "Bộ Tư lệnh 86", Phone = "0879 610 286", Email = "" }
                    }
                },
                // 6. Bộ Tài chính
                {
                    new[] { "BTC", "Bộ Tài chính", "Tài chính" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Như Sơn", Position = "Phó Cục trưởng", Department = "CNTT&CĐS", Phone = "0913 382 138", Email = "nguyennhuson@mof.gov.vn" },
                        new AgencyContactPerson { Name = "Hoàng Vương Nam", Position = "Chuyên viên", Department = "P KH-TH", Phone = "0917 232 297", Email = "hoangvuongnam@mof.gov.vn" }
                    }
                },
                // 7. Bộ Văn hóa, Thể thao và Du lịch
                {
                    new[] { "BVHTTDL", "Bộ Văn hóa", "Văn hóa, Thể thao và Du lịch" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Lê Mạnh Hùng", Position = "Phó Giám đốc", Department = "TT CĐS", Phone = "0905 188 881", Email = "lemanhhung@cntt.gov.vn" },
                        new AgencyContactPerson { Name = "Dương Anh Quân", Position = "Trưởng phòng", Department = "TT CĐS", Phone = "0915 091 580", Email = "quanda@cntt.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Việt Hà", Position = "Chuyên viên", Department = "TT CĐS", Phone = "0963 608 069", Email = "hanv@cntt.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Thị Nga", Position = "Chuyên viên", Department = "TT CĐS", Phone = "0973 223 407", Email = "ngant@cntt.gov.vn" }
                    }
                },
                // 8. Bộ Xây dựng
                {
                    new[] { "BXD", "Bộ Xây dựng", "Xây dựng" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Ngọc Quang", Position = "Phó giám đốc", Department = "TTCNTT", Phone = "0989 089 689", Email = "nguyenngocquang@moc.gov.vn" },
                        new AgencyContactPerson { Name = "Trần Thị Thanh Hương", Position = "Chuyên viên", Department = "P CĐS TTCNTT", Phone = "0973 031 435", Email = "huongtt@moc.gov.vn" }
                    }
                },
                // 9. Bắc Ninh
                {
                    new[] { "BACNINH", "Bắc Ninh" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Văn Khánh", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0886 088 666", Email = "khanhnv107@bacninh.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Văn Định", Position = "Chuyên viên", Department = "P CĐS", Phone = "0866 866 953", Email = "dinhnv@bacninh.gov.vn" }
                    }
                },
                // 10. Cà Mau
                {
                    new[] { "CAMAU", "Cà Mau" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Phạm Thống Nhất", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0913 892 892", Email = "skhcn.ptnhat@gmail.com" },
                        new AgencyContactPerson { Name = "Trần Quốc Toản", Position = "Phó Trưởng phòng", Department = "P CNS&BC", Phone = "0944 317 508", Email = "skhcn.tqtoan@camau.gov.vn" }
                    }
                },
                // 11. Đắk Lắk
                {
                    new[] { "DAKLAK", "Đắk Lắk", "Đắc Lắc" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Ra Lan Trương Thanh Hà", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0914 586 911", Email = "" },
                        new AgencyContactPerson { Name = "Lê An Pha", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0836 907 788", Email = "" }
                    }
                },
                // 12. Đồng Tháp
                {
                    new[] { "DONGTHAP", "Đồng Tháp" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Trần Văn Dũng", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0919 785 785", Email = "tvdung@dongthap.gov.vn" },
                        new AgencyContactPerson { Name = "Lê Minh Hiếu", Position = "Chuyên viên", Department = "P CĐS", Phone = "0977 926 457", Email = "leminhhieu@dongthap.gov.vn" }
                    }
                },
                // 13. Hưng Yên
                {
                    new[] { "HUNGYEN", "Hưng Yên" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Đỗ Đình Quang", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0982 256 237", Email = "ddquang@hungyen.gov.vn" },
                        new AgencyContactPerson { Name = "Bùi Anh Lâm", Position = "Trưởng phòng", Department = "P CĐSBCVT", Phone = "0963 688 686", Email = "balam@hungyen.gov.vn" }
                    }
                },
                // 14. Lai Châu
                {
                    new[] { "LAICHAU", "Lai Châu" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Phạm Quang Cường", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0976 819 323", Email = "cuongpq.sokhcn@laichau.gov.vn" },
                        new AgencyContactPerson { Name = "Bùi Thị Lan", Position = "Trưởng phòng", Department = "P Bưu chính, Viễn thông, CNTT", Phone = "", Email = "lanbt.sokhcn@laichau.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Thị Mai Liên", Position = "Chuyên viên", Department = "P Bưu chính, Viễn thông, CNTT", Phone = "0978 637 648", Email = "lienntm.sokhcn@laichau.gov.vn" }
                    }
                },
                // 15. Lạng Sơn
                {
                    new[] { "LANGSON", "Lạng Sơn" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Trần Hữu Giang", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0913 199 838", Email = "thgiang@langson.gov.vn" },
                        new AgencyContactPerson { Name = "Vũ Thùy Dung", Position = "Phó Trưởng phòng", Department = "P CĐS SKHCN", Phone = "0388 653 616", Email = "vtdung@langson.gov.vn" }
                    }
                },
                // 16. Lào Cai
                {
                    new[] { "LAOCAI", "Lào Cai" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Hồng Quang", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0913 287 426", Email = "nguyenhongquang@laocai.gov.vn" },
                        new AgencyContactPerson { Name = "Phùng Mạnh Sang", Position = "Chuyên viên", Department = "P CĐS", Phone = "0852 662 000", Email = "phungmanhsang@laocai.gov.vn" }
                    }
                },
                // 17. Lâm Đồng
                {
                    new[] { "LAMDONG", "Lâm Đồng" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Lê Thanh Liêm", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0941 777 799", Email = "liemlt@lamdong.gov.vn" },
                        new AgencyContactPerson { Name = "Võ Duy Phong", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0911 364 567", Email = "phongvd.skhcn@lamdong.gov.vn" }
                    }
                },
                // 18. Quảng Ngãi
                {
                    new[] { "QUANGNGAI", "Quảng Ngãi" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Quốc Huy Hoàng", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0982 142 211", Email = "nqhhoang-skh@quangngai.gov.vn" },
                        new AgencyContactPerson { Name = "Hà Thanh Tuấn", Position = "Trưởng phòng", Department = "Sở KHCN", Phone = "0905 257 799", Email = "httuan-skh@quangngai.gov.vn" },
                        new AgencyContactPerson { Name = "Đặng Bảo Hy", Position = "Chuyên viên", Department = "P BCVT&CĐS", Phone = "0905 181 088", Email = "" }
                    }
                },
                // 19. Quảng Ninh
                {
                    new[] { "QUANGNINH", "Quảng Ninh" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Trung Tiến", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0913 388 266", Email = "" },
                        new AgencyContactPerson { Name = "Vũ Thị Kim Minh Huệ", Position = "Phó Trưởng phòng", Department = "P Quản lý CNTT&CĐS", Phone = "0912 773 828", Email = "" },
                        new AgencyContactPerson { Name = "Phạm Thị Trang", Position = "Chuyên viên", Department = "P QLCN&CĐS", Phone = "0834 130 448", Email = "" }
                    }
                },
                // 20. Quảng Trị
                {
                    new[] { "QUANGTRI", "Quảng Trị" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Xuân Ngọc", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0935 596 599", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Thành Lê", Position = "Trưởng phòng", Department = "Sở KHCN", Phone = "0912 049 773", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Tiến Thành", Position = "Chuyên viên", Department = "P CNTT", Phone = "0982 576 767", Email = "" }
                    }
                },
                // 21. Sơn La
                {
                    new[] { "SONLA", "Sơn La" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Phạm Quốc Chinh", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0983 002 065", Email = "chinhpq.skhcn@sonla.gov.vn" },
                        new AgencyContactPerson { Name = "Ngô Thị Hồng Hạnh", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0912 109 204", Email = "hanhntn.skhcn@sonla.gov.vn" }
                    }
                },
                // 22. Tây Ninh
                {
                    new[] { "TAYNINH", "Tây Ninh" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Bùi Nguyên Khôi", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0889 691 777", Email = "nguyenkhoi@tayninh.gov.vn" },
                        new AgencyContactPerson { Name = "Tăng Thị Ngọc Em", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0946 700 369", Email = "ngocem@tayninh.gov.vn" }
                    }
                },
                // 23. Thanh Hoá
                {
                    new[] { "THANHHOA", "Thanh Hoá", "Thanh Hóa" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Thị Thu Hà", Position = "Phó Trưởng phòng", Department = "P Quản lý CNTT&CĐS", Phone = "0982 401 828", Email = "hantt.skhcn@thanhhoa.gov.vn" }
                    }
                },
                // 24. Tuyên Quang
                {
                    new[] { "TUYENQUANG", "Tuyên Quang" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Hồ Thị Phương Lan", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0915 160 266", Email = "" },
                        new AgencyContactPerson { Name = "Lê Thành Trung", Position = "Chuyên viên", Department = "P CĐS", Phone = "0979 982 266", Email = "" }
                    }
                },
                // 25. Thành phố Cần Thơ
                {
                    new[] { "CANTHO", "Cần Thơ" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Ngô Anh Tín", Position = "Giám đốc", Department = "SKHCN", Phone = "0917 939 939", Email = "anhtin@cantho.gov.vn" },
                        new AgencyContactPerson { Name = "Lê Hồng Anh", Position = "Quyền Trưởng phòng", Department = "P CĐS", Phone = "0932 898 964", Email = "lehonganh@cantho.gov.vn" }
                    }
                },
                // 26. Thành phố Hải Phòng
                {
                    new[] { "HAIPHONG", "Hải Phòng" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Minh Kha", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0904 081 819", Email = "" },
                        new AgencyContactPerson { Name = "Dương Thị Tú Anh", Position = "Phó Trưởng phòng", Department = "P CNTT", Phone = "0979 831 231", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Việt Anh", Position = "Chuyên viên", Department = "P CNTT", Phone = "0815 433 168", Email = "" }
                    }
                },
                // 27. Thành phố Hồ Chí Minh
                {
                    new[] { "TPHCM", "Hồ Chí Minh", "TP.HCM" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Võ Minh Thành", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0918 804 410", Email = "vmthanh.skhcn@tphcm.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Trọng Ngân", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0982 971 791", Email = "ngtngan.skhcn@tphcm.gov.vn" },
                        new AgencyContactPerson { Name = "Lê Hoài Nam", Position = "Chuyên viên", Department = "P CĐS", Phone = "0908 270 305", Email = "lhnam.skhcn@tphcm.gov.vn" }
                    }
                },
                // 28. Thành phố Huế
                {
                    new[] { "HUE", "Huế", "Thừa Thiên Huế" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Xuân Sơn", Position = "Giám đốc", Department = "SKHCN", Phone = "0914 202 345", Email = "nxson.skhcn@hue.gov.vn" },
                        new AgencyContactPerson { Name = "Phan Nữ Anh Thư", Position = "Phó Trưởng phòng", Department = "P CĐS SKHCN", Phone = "0905 979 772", Email = "pnathu.skhcn@hue.gov.vn" }
                    }
                }
            };
        }
    }

    public class UpdatePlansAndContactsDto
    {
        public List<AgencyContactPerson>? ContactPersons { get; set; }
        public List<AgencyPlanFile>? PlanFiles { get; set; }
        public List<Guid>? PlanFileIds { get; set; }
    }
}
