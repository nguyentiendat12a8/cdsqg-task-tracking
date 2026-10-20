using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAlertRssSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlertFeedId",
                table: "AiInboxItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "AiInboxItems",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiAlertFeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FeedUrl = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanMinute = table.Column<int>(type: "integer", nullable: false),
                    NextScanAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAlertFeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiAlertScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Trigger = table.Column<string>(type: "text", nullable: false),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Received = table.Column<int>(type: "integer", nullable: false),
                    Added = table.Column<int>(type: "integer", nullable: false),
                    Duplicates = table.Column<int>(type: "integer", nullable: false),
                    Blocked = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAlertScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAlertScans_AiAlertFeeds_FeedId",
                        column: x => x.FeedId,
                        principalTable: "AiAlertFeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiInboxItems_AlertFeedId",
                table: "AiInboxItems",
                column: "AlertFeedId");

            migrationBuilder.CreateIndex(
                name: "IX_AiAlertFeeds_FeedUrl",
                table: "AiAlertFeeds",
                column: "FeedUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiAlertScans_FeedId",
                table: "AiAlertScans",
                column: "FeedId",
                unique: true,
                filter: "\"Status\" IN ('Queued', 'Running')");

            migrationBuilder.CreateIndex(
                name: "IX_AiAlertScans_Status_LeaseUntil",
                table: "AiAlertScans",
                columns: new[] { "Status", "LeaseUntil" });

            migrationBuilder.AddForeignKey(
                name: "FK_AiInboxItems_AiAlertFeeds_AlertFeedId",
                table: "AiInboxItems",
                column: "AlertFeedId",
                principalTable: "AiAlertFeeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiInboxItems_AiAlertFeeds_AlertFeedId",
                table: "AiInboxItems");

            migrationBuilder.DropTable(
                name: "AiAlertScans");

            migrationBuilder.DropTable(
                name: "AiAlertFeeds");

            migrationBuilder.DropIndex(
                name: "IX_AiInboxItems_AlertFeedId",
                table: "AiInboxItems");

            migrationBuilder.DropColumn(
                name: "AlertFeedId",
                table: "AiInboxItems");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "AiInboxItems");
        }
    }
}
