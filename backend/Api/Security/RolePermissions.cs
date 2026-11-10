using Cdsqg.Core.Enums;

namespace Cdsqg.Api.Security;

/// <summary>
/// Bảng phân quyền tập trung theo vai trò × phân hệ × hành động (xem / thao tác).
/// ApiPermissionFilter là nơi duy nhất áp dụng bảng này cho API; frontend có bảng đối ứng
/// trong src/shared/permissions.js chỉ để ẩn/hiện nút. Đổi quyền thì sửa cả hai nơi.
///
/// | Phân hệ                                   | Admin    | Văn thư  | Cán bộ đơn vị                 |
/// |-------------------------------------------|----------|----------|-------------------------------|
/// | Tiện ích (VB pháp lý, KH CĐS, Tin tức CĐS)| Thao tác | Thao tác | Xem; KH CĐS của đơn vị mình   |
/// | Mục tiêu, nhiệm vụ, báo cáo, dashboard    | Thao tác | Xem      | Báo cáo phần được giao        |
/// | Thiết lập chung, người dùng, AI           | Thao tác | Không    | Không                         |
/// </summary>
public static class RolePermissions
{
    public const string UtilityEditorRoles = nameof(UserRoleEnum.Admin) + "," + nameof(UserRoleEnum.Clerk);

    /// <summary>Phân hệ quản trị: chỉ Admin được truy cập (kể cả xem).</summary>
    private static readonly HashSet<string> AdminOnlyControllers = new(StringComparer.Ordinal)
    {
        "User", "MasterData", "AiData", "AiInbox", "AiProvider", "AiAlerts"
    };

    /// <summary>Thao tác dữ liệu phần Tiện ích mà Văn thư được làm (controller, action).</summary>
    private static readonly HashSet<(string Controller, string Action)> UtilityWrites = new()
    {
        // Văn bản quy phạm pháp luật (không gồm "xóa toàn bộ" — chỉ Admin).
        ("LegalDocument", "CreateLegalDocument"), ("LegalDocument", "UpdateLegalDocument"),
        ("LegalDocument", "BulkImportLegalDocuments"), ("LegalDocument", "DeleteLegalDocument"),
        ("LegalDocument", "UploadLegalAttachment"),
        // Kế hoạch chuyển đổi số của cơ quan, đơn vị.
        ("Agency", "UpdatePlansAndContacts"), ("Agency", "UploadAgencyFile"), ("Agency", "UploadPlanFile"), ("Agency", "UploadPlanFileLegacy"),
        ("Agency", "DeletePlanFile"), ("Agency", "DeletePlanFileDirect"),
        // Tin tức chuyển đổi số.
        ("DigitalNews", "Criteria"), ("DigitalNews", "Save"), ("DigitalNews", "Monitoring"),
        ("DigitalNews", "Sync"), ("DigitalNews", "Read"),
        // Thông báo của chính tài khoản.
        ("Notification", "MarkAsRead"), ("Notification", "MarkAllAsRead")
    };

    public static bool SeesAllAgencies(bool admin, bool clerk) => admin || clerk;

    public static bool SeesAllAgencies(System.Security.Claims.ClaimsPrincipal user)
        => user.IsInRole(nameof(UserRoleEnum.Admin)) || user.IsInRole(nameof(UserRoleEnum.Clerk));

    public static bool IsAdminOnly(string? controller) => controller != null && AdminOnlyControllers.Contains(controller);

    public static bool ClerkCanWrite(string? controller, string? action)
        => controller != null && action != null && UtilityWrites.Contains((controller, action));
}
