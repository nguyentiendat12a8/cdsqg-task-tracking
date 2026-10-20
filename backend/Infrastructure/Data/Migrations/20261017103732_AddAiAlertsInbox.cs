using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAlertsInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AiDataSources\" SET \"Enabled\" = FALSE, \"NextCheckAt\" = NULL");
            migrationBuilder.CreateTable(
                name: "AiInboxItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    UrlHash = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    MetricKeysJson = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiInboxItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiInboxItems_Status_LeaseUntil",
                table: "AiInboxItems",
                columns: new[] { "Status", "LeaseUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_AiInboxItems_UrlHash",
                table: "AiInboxItems",
                column: "UrlHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiInboxItems");
        }
    }
}
