using System.Security.Claims;
using System.Text.Json;
using Cdsqg.Application.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cdsqg.Api.Controllers;
[ApiController,Route("api/ai-provider"),Authorize(Roles="Admin")]
public sealed class AiProviderController(AiSettingsStore settings,IHttpClientFactory clients):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await settings.PublicAsync(ct));
    [HttpPut,RequestSizeLimit(16384)]
    public async Task<IActionResult> Save(AiProviderInput input,CancellationToken ct)
    {
        try{await settings.SaveAsync(input,User.FindFirstValue("FullName")??User.Identity?.Name??"Admin",ct);return Ok(new{message=input.Enabled?"Đã lưu và bật AI. Các tài liệu đang chờ sẽ được xử lý bằng cấu hình này.":"Đã lưu cấu hình. AI đang tắt; tài liệu tiếp tục được giữ trong hàng đợi."});}
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
    }
    [HttpPost("test"),RequestSizeLimit(16384)]
    public Task<IActionResult> Test(AiProviderInput input,CancellationToken ct)=>RunProvider(()=>settings.TestAsync(input,clients,ct));
    [HttpPost("models"),RequestSizeLimit(16384)]
    public Task<IActionResult> Models(AiProviderInput input,CancellationToken ct)=>RunProvider(()=>settings.ListModelsAsync(input,clients,ct));
    private async Task<IActionResult> RunProvider(Func<Task<object>> action)
    {
        try{return Ok(await action());}
        catch(InvalidOperationException e){return BadRequest(new{message=e.Message});}
        catch(HttpRequestException){return BadRequest(new{message="Không kết nối được API. Kiểm tra hostname công khai và chứng chỉ HTTPS."});}
        catch(OperationCanceledException){return BadRequest(new{message="Kết nối hết thời gian hoặc đã hủy; cấu hình chưa được lưu."});}
        catch(Exception e)when(e is JsonException or KeyNotFoundException or IndexOutOfRangeException or ArgumentException){return BadRequest(new{message="Nhà cung cấp không trả đúng cấu trúc Chat Completions/JSON Schema."});}
    }
}
