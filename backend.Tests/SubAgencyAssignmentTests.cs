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

// Nhiều đơn vị trực thuộc (Cấp 3) cho một mục, độc lập với cơ quan đồng chủ trì.
public class SubAgencyAssignmentTests
{
    [Fact]
    public void PrimaryAgencyPrefersCoLeadThenFirstSubAgencyThenLead()
    {
        var lead = Guid.NewGuid(); var coLead = Guid.NewGuid(); var sub1 = Guid.NewGuid(); var sub2 = Guid.NewGuid();
        Assert.Equal(lead, AgencyAssignments.PrimaryAgencyId(new GoalTaskItem { LeadAgencyId = lead }));
        Assert.Equal(sub1, AgencyAssignments.PrimaryAgencyId(new GoalTaskItem { LeadAgencyId = lead, SubAgencyIds = new() { sub1, sub2 } }));
        Assert.Equal(coLead, AgencyAssignments.PrimaryAgencyId(new GoalTaskItem { LeadAgencyId = lead, AssignedAgencyId = coLead, SubAgencyIds = new() { sub1 } }));
    }

    [Fact]
    public void MinistryUnitGroupExpandsToActiveSiblingUnitsAndNeverRepresentsProgress()
    {
        var bkhcn = new Agency { Code = "BKHCN", Name = "Bộ KH&CN", Type = AgencyTypeEnum.Ministry };
        var cuc = new Agency { Code = "CDSQG", Name = "Cục", Type = AgencyTypeEnum.Internal, ParentId = bkhcn.Id };
        var vien = new Agency { Code = "VIEN", Name = "Viện", Type = AgencyTypeEnum.Internal, ParentId = bkhcn.Id };
        var old = new Agency { Code = "OLD", Name = "Đã giải thể", Type = AgencyTypeEnum.Internal, ParentId = bkhcn.Id, IsActive = false };
        var all = new Agency { Code = "ALL_UNITS_BKHCN", Name = "Tất cả cơ quan, đơn vị thuộc Bộ", Type = AgencyTypeEnum.Special, ParentId = bkhcn.Id };
        var other = new Agency { Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };
        var agencies = new[] { bkhcn, cuc, vien, old, all, other };
        Assert.True(ReportingAgencyGroups.IsGroup(all));
        Assert.Equal(new[] { cuc.Id, vien.Id }.Order(), ReportingAgencyGroups.Members(all, agencies).Select(a => a.Id).Order());
        // Mục chỉ giao cho nhóm: tiến độ chung vẫn lấy theo cơ quan chủ trì.
        var item = new GoalTaskItem { LeadAgencyId = bkhcn.Id, SubAgencyIds = new() { all.Id },
            AgencyExecutions = new List<AgencyTaskExecution> {
                new() { AgencyId = bkhcn.Id, IsLeadAssignment = true },
                new() { AgencyId = cuc.Id, IsLeadAssignment = true, IsLeadGroupAssignment = true },
                new() { AgencyId = vien.Id, IsLeadAssignment = true, IsLeadGroupAssignment = true } } };
        Assert.Equal(bkhcn.Id, AgencyAssignments.PrimaryAgencyId(item));
    }

    [PostgresFact]
    public async Task AssigningSeveralSubAgenciesKeepsCoLeadAndSyncsLeadRows()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "subagency_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection); await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();

            var bkhcn = new Agency { Code = "BKHCN", Name = "Bộ Khoa học và Công nghệ", Type = AgencyTypeEnum.Ministry };
            var bca = new Agency { Code = "BCA", Name = "Bộ Công an", Type = AgencyTypeEnum.Ministry };
            var cuc = new Agency { Code = "CDSQG", Name = "Cục Chuyển đổi số quốc gia", Type = AgencyTypeEnum.Internal, ParentId = bkhcn.Id };
            var vien = new Agency { Code = "VIEN", Name = "Viện Công nghệ số", Type = AgencyTypeEnum.Internal, ParentId = bkhcn.Id };
            var doc = new Document { DocumentNumber = "SUB", StartYear = 2026, EndYear = 2030 };
            var item = new GoalTaskItem { DocumentId = doc.Id, Code = "NV-31", Title = "NDXP", LeadAgencyId = bkhcn.Id, AssignedAgencyId = bca.Id, ItemType = ItemTypeEnum.Task };
            db.AddRange(bkhcn, bca, cuc, vien, doc, item);
            await db.SaveChangesAsync();
            await AgencyAssignments.SyncAsync(db, item); await db.SaveChangesAsync();

            var controller = new GoalTaskItemController(db);
            Assert.IsType<OkObjectResult>(await controller.AssignItem(item.Id, new AssignGoalTaskItemRequestDto { AssignedAgencyIds = new() { cuc.Id, vien.Id } }));
            db.ChangeTracker.Clear();
            var saved = await db.GoalTaskItems.SingleAsync(i => i.Id == item.Id);
            Assert.Equal(bca.Id, saved.AssignedAgencyId);
            Assert.Equal(new[] { cuc.Id, vien.Id }, saved.SubAgencyIds);
            var leads = await db.AgencyTaskExecutions.Where(e => e.GoalTaskId == item.Id && e.IsLeadAssignment).Select(e => e.AgencyId).ToListAsync();
            Assert.Equal(new[] { bkhcn.Id, bca.Id, cuc.Id, vien.Id }.Order(), leads.Order());

            // Bớt một đơn vị: dòng phân giao còn lại nhưng không còn vai trò chủ trì.
            Assert.IsType<OkObjectResult>(await controller.AssignItem(item.Id, new AssignGoalTaskItemRequestDto { AssignedAgencyIds = new() { vien.Id } }));
            db.ChangeTracker.Clear();
            Assert.False((await db.AgencyTaskExecutions.SingleAsync(e => e.GoalTaskId == item.Id && e.AgencyId == cuc.Id)).IsLeadAssignment);
            Assert.Equal(bca.Id, (await db.GoalTaskItems.SingleAsync(i => i.Id == item.Id)).AssignedAgencyId);

            // Cơ quan không có cấp trên không được giao như đơn vị trực thuộc.
            Assert.IsType<BadRequestObjectResult>(await controller.AssignItem(item.Id, new AssignGoalTaskItemRequestDto { AssignedAgencyIds = new() { bca.Id } }));

            // Bộ lọc đơn vị trực thuộc dùng danh sách mới.
            var planning = new PlanningService(db);
            var filtered = await planning.GetDocumentItemsPagedAsync(doc.Id, new DocumentItemsQueryDto { UserRole = "Admin", SelectedSubAgencyIds = new() { vien.Id }, PageSize = 10 });
            var row = Assert.Single(filtered.Items);
            Assert.Equal(new[] { "Viện Công nghệ số" }, row.SubAgencyNames);
        }
        finally
        {
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
}
