using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Stores data import audit history for LLM JSON imports, Excel/Word uploads, and document creation.
    /// </summary>
    public class DataImportLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = "LLM_JSON"; // LLM_JSON, Document_PDF, Excel
        
        private string? _category;

        [NotMapped]
        public string Category
        {
            get
            {
                if (!string.IsNullOrEmpty(_category)) return _category;
                if (!string.IsNullOrEmpty(FileType))
                {
                    if (FileType.Contains("Quyết định") || FileType.Contains("Document_PDF") || FileType.Contains("Văn bản"))
                        return "Minh chứng quyết định";
                    if (FileType.Contains("tiến độ") || FileType.Contains("Progress") || FileType.Contains("Minh chứng"))
                        return "Minh chứng báo cáo tiến độ";
                    if (FileType.Contains("LLM") || FileType.Contains("JSON"))
                        return "Nhập liệu AI LLM JSON";
                }
                return "Minh chứng quyết định";
            }
            set => _category = value;
        }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public string ImportedBy { get; set; } = "Chuyên viên nạp dữ liệu";

        public int TotalGoalsCreated { get; set; } = 0;
        public int TotalTasksCreated { get; set; } = 0;

        public string Status { get; set; } = "Thành công"; // Thành công, Thất bại, Cảnh báo
        public string SummaryNotes { get; set; } = string.Empty;
        public Guid? AgencyId { get; set; }
    }
}
