using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261022140000_AddGoalResultTarget")]
public class AddGoalResultTarget : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<decimal>(name: "ResultTarget", table: "GoalTaskItems", type: "numeric", nullable: true);
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "ResultTarget", table: "GoalTaskItems");
}
