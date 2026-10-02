using Microsoft.Extensions.Configuration;

namespace Cdsqg.Application.Services;

public static class DatabaseConfiguration
{
    public static string ResolveConnectionString(IConfiguration configuration)
    {
        // Deployment environment takes precedence over the checked-in local connection.
        var url = configuration["DATABASE_URL"];
        return !string.IsNullOrWhiteSpace(url) ? url : configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public static bool UsePostgreSql(IConfiguration configuration) =>
        configuration.GetValue<bool>("UsePostgreSQL") || !string.IsNullOrWhiteSpace(ResolveConnectionString(configuration));
}
