using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Cdsqg.Infrastructure.Data.Migrations;
using Npgsql;
using Xunit;

namespace Cdsqg.Tests;

public class CoordinatingProgressPostgresTests
{
    [PostgresFact]
    public async Task Migration_AddsRoleColumnWithLeadDefault()
    {
        var connection = Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection).Options);
        await using var sql = new NpgsqlConnection(connection);
        await sql.OpenAsync();
        await using (var setup = new NpgsqlCommand("CREATE TABLE \"ProgressLogs\" (\"Id\" uuid PRIMARY KEY); INSERT INTO \"ProgressLogs\" (\"Id\") VALUES (gen_random_uuid())", sql))
            await setup.ExecuteNonQueryAsync();

        var migration = new CoordinatingProgressReports();
        var generator = db.GetService<IMigrationsSqlGenerator>();
        foreach (var operation in generator.Generate(migration.UpOperations, db.Model))
        {
            await using var apply = new NpgsqlCommand(operation.CommandText, sql);
            await apply.ExecuteNonQueryAsync();
        }
        await using var command = new NpgsqlCommand("""
            SELECT column_default, is_nullable,
                   (SELECT COUNT(*) FROM "ProgressLogs" WHERE "IsCoordinatingReport" = false)
            FROM information_schema.columns
            WHERE table_schema = current_schema() AND table_name = 'ProgressLogs'
              AND column_name = 'IsCoordinatingReport'
            """, sql);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Contains("false", reader.GetString(0), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("NO", reader.GetString(1));
        Assert.Equal(1, reader.GetInt64(2));
    }
}
