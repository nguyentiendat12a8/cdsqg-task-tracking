namespace Cdsqg.Core.Entities;
public class AiAlertFeed
{
    public DateTime? DeletedAt { get; set; }
    public Guid Id {get;set;}=Guid.NewGuid();
    public Guid DocumentId {get;set;}=Guid.Parse("12660000-0000-0000-0000-000000001266");
    public string Name {get;set;}="";
    public string MetricKeysJson { get; set; } = "[]";
    public string AgencyIdsJson { get; set; } = "[]";
    public string FeedUrl {get;set;}="";
    public bool Enabled {get;set;}
    public int ScanMinute {get;set;}=480;
    public DateTime? NextScanAt {get;set;}
    public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
    public string UpdatedBy {get;set;}="";
}
public class AiAlertScan
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public Guid FeedId {get;set;}
    public string Status {get;set;}="Queued";
    public string Trigger {get;set;}="Manual";
    public string RequestedBy {get;set;}="";
    public DateTime QueuedAt {get;set;}=DateTime.UtcNow;
    public DateTime? StartedAt {get;set;}
    public DateTime? CompletedAt {get;set;}
    public DateTime? LeaseUntil {get;set;}
    public int Received {get;set;}
    public int Added {get;set;}
    public int Duplicates {get;set;}
    public int Blocked {get;set;}
    public string Error {get;set;}="";
}
