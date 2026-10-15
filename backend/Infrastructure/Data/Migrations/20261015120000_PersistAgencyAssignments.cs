using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cdsqg.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261015120000_PersistAgencyAssignments")]
public class PersistAgencyAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>("IsLeadAssignment", "AgencyTaskExecutions", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("IsCoordinatingAssignment", "AgencyTaskExecutions", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("IsLeadGroupAssignment", "AgencyTaskExecutions", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("IsCoordinatingGroupAssignment", "AgencyTaskExecutions", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTime>("AssignedAt", "AgencyTaskExecutions", type: "timestamp without time zone", nullable: true);
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_AgencyTaskExecutions_AgencyId\";");
        migrationBuilder.CreateIndex("IX_Execution_Agency_Lead_Task", "AgencyTaskExecutions", new[] { "AgencyId", "IsLeadAssignment", "GoalTaskId" });
        migrationBuilder.CreateIndex("IX_Execution_Agency_Coordinating_Task", "AgencyTaskExecutions", new[] { "AgencyId", "IsCoordinatingAssignment", "GoalTaskId" });
        // Set-based backfill. Never overwrite reports, approval decisions or execution IDs/caches.
        migrationBuilder.Sql("""
            WITH sources AS (
                SELECT "Id" AS task_id, "LeadAgencyId" AS source_id, true AS lead FROM "GoalTaskItems"
                UNION ALL SELECT "Id", "AssignedAgencyId", true FROM "GoalTaskItems" WHERE "AssignedAgencyId" IS NOT NULL
                UNION ALL SELECT i."Id", c.id::uuid, false FROM "GoalTaskItems" i
                    CROSS JOIN LATERAL jsonb_array_elements_text(i."CoordinatingAgencyIds") c(id)
            ), recipients AS (
                SELECT s.task_id, a."Id" AS agency_id, s.lead, false AS via_group
                FROM sources s JOIN "Agencies" a ON a."Id" = s.source_id
                WHERE a."Type" <> 'Special' AND upper(a."Code") NOT LIKE 'ALL\_%' ESCAPE '\'
                UNION ALL
                SELECT s.task_id, a."Id", s.lead, true
                FROM sources s JOIN "Agencies" g ON g."Id" = s.source_id
                JOIN "Agencies" a ON a."IsActive" AND a."ParentId" IS NULL
                    AND a."Type" IN ('Ministry','Province') AND upper(a."Code") NOT LIKE 'ALL\_%' ESCAPE '\'
                WHERE upper(g."Code") = 'ALL_AGENCIES'
                    OR upper(g."Code") IN ('ALL_MINISTRIES','ALL_MINISTRIES_DIRECT') AND a."Type" = 'Ministry'
                    OR upper(g."Code") IN ('ALL_PROVINCES','ALL_PROVINCES_UBND') AND a."Type" = 'Province'
            ), assignments AS (
                SELECT task_id, agency_id, bool_or(lead) AS lead, bool_or(NOT lead) AS coordination, bool_or(via_group AND lead) AS lead_group, bool_or(via_group AND NOT lead) AS coordination_group
                FROM recipients GROUP BY task_id, agency_id
            )
            INSERT INTO "AgencyTaskExecutions" ("Id", "GoalTaskId", "AgencyId", "IsLeadAssignment", "IsCoordinatingAssignment", "IsLeadGroupAssignment", "IsCoordinatingGroupAssignment", "AssignedAt", "CalculatedStatus", "ApprovalStatus", "CompletionPercentage", "Deliverables", "AttachmentFileUrls", "CreatedAt", "UpdatedAt")
            SELECT gen_random_uuid(), task_id, agency_id, lead, coordination, lead_group, coordination_group, (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
                'NotStarted', 1, 0, '[]'::jsonb, '[]'::jsonb, (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'), (CURRENT_TIMESTAMP AT TIME ZONE 'UTC') FROM assignments
            ON CONFLICT ("GoalTaskId", "AgencyId") DO UPDATE SET
                "IsLeadAssignment" = EXCLUDED."IsLeadAssignment",
                "IsCoordinatingAssignment" = EXCLUDED."IsCoordinatingAssignment",
                "IsLeadGroupAssignment" = EXCLUDED."IsLeadGroupAssignment",
                "IsCoordinatingGroupAssignment" = EXCLUDED."IsCoordinatingGroupAssignment",
                "AssignedAt" = COALESCE("AgencyTaskExecutions"."AssignedAt", EXCLUDED."AssignedAt");
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_Execution_Agency_Lead_Task", "AgencyTaskExecutions");
        migrationBuilder.DropIndex("IX_Execution_Agency_Coordinating_Task", "AgencyTaskExecutions");
        migrationBuilder.CreateIndex("IX_AgencyTaskExecutions_AgencyId", "AgencyTaskExecutions", "AgencyId");
        foreach (var name in new[] { "IsLeadAssignment", "IsCoordinatingAssignment", "IsLeadGroupAssignment", "IsCoordinatingGroupAssignment", "AssignedAt" }) migrationBuilder.DropColumn(name, "AgencyTaskExecutions");
    }
}
