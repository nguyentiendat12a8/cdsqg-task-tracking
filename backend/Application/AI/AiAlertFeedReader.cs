using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Cdsqg.Application.AI;
public sealed record AlertFeedEntry(string Title,string Url,DateTime? PublishedAt);
public interface IAiAlertFeedReader {Task<List<AlertFeedEntry>> ReadAsync(string url,CancellationToken ct);}
public sealed class AiAlertFeedReader(IHttpClientFactory clients):IAiAlertFeedReader
{
    public static string ValidateUrl(string url)
    {
        if(url.Length>1000||!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.Port!=443||uri.UserInfo!=""||uri.Fragment!="")
            throw new InvalidOperationException("URL RSS cần HTTPS từ Google Alerts, cổng thông tin nhà nước hoặc báo chí được phép.");
        var googleAlerts=(uri.IdnHost is "www.google.com" or "google.com")&&uri.Query==""&&Regex.IsMatch(uri.AbsolutePath,@"^/alerts/feeds/[0-9]+/[0-9]+/?$",RegexOptions.None,TimeSpan.FromSeconds(1));
        var query=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var nso=uri.IdnHost=="www.nso.gov.vn"&&uri.AbsolutePath=="/feed/"&&(query.Count==0 || query.Count==1&&query.TryGetValue("s",out var term)&&term.Count==1&&term.ToString().Length is >0 and <=100);
        var official=AiMetricCatalog.IsAllowedUrl(url)&&uri.IdnHost is not ("news.google.com" or "www.nso.gov.vn" or "nso.gov.vn");
        if(!googleAlerts&&!nso&&!official)throw new InvalidOperationException("Chỉ nhận RSS Google Alerts, cổng thông tin .gov.vn hoặc báo chí trong danh sách nguồn tin cho phép.");
        return googleAlerts?$"https://www.google.com{uri.AbsolutePath.TrimEnd('/')}":uri.AbsoluteUri;
    }
    public async Task<List<AlertFeedEntry>> ReadAsync(string url,CancellationToken ct)
    {
        using var response=await clients.CreateClient("ai-alert-feed").GetAsync(ValidateUrl(url),HttpCompletionOption.ResponseHeadersRead,ct);
        if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"Không đọc được RSS (HTTP {(int)response.StatusCode}). Kiểm tra URL hoặc quyền truy cập nguồn.");
        const int max=2*1024*1024;
        if(response.Content.Headers.ContentLength>max)throw new InvalidOperationException("RSS vượt giới hạn 2 MB.");
        await using var input=await response.Content.ReadAsStreamAsync(ct);using var output=new MemoryStream();var buffer=new byte[16384];int read;
        while((read=await input.ReadAsync(buffer,ct))>0){if(output.Length+read>max)throw new InvalidOperationException("RSS vượt giới hạn 2 MB.");output.Write(buffer,0,read);}
        return Parse(output.ToArray());
    }
    public static string ArticleUrl(string value)
    {
        if(Uri.TryCreate(value,UriKind.Absolute,out var uri)&&uri.Scheme=="https"&&uri.IdnHost is "www.google.com" or "google.com"&&uri.AbsolutePath=="/url")
        {
            var parts=uri.Query.TrimStart('?').Split('&').Select(p=>p.Split('=',2)).Where(p=>p.Length==2).ToList();
            value=parts.Where(p=>p[0]=="url").Select(p=>Uri.UnescapeDataString(p[1])).FirstOrDefault()
                ??parts.Where(p=>p[0]=="q").Select(p=>Uri.UnescapeDataString(p[1])).FirstOrDefault()??"";
        }
        return value;
    }
    public static List<AlertFeedEntry> Parse(byte[] bytes)
    {
        using var input=new MemoryStream(bytes);using var reader=XmlReader.Create(input,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=2*1024*1024});
        var doc=XDocument.Load(reader);var root=doc.Root??throw new InvalidOperationException("RSS không hợp lệ.");
        if(root.Name.LocalName is not ("feed" or "rss"))throw new InvalidOperationException("Nguồn không phải RSS/Atom.");
        var entries=root.Name.LocalName=="feed"?root.Elements().Where(e=>e.Name.LocalName=="entry"):root.Elements().Where(e=>e.Name.LocalName=="channel").Elements().Where(e=>e.Name.LocalName=="item");
        var result=new List<AlertFeedEntry>();
        foreach(var entry in entries.Take(200))
        {
            string Field(string name)=>entry.Elements().FirstOrDefault(e=>e.Name.LocalName==name)?.Value??"";
            var title=WebUtility.HtmlDecode(Regex.Replace(Field("title"),"<[^>]*>"," ",RegexOptions.None,TimeSpan.FromSeconds(1)));
            title=Regex.Replace(title,@"\s+"," ",RegexOptions.None,TimeSpan.FromSeconds(1)).Trim();
            var link=entry.Elements().Where(e=>e.Name.LocalName=="link"&&(e.Attribute("rel")==null||e.Attribute("rel")?.Value=="alternate")).FirstOrDefault();
            var url=ArticleUrl(link?.Attribute("href")?.Value??link?.Value??"");
            var date=Field("published");if(date=="")date=Field("updated");if(date=="")date=Field("pubDate");
            DateTime? published=DateTimeOffset.TryParse(date,System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.AllowWhiteSpaces,out var parsed)?parsed.UtcDateTime:null;
            result.Add(new(title,url,published));
        }
        return result;
    }
}
