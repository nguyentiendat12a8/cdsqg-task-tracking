using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;

namespace Cdsqg.Application.Services;

public static class ReportingAgencyGroups
{
    public static bool IsGroup(Agency agency) => agency.Type == AgencyTypeEnum.Special || agency.Code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase)
        || agency.Name.Trim().ToLowerInvariant() is "các doanh nghiệp công nghệ số" or "các doanh nghiệp viễn thông";
    public static List<Agency> Members(Agency group, IEnumerable<Agency> agencies)
    {
        var candidates = agencies.Where(a => a.IsActive && !a.ParentId.HasValue && !IsGroup(a));
        return group.Code.ToUpperInvariant() switch
        {
            "ALL_AGENCIES" => candidates.Where(a => a.Type is AgencyTypeEnum.Ministry or AgencyTypeEnum.Province).ToList(),
            "ALL_MINISTRIES" or "ALL_MINISTRIES_DIRECT" => candidates.Where(a => a.Type == AgencyTypeEnum.Ministry).ToList(),
            "ALL_PROVINCES" or "ALL_PROVINCES_UBND" => candidates.Where(a => a.Type == AgencyTypeEnum.Province).ToList(),
            _ => throw new InvalidOperationException("Nhóm cơ quan này chưa được khai báo danh sách thành viên; chọn cơ quan cụ thể.")
        };
    }
}
