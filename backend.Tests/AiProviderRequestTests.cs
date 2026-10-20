using System.Net;
using Cdsqg.Application.AI;
using Xunit;
namespace Cdsqg.Tests;
public class AiProviderRequestTests
{
    [Fact]
    public async Task TemporaryUnavailableRetriesButStopsAfterThreeAttempts()
    {
        var handler=new Responses(HttpStatusCode.ServiceUnavailable);
        using var client=new HttpClient(handler);
        using var response=await AiProviderRequest.SendAsync(client,new Uri("https://example.com/v1/chat/completions"),"fixture",new{model="test"},default);
        Assert.Equal(3,handler.Calls);Assert.Equal(HttpStatusCode.ServiceUnavailable,response.StatusCode);
    }
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.OK)]
    public async Task DoesNotRetryAuthQuotaOrSuccess(HttpStatusCode code)
    {
        var handler=new Responses(code);using var client=new HttpClient(handler);
        using var response=await AiProviderRequest.SendAsync(client,new Uri("https://example.com/v1/chat/completions"),"fixture",new{},default);
        Assert.Equal(1,handler.Calls);
    }
    [Fact]
    public void UnexpectedExceptionDetailsAreNotExposed()=>Assert.DoesNotContain("secret",AiProviderRequest.Error(new Exception("secret")));
    [Theory]
    [InlineData("https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",true)]
    [InlineData("https://api.openai.com/v1/chat/completions",false)]
    public async Task GeminiBudgetDoesNotChangeOtherProviders(string endpoint,bool google)
    {
        var handler=new Capture();using var client=new HttpClient(handler);
        using var response=await AiProviderRequest.SendAsync(client,new Uri(endpoint),"fixture",new{model="gemini-3-flash-preview",max_completion_tokens=3500},default);
        using var json=System.Text.Json.JsonDocument.Parse(handler.Body);
        Assert.Equal(google?8000:3500,json.RootElement.GetProperty("max_completion_tokens").GetInt32());
        Assert.Equal(google,json.RootElement.TryGetProperty("reasoning_effort",out _));
    }
    sealed class Capture:HttpMessageHandler
    {
        public string Body="";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){Body=await request.Content!.ReadAsStringAsync(ct);return new(HttpStatusCode.OK);}
    }
    sealed class Responses(HttpStatusCode code):HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){Calls++;return Task.FromResult(new HttpResponseMessage(code));}
    }
}
