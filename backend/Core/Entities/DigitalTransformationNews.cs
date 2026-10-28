namespace Cdsqg.Core.Entities;
public sealed class DigitalNewsItem
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public string AgencyName {get;set;}="";
    public string Title {get;set;}="";
    public string Url {get;set;}="";
    public string UrlHash {get;set;}="";
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
