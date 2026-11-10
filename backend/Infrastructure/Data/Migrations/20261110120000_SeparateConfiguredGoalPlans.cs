using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261110120000_SeparateConfiguredGoalPlans")]
public class SeparateConfiguredGoalPlans : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "ConfiguredTargetQuantity",
            table: "TargetBaselines",
            type: "numeric",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ConfiguredTargetQuantity", table: "TargetBaselines");
    }
}
