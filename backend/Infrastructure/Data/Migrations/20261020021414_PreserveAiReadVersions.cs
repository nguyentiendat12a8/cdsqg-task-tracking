using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreserveAiReadVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiSourceDocuments_SourceId_Url_ContentHash",
                table: "AiSourceDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_AiSourceDocuments_SourceId_Url_ContentHash",
                table: "AiSourceDocuments",
                columns: new[] { "SourceId", "Url", "ContentHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiSourceDocuments_SourceId_Url_ContentHash",
                table: "AiSourceDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_AiSourceDocuments_SourceId_Url_ContentHash",
                table: "AiSourceDocuments",
                columns: new[] { "SourceId", "Url", "ContentHash" },
                unique: true);
        }
    }
}
