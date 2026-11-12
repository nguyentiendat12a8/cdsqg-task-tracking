-- Bổ sung nhóm (cột "Group") cho 6 mục của QĐ 1266 đang để trống nhóm.
-- Chạy trên DB local trước; kiểm tra kết quả SELECT cuối rồi mới COMMIT.
BEGIN;

UPDATE "GoalTaskItems" g SET "Group" = v.grp
FROM "Documents" d, (VALUES
  ('Goal', 'MT-25', 'Mục tiêu đến năm 2045'),
  ('Goal', 'MT-26', 'Mục tiêu đến năm 2045'),
  ('Task', 'NV-02', 'Nhóm I: Hoàn thiện thể chế số'),
  ('Task', 'NV-04', 'Nhóm I: Hoàn thiện thể chế số'),
  ('Task', 'NV-14', 'Nhóm II: Phát triển hạ tầng số'),
  ('Task', 'NV-25', 'Nhóm III: Phát triển dữ liệu số, nền tảng số dùng chung')
) AS v(item_type, original_code, grp)
WHERE d."Id" = g."DocumentId" AND d."DocumentNumber" LIKE '1266%'
  AND g."ItemType" = v.item_type AND g."OriginalCode" = v.original_code
  AND coalesce(btrim(g."Group"), '') = '';

SELECT g."ItemType", g."OriginalCode", g."Code", g."Group"
FROM "GoalTaskItems" g JOIN "Documents" d ON d."Id" = g."DocumentId"
WHERE d."DocumentNumber" LIKE '1266%' AND g."OriginalCode" IN ('MT-25','MT-26','NV-02','NV-04','NV-14','NV-25')
ORDER BY 1, 2;

-- Đúng 6 dòng và đã có nhóm thì chạy: COMMIT;   Sai thì: ROLLBACK;
