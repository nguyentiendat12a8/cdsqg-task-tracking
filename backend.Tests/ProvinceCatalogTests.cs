using Cdsqg.Api.Controllers;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public class ProvinceCatalogTests
{
    [Fact]
    public void CatalogHasExactly34UniqueCurrentNamesAndCodes()
    {
        Assert.Equal(34, ProvinceCatalog.Names.Count);
        Assert.Equal(34, ProvinceCatalog.Names.Select(ProvinceCatalog.Code).Distinct().Count());
        Assert.Contains("Thành phố Hồ Chí Minh", ProvinceCatalog.Names);
        Assert.DoesNotContain("Bình Dương", ProvinceCatalog.Names);
        Assert.True(ProvinceCatalog.IsFixed(new Agency { Name = "UBND TP. Hồ Chí Minh", Code = "TPHCM" }));
        Assert.False(ProvinceCatalog.IsFixed(new Agency { Name = "UBND tỉnh Bình Dương", Code = "OLD" }));
    }
    [PostgresFact]
    public async Task SeedPreservesExistingAgencyAndIsRepeatableAndCatalogIdentityIsLocked()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "catalog_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection); await admin.OpenAsync();
        await using (var c = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await c.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options); await db.Database.MigrateAsync();
            var existing = new Agency { Name = "UBND TP. Hồ Chí Minh", Code = "TPHCM", Type = AgencyTypeEnum.Province };
            var other = new Agency { Name = "Cơ quan thử nghiệm", Code = "TEST", Type = AgencyTypeEnum.Ministry };
            var shortName = new Agency { Name = "  hÀ   nỘi  ", Code = "LEGACY_HN", Type = AgencyTypeEnum.Province };
            var prefixed = new Agency { Name = "UBND tỉnh Bắc Ninh", Code = "LEGACY_BN", Type = AgencyTypeEnum.Province };
            db.Agencies.AddRange(existing, other, shortName, prefixed); await db.SaveChangesAsync();
            ProvinceCatalog.Seed(db); var ids = await db.Agencies.Select(a => a.Id).ToListAsync();
            ProvinceCatalog.Seed(db); Assert.Equal(ids.Order(), (await db.Agencies.Select(a=>a.Id).ToListAsync()).Order());
            Assert.Equal(34, await db.Agencies.CountAsync(a => a.Type == AgencyTypeEnum.Province));
            Assert.Equal("TPHCM", (await db.Agencies.FindAsync(existing.Id))!.Code);
            Assert.Equal("Thành phố Hồ Chí Minh", ProvinceCatalog.DisplayName(existing));
            Assert.Equal("Thành phố Hà Nội", ProvinceCatalog.DisplayName(shortName));
            Assert.Equal("Thành phố Bắc Ninh", ProvinceCatalog.DisplayName(prefixed));
            Assert.Equal("Đồng Tháp", (await db.Agencies.SingleAsync(a => a.Code == ProvinceCatalog.Code("Đồng Tháp"))).Name);
            Assert.Equal("  hÀ   nỘi  ", (await db.Agencies.FindAsync(shortName.Id))!.Name);
            var controller = new AgencyController(db, null!);
            Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.DeleteAgency(existing.Id)).StatusCode);
            Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.UpdateAgency(existing.Id, new Agency { Code = existing.Code, Name = "Tên khác", Type = existing.Type })).StatusCode);
            Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.CreateAgency(new Agency { Type = AgencyTypeEnum.Province, Name = "Tỉnh mới" })).StatusCode);
            Assert.Equal(403, Assert.IsType<ObjectResult>(await controller.UpdateAgency(other.Id, new Agency { Name = "Tỉnh mới", Type = AgencyTypeEnum.Province })).StatusCode);
            Assert.Equal("UBND TP. Hồ Chí Minh", (await db.Agencies.FindAsync(existing.Id))!.Name);
        }
        finally { await using var c = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin); await c.ExecuteNonQueryAsync(); }
    }
}
