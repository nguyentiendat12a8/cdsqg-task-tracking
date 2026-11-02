using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class Seed2629GoalSqlRepairTests {
 [PostgresFact] public async Task PreviewApplyAndRepeatPreserveReportsPlansAndOtherDocuments(){
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="sqlrepair_"+Guid.NewGuid().ToString("N");
  await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
  try{
   var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
   var all=new Agency{Code="ALL_AGENCIES",Name="Các bộ, ngành, địa phương",Type=AgencyTypeEnum.Special};
   var vpcp=new Agency{Code="VPCP",Name="Văn phòng Chính phủ"};var bca=new Agency{Code="BCA",Name="Bộ Công an"};var bnv=new Agency{Code="BNV",Name="Bộ Nội vụ"};var bkhcn=new Agency{Code="BKHCN",Name="Bộ Khoa học và Công nghệ"};
   var doc=new Document{DocumentNumber="2629/QĐ-TTg"};var otherDoc=new Document{DocumentNumber="1266/QĐ-TTg"};db.AddRange(all,vpcp,bca,bnv,bkhcn,doc,otherDoc);await db.SaveChangesAsync();
   var bcaCodes=new[]{8,10,13,16,17,18,21,22};var vpCodes=new[]{1,2,5,6,7,14,15};var nvCodes=new[]{9,20};
   var items=Enumerable.Range(1,22).Select(i=>new GoalTaskItem{DocumentId=doc.Id,Code=$"MT-{i:00}",ItemType=ItemTypeEnum.Goal,Title=$"Mục tiêu {i}",LeadAgencyId=bcaCodes.Contains(i)?bca.Id:all.Id,LeadAgencyDisplayText=bcaCodes.Contains(i)?bca.Name:vpCodes.Contains(i)?vpcp.Name:nvCodes.Contains(i)?bnv.Name:bkhcn.Name,CustomBaseline=new(){["2026"]="66%"}}).ToArray();
   items[19].LeadAgencyId=bca.Id; // A deliberate manual reassignment must not be replaced.
   var other=new GoalTaskItem{DocumentId=otherDoc.Id,Code="MT-09",ItemType=ItemTypeEnum.Goal,LeadAgencyId=all.Id,LeadAgencyDisplayText=bnv.Name};
   var task=new GoalTaskItem{DocumentId=doc.Id,Code="NV-01",LeadAgencyId=all.Id,LeadAgencyDisplayText=bnv.Name};db.AddRange(items);db.AddRange(other,task);await db.SaveChangesAsync();await AgencyAssignments.InitializeMissingAsync(db);
   var mt09=items[8];var wrongRow=await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==mt09.Id&&e.AgencyId==bca.Id);wrongRow.LatestProgressValue=55;wrongRow.SummaryNotes="Keep historical cache";wrongRow.IsCoordinatingAssignment=true;wrongRow.IsCoordinatingGroupAssignment=true;
   var log=new ProgressLog{GoalTaskId=mt09.Id,AgencyId=bca.Id,PeriodYear=2026,ApprovalStatus=ApprovalStatusEnum.Approved,SummaryNotes="Preserve approved report"};db.Add(log);await db.SaveChangesAsync();
   var sql=await File.ReadAllTextAsync(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../scripts/repair_2629_goal_lead_assignments.sql")));
   var count=await db.AgencyTaskExecutions.CountAsync();await db.Database.ExecuteSqlRawAsync(sql);db.ChangeTracker.Clear();Assert.True((await db.GoalTaskItems.FindAsync(mt09.Id))!.IsGeneralTask);
   var apply=sql.Replace("INSERT INTO _r2629_options VALUES (false);","INSERT INTO _r2629_options VALUES (true);");await db.Database.ExecuteSqlRawAsync(apply);db.ChangeTracker.Clear();
   var fixedItem=await db.GoalTaskItems.FindAsync(mt09.Id);Assert.Equal(bnv.Id,fixedItem!.LeadAgencyId);Assert.False(fixedItem.IsGeneralTask);Assert.Equal(bca.Id,(await db.GoalTaskItems.FindAsync(items[19].Id))!.LeadAgencyId);Assert.Equal("66%",fixedItem.CustomBaseline!["2026"]);
   var oldRow=await db.AgencyTaskExecutions.FindAsync(wrongRow.Id);Assert.False(oldRow!.IsLeadAssignment);Assert.False(oldRow.IsLeadGroupAssignment);Assert.True(oldRow.IsCoordinatingAssignment);Assert.True(oldRow.IsCoordinatingGroupAssignment);Assert.Equal(55,oldRow.LatestProgressValue);Assert.Equal("Keep historical cache",oldRow.SummaryNotes);
   Assert.Equal("Preserve approved report",(await db.ProgressLogs.FindAsync(log.Id))!.SummaryNotes);Assert.Equal(ApprovalStatusEnum.Approved,(await db.ProgressLogs.FindAsync(log.Id))!.ApprovalStatus);
   Assert.Equal(count,await db.AgencyTaskExecutions.CountAsync());Assert.True((await db.GoalTaskItems.FindAsync(other.Id))!.IsGeneralTask);Assert.True((await db.GoalTaskItems.FindAsync(task.Id))!.IsGeneralTask);
   Assert.Equal(22,await db.AgencyTaskExecutions.CountAsync(e=>items.Select(i=>i.Id).Contains(e.GoalTaskId)&&e.IsLeadAssignment));
   await db.Database.ExecuteSqlRawAsync(apply);Assert.Equal(count,await db.AgencyTaskExecutions.CountAsync());
   db.Add(new Agency{Code="BNV_DUPLICATE",Name=bnv.Name});await db.SaveChangesAsync();
   var error=await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlRawAsync(apply));Assert.Contains("ambiguous",error.MessageText);await db.Database.ExecuteSqlRawAsync("ROLLBACK;");
   Assert.Equal(count,await db.AgencyTaskExecutions.CountAsync());Assert.Equal("Preserve approved report",(await db.ProgressLogs.AsNoTracking().SingleAsync(x=>x.Id==log.Id)).SummaryNotes);

  }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
 }
}
