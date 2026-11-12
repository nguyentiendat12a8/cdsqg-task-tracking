using System.Text.RegularExpressions;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cdsqg.Application.Services;

/// <summary>
/// Mã mục tiêu/nhiệm vụ theo cơ quan chủ trì: MT-BCA-01, NV-BKHCN-03. Mục giao chung (nhóm bộ, ngành, địa phương),
/// có cơ quan đồng chủ trì hoặc cơ quan chủ trì chưa có viết tắt dùng mã chung: NV-C-01.
/// OriginalCode giữ số thứ tự theo quyết định (MT-01, NV-31) để sắp xếp và đối chiếu.
/// </summary>
public static class ItemCodes
{
    public const string Shared = "C";
    private static readonly Regex ValidShortCode = new(@"^[A-ZĐ0-9]{1,15}$", RegexOptions.Compiled);
    private static readonly Regex LegacyCode = new(@"^(MT|NV)-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Prefix(ItemTypeEnum type) => type == ItemTypeEnum.Goal ? "MT" : "NV";

    public static bool IsGroupAgency(Agency agency) =>
        agency.Type == AgencyTypeEnum.Special || agency.Code.StartsWith("ALL_", StringComparison.OrdinalIgnoreCase);

    /// <summary>Viết tắt hợp lệ (chữ in hoa/số, tối đa 15 ký tự, không trùng mã chung "C") hoặc null.</summary>
    public static string? NormalizeShortCode(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToUpperInvariant();
        return s.Length > 0 && s != Shared && ValidShortCode.IsMatch(s) ? s : null;
    }

    public static string Abbreviation(Agency? lead, bool isGeneral, Guid? coLeadId)
    {
        if (isGeneral || coLeadId.HasValue || lead == null || IsGroupAgency(lead)) return Shared;
        return NormalizeShortCode(lead.ShortCode) ?? Shared;
    }

    /// <summary>Mã trống, chỉ có tiền tố hoặc theo kiểu cũ (NV-05) thì hệ thống tự cấp mã mới.</summary>
    public static bool NeedsGeneratedCode(string? code)
    {
        var s = (code ?? string.Empty).Trim();
        return s.Length == 0 || s.EndsWith('-') || LegacyCode.IsMatch(s);
    }

    private static int MaxNumber(IEnumerable<string?> codes, Regex pattern)
    {
        var max = 0;
        foreach (var code in codes)
        {
            var match = pattern.Match(code ?? string.Empty);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number) && number > max) max = number;
        }
        return max;
    }

    /// <summary>Mã mới tiếp theo theo viết tắt và số thứ tự gốc tiếp theo trong văn bản.</summary>
    public static async Task<(string Code, string OriginalCode)> NextAsync(AppDbContext db, Guid documentId, ItemTypeEnum type, string abbreviation)
    {
        var prefix = Prefix(type);
        var rows = await db.GoalTaskItems
            .Where(i => i.DocumentId == documentId && i.ItemType == type)
            .Select(i => new { i.Code, i.OriginalCode })
            .ToListAsync();
        var byAgency = new Regex($"^{prefix}-{Regex.Escape(abbreviation)}-(\\d+)$", RegexOptions.IgnoreCase);
        var original = new Regex($"^{prefix}-(\\d+)$", RegexOptions.IgnoreCase);
        var nextCode = MaxNumber(rows.Select(r => r.Code), byAgency) + 1;
        var nextOriginal = MaxNumber(rows.SelectMany(r => new[] { r.OriginalCode, r.Code }), original) + 1;
        return ($"{prefix}-{abbreviation}-{nextCode:D2}", $"{prefix}-{nextOriginal:D2}");
    }
}
