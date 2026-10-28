using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
        if(endpoint.Host.Equals("oxionai.com",StringComparison.OrdinalIgnoreCase)||endpoint.Host.Equals("www.oxionai.com",StringComparison.OrdinalIgnoreCase))
        {
            if(payload["max_completion_tokens"] is JsonNode budget)
            {
                payload["max_tokens"]=budget.DeepClone();
                payload.AsObject().Remove("max_completion_tokens");
            }
            if(payload["response_format"]?["type"]?.GetValue<string>()=="json_schema")
            {
                var schema=payload["response_format"]?["json_schema"]?["schema"];
                if(schema!=null && payload["messages"] is JsonArray messages)
                    messages.Insert(0,new JsonObject { ["role"]="system",["content"]="Chỉ trả JSON hợp lệ đúng cấu trúc sau, không thêm văn bản ngoài JSON: "+schema.ToJsonString() });
                payload["response_format"]=new JsonObject { ["type"]="json_object" };
            }
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
    // Return only allowlisted diagnostics; never expose the provider body, key, project or account identifiers.
    public static async Task<InvalidOperationException> FailureAsync(HttpResponseMessage response,string model,CancellationToken ct)
    {
        var code=(int)response.StatusCode;
        if(response.StatusCode!=HttpStatusCode.TooManyRequests)
            return new(response.StatusCode is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
                ? $"Nhà cung cấp AI trả HTTP {code}: dịch vụ tạm thời không đáp ứng. Tài liệu được giữ lại; hãy đọc lại sau."
                : response.StatusCode==HttpStatusCode.InternalServerError
                ? "Nhà cung cấp AI trả HTTP 500: yêu cầu đã tới dịch vụ nhưng không được xử lý. Có thể do lỗi phía nhà cung cấp hoặc định dạng request/model chưa tương thích; hãy kiểm tra lại sau khi cập nhật cấu hình. Tài liệu được giữ lại."
                : $"Nhà cung cấp AI trả HTTP {code}; kiểm tra cấu hình và quyền truy cập của model. Tài liệu được giữ lại.");
        var zero=false;var daily=false;var minute=false;var tokens=false;var billing=false;
        double? retrySeconds=response.Headers.RetryAfter?.Delta?.TotalSeconds;
        if(retrySeconds==null&&response.Headers.RetryAfter?.Date is DateTimeOffset retryDate)retrySeconds=(retryDate-DateTimeOffset.UtcNow).TotalSeconds;
        try{
            using var stream=await response.Content.ReadAsStreamAsync(ct);
            var bytes=new byte[65536];var length=0;
            while(length<bytes.Length){var read=await stream.ReadAsync(bytes.AsMemory(length),ct);if(read==0)break;length+=read;}
            using var doc=JsonDocument.Parse(bytes.AsMemory(0,length));
            if(doc.RootElement.ValueKind==JsonValueKind.Object&&doc.RootElement.TryGetProperty("error",out var error)&&error.ValueKind==JsonValueKind.Object){
                var message=Text(error,"message").ToLowerInvariant();
                var errorCode=(Text(error,"code")+" "+Text(error,"type")).ToLowerInvariant();
                billing=errorCode.Contains("insufficient_quota")||message.Contains("credits are depleted")||message.Contains("billing is not enabled")||message.Contains("billing account is disabled");
                daily=errorCode.Contains("quota_exceeded")||message.Contains("per day")||message.Contains("perday")||message.Contains("daily");
                minute=errorCode.Contains("rate_limit_exceeded")||message.Contains("per minute")||message.Contains("perminute");
                tokens=message.Contains("token");
                zero=Regex.IsMatch(message,@"limit\s*:\s*0(?:[^\d.]|$)",RegexOptions.None,TimeSpan.FromSeconds(1));
                if(error.TryGetProperty("details",out var details)&&details.ValueKind==JsonValueKind.Array)
                    foreach(var detail in details.EnumerateArray()){
                        if(detail.ValueKind!=JsonValueKind.Object)continue;
                        var type=Text(detail,"@type");
                        if(type.EndsWith("QuotaFailure",StringComparison.Ordinal)&&detail.TryGetProperty("violations",out var violations)&&violations.ValueKind==JsonValueKind.Array)
                            foreach(var violation in violations.EnumerateArray()){
                                if(violation.ValueKind!=JsonValueKind.Object)continue;
                                var metric=(Text(violation,"quotaMetric")+" "+Text(violation,"quotaId")).ToLowerInvariant();
                                daily|=metric.Contains("perday")||metric.Contains("per_day");
                                minute|=metric.Contains("perminute")||metric.Contains("per_minute");
                                tokens|=metric.Contains("token");
                                zero|=Text(violation,"quotaValue")=="0";
                            }
                        if(type.EndsWith("RetryInfo",StringComparison.Ordinal)){
                            var delay=Text(detail,"retryDelay");
                            if(delay.EndsWith('s')&&double.TryParse(delay[..^1],NumberStyles.Float,CultureInfo.InvariantCulture,out var seconds)&&seconds>0&&double.IsFinite(seconds))retrySeconds=Math.Max(retrySeconds??0,seconds);
                        }
                    }
            }
        }catch(Exception ex)when(ex is JsonException or IOException or RegexMatchTimeoutException){/* Keep generic 429 when the body is absent or malformed. */}
        var reason=zero?"Nhà cung cấp báo hạn mức bằng 0 cho yêu cầu/model này. Chờ rồi đọc lại sẽ không giải quyết được; cần kiểm tra model có quota và cấu hình thanh toán của project."
            :billing?"Project AI đã hết quota hoặc tín dụng sử dụng. Cần kiểm tra hạn mức/thanh toán của project trước khi đọc lại."
            :daily?"Đã chạm hạn mức AI theo ngày"+(tokens?" về token.":" về lượt gọi.")+" Cần chờ quota được cấp lại hoặc tăng hạn mức; nghỉ một phút không khắc phục được giới hạn ngày."
            :minute?"Đã chạm giới hạn AI theo phút"+(tokens?" về token.":" về lượt gọi.")+" Hãy chờ trước khi đọc lại."
            :"Nhà cung cấp đang giới hạn yêu cầu hoặc quota, nhưng phản hồi chưa xác định rõ loại hạn mức. Kiểm tra Rate limits/Usage của project và model đang dùng.";
        var retry=retrySeconds is >0 and <=86400?$" Nhà cung cấp đề nghị chờ ít nhất {Math.Ceiling(retrySeconds.Value).ToString(CultureInfo.InvariantCulture)} giây.":"";
        var safeModel=Regex.IsMatch(model,@"^[a-zA-Z0-9._:/-]{1,100}$",RegexOptions.None,TimeSpan.FromSeconds(1))?$" Model: {model}.":"";
        return new("Nhà cung cấp AI trả HTTP 429: "+reason+safeModel+retry+" Tài liệu được giữ lại; chưa có số liệu nào được tự ghi.");
    }
    private static string Text(JsonElement element,string name)=>element.TryGetProperty(name,out var value)?value.ValueKind==JsonValueKind.String?value.GetString()??"":value.ValueKind==JsonValueKind.Number?value.GetRawText():"":"";
    public static string Error(Exception ex)
    {
        if(ex is InvalidOperationException && (ex.Message.StartsWith("Nhà cung cấp AI trả HTTP ") || ex.Message.StartsWith("AI chưa trả kết quả") || ex.Message.StartsWith("Chưa bật AI")))return ex.Message;
        if(ex is Microsoft.EntityFrameworkCore.DbUpdateException)return "Không lưu được kết quả vào cơ sở dữ liệu. Tài liệu được giữ lại; cần kiểm tra lỗi dữ liệu trước khi đọc lại.";
        if(ex is OperationCanceledException)return "AI phản hồi quá thời gian chờ. Tài liệu được giữ lại; bạn có thể đọc lại sau.";
        if(ex is HttpRequestException)return "Không kết nối được nhà cung cấp AI. Tài liệu được giữ lại; kiểm tra kết nối và thử lại sau.";
        return "Chưa xử lý được tài liệu. Kiểm tra cấu hình AI và file rồi đọc lại.";
    }
}
