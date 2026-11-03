using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class DigitalPlanMonitoringTests {
 [Theory]
 [InlineData("UBND tỉnh Hà Nội ban hành Kế hoạch chuyển đổi số năm 2026","Annual",2026,2026,"Related")]
 [InlineData("Bộ Tài chính phê duyệt Kế hoạch chuyển đổi số 2026-2030","FiveYear",2026,2030,"Related")]
 [InlineData("Bộ Tài chính ban hành Kế hoạch chuyển đổi số 2025-2028","FiveYear",2025,2028,"NeedsReview")]
 [InlineData("Xã Thư Lâm ban hành Kế hoạch chuyển đổi số năm 2026","Annual",2026,2026,"NeedsReview")]
 [InlineData("Dự thảo Kế hoạch chuyển đổi số năm 2026 sẽ ban hành","Annual",2026,2026,"NeedsReview")]
 [InlineData("UBND triển khai Kế hoạch chuyển đổi số năm 2026","Annual",2026,2026,"NeedsReview")]
 [InlineData("Bộ Tài chính ban hành Kế hoạch chuyển đổi số năm 2026","Annual",2027,2027,"NeedsReview")]
 public void RelatedRequiresExplicitIssuancePeriodAndMatchingEvidence(string title,string kind,int start,int end,string expected){
  var result=DigitalPlanTitleClassifier.Validate(title,new("Related",kind,start,end,"Lý do",title));Assert.Equal(expected,result.Status);
 }
 [Fact] public void PublicationWindowUsesVietnamMonthAndIncludesBothBoundaryDays(){
  Assert.Equal(new DateOnly(2026,11,1),DigitalNewsPublicationWindow.MonthStart(new DateTime(2026,10,31,18,0,0)));
  var from=new DateOnly(2026,11,1);var to=new DateOnly(2026,11,2);
  Assert.True(DigitalNewsPublicationWindow.Includes(new DateTime(2026,10,31,17,0,0),from,to));
  Assert.True(DigitalNewsPublicationWindow.Includes(new DateTime(2026,11,2,16,59,59),from,to));
  Assert.False(DigitalNewsPublicationWindow.Includes(new DateTime(2026,10,31,16,59,59),from,to));
  Assert.False(DigitalNewsPublicationWindow.Includes(new DateTime(2026,11,2,17,0,0),from,to));
  Assert.False(DigitalNewsPublicationWindow.Includes(null,from,to));
  var url=Uri.UnescapeDataString(DigitalPlanDiscoveryReader.QueryUrl(new(){Domain="hanoi.gov.vn"},new(){Phrase="chuyển đổi số"},from,to).Query);
  Assert.Contains("after:2026-10-31",url);Assert.Contains("before:2026-11-03",url);
  Assert.Throws<InvalidOperationException>(()=>DigitalNewsPublicationWindow.Validate(to,from));
 }
 [Fact] public void EvidenceMustActuallyBeInTitle(){Assert.Equal("NeedsReview",DigitalPlanTitleClassifier.Validate("Bộ Tài chính ban hành Kế hoạch chuyển đổi số 2026",new("Related","Annual",2026,2026,"Có kế hoạch","Bằng chứng bịa đặt")).Status);}
 [Fact] public void NativeDefaultsMatchSheetAndAllQueriesUseFixedPublicEndpoint(){
  var sources=DigitalPlanMonitoringDefaults.Sources();var keywords=DigitalPlanMonitoringDefaults.Keywords();Assert.Equal(85,sources.Length);Assert.Equal(33,keywords.Length);Assert.Equal(15,keywords.Count(x=>x.Enabled));Assert.All(sources,x=>Assert.Equal("",x.RequiredTerms));
  foreach(var source in sources)foreach(var keyword in keywords){var url=DigitalPlanDiscoveryReader.QueryUrl(source,keyword,new DateOnly(2026,11,1));Assert.Equal("news.google.com",url.Host);Assert.Contains("site%3A",url.Query);}
  Assert.Throws<InvalidOperationException>(()=>DigitalPlanDiscoveryReader.ValidateDomain("localhost"));Assert.Throws<InvalidOperationException>(()=>DigitalPlanDiscoveryReader.ValidateDomain("hanoi.gov.vn@127.0.0.1"));
 }
 [Fact] public void CustomCriteriaAffectMatchingExclusionAndPlanScope(){
  var title="Bộ Tài chính thông qua Kế hoạch chính quyền số năm 2026";var verdict=new DigitalPlanVerdict("Related","Annual",2026,2026,"Có kế hoạch",title);
  var criteria=new DigitalPlanCriteria{RelatedKeywords="chính quyền số",ActionKeywords="thông qua",ExcludedKeywords="dự thảo"};
  Assert.Equal("Related",DigitalPlanTitleClassifier.Validate(title,verdict,criteria).Status);
  Assert.Equal("NeedsReview",DigitalPlanTitleClassifier.Validate(title,verdict,criteria with{ExcludedKeywords="chính quyền số"}).Status);
  Assert.Throws<InvalidOperationException>(()=>(criteria with{RelatedKeywords=""}).Validate());
 }
 [Theory]
 [InlineData("TP.HCM ban hành Khung kiến trúc số đến năm 2030: Đột phá quản trị","Related")]
 [InlineData("Bộ Tài chính phê duyệt Khung kiến trúc chính phủ số phiên bản 3.0","Related")]
 [InlineData("UBND tỉnh Hà Nội ban hành Khung kiến trúc chính quyền số","Related")]
 [InlineData("UBND tỉnh Hà Nội dự thảo Khung kiến trúc số","NeedsReview")]
 [InlineData("UBND tỉnh Hà Nội ban hành Kế hoạch xây dựng Khung kiến trúc số","NeedsReview")]
 public void ArchitectureDoesNotRequirePlanPeriod(string title,string expected){
  var result=DigitalPlanTitleClassifier.Validate(title,new("Related","Architecture",null,null,"Có khung kiến trúc",title),new());Assert.Equal(expected,result.Status);Assert.Null(result.StartYear);Assert.Null(result.EndYear);
 }
 [Fact] public void MergedCriteriaKeepCustomKeywordsAndIgnoreLegacySwitches(){
  var merged=DigitalPlanCriteria.Parse("""{"RelatedKeywords":"chuyển đổi số","ArchitectureKeywords":"khung kiến trúc số\nKHUNG KIẾN TRÚC SỐ\nkhung riêng","Annual":false,"FiveYear":false,"Architecture":false}""");
  Assert.Equal(3,merged.RelatedKeywords.Split('\n').Length);Assert.Contains("khung riêng",merged.RelatedKeywords);
  Assert.DoesNotContain("ArchitectureKeywords",merged.Json());Assert.DoesNotContain("Annual",merged.Json());
  var title="UBND tỉnh Hà Nội ban hành Khung kiến trúc số";
  Assert.Equal("Related",DigitalPlanTitleClassifier.Validate(title,new("Related","Architecture",null,null,"Có khung",title),merged).Status);
  Assert.Equal("NeedsReview",DigitalPlanTitleClassifier.Validate(title,new("Related","Architecture",null,null,"Có khung",title),new(){RelatedKeywords="chuyển đổi số"}).Status);
 }
 [Theory]
 [InlineData("Bộ Tài chính phê duyệt Đề án chuyển đổi số giai đoạn 2026-2030","DigitalDocument","Related")]
 [InlineData("UBND tỉnh Hà Nội thông qua Nghị quyết về phát triển chính quyền số","DigitalDocument","Related")]
 [InlineData("UBND tỉnh Hà Nội công bố Chiến lược chuyển đổi số","DigitalDocument","Related")]
 [InlineData("UBND tỉnh Hà Nội phê duyệt Kiến trúc chính quyền điện tử phiên bản 3.0","Architecture","Related")]
 [InlineData("UBND tỉnh Hà Nội lấy ý kiến Đề án chuyển đổi số","DigitalDocument","NeedsReview")]
 public void CloselyRelatedDocumentAndArchitectureTermsAreRecognized(string title,string kind,string expected){Assert.Equal(expected,DigitalPlanTitleClassifier.Validate(title,new("Related",kind,null,null,"Văn bản liên quan",title)).Status);}
 [Theory]
 [InlineData("Cà Mau: Đổi mới truyền thông, lan tỏa tinh thần thượng tôn pháp luật - Cổng Thông tin Bộ Công an",false)]
 [InlineData("Công an xã Mai Châu kịp thời tìm và trao trả tài sản cho du khách",false)]
 [InlineData("Cụm thi đua số 2 tổ chức Chương trình dân vận, thiện nguyện tại xã Đà Bắc",false)]
 [InlineData("Công an Khánh Hòa ra mắt sản phẩm công nghệ hưởng ứng ngày chuyển đổi số Quốc gia",false)]
 [InlineData("UBND tỉnh ban hành Kế hoạch chuyển đổi số năm 2026",true)]
 [InlineData("Bộ Tài chính phê duyệt Đề án chuyển đổi số",true)]
 [InlineData("UBND tỉnh công bố Kiến trúc chính quyền điện tử",true)]
 [InlineData("UBND tỉnh lấy ý kiến dự thảo ban hành Kế hoạch chuyển đổi số",true)]
 [InlineData("Hội nghị về chương trình chuyển đổi số",true)]
 public void CollectionPrefilterRejectsNoisyTitlesWithoutCallingAi(string title,bool expected){Assert.Equal(expected,DigitalPlanMonitoringDefaults.Keywords().Where(x=>x.Enabled).Any(keyword=>DigitalNewsCollectionFilter.Matches(title,keyword)));}
 [Fact] public void CollectionFilterHonorsItsOwnPhraseAndAnyCondition(){
  var keyword=new DigitalPlanKeyword{Phrase="đề án chuyển đổi số",Conditions="ban hành, phê duyệt"};
  Assert.True(DigitalNewsCollectionFilter.Matches("Bộ ban hành ĐỀ ÁN CHUYỂN ĐỔI SỐ",keyword));
  Assert.True(DigitalNewsCollectionFilter.Matches("Bộ tổ chức hội nghị về đề án chuyển đổi số",keyword));
  Assert.True(DigitalNewsCollectionFilter.Matches("Bộ phê duyệt kế hoạch chuyển đổi số",keyword));
 }
 [Fact] public void CollectionKeepsTopicEvenWhenTheTitleOmitsIssuanceAction(){
  var keyword=new DigitalPlanKeyword{Phrase="ban hành kế hoạch chuyển đổi số",Conditions="ban hành, phê duyệt"};
  Assert.True(DigitalNewsCollectionFilter.Matches("Kế hoạch thực hiện chuyển đổi số của tỉnh năm 2026",keyword));
  Assert.False(DigitalNewsCollectionFilter.Matches("Công an trả lại tài sản cho người dân",keyword));
  var query=Uri.UnescapeDataString(DigitalPlanDiscoveryReader.QueryUrl(new(){Domain="hanoi.gov.vn"},keyword,new DateOnly(2026,11,1)).Query);
  Assert.DoesNotContain("intitle:",query);Assert.DoesNotContain("ban hành",query);
 }
 [PostgresFact]
 public async Task OldNotRelevantNewsIsHiddenWithoutLosingItsUrlHash(){
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
  var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="news_retention_"+Guid.NewGuid().ToString("N");
  await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
  try{
   var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
   await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
   var now=DateTime.UtcNow;
   DigitalNewsItem item(string suffix,int ageDays)=>new(){AgencyName="Hà Nội",Title="Tin "+suffix,Url="https://example.com/"+suffix,UrlHash=DigitalPlanText.Hash("https://example.com/"+suffix),ImportedAt=now.AddDays(-ageDays)};
   var oldRejected=item("old-rejected",4);var recentRejected=item("recent-rejected",2);var oldRelated=item("old-related",4);var oldMixed=item("old-mixed",4);var oldUnread=item("old-unread",4);
   db.AddRange(oldRejected,recentRejected,oldRelated,oldMixed,oldUnread);
   db.AddRange(new DigitalPlanRead{ItemId=oldRejected.Id,Status="NotRelevant"},new DigitalPlanRead{ItemId=recentRejected.Id,Status="NotRelevant"},new DigitalPlanRead{ItemId=oldRelated.Id,Status="Related"},new DigitalPlanRead{ItemId=oldMixed.Id,Status="NotRelevant"},new DigitalPlanRead{ItemId=oldMixed.Id,Status="NeedsReview",CriteriaHash="other"});
   await db.SaveChangesAsync();
   var visible=await DigitalNewsRetention.Visible(db.DigitalNewsItems.AsNoTracking(),db,now).Select(x=>x.Id).ToListAsync();
   Assert.DoesNotContain(oldRejected.Id,visible);Assert.Contains(recentRejected.Id,visible);Assert.Contains(oldRelated.Id,visible);Assert.Contains(oldMixed.Id,visible);Assert.Contains(oldUnread.Id,visible);
   Assert.Equal(5,await db.DigitalNewsItems.CountAsync());Assert.Equal(oldRejected.UrlHash,(await db.DigitalNewsItems.SingleAsync(x=>x.Id==oldRejected.Id)).UrlHash);
  }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
 }
 [PostgresTheory]
 [InlineData("")]
 [InlineData("khung kiến trúc dùng riêng")]
 public async Task ArchitectureSeedOnlyFillsMissingFieldsAndDoesNotDuplicateCustomKeywords(string architectureKeywords){
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="arch_"+Guid.NewGuid().ToString("N");
  await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
  try{
   var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;await using var db=new AppDbContext(options);
   var previous=db.Database.GetMigrations().Single(x=>x.EndsWith("_DigitalPlanEvaluationCriteria"));await db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().MigrateAsync(previous);
   var custom=System.Text.Json.JsonSerializer.Serialize(new {RelatedKeywords="từ khóa riêng",ActionKeywords="thông qua",ExcludedKeywords="cụm loại trừ riêng",Annual=false,FiveYear=false,Architecture=false,ArchitectureKeywords=architectureKeywords});
   await db.DigitalNewsSyncSettings.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.EvaluationCriteriaJson,custom));
   var keyword=new DigitalPlanKeyword{Name="Tự thêm",Phrase="khung kiến trúc chính phủ số",Enabled=false};db.Add(keyword);await db.SaveChangesAsync();await db.Database.MigrateAsync();
   var stored=(await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync()).EvaluationCriteriaJson;Assert.DoesNotContain("ArchitectureKeywords",stored);Assert.DoesNotContain("Annual",stored);var criteria=DigitalPlanCriteria.Parse(stored);Assert.Contains("từ khóa riêng",criteria.RelatedKeywords);Assert.Contains("thông qua",criteria.ActionKeywords);Assert.Contains("công bố",criteria.ActionKeywords);Assert.Equal("cụm loại trừ riêng",criteria.ExcludedKeywords);
   Assert.Contains(architectureKeywords.Length==0?"khung kiến trúc số":architectureKeywords,criteria.RelatedKeywords);
   Assert.Equal(1,await db.DigitalPlanKeywords.CountAsync(x=>x.Phrase==keyword.Phrase));Assert.False((await db.DigitalPlanKeywords.AsNoTracking().SingleAsync(x=>x.Id==keyword.Id)).Enabled);
  }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
 }
 [PostgresTheory]
 [InlineData("Related")]
 [InlineData("NeedsReview")]
 public async Task NativeCollectionAndManualReadsPreserveLegacyDataAndNotifyOnce(string expectedStatus){
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="plans_"+Guid.NewGuid().ToString("N");
  await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
  try{
   var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
   await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
   Assert.Equal(85,await db.DigitalPlanSources.CountAsync());Assert.Equal(33,await db.DigitalPlanKeywords.CountAsync());
   var legacy=new DigitalNewsItem{AgencyName="Hà Nội",Title="Bài cũ",Url="https://hanoi.gov.vn/old",UrlHash=DigitalPlanText.Hash("https://hanoi.gov.vn/old")};db.Add(legacy);var user=new User{Username="plan-fixture",FullName="Fixture",PasswordHash="unused"};db.Add(user);await db.SaveChangesAsync();
   var reader=new FixtureDiscovery();var sync=new DigitalNewsSyncService(db,reader);
   await sync.SaveAsync(true,30,365,20,"Test",default);
   Assert.Equal(20,(await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync()).BatchSize);
   await Assert.ThrowsAsync<InvalidOperationException>(()=>sync.SaveAsync(true,30,365,21,"Test",default));
   await sync.SaveAsync(true,30,365,2,"Test",default);
   var result=await sync.RunAsync(true,default);Assert.Equal(1,result!.Added);Assert.Equal(2,result.Filtered);Assert.Equal(2,reader.Calls);Assert.Equal(2,reader.Sources.Count);Assert.Equal(2,await db.DigitalNewsItems.CountAsync());Assert.Equal(0,await db.DigitalPlanReads.CountAsync());Assert.Equal(0,await db.AiInboxItems.CountAsync());
   Assert.Null(await sync.RunAsync(false,default));Assert.Equal(0,(await sync.RunAsync(true,default))!.Added);Assert.Equal(4,(await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync()).QueryCursor);
   var listed=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await new Cdsqg.Api.Controllers.DigitalNewsController(db,sync).List(pageSize:1));
   var listedJson=System.Text.Json.JsonSerializer.SerializeToElement(listed.Value);Assert.Equal(1,listedJson.GetProperty("total").GetInt32());Assert.Equal(2,await db.DigitalNewsItems.CountAsync());
   var item=await db.DigitalNewsItems.SingleAsync(x=>x.Id!=legacy.Id);var classifier=new FixtureClassifier{Status=expectedStatus};var service=new DigitalPlanReadService(db,classifier);
   var queued=await service.QueueAsync(item.Id,user.Id,default);Assert.Equal("Queued",queued!.Status);Assert.Equal(queued.Id,(await service.QueueAsync(item.Id,user.Id,default))!.Id);
   Assert.True(await service.ProcessNextAsync(default));db.ChangeTracker.Clear();var saved=await db.DigitalPlanReads.SingleAsync();Assert.Equal(expectedStatus,saved.Status);Assert.Equal(1,classifier.Calls);Assert.Equal(1,await db.Notifications.CountAsync(x=>x.UserId==user.Id));
   Assert.Equal(expectedStatus,(await service.QueueAsync(item.Id,user.Id,default))!.Status);Assert.False(await service.ProcessNextAsync(default));Assert.Equal(1,await db.Notifications.CountAsync());Assert.Contains("&result="+saved.Id,(await db.Notifications.SingleAsync()).LinkUrl);if(expectedStatus=="NeedsReview")Assert.StartsWith("Cần xem nguồn:",(await db.Notifications.SingleAsync()).Title);
   var updatedCriteria=new DigitalPlanCriteria{RelatedKeywords="chính quyền số"};
   await db.DigitalNewsSyncSettings.ExecuteUpdateAsync(s=>s.SetProperty(x=>x.EvaluationCriteriaJson,updatedCriteria.Json()));
   var newRead=await service.QueueAsync(item.Id,user.Id,default);Assert.NotEqual(saved.Id,newRead!.Id);Assert.Equal(updatedCriteria.Hash(),newRead.CriteriaHash);Assert.Equal(updatedCriteria.Json(),newRead.CriteriaJson);
   var controller=new Cdsqg.Api.Controllers.DigitalNewsController(db,sync);
   var stillVisible=Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.List());Assert.Equal(1,System.Text.Json.JsonSerializer.SerializeToElement(stillVisible.Value).GetProperty("total").GetInt32());
   Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.Assessment(item.Id,default,result:saved.Id));
   Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await controller.Assessment(item.Id,default,notification:saved.NotificationId));
   Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(await controller.Assessment(legacy.Id,default,result:saved.Id));
   var current=await db.DigitalNewsItems.FindAsync(item.Id);current!.Title+=" (mới)";await db.SaveChangesAsync();Assert.NotEqual(saved.Id,(await service.QueueAsync(item.Id,user.Id,default))!.Id);
   reader.Fail=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>sync.RunAsync(true,default));Assert.Equal(2,await db.DigitalNewsItems.CountAsync());Assert.Equal(0,await db.ProgressLogs.CountAsync());
  }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
 }
 private sealed class FixtureDiscovery:IDigitalPlanDiscoveryReader{public int Calls;public HashSet<Guid> Sources=[];public bool Fail;public Task<IReadOnlyList<DigitalPlanFound>> ReadAsync(DigitalPlanSource source,DigitalPlanKeyword keyword,DateOnly from,DateOnly? to,CancellationToken ct){Calls++;Sources.Add(source.Id);if(Fail)throw new InvalidOperationException();return Task.FromResult<IReadOnlyList<DigitalPlanFound>>([new("Bộ Tài chính ban hành Kế hoạch chuyển đổi số năm 2026","https://mof.gov.vn/new",DateTime.UtcNow,true),new("Công an tìm và trả lại tài sản cho du khách","https://mof.gov.vn/noise",DateTime.UtcNow,true)]);}}
 private sealed class FixtureClassifier:IDigitalPlanTitleClassifier{public string Status="Related";public int Calls;public Task<DigitalPlanVerdict> ClassifyAsync(string title,string agency,CancellationToken ct,DigitalPlanCriteria? criteria=null){Calls++;return Task.FromResult(new DigitalPlanVerdict(Status,"Annual",2026,2026,"Có tin cần đối chiếu nguồn.",title,"fixture"));}}
}
