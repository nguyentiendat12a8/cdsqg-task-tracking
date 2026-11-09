using System;
using System.Collections.Generic;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Kế hoạch chuyển đổi số giai đoạn 5 năm (2026-2030) của Cơ quan / Đơn vị
    /// </summary>
    public class AgencyFiveYearPlan
    {
        public bool IsIssued { get; set; } = false;

        /// <summary>
        /// Đã rà soát, có Kế hoạch 05 năm (đánh dấu x)
        /// </summary>
        public bool HasReviewedPlan { get; set; } = false;

        /// <summary>
        /// Văn bản phúc đáp VB 5799/BKHCN-CĐSQG (và VB 8841/BKHCN-CĐSQG (lần 2))
        /// </summary>
        public string? ResponseDocument { get; set; }

        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Đã cập nhật theo Chiến lược quốc gia QĐ 1266/QĐ-TTg (đánh dấu x)
        /// </summary>
        public bool IsUpdated1266 { get; set; } = false;

        public DateTime? IssueDate { get; set; }
        public string? Period { get; set; } = "2026-2030";
        public string? Notes { get; set; }
        public List<AgencyPlanFile> Files { get; set; } = new List<AgencyPlanFile>();
    }
}
