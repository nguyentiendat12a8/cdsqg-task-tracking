using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DigitalArchitectureMonitoringDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only fill missing architecture fields; never replace the user's existing rules.
            migrationBuilder.Sql("""
                UPDATE "DigitalNewsSyncSettings"
                SET "EvaluationCriteriaJson" = (
                    CASE WHEN COALESCE(NULLIF(btrim("EvaluationCriteriaJson"),''),'{}')::jsonb ? 'Architecture'
                         THEN COALESCE(NULLIF(btrim("EvaluationCriteriaJson"),''),'{}')::jsonb
                         ELSE COALESCE(NULLIF(btrim("EvaluationCriteriaJson"),''),'{}')::jsonb || '{"Architecture":true}'::jsonb END
                    || CASE WHEN btrim(COALESCE(COALESCE(NULLIF(btrim("EvaluationCriteriaJson"),''),'{}')::jsonb->>'ArchitectureKeywords','')) = ''
                            THEN jsonb_build_object('ArchitectureKeywords', E'khung kiến trúc số\nkhung kiến trúc chính phủ số\nkhung kiến trúc chính quyền số')
                            ELSE '{}'::jsonb END
                )::text;
                INSERT INTO "DigitalPlanKeywords" ("Id","Conditions","Enabled","Name","Phrase","SortOrder")
                SELECT 'ef2ab949-f006-578c-9892-51a70614fc2f'::uuid,'',true,'Khung kiến trúc số','khung kiến trúc chính phủ số',22
                WHERE NOT EXISTS (SELECT 1 FROM "DigitalPlanKeywords" WHERE lower(btrim("Phrase"))='khung kiến trúc chính phủ số' OR "Id"='ef2ab949-f006-578c-9892-51a70614fc2f'::uuid);
                INSERT INTO "DigitalPlanKeywords" ("Id","Conditions","Enabled","Name","Phrase","SortOrder")
                SELECT 'effd0408-b6a6-5ac5-a00c-39b775f401c4'::uuid,'',true,'Khung kiến trúc số','khung kiến trúc chính quyền số',23
                WHERE NOT EXISTS (SELECT 1 FROM "DigitalPlanKeywords" WHERE lower(btrim("Phrase"))='khung kiến trúc chính quyền số' OR "Id"='effd0408-b6a6-5ac5-a00c-39b775f401c4'::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DigitalPlanKeywords",
                keyColumn: "Id",
                keyValue: new Guid("ef2ab949-f006-578c-9892-51a70614fc2f"));

            migrationBuilder.DeleteData(
                table: "DigitalPlanKeywords",
                keyColumn: "Id",
                keyValue: new Guid("effd0408-b6a6-5ac5-a00c-39b775f401c4"));
        }
    }
}
