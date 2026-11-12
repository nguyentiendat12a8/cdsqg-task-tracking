using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

// Existing reports belong to the lead workflow. Coordination reports use the
// same per-item/per-agency log storage but have an independent reporting role.
[DbContext(typeof(AppDbContext))]
[Migration("20261112110000_CoordinatingProgressReports")]
public class CoordinatingProgressReports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsCoordinatingReport", table: "ProgressLogs", type: "boolean",
            nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsCoordinatingReport", table: "ProgressLogs");
    }
}
