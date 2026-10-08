using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdsqg.Application.Services;
using Cdsqg.Core.Entities;
using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;

public class PostgresIntegrationTests
{
    [PostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnnualMigrationDeletesChildrenAndQuarterReportsButPreservesAnnualDataAndAgencyTree(bool legacySchema)
    {
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var schema = "annual_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            await db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().MigrateAsync("20261008151614_PasswordRecovery");
            var agency = new Agency { Code = "ANNUAL" };
            var subAgency = new Agency { Code = "CHILD", ParentId = agency.Id };
            var document = new Document { DocumentNumber = "ANNUAL-TEST" };
            var root = new GoalTaskItem { DocumentId = document.Id, LeadAgencyId = agency.Id, CustomBaseline = new() { ["2026"] = "50", ["Q1_2026"] = "10", ["M1_2026"] = "2", ["hasQuarter"] = "true" } };
            var child = new GoalTaskItem { DocumentId = document.Id, LeadAgencyId = agency.Id };
            var grandchild = new GoalTaskItem { DocumentId = document.Id, LeadAgencyId = agency.Id };
            db.AddRange(agency, subAgency, document, root, child, grandchild);
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"GoalTaskItems\" SET \"ParentId\"={root.Id} WHERE \"Id\"={child.Id}");
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"GoalTaskItems\" SET \"ParentId\"={child.Id} WHERE \"Id\"={grandchild.Id}");
            foreach (var item in new[] { root, child, grandchild })
            {
                foreach (var quarter in new[] { 0, 1 })
                {
                    await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"TargetBaselines\" (\"Id\",\"GoalTaskId\",\"Year\",\"Quarter\",\"TargetQuantity\",\"IsCustomOverride\") VALUES ({Guid.NewGuid()},{item.Id},2026,{quarter},50,false)");
                    await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"ProgressLogs\" (\"Id\",\"GoalTaskId\",\"PeriodYear\",\"PeriodQuarter\",\"LogDate\",\"QuantitativeValue\",\"SummaryNotes\",\"AttachmentFileUrls\",\"CalculatedAlert\",\"CreatedBy\",\"ApprovalStatus\",\"AgencyId\") VALUES ({Guid.NewGuid()},{item.Id},2026,{quarter},{DateTime.UtcNow},25,'','[]'::jsonb,'Yellow','test',1,{agency.Id})");
                }
                db.TaskUrgeLogs.Add(new() { GoalTaskId = item.Id });
                db.Notifications.Add(new() { LinkUrl = "/document-detail?taskId=" + item.Id });
                db.AgencyTaskExecutions.Add(new() { GoalTaskId = item.Id, AgencyId = agency.Id, CompletionPercentage = 99 });
            }
            await db.SaveChangesAsync();
            if (legacySchema)
            {
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"GoalTaskItems\" DROP CONSTRAINT \"FK_GoalTaskItems_GoalTaskItems_ParentId\"; DROP INDEX \"IX_GoalTaskItems_ParentId\"; DROP INDEX \"IX_TargetBaselines_GoalTaskId_Year_Quarter\"");
            }
            await db.Database.MigrateAsync();
            db.ChangeTracker.Clear();
            Assert.Equal(root.Id, (await db.GoalTaskItems.SingleAsync()).Id);
            Assert.Single(await db.TargetBaselines.ToListAsync());
            Assert.Single(await db.ProgressLogs.ToListAsync());
            Assert.Single(await db.TaskUrgeLogs.ToListAsync());
            Assert.Single(await db.Notifications.ToListAsync());
            Assert.Equal(agency.Id, (await db.Agencies.SingleAsync(a => a.Id == subAgency.Id)).ParentId);
            var itemAfter = await db.GoalTaskItems.Include(i => i.Baselines).Include(i => i.ProgressLogs).SingleAsync();
            Assert.Single(itemAfter.CustomBaseline);
            Assert.Equal("50", itemAfter.CustomBaseline["2026"]);
            await ProgressCalculator.RefreshCachesAsync(db, itemAfter);
            await db.SaveChangesAsync();
            Assert.Equal(50m, (await db.AgencyTaskExecutions.SingleAsync()).CompletionPercentage);
            await db.Database.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
        }
        finally
        {
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }

    [PostgresFact]
    public async Task MigrationsAndOneTimeRecoveryWorkOnPostgreSql()
    {
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Set CDSQG_TEST_POSTGRES to a disposable PostgreSQL test database.");
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var schema = "review_" + Guid.NewGuid().ToString("N");
        var builder = new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema };
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await command.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(builder.ConnectionString).Options;
            await using var db = new AppDbContext(options);
            var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
            await migrator.MigrateAsync("20261008151316_InitialSchema");
            await db.Database.ExecuteSqlRawAsync("DROP TABLE \"__EFMigrationsHistory\"");
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "scripts", "adopt-legacy-baseline.sql"))) root = root.Parent;
            var baseline = await File.ReadAllTextAsync(Path.Combine(root!.FullName, "scripts", "adopt-legacy-baseline.sql"));
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ALTER COLUMN \"FullName\" TYPE varchar(42)");
            await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(baseline));
            await db.Database.ExecuteSqlRawAsync("ROLLBACK");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ALTER COLUMN \"FullName\" TYPE text");
            await db.Database.ExecuteSqlRawAsync(baseline);
            await db.Database.MigrateAsync();
            await db.Database.MigrateAsync();
            Assert.Equal(3, (await db.Database.GetAppliedMigrationsAsync()).Count());
            Assert.False(db.Database.HasPendingModelChanges());
            var hasher = new PasswordHasher();
            var user = new User { Username = "recovery", Email = "review@example.test", PasswordHash = hasher.HashPassword("OldPassword123!") };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var sender = new CapturingSender();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["PasswordRecovery:FrontendUrl"] = "https://example.test" }).Build();
            PasswordRecoveryService Service(AppDbContext context) => new(context, hasher, sender, config, NullLogger<PasswordRecoveryService>.Instance);
            await Service(db).RequestAsync("unknown");
            Assert.Null(sender.Url);
            await Service(db).RequestAsync("recovery");
            var token = sender.Url!.Split("token=")[1];
            Assert.NotEqual(token, user.PasswordResetTokenHash);
            var stamp = user.SecurityStamp;
            await using var db2 = new AppDbContext(options);
            var results = await Task.WhenAll(Service(db).ResetAsync(token, "NewPassword123!"), Service(db2).ResetAsync(token, "OtherPassword123!"));
            Assert.Single(results, r => r);
            Assert.False(await Service(db).ResetAsync(token, "NewPassword123!"));
            db.ChangeTracker.Clear();
            var updated = await db.Users.SingleAsync();
            Assert.NotEqual(stamp, updated.SecurityStamp);
            Assert.Null(updated.PasswordResetTokenHash);
            await Service(db).RequestAsync("recovery");
            token = sender.Url!.Split("token=")[1];
            await db.Users.ExecuteUpdateAsync(s => s.SetProperty(u => u.PasswordResetExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
            Assert.False(await Service(db).ResetAsync(token, "NewPassword123!"));
            Assert.False(await Service(db).ResetAsync("invalid", "short"));
        }
        finally
        {
            // The schema name is generated above; never clean a user-provided schema.
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
    private sealed class CapturingSender : IRecoveryEmailSender
    {
        public string? Url;
        public Task SendAsync(string email, string resetUrl) { Url = resetUrl; return Task.CompletedTask; }
    }
}
public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")))
            Skip = "Set CDSQG_TEST_POSTGRES to run integration tests on a disposable database.";
    }
}
public sealed class PostgresTheoryAttribute : TheoryAttribute
{
    public PostgresTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")))
            Skip = "Set CDSQG_TEST_POSTGRES to run integration tests on a disposable database.";
    }
}
