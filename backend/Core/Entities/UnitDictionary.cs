using System;
using Cdsqg.Core.Enums;

namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Module 1: Unit Dictionary (Từ điển đơn vị tính động: %, Lượt, Văn bản, Hệ thống...)
    /// </summary>
    public class UnitDictionary
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Mã đơn vị tính (e.g. "PERCENT", "LUOT", "DOC", "SYSTEM")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên đơn vị tính hiển thị (e.g. "%", "Lượt", "Văn bản", "Hệ thống")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kiểu dữ liệu hỗ trợ: Integer, Decimal, Boolean, Text_Status
        /// </summary>
        public UnitDataTypeEnum DataType { get; set; } = UnitDataTypeEnum.Decimal;

        public bool IsActive { get; set; } = true;
    }
}
