using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiInboxNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "AiInboxNumbers");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                table: "AiInboxItems",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('\"AiInboxNumbers\"')");

            // Backfill deterministically without changing existing IDs or workflow state.
            migrationBuilder.Sql("""
                WITH numbered AS (
                    SELECT "Id", row_number() OVER (ORDER BY "ReceivedAt", "Id") AS n FROM "AiInboxItems"
                ) UPDATE "AiInboxItems" i SET "SequenceNumber" = n.n FROM numbered n WHERE i."Id" = n."Id";
                SELECT setval('"AiInboxNumbers"', COALESCE(MAX("SequenceNumber"), 0) + 1, false) FROM "AiInboxItems";
                """);
            migrationBuilder.CreateIndex(
                name: "IX_AiInboxItems_SequenceNumber",
                table: "AiInboxItems",
                column: "SequenceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiInboxItems_SequenceNumber",
                table: "AiInboxItems");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "AiInboxItems");

            migrationBuilder.DropSequence(
                name: "AiInboxNumbers");
        }
    }
}
