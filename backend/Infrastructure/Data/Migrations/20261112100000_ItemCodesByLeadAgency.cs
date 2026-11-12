using Cdsqg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

// Đánh mã mục tiêu/nhiệm vụ theo cơ quan chủ trì (MT-BCA-01, NV-C-01):
// - Agencies.ShortCode: viết tắt cơ quan dùng trong mã.
// - GoalTaskItems.OriginalCode: giữ số thứ tự gốc theo quyết định (MT-01, NV-31) để sắp xếp, đối chiếu.
// Migration chỉ thêm cột và sao chép mã hiện tại sang OriginalCode; việc đổi mã chạy bằng
// scripts/doi_ma_theo_co_quan_chu_tri.sql để kiểm tra trước trên local.
[DbContext(typeof(AppDbContext))]
[Migration("20261112100000_ItemCodesByLeadAgency")]
public class ItemCodesByLeadAgency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ShortCode",
            table: "Agencies",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OriginalCode",
            table: "GoalTaskItems",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("""UPDATE "GoalTaskItems" SET "OriginalCode" = "Code" WHERE "OriginalCode" = '';""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Trả lại mã gốc trước khi bỏ cột.
        migrationBuilder.Sql("""UPDATE "GoalTaskItems" SET "Code" = "OriginalCode" WHERE "OriginalCode" <> '';""");
        migrationBuilder.DropColumn(name: "OriginalCode", table: "GoalTaskItems");
        migrationBuilder.DropColumn(name: "ShortCode", table: "Agencies");
    }
}
