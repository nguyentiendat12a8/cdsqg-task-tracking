-- ==============================================================================
-- SQL Script Reset Tiến Độ, Báo Cáo, Đôn Đốc & Thông Báo Hệ Thống CDSQG (PostgreSQL)
-- Giữ nguyên Danh sách Mục tiêu, Nhiệm vụ, Đơn vị, Người dùng.
-- ==============================================================================

BEGIN;

-- 1. Xóa toàn bộ lịch sử báo cáo tiến độ, minh chứng
DELETE FROM "ProgressLogs";

-- 2. Xóa toàn bộ lịch sử đôn đốc / nhắc nhở
DELETE FROM "TaskUrgeLogs";

-- 3. Xóa toàn bộ thông báo hệ thống
DELETE FROM "Notifications";

-- 4. Xóa kết quả thực thi theo đơn vị
DELETE FROM "AgencyTaskExecutions";

-- 5. Reset danh mục sản phẩm đầu ra theo từng đơn vị trên bảng GoalTaskItems
UPDATE "GoalTaskItems"
SET "AgencyDeliverables" = '{}'::jsonb;

-- 6. Reset trạng thái sản phẩm đầu ra chính về "NotStarted" (nếu có sản phẩm)
UPDATE "GoalTaskItems"
SET "Deliverables" = (
    SELECT jsonb_agg(
        elem 
        || jsonb_build_object('CurrentStatus', 'NotStarted')
        || jsonb_build_object('DocumentNumber', NULL)
        || jsonb_build_object('PromulgationDate', NULL)
        || jsonb_build_object('AttachmentUrl', NULL)
        || jsonb_build_object('AttachmentName', NULL)
    )
    FROM jsonb_array_elements("Deliverables") elem
)
WHERE "Deliverables" IS NOT NULL AND jsonb_array_length("Deliverables") > 0;

COMMIT;

SELECT 'Đã reset toàn bộ tiến độ, báo cáo, lịch sử đôn đốc và thông báo thành công!' AS result;
