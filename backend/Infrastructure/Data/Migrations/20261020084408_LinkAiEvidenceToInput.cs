using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkAiEvidenceToInput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InboxItemId",
                table: "AiSourceDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "AiSourceDocuments" d SET "InboxItemId"=i."Id"
                FROM "AiInboxItems" i WHERE i."StorageKey"<>'' AND d."Url"='/api/ai-inbox/'||i."Id"::text||'/file';
                UPDATE "AiSourceDocuments" d SET "InboxItemId"=i."Id"
                FROM "AiInboxItems" i WHERE d."InboxItemId" IS NULL AND i."AlertFeedId" IS NOT NULL AND i."StorageKey"='' AND d."Url"=i."Url"
                AND (SELECT count(*) FROM "AiInboxItems" x WHERE x."Url"=d."Url" AND x."AlertFeedId" IS NOT NULL)=1;
                """);
            migrationBuilder.CreateIndex(
                name: "IX_AiSourceDocuments_InboxItemId",
                table: "AiSourceDocuments",
                column: "InboxItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiSourceDocuments_AiInboxItems_InboxItemId",
                table: "AiSourceDocuments",
                column: "InboxItemId",
                principalTable: "AiInboxItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiSourceDocuments_AiInboxItems_InboxItemId",
                table: "AiSourceDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AiSourceDocuments_InboxItemId",
                table: "AiSourceDocuments");

            migrationBuilder.DropColumn(
                name: "InboxItemId",
                table: "AiSourceDocuments");
        }
    }
}
