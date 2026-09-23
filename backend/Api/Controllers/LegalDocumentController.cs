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

        public LegalDocumentController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Seed sample legal documents if DB table is empty
        /// </summary>
        private async Task EnsureSeedDataAsync()
        {
            if (await _context.LegalDocuments.CountAsync() >= 8) return;

            var agencies = await _context.Agencies.ToListAsync();
            var bkhcn = agencies.FirstOrDefault(a => a.Name.Contains("Khoa học và Công nghệ")) ?? agencies.FirstOrDefault();
            var bca = agencies.FirstOrDefault(a => a.Name.Contains("Công an"));
            var subUnit = agencies.FirstOrDefault(a => a.ParentId != null) ?? bkhcn;
            var tphcm = agencies.FirstOrDefault(a => a.Name.Contains("Hồ Chí Minh") || a.Name.Contains("TPHCM") || a.Code == "TPHCM");

            var existingCodes = await _context.LegalDocuments.Select(d => d.Code).ToListAsync();
            var sampleDocs = new List<LegalDocument>();

            void AddIfMissing(LegalDocument doc)
            {
                if (!existingCodes.Contains(doc.Code))
                {
                    sampleDocs.Add(doc);
                }
            }

            AddIfMissing(new LegalDocument
            {
                Code = "52/2023/QH15",
                Title = "Luật Giao dịch Điện tử năm 2023",
                DocumentType = "Luật",
                IssuingAgencyId = bkhcn?.Id,
                IssuingAgencyName = bkhcn?.Name ?? "Bộ Khoa học và Công nghệ",
                DraftingAgencyId = subUnit?.Id ?? bkhcn?.Id,
                DraftingAgencyName = subUnit?.Name ?? "Cục Chuyển đổi số Quốc gia",
                SignerName = "Vương Đình Huệ",
                SignerTitle = "Chủ tịch Quốc hội",
                IssuedDate = new DateTime(2023, 6, 22),
                EffectiveDate = new DateTime(2024, 7, 1),
                EffectStatus = "Còn hiệu lực",
                Field = "Thể chế số",
                Scope = "Toàn quốc",
                Notes = "Luật khung nền tảng cho giao dịch điện tử và dữ liệu số",
                AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                {
                    new LegalDocumentAttachmentDto { FileName = "Luat_52_2023_QH15.pdf", CleanName = "Luật Giao dịch Điện tử 52/2023/QH15.pdf", FileUrl = "/uploads/Luat_52_2023.pdf", FileSize = 4120000, FileType = "Văn bản chính" }
                })
            });

            AddIfMissing(new LegalDocument
            {
                Code = "1266/QĐ-TTg",
                Title = "Quyết định phê duyệt Đề án Phát triển và Theo dõi Chiến lược Chuyển đổi số Quốc gia",
                DocumentType = "Quyết định",
                IssuingAgencyId = bkhcn?.Id,
                IssuingAgencyName = bkhcn?.Name ?? "Bộ Khoa học và Công nghệ",
                DraftingAgencyId = subUnit?.Id ?? bkhcn?.Id,
                DraftingAgencyName = subUnit?.Name ?? "Cục Chuyển đổi số Quốc gia",
                SignerName = "Trần Lưu Quang",
                SignerTitle = "Phó Thủ tướng Chính phủ",
                IssuedDate = new DateTime(2026, 7, 14),
                EffectiveDate = new DateTime(2026, 7, 14),
                EffectStatus = "Còn hiệu lực",
                Field = "Thể chế số",
                Scope = "Toàn quốc",
                Notes = "Văn bản chỉ đạo khung chiến lược chuyển đổi số quốc gia 2026-2030",
                AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                {
                    new LegalDocumentAttachmentDto { FileName = "1266_QD_TTg_Chinhthuc.pdf", CleanName = "Quyết định 1266/QĐ-TTg (Văn bản chính).pdf", FileUrl = "/uploads/1266_QD_TTg.pdf", FileSize = 2450112, FileType = "Văn bản chính" },
                    new LegalDocumentAttachmentDto { FileName = "Phu_luc_Chi_tieu_1266.pdf", CleanName = "Phụ lục I - Danh mục Chỉ tiêu Kế hoạch.pdf", FileUrl = "/uploads/Phu_luc_Chi_tieu.pdf", FileSize = 1150000, FileType = "Phụ lục" }
                })
            });

            AddIfMissing(new LegalDocument
            {
                Code = "15/2026/NĐ-CP",
                Title = "Nghị định quy định chi tiết thi hành Luật Giao dịch Điện tử về Hạ tầng và Dữ liệu số",
                DocumentType = "Nghị định",
                IssuingAgencyId = bkhcn?.Id,
                IssuingAgencyName = bkhcn?.Name ?? "Bộ Khoa học và Công nghệ",
                DraftingAgencyId = bkhcn?.Id,
                DraftingAgencyName = bkhcn?.Name ?? "Bộ Khoa học và Công nghệ",
                SignerName = "Phạm Minh Chính",
                SignerTitle = "Thủ tướng Chính phủ",
                IssuedDate = new DateTime(2026, 3, 15),
                EffectiveDate = new DateTime(2026, 5, 1),
                EffectStatus = "Còn hiệu lực",
                Field = "Dữ liệu số",
                Scope = "Toàn quốc",
                Notes = "Quy định quy chuẩn kết nối và chia sẻ dữ liệu dùng chung",
                AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                {
                    new LegalDocumentAttachmentDto { FileName = "Nghidinh_15_2026_ND_CP.pdf", CleanName = "Nghị định 15/2026/NĐ-CP.pdf", FileUrl = "/uploads/ND_15_2026.pdf", FileSize = 3200100, FileType = "Văn bản chính" }
                })
            });

            AddIfMissing(new LegalDocument
            {
                Code = "08/2026/TT-BKHCN",
                Title = "Thông tư hướng dẫn chuẩn hóa cấu trúc dữ liệu và báo cáo tiến độ mục tiêu CĐS",
                DocumentType = "Thông tư",
                IssuingAgencyId = bkhcn?.Id,
                IssuingAgencyName = bkhcn?.Name ?? "Bộ Khoa học và Công nghệ",
                DraftingAgencyId = subUnit?.Id ?? bkhcn?.Id,
                DraftingAgencyName = subUnit?.Name ?? "Trung tâm CNTT & Dữ liệu số",
                SignerName = "Huỳnh Thành Đạt",
                SignerTitle = "Bộ trưởng",
                IssuedDate = new DateTime(2026, 5, 20),
                EffectiveDate = new DateTime(2026, 7, 1),
                EffectStatus = "Còn hiệu lực",
                Field = "Chính phủ số",
                Scope = "Bộ/Ngành",
                Notes = "Thông tư chuyên ngành áp dụng cho các đơn vị trực thuộc",
                AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                {
                    new LegalDocumentAttachmentDto { FileName = "Thongtu_08_2026_TT_BKHCN.pdf", CleanName = "Thông tư 08/2026/TT-BKHCN.pdf", FileUrl = "/uploads/TT_08_2026.pdf", FileSize = 1800500, FileType = "Văn bản chính" }
                })
            });

            if (subUnit != null)
            {
                AddIfMissing(new LegalDocument
                {
                    Code = "05/2026/QC-CĐSQG",
                    Title = "Quy chế Quản lý, Vận hành và An toàn Thông tin Hệ thống Dữ liệu Chuyển đổi số",
                    DocumentType = "Quy chế",
                    IssuingAgencyId = subUnit.Id,
                    IssuingAgencyName = subUnit.Name,
                    DraftingAgencyId = subUnit.Id,
                    DraftingAgencyName = subUnit.Name,
                    SignerName = "Cục trưởng",
                    SignerTitle = "Cục trưởng Cục CĐSQG",
                    IssuedDate = new DateTime(2026, 4, 12),
                    EffectiveDate = new DateTime(2026, 4, 15),
                    EffectStatus = "Còn hiệu lực",
                    Field = "An toàn thông tin",
                    Scope = "Bộ/Ngành",
                    Notes = "Quy chế nội bộ áp dụng cho đơn vị trực thuộc",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "Quyche_05_2026_CDSQG.pdf", CleanName = "Quy chế 05/2026/QC-CĐSQG.pdf", FileUrl = "/uploads/QC_05_2026.pdf", FileSize = 1450000, FileType = "Văn bản chính" }
                    })
                });
            }

            if (bca != null)
            {
                AddIfMissing(new LegalDocument
                {
                    Code = "42/2026/TT-BCA",
                    Title = "Thông tư quy định về bảo đảm an toàn thông tin và an ninh mạng trong vận hành hệ thống CĐS",
                    DocumentType = "Thông tư",
                    IssuingAgencyId = bca.Id,
                    IssuingAgencyName = bca.Name,
                    DraftingAgencyId = bca.Id,
                    DraftingAgencyName = bca.Name,
                    SignerName = "Lương Tam Quang",
                    SignerTitle = "Bộ trưởng",
                    IssuedDate = new DateTime(2026, 6, 10),
                    EffectiveDate = new DateTime(2026, 8, 1),
                    EffectStatus = "Còn hiệu lực",
                    Field = "Hạ tầng số",
                    Scope = "Toàn quốc",
                    Notes = "Quy định bảo mật hệ thống thông tin quốc gia",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "TT_42_2026_BCA.pdf", CleanName = "Thông tư 42/2026/TT-BCA.pdf", FileUrl = "/uploads/TT_42_BCA.pdf", FileSize = 2100000, FileType = "Văn bản chính" }
                    })
                });

                AddIfMissing(new LegalDocument
                {
                    Code = "03/2026/CT-BCA",
                    Title = "Chỉ thị đẩy mạnh triển khai Đề án 06 và dịch vụ công trực tuyến năm 2026",
                    DocumentType = "Chỉ thị",
                    IssuingAgencyId = bca.Id,
                    IssuingAgencyName = bca.Name,
                    DraftingAgencyId = bca.Id,
                    DraftingAgencyName = bca.Name,
                    SignerName = "Lương Tam Quang",
                    SignerTitle = "Bộ trưởng",
                    IssuedDate = new DateTime(2026, 2, 18),
                    EffectiveDate = new DateTime(2026, 2, 18),
                    EffectStatus = "Còn hiệu lực",
                    Field = "Dữ liệu số",
                    Scope = "Toàn quốc",
                    Notes = "Chỉ thị tăng cường bảo mật và kết nối dữ liệu dân cư",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "CT_03_2026_BCA.pdf", CleanName = "Chỉ thị 03/2026/CT-BCA.pdf", FileUrl = "/uploads/CT_03_BCA.pdf", FileSize = 1750000, FileType = "Văn bản chính" }
                    })
                });
            }

            if (tphcm != null)
            {
                AddIfMissing(new LegalDocument
                {
                    Code = "88/2026/QĐ-UBND",
                    Title = "Quyết định ban hành Kế hoạch Chuyển đổi số và Đô thị thông minh TP. Hồ Chí Minh năm 2026",
                    DocumentType = "Quyết định",
                    IssuingAgencyId = tphcm.Id,
                    IssuingAgencyName = tphcm.Name,
                    DraftingAgencyId = tphcm.Id,
                    DraftingAgencyName = tphcm.Name,
                    SignerName = "Phan Văn Mãi",
                    SignerTitle = "Chủ tịch UBND TP",
                    IssuedDate = new DateTime(2026, 4, 10),
                    EffectiveDate = new DateTime(2026, 4, 20),
                    EffectStatus = "Còn hiệu lực",
                    Field = "Chính phủ số",
                    Scope = "Địa phương",
                    Notes = "Văn bản chỉ đạo cấp địa phương",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "88_2026_QD_UBND.pdf", CleanName = "Quyết định 88/2026/QĐ-UBND.pdf", FileUrl = "/uploads/88_2026_UBND.pdf", FileSize = 1950000, FileType = "Văn bản chính" }
                    })
                });

                AddIfMissing(new LegalDocument
                {
                    Code = "18/2026/NQ-HĐND",
                    Title = "Nghị quyết thông qua Đề án Phát triển Hạ tầng số và Đô thị thông minh giai đoạn 2026-2030",
                    DocumentType = "Nghị quyết",
                    IssuingAgencyId = tphcm.Id,
                    IssuingAgencyName = tphcm.Name,
                    DraftingAgencyId = tphcm.Id,
                    DraftingAgencyName = tphcm.Name,
                    SignerName = "Chủ tịch HĐND",
                    SignerTitle = "Chủ tịch Hội đồng Nhân dân TP",
                    IssuedDate = new DateTime(2026, 7, 5),
                    EffectiveDate = new DateTime(2026, 7, 15),
                    EffectStatus = "Còn hiệu lực",
                    Field = "Hạ tầng số",
                    Scope = "Địa phương",
                    Notes = "Nghị quyết HĐND ban hành chính sách và ngân sách cho CĐS địa phương",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "NQ_18_2026_HDND.pdf", CleanName = "Nghị quyết 18/2026/NQ-HĐND.pdf", FileUrl = "/uploads/NQ_18_2026.pdf", FileSize = 2850000, FileType = "Văn bản chính" }
                    })
                });

                AddIfMissing(new LegalDocument
                {
                    Code = "12/2026/CT-UBND",
                    Title = "Chỉ thị về việc tăng cường bảo đảm an toàn thông tin và thúc đẩy thanh toán không dùng tiền mặt",
                    DocumentType = "Chỉ thị",
                    IssuingAgencyId = tphcm.Id,
                    IssuingAgencyName = tphcm.Name,
                    DraftingAgencyId = tphcm.Id,
                    DraftingAgencyName = tphcm.Name,
                    SignerName = "Phan Văn Mãi",
                    SignerTitle = "Chủ tịch UBND TP",
                    IssuedDate = new DateTime(2026, 6, 5),
                    EffectiveDate = new DateTime(2026, 6, 5),
                    EffectStatus = "Còn hiệu lực",
                    Field = "Kinh tế số",
                    Scope = "Địa phương",
                    Notes = "Chỉ thị chỉ đạo các sở ngành địa phương",
                    AttachmentsJson = JsonSerializer.Serialize(new List<LegalDocumentAttachmentDto>
                    {
                        new LegalDocumentAttachmentDto { FileName = "12_2026_CT_UBND.pdf", CleanName = "Chỉ thị 12/2026/CT-UBND.pdf", FileUrl = "/uploads/12_2026_CT.pdf", FileSize = 1420000, FileType = "Văn bản chính" }
                    })
                });
            }

            if (sampleDocs.Count > 0)
            {
                await _context.LegalDocuments.AddRangeAsync(sampleDocs);
                await _context.SaveChangesAsync();
            }
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

            // 1. DATA ACCESS PERMISSION (3-Tier Data Access Logic)
            bool isAdminUser = string.IsNullOrWhiteSpace(userRole) || userRole.ToLower() == "admin" || userRole == "1";

            if (!isAdminUser && userAgencyId.HasValue)
            {
                var userAgId = userAgencyId.Value;
                var userAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == userAgId);

                if (userAgency != null)
                {
                    bool isLevel2 = userAgency.ParentId == null;

                    if (isLevel2)
                    {
                        // Level 2 (Bộ/Ngành/Địa phương): Sees documents created by, issued by, or drafted by current agency OR any of its subordinate child agencies
                        var childAgencyIds = await _context.Agencies
                            .Where(a => a.ParentId == userAgId)
                            .Select(a => a.Id)
                            .ToListAsync();
                        childAgencyIds.Add(userAgId);

                        query = query.Where(d =>
                            (d.IssuingAgencyId.HasValue && childAgencyIds.Contains(d.IssuingAgencyId.Value)) ||
                            (d.DraftingAgencyId.HasValue && childAgencyIds.Contains(d.DraftingAgencyId.Value)) ||
                            (d.CreatedByAgencyId.HasValue && childAgencyIds.Contains(d.CreatedByAgencyId.Value)));
                    }
                    else
                    {
                        // Level 3 (Đơn vị trực thuộc): Sees documents created by, issued by, or drafted by current agency
                        query = query.Where(d =>
                            d.IssuingAgencyId == userAgId ||
                            d.DraftingAgencyId == userAgId ||
                            d.CreatedByAgencyId == userAgId);
                    }
                }
            }

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

            // 1. DATA ACCESS PERMISSION FOR DASHBOARD (Apply 3-Tier Data Scoping to baseQuery)
            if (!isAdmin && currAgId.HasValue && currAgency != null)
            {
                if (isLevel2)
                {
                    // Level 2 (Bộ/Ngành/Địa phương): Only include documents created by, issued by, or drafted by current agency OR any of its subordinate child agencies
                    var childAgencyIds = allAgencies
                        .Where(a => a.ParentId == currAgId.Value)
                        .Select(a => a.Id)
                        .ToList();
                    childAgencyIds.Add(currAgId.Value);

                    baseQuery = baseQuery.Where(d =>
                        (d.IssuingAgencyId.HasValue && childAgencyIds.Contains(d.IssuingAgencyId.Value)) ||
                        (d.DraftingAgencyId.HasValue && childAgencyIds.Contains(d.DraftingAgencyId.Value)) ||
                        (d.CreatedByAgencyId.HasValue && childAgencyIds.Contains(d.CreatedByAgencyId.Value)));
                }
                else if (isLevel3)
                {
                    // Level 3 (Đơn vị trực thuộc): Only include documents created by, issued by, or drafted by current agency
                    baseQuery = baseQuery.Where(d =>
                        d.IssuingAgencyId == currAgId.Value ||
                        d.DraftingAgencyId == currAgId.Value ||
                        d.CreatedByAgencyId == currAgId.Value);
                }
            }

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
        /// Tạo mới văn bản QPPL với phân quyền khóa/giới hạn Cơ quan ban hành theo Cấp tài khoản.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLegalDocument([FromBody] CreateLegalDocumentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Số ký hiệu và Tên văn bản." });
            }

            var entity = new LegalDocument
            {
                Code = dto.Code.Trim(),
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
        public async Task<IActionResult> UpdateLegalDocument(Guid id, [FromBody] UpdateLegalDocumentDto dto)
        {
            var entity = await _context.LegalDocuments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Không tìm thấy văn bản QPPL." });

            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Số ký hiệu và Tên văn bản." });
            }

            entity.Code = dto.Code.Trim();
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

        /// <summary>
        /// DELETE /api/legaldocuments/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLegalDocument(Guid id)
        {
            var entity = await _context.LegalDocuments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Không tìm thấy văn bản QPPL." });

            _context.LegalDocuments.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa văn bản QPPL thành công." });
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
