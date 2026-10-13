-- PostgreSQL. Stop backend and run the entire script AFTER the demo.
-- Discards demo reports/executions and restores notifications/import/urge audit
-- to the snapshot. No concurrent non-demo work should take place.
-- Schema must remain unchanged between reset and restore.
BEGIN;
SET LOCAL lock_timeout = '10s';
LOCK TABLE public."GoalTaskItems", public."ProgressLogs",
    public."AgencyTaskExecutions", public."Notifications",
    public."DataImportLogs", public."TaskUrgeLogs" IN ACCESS EXCLUSIVE MODE;

-- Fail BEFORE removing anything if the complete backup is unavailable.
DO $$
BEGIN
    IF to_regclass('demo_progress_backup_20261013.progress_logs') IS NULL
       OR to_regclass('demo_progress_backup_20261013.agency_executions') IS NULL
       OR to_regclass('demo_progress_backup_20261013.item_products') IS NULL
       OR to_regclass('demo_progress_backup_20261013.notifications') IS NULL
       OR to_regclass('demo_progress_backup_20261013.import_logs') IS NULL
       OR to_regclass('demo_progress_backup_20261013.urge_logs') IS NULL THEN
        RAISE EXCEPTION 'Demo backup is missing or incomplete; restore cancelled';
    END IF;
    IF EXISTS (
        SELECT 1 FROM demo_progress_backup_20261013.item_products b
        LEFT JOIN public."GoalTaskItems" i ON i."Id" = b."Id"
        WHERE i."Id" IS NULL
    ) OR EXISTS (
        SELECT 1 FROM public."GoalTaskItems" i
        LEFT JOIN demo_progress_backup_20261013.item_products b ON b."Id" = i."Id"
        WHERE b."Id" IS NULL
    ) THEN
        RAISE EXCEPTION 'Goal/task list changed during demo; restore cancelled';
    END IF;
END $$;

DELETE FROM public."ProgressLogs";
DELETE FROM public."AgencyTaskExecutions";
DELETE FROM public."Notifications";
DELETE FROM public."DataImportLogs";
DELETE FROM public."TaskUrgeLogs";

UPDATE public."GoalTaskItems" i
SET "Deliverables" = b."Deliverables", "AgencyDeliverables" = b."AgencyDeliverables"
FROM demo_progress_backup_20261013.item_products b WHERE i."Id" = b."Id";

INSERT INTO public."AgencyTaskExecutions" SELECT * FROM demo_progress_backup_20261013.agency_executions;
INSERT INTO public."ProgressLogs" SELECT * FROM demo_progress_backup_20261013.progress_logs;
INSERT INTO public."Notifications" SELECT * FROM demo_progress_backup_20261013.notifications;
INSERT INTO public."DataImportLogs" SELECT * FROM demo_progress_backup_20261013.import_logs;
INSERT INTO public."TaskUrgeLogs" SELECT * FROM demo_progress_backup_20261013.urge_logs;

SELECT 'Restored pre-demo progress' AS result,
    (SELECT count(*) FROM public."ProgressLogs") AS restored_reports,
    (SELECT count(*) FROM demo_progress_backup_20261013.progress_logs) AS expected_reports;
COMMIT;

-- Restart backend and refresh browser. Backup remains available.
-- Do not delete physical evidence files: restored reports refer to them.
