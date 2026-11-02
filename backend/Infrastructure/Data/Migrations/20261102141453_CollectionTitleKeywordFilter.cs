using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CollectionTitleKeywordFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "DigitalPlanKeywords" SET "Conditions"='ban hành, phê duyệt, thông qua, công bố'
                WHERE "Phrase" IN ('chuyển đổi số','chính phủ số','chính quyền số')
                  AND "Conditions"='kế hoạch, đề án, chương trình, nghị quyết, chiến lược';
                """);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preserve later user edits to collection keywords.
        }
    }
}
