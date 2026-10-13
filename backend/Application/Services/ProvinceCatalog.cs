using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
namespace Cdsqg.Application.Services;
public static class ProvinceCatalog
{
    public static readonly IReadOnlyList<string> Names = Array.AsReadOnly(new[] {
        "An Giang",
        "Thành phố Bắc Ninh",
        "Cao Bằng",
        "Cà Mau",
        "Thành phố Cần Thơ",
        "Gia Lai",
        "Thành phố Huế",
        "Thành phố Hà Nội",
        "Hà Tĩnh",
        "Hưng Yên",
        "Thành phố Hải Phòng",
        "Khánh Hòa",
        "Lai Châu",
        "Lào Cai",
        "Lâm Đồng",
        "Lạng Sơn",
        "Đồng Tháp",
        "Nghệ An",
        "Ninh Bình",
        "Phú Thọ",
        "Quảng Ngãi",
        "Thành phố Quảng Ninh",
        "Quảng Trị",
        "Sơn La",
        "Thành phố Hồ Chí Minh",
        "Thanh Hóa",
        "Thái Nguyên",
        "Tuyên Quang",
        "Tây Ninh",
        "Vĩnh Long",
        "Điện Biên",
        "Thành phố Đà Nẵng",
        "Đắk Lắk",
        "Thành phố Đồng Nai",
    });
    public static string Normalize(string name)
    {
        var plain = new string(name.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ','d').Replace('Đ','D').ToLowerInvariant();
        plain = Regex.Replace(plain.Trim(), @"\s+", " ");
        var normalized = Regex.Replace(plain, @"^(?:(?:ubnd|uy ban nhan dan)\s+)?(?:(?:tinh|thanh pho|tp\.?)\s+)?", "").Trim();
        if (normalized == "thua thien hue") normalized = "hue";
        return normalized;
    }
    public static string Code(string name) => "PROVINCE_" + Normalize(name).Replace(' ', '_').ToUpperInvariant();
    public static bool IsFixed(Agency agency) => Names.Any(name => string.Equals(agency.Code, Code(name), StringComparison.OrdinalIgnoreCase) || ((!agency.ParentId.HasValue || agency.Type == AgencyTypeEnum.Province) && Normalize(agency.Name) == Normalize(name)));
    public static string DisplayName(Agency agency) => IsFixed(agency) ? Names.FirstOrDefault(name => Normalize(agency.Name) == Normalize(name) || string.Equals(agency.Code, Code(name), StringComparison.OrdinalIgnoreCase)) ?? agency.Name : agency.Name;
    public static bool Conflicts(Agency agency) => agency.Type == AgencyTypeEnum.Province || IsFixed(agency);
    public static void Seed(AppDbContext db)
    {
        var existing = db.Agencies.ToList();
        foreach (var name in Names)
        {
            var match = existing.FirstOrDefault(a => string.Equals(a.Code, Code(name), StringComparison.OrdinalIgnoreCase) || Normalize(a.Name) == Normalize(name));
            if (match != null)
            {
                if (match.Name != name)
                {
                    match.Name = name;
                }
                if (match.Code != null && match.Code != match.Code.Trim())
                {
                    match.Code = match.Code.Trim();
                }
                continue;
            }
            var agency = new Agency { Code = Code(name), Name = name, Type = AgencyTypeEnum.Province };
            db.Agencies.Add(agency); existing.Add(agency);
        }
        db.SaveChanges();
    }
}
