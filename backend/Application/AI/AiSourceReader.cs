using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Cdsqg.Application.AI;

public interface IAiSourceReader { Task<CollectedDocument> ReadAsync(string url, CancellationToken ct); }
public sealed class AiSourceReader : IAiSourceReader
{
    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (IPAddress.IsLoopback(address)) return false;
        var b = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
            return b[0] is not (0 or 10 or 127) && !(b[0] == 169 && b[1] == 254) && !(b[0] == 172 && b[1] is >= 16 and <= 31)
                && !(b[0] == 192 && b[1] == 168) && !(b[0] == 100 && b[1] is >= 64 and <= 127) && b[0] < 224;
        return address.AddressFamily == AddressFamily.InterNetworkV6 && (b[0] & 0xE0) == 0x20;
    }
    public static SocketsHttpHandler CreatePublicHandler()
    {
        return new SocketsHttpHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All,
            ConnectCallback = async (context, token) => {
                var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, token);
                if (addresses.Length == 0 || addresses.Any(a => !IsPublicAddress(a))) throw new InvalidOperationException("Nguồn không trỏ tới địa chỉ Internet công khai.");
                var socket = new Socket(addresses[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try { await socket.ConnectAsync(new IPEndPoint(addresses[0], context.DnsEndPoint.Port), token); return new NetworkStream(socket, true); }
                catch { socket.Dispose(); throw; }
            }};
    }
    public async Task<CollectedDocument> ReadAsync(string url, CancellationToken ct)
    {
        using var handler = CreatePublicHandler();
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(45) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CDSQG-SourceReview/1.0");
        for (var redirect = 0; redirect < 5; redirect++)
        {
            if (!AiMetricCatalog.IsAllowedUrl(url)) throw new InvalidOperationException("Chỉ được đọc HTTPS từ danh sách tên miền chính thức đã cho phép.");
            using var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if ((int)res.StatusCode is >= 300 and < 400 && res.Headers.Location != null) { url = new Uri(new Uri(url), res.Headers.Location).AbsoluteUri; continue; }
            res.EnsureSuccessStatusCode();
            const int maxBytes = 12 * 1024 * 1024;
            if (res.Content.Headers.ContentLength > maxBytes) throw new InvalidOperationException("Tài liệu vượt giới hạn 12 MB.");
            await using var input = await res.Content.ReadAsStreamAsync(ct);
            using var output = new MemoryStream(); var buffer = new byte[16384]; int read;
            while ((read = await input.ReadAsync(buffer, ct)) > 0) { if (output.Length + read > maxBytes) throw new InvalidOperationException("Tài liệu vượt giới hạn 12 MB."); await output.WriteAsync(buffer.AsMemory(0, read), ct); }
            var bytes = output.ToArray(); var pages = new List<SourcePage>(); var links = new List<string>(); var title = url;
            if (bytes.AsSpan().StartsWith("%PDF"u8))
            {
                using var pdf = PdfDocument.Open(bytes);
                if (pdf.NumberOfPages > 350) throw new InvalidOperationException("PDF vượt giới hạn 350 trang; chọn phụ lục hoặc tài liệu ngắn hơn.");
                var length = 0;
                foreach (var page in pdf.GetPages()) { ct.ThrowIfCancellationRequested(); var text = ContentOrderTextExtractor.GetText(page); length += text.Length; if (length > 900000) throw new InvalidOperationException("Nội dung vượt giới hạn xử lý; chọn tài liệu nhỏ hơn."); pages.Add(new(page.Number, text)); }
                if (pages.Sum(p => p.Text.Trim().Length) < 100) throw new InvalidOperationException("PDF dạng ảnh chưa có lớp văn bản; cần bản có thể chọn chữ hoặc OCR trước.");
            }
            else
            {
                var type = res.Content.Headers.ContentType?.MediaType ?? "";
                if (!type.Contains("html") && !type.StartsWith("text/")) throw new InvalidOperationException("Nguồn chỉ hỗ trợ HTML, văn bản hoặc PDF có lớp chữ.");
                var html = Encoding.UTF8.GetString(bytes);
                title = WebUtility.HtmlDecode(Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(1)).Groups[1].Value);
                foreach (Match m in Regex.Matches(html, "href\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                    if (Uri.TryCreate(new Uri(url), WebUtility.HtmlDecode(m.Groups[1].Value), out var link) && AiMetricCatalog.IsAllowedUrl(link.AbsoluteUri)) links.Add(link.AbsoluteUri.Split('#')[0]);
                html = Regex.Replace(html, @"<(script|style|nav|header|footer)\b[^>]*>.*?</\1>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(2));
                html = Regex.Replace(html, @"</(?:p|div|tr|h\d|li)>|<br\s*/?>", "\n", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                html = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", " ", RegexOptions.None, TimeSpan.FromSeconds(1)));
                if (html.Length > 300000) throw new InvalidOperationException("Trang nguồn quá dài; chọn bài báo hoặc PDF cụ thể.");
                pages.Add(new(1, html));
            }
            return new(url, string.IsNullOrWhiteSpace(title) ? url : title[..Math.Min(title.Length, 500)], bytes, pages, links.Distinct().ToList());
        }
        throw new InvalidOperationException("Nguồn chuyển hướng quá nhiều lần.");
    }
}

