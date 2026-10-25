namespace Cdsqg.Core.Entities;

public class AiDataSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string MetricKey { get; set; } = "";
    public string Url { get; set; } = "";
    public bool Enabled { get; set; }
    public int IntervalHours { get; set; } = 168;
    public DateTime? NextCheckAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public string LastStatus { get; set; } = "Chưa thu thập";
    public string LastError { get; set; } = "";
}
public class AiSourceDocument
{
    public Guid? InboxItemId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceId { get; set; }
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public string ContentHash { get; set; } = "";
    public string PagesJson { get; set; } = "[]";
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Collected";
    public string Error { get; set; } = "";
    public string Model { get; set; } = "";
}
public class AiDataSuggestion
{
    public Guid? TargetAgencyId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public string MetricKey { get; set; } = "";
    public int DataYear { get; set; }
    public string PeriodLabel { get; set; } = "";
    public decimal Value { get; set; }
    public string Unit { get; set; } = "";
    public string Scope { get; set; } = "Viet Nam";
    public string Quote { get; set; } = "";
    public int Page { get; set; }
    public string Methodology { get; set; } = "";
    public string Warnings { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string ReviewedBy { get; set; } = "";
    public string ReviewNote { get; set; } = "";
    public decimal? AcceptedValue { get; set; }
    public bool IsCurrent { get; set; }
}

public class AiGoalDataLink
{
    public Guid GoalTaskId { get; set; }
    public Guid SuggestionId { get; set; }
    public Guid? ProgressLogId { get; set; }
}

public class AiInboxItem
{
    public long SequenceNumber { get; set; }
    public Guid? AlertFeedId {get;set;}
    public DateTime? PublishedAt {get;set;}
    public Guid Id {get;set;}=Guid.NewGuid();
    public string Url {get;set;}="";
    public string UrlHash {get;set;}="";
    public string Title {get;set;}="";
    public string Origin {get;set;}="Google Alerts";
    public DateTime ReceivedAt {get;set;}=DateTime.UtcNow;
    public string Status {get;set;}="TitleQueued";
    public string MetricKeysJson {get;set;}="[]";
    public string Reason {get;set;}="";
    public string Error {get;set;}="";
    public string Model {get;set;}="";
    public string StorageKey {get;set;}="";
    public DateTime? LeaseUntil {get;set;}
}

public class AiInboxQueueState
{
    public int Id { get; set; } = 1;
    public Guid? Owner { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public DateTime NextReadAt { get; set; } = new(2000,1,1,0,0,0,DateTimeKind.Utc);
}

public class AiGoalKeyword
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; } = Guid.Parse("12660000-0000-0000-0000-000000001266");
    public string GoalCode { get; set; } = "";
    public string GoalTitle { get; set; } = "";
    public string KeywordsVi { get; set; } = "";
    public string KeywordsEn { get; set; } = "";
    public string ExpectedUnit { get; set; } = "";
    public string ExtractionPrompt { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
