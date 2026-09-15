using System;
using System.Collections.Generic;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 2: Level 0 Document Container (Quyết định / Văn bản chỉ đạo)
    /// </summary>
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Số hiệu văn bản / Quyết định (e.g. "749/QĐ-TTg")
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Trích yếu / Tên Quyết định
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tóm tắt nội dung / Trích yếu chi tiết văn bản
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Người ký văn bản / Quyết định (e.g. "Thủ tướng Phạm Minh Chính")
        /// </summary>
        public string? Signer { get; set; }

        /// <summary>
        /// Ngày ban hành
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Thời hạn thực hiện: SpecificYear, Range (e.g. 2026-2030), Continuous
        /// </summary>
        public TimeResolutionEnum TimeResolution { get; set; } = TimeResolutionEnum.Range;

        public int? StartYear { get; set; }
        public int? EndYear { get; set; }

        /// <summary>
        /// Đường dẫn lưu file PDF/Doc minh chứng Quyết định đính kèm
        /// </summary>
        public string? AttachmentPath { get; set; }

        /// <summary>
        /// Mảng đường dẫn nhiều file PDF/Word đính kèm (Mapped to PostgreSQL JSONB)
        /// </summary>
        public List<string> AttachmentFilePaths { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation collection
        public ICollection<GoalTaskItem> Items { get; set; } = new List<GoalTaskItem>();
    }
}
