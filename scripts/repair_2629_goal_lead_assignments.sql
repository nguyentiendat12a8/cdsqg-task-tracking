-- PostgreSQL. Run this WHOLE file in a new session, outside any existing transaction.
-- PREVIEW by default. For applying, change ONLY VALUES (false) below to VALUES (true).
-- Scope: 22 seeded GOALS of decision 2629, not Tasks, 1266, plans, reports or approvals.
BEGIN;
SET LOCAL lock_timeout='5s';
SET LOCAL statement_timeout='60s';
CREATE TEMP TABLE _r2629_options(apply_changes boolean NOT NULL) ON COMMIT DROP;
INSERT INTO _r2629_options VALUES (false);
CREATE TEMP TABLE _r2629_expected(code text PRIMARY KEY,agency_code text NOT NULL,agency_name text NOT NULL) ON COMMIT DROP;
INSERT INTO _r2629_expected VALUES
('MT-01','VPCP','Văn phòng Chính phủ'),
('MT-02','VPCP','Văn phòng Chính phủ'),
('MT-03','BKHCN','Bộ Khoa học và Công nghệ'),
('MT-04','BKHCN','Bộ Khoa học và Công nghệ'),
('MT-05','VPCP','Văn phòng Chính phủ'),
('MT-06','VPCP','Văn phòng Chính phủ'),
('MT-07','VPCP','Văn phòng Chính phủ'),
('MT-08','BCA','Bộ Công an'),
('MT-09','BNV','Bộ Nội vụ'),
('MT-10','BCA','Bộ Công an'),
('MT-11','BKHCN','Bộ Khoa học và Công nghệ'),
('MT-12','BKHCN','Bộ Khoa học và Công nghệ'),
('MT-13','BCA','Bộ Công an'),
('MT-14','VPCP','Văn phòng Chính phủ'),
('MT-15','VPCP','Văn phòng Chính phủ'),
('MT-16','BCA','Bộ Công an'),
('MT-17','BCA','Bộ Công an'),
('MT-18','BCA','Bộ Công an'),
('MT-19','BKHCN','Bộ Khoa học và Công nghệ'),
('MT-20','BNV','Bộ Nội vụ'),
('MT-21','BCA','Bộ Công an'),
('MT-22','BCA','Bộ Công an');
DO $preflight$
DECLARE doc_count int; agency_count int; agency record;
BEGIN
 SELECT count(*) INTO doc_count FROM "Documents" WHERE "DocumentNumber" ~ '^2629([/[:space:]]|$)';
 IF doc_count<>1 THEN RAISE EXCEPTION 'Expected exactly one document numbered 2629; found %',doc_count; END IF;
 FOR agency IN SELECT DISTINCT agency_code,agency_name FROM _r2629_expected LOOP
  SELECT count(*) INTO agency_count FROM "Agencies" a WHERE upper(btrim(a."Code"))=agency.agency_code OR lower(btrim(a."Name"))=lower(agency.agency_name);
  IF agency_count<>1 THEN RAISE EXCEPTION 'Agency % missing or ambiguous (% matches). No updates applied.',agency.agency_code,agency_count; END IF;
  IF EXISTS(SELECT 1 FROM "Agencies" a WHERE (upper(btrim(a."Code"))=agency.agency_code OR lower(btrim(a."Name"))=lower(agency.agency_name)) AND NOT a."IsActive") THEN RAISE EXCEPTION 'Expected agency % is inactive',agency.agency_code; END IF;
 END LOOP;
 IF (SELECT apply_changes FROM _r2629_options) THEN
  PERFORM i."Id" FROM "GoalTaskItems" i JOIN "Documents" d ON d."Id"=i."DocumentId" WHERE d."DocumentNumber" ~ '^2629([/[:space:]]|$)' AND i."ItemType"='Goal' FOR UPDATE OF i;
  PERFORM e."Id" FROM "AgencyTaskExecutions" e JOIN "GoalTaskItems" i ON i."Id"=e."GoalTaskId" JOIN "Documents" d ON d."Id"=i."DocumentId" WHERE d."DocumentNumber" ~ '^2629([/[:space:]]|$)' AND i."ItemType"='Goal' FOR UPDATE OF e;
 END IF;
END $preflight$;
CREATE TEMP TABLE _r2629_scope ON COMMIT DROP AS
SELECT i."Id" AS item_id,i."Code" AS item_code,i."LeadAgencyId" AS old_lead_id,
 i."IsGeneralTask" AS old_general,i."AssignedAgencyId" AS second_lead_id,
 old."Code" AS old_lead_code,i."LeadAgencyDisplayText" AS display_text,
 expected.agency_code,expected.agency_name,a."Id" AS target_id,
 CASE
 WHEN lower(btrim(COALESCE(i."LeadAgencyDisplayText",'')))<>lower(expected.agency_name) THEN 'SKIP_DISPLAY_CHANGED'
 WHEN i."AssignedAgencyId" IS NOT NULL THEN 'SKIP_SECOND_LEAD'
 WHEN i."LeadAgencyId"<>a."Id" AND upper(COALESCE(old."Code",''))<>'ALL_AGENCIES' THEN 'SKIP_MANUAL_LEAD'
 WHEN i."LeadAgencyId"=a."Id" AND NOT i."IsGeneralTask" THEN 'ALREADY_CORRECT'
 ELSE 'REPAIR' END AS action
FROM "GoalTaskItems" i JOIN "Documents" d ON d."Id"=i."DocumentId"
JOIN _r2629_expected expected ON expected.code=regexp_replace(i."Code",'^MT-2629-','MT-')
JOIN "Agencies" a ON upper(btrim(a."Code"))=expected.agency_code OR lower(btrim(a."Name"))=lower(expected.agency_name)
LEFT JOIN "Agencies" old ON old."Id"=i."LeadAgencyId"
WHERE d."DocumentNumber" ~ '^2629([/[:space:]]|$)' AND i."ItemType"='Goal';
DO $guard$
BEGIN
 IF (SELECT count(*) FROM _r2629_scope)<>22 OR (SELECT count(DISTINCT item_code) FROM _r2629_scope)<>22 THEN RAISE EXCEPTION 'Expected exactly the 22 seeded goals MT-01..MT-22. No updates applied.'; END IF;
 IF EXISTS(SELECT 1 FROM _r2629_scope s WHERE s.action='REPAIR' AND NOT EXISTS(SELECT 1 FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId"=s.item_id AND e."AgencyId"=s.target_id)) THEN
  RAISE EXCEPTION 'Missing execution row for an expected agency. Stop for manual review; no updates applied.';
 END IF;
END $guard$;
-- Review this output BEFORE enabling apply_changes.
SELECT s.item_code,s.display_text,s.old_lead_code,s.old_general,s.agency_code AS expected_lead,s.action,
 (SELECT count(*) FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId"=s.item_id AND e."IsLeadAssignment") AS current_lead_recipients,
 (SELECT count(*) FROM "ProgressLogs" l WHERE l."GoalTaskId"=s.item_id) AS reports_kept
FROM _r2629_scope s ORDER BY s.item_code;
CREATE TEMP TABLE _r2629_unchanged ON COMMIT DROP AS
SELECT 'plans_and_item_data' AS kind,i."Id" AS id,to_jsonb(i)-'LeadAgencyId'-'IsGeneralTask' AS payload
FROM "GoalTaskItems" i JOIN _r2629_scope s ON s.item_id=i."Id"
UNION ALL SELECT 'execution_data',e."Id",to_jsonb(e)-'IsLeadAssignment'-'IsLeadGroupAssignment'
FROM "AgencyTaskExecutions" e JOIN _r2629_scope s ON s.item_id=e."GoalTaskId"
UNION ALL SELECT 'reports',l."Id",to_jsonb(l) FROM "ProgressLogs" l JOIN _r2629_scope s ON s.item_id=l."GoalTaskId";
DO $apply$
BEGIN
 IF NOT (SELECT apply_changes FROM _r2629_options) THEN RAISE NOTICE 'PREVIEW ONLY: no persistent data changed. Set apply_changes=true to apply.'; RETURN; END IF;
 UPDATE "GoalTaskItems" i SET "LeadAgencyId"=s.target_id,"IsGeneralTask"=false FROM _r2629_scope s WHERE i."Id"=s.item_id AND s.action='REPAIR';
 -- Withdraw incorrect LEAD roles, keeping rows, coordination roles and all report/cache data.
 UPDATE "AgencyTaskExecutions" e SET "IsLeadAssignment"=(e."AgencyId"=s.target_id),"IsLeadGroupAssignment"=false
 FROM _r2629_scope s WHERE e."GoalTaskId"=s.item_id AND s.action='REPAIR';
 IF EXISTS(SELECT 1 FROM _r2629_scope s WHERE s.action='REPAIR' AND (
  (SELECT count(*) FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId"=s.item_id AND e."IsLeadAssignment")<>1 OR
  NOT EXISTS(SELECT 1 FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId"=s.item_id AND e."AgencyId"=s.target_id AND e."IsLeadAssignment" AND NOT e."IsLeadGroupAssignment"))) THEN RAISE EXCEPTION 'Assignment verification failed; transaction must roll back.'; END IF;
 IF EXISTS(
  SELECT 1 FROM _r2629_unchanged b
  LEFT JOIN "GoalTaskItems" i ON b.kind='plans_and_item_data' AND i."Id"=b.id
  LEFT JOIN "AgencyTaskExecutions" e ON b.kind='execution_data' AND e."Id"=b.id
  LEFT JOIN "ProgressLogs" l ON b.kind='reports' AND l."Id"=b.id
  WHERE b.payload IS DISTINCT FROM CASE b.kind WHEN 'plans_and_item_data' THEN to_jsonb(i)-'LeadAgencyId'-'IsGeneralTask' WHEN 'execution_data' THEN to_jsonb(e)-'IsLeadAssignment'-'IsLeadGroupAssignment' ELSE to_jsonb(l) END
 ) THEN RAISE EXCEPTION 'Plans, reports or execution history changed unexpectedly; transaction must roll back.'; END IF;
END $apply$;
SELECT CASE WHEN (SELECT apply_changes FROM _r2629_options) THEN 'APPLIED' ELSE 'PREVIEW' END AS mode,
 count(*) FILTER(WHERE action='REPAIR') AS legacy_goals_to_repair,
 count(*) FILTER(WHERE action='ALREADY_CORRECT') AS already_correct,
 count(*) FILTER(WHERE action LIKE 'SKIP_%') AS preserved_manual_rows FROM _r2629_scope;
COMMIT;
