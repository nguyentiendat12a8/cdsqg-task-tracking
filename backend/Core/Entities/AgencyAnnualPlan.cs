using System;
using System.Collections.Generic;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Kế hoạch chuyển đổi số hàng năm của Cơ quan / Đơn vị
    /// </summary>
    public class AgencyAnnualPlan
    {
        public int Year { get; set; }
        public bool IsIssued { get; set; } = false;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Notes { get; set; }
        public List<AgencyPlanFile> Files { get; set; } = new List<AgencyPlanFile>();
    }
}
