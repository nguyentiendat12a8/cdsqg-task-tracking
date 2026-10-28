using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DigitalTransformationNewsSheetSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalNewsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Title = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Url = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    UrlHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceSheetUrl = table.Column<string>(type: "text", nullable: false),
                    SourceRow = table.Column<int>(type: "integer", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalNewsItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DigitalNewsSyncSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SheetUrl = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    NextRunAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastStartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastCompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastStatus = table.Column<string>(type: "text", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: false),
                    Added = table.Column<int>(type: "integer", nullable: false),
                    Updated = table.Column<int>(type: "integer", nullable: false),
                    Unchanged = table.Column<int>(type: "integer", nullable: false),
                    Skipped = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    LeaseToken = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalNewsSyncSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DigitalNewsSyncSettings",
                columns: new[] { "Id", "Added", "Enabled", "IntervalHours", "LastCompletedAt", "LastError", "LastStartedAt", "LastStatus", "LeaseToken", "LeaseUntil", "NextRunAt", "SheetUrl", "Skipped", "Unchanged", "Updated", "UpdatedBy" },
                values: new object[] { 1, 0, true, 2, null, "", null, "NotStarted", null, null, null, "https://docs.google.com/spreadsheets/d/1fOGT8aBm6X5ul2pk8p7zxzixEMklpqLVXEgsSCFrEDI/edit?gid=731708878", 0, 0, 0, "" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalNewsItems_ImportedAt_Id",
                table: "DigitalNewsItems",
                columns: new[] { "ImportedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalNewsItems_UrlHash",
                table: "DigitalNewsItems",
                column: "UrlHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalNewsItems");

            migrationBuilder.DropTable(
                name: "DigitalNewsSyncSettings");
        }
    }
}
