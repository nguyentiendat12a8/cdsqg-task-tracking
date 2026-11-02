using System.Text.Json;
namespace Cdsqg.Application.Services;
public sealed record DigitalPlanCriteria {
 public string RelatedKeywords {get;init;}="chuyển đổi số\nkhung kiến trúc số\nkhung kiến trúc chính phủ số\nkhung kiến trúc chính quyền số\nkhung kiến trúc chính phủ điện tử\nkhung kiến trúc chính quyền điện tử\nkiến trúc chính phủ số\nkiến trúc chính quyền số\nkiến trúc chính phủ điện tử\nkiến trúc chính quyền điện tử\nchính phủ số\nchính quyền số";
 public string ActionKeywords {get;init;}="ban hành\nphê duyệt\nthông qua\ncông bố";
 public string ExcludedKeywords {get;init;}="dự thảo\ndự kiến\nlấy ý kiến\nchưa ban hành\nsẽ phê duyệt\nsẽ ban hành\nsắp ban hành\nxã\nphường\ntrường học\ncông ty\nđảng bộ\ncơ quan đảng";
 public static DigitalPlanCriteria Parse(string json){
  if(string.IsNullOrWhiteSpace(json))return new();
  using var doc=JsonDocument.Parse(json);var value=JsonSerializer.Deserialize<DigitalPlanCriteria>(json)!;
  if(doc.RootElement.TryGetProperty("ArchitectureKeywords",out var old)&&old.ValueKind==JsonValueKind.String&&!string.IsNullOrWhiteSpace(old.GetString()))
   value=value with{RelatedKeywords=MergeKeywords(value.RelatedKeywords,old.GetString()!)};
  return value;
 }
 public static string MergeKeywords(string first,string second)=>string.Join("\n",(first+"\n"+second).Split('\n',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries).DistinctBy(DigitalPlanText.Normalize));
 public static string[] Terms(string value)=>value.Split('\n',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries).Select(DigitalPlanText.Normalize).Distinct().ToArray();
 public DigitalPlanCriteria Validate(){
  if(RelatedKeywords==null||ActionKeywords==null||ExcludedKeywords==null||RelatedKeywords.Length>6200||ActionKeywords.Length>3000||ExcludedKeywords.Length>3000)throw new InvalidOperationException("Từ khóa liên quan tối đa 6200 ký tự; mỗi nhóm còn lại tối đa 3000 ký tự.");
  foreach(var (value,limit) in new[]{(RelatedKeywords,100),(ActionKeywords,50),(ExcludedKeywords,50)})if(Terms(value).Length>limit||value.Split('\n').Any(x=>x.Length>200)||value.Any(c=>char.IsControl(c)&&c is not ('\n' or '\r' or '\t')))throw new InvalidOperationException("Nhóm liên quan tối đa 100 cụm, nhóm còn lại tối đa 50 cụm; mỗi dòng tối đa 200 ký tự.");
  if(Terms(RelatedKeywords).Length==0||Terms(ActionKeywords).Length==0)throw new InvalidOperationException("Cần ít nhất một từ khóa liên quan và một từ khóa hành động.");
  return this with{RelatedKeywords=RelatedKeywords.Trim(),ActionKeywords=ActionKeywords.Trim(),ExcludedKeywords=ExcludedKeywords.Trim()};
 }
 public string Json()=>JsonSerializer.Serialize(this);
 public string Hash()=>DigitalPlanText.Hash(Json());
 public bool MatchesAny(string text,string terms)=>Terms(terms).Any(term=>(" "+text+" ").Contains(" "+term+" ",StringComparison.Ordinal));
}
