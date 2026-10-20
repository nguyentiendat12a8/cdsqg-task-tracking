namespace Cdsqg.Core.Entities;
public class AiProviderConfiguration
{
    public int Id {get;set;}=1;
    public bool Enabled {get;set;}
    public string BaseUrl {get;set;}="https://api.openai.com/v1";
    public string Model {get;set;}="";
    public string ProtectedApiKey {get;set;}="";
    public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
    public string UpdatedBy {get;set;}="";
}
