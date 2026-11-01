using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cdsqg.Application.AI;

public static class GoogleNewsDecoder
{
    private static readonly Regex ArtIdRegex = new(@"/(?:articles|read)/([^/?]+)", RegexOptions.Compiled);
    private static readonly Regex SgRegex = new(@"data-n-a-sg=""([^""]+)""", RegexOptions.Compiled);
    private static readonly Regex TsRegex = new(@"data-n-a-ts=""([^""]+)""", RegexOptions.Compiled);

    public static async Task<string> DecodeUrlAsync(string url, HttpClient? client = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.Contains("news.google.com")) return url;
        var match = ArtIdRegex.Match(url);
        if (!match.Success) return url;
        var artId = match.Groups[1].Value;

        var shouldDispose = client == null;
        var httpClient = client ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }) { Timeout = TimeSpan.FromSeconds(15) };
        if (shouldDispose) httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        try
        {
            var pageUrl = $"https://news.google.com/rss/articles/{artId}?hl=vi&gl=VN&ceid=VN:vi";
            var html = await httpClient.GetStringAsync(pageUrl, ct);

            var sgMatch = SgRegex.Match(html);
            var tsMatch = TsRegex.Match(html);
            if (!sgMatch.Success || !tsMatch.Success) return url;

            var sg = sgMatch.Groups[1].Value;
            var tsStr = tsMatch.Groups[1].Value;
            long.TryParse(tsStr, out var ts);

            var ctx = new object?[] {
                new object?[] { "X", "X", new object[] { "X", "X" }, null, null, 1, 1, "US:en", null, 1, null, null, null, null, null, 0, 1 },
                "X", "X", 1, new object[] { 1, 1, 1 }, 1, 1, null, 0, 0, null, 0
            };
            var inner = new object?[] { "garturlreq", ctx, artId, ts, sg };
            var innerJson = JsonSerializer.Serialize(inner);
            var envelope = new object?[] { new object?[] { "Fbv4je", innerJson, null, "0" } };
            var envelopesJson = JsonSerializer.Serialize(new object?[] { envelope });

            var content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("f.req", envelopesJson)
            ]);

            var postRes = await httpClient.PostAsync("https://news.google.com/_/DotsSplashUi/data/batchexecute", content, ct);
            var resText = await postRes.Content.ReadAsStringAsync(ct);

            if (resText.StartsWith(")]}'"))
            {
                var idx = resText.IndexOf('\n');
                if (idx >= 0) resText = resText.Substring(idx + 1);
            }

            using var doc = JsonDocument.Parse(resText);
            foreach (var row in doc.RootElement.EnumerateArray())
            {
                if (row.ValueKind == JsonValueKind.Array && row.GetArrayLength() >= 3)
                {
                    var payload = row[2];
                    if (payload.ValueKind == JsonValueKind.String)
                    {
                        var payloadStr = payload.GetString();
                        if (payloadStr != null && payloadStr.Contains("garturlres"))
                        {
                            using var innerDoc = JsonDocument.Parse(payloadStr);
                            if (innerDoc.RootElement.ValueKind == JsonValueKind.Array && innerDoc.RootElement.GetArrayLength() >= 2)
                            {
                                var decoded = innerDoc.RootElement[1].GetString();
                                if (!string.IsNullOrWhiteSpace(decoded) && Uri.TryCreate(decoded, UriKind.Absolute, out _))
                                    return decoded;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // fallback to original URL
        }
        finally
        {
            if (shouldDispose) httpClient.Dispose();
        }

        return url;
    }
}
