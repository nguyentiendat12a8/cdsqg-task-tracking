-- =============================================================================
-- ĐỔI MÃ MỤC TIÊU, NHIỆM VỤ THEO CƠ QUAN CHỦ TRÌ (QĐ 2629, QĐ 1266 và các văn bản khác)
--
--   Một cơ quan chủ trì       : MT-BCA-01, NV-BKHCN-03 ... (viết tắt lấy từ Agencies.ShortCode)
--   Giao chung / nhiều cơ quan: MT-C-01, NV-C-01 ...
--     (nhiệm vụ chung, chủ trì là nhóm "Các bộ, ngành, địa phương"..., có cơ quan đồng chủ trì,
--      hoặc cơ quan chủ trì chưa có viết tắt như "Các doanh nghiệp công nghệ số", "Đài THVN, Đài TNVN, TTXVN, Báo Nhân dân")
--   Số thứ tự đánh riêng cho từng văn bản, từng loại (MT/NV), từng viết tắt, theo đúng thứ tự trong quyết định.
--
-- YÊU CẦU: backend mới đã chạy (migration 20261112100000_ItemCodesByLeadAgency: cột Agencies.ShortCode và
-- GoalTaskItems.OriginalCode). Script dừng nếu chưa có các cột này.
--
-- - Mã gốc theo quyết định (MT-01, NV-31) được giữ ở cột OriginalCode: hệ thống vẫn sắp xếp theo thứ tự quyết định,
--   bộ từ khóa AI và cảnh báo AI vẫn khớp theo mã gốc.
-- - Viết tắt chỉ điền cho cơ quan chưa có viết tắt (không ghi đè viết tắt đã sửa trên giao diện).
-- - Chạy lại nhiều lần vẫn cho cùng kết quả. Lịch sử đôn đốc, thông báo cũ giữ nguyên mã cũ.
-- - Hoàn tác (nếu cần): UPDATE "GoalTaskItems" SET "Code" = "OriginalCode" WHERE "OriginalCode" <> '';
-- =============================================================================

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'GoalTaskItems' AND column_name = 'OriginalCode')
     OR NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Agencies' AND column_name = 'ShortCode') THEN
    RAISE EXCEPTION 'Chưa có cột OriginalCode/ShortCode. Hãy chạy backend mới (tự áp migration) rồi chạy lại script.';
  END IF;
END $$;

DROP TABLE IF EXISTS _abbr, _recode;
BEGIN;

-- 1) Viết tắt cơ quan (chỉ cơ quan cấp bộ/ngành, chưa có viết tắt).
CREATE TEMP TABLE _abbr (name text, short_code text);
INSERT INTO _abbr VALUES
  ('Bộ Khoa học và Công nghệ', 'BKHCN'),
  ('Bộ Công an', 'BCA'),
  ('Bộ Nội vụ', 'BNV'),
  ('Bộ Tư pháp', 'BTP'),
  ('Bộ Y tế', 'BYT'),
  ('Bộ Giáo dục và Đào tạo', 'BGDĐT'),
  ('Văn phòng Chính phủ', 'VPCP'),
  ('Bộ Tài chính', 'BTC'),
  ('Bộ Công Thương', 'BCT'),
  ('Ngân hàng Nhà nước Việt Nam', 'NHNN'),
  ('Bộ Văn hóa, Thể thao và Du lịch', 'BVHTTDL'),
  ('Bộ Nông nghiệp và Môi trường', 'BNNMT'),
  ('Ban Cơ yếu Chính phủ', 'BCYCP'),
  ('Bộ Ngoại giao', 'BNG'),
  ('Bộ Quốc phòng', 'BQP'),
  ('Bộ Xây dựng', 'BXD'),
  ('Bộ Dân tộc và Tôn giáo', 'BDTTG');

UPDATE "Agencies" a SET "ShortCode" = x.short_code
FROM _abbr x
WHERE a."ParentId" IS NULL AND a."ShortCode" IS NULL
  AND lower(btrim(a."Name")) = lower(x.name)
  AND NOT EXISTS (SELECT 1 FROM "Agencies" o WHERE o."ShortCode" = x.short_code AND o."Id" <> a."Id");

-- 2) Mã gốc theo quyết định (lần đầu = mã hiện tại).
UPDATE "GoalTaskItems" SET "OriginalCode" = "Code" WHERE "OriginalCode" = '';

-- 3) Mã mới.
CREATE TEMP TABLE _recode AS
WITH base AS (
  SELECT g."Id", g."DocumentId", g."ItemType", g."Code" AS old_code, g."OriginalCode",
         CASE WHEN g."ItemType" = 'Goal' THEN 'MT' ELSE 'NV' END AS prefix,
         CASE WHEN g."IsGeneralTask" OR g."AssignedAgencyId" IS NOT NULL
                OR a."Type" = 'Special' OR a."Code" LIKE 'ALL\_%'
                OR coalesce(btrim(a."ShortCode"), '') = '' THEN 'C'
              ELSE upper(btrim(a."ShortCode")) END AS abbr,
         coalesce(substring(g."OriginalCode" from '(\d+)\s*$')::int, 0) AS stt
  FROM "GoalTaskItems" g
  LEFT JOIN "Agencies" a ON a."Id" = g."LeadAgencyId"
)
SELECT b.*, b.prefix || '-' || b.abbr || '-' ||
       lpad((row_number() OVER (PARTITION BY b."DocumentId", b."ItemType", b.abbr ORDER BY b.stt, b."OriginalCode", b."Id"))::text, 2, '0') AS new_code
FROM base b;

DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM _recode GROUP BY "DocumentId", new_code HAVING count(*) > 1) THEN
    RAISE EXCEPTION 'Mã mới bị trùng trong cùng văn bản. Không thay đổi gì.';
  END IF;
END $$;

UPDATE "GoalTaskItems" g SET "Code" = r.new_code
FROM _recode r WHERE g."Id" = r."Id" AND g."Code" IS DISTINCT FROM r.new_code;

COMMIT;

-- Kết quả: số mục theo văn bản / loại / viết tắt và khoảng mã.
SELECT d."DocumentNumber" AS van_ban, r.prefix AS loai, r.abbr AS viet_tat, count(*) AS so_muc,
       min(r.new_code) AS tu_ma, max(r.new_code) AS den_ma
FROM _recode r JOIN "Documents" d ON d."Id" = r."DocumentId"
GROUP BY 1, 2, 3 ORDER BY 1, 2, (r.abbr = 'C'), 3;

-- Bảng đối chiếu chi tiết (bỏ dấu -- để xem):
-- SELECT d."DocumentNumber", r."OriginalCode" AS ma_goc, r.new_code AS ma_moi FROM _recode r JOIN "Documents" d ON d."Id" = r."DocumentId" ORDER BY 1, r.prefix, r.stt;
