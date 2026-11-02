using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Cdsqg.Application.AI;
using Cdsqg.Core.Entities;
namespace Cdsqg.Application.Services;
public static class DigitalPlanText {
 public static string Normalize(string text) {var b=new StringBuilder();foreach(var c in text.ToLowerInvariant().Replace('đ','d').Normalize(NormalizationForm.FormD))if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)b.Append(char.IsLetterOrDigit(c)?c:' ');return Regex.Replace(b.ToString(),@"\s+"," ").Trim();}
 public static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
 public static string TitleKey(string agency,string title)=>Hash(Regex.Replace(Normalize(agency),@"^(tinh|thanh pho) ","")+"|"+Normalize(title));
 public static string ReadHash(DigitalNewsItem item)=>Hash(item.AgencyName+"\n"+item.Title);
}
public sealed record DigitalPlanFound(string Title,string Url,DateTime? PublishedAt,bool OriginalUrlVerified);
public interface IDigitalPlanDiscoveryReader {Task<IReadOnlyList<DigitalPlanFound>> ReadAsync(DigitalPlanSource source,DigitalPlanKeyword keyword,DateOnly from,DateOnly? to,CancellationToken ct);}
public sealed class DigitalPlanDiscoveryReader(IHttpClientFactory factory):IDigitalPlanDiscoveryReader {
 public static string ValidateDomain(string value) {value=value.Trim().ToLowerInvariant();if(value.Length>250||!Regex.IsMatch(value,@"^(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:gov|chinhphu)\.vn(?:/[a-z0-9_./-]*)?$"))throw new InvalidOperationException("Tên miền nguồn phải là tên miền gov.vn hoặc chinhphu.vn, có thể kèm đường dẫn chuyên mục.");return value;}
 public static Uri QueryUrl(DigitalPlanSource source,DigitalPlanKeyword keyword,DateOnly from,DateOnly? to=null) {
  var domain=ValidateDomain(source.Domain);var phrase=keyword.Phrase.Replace("\"","");
  var terms=keyword.Conditions.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Where(x=>!DigitalNewsCollectionFilter.IsAction(x)).Select(x=>"\""+x.Replace("\"","")+"\"");
  var condition=string.Join(" OR ",terms);
  var query=$"site:{domain} {DigitalNewsCollectionFilter.SearchTopic(phrase)} {source.RequiredTerms} {(condition.Length>0?"("+condition+")":"")} after:{from.AddDays(-1):yyyy-MM-dd} before:{(to??DigitalNewsPublicationWindow.Today).AddDays(1):yyyy-MM-dd}";
  return new Uri("https://news.google.com/rss/search?q="+Uri.EscapeDataString(query)+"&hl=vi&gl=VN&ceid=VN:vi");
 }
 public async Task<IReadOnlyList<DigitalPlanFound>> ReadAsync(DigitalPlanSource source,DigitalPlanKeyword keyword,DateOnly from,DateOnly? to,CancellationToken ct) {
  using var client=factory.CreateClient("digital-plan-discovery");using var response=await client.GetAsync(QueryUrl(source,keyword,from,to),HttpCompletionOption.ResponseHeadersRead,ct);response.EnsureSuccessStatusCode();
  using var stream=await response.Content.ReadAsStreamAsync(ct);using var output=new MemoryStream();var buffer=new byte[8192];int n;
  while((n=await stream.ReadAsync(buffer,ct))>0){if(output.Length+n>2_000_000)throw new InvalidOperationException("Nguồn RSS vượt dung lượng cho phép.");output.Write(buffer,0,n);}
  var result=new List<DigitalPlanFound>();
  foreach(var row in AiAlertFeedReader.Parse(output.ToArray())) {
   var url=DigitalNewsSheetReader.CanonicalArticle(row.Url);if(url==null)continue;
   var uri=new Uri(url);var official=uri.Host==source.Domain.Split('/')[0]||uri.Host.EndsWith("."+source.Domain.Split('/')[0],StringComparison.OrdinalIgnoreCase);
   var wrapper=uri.Host=="news.google.com"&&uri.AbsolutePath.StartsWith("/rss/articles/");
   if((!official&&!wrapper)||row.Title.Length is <3 or >2000||!DigitalNewsPublicationWindow.Includes(row.PublishedAt,from,to))continue;
   result.Add(new(row.Title,url,row.PublishedAt,official));
  }return result;
 }
}
