using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

// Cho phép giao nhiều đơn vị trực thuộc (Cấp 3) cho một mục tiêu/nhiệm vụ, tách khỏi
// cơ quan đồng chủ trì (AssignedAgencyId). Dữ liệu cũ: AssignedAgencyId đang trỏ tới
// đơn vị trực thuộc được chuyển sang SubAgencyIds; cơ quan đồng chủ trì giữ nguyên.
[DbContext(typeof(AppDbContext))]
[Migration("20261111120000_MultipleSubAgencyAssignments")]
public class MultipleSubAgencyAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SubAgencyIds",
            table: "GoalTaskItems",
            type: "jsonb",
            nullable: false,
            defaultValueSql: "'[]'::jsonb");

        migrationBuilder.Sql("""
            UPDATE "GoalTaskItems" g
            SET "SubAgencyIds" = jsonb_build_array(g."AssignedAgencyId"),
                "AssignedAgencyId" = NULL
            FROM "Agencies" a
            WHERE a."Id" = g."AssignedAgencyId" AND a."ParentId" IS NOT NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Chỉ khôi phục được một đơn vị trực thuộc (đơn vị đầu mối) vào cột cũ.
        migrationBuilder.Sql("""
            UPDATE "GoalTaskItems"
            SET "AssignedAgencyId" = ("SubAgencyIds"->>0)::uuid
            WHERE "AssignedAgencyId" IS NULL AND jsonb_array_length("SubAgencyIds") > 0;
            """);
        migrationBuilder.DropColumn(name: "SubAgencyIds", table: "GoalTaskItems");
    }
}
