using System;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Thông tin tệp đính kèm Kế hoạch giai đoạn 2026-2030 của Cơ quan / Đơn vị
    /// </summary>
    public class AgencyPlanFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? UploadedBy { get; set; }
    }
}
