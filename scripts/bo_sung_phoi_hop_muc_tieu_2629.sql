-- =============================================================================
-- BỔ SUNG CƠ QUAN PHỐI HỢP CHO 21 CHỈ TIÊU (MỤC TIÊU) CỦA QĐ 2629/QĐ-TTg
--
-- Phụ lục I của QĐ 2629 không có cột cơ quan phối hợp. Theo phương án đã duyệt (15/11/2026):
--   - 21 chỉ tiêu: phối hợp = "Các bộ, ngành, địa phương" (ALL_AGENCIES).
--   - Riêng STT gốc MT-04 (nền tảng số quốc gia, nền tảng số dùng chung): KHÔNG có cơ quan phối hợp.
--
-- Script làm đúng như khi sửa "Cơ quan phối hợp" trên giao diện (AgencyAssignments.SyncAsync):
--   1. Ghi CoordinatingAgencyIds + tên hiển thị cho mục tiêu.
--   2. Tạo/cập nhật dòng phân giao phối hợp cho từng bộ, tỉnh thành viên của nhóm.
-- Không đụng tới chủ trì, báo cáo, kế hoạch. Chỉ áp dụng cho mục tiêu đang chưa có cơ quan phối hợp,
-- nên chạy lại nhiều lần vẫn an toàn.
--
-- CÁCH CHẠY: chạy toàn bộ file trên DB local trước. Script tự kiểm tra phân giao rồi COMMIT; có lỗi sẽ không lưu.
-- =============================================================================
BEGIN;

CREATE TEMP TABLE _ph2629 ON COMMIT DROP AS
SELECT g."Id" AS goal_id, g."OriginalCode", g."Code"
FROM "GoalTaskItems" g
JOIN "Documents" d ON d."Id" = g."DocumentId"
WHERE d."DocumentNumber" LIKE '2629%' AND g."ItemType" = 'Goal'
  AND g."OriginalCode" ~ '^MT-(0[1-9]|1[0-9]|2[0-2])$' AND g."OriginalCode" <> 'MT-04'
  AND jsonb_array_length(coalesce(g."CoordinatingAgencyIds", '[]'::jsonb)) = 0;

DO $$
BEGIN
  IF (SELECT count(*) FROM "Agencies" WHERE "Code" = 'ALL_AGENCIES') <> 1 THEN
    RAISE EXCEPTION 'Cần đúng 1 danh mục ALL_AGENCIES (Các bộ, ngành, địa phương).';
  END IF;
END $$;

-- Thành viên nhóm "Các bộ, ngành, địa phương": bộ, tỉnh đang hoạt động, không phải đơn vị trực thuộc, không phải nhóm.
CREATE TEMP TABLE _tv ON COMMIT DROP AS
SELECT a."Id" AS agency_id
FROM "Agencies" a
WHERE a."IsActive" AND a."ParentId" IS NULL
  AND a."Type" IN ('Ministry', 'Province')
  AND upper(a."Code") NOT LIKE 'ALL\_%'
  AND lower(btrim(a."Name")) NOT IN ('các doanh nghiệp công nghệ số', 'các doanh nghiệp viễn thông');

-- 1. Ghi cơ quan phối hợp cho mục tiêu
UPDATE "GoalTaskItems" g
SET "CoordinatingAgencyIds" = jsonb_build_array((SELECT "Id" FROM "Agencies" WHERE "Code" = 'ALL_AGENCIES')),
    "CoordinatingAgenciesDisplayText" = 'Các bộ, ngành, địa phương'
FROM _ph2629 p WHERE p.goal_id = g."Id";

-- 2a. Dòng phân giao đã có (ví dụ cơ quan chủ trì): bật thêm vai trò phối hợp
UPDATE "AgencyTaskExecutions" e
SET "IsCoordinatingAssignment" = TRUE, "IsCoordinatingGroupAssignment" = TRUE,
    "AssignedAt" = CASE WHEN NOT e."IsLeadAssignment" AND NOT e."IsCoordinatingAssignment" THEN now() AT TIME ZONE 'utc' ELSE e."AssignedAt" END
FROM _ph2629 p, _tv t
WHERE e."GoalTaskId" = p.goal_id AND e."AgencyId" = t.agency_id;

-- 2b. Dòng phân giao chưa có: tạo mới với vai trò phối hợp
INSERT INTO "AgencyTaskExecutions"
  ("Id", "GoalTaskId", "AgencyId", "IsLeadAssignment", "IsCoordinatingAssignment", "IsLeadGroupAssignment", "IsCoordinatingGroupAssignment",
   "AssignedAt", "CalculatedStatus", "ApprovalStatus", "CompletionPercentage", "Deliverables", "AttachmentFileUrls", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), p.goal_id, t.agency_id, FALSE, TRUE, FALSE, TRUE,
       now() AT TIME ZONE 'utc', 'NotStarted', 1, 0, '[]'::jsonb, '[]'::jsonb, now() AT TIME ZONE 'utc', now() AT TIME ZONE 'utc'
FROM _ph2629 p CROSS JOIN _tv t
WHERE NOT EXISTS (SELECT 1 FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId" = p.goal_id AND e."AgencyId" = t.agency_id);

-- KIỂM TRA 1: 22 mục tiêu 2629. Mong đợi: MT-04 trống phối hợp và 0 dòng phối hợp; 21 mục còn lại có số dòng phối hợp = số bộ, tỉnh (51).
SELECT g."OriginalCode" AS stt_goc, g."Code" AS ma, coalesce(nullif(g."CoordinatingAgenciesDisplayText", ''), '(trống)') AS phoi_hop,
       (SELECT count(*) FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId" = g."Id" AND e."IsCoordinatingAssignment") AS so_dong_phoi_hop,
       (SELECT count(*) FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId" = g."Id" AND e."IsLeadAssignment") AS so_dong_chu_tri
FROM "GoalTaskItems" g JOIN "Documents" d ON d."Id" = g."DocumentId"
WHERE d."DocumentNumber" LIKE '2629%' AND g."ItemType" = 'Goal'
ORDER BY g."OriginalCode";

-- KIỂM TRA 2: số mục tiêu vừa cập nhật (mong đợi 21 ở lần chạy đầu) và số thành viên nhóm (mong đợi 51).
SELECT (SELECT count(*) FROM _ph2629) AS so_muc_tieu_cap_nhat, (SELECT count(*) FROM _tv) AS so_bo_tinh;

-- Chỉ lưu khi toàn bộ mục tiêu vừa cập nhật có đủ phân giao phối hợp.
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM _tv) THEN
    RAISE EXCEPTION 'Không có thành viên nhóm cơ quan phối hợp; chưa lưu dữ liệu.';
  END IF;
  IF EXISTS (
    SELECT 1 FROM _ph2629 p
    WHERE EXISTS (
      SELECT 1 FROM _tv t WHERE NOT EXISTS (
        SELECT 1 FROM "AgencyTaskExecutions" e
        WHERE e."GoalTaskId" = p.goal_id AND e."AgencyId" = t.agency_id
          AND e."IsCoordinatingAssignment" AND e."IsCoordinatingGroupAssignment"
      )
    )
  ) THEN
    RAISE EXCEPTION 'Phân giao phối hợp chưa đầy đủ; chưa lưu dữ liệu.';
  END IF;
END $$;
COMMIT;
