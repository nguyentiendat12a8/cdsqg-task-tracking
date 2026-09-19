namespace Cdsqg.Core.Entities
{
    /// <summary>
    /// Thông tin cán bộ đầu mối liên hệ của Cơ quan / Đơn vị
    /// </summary>
    public class AgencyContactPerson
    {
        public string Name { get; set; } = string.Empty;       // Họ và tên (e.g. "Phạm Quang Cường")
        public string Position { get; set; } = string.Empty;   // Chức vụ (e.g. "Phó giám đốc")
        public string Department { get; set; } = string.Empty; // Phòng ban (e.g. "Sở KHCN")
        public string Phone { get; set; } = string.Empty;      // Số điện thoại (e.g. "0976 819 323")
        public string Email { get; set; } = string.Empty;      // Email (e.g. "cuongpq.sokhcn@laichau.gov.vn")
    }
}
