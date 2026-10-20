with open('backend/Application/AI/AiMetricCatalog.cs', 'r', encoding='utf-8') as f:
    text = f.read()

new_content = '''using Cdsqg.Infrastructure.Data;
using Cdsqg.Core.Enums;
using Microsoft.EntityFrameworkCore;
namespace Cdsqg.Application.AI;

public sealed record AiMetric(string Key, string Name, string Unit, string Definition, string[] GoalCodes, string KeywordsVi = "", string KeywordsEn = "", string ExtractionPrompt = "");
public static class AiMetricCatalog
{
    public static readonly AiMetric[] Metrics = [
        new("EGDI_RANK", "Thứ hạng EGDI toàn cầu của Việt Nam", "rank",
            "Thứ hạng EGDI toàn cầu của Việt Nam trong UN E-Government Survey. Không lấy EGDI score, EPI, OSI, LOSI hoặc thứ hạng ASEAN.",
            ["MT-12", "MT-25"],
            "chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí",
            "e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam",
            "Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank)."),
        new("DIGITAL_GDP_SHARE", "Tỷ trọng kinh tế số trong GDP", "%",
            "Tỷ trọng kinh tế số trong GDP toàn quốc Việt Nam, không phải GRDP địa phương, tốc độ tăng trưởng hoặc doanh thu ICT. Lưu rõ phương pháp, sơ bộ/ước tính/chính thức.",
            ["MT-13", "MT-26"],
            "kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng",
            "digital economy, gdp, share of gdp, gross domestic product, digital economy share, vietnam",
            "Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%)."),
        new("ECOM_RETAIL_SHARE", "Tỷ trọng doanh thu thương mại điện tử", "%",
            "Tỷ trọng doanh thu thương mại điện tử trong tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng toàn quốc Việt Nam; không phải tốc độ tăng trưởng hoặc thị phần sàn.",
            ["MT-16"],
            "thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c",
            "e-commerce, ecommerce, retail, consumer services, b2c, online retail share, vietnam",
            "Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%).")
    ];
    public static AiMetric Get(string key) => Metrics.FirstOrDefault(x => x.Key == key)
        ?? throw new InvalidOperationException("Chỉ hỗ trợ 3 chỉ số của 5 mục tiêu ưu tiên.");
    public static async Task<AiMetric> ResolveAsync(AppDbContext db, string key, CancellationToken ct)
    {
        if (!key.StartsWith("GOAL_", StringComparison.Ordinal))
        {
            var baseMetric = Get(key);
            var kw = await db.AiGoalKeywords.AsNoTracking().FirstOrDefaultAsync(k => baseMetric.GoalCodes.Contains(k.GoalCode), ct);
            if (kw != null)
            {
                return baseMetric with
                {
                    Unit = !string.IsNullOrWhiteSpace(kw.ExpectedUnit) ? kw.ExpectedUnit : baseMetric.Unit,
                    KeywordsVi = !string.IsNullOrWhiteSpace(kw.KeywordsVi) ? kw.KeywordsVi : baseMetric.KeywordsVi,
                    KeywordsEn = !string.IsNullOrWhiteSpace(kw.KeywordsEn) ? kw.KeywordsEn : baseMetric.KeywordsEn,
                    ExtractionPrompt = !string.IsNullOrWhiteSpace(kw.ExtractionPrompt) ? kw.ExtractionPrompt : baseMetric.ExtractionPrompt
                };
            }
            return baseMetric;
        }

        if (!Guid.TryParse(key[5..], out var id)) throw new InvalidOperationException("Mục tiêu không hợp lệ.");
        var goal = await db.GoalTaskItems.Include(g => g.Unit).AsNoTracking().SingleOrDefaultAsync(g => g.Id == id && g.ItemType == ItemTypeEnum.Goal, ct)
            ?? throw new InvalidOperationException("Không tìm thấy mục tiêu đã chọn.");
        if (goal.EvaluationType != EvaluationTypeEnum.Quantitative) throw new InvalidOperationException("Mục tiêu định tính chưa có chỉ tiêu số để bóc tách. Hãy chọn mục tiêu định lượng.");

        var kwGoal = await db.AiGoalKeywords.AsNoTracking().FirstOrDefaultAsync(k => k.GoalCode == goal.Code, ct);

        var isRank = (kwGoal != null && kwGoal.ExpectedUnit.Equals("rank", StringComparison.OrdinalIgnoreCase))
            || goal.Code is "MT-12" or "MT-25"
            || goal.Unit?.Code.Equals("RANK", StringComparison.OrdinalIgnoreCase) == true
            || goal.Unit?.Name.Trim().ToLowerInvariant() is "hạng" or "thứ hạng" or "rank";

        var unit = kwGoal != null && !string.IsNullOrWhiteSpace(kwGoal.ExpectedUnit)
            ? kwGoal.ExpectedUnit
            : (isRank ? "rank" : (!goal.UnitId.HasValue || goal.Unit?.Name == "%" || goal.Unit?.Code == "PERCENT" ? "%" : "số lượng"));

        var prompt = kwGoal != null && !string.IsNullOrWhiteSpace(kwGoal.ExtractionPrompt)
            ? kwGoal.ExtractionPrompt
            : $"Tìm số liệu thực tế đo đúng mục tiêu: {goal.Title}. Đơn vị: {unit}. Không lấy ngưỡng yêu cầu trong tên mục tiêu làm kết quả. Nếu có nhiều chỉ tiêu không xác định được giá trị đại diện thì không trả kết quả. Chỉ lấy số liệu có dẫn chứng rõ về năm và phạm vi Việt Nam.";

        return new(
            key,
            goal.Code + " · " + goal.Title,
            unit,
            prompt,
            [goal.Code],
            kwGoal?.KeywordsVi ?? goal.Title,
            kwGoal?.KeywordsEn ?? "",
            prompt);
    }
    public static readonly string[] AllowedHosts = ["desapublications.un.org", "publicadministration.un.org", "publicadministration.desa.un.org", "nso.gov.vn", "www.nso.gov.vn", "gso.gov.vn", "www.gso.gov.vn", "moit.gov.vn", "www.moit.gov.vn", "idea.gov.vn", "www.idea.gov.vn", "mst.gov.vn", "www.mst.gov.vn"];
    public static bool IsAllowedUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == "https" && u.Port == 443 && string.IsNullOrEmpty(u.UserInfo) && AllowedHosts.Contains(u.IdnHost, StringComparer.OrdinalIgnoreCase);
    public static bool Reached(string code, decimal value) => code switch {
        "MT-12" => value <= 50, "MT-25" => value <= 30, "MT-26" => value >= 50,
        "MT-13" => value >= 30, "MT-16" => value >= 15, _ => false
    };
}
public class AiDataOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = "";
    public string UploadDirectory { get; set; } = "";
    public string Model { get; set; } = "";
    public int MaxDocumentsPerRun { get; set; } = 3;
    public int MaxChunksPerDocument { get; set; } = 3;
    public bool Ready => Enabled && !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Model);
}
public sealed record SourcePage(int Number, string Text);
public sealed record CollectedDocument(string Url, string Title, byte[] Bytes, List<SourcePage> Pages, List<string> Links);
public class ExtractedMetric
{
    public int DataYear { get; set; }
    public string PeriodLabel { get; set; } = "";
    public decimal Value { get; set; }
    public string Unit { get; set; } = "";
    public string Scope { get; set; } = "";
    public string Quote { get; set; } = "";
    public int Page { get; set; }
    public string Methodology { get; set; } = "";
    public bool IsForecastOrTarget { get; set; }
    public string Warnings { get; set; } = "";
}
'''

with open('backend/Application/AI/AiMetricCatalog.cs', 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(new_content)
print("Updated AiMetricCatalog.cs successfully")
