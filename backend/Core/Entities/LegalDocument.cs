using System;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Phân hệ Văn bản Quy phạm Pháp luật (VB QPPL)
    /// </summary>
    public class LegalDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Mã / Số ký hiệu văn bản (e.g. "1266/QĐ-TTg", "15/2020/NĐ-CP")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên / Trích yếu nội dung văn bản
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại văn bản: Luật, Nghị định, Thông tư, Quyết định, Nghị quyết, Chỉ thị, Khác
        /// </summary>
        public string DocumentType { get; set; } = "Quyết định";

        /// <summary>
        /// Cơ quan ban hành (ID & Tên)
        /// </summary>
        public Guid? IssuingAgencyId { get; set; }
        public string? IssuingAgencyName { get; set; }

        /// <summary>
        /// Cơ quan dự thảo (ID & Tên)
        /// </summary>
        public Guid? DraftingAgencyId { get; set; }
        public string? DraftingAgencyName { get; set; }

        /// <summary>
        /// Người ký & Chức danh (e.g. "Trần Lưu Quang", "Phó Thủ tướng")
        /// </summary>
        public string? SignerName { get; set; }
        public string? SignerTitle { get; set; }

        /// <summary>
        /// Ngày ban hành & Ngày có hiệu lực
        /// </summary>
        public DateTime? IssuedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Trạng thái hiệu lực: Còn hiệu lực, Hết hiệu lực toàn bộ, Hết hiệu lực một phần, Chưa có hiệu lực
        /// </summary>
        public string EffectStatus { get; set; } = "Còn hiệu lực";

        /// <summary>
        /// Lĩnh vực / Nhóm CĐS: Thể chế số, Chính phủ số, Kinh tế số, Xã hội số, Hạ tầng số, Dữ liệu số, Khác
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Phạm vi áp dụng: Toàn quốc, Bộ/Ngành, Địa phương
        /// </summary>
        public string? Scope { get; set; } = "Toàn quốc";

        /// <summary>
        /// JSON lưu trữ danh sách tệp đính kèm
        /// [{ fileName, cleanName, fileUrl, fileSize, fileType }]
        /// </summary>
        public string? AttachmentsJson { get; set; }

        /// <summary>
        /// Ghi chú chi tiết
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID Đơn vị / Cơ quan và Người khởi tạo văn bản
        /// </summary>
        public Guid? CreatedByAgencyId { get; set; }
        public Guid? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
