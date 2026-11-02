namespace Cdsqg.Core.Entities;
public sealed class DigitalNewsItem
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public string AgencyName {get;set;}="";
    public string Title {get;set;}="";
    public string Url {get;set;}="";
    public string UrlHash {get;set;}="";
    public string? TitleKey {get;set;}
    public DateTime? PublishedAt {get;set;}
    public bool OriginalUrlVerified {get;set;}
    public string SourceSheetUrl {get;set;}="";
    public int SourceRow {get;set;}
    public DateTime ImportedAt {get;set;}=DateTime.UtcNow;
    public DateTime UpdatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class DigitalNewsSyncSettings
{
    public const string InitialSheet="https://docs.google.com/spreadsheets/d/1fOGT8aBm6X5ul2pk8p7zxzixEMklpqLVXEgsSCFrEDI/edit?gid=731708878";
    public int Id {get;set;}=1;
    public string SheetUrl {get;set;}=InitialSheet;
    public bool Enabled {get;set;}=true;
    public int IntervalMinutes {get;set;}=30;
    public DateOnly? PublishedFrom {get;set;}
    public DateOnly? PublishedTo {get;set;}
    public int LookbackDays {get;set;}=365;
    public int BatchSize {get;set;}=12;
    public string EvaluationCriteriaJson {get;set;}="";
    public int QueryCursor {get;set;}
    public int IntervalHours {get;set;}=2;
    public DateTime? NextRunAt {get;set;}
    public DateTime? LastStartedAt {get;set;}
    public DateTime? LastCompletedAt {get;set;}
    public string LastStatus {get;set;}="NotStarted";
    public string LastError {get;set;}="";
    public int Added {get;set;}
    public int Updated {get;set;}
    public int Unchanged {get;set;}
    public int Skipped {get;set;}
    public string UpdatedBy {get;set;}="";
    public Guid? LeaseToken {get;set;}
    public DateTime? LeaseUntil {get;set;}
}

public sealed class DigitalPlanSource {
 public Guid Id {get;set;}=Guid.NewGuid(); public int SortOrder {get;set;}
 public string AgencyName {get;set;}=""; public string Name {get;set;}=""; public string Domain {get;set;}="";
 public string RequiredTerms {get;set;}=""; public bool Enabled {get;set;}=true;
}
public sealed class DigitalPlanKeyword {
 public Guid Id {get;set;}=Guid.NewGuid(); public int SortOrder {get;set;}
 public string Name {get;set;}=""; public string Phrase {get;set;}=""; public string Conditions {get;set;}=""; public bool Enabled {get;set;}=true;
}
public sealed class DigitalPlanRead {
 public Guid Id {get;set;}=Guid.NewGuid(); public Guid ItemId {get;set;}
 public string CriteriaHash {get;set;}=""; public string CriteriaJson {get;set;}="";
 public string TitleHash {get;set;}=""; public string Title {get;set;}=""; public string AgencyName {get;set;}="";
 public string Status {get;set;}="Queued"; public string PlanKind {get;set;}="";
 public int? StartYear {get;set;} public int? EndYear {get;set;}
 public string Reason {get;set;}=""; public string Evidence {get;set;}=""; public string Model {get;set;}="";
 public Guid? RequestedByUserId {get;set;} public Guid? NotificationId {get;set;}
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow; public DateTime? CompletedAt {get;set;}
 public Guid? LeaseToken {get;set;} public DateTime? LeaseUntil {get;set;}
}
