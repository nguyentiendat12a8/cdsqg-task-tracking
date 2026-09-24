using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/legaldocuments")]
    public class LegalDocumentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Luật", "Nghị định", "Thông tư", "Quyết định", "Nghị quyết", "Chỉ thị", "Khác"
        };

        private static readonly HashSet<string> AllowedEffectStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Còn hiệu lực", "Hết hiệu lực toàn bộ", "Hết hiệu lực một phần", "Chưa có hiệu lực"
        };

        private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Thể chế số", "Chính phủ số", "Kinh tế số", "Xã hội số", "Hạ tầng số", "Dữ liệu số", "Khác"
        };

        private static readonly HashSet<string> AllowedScopes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Toàn quốc", "Bộ/Ngành", "Địa phương"
        };

        public LegalDocumentController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Seed sample legal documents if DB table is empty (Disabled to allow permanent user deletion)
        /// </summary>
        private async Task EnsureSeedDataAsync()
        {
            await Task.CompletedTask;
        }

        private LegalDocumentDto MapToDto(LegalDocument entity)
        {
            List<LegalDocumentAttachmentDto> attachments = new();
            if (!string.IsNullOrWhiteSpace(entity.AttachmentsJson))
            {
                try
                {
                    attachments = JsonSerializer.Deserialize<List<LegalDocumentAttachmentDto>>(entity.AttachmentsJson) ?? new();
                }
                catch { }
            }

            return new LegalDocumentDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Title = entity.Title,
                DocumentType = entity.DocumentType,
                IssuingAgencyId = entity.IssuingAgencyId,
                IssuingAgencyName = entity.IssuingAgencyName,
                DraftingAgencyId = entity.DraftingAgencyId,
                DraftingAgencyName = entity.DraftingAgencyName,
                SignerName = entity.SignerName,
                SignerTitle = entity.SignerTitle,
                IssuedDate = entity.IssuedDate,
                EffectiveDate = entity.EffectiveDate,
                EffectStatus = entity.EffectStatus,
                Field = entity.Field,
                Scope = entity.Scope,
                Attachments = attachments,
                Notes = entity.Notes,
                CreatedByAgencyId = entity.CreatedByAgencyId,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// GET /api/legaldocuments
        /// Lấy danh sách Văn bản QPPL theo 3 cấp phân quyền dữ liệu, có bộ lọc & phân trang server-side.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLegalDocuments(
            [FromQuery] string? searchQuery = null,
            [FromQuery] string? documentType = null,
            [FromQuery] string? effectStatus = null,
            [FromQuery] string? field = null,
            [FromQuery] Guid? issuingAgencyId = null,
            [FromQuery] Guid? draftingAgencyId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? userAgencyId = null,
            [FromQuery] string? userRole = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            await EnsureSeedDataAsync();

            var query = _context.LegalDocuments.AsQueryable();

            // 1. DATA ACCESS PERMISSION: All authenticated accounts can view all legal documents
            // (Creation, modification, and deletion are restricted to Admin on frontend & backend write endpoints)

            // 2. FILTERS
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                query = query.Where(d =>
                    d.Code.ToLower().Contains(q) ||
                    d.Title.ToLower().Contains(q) ||
                    (d.SignerName != null && d.SignerName.ToLower().Contains(q)) ||
                    (d.IssuingAgencyName != null && d.IssuingAgencyName.ToLower().Contains(q)) ||
                    (d.DraftingAgencyName != null && d.DraftingAgencyName.ToLower().Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(documentType))
            {
                query = query.Where(d => d.DocumentType == documentType);
            }

            if (!string.IsNullOrWhiteSpace(effectStatus))
            {
                query = query.Where(d => d.EffectStatus == effectStatus);
            }

            if (!string.IsNullOrWhiteSpace(field))
            {
                query = query.Where(d => d.Field == field);
            }

            if (issuingAgencyId.HasValue)
            {
                query = query.Where(d => d.IssuingAgencyId == issuingAgencyId.Value);
            }

            if (draftingAgencyId.HasValue)
            {
                query = query.Where(d => d.DraftingAgencyId == draftingAgencyId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.IssuedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.IssuedDate <= toDate.Value);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.IssuedDate ?? d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            return Ok(new
            {
                items = dtos,
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        /// <summary>
        /// GET /api/legaldocuments/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLegalDocumentById(Guid id)
        {
            var doc = await _context.LegalDocuments.FindAsync(id);
            if (doc == null) return NotFound(new { message = "Không tìm thấy văn bản quy phạm pháp luật." });
            return Ok(MapToDto(doc));
        }

        /// <summary>
        /// GET /api/legaldocuments/dashboard-stats
        /// Biểu đồ & Widget thống kê VB QPPL trên trang chủ theo 3 cấp tài khoản
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats(
            [FromQuery] Guid? userAgencyId = null,
            [FromQuery] string? userRole = null,
            [FromQuery] string? documentType = null,
            [FromQuery] DateTime? issuedFromDate = null,
            [FromQuery] DateTime? issuedToDate = null,
            [FromQuery] DateTime? effectiveFromDate = null,
            [FromQuery] DateTime? effectiveToDate = null,
            [FromQuery] Guid? issuingAgencyId = null,
            [FromQuery] Guid? draftingAgencyId = null,
            [FromQuery] string? field = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            await EnsureSeedDataAsync();

            var baseQuery = _context.LegalDocuments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(documentType))
            {
                baseQuery = baseQuery.Where(d => d.DocumentType == documentType);
            }

            if (!string.IsNullOrWhiteSpace(field))
            {
                baseQuery = baseQuery.Where(d => d.Field == field);
            }

            var startIssued = issuedFromDate ?? fromDate;
            if (startIssued.HasValue)
            {
                baseQuery = baseQuery.Where(d => d.IssuedDate >= startIssued.Value);
            }

            var endIssued = issuedToDate ?? toDate;
            if (endIssued.HasValue)
            {
                baseQuery = baseQuery.Where(d => d.IssuedDate <= endIssued.Value);
            }

            if (effectiveFromDate.HasValue)
            {
                baseQuery = baseQuery.Where(d => d.EffectiveDate >= effectiveFromDate.Value);
            }

            if (effectiveToDate.HasValue)
            {
                baseQuery = baseQuery.Where(d => d.EffectiveDate <= effectiveToDate.Value);
            }

            if (issuingAgencyId.HasValue && issuingAgencyId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(d => d.IssuingAgencyId == issuingAgencyId.Value);
            }

            if (draftingAgencyId.HasValue && draftingAgencyId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(d => d.DraftingAgencyId == draftingAgencyId.Value);
            }

            var allAgencies = await _context.Agencies.ToListAsync();

            bool isAdmin = string.IsNullOrWhiteSpace(userRole) || userRole.ToLower() == "admin" || userRole == "1";
            Guid? currAgId = userAgencyId;
            var currAgency = currAgId.HasValue ? allAgencies.FirstOrDefault(a => a.Id == currAgId.Value) : null;
            bool isLevel2 = currAgency != null && currAgency.ParentId == null;
            bool isLevel3 = currAgency != null && currAgency.ParentId != null;

            // 1. DATA ACCESS PERMISSION FOR DASHBOARD: All authenticated accounts view statistics across all legal documents

            var allDocs = await baseQuery.ToListAsync();
            var result = new LegalDocumentChartStatsDto();

            if (isAdmin)
            {
                result.RoleLevel = 1;
                // CẤP 1 (ADMIN): Thống kê số lượng & phân loại VB QPPL của từng cơ quan Cấp 1 (Primary Agencies)
                var primaryAgencies = allAgencies.Where(a => a.ParentId == null && a.Code != "ALL_AGENCIES" && a.Id != Guid.Parse("00000000-0000-0000-0000-000000009999") && !a.Name.Contains("Các bộ, ngành, địa phương")).ToList();

                foreach (var ag in primaryAgencies)
                {
                    var agDocs = allDocs.Where(d => d.IssuingAgencyId == ag.Id || d.DraftingAgencyId == ag.Id).ToList();
                    var typeCounts = agDocs.GroupBy(d => d.DocumentType)
                                           .ToDictionary(g => g.Key, g => g.Count());
                    var fieldCounts = agDocs.Where(d => !string.IsNullOrWhiteSpace(d.Field))
                                           .GroupBy(d => d.Field!)
                                           .ToDictionary(g => g.Key, g => g.Count());

                    bool isProv = ag.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Province ||
                                 ag.Type.ToString().Equals("Province", StringComparison.OrdinalIgnoreCase) ||
                                 ag.Name.StartsWith("Tỉnh") ||
                                 ag.Name.StartsWith("Thành phố") ||
                                 ag.Name.StartsWith("UBND");

                    var dto = new AgencyLegalDocStatDto
                    {
                        AgencyId = ag.Id,
                        AgencyName = ag.Name,
                        AgencyType = isProv ? "Province" : "Ministry",
                        TotalCount = agDocs.Count,
                        TypeCounts = typeCounts,
                        ByFieldCounts = fieldCounts
                    };

                    result.AgencyStats.Add(dto);
                    if (isProv)
                    {
                        result.ProvincesStats.Add(dto);
                    }
                    else
                    {
                        result.MinistriesStats.Add(dto);
                    }
                }
            }
            else if (isLevel2 && currAgId.HasValue)
            {
                result.RoleLevel = 2;
                // CẤP 2 (BỘ / NGÀNH / TỈNH): Thống kê số lượng & phân loại VB QPPL của từng cơ quan trực thuộc
                var subAgencies = allAgencies.Where(a => a.ParentId == currAgId.Value).ToList();
                // Include level 2 agency itself as first bar
                if (currAgency != null) subAgencies.Insert(0, currAgency);

                bool isCurrProv = currAgency != null && (currAgency.Type == Cdsqg.Core.Enums.AgencyTypeEnum.Province ||
                                  currAgency.Type.ToString().Equals("Province", StringComparison.OrdinalIgnoreCase) ||
                                  currAgency.Name.StartsWith("Tỉnh") || currAgency.Name.StartsWith("Thành phố") || currAgency.Name.StartsWith("UBND"));

                foreach (var ag in subAgencies)
                {
                    var agDocs = allDocs.Where(d => d.IssuingAgencyId == ag.Id || d.DraftingAgencyId == ag.Id).ToList();
                    var typeCounts = agDocs.GroupBy(d => d.DocumentType)
                                           .ToDictionary(g => g.Key, g => g.Count());
                    var fieldCounts = agDocs.Where(d => !string.IsNullOrWhiteSpace(d.Field))
                                           .GroupBy(d => d.Field!)
                                           .ToDictionary(g => g.Key, g => g.Count());

                    var dto = new AgencyLegalDocStatDto
                    {
                        AgencyId = ag.Id,
                        AgencyName = ag.Name,
                        AgencyType = isCurrProv ? "Province" : "Ministry",
                        TotalCount = agDocs.Count,
                        TypeCounts = typeCounts,
                        ByFieldCounts = fieldCounts
                    };

                    result.AgencyStats.Add(dto);
                    if (isCurrProv)
                    {
                        result.ProvincesStats.Add(dto);
                    }
                    else
                    {
                        result.MinistriesStats.Add(dto);
                    }
                }
            }
            else if (isLevel3 && currAgId.HasValue)
            {
                result.RoleLevel = 3;
                // CẤP 3: Widget danh sách 5 văn bản QPPL mới nhất của cơ quan mình
                var myDocs = allDocs
                    .Where(d => d.IssuingAgencyId == currAgId.Value || d.DraftingAgencyId == currAgId.Value || d.Scope == "Toàn quốc")
                    .OrderByDescending(d => d.IssuedDate ?? d.CreatedAt)
                    .Take(5)
                    .Select(MapToDto)
                    .ToList();

                result.RecentDocuments = myDocs;
            }
            else
            {
                result.RoleLevel = 1;
            }

            result.CategoryCounts = allDocs
                .Where(d => !string.IsNullOrWhiteSpace(d.DocumentType))
                .GroupBy(d => d.DocumentType)
                .ToDictionary(g => g.Key, g => g.Count());

            result.TotalCount = allDocs.Count;
            return Ok(result);
        }

        /// <summary>
        /// POST /api/legaldocuments
        /// Tạo mới văn bản QPPL (Chỉ tài khoản Cấp 1 Admin mới có quyền tạo).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLegalDocument([FromBody] CreateLegalDocumentDto dto, [FromQuery] string? userRole = null)
        {
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                bool isAdminUser = userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole == "1";
                if (!isAdminUser)
                {
                    return StatusCode(403, new { message = "Chỉ tài khoản Cấp 1 (Admin) mới có quyền nhập mới văn bản Quy Phạm Pháp Luật." });
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Số ký hiệu và Tên văn bản." });
            }

            var cleanCode = dto.Code.Trim();
            bool codeExists = await _context.LegalDocuments.AnyAsync(d => d.Code.ToLower() == cleanCode.ToLower());
            if (codeExists)
            {
                return BadRequest(new { message = $"Số ký hiệu '{cleanCode}' đã tồn tại trong hệ thống. Vui lòng nhập số ký hiệu khác." });
            }

            var entity = new LegalDocument
            {
                Code = cleanCode,
                Title = dto.Title.Trim(),
                DocumentType = dto.DocumentType ?? "Quyết định",
                IssuingAgencyId = dto.IssuingAgencyId,
                IssuingAgencyName = dto.IssuingAgencyName,
                DraftingAgencyId = dto.DraftingAgencyId,
                DraftingAgencyName = dto.DraftingAgencyName,
                SignerName = dto.SignerName,
                SignerTitle = dto.SignerTitle,
                IssuedDate = dto.IssuedDate,
                EffectiveDate = dto.EffectiveDate,
                EffectStatus = dto.EffectStatus ?? "Còn hiệu lực",
                Field = dto.Field,
                Scope = dto.Scope ?? "Toàn quốc",
                Notes = dto.Notes,
                AttachmentsJson = JsonSerializer.Serialize(dto.Attachments ?? new()),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.LegalDocuments.AddAsync(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(entity));
        }

        /// <summary>
        /// PUT /api/legaldocuments/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLegalDocument(Guid id, [FromBody] UpdateLegalDocumentDto dto, [FromQuery] string? userRole = null)
        {
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                bool isAdminUser = userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole == "1";
                if (!isAdminUser)
                {
                    return StatusCode(403, new { message = "Chỉ tài khoản Cấp 1 (Admin) mới có quyền chỉnh sửa văn bản Quy Phạm Pháp Luật." });
                }
            }

            var entity = await _context.LegalDocuments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Không tìm thấy văn bản QPPL." });

            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Số ký hiệu và Tên văn bản." });
            }

            var cleanCode = dto.Code.Trim();
            bool codeExists = await _context.LegalDocuments.AnyAsync(d => d.Id != id && d.Code.ToLower() == cleanCode.ToLower());
            if (codeExists)
            {
                return BadRequest(new { message = $"Số ký hiệu '{cleanCode}' đã tồn tại ở một văn bản khác trong hệ thống." });
            }

            entity.Code = cleanCode;
            entity.Title = dto.Title.Trim();
            entity.DocumentType = dto.DocumentType ?? "Quyết định";
            entity.IssuingAgencyId = dto.IssuingAgencyId;
            entity.IssuingAgencyName = dto.IssuingAgencyName;
            entity.DraftingAgencyId = dto.DraftingAgencyId;
            entity.DraftingAgencyName = dto.DraftingAgencyName;
            entity.SignerName = dto.SignerName;
            entity.SignerTitle = dto.SignerTitle;
            entity.IssuedDate = dto.IssuedDate;
            entity.EffectiveDate = dto.EffectiveDate;
            entity.EffectStatus = dto.EffectStatus ?? "Còn hiệu lực";
            entity.Field = dto.Field;
            entity.Scope = dto.Scope ?? "Toàn quốc";
            entity.Notes = dto.Notes;
            entity.AttachmentsJson = JsonSerializer.Serialize(dto.Attachments ?? new());
            entity.UpdatedAt = DateTime.UtcNow;

            _context.LegalDocuments.Update(entity);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(entity));
        }

        private Agency? ResolveAgency(string? inputStr, List<Agency> allAgencies)
        {
            if (string.IsNullOrWhiteSpace(inputStr) || inputStr.Trim() == "—") return null;

            var cleanInput = inputStr.Trim();

            // 1. Direct match on Name or Code (case-insensitive)
            var match = allAgencies.FirstOrDefault(a =>
                a.Name.Trim().Equals(cleanInput, StringComparison.OrdinalIgnoreCase) ||
                a.Code.Trim().Equals(cleanInput, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            // 2. Standardize "&" vs "và"
            var stdInput = cleanInput.ToLower().Replace("&", "và").Replace("  ", " ");
            match = allAgencies.FirstOrDefault(a => {
                var stdName = a.Name.Trim().ToLower().Replace("&", "và").Replace("  ", " ");
                return stdName == stdInput;
            });
            if (match != null) return match;

            return null;
        }

        /// <summary>
        /// POST /api/legaldocuments/bulk-import
        /// Nhập hàng loạt văn bản QPPL từ Excel (Yêu cầu duy nhất số ký hiệu & cơ quan hợp lệ)
        /// </summary>
        [HttpPost("bulk-import")]
        public async Task<IActionResult> BulkImportLegalDocuments([FromBody] BulkImportLegalDocumentRequestDto req, [FromQuery] string? userRole = null)
        {
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                bool isAdminUser = userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole == "1";
                if (!isAdminUser)
                {
                    return StatusCode(403, new { message = "Chỉ tài khoản Cấp 1 (Admin) mới có quyền import văn bản Quy Phạm Pháp Luật." });
                }
            }

            if (req == null || req.Items == null || req.Items.Count == 0)
            {
                return BadRequest(new { message = "Dữ liệu import rỗng. Vui lòng kiểm tra lại file Excel." });
            }

            var existingCodes = await _context.LegalDocuments.Select(d => d.Code.ToLower()).ToListAsync();
            var existingCodesSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
            var allAgencies = await _context.Agencies.ToListAsync();

            var seenBatchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var errors = new List<LegalDocumentImportErrorDto>();
            var entitiesToInsert = new List<LegalDocument>();

            foreach (var item in req.Items)
            {
                var itemErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(item.Code))
                {
                    itemErrors.Add("Số ký hiệu không được để trống");
                }

                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    itemErrors.Add("Trích yếu nội dung không được để trống");
                }

                if (!string.IsNullOrWhiteSpace(item.Code))
                {
                    var cleanCode = item.Code.Trim();
                    if (existingCodesSet.Contains(cleanCode))
                    {
                        itemErrors.Add($"Số ký hiệu '{cleanCode}' đã tồn tại trong hệ thống");
                    }
                    else if (seenBatchCodes.Contains(cleanCode))
                    {
                        itemErrors.Add($"Số ký hiệu '{cleanCode}' bị trùng lặp trong file import");
                    }
                    else
                    {
                        seenBatchCodes.Add(cleanCode);
                    }
                }

                // Validate DocumentType against options
                string docType = "Quyết định";
                if (!string.IsNullOrWhiteSpace(item.DocumentType) && item.DocumentType.Trim() != "—")
                {
                    var cleanType = item.DocumentType.Trim();
                    var matchedType = AllowedDocumentTypes.FirstOrDefault(t => t.Equals(cleanType, StringComparison.OrdinalIgnoreCase));
                    if (matchedType != null)
                    {
                        docType = matchedType;
                    }
                    else
                    {
                        itemErrors.Add($"Loại văn bản '{cleanType}' không thuộc danh sách lựa chọn (Luật, Nghị định, Thông tư, Quyết định, Nghị quyết, Chỉ thị, Khác)");
                    }
                }

                // Validate EffectStatus against options
                string effectStatus = "Còn hiệu lực";
                if (!string.IsNullOrWhiteSpace(item.EffectStatus) && item.EffectStatus.Trim() != "—")
                {
                    var cleanStatus = item.EffectStatus.Trim();
                    var matchedStatus = AllowedEffectStatuses.FirstOrDefault(s => s.Equals(cleanStatus, StringComparison.OrdinalIgnoreCase));
                    if (matchedStatus != null)
                    {
                        effectStatus = matchedStatus;
                    }
                    else
                    {
                        itemErrors.Add($"Trạng thái hiệu lực '{cleanStatus}' không thuộc danh sách lựa chọn (Còn hiệu lực, Hết hiệu lực toàn bộ, Hết hiệu lực một phần, Chưa có hiệu lực)");
                    }
                }

                // Validate Field against options
                string field = "Thể chế số";
                if (!string.IsNullOrWhiteSpace(item.Field) && item.Field.Trim() != "—")
                {
                    var cleanField = item.Field.Trim();
                    var matchedField = AllowedFields.FirstOrDefault(f => f.Equals(cleanField, StringComparison.OrdinalIgnoreCase));
                    if (matchedField != null)
                    {
                        field = matchedField;
                    }
                    else
                    {
                        itemErrors.Add($"Lĩnh vực '{cleanField}' không thuộc danh sách lựa chọn (Thể chế số, Chính phủ số, Kinh tế số, Xã hội số, Hạ tầng số, Dữ liệu số, Khác)");
                    }
                }

                // Validate Scope against options
                string scope = "Toàn quốc";
                if (!string.IsNullOrWhiteSpace(item.Scope) && item.Scope.Trim() != "—")
                {
                    var cleanScope = item.Scope.Trim();
                    var matchedScope = AllowedScopes.FirstOrDefault(s => s.Equals(cleanScope, StringComparison.OrdinalIgnoreCase));
                    if (matchedScope != null)
                    {
                        scope = matchedScope;
                    }
                    else
                    {
                        itemErrors.Add($"Phạm vi áp dụng '{cleanScope}' không thuộc danh sách lựa chọn (Toàn quốc, Bộ/Ngành, Địa phương)");
                    }
                }

                // Validate Issuing Agency existence
                Guid? issuingId = null;
                string? issuingName = null;
                if (!string.IsNullOrWhiteSpace(item.IssuingAgencyName) && item.IssuingAgencyName.Trim() != "—")
                {
                    var ag = ResolveAgency(item.IssuingAgencyName, allAgencies);
                    if (ag != null)
                    {
                        issuingId = ag.Id;
                        issuingName = ag.Name;
                    }
                    else
                    {
                        itemErrors.Add($"Cơ quan ban hành '{item.IssuingAgencyName.Trim()}' không tồn tại trong danh mục cơ quan");
                    }
                }

                // Validate Drafting Agency existence
                Guid? draftingId = null;
                string? draftingName = null;
                if (!string.IsNullOrWhiteSpace(item.DraftingAgencyName) && item.DraftingAgencyName.Trim() != "—")
                {
                    var ag = ResolveAgency(item.DraftingAgencyName, allAgencies);
                    if (ag != null)
                    {
                        draftingId = ag.Id;
                        draftingName = ag.Name;
                    }
                    else
                    {
                        itemErrors.Add($"Cơ quan dự thảo '{item.DraftingAgencyName.Trim()}' không tồn tại trong danh mục cơ quan");
                    }
                }

                if (itemErrors.Count > 0)
                {
                    errors.Add(new LegalDocumentImportErrorDto
                    {
                        RowIndex = item.RowIndex,
                        Code = item.Code ?? string.Empty,
                        ErrorDetail = string.Join("; ", itemErrors)
                    });
                }
                else
                {
                    entitiesToInsert.Add(new LegalDocument
                    {
                        Code = item.Code.Trim(),
                        Title = item.Title.Trim(),
                        DocumentType = docType,
                        IssuingAgencyId = issuingId,
                        IssuingAgencyName = issuingName,
                        DraftingAgencyId = draftingId,
                        DraftingAgencyName = draftingName,
                        SignerName = item.SignerName?.Trim(),
                        SignerTitle = item.SignerTitle?.Trim(),
                        IssuedDate = item.IssuedDate,
                        EffectiveDate = item.EffectiveDate,
                        EffectStatus = effectStatus,
                        Field = field,
                        Scope = scope,
                        Notes = item.Notes?.Trim(),
                        AttachmentsJson = "[]",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            if (errors.Count > 0)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Phát hiện {errors.Count} dòng bị lỗi trong file Excel.",
                    errors = errors
                });
            }

            await _context.LegalDocuments.AddRangeAsync(entitiesToInsert);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Đã nhập thành công {entitiesToInsert.Count} văn bản QPPL.",
                importedCount = entitiesToInsert.Count
            });
        }

        /// <summary>
        /// DELETE /api/legaldocuments/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLegalDocument(Guid id, [FromQuery] string? userRole = null)
        {
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                bool isAdminUser = userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole == "1";
                if (!isAdminUser)
                {
                    return StatusCode(403, new { message = "Chỉ tài khoản Cấp 1 (Admin) mới có quyền xóa văn bản Quy Phạm Pháp Luật." });
                }
            }

            var entity = await _context.LegalDocuments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Không tìm thấy văn bản QPPL." });

            _context.LegalDocuments.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa văn bản QPPL thành công." });
        }

        /// <summary>
        /// DELETE /api/legaldocuments/clear-all
        /// Xóa toàn bộ văn bản quy phạm pháp luật demo
        /// </summary>
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllLegalDocuments([FromQuery] string? userRole = null)
        {
            if (!string.IsNullOrWhiteSpace(userRole))
            {
                bool isAdminUser = userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) || userRole == "1";
                if (!isAdminUser)
                {
                    return StatusCode(403, new { message = "Chỉ tài khoản Cấp 1 (Admin) mới có quyền xóa tất cả văn bản." });
                }
            }

            _context.LegalDocuments.RemoveRange(_context.LegalDocuments);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sạch toàn bộ văn bản QPPL demo thành công." });
        }

        /// <summary>
        /// POST /api/legaldocuments/upload-file
        /// Upload tệp đính kèm văn bản QPPL
        /// </summary>
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadLegalAttachment(IFormFile file, [FromQuery] string fileType = "Văn bản chính")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không tìm thấy file tải lên." });
            }

            var fileUrl = await _fileStorageService.SaveDocumentFileAsync(file);
            var attachment = new LegalDocumentAttachmentDto
            {
                FileName = file.FileName,
                CleanName = file.FileName,
                FileUrl = fileUrl,
                FileSize = file.Length,
                FileType = string.IsNullOrWhiteSpace(fileType) ? "Văn bản chính" : fileType
            };

            return Ok(attachment);
        }
    }
}
