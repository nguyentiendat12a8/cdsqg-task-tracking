using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cdsqg.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedDigitalPlanCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Merge only the active settings; historical read snapshots and results remain intact.
            migrationBuilder.Sql("""
                WITH original AS (
                    SELECT "Id", COALESCE(NULLIF(btrim("EvaluationCriteriaJson"),''),'{}')::jsonb AS j
                    FROM "DigitalNewsSyncSettings"
                )
                UPDATE "DigitalNewsSyncSettings" AS settings
                SET "EvaluationCriteriaJson" = jsonb_set(
                    original.j - 'ArchitectureKeywords' - 'Architecture' - 'Annual' - 'FiveYear',
                    '{RelatedKeywords}',
                    to_jsonb(COALESCE((
                        SELECT string_agg(merged.term, E'\n' ORDER BY merged.ord)
                        FROM (
                            SELECT DISTINCT ON (lower(btrim(parts.term))) btrim(parts.term) AS term, parts.ord
                            FROM regexp_split_to_table(
                                COALESCE(original.j->>'RelatedKeywords', E'chuyển đổi số\nkhung kiến trúc số\nkhung kiến trúc chính phủ số\nkhung kiến trúc chính quyền số')
                                || E'\n' || COALESCE(original.j->>'ArchitectureKeywords',''), E'\r?\n'
                            ) WITH ORDINALITY AS parts(term,ord)
                            WHERE btrim(parts.term) <> ''
                            ORDER BY lower(btrim(parts.term)), parts.ord
                        ) AS merged
                    ), ''))
                )::text
                FROM original WHERE settings."Id"=original."Id";
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
