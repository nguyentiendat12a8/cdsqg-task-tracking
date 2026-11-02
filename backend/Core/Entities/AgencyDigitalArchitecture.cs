using System;
using System.Collections.Generic;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Khung kiến trúc số / Chính phủ điện tử / Chính quyền số của Cơ quan / Đơn vị
    /// </summary>
    public class AgencyDigitalArchitecture
    {
        public bool IsIssued { get; set; } = false;
        public string? DocumentNumber { get; set; }
        public string? Version { get; set; }
        public DateTime? IssueDate { get; set; }
        public string? Notes { get; set; }
        public List<AgencyPlanFile> Files { get; set; } = new List<AgencyPlanFile>();
    }
}
