using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261022100000_AddAgencyDisplayNames")]
public class AddAgencyDisplayNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Some local installations already added these optional fields manually.
        migrationBuilder.Sql("""
            ALTER TABLE "GoalTaskItems" ADD COLUMN IF NOT EXISTS "LeadAgencyDisplayText" text NULL;
            ALTER TABLE "GoalTaskItems" ADD COLUMN IF NOT EXISTS "CoordinatingAgenciesDisplayText" text NULL;
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("LeadAgencyDisplayText", "GoalTaskItems");
        migrationBuilder.DropColumn("CoordinatingAgenciesDisplayText", "GoalTaskItems");
    }
}
