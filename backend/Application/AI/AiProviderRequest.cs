using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
namespace Cdsqg.Application.AI;
public static class AiProviderRequest
{
    public static async Task<HttpResponseMessage> SendAsync(HttpClient client, Uri endpoint, string key, object body, CancellationToken ct)
    {
        var payload=System.Text.Json.JsonSerializer.SerializeToNode(body)!;
        // Gemini's thinking tokens share the completion budget; keep extraction bounded without truncating JSON.
        if(endpoint.Host=="generativelanguage.googleapis.com" && payload["model"]?.GetValue<string>().Contains("gemini-3",StringComparison.OrdinalIgnoreCase)==true)
        {
            payload["reasoning_effort"]="low";
            if(payload["max_completion_tokens"]?.GetValue<int>()==3500)payload["max_completion_tokens"]=8000;
        }
        for(var attempt=0;;attempt++)
        {
            using var request=new HttpRequestMessage(HttpMethod.Post,endpoint){Content=JsonContent.Create(payload)};
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",key);
            var response=await client.SendAsync(request,ct);
            if(attempt>=2 || response.StatusCode is not (HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout))return response;
            response.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(attempt+1),ct);
        }
    }
    public static string Error(Exception ex)
    {
        if(ex is InvalidOperationException && (ex.Message.StartsWith("Nhà cung cấp AI trả HTTP ") || ex.Message.StartsWith("AI chưa trả kết quả") || ex.Message.StartsWith("Chưa bật AI")))return ex.Message;
        if(ex is Microsoft.EntityFrameworkCore.DbUpdateException)return "Không lưu được kết quả vào cơ sở dữ liệu. Tài liệu được giữ lại; cần kiểm tra lỗi dữ liệu trước khi đọc lại.";
        if(ex is OperationCanceledException)return "AI phản hồi quá thời gian chờ. Tài liệu được giữ lại; bạn có thể đọc lại sau.";
        if(ex is HttpRequestException)return "Không kết nối được nhà cung cấp AI. Tài liệu được giữ lại; kiểm tra kết nối và thử lại sau.";
        return "Chưa xử lý được tài liệu. Kiểm tra cấu hình AI và file rồi đọc lại.";
    }
}
