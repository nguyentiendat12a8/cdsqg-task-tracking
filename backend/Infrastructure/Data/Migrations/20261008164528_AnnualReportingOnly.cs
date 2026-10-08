using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AnnualReportingOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Back up the database before applying: retired reports and child items are intentionally deleted.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE retired_child_items ON COMMIT DROP AS
                    SELECT "Id" FROM "GoalTaskItems" WHERE "ParentId" IS NOT NULL;
                DELETE FROM "Notifications" n WHERE EXISTS
                    (SELECT 1 FROM retired_child_items c WHERE n."LinkUrl" LIKE '%' || c."Id"::text || '%');
                DELETE FROM "TaskUrgeLogs" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items)
                    OR "GoalTaskItemId" IN (SELECT "Id" FROM retired_child_items);
                DELETE FROM "AgencyTaskExecutions" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
                DELETE FROM "ProgressLogs" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
                DELETE FROM "TargetBaselines" WHERE "GoalTaskId" IN (SELECT "Id" FROM retired_child_items);
                DELETE FROM "GoalTaskItems" WHERE "Id" IN (SELECT "Id" FROM retired_child_items);
                DELETE FROM "ProgressLogs" WHERE "PeriodQuarter" <> 0;
                DELETE FROM "TargetBaselines" WHERE "Quarter" <> 0;
                UPDATE "GoalTaskItems" i SET "CustomBaseline" = COALESCE(
                    (SELECT jsonb_object_agg(key, value) FROM jsonb_each(i."CustomBaseline")
                        WHERE key ~ '^[0-9]{4}$'), '{}'::jsonb), "AgencyDeliverables" = '{}'::jsonb;
                UPDATE "AgencyTaskExecutions" SET "CompletionPercentage" = 0,
                    "LatestProgressValue" = NULL, "LatestQualitativeStatus" = NULL,
                    "CalculatedStatus" = 'NotStarted', "Deliverables" = '[]'::jsonb,
                    "SummaryNotes" = NULL, "AttachmentFileUrls" = '[]'::jsonb;
                """);
            // Legacy installations may have the columns but not the self-reference or its index.
            migrationBuilder.Sql("""
                ALTER TABLE "GoalTaskItems" DROP CONSTRAINT IF EXISTS "FK_GoalTaskItems_GoalTaskItems_ParentId";
                DROP INDEX IF EXISTS "IX_TargetBaselines_GoalTaskId_Year_Quarter";
                DROP INDEX IF EXISTS "IX_GoalTaskItems_ParentId";
                """);

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "TargetBaselines");

            migrationBuilder.DropColumn(
                name: "PeriodQuarter",
                table: "ProgressLogs");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "GoalTaskItems");

            migrationBuilder.CreateIndex(
                name: "IX_TargetBaselines_GoalTaskId_Year",
                table: "TargetBaselines",
                columns: new[] { "GoalTaskId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TargetBaselines_GoalTaskId_Year",
                table: "TargetBaselines");

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "TargetBaselines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeriodQuarter",
                table: "ProgressLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "GoalTaskItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetBaselines_GoalTaskId_Year_Quarter",
                table: "TargetBaselines",
                columns: new[] { "GoalTaskId", "Year", "Quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalTaskItems_ParentId",
                table: "GoalTaskItems",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalTaskItems_GoalTaskItems_ParentId",
                table: "GoalTaskItems",
                column: "ParentId",
                principalTable: "GoalTaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
