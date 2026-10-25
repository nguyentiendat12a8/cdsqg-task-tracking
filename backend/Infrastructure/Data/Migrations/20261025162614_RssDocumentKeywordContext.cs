using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RssDocumentKeywordContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiAlertFeeds_FeedUrl",
                table: "AiAlertFeeds");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "AiAlertFeeds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("12660000-0000-0000-0000-000000001266"));

            // Preserve old sources: infer their document only when all configured goal IDs agree.
            migrationBuilder.Sql("""
                UPDATE "AiAlertFeeds" f SET "DocumentId"=x.doc
                FROM (
                  SELECT f2."Id", (array_agg(DISTINCT g."DocumentId"))[1] AS doc
                  FROM "AiAlertFeeds" f2 JOIN "GoalTaskItems" g
                    ON position('"GOAL_' || g."Id"::text || '"' in f2."MetricKeysJson") > 0
                  GROUP BY f2."Id" HAVING count(DISTINCT g."DocumentId")=1
                ) x WHERE f."Id"=x."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AiAlertFeeds_DocumentId_FeedUrl",
                table: "AiAlertFeeds",
                columns: new[] { "DocumentId", "FeedUrl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiAlertFeeds_DocumentId_FeedUrl",
                table: "AiAlertFeeds");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "AiAlertFeeds");

            migrationBuilder.CreateIndex(
                name: "IX_AiAlertFeeds_FeedUrl",
                table: "AiAlertFeeds",
                column: "FeedUrl",
                unique: true);
        }
    }
}
