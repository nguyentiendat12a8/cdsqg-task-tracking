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
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Period { get; set; } = "2026-2030";
        public string? Notes { get; set; }
        public List<AgencyPlanFile> Files { get; set; } = new List<AgencyPlanFile>();
    }
}
