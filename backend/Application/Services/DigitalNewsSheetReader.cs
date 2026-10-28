using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
namespace Cdsqg.Application.Services;
public sealed record DigitalNewsRow(string AgencyName,string Title,string Url,string Hash,int Row);
public sealed record DigitalNewsSheet(List<DigitalNewsRow> Rows,int Skipped);
public interface IDigitalNewsSheetReader {Task<DigitalNewsSheet> ReadAsync(string sheetUrl,CancellationToken ct);}
public sealed class DigitalNewsSheetReader(IHttpClientFactory clients):IDigitalNewsSheetReader
{
    public static (string Url,string CsvUrl) ValidateSheetUrl(string value)
    {
        if(value.Length>1000||!Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.IdnHost!="docs.google.com"||uri.Port!=443||uri.UserInfo!="")throw new InvalidOperationException("Nhập link Google Sheets HTTPS hợp lệ.");
        var match=Regex.Match(uri.AbsolutePath,@"^/spreadsheets/d/([a-zA-Z0-9_-]{20,100})(?:/[^/]*)?/?$",RegexOptions.None,TimeSpan.FromSeconds(1));
        if(!match.Success)throw new InvalidOperationException("Link phải có dạng docs.google.com/spreadsheets/d/mã-sheet/edit.");
        var query=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var fragment=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery("?"+uri.Fragment.TrimStart('#'));
        string? gid=query.TryGetValue("gid",out var q)?q.ToString():fragment.TryGetValue("gid",out var f)?f.ToString():null;
        if(gid!=null&&!Regex.IsMatch(gid,@"^\d{1,20}$"))throw new InvalidOperationException("Mã trang tính (gid) không hợp lệ.");
        var root="https://docs.google.com/spreadsheets/d/"+match.Groups[1].Value;
        return (root+"/edit"+(gid==null?"":"?gid="+gid),root+"/gviz/tq?tqx=out:csv"+(gid==null?"":"&gid="+gid));
    }
    public async Task<DigitalNewsSheet> ReadAsync(string sheetUrl,CancellationToken ct)
    {
        var (_,url)=ValidateSheetUrl(sheetUrl);
        using var response=await clients.CreateClient("digital-news-sheet").GetAsync(url,HttpCompletionOption.ResponseHeadersRead,ct);
        if(!response.IsSuccessStatusCode)throw new InvalidOperationException("Không đọc được Google Sheets. Hãy chia sẻ sheet ở chế độ Bất kỳ ai có đường liên kết – Người xem (HTTP "+(int)response.StatusCode+").");
        const int max=5*1024*1024;
        if(response.Content.Headers.ContentLength>max)throw new InvalidOperationException("Sheet vượt giới hạn 5 MB.");
        await using var stream=await response.Content.ReadAsStreamAsync(ct);using var output=new MemoryStream();var buffer=new byte[16384];int size;
        while((size=await stream.ReadAsync(buffer,ct))>0){if(output.Length+size>max)throw new InvalidOperationException("Sheet vượt giới hạn 5 MB.");output.Write(buffer,0,size);}
        try{return Parse(new UTF8Encoding(false,true).GetString(output.ToArray()).TrimStart('\uFEFF'));}catch(DecoderFallbackException){throw new InvalidOperationException("Nội dung sheet không có mã hóa UTF-8 hợp lệ.");}
    }
    public static DigitalNewsSheet Parse(string csv)
    {
        if(csv.Length>5*1024*1024)throw new InvalidOperationException("Sheet vượt giới hạn 5 MB.");
        using var parser=new TextFieldParser(new StringReader(csv)){TextFieldType=FieldType.Delimited,HasFieldsEnclosedInQuotes=true,TrimWhiteSpace=true};parser.SetDelimiters(",");
        try{
            string[]? header=null;int agency=-1,title=-1,url=-1,row=0,skipped=0;
            while(!parser.EndOfData&&row<10){header=parser.ReadFields();row++;if(header==null)continue;var columns=header.Select(Normalize).ToArray();agency=Array.FindIndex(columns,x=>x is "ten bo tinh" or "bo tinh" or "co quan" or "don vi");title=Array.FindIndex(columns,x=>x is "ten bai viet" or "tieu de" or "bai viet");url=Array.FindIndex(columns,x=>x is "link nguon bai viet" or "link bai viet" or "url" or "link");if(agency>=0&&title>=0&&url>=0)break;}
            if(agency<0||title<0||url<0)throw new InvalidOperationException("Không tìm thấy cột Tên bộ/tỉnh, Tên bài viết, Link nguồn bài viết. Kiểm tra trang tính (gid) và quyền xem sheet.");
            var rows=new Dictionary<string,DigitalNewsRow>();
            while(!parser.EndOfData){var fields=parser.ReadFields()??[];row++;if(row>20010)throw new InvalidOperationException("Sheet vượt giới hạn 20.000 bản ghi.");if(fields.All(string.IsNullOrWhiteSpace))continue;if(fields.Length<=Math.Max(agency,Math.Max(title,url))){skipped++;continue;}
                var a=fields[agency].Trim();var t=fields[title].Trim();var link=CanonicalArticle(fields[url].Trim());
                if(a.Length is <1 or >300||t.Length is <1 or >2000||link==null){skipped++;continue;}
                var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(link)));
                if(rows.ContainsKey(hash))skipped++;rows[hash]=new(a,t,link,hash,row);
            }
            return new(rows.Values.ToList(),skipped);
        }catch(MalformedLineException){throw new InvalidOperationException("CSV của sheet bị lỗi định dạng; chưa nhập dữ liệu.");}
    }
    public static string? CanonicalArticle(string value)
    {
        if(value.Length>3000||!Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme is not ("https" or "http")||uri.UserInfo!=""||!uri.IdnHost.Contains('.')||IPAddress.TryParse(uri.IdnHost,out _)||uri.IdnHost.EndsWith(".local",StringComparison.OrdinalIgnoreCase)||uri.IdnHost.EndsWith(".localhost",StringComparison.OrdinalIgnoreCase))return null;
        var builder=new UriBuilder(uri){Fragment=""};builder.Query=string.Join("&",uri.Query.TrimStart('?').Split('&',StringSplitOptions.RemoveEmptyEntries).Where(p=>!p.StartsWith("utm_",StringComparison.OrdinalIgnoreCase)));return builder.Uri.AbsoluteUri;
    }
    private static string Normalize(string text)=>Regex.Replace(string.Concat(text.ToLowerInvariant().Replace('đ','d').Normalize(NormalizationForm.FormD).Where(c=>CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)).Normalize(NormalizationForm.FormC),@"[^a-z0-9]+"," ").Trim();
}
