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
        /// Viết tắt dùng trong mã mục tiêu/nhiệm vụ (ví dụ BCA → NV-BCA-01). Trống: mục do cơ quan này chủ trì dùng mã chung (NV-C-01).
        /// </summary>
        public string? ShortCode { get; set; }

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

        /// <summary>
        /// Kế hoạch CĐS 5 năm (2026-2030)
        /// </summary>
        public AgencyFiveYearPlan? FiveYearPlan { get; set; }

        /// <summary>
        /// Kế hoạch CĐS hàng năm
        /// </summary>
        public List<AgencyAnnualPlan> AnnualPlans { get; set; } = new List<AgencyAnnualPlan>();

        /// <summary>
        /// Khung kiến trúc số
        /// </summary>
        public AgencyDigitalArchitecture? DigitalArchitecture { get; set; }

        // Tập hợp các Cơ quan trực thuộc
        public ICollection<Agency> ChildAgencies { get; set; } = new List<Agency>();
    }
}
