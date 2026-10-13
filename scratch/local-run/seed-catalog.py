import json
from pathlib import Path
root=Path('.')
names=sorted(p['name'] for p in json.loads(Path('frontend/src/features/dashboard/map/provinces.json').read_text(encoding='utf-8'))['provinces'])
code='''using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
namespace Cdsqg.Application.Services;
public static class ProvinceCatalog
{
    public static readonly IReadOnlyList<string> Names = Array.AsReadOnly(new[] {
NAMES
    });
    public static string Normalize(string name)
    {
        var plain = new string(name.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).Replace('đ','d').Replace('Đ','D').ToLowerInvariant();
        return Regex.Replace(plain.Trim(), @"^(ubnd\s*)?(tinh|thanh pho|tp\.?)?\s*", "").Trim();
    }
    public static string Code(string name) => "PROVINCE_" + Normalize(name).Replace(' ', '_').ToUpperInvariant();
    public static bool IsFixed(Agency agency) => Names.Any(name => string.Equals(agency.Code, Code(name), StringComparison.OrdinalIgnoreCase) || (!agency.ParentId.HasValue && Normalize(agency.Name) == Normalize(name)));
    public static bool Conflicts(Agency agency) => agency.Type == AgencyTypeEnum.Province || IsFixed(agency);
    public static void Seed(AppDbContext db)
    {
        var existing = db.Agencies.ToList();
        foreach (var name in Names)
        {
            // Preserve existing IDs, contacts, plans and linked business data.
            if (existing.Any(a => string.Equals(a.Code, Code(name), StringComparison.OrdinalIgnoreCase) || (!a.ParentId.HasValue && Normalize(a.Name) == Normalize(name)))) continue;
            var agency = new Agency { Code = Code(name), Name = "UBND " + (new[] { "Hà Nội", "Hải Phòng", "Huế", "Đà Nẵng", "Hồ Chí Minh", "Cần Thơ" }.Contains(name) ? "Thành phố " : "tỉnh ") + name, Type = AgencyTypeEnum.Province };
            db.Agencies.Add(agency); existing.Add(agency);
        }
        db.SaveChanges();
    }
}
'''.replace('NAMES','\n'.join('        '+json.dumps(n,ensure_ascii=False)+',' for n in names))
Path('backend/Application/Services/ProvinceCatalog.cs').write_text(code,encoding='utf-8')
p=Path('backend/Program.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('    // 2. Seed Reference Units Dictionary if missing','    ProvinceCatalog.Seed(db);\n\n    // 2. Seed Reference Units Dictionary if missing');p.write_text(s,encoding='utf-8')
p=Path('backend/Api/Controllers/AgencyController.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('public async Task<IActionResult> CreateAgency([FromBody] Agency agency)\n        {','public async Task<IActionResult> CreateAgency([FromBody] Agency agency)\n        {\n            if (ProvinceCatalog.Conflicts(agency)) return StatusCode(403, new { error = "Danh mục 34 tỉnh/thành cố định không được thêm mới." });')
s=s.replace('bool isFixedSystemAgency = string.Equals(existing.Code, "bkhcn",','bool isFixedSystemAgency = ProvinceCatalog.IsFixed(existing) || string.Equals(existing.Code, "bkhcn",')
marker='            if (isFixedSystemAgency)\n            {\n                // Cho phép'
guard='''            if (ProvinceCatalog.IsFixed(existing) && (dto.Code != existing.Code || dto.Name != existing.Name || dto.Type != existing.Type || dto.ParentId != existing.ParentId || dto.IsActive != existing.IsActive))
                return StatusCode(403, new { error = "Danh mục 34 tỉnh/thành không được sửa mã, tên, phân loại, cấp trên hoặc trạng thái hoạt động." });
            if (!ProvinceCatalog.IsFixed(existing) && ProvinceCatalog.Conflicts(dto))
                return StatusCode(403, new { error = "Không được chuyển cơ quan khác vào danh mục tỉnh/thành cố định." });

'''
s=s.replace(marker,guard+marker)
s=s.replace('a.ParentId,\n','a.ParentId,\n                        IsFixedProvince = ProvinceCatalog.IsFixed(a),\n');p.write_text(s,encoding='utf-8')
p=Path('frontend/src/components/AgencyManagement.vue');s=p.read_text(encoding='utf-8-sig');s=s.replace('  if (!agency) return false;','  if (!agency) return false;\n  if (agency.isFixedProvince) return true;');p.write_text(s,encoding='utf-8')
