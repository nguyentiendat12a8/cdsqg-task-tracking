using System.Text.RegularExpressions;
using Cdsqg.Core.Entities;
namespace Cdsqg.Application.Services;
public static class DigitalNewsCollectionFilter {
 private static readonly string[] Actions=["ban hanh","phe duyet","thong qua","cong bo"];
 private static readonly (string Key,string Text)[] Topics=[("chuyen doi so","chuyển đổi số"),("chinh phu so","chính phủ số"),("chinh quyen so","chính quyền số"),("chinh phu dien tu","chính phủ điện tử"),("chinh quyen dien tu","chính quyền điện tử")];
 private static bool Has(string text,string phrase)=>(" "+text+" ").Contains(" "+phrase+" ",StringComparison.Ordinal);
 public static bool IsAction(string term)=>Actions.Contains(DigitalPlanText.Normalize(term));
 public static string SearchTopic(string phrase){var normalized=DigitalPlanText.Normalize(phrase);if(normalized.Contains("kien truc"))return "\"kiến trúc\" (\"số\" OR \"điện tử\")";foreach(var topic in Topics)if(Has(normalized,topic.Key))return "\""+topic.Text+"\" (\"kế hoạch\" OR \"đề án\" OR \"chương trình\" OR \"nghị quyết\" OR \"chiến lược\")";return "\""+phrase.Replace("\"","")+"\"";}
 public static bool Matches(string title,DigitalPlanKeyword keyword){
  var text=DigitalPlanText.Normalize(title);var phrase=DigitalPlanText.Normalize(keyword.Phrase);
  if(phrase.Length==0)return false;
  var architecture=phrase.Contains("kien truc");
  var topics=Topics.Where(t=>Has(phrase,t.Key)).ToArray();
  if(architecture){if(!Has(text,"kien truc")||!Regex.IsMatch(text,@"\b(so|dien tu)\b"))return false;}
  else if(topics.Length>0){if(!topics.Any(t=>Has(text,t.Key))||!Regex.IsMatch(text,@"\b(ke hoach|de an|chuong trinh|nghi quyet|chien luoc)\b"))return false;}
  else if(!Has(text,phrase))return false;
  var conditions=keyword.Conditions.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Where(x=>!IsAction(x)).ToArray();
  // Issuance actions are evidence for the manual AI read, not a mandatory collection gate.
  if(conditions.Length>0&&!conditions.Any(x=>Has(text,DigitalPlanText.Normalize(x))))return false;
  return true;
 }
}
