using Cdsqg.Api.Controllers;
using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;

public class LegalStatsPostgresTests
{
    [PostgresFact]
    public async Task IndexedStatsPreserveCountsFiltersRecentDocumentsAndDoNotTrackReadEntities()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "legal_perf_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            var ministry = new Agency { Code = "M", Name = "Bộ kiểm thử", Type = AgencyTypeEnum.Ministry };
            var province = new Agency { Code = "P", Name = "Tỉnh kiểm thử", Type = AgencyTypeEnum.Province };
            var child = new Agency { Code = "C", Name = "Đơn vị con", ParentId = ministry.Id };
            var documents = new[] {
                new LegalDocument { Code="SAME", Title="Trích yếu dài", DocumentType="Luật", Field="Dữ liệu số", IssuingAgencyId=ministry.Id, DraftingAgencyId=ministry.Id, IssuedDate=new(2026,10,1), Notes="Ghi chú cần giữ", AttachmentsJson="[{\"fileName\":\"test.pdf\",\"fileUrl\":\"/test.pdf\"}]" },
                new LegalDocument { Code="BOTH", DocumentType="Luật", IssuingAgencyId=ministry.Id, DraftingAgencyId=province.Id },
                new LegalDocument { Code="CHILD", DocumentType="Quyết định", IssuingAgencyId=child.Id },
                new LegalDocument { Code="NONE", DocumentType="Thông tư" }
            };
            db.AddRange(ministry, province, child); db.AddRange(documents);
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            var controller = new LegalDocumentController(db, new UnusedStorage());
            var stats = Assert.IsType<LegalDocumentChartStatsDto>(Assert.IsType<OkObjectResult>(await controller.GetDashboardStats(userRole:"Admin")).Value);
            Assert.Equal(4, stats.TotalCount);
            foreach (var agency in new[] { ministry, province })
                Assert.Equal(documents.Count(d => d.IssuingAgencyId == agency.Id || d.DraftingAgencyId == agency.Id), stats.AgencyStats.Single(a => a.AgencyId == agency.Id).TotalCount);
            Assert.Equal(2, stats.MinistriesStats.Single().TypeCounts["Luật"]);
            Assert.Empty(db.ChangeTracker.Entries());
            var filtered = Assert.IsType<LegalDocumentChartStatsDto>(Assert.IsType<OkObjectResult>(await controller.GetDashboardStats(userRole:"Admin", documentType:"Luật", field:"Dữ liệu số")).Value);
            Assert.Equal(1, filtered.TotalCount);
            var level2 = Assert.IsType<LegalDocumentChartStatsDto>(Assert.IsType<OkObjectResult>(await controller.GetDashboardStats(userRole:"Level2", userAgencyId:ministry.Id)).Value);
            Assert.Equal(1, level2.AgencyStats.Single(a=>a.AgencyId==child.Id).TotalCount);
            var level3 = Assert.IsType<LegalDocumentChartStatsDto>(Assert.IsType<OkObjectResult>(await controller.GetDashboardStats(userRole:"Level3", userAgencyId:child.Id)).Value);
            var recent = level3.RecentDocuments.Single(d=>d.Code=="SAME");
            Assert.Equal("Trích yếu dài", recent.Title);
            Assert.Equal("Ghi chú cần giữ", recent.Notes);
            Assert.Single(recent.Attachments);
            Assert.Empty(db.ChangeTracker.Entries());
            Assert.IsType<OkObjectResult>(await new AgencyController(db, new UnusedStorage()).GetAllAgencies());
            Assert.Empty(db.ChangeTracker.Entries());
        }
        finally
        {
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
    private sealed class UnusedStorage : IFileStorageService
    {
        public Task<string> SaveEvidenceFileAsync(IFormFile file) => throw new NotSupportedException();
        public Task<string> SaveDocumentFileAsync(IFormFile file) => throw new NotSupportedException();
    }
}
