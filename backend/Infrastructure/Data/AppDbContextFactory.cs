using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cdsqg.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    static AppDbContextFactory() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    public AppDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(Environment.GetEnvironmentVariable("CDSQG_MIGRATION_CONNECTION")
            ?? "Host=localhost;Database=cdsqg_design;Username=postgres") .Options);
}
