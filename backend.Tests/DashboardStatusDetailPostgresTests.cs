using Cdsqg.Api.Controllers;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;
public class DashboardStatusDetailPostgresTests
{
    [PostgresFact]
    public async Task DrilldownMatchesCountersAndKeepsSeventeenGroupRecipientsSeparate()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema="details_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection); await admin.OpenAsync();
        await using(var command=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await command.ExecuteNonQueryAsync();
        try
        {
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options); await db.Database.MigrateAsync();
            var ministries=Enumerable.Range(1,17).Select(n=>new Agency {Code=$"M{n:D2}",Name=$"Bộ {n:D2}",Type=AgencyTypeEnum.Ministry}).ToList();
            var group=new Agency {Code="ALL_MINISTRIES_DIRECT",Name="Các bộ, ngành",Type=AgencyTypeEnum.Special};
            var doc=new Document {DocumentNumber="DETAIL_TEST",StartYear=2026,EndYear=2030};
            var shared=new GoalTaskItem {Group="Phát triển nhân lực",Code="MT-1",Title="Mục tiêu giao chung",DocumentId=doc.Id,LeadAgencyId=group.Id,
                ItemType=ItemTypeEnum.Goal,IsGeneralTask=true,DueDate=new(2030,12,31)};
            var specific=new GoalTaskItem {Code="MT-2",Title="Mục tiêu giao riêng",DocumentId=doc.Id,LeadAgencyId=ministries[0].Id,
                ItemType=ItemTypeEnum.Goal,DueDate=new(2030,12,31)};
            db.AddRange(ministries); db.AddRange(group,doc,shared,specific); await db.SaveChangesAsync();
            await AgencyAssignments.SyncAsync(db,shared); await AgencyAssignments.SyncAsync(db,specific); await db.SaveChangesAsync();
            db.AddRange(new ProgressLog {GoalTaskId=shared.Id,AgencyId=ministries[0].Id,PeriodYear=2026,QuantitativeValue=100,ApprovalStatus=ApprovalStatusEnum.Approved,LogDate=new(2026,10,1)},
                new ProgressLog {GoalTaskId=shared.Id,AgencyId=ministries[1].Id,PeriodYear=2026,QuantitativeValue=20,ApprovalStatus=ApprovalStatusEnum.Approved,LogDate=new(2026,10,1)});
            await db.SaveChangesAsync();
            var controller=new DashboardController(db);
            async Task<PagedResultDto<DashboardAgencyItemDto>> Read(string mode,string? status=null,string scope="specific",int page=1,int size=100)
                => Assert.IsType<PagedResultDto<DashboardAgencyItemDto>>(Assert.IsType<OkObjectResult>(await controller.GetDashboardMetrics(reportYear:2026,itemType:"Goal",detailMode:mode,detailStatus:status,detailScope:scope,pageNumber:page,pageSize:size)).Value);
            var expanded=await Read("assignments");
            Assert.Equal(18,expanded.TotalCount);
            var groupRows=expanded.Items.Where(r=>r.Code=="MT-1").ToList();
            Assert.Equal(17,groupRows.Count); Assert.Equal(17,groupRows.Select(r=>r.ProgressAgencyId).Distinct().Count());
            Assert.All(groupRows,r=>Assert.StartsWith("Bộ ",r.LeadAgencyName));
            Assert.Equal("Completed",groupRows.Single(r=>r.ProgressAgencyId==ministries[0].Id).CalculatedStatus);
            Assert.Equal(20d,groupRows.Single(r=>r.ProgressAgencyId==ministries[1].Id).LatestProgressPercent);
            Assert.Equal(16,(await Read("assignments","NotStarted")).TotalCount);
            Assert.Equal(1,(await Read("items","NotStarted")).TotalCount);
            Assert.Equal(2,(await Read("items",scope:"all")).TotalCount);
            Assert.Equal(1,(await Read("items",scope:"general")).TotalCount);
            var pages=new List<DashboardAgencyItemDto>();
            for(int page=1;page<=4;page++)pages.AddRange((await Read("assignments",page:page,size:5)).Items);
            Assert.Equal(18,pages.Select(r=>(r.Id,r.ProgressAgencyId)).Distinct().Count());
            var metrics=System.Text.Json.JsonSerializer.SerializeToElement(Assert.IsType<OkObjectResult>(await controller.GetDashboardMetrics(reportYear:2026,itemType:"Goal")).Value);
            Assert.Equal((await Read("assignments","NotStarted")).TotalCount,metrics.GetProperty("goalStatusSummary").GetProperty("notStarted").GetInt32());
            Assert.Equal((await Read("items","NotStarted")).TotalCount,metrics.GetProperty("createdMetrics").GetProperty("goalStatusSummary").GetProperty("notStarted").GetInt32());
            var grouped=metrics.GetProperty("groupStatistics").EnumerateArray().ToList();
            var human=grouped.Single(g=>g.GetProperty("group").GetString()=="Phát triển nhân lực");
            Assert.Equal("Goal",human.GetProperty("itemType").GetString());
            Assert.Equal(1,human.GetProperty("total").GetInt32()); // Seventeen recipients still count as one shared item.
            Assert.Equal(1,human.GetProperty("inProgress").GetInt32());
            Assert.Equal(0m,human.GetProperty("completionRate").GetDecimal());
            Assert.Equal(1,grouped.Single(g=>g.GetProperty("group").GetString()=="Chưa phân nhóm").GetProperty("notStarted").GetInt32());
            var scoped=System.Text.Json.JsonSerializer.SerializeToElement(Assert.IsType<OkObjectResult>(await controller.GetDashboardMetrics(documentId:doc.Id,agencyId:new[]{ministries[0].Id},reportYear:2026)).Value);
            Assert.Equal(1,scoped.GetProperty("groupStatistics").EnumerateArray().Single(g=>g.GetProperty("group").GetString()=="Phát triển nhân lực").GetProperty("completed").GetInt32());
            var states = metrics.GetProperty("goalStatusSummary");
            Assert.Equal(new[] { "completed", "inProgress", "notStarted" }, states.EnumerateObject().Select(p => p.Name).OrderBy(n => n));
            Assert.Equal(expanded.TotalCount, states.EnumerateObject().Sum(p => p.Value.GetInt32()));
            foreach (var state in ExecutionStatuses.All)
            {
                var key = char.ToLowerInvariant(state.ToString()[0]) + state.ToString()[1..];
                Assert.Equal((await Read("assignments",state.ToString())).TotalCount, states.GetProperty(key).GetInt32());
            }
            controller.ControllerContext=new ControllerContext { HttpContext=new Microsoft.AspNetCore.Http.DefaultHttpContext() };
            Assert.IsType<ForbidResult>(await controller.GetDashboardMetrics(detailMode:"assignments"));
        }
        finally { await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await command.ExecuteNonQueryAsync(); }
    }
}
