using Cdsqg.Application.AI;
var reader=new AiAlertFeedReader(new FeedClientFactory());
try {
    var entries=await reader.ReadAsync(args[0],default);
    Console.WriteLine($"RSS_OK entries={entries.Count}; allowed={entries.Count(e=>AiMetricCatalog.IsAllowedUrl(e.Url))}");
    foreach(var entry in entries.Take(5))Console.WriteLine($"{new Uri(entry.Url).Host}: {entry.Title}");
} catch(Exception ex) { Console.WriteLine($"RSS_FAILED {ex.GetType().Name}");Environment.ExitCode=1; }
sealed class FeedClientFactory:IHttpClientFactory {public HttpClient CreateClient(string name)=>new(AiSourceReader.CreatePublicHandler()){Timeout=TimeSpan.FromSeconds(30)};}
