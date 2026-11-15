using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;

public class CoordinatingReportRoundTripPostgresTests
{
    [PostgresFact]
    public async Task CoordinationReportExportImport_RetainsAgencyYearRoleAndLeadResults()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "coord_roundtrip_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection); await admin.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await cmd.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options); await db.Database.MigrateAsync();
            var lead = new Agency { Code="LEAD", Name="Chủ trì", Type=AgencyTypeEnum.Ministry, IsActive=true };
            var coord = new Agency { Code="COORD", Name="Phối hợp", Type=AgencyTypeEnum.Ministry, IsActive=true };
            var other = new Agency { Code="OTHER", Name="Không được giao", Type=AgencyTypeEnum.Ministry, IsActive=true };
            var d1 = new Document { Id=Guid.Parse("26290000-0000-0000-0000-000000002629"),DocumentNumber="2629/QĐ-TTg",Name="2629" };
            var d2 = new Document { Id=Guid.Parse("12660000-0000-0000-0000-000000001266"),DocumentNumber="1266/QĐ-TTg",Name="1266" };
            GoalTaskItem Goal(Document doc) => new() { Code="MT-01", Title="Mục tiêu " + doc.Name, DocumentId=doc.Id, LeadAgencyId=lead.Id,
                ItemType=ItemTypeEnum.Goal, EvaluationType=EvaluationTypeEnum.Quantitative, StartDate=new DateTime(2025,12,1), DueDate=new DateTime(2030,12,31),ResultTarget=100 };
            var g1=Goal(d1);g1.DueDate=new DateTime(2027,12,31);var g2=Goal(d2);
            var task=new GoalTaskItem {Code="NV-01",Title="Nhiệm vụ phối hợp",DocumentId=d1.Id,LeadAgencyId=lead.Id,ItemType=ItemTypeEnum.Task,EvaluationType=EvaluationTypeEnum.Qualitative,DueDate=new DateTime(2030,12,31)};
            task.Deliverables = new() { new() { Id="p1",Title="Sản phẩm chung",CurrentStatus="Completed",DocumentNumber="LEAD-DOC" } };
            db.AddRange(lead,coord,other,d1,d2,g1,g2,task);await db.SaveChangesAsync();
            foreach(var item in new[]{g1,g2,task}) db.AgencyTaskExecutions.AddRange(new AgencyTaskExecution{GoalTaskId=item.Id,AgencyId=lead.Id,IsLeadAssignment=true},new AgencyTaskExecution{GoalTaskId=item.Id,AgencyId=coord.Id,IsCoordinatingAssignment=true});
            db.ProgressLogs.AddRange(new ProgressLog{GoalTaskId=g1.Id,AgencyId=lead.Id,PeriodYear=2026,QuantitativeValue=70,ApprovalStatus=ApprovalStatusEnum.Approved},new ProgressLog{GoalTaskId=task.Id,AgencyId=lead.Id,PeriodYear=2026,QualitativeStatus=TextStatusEnum.Completed,ApprovalStatus=ApprovalStatusEnum.Approved});
            await db.SaveChangesAsync();db.ChangeTracker.Clear();
            var planning=new PlanningService(db);var execution=new ExecutionService(db,null!);
            var before=await planning.GetAgencyProgressReportAsync(coord.Id,2026,null,"Admin",fetchAll:true);
            Assert.Equal(2,before.Goals.Count);Assert.Single(before.Tasks);Assert.All(before.Goals,g=>{Assert.True(g.IsCoordinatingAssignment);Assert.Null(g.Estimate2026);Assert.DoesNotContain(2027,g.ApplicableYears);});
            Assert.Equal("Chưa thực hiện",before.Tasks[0].StatusLabel);
            var imported=await execution.ImportProgressBulkAsync(new(){UserRole="Admin",ReportingAgencyId=coord.Id,CreatedBy="Admin test",Items=new(){
                new(){Id=g1.Id,Code="MT-01",Value=10,PeriodYear=2025,SummaryNotes="Phối hợp 2025"},new(){Id=g1.Id,Code="MT-01",Value=45,PeriodYear=2026,SummaryNotes="Phối hợp 2629"},
                new(){Id=g2.Id,Code="MT-01",Value=55,PeriodYear=2026,SummaryNotes="Phối hợp 1266"},new(){Id=task.Id,Code="NV-01",Status=TextStatusEnum.Drafting,PeriodYear=2026,SummaryNotes="Ghi chú phối hợp"}}});
            Assert.Equal(4,imported.SuccessCount);Assert.Equal(0,imported.FailureCount);db.ChangeTracker.Clear();
            var after=await planning.GetAgencyProgressReportAsync(coord.Id,2026,null,"Admin",fetchAll:true);
            Assert.Equal(10,after.Goals.Single(g=>g.Id==g1.Id).Actual2025);Assert.Equal(45,after.Goals.Single(g=>g.Id==g1.Id).Estimate2026);Assert.Equal(55,after.Goals.Single(g=>g.Id==g2.Id).Estimate2026);
            Assert.Equal("Đang thực hiện",after.Tasks.Single().StatusLabel);Assert.Equal("Ghi chú phối hợp",after.Tasks.Single().Notes);
            Assert.Equal(4,await db.ProgressLogs.CountAsync(p=>p.AgencyId==coord.Id&&p.IsCoordinatingReport&&p.ApprovalStatus==ApprovalStatusEnum.Approved));
            var coordTaskLog=await db.ProgressLogs.SingleAsync(p=>p.GoalTaskId==task.Id&&p.AgencyId==coord.Id&&p.IsCoordinatingReport);
            Assert.Null(Assert.Single(coordTaskLog.Deliverables!).DocumentNumber);
            Assert.Equal("LEAD-DOC",Assert.Single((await db.GoalTaskItems.SingleAsync(t=>t.Id==task.Id)).Deliverables).DocumentNumber);
            coordTaskLog.Deliverables![0].DocumentNumber="COORD-DOC";await db.SaveChangesAsync();
            var next=await execution.ImportProgressBulkAsync(new(){UserRole="Admin",ReportingAgencyId=coord.Id,Items=new(){new(){Id=task.Id,Code=task.Code,Status=TextStatusEnum.Completed,PeriodYear=2026}}});
            Assert.Equal(1,next.SuccessCount);db.ChangeTracker.Clear();
            var ownEvidence=await db.ProgressLogs.Where(p=>p.GoalTaskId==task.Id&&p.AgencyId==coord.Id&&p.IsCoordinatingReport).OrderByDescending(p=>p.LogDate).FirstAsync();
            Assert.Equal("COORD-DOC",Assert.Single(ownEvidence.Deliverables!).DocumentNumber);

            var leadReport=await planning.GetAgencyProgressReportAsync(lead.Id,2026,null,"Admin",fetchAll:true);
            Assert.Equal(70,leadReport.Goals.Single(g=>g.Id==g1.Id).Estimate2026);Assert.Equal("Đã hoàn thành",leadReport.Tasks.Single().StatusLabel);
            var rejected=await execution.ImportProgressBulkAsync(new(){UserRole="Admin",ReportingAgencyId=other.Id,Items=new(){new(){Id=g1.Id,Code="MT-01",Value=99,PeriodYear=2026}}});
            Assert.Equal(1,rejected.FailureCount);Assert.False(await db.ProgressLogs.AnyAsync(p=>p.AgencyId==other.Id));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>planning.ImportYearlyTargetsAsync(d1.Id,new(){ReportingAgencyId=coord.Id,ConfirmOverwrite=true,Rows=new(){new(){TaskId=g1.Id,Year=2027,TargetQuantity=90}}},null,true));
            var pending=await execution.ImportProgressBulkAsync(new(){UserRole="AgencyUser",UserAgencyId=coord.Id,ReportingAgencyId=coord.Id,Items=new(){new(){Id=g1.Id,Code="MT-01",Value=60,PeriodYear=2026}}});
            Assert.Equal(1,pending.PendingCount);db.ChangeTracker.Clear();
            var final=await planning.GetAgencyProgressReportAsync(coord.Id,2026,null,"Admin",fetchAll:true);Assert.Equal(45,final.Goals.Single(g=>g.Id==g1.Id).Estimate2026);
        }
        finally { await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync(); }
    }
}
