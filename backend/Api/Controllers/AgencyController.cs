using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// Thứ tự sắp xếp cơ quan:
        /// 1: Đặc biệt (Special, ALL_*)
        /// 2: Bộ, ngành (Ministry)
        /// 3: Địa phương (Province / Danh mục tỉnh/thành)
        /// 4: Đơn vị trực thuộc / Nội bộ (Internal / Child agencies)
        /// 5: Khác (Other)
        /// </summary>
        public static int GetAgencyTypeSortOrder(Agency a)
        {
            if (a.ParentId.HasValue && a.ParentId.Value != Guid.Empty)
                return 4;
            if (a.Type == AgencyTypeEnum.Special || (!string.IsNullOrEmpty(a.Code) && a.Code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase)))
                return 1;
            if (a.Type == AgencyTypeEnum.Ministry)
                return 2;
            if (a.Type == AgencyTypeEnum.Province || ProvinceCatalog.IsFixed(a))
                return 3;
            if (a.Type == AgencyTypeEnum.Internal)
                return 4;
            return 5;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgencies(
            [FromQuery] string? search = null,
            [FromQuery] Guid? parentId = null,
            [FromQuery] Guid? agencyId = null,
            [FromQuery] bool? excludeSpecial = null,
            [FromQuery] string? contactFilter = null,
            [FromQuery] string? planFileFilter = null,
            [FromQuery] string? fiveYearPlanFilter = null,
            [FromQuery] string? responseDocFilter = null,
            [FromQuery] string? updated1266Filter = null,
            [FromQuery] string? annualPlanFilter = null,
            [FromQuery] string? architectureFilter = null,
            [FromQuery] string? typeFilter = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] Guid? userAgencyId = null,
            [FromQuery] bool? restrictForUser = null,
            [FromQuery] bool? prioritizeContacts = null)
        {
            var query = _context.Agencies.Include(a => a.ParentAgency).AsNoTrackingWithIdentityResolution();

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

            var viComparer = StringComparer.Create(new CultureInfo("vi-VN"), false);
            var list = await query.ToListAsync();
            list = list
                .OrderBy(GetAgencyTypeSortOrder)
                .ThenBy(a => ProvinceCatalog.DisplayName(a), viComparer)
                .ToList();

            if (excludeSpecial == true)
            {
                list = list.Where(a => a.Type != AgencyTypeEnum.Special && (string.IsNullOrEmpty(a.Code) || !a.Code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase) || a.ParentId.HasValue)).ToList();
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
                    list = list.Where(a => (a.FiveYearPlan != null && (a.FiveYearPlan.IsIssued || (a.FiveYearPlan.Files != null && a.FiveYearPlan.Files.Count > 0))) || (a.PlanFiles != null && a.PlanFiles.Count > 0)).ToList();
                }
                else if (planFileFilter == "NOT_SENT")
                {
                    list = list.Where(a => (a.FiveYearPlan == null || (!a.FiveYearPlan.IsIssued && (a.FiveYearPlan.Files == null || a.FiveYearPlan.Files.Count == 0))) && (a.PlanFiles == null || a.PlanFiles.Count == 0)).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(fiveYearPlanFilter) && fiveYearPlanFilter != "ALL")
            {
                if (fiveYearPlanFilter == "SENT" || fiveYearPlanFilter == "PROVIDED" || fiveYearPlanFilter == "ISSUED")
                {
                    list = list.Where(a => (a.FiveYearPlan != null && (a.FiveYearPlan.IsIssued || a.FiveYearPlan.HasReviewedPlan || !string.IsNullOrWhiteSpace(a.FiveYearPlan.DocumentNumber) || (a.FiveYearPlan.Files != null && a.FiveYearPlan.Files.Count > 0))) || (a.PlanFiles != null && a.PlanFiles.Count > 0)).ToList();
                }
                else if (fiveYearPlanFilter == "NOT_SENT" || fiveYearPlanFilter == "NOT_PROVIDED" || fiveYearPlanFilter == "NOT_ISSUED")
                {
                    list = list.Where(a => (a.FiveYearPlan == null || (!a.FiveYearPlan.IsIssued && !a.FiveYearPlan.HasReviewedPlan && string.IsNullOrWhiteSpace(a.FiveYearPlan.DocumentNumber) && (a.FiveYearPlan.Files == null || a.FiveYearPlan.Files.Count == 0))) && (a.PlanFiles == null || a.PlanFiles.Count == 0)).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(responseDocFilter) && responseDocFilter != "ALL")
            {
                if (responseDocFilter == "PROVIDED" || responseDocFilter == "SENT" || responseDocFilter == "YES")
                {
                    list = list.Where(a => a.FiveYearPlan != null && !string.IsNullOrWhiteSpace(a.FiveYearPlan.ResponseDocument)).ToList();
                }
                else if (responseDocFilter == "NOT_PROVIDED" || responseDocFilter == "NOT_SENT" || responseDocFilter == "NO")
                {
                    list = list.Where(a => a.FiveYearPlan == null || string.IsNullOrWhiteSpace(a.FiveYearPlan.ResponseDocument)).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(updated1266Filter) && updated1266Filter != "ALL")
            {
                if (updated1266Filter == "UPDATED" || updated1266Filter == "YES")
                {
                    list = list.Where(a => a.FiveYearPlan != null && a.FiveYearPlan.IsUpdated1266).ToList();
                }
                else if (updated1266Filter == "NOT_UPDATED" || updated1266Filter == "NO")
                {
                    list = list.Where(a => a.FiveYearPlan == null || !a.FiveYearPlan.IsUpdated1266).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(annualPlanFilter) && annualPlanFilter != "ALL")
            {
                if (annualPlanFilter == "PROVIDED" || annualPlanFilter == "ISSUED")
                {
                    list = list.Where(a => a.AnnualPlans != null && a.AnnualPlans.Any(p => p.IsIssued || (p.Files != null && p.Files.Count > 0))).ToList();
                }
                else if (annualPlanFilter == "NOT_PROVIDED" || annualPlanFilter == "NOT_ISSUED")
                {
                    list = list.Where(a => a.AnnualPlans == null || !a.AnnualPlans.Any(p => p.IsIssued || (p.Files != null && p.Files.Count > 0))).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(architectureFilter) && architectureFilter != "ALL")
            {
                if (architectureFilter == "PROVIDED" || architectureFilter == "ISSUED")
                {
                    list = list.Where(a => a.DigitalArchitecture != null && (a.DigitalArchitecture.IsIssued || (a.DigitalArchitecture.Files != null && a.DigitalArchitecture.Files.Count > 0) || !string.IsNullOrWhiteSpace(a.DigitalArchitecture.DocumentNumber))).ToList();
                }
                else if (architectureFilter == "NOT_PROVIDED" || architectureFilter == "NOT_ISSUED")
                {
                    list = list.Where(a => a.DigitalArchitecture == null || (!a.DigitalArchitecture.IsIssued && (a.DigitalArchitecture.Files == null || a.DigitalArchitecture.Files.Count == 0) && string.IsNullOrWhiteSpace(a.DigitalArchitecture.DocumentNumber))).ToList();
                }
            }

            if (prioritizeContacts == true)
            {
                list = list
                    .OrderBy(GetAgencyTypeSortOrder)
                    .ThenByDescending(a => a.ContactPersons != null && a.ContactPersons.Any(c => !string.IsNullOrWhiteSpace(c.Name)))
                    .ThenBy(a => ProvinceCatalog.DisplayName(a), viComparer)
                    .ToList();
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
                        a.ShortCode,
                        Name = ProvinceCatalog.DisplayName(a),
                        a.Type,
                        a.ParentId,
                        IsFixedProvince = ProvinceCatalog.IsFixed(a),
                        ParentName = a.ParentAgency?.Name,
                        a.ContactPersons,
                        a.PlanFiles,
                        a.FiveYearPlan,
                        a.AnnualPlans,
                        a.DigitalArchitecture,
                        HasContacts = a.ContactPersons != null && a.ContactPersons.Count > 0,
                        HasFiveYearPlan = (a.FiveYearPlan != null && (a.FiveYearPlan.IsIssued || a.FiveYearPlan.HasReviewedPlan || !string.IsNullOrWhiteSpace(a.FiveYearPlan.DocumentNumber) || (a.FiveYearPlan.Files != null && a.FiveYearPlan.Files.Count > 0))) || (a.PlanFiles != null && a.PlanFiles.Count > 0),
                        HasResponseDoc = a.FiveYearPlan != null && !string.IsNullOrWhiteSpace(a.FiveYearPlan.ResponseDocument),
                        IsUpdated1266 = a.FiveYearPlan != null && a.FiveYearPlan.IsUpdated1266,
                        HasAnnualPlan = a.AnnualPlans != null && a.AnnualPlans.Any(p => p.IsIssued || (p.Files != null && p.Files.Count > 0)),
                        HasDigitalArchitecture = a.DigitalArchitecture != null && (a.DigitalArchitecture.IsIssued || (a.DigitalArchitecture.Files != null && a.DigitalArchitecture.Files.Count > 0) || !string.IsNullOrWhiteSpace(a.DigitalArchitecture.DocumentNumber)),
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
                a.ShortCode,
                Name = ProvinceCatalog.DisplayName(a),
                a.Type,
                a.ParentId,
                        IsFixedProvince = ProvinceCatalog.IsFixed(a),
                ParentName = a.ParentAgency?.Name,
                a.ContactPersons,
                a.PlanFiles,
                a.FiveYearPlan,
                a.AnnualPlans,
                a.DigitalArchitecture,
                HasContacts = a.ContactPersons != null && a.ContactPersons.Count > 0,
                HasFiveYearPlan = (a.FiveYearPlan != null && (a.FiveYearPlan.IsIssued || a.FiveYearPlan.HasReviewedPlan || !string.IsNullOrWhiteSpace(a.FiveYearPlan.DocumentNumber) || (a.FiveYearPlan.Files != null && a.FiveYearPlan.Files.Count > 0))) || (a.PlanFiles != null && a.PlanFiles.Count > 0),
                HasResponseDoc = a.FiveYearPlan != null && !string.IsNullOrWhiteSpace(a.FiveYearPlan.ResponseDocument),
                IsUpdated1266 = a.FiveYearPlan != null && a.FiveYearPlan.IsUpdated1266,
                HasAnnualPlan = a.AnnualPlans != null && a.AnnualPlans.Any(p => p.IsIssued || (p.Files != null && p.Files.Count > 0)),
                HasDigitalArchitecture = a.DigitalArchitecture != null && (a.DigitalArchitecture.IsIssued || (a.DigitalArchitecture.Files != null && a.DigitalArchitecture.Files.Count > 0) || !string.IsNullOrWhiteSpace(a.DigitalArchitecture.DocumentNumber)),
                a.IsActive,
                a.CreatedAt,
                usedCount = leadCounts.GetValueOrDefault(a.Id, 0) + urgeCounts.GetValueOrDefault(a.Id, 0)
            });
            return Ok(resultList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromBody] Agency agency)
        {
            if (ProvinceCatalog.Conflicts(agency)) return StatusCode(403, new { error = "Danh mục 34 tỉnh/thành cố định không được thêm mới." });
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

            var shortCodeError = await ApplyShortCodeAsync(agency, agency.ShortCode, null);
            if (shortCodeError != null) return BadRequest(new { error = shortCodeError });

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

        /// <summary>
        /// Viết tắt dùng đánh mã mục tiêu/nhiệm vụ (NV-BCA-01). Không đổi mã các mục đã có; mục mới tạo sau mới dùng viết tắt mới.
        /// </summary>
        private async Task<string?> ApplyShortCodeAsync(Agency target, string? value, Guid? currentId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                target.ShortCode = null;
                return null;
            }
            var shortCode = ItemCodes.NormalizeShortCode(value);
            if (shortCode == null)
                return "Viết tắt chỉ gồm chữ in hoa và số (tối đa 15 ký tự), không dùng \"C\" (mã chung).";
            if (await _context.Agencies.AnyAsync(a => a.Id != currentId && a.ShortCode == shortCode))
                return $"Viết tắt '{shortCode}' đã được dùng cho cơ quan khác.";
            target.ShortCode = shortCode;
            return null;
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] Agency dto)
        {
            var existing = await _context.Agencies.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Không tìm thấy Cơ quan." });

            bool isFixedSystemAgency = ProvinceCatalog.IsFixed(existing) || string.Equals(existing.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_AGENCIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES_UBND", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES_DIRECT", StringComparison.OrdinalIgnoreCase) ||
                                       (!existing.ParentId.HasValue && existing.Name.StartsWith("Bộ", StringComparison.OrdinalIgnoreCase) &&
                                        (existing.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                         existing.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)));

            if (ProvinceCatalog.IsFixed(existing) && (dto.Code != existing.Code || ProvinceCatalog.Normalize(dto.Name) != ProvinceCatalog.Normalize(existing.Name) || dto.Type != existing.Type || dto.ParentId != existing.ParentId || dto.IsActive != existing.IsActive))
                return StatusCode(403, new { error = "Danh mục 34 tỉnh/thành không được sửa mã, tên, phân loại, cấp trên hoặc trạng thái hoạt động." });
            if (!ProvinceCatalog.IsFixed(existing) && ProvinceCatalog.Conflicts(dto))
                return StatusCode(403, new { error = "Không được chuyển cơ quan khác vào danh mục tỉnh/thành cố định." });

            if (isFixedSystemAgency)
            {
                // Cho phép cập nhật danh sách đầu mối và file kế hoạch của cơ quan cố định, giữ nguyên thông tin cơ quan gốc
                existing.ContactPersons = dto.ContactPersons ?? new List<AgencyContactPerson>();
                if (dto.PlanFiles != null) existing.PlanFiles = dto.PlanFiles;
            if (dto.FiveYearPlan != null) existing.FiveYearPlan = dto.FiveYearPlan;
            if (dto.AnnualPlans != null) existing.AnnualPlans = dto.AnnualPlans;
            if (dto.DigitalArchitecture != null) existing.DigitalArchitecture = dto.DigitalArchitecture;
                if (dto.FiveYearPlan != null) existing.FiveYearPlan = dto.FiveYearPlan;
                if (dto.AnnualPlans != null) existing.AnnualPlans = dto.AnnualPlans;
                if (dto.DigitalArchitecture != null) existing.DigitalArchitecture = dto.DigitalArchitecture;
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

            var updateShortCodeError = await ApplyShortCodeAsync(existing, dto.ShortCode, id);
            if (updateShortCodeError != null) return BadRequest(new { error = updateShortCodeError });

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
            if (dto.FiveYearPlan != null) existing.FiveYearPlan = dto.FiveYearPlan;
            if (dto.AnnualPlans != null) existing.AnnualPlans = dto.AnnualPlans;
            if (dto.DigitalArchitecture != null) existing.DigitalArchitecture = dto.DigitalArchitecture;

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

            bool isFixedSystemAgency = ProvinceCatalog.IsFixed(existing) || string.Equals(existing.Code, "bkhcn", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_AGENCIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_PROVINCES_UBND", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Code, "ALL_MINISTRIES_DIRECT", StringComparison.OrdinalIgnoreCase) ||
                                       (!existing.ParentId.HasValue && existing.Name.StartsWith("Bộ", StringComparison.OrdinalIgnoreCase) &&
                                        (existing.Name.Contains("Khoa học và Công nghệ", StringComparison.OrdinalIgnoreCase) ||
                                         existing.Name.Contains("Khoa học & Công nghệ", StringComparison.OrdinalIgnoreCase)));

            if (ProvinceCatalog.IsFixed(existing)) return StatusCode(403, new { error = "Danh mục 34 tỉnh/thành cố định không thể xóa." });
            if (isFixedSystemAgency)
            {
                return BadRequest(new { error = "Cơ quan / Đơn vị hệ thống cố định không thể xóa." });
            }

            var usages = await GetAgencyUsagesAsync(existing);
            if (usages.Count > 0)
            {
                return Conflict(new
                {
                    error = "Không thể xóa cơ quan/đơn vị đã phát sinh dữ liệu: " + string.Join("; ", usages) +
                            ". Nếu không còn sử dụng, hãy chuyển sang trạng thái ngừng hoạt động thay vì xóa.",
                    usages
                });
            }

            _context.Agencies.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        /// <summary>
        /// Liệt kê mọi dữ liệu đã phát sinh gắn với cơ quan/đơn vị. Danh sách rỗng mới được phép xóa.
        /// Phải kiểm tra đủ vì nhiều liên kết không có khóa ngoại chặn (jsonb) hoặc bị SetNull/Cascade âm thầm khi xóa.
        /// </summary>
        private async Task<List<string>> GetAgencyUsagesAsync(Agency agency)
        {
            var id = agency.Id;
            var idText = id.ToString();
            var usages = new List<string>();
            void Add(int count, string label) { if (count > 0) usages.Add($"{label} ({count})"); }

            Add(await _context.Agencies.CountAsync(a => a.ParentId == id), "cơ quan/đơn vị trực thuộc");
            Add(await _context.GoalTaskItems.CountAsync(g => g.LeadAgencyId == id), "mục tiêu/nhiệm vụ chủ trì");
            Add(await _context.GoalTaskItems.CountAsync(g => g.AssignedAgencyId == id), "mục tiêu/nhiệm vụ đồng chủ trì");

            var jsonRefs = await _context.Database.SqlQuery<int>($@"
                SELECT count(*)::int AS ""Value"" FROM ""GoalTaskItems""
                WHERE ""CoordinatingAgencyIds"" @> jsonb_build_array({idText}::text)
                   OR ""SubAgencyIds"" @> jsonb_build_array({idText}::text)
                   OR jsonb_exists(coalesce(""AgencyDeliverables"", '{{}}'::jsonb), {idText}::text)").SingleAsync();
            Add(jsonRefs, "mục tiêu/nhiệm vụ phối hợp, đơn vị trực thuộc hoặc sản phẩm riêng");

            Add(await _context.AgencyTaskExecutions.CountAsync(e => e.AgencyId == id || e.AssignedAgencyId == id), "phân giao/kết quả thực hiện");
            Add(await _context.ProgressLogs.CountAsync(p => p.AgencyId == id), "báo cáo tiến độ");
            Add(await _context.TaskUrgeLogs.CountAsync(u => u.LeadAgencyId == id), "văn bản đôn đốc");
            Add(await _context.Users.CountAsync(u => u.AgencyId == id), "tài khoản người dùng");
            Add(await _context.Notifications.CountAsync(n => n.AgencyId == id), "thông báo");
            Add(await _context.DataImportLogs.CountAsync(l => l.AgencyId == id), "lịch sử nhập dữ liệu");
            Add(await _context.LegalDocuments.CountAsync(d => d.IssuingAgencyId == id || d.DraftingAgencyId == id || d.CreatedByAgencyId == id), "văn bản pháp lý");
            Add(await _context.AiDataSuggestions.CountAsync(s => s.TargetAgencyId == id), "dữ liệu AI đề xuất");
            Add(await _context.AiAlertFeeds.CountAsync(f => f.DeletedAt == null && f.AgencyIdsJson.Contains(idText)), "nguồn cảnh báo AI");

            if (HasPlanData(agency)) usages.Add("kế hoạch/kiến trúc số đã cập nhật hoặc tệp đính kèm");
            return usages;
        }

        private static bool HasPlanData(Agency a)
        {
            static bool AnyFiles(List<AgencyPlanFile>? files) => files != null && files.Count > 0;
            if (AnyFiles(a.PlanFiles)) return true;
            var f = a.FiveYearPlan;
            if (f != null && (f.IsIssued || f.HasReviewedPlan || f.IsUpdated1266 || f.IssueDate.HasValue || AnyFiles(f.Files) ||
                              !string.IsNullOrWhiteSpace(f.ResponseDocument) || !string.IsNullOrWhiteSpace(f.DocumentNumber) ||
                              !string.IsNullOrWhiteSpace(f.Notes))) return true;
            if ((a.AnnualPlans ?? new List<AgencyAnnualPlan>()).Any(p => p != null && (p.IsIssued || p.IssueDate.HasValue || AnyFiles(p.Files) ||
                                       !string.IsNullOrWhiteSpace(p.DocumentNumber) || !string.IsNullOrWhiteSpace(p.Notes)))) return true;
            var d = a.DigitalArchitecture;
            return d != null && (d.IsIssued || d.IssueDate.HasValue || AnyFiles(d.Files) ||
                                 !string.IsNullOrWhiteSpace(d.DocumentNumber) || !string.IsNullOrWhiteSpace(d.Version) ||
                                 !string.IsNullOrWhiteSpace(d.Notes));
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

            if (dto.FiveYearPlan != null)
            {
                existing.FiveYearPlan = dto.FiveYearPlan;
            }
            if (dto.AnnualPlans != null)
            {
                existing.AnnualPlans = dto.AnnualPlans;
            }
            if (dto.DigitalArchitecture != null)
            {
                existing.DigitalArchitecture = dto.DigitalArchitecture;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadAgencyFile(IFormFile file)
        {
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

            return Ok(planFile);
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

        [HttpPost("seed-digital-plans")]
        public async Task<IActionResult> SeedDigitalPlans()
        {
            var agencies = await _context.Agencies.ToListAsync();
            var seedItems = DigitalPlanSeedData.GetSeedItems();
            int updatedCount = 0;
            var updatedAgencies = new List<string>();

            foreach (var item in seedItems)
            {
                var matchedAgency = agencies.FirstOrDefault(a =>
                    a.Name.Equals(item.AgencyName, StringComparison.OrdinalIgnoreCase) ||
                    ProvinceCatalog.DisplayName(a).Equals(item.AgencyName, StringComparison.OrdinalIgnoreCase) ||
                    (item.AgencyName.Contains(a.Name) && a.Name.Length > 3) ||
                    (a.Name.Contains(item.AgencyName) && item.AgencyName.Length > 3)
                );

                if (matchedAgency != null)
                {
                    matchedAgency.FiveYearPlan ??= new AgencyFiveYearPlan();
                    matchedAgency.FiveYearPlan.ResponseDocument = item.ResponseDocument;
                    matchedAgency.FiveYearPlan.HasReviewedPlan = item.HasReviewedPlan;
                    matchedAgency.FiveYearPlan.IsIssued = item.HasReviewedPlan;
                    matchedAgency.FiveYearPlan.DocumentNumber = item.DocumentNumber;
                    matchedAgency.FiveYearPlan.IsUpdated1266 = item.IsUpdated1266;
                    matchedAgency.FiveYearPlan.Notes = item.Notes;
                    matchedAgency.FiveYearPlan.Period = "2026-2030";
                    updatedCount++;
                    updatedAgencies.Add(matchedAgency.Name);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã đồng bộ thành công Kế hoạch CĐS 5 năm cho {updatedCount} cơ quan/đơn vị!", updatedAgencies });
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
                // 9. Thành phố Bắc Ninh
                {
                    new[] { "BACNINH", "Bắc Ninh", "Thành phố Bắc Ninh" },
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
                // 19. Thành phố Quảng Ninh
                {
                    new[] { "QUANGNINH", "Quảng Ninh", "Thành phố Quảng Ninh" },
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
                    new[] { "CANTHO", "Cần Thơ", "Thành phố Cần Thơ" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Ngô Anh Tín", Position = "Giám đốc", Department = "SKHCN", Phone = "0917 939 939", Email = "anhtin@cantho.gov.vn" },
                        new AgencyContactPerson { Name = "Lê Hồng Anh", Position = "Quyền Trưởng phòng", Department = "P CĐS", Phone = "0932 898 964", Email = "lehonganh@cantho.gov.vn" }
                    }
                },
                // 26. Thành phố Hải Phòng
                {
                    new[] { "HAIPHONG", "Hải Phòng", "Thành phố Hải Phòng" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Nguyễn Minh Kha", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0904 081 819", Email = "" },
                        new AgencyContactPerson { Name = "Dương Thị Tú Anh", Position = "Phó Trưởng phòng", Department = "P CNTT", Phone = "0979 831 231", Email = "" },
                        new AgencyContactPerson { Name = "Nguyễn Việt Anh", Position = "Chuyên viên", Department = "P CNTT", Phone = "0815 433 168", Email = "" }
                    }
                },
                // 27. Thành phố Hồ Chí Minh
                {
                    new[] { "TPHCM", "Hồ Chí Minh", "TP.HCM", "Thành phố Hồ Chí Minh" },
                    new List<AgencyContactPerson>
                    {
                        new AgencyContactPerson { Name = "Võ Minh Thành", Position = "Phó giám đốc", Department = "Sở KHCN", Phone = "0918 804 410", Email = "vmthanh.skhcn@tphcm.gov.vn" },
                        new AgencyContactPerson { Name = "Nguyễn Trọng Ngân", Position = "Trưởng phòng", Department = "P CĐS", Phone = "0982 971 791", Email = "ngtngan.skhcn@tphcm.gov.vn" },
                        new AgencyContactPerson { Name = "Lê Hoài Nam", Position = "Chuyên viên", Department = "P CĐS", Phone = "0908 270 305", Email = "lhnam.skhcn@tphcm.gov.vn" }
                    }
                },
                // 28. Thành phố Huế
                {
                    new[] { "HUE", "Huế", "Thừa Thiên Huế", "Thành phố Huế" },
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
        public AgencyFiveYearPlan? FiveYearPlan { get; set; }
        public List<AgencyAnnualPlan>? AnnualPlans { get; set; }
        public AgencyDigitalArchitecture? DigitalArchitecture { get; set; }
    }
}
