using System;
using System.Collections.Generic;

namespace Cdsqg.Application.DTOs
{
    public class LegalDocumentAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string CleanName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = "Văn bản chính";
    }

    public class LegalDocumentDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "Quyết định";
        public Guid? IssuingAgencyId { get; set; }
        public string? IssuingAgencyName { get; set; }
        public Guid? DraftingAgencyId { get; set; }
        public string? DraftingAgencyName { get; set; }
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string EffectStatus { get; set; } = "Còn hiệu lực";
        public string? Field { get; set; }
        public string? Scope { get; set; } = "Toàn quốc";
        public List<LegalDocumentAttachmentDto> Attachments { get; set; } = new();
        public string? Notes { get; set; }
        public Guid? CreatedByAgencyId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateLegalDocumentDto
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "Quyết định";
        public Guid? IssuingAgencyId { get; set; }
        public string? IssuingAgencyName { get; set; }
        public Guid? DraftingAgencyId { get; set; }
        public string? DraftingAgencyName { get; set; }
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string EffectStatus { get; set; } = "Còn hiệu lực";
        public string? Field { get; set; }
        public string? Scope { get; set; } = "Toàn quốc";
        public List<LegalDocumentAttachmentDto> Attachments { get; set; } = new();
        public string? Notes { get; set; }
        public Guid? CreatedByAgencyId { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }

    public class UpdateLegalDocumentDto : CreateLegalDocumentDto
    {
    }

    public class LegalDocumentFilterDto
    {
        public string? SearchQuery { get; set; }
        public string? DocumentType { get; set; }
        public string? EffectStatus { get; set; }
        public string? Field { get; set; }
        public Guid? IssuingAgencyId { get; set; }
        public Guid? DraftingAgencyId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AgencyLegalDocStatDto
    {
        public Guid? AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public string AgencyType { get; set; } = "Ministry";
        public int TotalCount { get; set; }
        public Dictionary<string, int> TypeCounts { get; set; } = new();
        public Dictionary<string, int> ByFieldCounts { get; set; } = new();
    }

    public class LegalDocumentChartStatsDto
    {
        public int RoleLevel { get; set; } = 1;
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public List<AgencyLegalDocStatDto> MinistriesStats { get; set; } = new();
        public List<AgencyLegalDocStatDto> ProvincesStats { get; set; } = new();
        public List<AgencyLegalDocStatDto> AgencyStats { get; set; } = new();
        public List<LegalDocumentDto> RecentDocuments { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class BulkImportLegalDocumentItemDto
    {
        public int RowIndex { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = "Quyết định";
        public string? IssuingAgencyName { get; set; }
        public string? DraftingAgencyName { get; set; }
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string EffectStatus { get; set; } = "Còn hiệu lực";
        public string? Field { get; set; }
        public string? Scope { get; set; } = "Toàn quốc";
        public string? Notes { get; set; }
    }

    public class LegalDocumentImportErrorDto
    {
        public int RowIndex { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ErrorDetail { get; set; } = string.Empty;
    }

    public class BulkImportLegalDocumentRequestDto
    {
        public List<BulkImportLegalDocumentItemDto> Items { get; set; } = new();
    }
}
