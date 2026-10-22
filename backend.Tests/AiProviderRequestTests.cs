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

    [Theory]
    [InlineData("GenerateRequestsPerDayPerProjectPerModel-FreeTier","20","theo ngày")]
    [InlineData("GenerateTokensPerMinutePerProjectPerModel","1000","theo phút")]
    [InlineData("GenerateRequestsPerDayPerProjectPerModel-FreeTier","0","hạn mức bằng 0")]
    public async Task QuotaDiagnosticsUseStructuredLimitsWithoutExposingProviderDetails(string quotaId,string value,string expected)
    {
        var body=System.Text.Json.JsonSerializer.Serialize(new{error=new{code=429,message="You exceeded your current quota, check your plan and billing details. SECRET_KEY",details=new object[]{
            new Dictionary<string,object>{{"@type","type.googleapis.com/google.rpc.QuotaFailure"},{"violations",new[]{new{quotaId,quotaValue=value,quotaMetric="generate_content",project="PRIVATE_PROJECT"}}}},
            new Dictionary<string,object>{{"@type","type.googleapis.com/google.rpc.RetryInfo"},{"retryDelay","42.2s"}}
        }}});
        using var response=new HttpResponseMessage(HttpStatusCode.TooManyRequests){Content=new StringContent(body)};
        var failure=await AiProviderRequest.FailureAsync(response,"gemini-test",default);
        Assert.Contains(expected,failure.Message);Assert.Contains("43 giây",failure.Message);Assert.Contains("Model: gemini-test",failure.Message);
        Assert.DoesNotContain("SECRET_KEY",failure.Message);Assert.DoesNotContain("PRIVATE_PROJECT",failure.Message);Assert.DoesNotContain(quotaId,failure.Message);
        Assert.Equal(failure.Message,AiProviderRequest.Error(failure));
    }
    [Fact]
    public async Task UnstructuredQuotaDoesNotInventMinuteOrDailyLimit()
    {
        using var response=new HttpResponseMessage(HttpStatusCode.TooManyRequests){Content=new StringContent("<html>SECRET_KEY</html>")};
        response.Headers.RetryAfter=new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(17));
        var failure=await AiProviderRequest.FailureAsync(response,"invalid model\nSECRET",default);
        Assert.Contains("chưa xác định rõ",failure.Message);Assert.Contains("17 giây",failure.Message);Assert.DoesNotContain("SECRET",failure.Message);Assert.DoesNotContain("theo ngày",failure.Message);Assert.DoesNotContain("theo phút",failure.Message);
    }
    [Fact]
    public async Task InsufficientQuotaIsDistinctFromShortRateLimit()
    {
        using var response=new HttpResponseMessage(HttpStatusCode.TooManyRequests){Content=new StringContent("""{"error":{"code":"insufficient_quota","message":"private account details"}}""")};
        var failure=await AiProviderRequest.FailureAsync(response,"fixture",default);
        Assert.Contains("hết quota",failure.Message);Assert.DoesNotContain("private account",failure.Message);
    }

    [Fact]
    public async Task ExtractorPreservesSpecificQuotaDiagnosis()
    {
        using var client=new HttpClient(new QuotaResponse());
        var options=Microsoft.Extensions.Options.Options.Create(new AiDataOptions{Enabled=true,ApiKey="SECRET_KEY",Model="gemini-test"});
        var extractor=new AiMetricExtractor(options,new Factory(client));
        var exception=await Assert.ThrowsAsync<InvalidOperationException>(()=>extractor.ExtractAsync(AiMetricCatalog.Get("EGDI_RANK"),new(){new(1,"Viet Nam EGDI Rank 71")},default));
        Assert.Contains("hạn mức bằng 0",exception.Message);Assert.Contains("HTTP 429",exception.Message);Assert.DoesNotContain("SECRET_KEY",exception.Message);
    }
    sealed class Factory(HttpClient client):IHttpClientFactory
    {
        public HttpClient CreateClient(string name)=>client;
    }
    sealed class QuotaResponse:HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests){Content=new StringContent("""{"error":{"message":"quota limit: 0; SECRET_KEY"}}""")});
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
