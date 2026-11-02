using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DigitalPlanEvaluationCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DigitalPlanReads_ItemId_TitleHash",
                table: "DigitalPlanReads");

            migrationBuilder.AddColumn<string>(
                name: "CriteriaHash",
                table: "DigitalPlanReads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CriteriaJson",
                table: "DigitalPlanReads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EvaluationCriteriaJson",
                table: "DigitalNewsSyncSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DigitalNewsSyncSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "EvaluationCriteriaJson",
                value: "");

            // Freeze the previous title rules for existing reads; no AI calls or result deletion.
            var snapshot = System.Text.Json.JsonSerializer.Serialize(new {
                RelatedKeywords="chuyển đổi số",ActionKeywords="ban hành\nphê duyệt",
                ExcludedKeywords="dự thảo\ndự kiến\nlấy ý kiến\nchưa ban hành\nsẽ phê duyệt\nsẽ ban hành\nsắp ban hành\nxã\nphường\ntrường học\ncông ty\nđảng bộ\ncơ quan đảng",
                Annual=true,FiveYear=true
            });
            var hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(snapshot)));
            migrationBuilder.Sql($"UPDATE \"DigitalPlanReads\" SET \"CriteriaJson\"='{snapshot.Replace("'","''")}', \"CriteriaHash\"='{hash}' WHERE \"CriteriaHash\"='';");
            migrationBuilder.Sql($"UPDATE \"DigitalNewsSyncSettings\" SET \"EvaluationCriteriaJson\"='{snapshot.Replace("'","''")}' WHERE \"EvaluationCriteriaJson\"='';");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPlanReads_ItemId_TitleHash_CriteriaHash",
                table: "DigitalPlanReads",
                columns: new[] { "ItemId", "TitleHash", "CriteriaHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DigitalPlanReads_ItemId_TitleHash_CriteriaHash",
                table: "DigitalPlanReads");

            migrationBuilder.DropColumn(
                name: "CriteriaHash",
                table: "DigitalPlanReads");

            migrationBuilder.DropColumn(
                name: "CriteriaJson",
                table: "DigitalPlanReads");

            migrationBuilder.DropColumn(
                name: "EvaluationCriteriaJson",
                table: "DigitalNewsSyncSettings");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPlanReads_ItemId_TitleHash",
                table: "DigitalPlanReads",
                columns: new[] { "ItemId", "TitleHash" },
                unique: true);
        }
    }
}
