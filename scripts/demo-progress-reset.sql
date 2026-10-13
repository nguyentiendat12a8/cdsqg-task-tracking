-- PostgreSQL. Run the entire script while the backend is stopped.
-- Scope: ALL goals/tasks, ALL reporting years.
-- Do not edit goals, assignments, users or reference data during the demo.
-- CREATE SCHEMA deliberately fails if a previous backup exists.
BEGIN;
SET LOCAL lock_timeout = '10s';
LOCK TABLE public."GoalTaskItems", public."ProgressLogs",
    public."AgencyTaskExecutions", public."Notifications",
    public."DataImportLogs", public."TaskUrgeLogs" IN ACCESS EXCLUSIVE MODE;

CREATE SCHEMA demo_progress_backup_20261013;
CREATE TABLE demo_progress_backup_20261013.progress_logs AS
    TABLE public."ProgressLogs";
CREATE TABLE demo_progress_backup_20261013.agency_executions AS
    TABLE public."AgencyTaskExecutions";
CREATE TABLE demo_progress_backup_20261013.item_products AS
    SELECT "Id", "Deliverables", "AgencyDeliverables" FROM public."GoalTaskItems";
-- Preserve side effects (notifications, import audit, urges) for restoration.
CREATE TABLE demo_progress_backup_20261013.notifications AS TABLE public."Notifications";
CREATE TABLE demo_progress_backup_20261013.import_logs AS TABLE public."DataImportLogs";
CREATE TABLE demo_progress_backup_20261013.urge_logs AS TABLE public."TaskUrgeLogs";

DELETE FROM public."ProgressLogs";
UPDATE public."AgencyTaskExecutions"
SET "CalculatedStatus" = 'NotStarted',
    "ApprovalStatus" = 1, "RejectionReason" = NULL,
    "LatestProgressValue" = NULL, "LatestQualitativeStatus" = NULL,
    "CompletionPercentage" = 0, "Deliverables" = '[]'::jsonb,
    "SummaryNotes" = NULL, "AttachmentFileUrls" = '[]'::jsonb,
    "LastReportedAt" = NULL, "LastReportedBy" = NULL;

-- Keep product identity/title/deadline; clear only reported results/evidence.
-- JSON property names are PascalCase in the EF database converter.
UPDATE public."GoalTaskItems" i
SET "Deliverables" = COALESCE((
    SELECT jsonb_agg(p.value || jsonb_build_object(
        'CurrentStatus', 'NotStarted', 'DocumentNumber', NULL,
        'PromulgationDate', NULL, 'AttachmentUrl', NULL, 'AttachmentName', NULL)
        ORDER BY p.ordinality)
    FROM jsonb_array_elements(COALESCE(i."Deliverables", '[]'::jsonb))
        WITH ORDINALITY AS p(value, ordinality)
), '[]'::jsonb),
    "AgencyDeliverables" = '{}'::jsonb;

SELECT 'Backup complete; progress reset' AS result,
    (SELECT count(*) FROM demo_progress_backup_20261013.progress_logs) AS backed_up_reports,
    (SELECT count(*) FROM public."ProgressLogs") AS remaining_reports;
COMMIT;

-- Restart backend, refresh browser, then demo reporting/import/approval.
-- Physical uploaded files are deliberately left on disk.
