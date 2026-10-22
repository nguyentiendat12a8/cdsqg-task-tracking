using Cdsqg.Api.Controllers;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using Xunit;
namespace Cdsqg.Tests;
public class DashboardTimeFilterTests
{
    [Fact]
    public void ValidationAndExecutionOverlapIncludeBoundariesAndOngoingItems()
    {
        Assert.NotNull(new DashboardTimeFilter(2028,2026,null,null).ValidationError());
        Assert.NotNull(new DashboardTimeFilter(1899,null,null,null).ValidationError());
        Assert.NotNull(new DashboardTimeFilter(null,null,new(2027,1,1),new(2026,1,1)).ValidationError());
        var items=new[]{
            new GoalTaskItem {Code="inside",DueDate=new(2026,6,15)},
            new GoalTaskItem {Code="start_boundary",DueDate=new(2026,1,1)},
            new GoalTaskItem {Code="end_boundary",DueDate=new(2026,12,31,23,59,59)},
            new GoalTaskItem {Code="before",DueDate=new(2025,12,31,23,59,59)},
            new GoalTaskItem {Code="after",DueDate=new(2027,1,1)},
            new GoalTaskItem {Code="no_due",DueDate=null}
        };
        var filter=new DashboardTimeFilter(null,null,new(2026,1,1),new(2026,12,31));
        Assert.Equal(new[]{"inside","start_boundary","end_boundary"},filter.ApplyExecutionWindow(items.AsQueryable()).Select(i=>i.Code));
    }

    [Fact]
    public void EmptyReportWindowDoesNotUseCachedCompletedProducts()
    {
        var product=new TaskDeliverable {Id="p",Title="Product",CurrentStatus="Completed"};
        var item=new GoalTaskItem {Deliverables=new(){product},ProgressLogs=new List<ProgressLog> {new(){PeriodYear=2027,ApprovalStatus=ApprovalStatusEnum.Approved}}};
        new DashboardTimeFilter(2025,2026,null,null).RestrictReports(new[]{item});
        Assert.Empty(item.ProgressLogs);
        Assert.Equal("NotStarted",Assert.Single(item.Deliverables).CurrentStatus);
        Assert.Equal("Completed",product.CurrentStatus);
    }

    [PostgresFact]
    public async Task ReportRangeUsesLatestApprovedTotalAndAllViewsShareExecutionWindow()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="dash_time_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();
        await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
        try
        {
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
            var agency=new Agency {Code="TIME",Name="Time ministry",Type=AgencyTypeEnum.Ministry,IsActive=true};
            var doc=new Document {DocumentNumber="TIME",StartYear=2025,EndYear=2030};
            var otherDoc=new Document {DocumentNumber="OTHER",StartYear=2025,EndYear=2030};
            GoalTaskItem Item(string code,DateTime start,DateTime due,bool ongoing=false)=>new(){Code=code,Title=code,DocumentId=doc.Id,LeadAgencyId=agency.Id,ItemType=ItemTypeEnum.Task,EvaluationType=EvaluationTypeEnum.Quantitative,StartDate=start,DueDate=due,IsOngoing=ongoing};
            var span=Item("SPAN",new(2025,1,1),new(2026,12,31));
            var boundary=Item("BOUNDARY",new(2025,1,1),new(2026,1,1));
            var late=Item("LATE",new(2028,1,1),new(2030,12,31));
            var ongoing=Item("ONGOING",new(2025,1,1),new(2026,6,30),true);
            var other=Item("OTHER",new(2025,1,1),new(2026,12,31));other.DocumentId=otherDoc.Id;
            db.AddRange(agency,doc,otherDoc,span,boundary,late,ongoing,other);
            foreach(var (year,value,approval) in new[]{(2025,20m,ApprovalStatusEnum.Approved),(2026,40m,ApprovalStatusEnum.Approved),(2027,100m,ApprovalStatusEnum.Approved),(2026,99m,ApprovalStatusEnum.Pending)})
                db.Add(new ProgressLog {GoalTaskId=span.Id,AgencyId=agency.Id,PeriodYear=year,QuantitativeValue=value,LogDate=new(year,10,1),ApprovalStatus=approval});
            await db.SaveChangesAsync();await AssignmentTestData.PrepareAsync(db);
            var controller=new DashboardController(db);
            var optionsJson=new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var response=Assert.IsType<OkObjectResult>(await controller.GetAgencyItems(agency.Id,documentId:doc.Id,reportFromYear:2025,reportToYear:2026,executionFrom:new(2026,1,1),executionTo:new(2026,12,31)));
            var rows=JsonSerializer.SerializeToElement(response.Value,optionsJson).EnumerateArray().ToArray();
            Assert.Equal(3,rows.Length);Assert.DoesNotContain(rows,r=>r.GetProperty("code").GetString() is "LATE" or "OTHER");
            Assert.Equal(40m,Assert.Single(rows,r=>r.GetProperty("code").GetString()=="SPAN").GetProperty("latestProgressValue").GetDecimal());
            var metrics=Assert.IsType<OkObjectResult>(await controller.GetDashboardMetrics(documentId:doc.Id,reportFromYear:2025,reportToYear:2026,executionFrom:new(2026,1,1),executionTo:new(2026,12,31)));
            var summary=JsonSerializer.SerializeToElement(metrics.Value,optionsJson);Assert.Equal(3,summary.GetProperty("totalTasks").GetInt32());Assert.Equal(0,summary.GetProperty("completedTasks").GetInt32());
            var onlyOngoing=Assert.IsType<OkObjectResult>(await controller.GetAgencyItems(agency.Id,documentId:doc.Id,isOngoing:true,executionFrom:new(2026,1,1),executionTo:new(2026,12,31)));
            Assert.Single(JsonSerializer.SerializeToElement(onlyOngoing.Value,optionsJson).EnumerateArray());
            var before=Assert.IsType<OkObjectResult>(await controller.GetAgencyItems(agency.Id,documentId:doc.Id,reportToYear:2024));
            Assert.Equal(JsonValueKind.Null,Assert.Single(JsonSerializer.SerializeToElement(before.Value,optionsJson).EnumerateArray(),r=>r.GetProperty("code").GetString()=="SPAN").GetProperty("latestProgressValue").ValueKind);
            Assert.IsType<BadRequestObjectResult>(await controller.GetDashboardMetrics(reportFromYear:2028,reportToYear:2026));
            Assert.IsType<BadRequestObjectResult>(await controller.GetAgencyItems(agency.Id,executionFrom:new(2027,1,1),executionTo:new(2026,1,1)));
            Assert.Equal(4,await db.ProgressLogs.CountAsync());
        }
        finally {await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
    }
}