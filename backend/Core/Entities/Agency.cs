using System;
using System.Collections.Generic;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 1: Agency Catalog (Bộ, Ngành, Địa phương, Đơn vị nội bộ)
    /// Hỗ trợ cơ cấu phân cấp Cơ quan trực thuộc (Parent - Child Agencies)
    /// </summary>
    public class Agency
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Mã định danh cơ quan (e.g. "BTTTT", "BCA", "BKHĐT", "HANOI", "TPHCM")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ cơ quan
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Phân loại cơ quan: Ministry, Province, Internal, Other
        /// </summary>
        public AgencyTypeEnum Type { get; set; } = AgencyTypeEnum.Ministry;

        /// <summary>
        /// ID Cơ quan cấp trên (nếu là cơ quan trực thuộc)
        /// </summary>
        public Guid? ParentId { get; set; }
        public Agency? ParentAgency { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Danh sách cán bộ đầu mối của cơ quan
        /// </summary>
        public List<AgencyContactPerson> ContactPersons { get; set; } = new List<AgencyContactPerson>();

        /// <summary>
        /// Danh sách tệp đính kèm Kế hoạch
        /// </summary>
        public List<AgencyPlanFile> PlanFiles { get; set; } = new List<AgencyPlanFile>();

        // Tập hợp các Cơ quan trực thuộc
        public ICollection<Agency> ChildAgencies { get; set; } = new List<Agency>();
    }
}
