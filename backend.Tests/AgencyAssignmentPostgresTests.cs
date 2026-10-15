using System.Data.Common;
using System.Text.Json;
using Cdsqg.Api.Controllers;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;

public class AgencyAssignmentPostgresTests
{
    private sealed class Trace : DbCommandInterceptor
    {
        public readonly List<string> Sql = new();
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken token = default)
        { Sql.Add(command.CommandText); return new(result); }
    }
    private static JsonElement Json(object value) => JsonSerializer.SerializeToElement(value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    [Fact]
    public async Task RefreshAssignmentsRejectsAnonymousAndNonAdminBeforeReadingDatabase()
    {
        var controller = new GoalTaskItemController(null!);
        controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
        Assert.IsType<UnauthorizedResult>(await controller.RefreshAssignments(Guid.NewGuid()));
        controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role,"AgencyUser") }, "test"));
        Assert.IsType<ForbidResult>(await controller.RefreshAssignments(Guid.NewGuid()));
        var attribute = (Microsoft.AspNetCore.Authorization.AuthorizeAttribute)Attribute.GetCustomAttribute(typeof(GoalTaskItemController).GetMethod("RefreshAssignments")!, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))!;
        Assert.Equal("Admin", attribute.Roles);
    }

    [PostgresFact]
    public async Task BackfillAndAssignmentRolesKeepAgencyProgressIsolatedAcrossAllViews()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "assign_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection); await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var trace = new Trace();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).AddInterceptors(trace).Options;
            await using var db = new AppDbContext(options);
            await db.GetService<IMigrator>().MigrateAsync("20261008164528_AnnualReportingOnly");
            var bca = new Agency { Code="BCA", Name="Bộ Công an", Type=AgencyTypeEnum.Ministry };
            var btc = new Agency { Code="BTC", Name="Bộ Tài chính", Type=AgencyTypeEnum.Ministry };
            var province = new Agency { Code="HN", Name="Hà Nội", Type=AgencyTypeEnum.Province };
            var inactive = new Agency { Code="OLD", Name="Inactive", Type=AgencyTypeEnum.Ministry, IsActive=false };
            var all = new Agency { Code="ALL_AGENCIES", Type=AgencyTypeEnum.Special };
            var ministries = new Agency { Code="ALL_MINISTRIES", Type=AgencyTypeEnum.Special };
            var provinces = new Agency { Code="ALL_PROVINCES", Type=AgencyTypeEnum.Special };
            var doc = new Document { DocumentNumber="ASSIGN", StartYear=2026, EndYear=2030 };
            GoalTaskItem Goal(string code, Guid lead, params Guid[] coord) => new() { Code=code, Title=code, DocumentId=doc.Id, LeadAgencyId=lead, IsGeneralTask=lead==all.Id || lead==ministries.Id || lead==provinces.Id, ItemType=ItemTypeEnum.Goal, EvaluationType=EvaluationTypeEnum.Quantitative, DueDate=new(2030,12,31), CustomBaseline=new() { ["2026"]="75", ["2027"]="150" }, CoordinatingAgencyIds=coord.ToList() };
            var direct=Goal("DIRECT",bca.Id); var general=Goal("GENERAL",all.Id); var ministry=Goal("MINISTRY",ministries.Id);
            var coordination=Goal("COORD",btc.Id,ministries.Id); var both=Goal("BOTH",bca.Id,ministries.Id); var provinceOnly=Goal("PROVINCE",provinces.Id);
            db.AddRange(bca,btc,province,inactive,all,ministries,provinces,doc,direct,general,ministry,coordination,both,provinceOnly);
            db.AddRange(new ProgressLog { GoalTaskId=direct.Id, AgencyId=bca.Id, PeriodYear=2026, QuantitativeValue=50, ApprovalStatus=ApprovalStatusEnum.Approved, LogDate=new(2026,10,1) },
                new ProgressLog { GoalTaskId=general.Id, AgencyId=btc.Id, PeriodYear=2026, QuantitativeValue=150, ApprovalStatus=ApprovalStatusEnum.Approved },
                new ProgressLog { GoalTaskId=coordination.Id, AgencyId=bca.Id, PeriodYear=2026, QuantitativeValue=75, ApprovalStatus=ApprovalStatusEnum.Approved });
            await db.SaveChangesAsync();
            var originalLogIds=await db.ProgressLogs.Select(l=>l.Id).ToListAsync();
            var legacyId=Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"AgencyTaskExecutions\" (\"Id\",\"GoalTaskId\",\"AgencyId\",\"CalculatedStatus\",\"ApprovalStatus\",\"CompletionPercentage\",\"Deliverables\",\"AttachmentFileUrls\",\"CreatedAt\",\"UpdatedAt\") VALUES ({legacyId},{direct.Id},{bca.Id},'CompletedOnTime',3,999,'[]'::jsonb,'[]'::jsonb,CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)");
            await db.Database.MigrateAsync(); db.ChangeTracker.Clear();
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Equal(originalLogIds.Order(), (await db.ProgressLogs.Select(l=>l.Id).ToListAsync()).Order());
            var legacy=await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==direct.Id && e.AgencyId==bca.Id);
            Assert.Equal(legacyId,legacy.Id); Assert.Equal(ApprovalStatusEnum.Rejected,legacy.ApprovalStatus); Assert.Equal(999m,legacy.CompletionPercentage);
            Assert.True(legacy.IsLeadAssignment);
            Assert.Equal(3,await db.AgencyTaskExecutions.CountAsync(e=>e.GoalTaskId==general.Id && e.IsLeadAssignment));
            Assert.False(await db.AgencyTaskExecutions.AnyAsync(e=>e.AgencyId==inactive.Id));
            var dual=await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==both.Id && e.AgencyId==bca.Id);
            Assert.True(dual.IsLeadAssignment && dual.IsCoordinatingAssignment);
            Assert.False(dual.IsLeadGroupAssignment); Assert.True(dual.IsCoordinatingGroupAssignment);
            await db.Database.MigrateAsync();
            var planning=new PlanningService(db); var dashboard=new DashboardController(db);
            trace.Sql.Clear();
            var lead=await planning.GetDocumentItemsPagedAsync(doc.Id,new DocumentItemsQueryDto { UserRole="Admin", SelectedAgencyIds=new(){bca.Id}, PageSize=100 });
            Assert.Equal(new[]{"BOTH","DIRECT","GENERAL","MINISTRY"},lead.Items.Select(i=>i.Code).Order());
            Assert.Equal(lead.Items.Count,lead.Items.Select(i=>i.TaskId).Distinct().Count());
            Assert.Equal(66.67m,lead.Items.Single(i=>i.Code=="DIRECT").CompletionPercentage);
            Assert.Equal("InProgressOnTime",lead.Items.Single(i=>i.Code=="DIRECT").CalculatedStatus);
            Assert.Equal("Direct",lead.Items.Single(i=>i.Code=="BOTH").AssignmentKind);
            Assert.Equal("NotStarted",lead.Items.Single(i=>i.Code=="GENERAL").CalculatedStatus);
            Assert.All(lead.Items,i=>Assert.Equal(bca.Id,i.ProgressAgencyId));
            Assert.InRange(trace.Sql.Count,1,12); Assert.Contains(trace.Sql,sql=>sql.Contains("IsLeadAssignment") && sql.Contains("EXISTS"));
            var allCoord=await planning.GetDocumentItemsPagedAsync(doc.Id,new DocumentItemsQueryDto { UserRole="Admin", AssignmentRole="Coordinating", PageSize=100 });
            Assert.Equal(new[]{"BOTH","COORD"},allCoord.Items.Select(i=>i.Code).Order());
            var coord=await planning.GetDocumentItemsPagedAsync(doc.Id,new DocumentItemsQueryDto { UserRole="Admin", SelectedAgencyIds=new(){bca.Id}, AssignmentRole="Coordinating", PageSize=100 });
            Assert.Equal(new[]{"BOTH","COORD"},coord.Items.Select(i=>i.Code).Order());
            Assert.Equal("CompletedOnTime",coord.Items.Single(i=>i.Code=="COORD").CalculatedStatus);
            Assert.Equal("Group",coord.Items.Single(i=>i.Code=="BOTH").AssignmentKind);
            var completeCoord=await planning.GetDocumentItemsPagedAsync(doc.Id,new DocumentItemsQueryDto { UserRole="Admin", SelectedAgencyIds=new(){bca.Id}, AssignmentRole="Coordinating", SelectedStatuses=new(){"CompletedOnTime"} });
            Assert.Equal("COORD",Assert.Single(completeCoord.Items).Code);
            var detail=Json(Assert.IsType<OkObjectResult>(await dashboard.GetAgencyItems(bca.Id)).Value!);
            Assert.Equal(4,detail.GetArrayLength()); Assert.Equal("InProgressOnTime",detail.EnumerateArray().Single(i=>i.GetProperty("code").GetString()=="DIRECT").GetProperty("status").GetString());
            var coordinationDetail=Json(Assert.IsType<OkObjectResult>(await dashboard.GetAgencyItems(bca.Id,assignmentRole:"Coordinating")).Value!);
            Assert.Equal(2,coordinationDetail.GetArrayLength());
            var metrics=Json(Assert.IsType<OkObjectResult>(await dashboard.GetDashboardMetrics(agencyId:new[]{bca.Id})).Value!);
            var bcaSummary=metrics.GetProperty("ministriesPerformance").EnumerateArray().Single(i=>i.GetProperty("agencyId").GetGuid()==bca.Id);
            Assert.Equal(4,bcaSummary.GetProperty("totalGoals").GetInt32()); Assert.Equal(1,bcaSummary.GetProperty("goalInProgressOnTime").GetInt32());
            var matrix=Json(Assert.IsType<OkObjectResult>(await new ExecutionController(null!,db).GetTaskAgenciesExecution(general.Id)).Value!);
            var unreported=matrix.GetProperty("agencyExecutions").EnumerateArray().Single(i=>i.GetProperty("agencyId").GetGuid()==bca.Id);
            Assert.Equal("NotReported",unreported.GetProperty("approvalStatus").GetString());
            Assert.True(await AgencyScopePolicy.CanReportAsync(db,general.Id,bca.Id));
            Assert.False(await AgencyScopePolicy.CanReportAsync(db,provinceOnly.Id,bca.Id));
            Assert.False(await AgencyScopePolicy.CanReportAsync(db,coordination.Id,bca.Id));
            // Bulk fixtures prove page two is distinct and query count does not grow per item.
            db.AddRange(Enumerable.Range(0,105).Select(i => new GoalTaskItem { DocumentId=doc.Id, Code=$"PAGE-{i:D3}", Title="Paging", LeadAgencyId=bca.Id, ItemType=ItemTypeEnum.Goal,
                AgencyExecutions=new List<AgencyTaskExecution> { new() { AgencyId=bca.Id, IsLeadAssignment=true, AssignedAt=DateTime.UtcNow } } }));
            await db.SaveChangesAsync(); trace.Sql.Clear();
            var page2=await planning.GetDocumentItemsPagedAsync(doc.Id,new DocumentItemsQueryDto { UserRole="Admin", SelectedAgencyIds=new(){bca.Id}, PageSize=10, PageNumber=2 });
            Assert.Equal(109,page2.TotalCount);Assert.Equal(10,page2.Items.Count);Assert.Equal(10,page2.Items.Select(i=>i.TaskId).Distinct().Count());
            Assert.InRange(trace.Sql.Count,1,12);Assert.Contains(trace.Sql,sql=>sql.Contains("LIMIT") && sql.Contains("OFFSET"));
            // Membership is a snapshot. Metadata additions do not silently change old assignments.
            var added=new Agency { Code="NEW_MIN", Type=AgencyTypeEnum.Ministry };db.Add(added);await db.SaveChangesAsync();
            Assert.False(await db.AgencyTaskExecutions.AnyAsync(e=>e.GoalTaskId==general.Id && e.AgencyId==added.Id));
            var changed=await db.GoalTaskItems.FindAsync(general.Id);await AgencyAssignments.SyncAsync(db,changed!);await db.SaveChangesAsync();
            var count=await db.AgencyTaskExecutions.CountAsync(); await AgencyAssignments.SyncAsync(db,changed!);await db.SaveChangesAsync();Assert.Equal(count,await db.AgencyTaskExecutions.CountAsync());
            changed!.LeadAgencyId=bca.Id; changed.IsGeneralTask=false;await AgencyAssignments.SyncAsync(db,changed);await db.SaveChangesAsync();
            Assert.False((await db.AgencyTaskExecutions.SingleAsync(e=>e.GoalTaskId==general.Id && e.AgencyId==btc.Id)).IsLeadAssignment);
            Assert.True(await db.ProgressLogs.AnyAsync(l=>l.GoalTaskId==general.Id && l.AgencyId==btc.Id));
        }
        finally { await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await command.ExecuteNonQueryAsync(); }
    }
}
