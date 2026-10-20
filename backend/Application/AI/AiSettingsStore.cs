using System.Security.Cryptography;
using System.Net.Http.Json;
using System.Text.Json;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cdsqg.Application.AI;
public sealed record AiProviderInput(bool Enabled,string BaseUrl,string Model,string ApiKey="",bool RemoveKey=false);
public sealed class AiSettingsStore(AppDbContext db,IDataProtectionProvider protection,IOptions<AiDataOptions> environment)
{
    private readonly IDataProtector protector=protection.CreateProtector("CDSQG.AiProvider.ApiKey.v1");
    public static void Validate(AiProviderInput input)
    {
        if(input.BaseUrl.Length>500||!Uri.TryCreate(input.BaseUrl,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.Port!=443||uri.UserInfo.Length>0||uri.Query.Length>0||uri.Fragment.Length>0
            || uri.HostNameType!=UriHostNameType.Dns || uri.Host.Equals("localhost",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("API Base URL cần HTTPS, hostname công khai, không chứa tài khoản/query/fragment.");
        if(input.Model.Length>200||input.Model.Any(char.IsControl)||input.ApiKey.Length>4096||input.ApiKey.Any(char.IsControl))throw new InvalidOperationException("Model hoặc khóa API không hợp lệ.");
    }
    public async Task<AiDataOptions> GetAsync(CancellationToken ct=default)
    {
        var row=await db.AiProviderConfigurations.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==1,ct);
        return row==null?Copy(environment.Value):FromRow(row);
    }
    private AiDataOptions FromRow(AiProviderConfiguration row)
    {
        var options=Copy(environment.Value);options.Enabled=row.Enabled;options.BaseUrl=row.BaseUrl;options.Model=row.Model;
        try{options.ApiKey=string.IsNullOrEmpty(row.ProtectedApiKey)?"":protector.Unprotect(row.ProtectedApiKey);}
        catch(CryptographicException){options.ApiKey="";}
        return options;
    }
    private static AiDataOptions Copy(AiDataOptions value)=>new(){Enabled=value.Enabled,BaseUrl=value.BaseUrl,Model=value.Model,ApiKey=value.ApiKey,UploadDirectory=value.UploadDirectory,MaxChunksPerDocument=value.MaxChunksPerDocument,MaxDocumentsPerRun=value.MaxDocumentsPerRun};
    public async Task<AiDataOptions> PreviewAsync(AiProviderInput input,CancellationToken ct)
    {
        Validate(input);var value=await GetAsync(ct);value.Enabled=input.Enabled;value.BaseUrl=input.BaseUrl.TrimEnd('/');value.Model=input.Model.Trim();
        if(input.RemoveKey)value.ApiKey="";else if(!string.IsNullOrWhiteSpace(input.ApiKey))value.ApiKey=input.ApiKey.Trim();
        return value;
    }
    public async Task<object> PublicAsync(CancellationToken ct)
    {
        var options=await GetAsync(ct);var row=await db.AiProviderConfigurations.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==1,ct);
        return new{options.Enabled,options.BaseUrl,options.Model,hasApiKey=!string.IsNullOrWhiteSpace(options.ApiKey),configured=options.Ready,source=row==null?"Environment":"Database",row?.UpdatedAt,row?.UpdatedBy};
    }
    public async Task SaveAsync(AiProviderInput input,string actor,CancellationToken ct)
    {
        var options=await PreviewAsync(input,ct);
        if(options.Enabled&&!options.Ready)throw new InvalidOperationException("Cần nhập model và API key trước khi bật đọc tài liệu.");
        var row=await db.AiProviderConfigurations.SingleOrDefaultAsync(x=>x.Id==1,ct);
        if(row==null){row=new();db.Add(row);}
        row.Enabled=options.Enabled;row.BaseUrl=options.BaseUrl;row.Model=options.Model;row.ProtectedApiKey=string.IsNullOrEmpty(options.ApiKey)?"":protector.Protect(options.ApiKey);row.UpdatedAt=DateTime.UtcNow;row.UpdatedBy=actor;
        await db.SaveChangesAsync(ct);
    }
    public async Task<object> TestAsync(AiProviderInput input,IHttpClientFactory clients,CancellationToken ct)
    {
        var options=await PreviewAsync(input,ct);
        if(string.IsNullOrWhiteSpace(options.ApiKey)||string.IsNullOrWhiteSpace(options.Model))throw new InvalidOperationException("Cần model và API key để kiểm tra kết nối.");
        var body=new{model=options.Model,max_completion_tokens=1500,messages=new[]{new{role="user",content="Return JSON with ok=true."}},response_format=new{type="json_schema",json_schema=new{name="connection_test",strict=true,schema=new{type="object",properties=new{ok=new{type="boolean"}},required=new[]{"ok"},additionalProperties=false}}}};
        using var request=new HttpRequestMessage(HttpMethod.Post,options.BaseUrl+"/chat/completions"){Content=JsonContent.Create(body)};
        request.Headers.Authorization=new("Bearer",options.ApiKey);
        using var response=await clients.CreateClient("ai-provider").SendAsync(request,ct);
        if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"Nhà cung cấp trả HTTP {(int)response.StatusCode}. Kiểm tra URL, model, API key/quota hoặc hỗ trợ JSON Schema.");
        using var data=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var choice=data.RootElement.GetProperty("choices")[0];
        if(choice.GetProperty("finish_reason").GetString()!="stop")throw new InvalidOperationException("Kết nối có phản hồi nhưng bài kiểm tra chưa hoàn tất; kiểm tra model/ngân sách output.");
        using var content=JsonDocument.Parse(choice.GetProperty("message").GetProperty("content").GetString()!);
        if(!content.RootElement.GetProperty("ok").GetBoolean())throw new InvalidOperationException("Model chưa trả đúng cấu trúc dữ liệu cần dùng.");
        return new{message="Kết nối thành công; model hỗ trợ cấu trúc JSON cần dùng. Chưa lưu cấu hình và chưa đọc tài liệu.",model=options.Model};
    }
    public async Task<object> ListModelsAsync(AiProviderInput input,IHttpClientFactory clients,CancellationToken ct)
    {
        var options=await PreviewAsync(input,ct);
        if(string.IsNullOrWhiteSpace(options.ApiKey))throw new InvalidOperationException("Nhập hoặc lưu API key trước khi lấy danh sách AI.");
        using var request=new HttpRequestMessage(HttpMethod.Get,options.BaseUrl+"/models");
        request.Headers.Authorization=new("Bearer",options.ApiKey);
        using var response=await clients.CreateClient("ai-provider").SendAsync(request,ct);
        if(!response.IsSuccessStatusCode)throw new InvalidOperationException($"Không lấy được danh sách AI (HTTP {(int)response.StatusCode}). Kiểm tra khóa và quyền đọc danh sách model.");
        using var data=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var ids=data.RootElement.GetProperty("data").EnumerateArray().Take(2000).Select(x=>x.GetProperty("id").GetString()).Where(x=>!string.IsNullOrWhiteSpace(x)&&x.Length<=200).Select(x=>x!).Distinct().OrderBy(x=>x).ToList();
        if(new Uri(options.BaseUrl).Host.Equals("api.openai.com",StringComparison.OrdinalIgnoreCase))
            ids=ids.Where(id=>(id.StartsWith("gpt-",StringComparison.OrdinalIgnoreCase)||System.Text.RegularExpressions.Regex.IsMatch(id,@"^o\d"))&&!new[]{"image","audio","realtime","transcribe","search","tts","moderation"}.Any(term=>id.Contains(term,StringComparison.OrdinalIgnoreCase))).ToList();
        return new{models=ids};
    }
}
