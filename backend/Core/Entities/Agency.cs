using System;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 1: Agency Catalog (Bộ, Ngành, Địa phương, Đơn vị nội bộ)
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

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
