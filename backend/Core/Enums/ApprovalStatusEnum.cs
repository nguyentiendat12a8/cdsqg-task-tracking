namespace Cdsqg.Core.Enums
{
    public enum ApprovalStatusEnum
    {
        Approved = 1, // Đã duyệt (Mặc định cho Cấp 1 Admin & Cấp 2 Bộ/Ngành/Địa phương)
        Pending = 2,  // Chờ duyệt (Khi Cấp 3 Báo cáo tiến độ)
        Rejected = 3  // Từ chối (Khi Cấp 2 từ chối báo cáo của Cấp 3)
    }
}
