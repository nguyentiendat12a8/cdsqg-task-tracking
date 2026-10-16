using Cdsqg.Application.DTOs;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Core.Enums;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;
public class DocumentSortPostgresTests
{
    [PostgresFact]
    public async Task GoalAndTaskPagesPreserveRequestedOrderInsteadOfUpdateTime()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        var schema = "sort_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync();
            var agency = new Agency { Code = "SORT", Name = "Sort test", Type = AgencyTypeEnum.Ministry, IsActive = true };
            var doc = new Document { DocumentNumber = "SORT", StartYear = 2026, EndYear = 2030 };
            db.AddRange(agency, doc);
            foreach (var kind in new[] { ItemTypeEnum.Goal, ItemTypeEnum.Task })
                for (var n = 1; n <= 14; n++)
                    db.Add(new GoalTaskItem { DocumentId = doc.Id, LeadAgencyId = agency.Id, ItemType = kind,
                        Code = $"{(kind == ItemTypeEnum.Goal ? "MT" : "NV")}-{n:D2}", Title = $"Title {n:D2}",
                        CreatedAt = new DateTime(2026, 1, n), LastUpdated = new DateTime(2026, 2, n) });
            await db.SaveChangesAsync();
            var service = new PlanningService(db);
            foreach (var kind in new[] { "Goal", "Task" })
            foreach (var field in new[] { "code", "title" })
            foreach (var direction in new[] { "asc", "desc" })
            foreach (var search in new[] { "", "Title" })
            {
                var actual = new List<string>();
                for (var page = 1; page <= 2; page++)
                {
                    var result = await service.GetDocumentItemsPagedAsync(doc.Id, new DocumentItemsQueryDto {
                        UserRole = "Admin", ItemType = kind, SortBy = field, SortOrder = direction,
                        Search = search, PageSize = 10, PageNumber = page });
                    Assert.Equal(14, result.TotalCount);
                    Assert.Equal(page == 1 ? 10 : 4, result.Items.Count);
                    actual.AddRange(result.Items.Select(row => row.Code));
                }
                var numbers = direction == "asc" ? Enumerable.Range(1,14) : Enumerable.Range(1,14).Reverse();
                Assert.Equal(numbers.Select(n => $"{(kind == "Goal" ? "MT" : "NV")}-{n:D2}"), actual);
            }
        }
        finally
        {
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
}
