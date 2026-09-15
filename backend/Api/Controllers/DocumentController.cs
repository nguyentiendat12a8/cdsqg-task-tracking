using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;

namespace Cdsqg.Api.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public DocumentController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// GET /api/documents
        /// Trả về danh sách tất cả các Văn bản / Quyết định Level 0 kèm thống kê tổng số Goals, Tasks và Tỉ lệ hoàn thành.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments(
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Documents
                .Include(d => d.Items)
                    .ThenInclude(i => i.ProgressLogs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(d => 
                    d.DocumentNumber.ToLower().Contains(s) ||
                    d.Name.ToLower().Contains(s) ||
                    (d.Summary != null && d.Summary.ToLower().Contains(s)) ||
                    (d.Signer != null && d.Signer.ToLower().Contains(s)));
            }

            int totalCount = await query.CountAsync();

            var docs = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = docs.Select(d =>
            {
                int totalGoals = d.Items.Count(i => i.ItemType == ItemTypeEnum.Goal);
                int totalTasks = d.Items.Count(i => i.ItemType == ItemTypeEnum.Task);

                var tasks = d.Items.Where(i => i.ItemType == ItemTypeEnum.Task).ToList();
                decimal completionRate = 0;
                if (tasks.Count > 0)
                {
                    decimal totalPct = 0;
                    foreach (var task in tasks)
                    {
                        var latestLog = task.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                        if (latestLog != null)
                        {
                            if (task.EvaluationType == EvaluationTypeEnum.Qualitative)
                            {
                                totalPct += latestLog.QualitativeStatus == TextStatusEnum.Completed ? 100 :
                                             latestLog.QualitativeStatus == TextStatusEnum.Reviewing ? 75 :
                                             latestLog.QualitativeStatus == TextStatusEnum.Drafting ? 40 : 0;
                            }
                                decimal target = 100m;
                                var lastStr = task.CustomBaseline?.Values.LastOrDefault();
                                if (!string.IsNullOrEmpty(lastStr) && decimal.TryParse(lastStr, out decimal parsedTarget))
                                {
                                    target = parsedTarget;
                                }
                                if (target <= 0) target = 100m;
                                decimal pct = Math.Min(100, Math.Max(0, (latestLog.QuantitativeValue ?? 0) / target * 100));
                                totalPct += pct;
                        }
                    }
                    completionRate = Math.Round(totalPct / tasks.Count, 1);
                }

                return new DocumentSummaryDto
                {
                    Id = d.Id,
                    DocumentNumber = d.DocumentNumber,
                    Name = d.Name,
                    Summary = d.Summary,
                    Signer = d.Signer,
                    IssueDate = d.IssueDate,
                    TimeResolution = d.TimeResolution.ToString(),
                    StartYear = d.StartYear,
                    EndYear = d.EndYear,
                    AttachmentPath = d.AttachmentPath,
                    TotalGoals = totalGoals,
                    TotalTasks = totalTasks,
                    OverallCompletionRate = completionRate,
                    CreatedAt = d.CreatedAt
                };
            }).ToList();

            return Ok(new PagedResultDto<DocumentSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// GET /api/documents/{id}
        /// Chi tiết văn bản và danh sách các Mục tiêu (1A) & Nhiệm vụ (1B) thuộc văn bản đó.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            var doc = await _context.Documents
                .Include(d => d.Items)
                    .ThenInclude(i => i.LeadAgency)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Unit)
                .Include(d => d.Items)
                    .ThenInclude(i => i.ProgressLogs)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Baselines)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
            {
                var taskItem = await _context.GoalTaskItems.FirstOrDefaultAsync(i => i.Id == id);
                if (taskItem != null)
                {
                    doc = await _context.Documents
                        .Include(d => d.Items).ThenInclude(i => i.LeadAgency)
                        .Include(d => d.Items).ThenInclude(i => i.Unit)
                        .Include(d => d.Items).ThenInclude(i => i.ProgressLogs)
                        .Include(d => d.Items).ThenInclude(i => i.Baselines)
                        .FirstOrDefaultAsync(d => d.Id == taskItem.DocumentId);
                }
            }

            if (doc == null)
            {
                return NotFound(new { error = $"Không tìm thấy Văn bản / Quyết định với ID: {id}" });
            }

            int goalCounter = 1;
            int taskCounter = 1;
            bool dbChanged = false;

            foreach (var item in doc.Items.OrderBy(i => i.CreatedAt))
            {
                if (string.IsNullOrWhiteSpace(item.Code) || !item.Code.Any(char.IsDigit))
                {
                    if (item.ItemType == ItemTypeEnum.Goal)
                    {
                        item.Code = $"MT-{goalCounter:D2}";
                    }
                    else
                    {
                        item.Code = $"NV-{taskCounter:D2}";
                    }
                    _context.Entry(item).State = EntityState.Modified;
                    dbChanged = true;
                }

                if (item.ItemType == ItemTypeEnum.Goal) goalCounter++;
                else taskCounter++;

                var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                if (latestLog != null)
                {
                    item.LatestProgressValue = latestLog.QuantitativeValue;
                    item.LatestProgressStatus = latestLog.QualitativeStatus?.ToString();
                    item.LastUpdated = latestLog.LogDate;
                }
            }

            if (dbChanged)
            {
                await _context.SaveChangesAsync();
            }

            var mappedItems = doc.Items.OrderBy(i => i.CreatedAt).Select(item =>
            {
                var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                var baselinesList = item.Baselines.Select(b => new
                {
                    year = b.Year,
                    quarter = b.Quarter,
                    targetQuantity = b.TargetQuantity,
                    targetQualitativeStatus = b.TargetQualitativeStatus?.ToString()
                }).ToList();

                return new
                {
                    id = item.Id,
                    documentId = item.DocumentId,
                    itemType = item.ItemType == ItemTypeEnum.Goal ? "Goal" : "Task",
                    itemTypeEnum = (int)item.ItemType,
                    code = item.Code,
                    title = item.Title,
                    category = item.Category,
                    leadAgencyId = item.LeadAgencyId,
                    leadAgency = item.LeadAgency != null ? new
                    {
                        id = item.LeadAgency.Id,
                        code = item.LeadAgency.Code,
                        name = item.LeadAgency.Name,
                        type = item.LeadAgency.Type
                    } : null,
                    coordinatingAgencyIds = item.CoordinatingAgencyIds,
                    unitId = item.UnitId,
                    unit = item.Unit != null ? new
                    {
                        id = item.Unit.Id,
                        code = item.Unit.Code,
                        name = item.Unit.Name,
                        dataType = item.Unit.DataType
                    } : null,
                    evaluationType = item.EvaluationType == EvaluationTypeEnum.Quantitative ? "Quantitative" : "Qualitative",
                    calculationMethod = item.CalculationMethod.ToString(),
                    customBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    baselines = baselinesList,
                    createdAt = item.CreatedAt,
                    latestProgressValue = latestLog?.QuantitativeValue,
                    latestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    lastUpdated = latestLog?.LogDate
                };
            }).ToList();

            var resultPayload = new
            {
                id = doc.Id,
                documentNumber = doc.DocumentNumber,
                name = doc.Name,
                summary = doc.Summary,
                signer = doc.Signer,
                issueDate = doc.IssueDate,
                timeResolution = doc.TimeResolution.ToString(),
                startYear = doc.StartYear,
                endYear = doc.EndYear,
                attachmentPath = doc.AttachmentPath,
                attachmentFilePaths = doc.AttachmentFilePaths,
                createdAt = doc.CreatedAt,
                items = mappedItems
            };

            return Ok(resultPayload);
        }

        /// <summary>
        /// GET /api/documents/{id}/items
        /// Lấy danh sách Mục tiêu / Nhiệm vụ thuộc Văn bản có phân trang Server (Server-side Pagination)
        /// </summary>
        [HttpGet("{id:guid}/items")]
        public async Task<IActionResult> GetDocumentItems(
            Guid id,
            [FromQuery] string? itemType = null,
            [FromQuery] string? search = null,
            [FromQuery] Guid? agencyId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.GoalTaskItems
                .Include(i => i.LeadAgency)
                .Include(i => i.Unit)
                .Include(i => i.ProgressLogs)
                .Include(i => i.Baselines)
                .Where(i => i.DocumentId == id);

            if (!string.IsNullOrWhiteSpace(itemType))
            {
                if (itemType.Equals("Goal", StringComparison.OrdinalIgnoreCase) || itemType == "1")
                    query = query.Where(i => i.ItemType == ItemTypeEnum.Goal);
                else if (itemType.Equals("Task", StringComparison.OrdinalIgnoreCase) || itemType == "2")
                    query = query.Where(i => i.ItemType == ItemTypeEnum.Task);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(i =>
                    i.Code.ToLower().Contains(s) ||
                    i.Title.ToLower().Contains(s) ||
                    (i.Category != null && i.Category.ToLower().Contains(s)));
            }

            if (agencyId.HasValue && agencyId.Value != Guid.Empty)
            {
                query = query.Where(i => i.LeadAgencyId == agencyId.Value);
            }

            int totalCount = await query.CountAsync();
            int pNum = pageNumber > 0 ? pageNumber : 1;
            int pSize = pageSize > 0 ? pageSize : 10;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);

            var items = await query
                .OrderBy(i => i.CreatedAt)
                .Skip((pNum - 1) * pSize)
                .Take(pSize)
                .ToListAsync();

            var mappedItems = items.Select(item =>
            {
                var latestLog = item.ProgressLogs.OrderByDescending(l => l.LogDate).FirstOrDefault();
                var baselinesList = item.Baselines.Select(b => new
                {
                    year = b.Year,
                    quarter = b.Quarter,
                    targetQuantity = b.TargetQuantity,
                    targetQualitativeStatus = b.TargetQualitativeStatus?.ToString()
                }).ToList();

                return new
                {
                    id = item.Id,
                    documentId = item.DocumentId,
                    itemType = item.ItemType == ItemTypeEnum.Goal ? "Goal" : "Task",
                    itemTypeEnum = (int)item.ItemType,
                    code = item.Code,
                    title = item.Title,
                    category = item.Category,
                    leadAgencyId = item.LeadAgencyId,
                    leadAgency = item.LeadAgency != null ? new
                    {
                        id = item.LeadAgency.Id,
                        code = item.LeadAgency.Code,
                        name = item.LeadAgency.Name,
                        type = (int)item.LeadAgency.Type
                    } : null,
                    coordinatingAgencyIds = item.CoordinatingAgencyIds,
                    unitId = item.UnitId,
                    unit = item.Unit != null ? new
                    {
                        id = item.Unit.Id,
                        code = item.Unit.Code,
                        name = item.Unit.Name,
                        dataType = (int)item.Unit.DataType
                    } : null,
                    evaluationType = item.EvaluationType == EvaluationTypeEnum.Quantitative ? "Quantitative" : "Qualitative",
                    calculationMethod = item.CalculationMethod.ToString(),
                    customBaseline = item.CustomBaseline ?? new Dictionary<string, string>(),
                    baselines = baselinesList,
                    createdAt = item.CreatedAt,
                    latestProgressValue = latestLog?.QuantitativeValue,
                    latestProgressStatus = latestLog?.QualitativeStatus?.ToString(),
                    lastUpdated = latestLog?.LogDate
                };
            }).ToList();

            return Ok(new
            {
                items = mappedItems,
                totalCount,
                pageNumber = pNum,
                pageSize = pSize,
                totalPages = Math.Max(1, totalPages)
            });
        }

        /// <summary>
        /// POST /api/documents (multipart/form-data via [FromForm])
        /// Tạo thủ công Văn bản / Quyết định Level 0 kèm file đính kèm PDF/Doc minh chứng.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (string.IsNullOrWhiteSpace(dto.DocumentNumber))
                {
                    return BadRequest(new { error = "Số hiệu văn bản không được để trống." });
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BadRequest(new { error = "Trích yếu / Tên quyết định không được để trống." });
                }

                if (await _context.Documents.AnyAsync(d => d.DocumentNumber == dto.DocumentNumber))
                {
                    return BadRequest(new { error = $"Số hiệu văn bản '{dto.DocumentNumber}' đã tồn tại trong hệ thống." });
                }

                List<string> uploadedPaths = new List<string>();
                if (dto.AttachmentFiles != null && dto.AttachmentFiles.Count > 0)
                {
                    foreach (var file in dto.AttachmentFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var path = await _fileStorageService.SaveDocumentFileAsync(file);
                            if (!string.IsNullOrEmpty(path)) uploadedPaths.Add(path);
                        }
                    }
                }

                if (dto.AttachmentFile != null && dto.AttachmentFile.Length > 0)
                {
                    var path = await _fileStorageService.SaveDocumentFileAsync(dto.AttachmentFile);
                    if (!string.IsNullOrEmpty(path) && !uploadedPaths.Contains(path))
                    {
                        uploadedPaths.Add(path);
                    }
                }

                var doc = new Document
                {
                    Id = Guid.NewGuid(),
                    DocumentNumber = dto.DocumentNumber.Trim(),
                    Name = dto.Name.Trim(),
                    Summary = dto.Summary?.Trim(),
                    Signer = dto.Signer?.Trim(),
                    IssueDate = DateTime.SpecifyKind(dto.IssueDate, DateTimeKind.Utc),
                    TimeResolution = dto.TimeResolution,
                    StartYear = dto.StartYear ?? 2026,
                    EndYear = dto.EndYear ?? 2030,
                    AttachmentPath = uploadedPaths.FirstOrDefault(),
                    AttachmentFilePaths = uploadedPaths,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Documents.Add(doc);

                // Audit log creation
                var importLog = new DataImportLog
                {
                    FileName = uploadedPaths.Count > 0 ? string.Join(", ", uploadedPaths.Select(p => System.IO.Path.GetFileName(p))) : $"Khởi tạo thủ công ({doc.DocumentNumber})",
                    FileType = "Văn bản Quyết định",
                    Category = "Minh chứng quyết định",
                    ImportedBy = dto.Signer ?? "Chuyên viên theo dõi",
                    ImportedAt = DateTime.UtcNow,
                    TotalGoalsCreated = 0,
                    TotalTasksCreated = 0,
                    Status = "Thành công",
                    SummaryNotes = $"Tạo mới văn bản {doc.DocumentNumber}: {doc.Name}"
                };
                _context.DataImportLogs.Add(importLog);

                await _context.SaveChangesAsync();

                return Ok(doc);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống khi tạo văn bản: " + ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/documents/{id} (multipart/form-data via [FromForm])
        /// Chỉnh sửa thông tin chi tiết Văn bản / Quyết định Level 0.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromForm] UpdateDocumentRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
                if (doc == null)
                {
                    return NotFound(new { error = $"Không tìm thấy Văn bản / Quyết định với ID: {id}" });
                }

                if (!string.IsNullOrWhiteSpace(dto.DocumentNumber) && dto.DocumentNumber != doc.DocumentNumber)
                {
                    if (await _context.Documents.AnyAsync(d => d.DocumentNumber == dto.DocumentNumber && d.Id != id))
                    {
                        return BadRequest(new { error = $"Số hiệu văn bản '{dto.DocumentNumber}' đã trùng với văn bản khác." });
                    }
                    doc.DocumentNumber = dto.DocumentNumber.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    doc.Name = dto.Name.Trim();
                }

                doc.Summary = dto.Summary?.Trim();
                doc.Signer = dto.Signer?.Trim();
                doc.IssueDate = DateTime.SpecifyKind(dto.IssueDate, DateTimeKind.Utc);
                doc.TimeResolution = dto.TimeResolution;
                doc.StartYear = dto.StartYear ?? 2026;
                doc.EndYear = dto.EndYear ?? 2030;

                List<string> uploadedPaths = doc.AttachmentFilePaths ?? new List<string>();
                if (dto.AttachmentFiles != null && dto.AttachmentFiles.Count > 0)
                {
                    foreach (var file in dto.AttachmentFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var path = await _fileStorageService.SaveDocumentFileAsync(file);
                            if (!string.IsNullOrEmpty(path) && !uploadedPaths.Contains(path))
                            {
                                uploadedPaths.Add(path);
                            }
                        }
                    }
                }

                if (dto.AttachmentFile != null && dto.AttachmentFile.Length > 0)
                {
                    var path = await _fileStorageService.SaveDocumentFileAsync(dto.AttachmentFile);
                    if (!string.IsNullOrEmpty(path) && !uploadedPaths.Contains(path))
                    {
                        uploadedPaths.Add(path);
                    }
                }

                doc.AttachmentFilePaths = uploadedPaths;
                if (uploadedPaths.Count > 0)
                {
                    doc.AttachmentPath = uploadedPaths.First();
                }

                await _context.SaveChangesAsync();
                return Ok(doc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống khi cập nhật văn bản: " + ex.Message });
            }
        }

        /// <summary>
        /// GET /api/documents/import-history
        /// Trả về danh sách lịch sử tất cả các file dữ liệu nạp vào hệ thống (Hỗ trợ Phân trang Server & Tìm kiếm & Phân loại)
        /// </summary>
        [HttpGet("import-history")]
        public async Task<IActionResult> GetImportHistory(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                var logs = await _context.DataImportLogs
                    .OrderByDescending(l => l.ImportedAt)
                    .ToListAsync();

                IEnumerable<DataImportLog> filtered = logs;

                if (!string.IsNullOrWhiteSpace(category))
                {
                    var cat = category.Trim();
                    filtered = filtered.Where(l => string.Equals(l.Category, cat, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    filtered = filtered.Where(l =>
                        (l.FileName != null && l.FileName.ToLower().Contains(s)) ||
                        (l.FileType != null && l.FileType.ToLower().Contains(s)) ||
                        (l.Category != null && l.Category.ToLower().Contains(s)) ||
                        (l.ImportedBy != null && l.ImportedBy.ToLower().Contains(s)) ||
                        (l.SummaryNotes != null && l.SummaryNotes.ToLower().Contains(s)));
                }

                var filteredList = filtered.ToList();
                int totalCount = filteredList.Count;

                if (pageNumber.HasValue && pageSize.HasValue && pageSize.Value > 0)
                {
                    int pNum = pageNumber.Value > 0 ? pageNumber.Value : 1;
                    int pSize = pageSize.Value;
                    int totalPages = (int)Math.Ceiling(totalCount / (double)pSize);

                    var pagedItems = filteredList
                        .Skip((pNum - 1) * pSize)
                        .Take(pSize)
                        .ToList();

                    return Ok(new
                    {
                        items = pagedItems,
                        totalCount,
                        pageNumber = pNum,
                        pageSize = pSize,
                        totalPages
                    });
                }

                return Ok(filteredList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi tải lịch sử nạp dữ liệu: " + ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/documents/{id}
        /// Xóa văn bản và toàn bộ các mục tiêu/nhiệm vụ trực thuộc.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var doc = await _context.Documents
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
            {
                return NotFound(new { error = "Không tìm thấy văn bản để xóa." });
            }

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã xóa thành công văn bản {doc.DocumentNumber}" });
        }
    }
}
