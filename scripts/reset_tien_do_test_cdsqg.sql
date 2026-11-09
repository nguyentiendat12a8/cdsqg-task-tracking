-- =============================================================================
--  RESET TRẠNG THÁI / TIẾN ĐỘ TEST CỦA MỤC TIÊU & NHIỆM VỤ  –  CDSQG (PostgreSQL)
--  Viết theo schema EF hiện tại (AppDbContextModelSnapshot, migration 20261102141453).
--
--  XÓA / ĐƯA VỀ BAN ĐẦU:
--    - ProgressLogs           : toàn bộ báo cáo tiến độ, minh chứng test
--    - AgencyTaskExecutions   : giữ dòng phân công, xóa kết quả báo cáo
--                               -> 'NotStarted', 0%, Approved, không minh chứng
--    - GoalTaskItems          : sản phẩm đầu ra -> 'NotStarted' (giữ tên, hạn),
--                               xóa sản phẩm theo đơn vị (AgencyDeliverables)
--    - TaskUrgeLogs           : lịch sử đôn đốc
--    - Notifications          : thông báo
--    - DataImportLogs         : lịch sử nạp file test
--    - AiGoalDataLinks        : bỏ liên kết tới ProgressLog (FK Restrict, nếu không
--                               sẽ chặn xóa ProgressLogs); giữ nguyên liên kết AI
--
--  GIỮ NGUYÊN: Documents, GoalTaskItems (danh sách mục tiêu/nhiệm vụ), TargetBaselines
--    (chỉ tiêu theo năm), phân công đơn vị (cột Is*Assignment, AssignedAgencyId),
--    Agencies, Users, Units, LegalDocuments, dữ liệu AI, tin tức, kế hoạch CĐS.
--
--  CÁCH CHẠY: chạy CẢ FILE một lần (một transaction). Lỗi ở bất kỳ bước nào
--  -> rollback toàn bộ, không có gì bị thay đổi.
--  Chạy trên DB local trước (localhost:5433 / cdsqg_db theo appsettings.json),
--  kiểm tra giao diện, rồi mới chạy trên Neon.
--  Trên Neon: nên tạo thêm branch sao lưu trước (Branches -> Create branch).
-- =============================================================================

BEGIN;
SET LOCAL lock_timeout = '10s';

LOCK TABLE public."GoalTaskItems", public."ProgressLogs", public."AgencyTaskExecutions",
           public."TaskUrgeLogs", public."Notifications", public."DataImportLogs",
           public."AiGoalDataLinks"
  IN ACCESS EXCLUSIVE MODE;

-- -----------------------------------------------------------------------------
-- 0. Sao lưu ngay trong DB (schema riêng). Nếu schema đã tồn tại, lệnh sẽ báo lỗi
--    và dừng – đổi tên schema (vd. thêm _2) rồi chạy lại.
-- -----------------------------------------------------------------------------
CREATE SCHEMA reset_backup_20261109;
CREATE TABLE reset_backup_20261109.progress_logs     AS TABLE public."ProgressLogs";
CREATE TABLE reset_backup_20261109.agency_executions AS TABLE public."AgencyTaskExecutions";
CREATE TABLE reset_backup_20261109.item_products     AS
    SELECT "Id", "Deliverables", "AgencyDeliverables" FROM public."GoalTaskItems";
CREATE TABLE reset_backup_20261109.urge_logs         AS TABLE public."TaskUrgeLogs";
CREATE TABLE reset_backup_20261109.notifications     AS TABLE public."Notifications";
CREATE TABLE reset_backup_20261109.import_logs       AS TABLE public."DataImportLogs";
CREATE TABLE reset_backup_20261109.ai_goal_links     AS TABLE public."AiGoalDataLinks";

-- -----------------------------------------------------------------------------
-- 1. Bỏ liên kết AI -> báo cáo tiến độ, rồi xóa báo cáo tiến độ
-- -----------------------------------------------------------------------------
UPDATE public."AiGoalDataLinks" SET "ProgressLogId" = NULL WHERE "ProgressLogId" IS NOT NULL;
DELETE FROM public."ProgressLogs";

-- -----------------------------------------------------------------------------
-- 2. Đưa kết quả thực hiện theo đơn vị về ban đầu (GIỮ dòng phân công)
--    ApprovalStatus: 1 = Approved (mặc định của hệ thống)
-- -----------------------------------------------------------------------------
UPDATE public."AgencyTaskExecutions"
SET "CalculatedStatus"        = 'NotStarted',
    "LatestProgressValue"     = NULL,
    "LatestQualitativeStatus" = NULL,
    "CompletionPercentage"    = 0,
    "ApprovalStatus"          = 1,
    "RejectionReason"         = NULL,
    "SummaryNotes"            = NULL,
    "Deliverables"            = '[]'::jsonb,
    "AttachmentFileUrls"      = '[]'::jsonb,
    "LastReportedAt"          = NULL,
    "LastReportedBy"          = NULL,
    "UpdatedAt"               = (now() AT TIME ZONE 'utc');

-- -----------------------------------------------------------------------------
-- 3. Sản phẩm đầu ra của mục tiêu/nhiệm vụ: giữ Id/tên/hạn, xóa kết quả & minh chứng
--    (tên thuộc tính JSON là PascalCase theo converter EF)
-- -----------------------------------------------------------------------------
UPDATE public."GoalTaskItems" i
SET "Deliverables" = COALESCE((
        SELECT jsonb_agg(p.value || jsonb_build_object(
                   'CurrentStatus', 'NotStarted', 'DocumentNumber', NULL,
                   'PromulgationDate', NULL, 'AttachmentUrl', NULL, 'AttachmentName', NULL)
               ORDER BY p.ordinality)
        FROM jsonb_array_elements(COALESCE(i."Deliverables"::jsonb, '[]'::jsonb))
             WITH ORDINALITY AS p(value, ordinality)
    ), '[]'::jsonb),
    "AgencyDeliverables" = '{}'::jsonb;

-- -----------------------------------------------------------------------------
-- 4. Xóa đôn đốc, thông báo, lịch sử nạp file test
-- -----------------------------------------------------------------------------
DELETE FROM public."TaskUrgeLogs";
DELETE FROM public."Notifications";
DELETE FROM public."DataImportLogs";

-- -----------------------------------------------------------------------------
-- 5. Kiểm tra kết quả (trước khi COMMIT)
-- -----------------------------------------------------------------------------
SELECT
  (SELECT count(*) FROM reset_backup_20261109.progress_logs)  AS bao_cao_da_sao_luu,
  (SELECT count(*) FROM public."ProgressLogs")                AS bao_cao_con_lai,
  (SELECT count(*) FROM public."AgencyTaskExecutions")        AS dong_phan_cong_giu_lai,
  (SELECT count(*) FROM public."AgencyTaskExecutions"
     WHERE "CalculatedStatus" <> 'NotStarted' OR "CompletionPercentage" <> 0) AS phan_cong_chua_reset,
  (SELECT count(*) FROM public."GoalTaskItems")               AS muc_tieu_nhiem_vu_giu_lai,
  (SELECT count(*) FROM public."TargetBaselines")             AS chi_tieu_nam_giu_lai,
  (SELECT count(*) FROM public."TaskUrgeLogs")                AS don_doc_con_lai,
  (SELECT count(*) FROM public."Notifications")               AS thong_bao_con_lai,
  (SELECT count(*) FROM public."DataImportLogs")              AS lich_su_nap_con_lai;

COMMIT;

-- =============================================================================
--  SAU KHI CHẠY
--  - Khởi động lại backend / tải lại trình duyệt; dashboard phải về "Chưa thực hiện".
--  - Sau đó mới đẩy dữ liệu chuẩn (vd. scripts/clean_and_dump_production_data.sql).
--  - File minh chứng đã upload trên đĩa/máy chủ KHÔNG bị xóa bởi script này.
--
--  HOÀN TÁC (nếu cần), chạy trong một transaction:
--    BEGIN;
--    DELETE FROM public."ProgressLogs";
--    INSERT INTO public."ProgressLogs"   SELECT * FROM reset_backup_20261109.progress_logs;
--    UPDATE public."AiGoalDataLinks" l SET "ProgressLogId" = b."ProgressLogId"
--      FROM reset_backup_20261109.ai_goal_links b
--      WHERE l."GoalTaskId" = b."GoalTaskId" AND l."SuggestionId" = b."SuggestionId";
--    DELETE FROM public."AgencyTaskExecutions";
--    INSERT INTO public."AgencyTaskExecutions" SELECT * FROM reset_backup_20261109.agency_executions;
--    UPDATE public."GoalTaskItems" i SET "Deliverables" = b."Deliverables",
--           "AgencyDeliverables" = b."AgencyDeliverables"
--      FROM reset_backup_20261109.item_products b WHERE i."Id" = b."Id";
--    INSERT INTO public."TaskUrgeLogs"   SELECT * FROM reset_backup_20261109.urge_logs;
--    INSERT INTO public."Notifications"  SELECT * FROM reset_backup_20261109.notifications;
--    INSERT INTO public."DataImportLogs" SELECT * FROM reset_backup_20261109.import_logs;
--    COMMIT;
--
--  Khi đã chắc chắn không cần hoàn tác:
--    DROP SCHEMA reset_backup_20261109 CASCADE;
-- =============================================================================
