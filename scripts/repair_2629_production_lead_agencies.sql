-- ==============================================================================
-- SCRIPT CẬP NHẬT CHUẨN HÓA CƠ QUAN CHỦ TRÌ CHO CÁC MỤC TIÊU & NHIỆM VỤ QĐ 2629
-- ==============================================================================
-- Lý do: Bản seed cũ đưa 14 mục tiêu và nhiều nhiệm vụ về nhóm 'Các bộ, ngành, địa phương' (ALL_AGENCIES),
-- khiến bộ lọc Cơ quan chủ trì luôn trả về kết quả cũ và hiển thị 'Các bộ, ngành, địa phương'.
-- Script này ánh xạ đúng Cơ quan chủ trì theo Văn bản QĐ 2629/QĐ-TTg và cập nhật bảng liên kết phân công.
-- ==============================================================================

BEGIN;

-- 0. Đảm bảo có Ban Cơ yếu Chính phủ trong danh mục cơ quan
INSERT INTO "Agencies" ("Id", "Code", "Name", "Type", "IsActive", "CreatedAt", "ContactPersons", "PlanFiles", "AnnualPlans", "DigitalArchitecture", "FiveYearPlan")
SELECT gen_random_uuid(), 'BCYCP', 'Ban Cơ yếu Chính phủ', 'Other', true, NOW(), '[]', '[]', '[]', '', ''
WHERE NOT EXISTS (SELECT 1 FROM "Agencies" WHERE "Code" = 'BCYCP' OR "Name" ILIKE '%Ban Cơ yếu%');

-- 1. Cập nhật LeadAgencyId cho 14 Mục tiêu QĐ 2629
-- Văn phòng Chính phủ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'VPCP' OR "Name" ILIKE '%Văn phòng Chính phủ%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Goal'
  AND "Code" IN ('MT-01', 'MT-02', 'MT-05', 'MT-06', 'MT-07', 'MT-14', 'MT-15');

-- Bộ Khoa học và Công nghệ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BKHCN' OR "Name" ILIKE '%Bộ Khoa học và Công nghệ%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Goal'
  AND "Code" IN ('MT-03', 'MT-04', 'MT-11', 'MT-12', 'MT-19');

-- Bộ Nội vụ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BNV' OR "Name" ILIKE '%Bộ Nội vụ%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Goal'
  AND "Code" IN ('MT-09', 'MT-20');

-- 2. Cập nhật LeadAgencyId cho các Nhiệm vụ thuộc QĐ 2629
-- Văn phòng Chính phủ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'VPCP' OR "Name" ILIKE '%Văn phòng Chính phủ%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Task'
  AND "Code" IN ('NV-21', 'NV-23', 'NV-24', 'NV-25', 'NV-29', 'NV-30');

-- Bộ Khoa học và Công nghệ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BKHCN' OR "Name" ILIKE '%Bộ Khoa học và Công nghệ%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Task'
  AND "Code" IN ('NV-01', 'NV-02', 'NV-07', 'NV-08', 'NV-09', 'NV-10', 'NV-35', 'NV-40', 'NV-41', 'NV-54', 'NV-57', 'NV-65', 'NV-66', 'NV-69', 'NV-70', 'NV-71', 'NV-74', 'NV-75');

-- Bộ Tài chính
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BTC' OR "Name" ILIKE '%Bộ Tài chính%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Task'
  AND "Code" IN ('NV-13', 'NV-20', 'NV-72', 'NV-73');

-- Ban Cơ yếu Chính phủ
UPDATE "GoalTaskItems"
SET "LeadAgencyId" = (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BCYCP' OR "Name" ILIKE '%Ban Cơ yếu%' LIMIT 1),
    "IsGeneralTask" = false
WHERE "DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND "ItemType" = 'Task'
  AND "Code" IN ('NV-56');

-- 3. Đồng bộ lại AgencyTaskExecutions cho các mục tiêu/nhiệm vụ vừa cập nhật:
-- Xóa các bản ghi gán tràn toàn bộ 51 đơn vị (IsLeadGroupAssignment = true) của các mục tiêu/nhiệm vụ này
DELETE FROM "AgencyTaskExecutions" e
USING "GoalTaskItems" i
WHERE e."GoalTaskId" = i."Id"
  AND i."DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND i."Code" IN (
    'MT-01', 'MT-02', 'MT-03', 'MT-04', 'MT-05', 'MT-06', 'MT-07', 'MT-09', 'MT-11', 'MT-12', 'MT-14', 'MT-15', 'MT-19', 'MT-20',
    'NV-01', 'NV-02', 'NV-07', 'NV-08', 'NV-09', 'NV-10', 'NV-13', 'NV-20', 'NV-21', 'NV-23', 'NV-24', 'NV-25', 'NV-29', 'NV-30',
    'NV-35', 'NV-40', 'NV-41', 'NV-54', 'NV-56', 'NV-57', 'NV-65', 'NV-66', 'NV-69', 'NV-70', 'NV-71', 'NV-72', 'NV-73', 'NV-74', 'NV-75'
  )
  AND e."AgencyId" != i."LeadAgencyId";

-- Đảm bảo có bản ghi phân công chủ trì cho cơ quan chủ trì mới
INSERT INTO "AgencyTaskExecutions" ("Id", "GoalTaskId", "AgencyId", "IsLeadAssignment", "IsCoordinatingAssignment", "IsLeadGroupAssignment", "IsCoordinatingGroupAssignment", "AssignedAt", "CalculatedStatus", "CompletionPercentage", "ApprovalStatus", "CreatedAt", "UpdatedAt")
SELECT
    gen_random_uuid(),
    i."Id",
    i."LeadAgencyId",
    true,
    false,
    false,
    false,
    NOW(),
    'NotStarted',
    0,
    1,
    NOW(),
    NOW()
FROM "GoalTaskItems" i
WHERE i."DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND i."Code" IN (
    'MT-01', 'MT-02', 'MT-03', 'MT-04', 'MT-05', 'MT-06', 'MT-07', 'MT-09', 'MT-11', 'MT-12', 'MT-14', 'MT-15', 'MT-19', 'MT-20',
    'NV-01', 'NV-02', 'NV-07', 'NV-08', 'NV-09', 'NV-10', 'NV-13', 'NV-20', 'NV-21', 'NV-23', 'NV-24', 'NV-25', 'NV-29', 'NV-30',
    'NV-35', 'NV-40', 'NV-41', 'NV-54', 'NV-56', 'NV-57', 'NV-65', 'NV-66', 'NV-69', 'NV-70', 'NV-71', 'NV-72', 'NV-73', 'NV-74', 'NV-75'
  )
  AND NOT EXISTS (
    SELECT 1 FROM "AgencyTaskExecutions" ex WHERE ex."GoalTaskId" = i."Id" AND ex."AgencyId" = i."LeadAgencyId"
  );

-- Cập nhật cờ chủ trì cho bản ghi của chính LeadAgencyId nếu đã tồn tại
UPDATE "AgencyTaskExecutions" e
SET "IsLeadAssignment" = true,
    "IsLeadGroupAssignment" = false,
    "UpdatedAt" = NOW()
FROM "GoalTaskItems" i
WHERE e."GoalTaskId" = i."Id"
  AND i."DocumentId" = '26290000-0000-0000-0000-000000002629'
  AND e."AgencyId" = i."LeadAgencyId";

COMMIT;
