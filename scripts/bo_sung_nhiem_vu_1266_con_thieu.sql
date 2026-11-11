-- =============================================================================
-- BỔ SUNG 16 NHIỆM VỤ QĐ 1266 CÒN THIẾU TRÊN HỆ THỐNG (theo Phan_cong_thuc_hien_QD1266.xlsx)
-- STT 1-14 (gồm 6.1, 6.2): nhiệm vụ tổ chức thực hiện Chiến lược (Khoản 2, 4 Điều 2)
-- STT 43: Kết nối hệ sinh thái đổi mới sáng tạo (Nhóm V)
--
-- - Cơ quan chủ trì: Bộ Khoa học và Công nghệ. Đơn vị trực thuộc theo cột "Đơn vị chủ trì" của Excel
--   (STT 6.1: Cục CĐSQG + Viện CNS&CĐSQG; STT 6.2: Cục CĐSQG + NXB KH-CN-TT;
--    STT 10, 11, 12: danh mục "Tất cả cơ quan, đơn vị thuộc Bộ" = giao cho tất cả đơn vị trực thuộc Bộ).
-- - Loại nhiệm vụ: định tính, thực hiện thường xuyên 14/07/2026 - 31/12/2030 (như các nhiệm vụ 1266 hiện có).
-- - Sản phẩm đầu ra lấy từ cột "Kết quả nhiệm vụ"; Nhóm: "Nhóm Thường Xuyên..." (STT 1-14), "Nhóm V..." (STT 43).
-- - Mã tự cấp tiếp theo mã NV lớn nhất của QĐ 1266 (hiện tại: NV-56 trở đi).
-- - Bỏ qua nhiệm vụ đã có cùng tên trong QĐ 1266, nên chạy lại nhiều lần vẫn an toàn.
-- YÊU CẦU: backend mới đã deploy (cột SubAgencyIds, nhóm đơn vị thuộc Bộ).
-- =============================================================================

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'GoalTaskItems' AND column_name = 'SubAgencyIds') THEN
    RAISE EXCEPTION 'Chưa có cột SubAgencyIds. Hãy deploy backend mới (tự chạy migration) rồi chạy lại script.';
  END IF;
END $$;

DROP TABLE IF EXISTS _new, _pairs, _exec;
BEGIN;

-- Danh mục đặc biệt thuộc Bộ KH&CN (ghi đúng như Quyết định), hiểu là giao cho TẤT CẢ đơn vị thuộc Bộ.
-- Tạo nếu chưa có (Type = Special, mã ALL_UNITS_...). Backend tự mở rộng thành mọi đơn vị trực thuộc đang hoạt động.
INSERT INTO "Agencies" ("Id", "Code", "Name", "Type", "ParentId", "IsActive", "CreatedAt", "AnnualPlans", "ContactPersons", "PlanFiles")
SELECT v.id::uuid, v.code, v.name, 'Special', b."Id", TRUE, now() AT TIME ZONE 'utc', '[]', '[]', '[]'
FROM (VALUES
  ('b0c11111-0000-4000-8000-000000000001', 'ALL_UNITS_BKHCN', 'Tất cả cơ quan, đơn vị thuộc Bộ'),
  ('b0c11111-0000-4000-8000-000000000002', 'ALL_UNITS_BKHCN_NOIBO', 'Các đơn vị trong Bộ'),
  ('b0c11111-0000-4000-8000-000000000003', 'ALL_UNITS_BKHCN_CSDL', 'Các đơn vị quản lý cơ sở dữ liệu quốc gia, cơ sở dữ liệu chuyên ngành')
) v(id, code, name)
JOIN "Agencies" b ON b."Id" = 'fdd14728-fda5-41da-a232-cae702abfcc1'
ON CONFLICT ("Id") DO NOTHING;

CREATE TEMP TABLE _new (ord int, stt text, title text, grp text, deliverable text, sub_ids jsonb, excel_unit text, deadline text, item_id uuid, code text);
INSERT INTO _new (ord, stt, title, grp, deliverable, sub_ids, excel_unit, deadline) VALUES
  (1, '1.', 'Chủ trì tổ chức triển khai, theo dõi, đôn đốc việc thực hiện Chiến lược quốc gia về chuyển đổi số sau khi được cấp có thẩm quyền ban hành.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các hoạt động tổ chức triển khai, theo dõi, đôn đốc việc thực hiện Chiến lược', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (2, '2.', 'Chủ trì điều phối liên ngành trong quá trình tổ chức thực hiện Chiến lược; hướng dẫn, đôn đốc, kiểm tra, giám sát việc thực hiện Chiến lược của các bộ, ngành, địa phương, cơ quan trung ương, doanh nghiệp, tổ chức có liên quan.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các hoạt động điều phối liên ngành trong quá trình tổ chức thực hiện Chiến lược; hướng dẫn, đôn đốc, kiểm tra, giám sát việc thực hiện Chiến lược', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (3, '3.', 'Chủ trì xây dựng, triển khai các giải pháp công nghệ để theo dõi, thống kê, đo lường, giám sát, đánh giá được các mục tiêu, kết quả thực hiện nhiệm vụ của Chiến lược trên môi trường số, bảo đảm tự động tối đa, kịp thời, công khai, minh bạch', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các giải pháp công nghệ để theo dõi, thống kê, đo lường, giám sát, đánh giá được các mục tiêu, kết quả thực hiện nhiệm vụ của Chiến lược', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (4, '4.', 'Chủ trì xây dựng, hoàn thiện Bộ chỉ số đánh giá chuyển đổi số cấp quốc gia, cấp bộ, cấp tỉnh phiên bản mới, phù hợp với Chiến lược và thông lệ quốc tế; thực hiện đánh giá chuyển đổi số quốc gia và công bố công khai kết quả đánh giá định kỳ hằng năm làm căn cứ theo dõi, đôn đốc, điều chỉnh Chiến lược, làm cơ sở theo dõi, đôn đốc, đánh giá kết quả thực hiện, xem xét biểu dương, khen thưởng hoặc đề xuất biện pháp chấn chỉnh đối với các cơ quan, tổ chức có kết quả thực hiện chưa đạt yêu cầu, tạo động lực thi đua chuyển đổi số', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Bộ chỉ số đánh giá chuyển đổi số cấp quốc gia, cấp bộ, cấp tỉnh phiên bản mới, phù hợp với Chiến lược và thông lệ quốc tế được ban hành', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (5, '5.', 'Tổng hợp, báo cáo Thủ tướng Chính phủ về tình hình và kết quả thực hiện Chiến lược; chủ trì tổ chức sơ kết, tổng kết, đánh giá giữa kỳ, đánh giá cuối kỳ; kịp thời báo cáo, đề xuất cấp có thẩm quyền xem xét điều chỉnh mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp khi cần thiết', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Báo cáo Thủ tướng Chính phủ về tình hình và kết quả thực hiện Chiến lược; chủ trì tổ chức sơ kết, tổng kết, đánh giá giữa kỳ, đánh giá cuối kỳ; kịp thời báo cáo, đề xuất cấp có thẩm quyền xem xét điều chỉnh mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp khi cần thiết', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Hằng năm; giữa kỳ; cuối kỳ'),
  (6, '6.1', 'Chủ trì tổ chức nghiên cứu, khảo sát, xây dựng, đề xuất, công bố các mô hình chuyển đổi số mới, mô hình tham chiếu phục vụ chuyển đổi số', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Tổ chức nghiên cứu, khảo sát, xây dựng, đề xuất, công bố các mô hình chuyển đổi số mới, mô hình tham chiếu phục vụ chuyển đổi số', '["ddfd2ca9-4596-412c-b854-d4a7839043d5", "99796db2-f6c1-4cdd-9a1f-bad99f87f57b"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (7, '6.2', 'Tổ chức xuất bản tài liệu hướng dẫn, sổ tay, tài liệu tham khảo kinh nghiệm quản lý nhà nước về chuyển đổi số tại các quốc gia phát triển', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Tài liệu hướng dẫn, sổ tay, xuất bản phẩm tham khảo kinh nghiệm quản lý nhà nước về chuyển đổi số tại các quốc gia phát triển', '["ddfd2ca9-4596-412c-b854-d4a7839043d5", "1f165828-c6d9-4df2-8c03-17991eb2e559"]'::jsonb, 'Cục Chuyển đổi số quốc gia Nhà xuất bản Khoa học - Công nghệ - Truyền thông', 'Thường xuyên'),
  (8, '7.', 'Chủ trì xây dựng các giải pháp hỗ trợ công tác quản lý, giám sát thực thi trong hoạt động chuyển đổi số', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các giải pháp hỗ trợ công tác quản lý, giám sát thực thi trong hoạt động chuyển đổi số', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (9, '8.', 'Chủ trì tổ chức truyền thông, phổ biến, nâng cao nhận thức, đào tạo, bồi dưỡng năng lực chuyển đổi số phục vụ thực hiện Chiến lược; xây dựng, tổ chức các sự kiện, giải thưởng nhằm thúc đẩy chuyển đổi số', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các hoạt động truyền thông, phổ biến, nâng cao nhận thức về chuyển đổi số; đào tạo, bồi dưỡng năng lực chuyển đổi số (bài viết tuyên truyền, video, hội thảo, ....) phục vụ thực hiện Chiến lược; xây dựng, tổ chức các sự kiện, giải thưởng nhằm thúc đẩy chuyển đổi số', '["ddfd2ca9-4596-412c-b854-d4a7839043d5"]'::jsonb, 'Cục Chuyển đổi số quốc gia', 'Thường xuyên'),
  (10, '9.', 'Tổ chức truyền thông, phổ biến nội dung, mục tiêu, nhiệm vụ, giải pháp, mô hình và kết quả thực hiện Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Các sản phẩm truyền thông (tin, bài, phóng sự, infographic, video clip, podcast,…) về chuyển đổi số đăng tải trên Cổng Thông tin điện tử Bộ KH&CN, nền tảng số và mạng xã hội; Các chiến dịch truyền thông về chuyển đổi số hằng năm; Các hoạt động tập huấn nâng cao năng lực truyền thông về chuyển đổi số; Các sản phẩm truyền thông đa phương tiện bằng ngôn ngữ và định dạng dễ tiếp cận về chuyển đổi số theo hướng ngắn gọn, trực quan, dễ hiểu và phù hợp với từng nhóm đối tượng; Theo dõi, tổng hợp, phân tích thông tin báo chí, dư luận xã hội và xây dựng cơ sở dữ liệu, kho thông tin, tư liệu truyền thông phục vụ công tác chỉ đạo, điều hành của Bộ', '["14017696-e831-4fb9-82eb-35712b70ca1f"]'::jsonb, 'Trung tâm Truyền thông khoa học và công nghệ', 'Thường xuyên'),
  (11, '10.', 'Tổ chức quán triệt, cụ thể hóa đầy đủ các mục tiêu, nhiệm vụ, giải pháp của Chiến lược vào chương trình, kế hoạch, đề án của cơ quan, đơn vị; xác định rõ lộ trình, nhiệm vụ, nguồn lực và trách nhiệm thực hiện đối với từng mục tiêu, chỉ tiêu thuộc phạm vi quản lý.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Quán triệt, cụ thể hóa đầy đủ các mục tiêu, nhiệm vụ, giải pháp của Chiến lược vào chương trình, kế hoạch, đề án của cơ quan, đơn vị; xác định rõ lộ trình, nhiệm vụ, nguồn lực và trách nhiệm thực hiện đối với từng mục tiêu, chỉ tiêu thuộc phạm vi quản lý.', '["b0c11111-0000-4000-8000-000000000001"]'::jsonb, 'Tất cả cơ quan, đơn vị thuộc Bộ', 'Thường xuyên'),
  (12, '11.', 'Rà soát, sửa đổi, bổ sung hoặc đề xuất cấp có thẩm quyền sửa đổi, bổ sung cơ chế, chính sách, quy định thuộc phạm vi quản lý để bảo đảm thống nhất, đồng bộ với Chiến lược; kịp thời phản ánh khó khăn, vướng mắc và đề xuất giải pháp xử lý trong quá trình thực hiện', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Báo cáo rà soát, sửa đổi, bổ sung cơ chế, chính sách, quy định thuộc phạm vi quản lý để bảo đảm thống nhất, đồng bộ với Chiến lược; kịp thời phản ánh khó khăn, vướng mắc và đề xuất giải pháp xử lý trong quá trình thực hiện', '["b0c11111-0000-4000-8000-000000000001"]'::jsonb, 'Tất cả cơ quan, đơn vị thuộc Bộ', 'Thường xuyên'),
  (13, '12.', 'Bố trí nhân lực, kinh phí, hạ tầng và các điều kiện cần thiết khác để thực hiện Chiến lược; lồng ghép, huy động và sử dụng hiệu quả nguồn lực từ các chương trình, kế hoạch, đề án có liên quan; ưu tiên triển khai các nhiệm vụ trọng tâm, cấp bách, có tác động lan tỏa.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Bố trí nhân lực, kinh phí, hạ tầng và các điều kiện cần thiết khác để thực hiện Chiến lược', '["b0c11111-0000-4000-8000-000000000001"]'::jsonb, 'Tất cả cơ quan, đơn vị thuộc Bộ', 'Thường xuyên'),
  (14, '13.', 'Tổ chức theo dõi, kiểm tra, đánh giá việc thực hiện Chiến lược trong phạm vi quản lý của Bộ KH&CN; thực hiện chế độ báo cáo định kỳ, đột xuất theo quy định; phối hợp với Cục Chuyển đổi số quốc gia trong công tác hướng dẫn, đôn đốc, kiểm tra, giám sát việc thực hiện Chiến lược.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Tổ chức theo dõi, kiểm tra, đánh giá; báo cáo định kỳ, đột xuất việc thực hiện Chiến lược', '["3d860a72-0b43-4883-8e73-b75d46802de1"]'::jsonb, 'Trung tâm Chuyển đổi số', 'Thường xuyên'),
  (15, '14.', 'Chủ trì hướng dẫn, rà soát, tổng hợp chung kinh phí thực hiện Chiến lược trong phương án dự toán chi ngân sách nhà nước cho lĩnh vực khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số gửi Bộ Tài chính tổng hợp, trình cấp có thẩm quyền xem xét, quyết định; Phối hợp với các cơ quan, đơn vị được giao chủ trì nhiệm vụ tổng hợp, cân đối, bố trí kinh phí thực hiện các nhiệm vụ được giao tại Quyết định này.', 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', 'Hướng dẫn, rà soát, tổng hợp chung kinh phí thực hiện Chiến lược trong phương án dự toán chi ngân sách nhà nước cho lĩnh vực khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số gửi Bộ Tài chính tổng hợp, trình cấp có thẩm quyền xem xét, quyết định; Tổng hợp, cân đối, bố trí kinh phí thực hiện các nhiệm vụ được giao tại Quyết định này.', '["73462e04-354e-4e60-80d8-808818fa68fe"]'::jsonb, 'Vụ Kế hoạch – Tài chính', 'Thường xuyên'),
  (16, '43.', 'Kết nối hệ sinh thái đổi mới sáng tạo; hỗ trợ doanh nghiệp khởi nghiệp sáng tạo, doanh nghiệp công nghệ số thử nghiệm, hoàn thiện và thương mại hóa các sản phẩm, giải pháp phục vụ chuyển đổi số', 'Nhóm V: Phát triển doanh nghiệp công nghệ số', 'Các hoạt động, chương trình, giải pháp, mô hình kết nối hệ sinh thái đổi mới sáng tạo; hỗ trợ doanh nghiệp khởi nghiệp sáng tạo, doanh nghiệp công nghệ số thử nghiệm, hoàn thiện và thương mại hóa các sản phẩm, giải pháp phục vụ chuyển đổi số.', '["36a5fadf-cb02-4624-88aa-9f393150f6e3"]'::jsonb, 'Viện Đổi mới sáng tạo quốc gia', 'Thường xuyên');

DO $$
DECLARE doc uuid := '12660000-0000-0000-0000-000000001266'; bkhcn uuid := 'fdd14728-fda5-41da-a232-cae702abfcc1';
BEGIN
  IF NOT EXISTS (SELECT 1 FROM "Documents" WHERE "Id" = doc) THEN RAISE EXCEPTION 'Không tìm thấy văn bản 1266/QĐ-TTg.'; END IF;
  IF NOT EXISTS (SELECT 1 FROM "Agencies" WHERE "Id" = bkhcn AND "ParentId" IS NULL) THEN RAISE EXCEPTION 'Không tìm thấy Bộ Khoa học và Công nghệ.'; END IF;
  IF EXISTS (SELECT 1 FROM _new n, jsonb_array_elements_text(n.sub_ids) s(id)
             LEFT JOIN "Agencies" a ON a."Id" = s.id::uuid WHERE a."Id" IS NULL OR a."ParentId" IS DISTINCT FROM bkhcn) THEN
    RAISE EXCEPTION 'Có đơn vị trực thuộc không tồn tại hoặc không thuộc Bộ KH&CN. Không thay đổi gì.';
  END IF;
END $$;

-- Đã có nhiệm vụ cùng tên trong QĐ 1266: bỏ qua.
DELETE FROM _new n USING "GoalTaskItems" g
WHERE g."DocumentId" = '12660000-0000-0000-0000-000000001266' AND g."ItemType" = 'Task'
  AND lower(btrim(g."Title")) = lower(btrim(n.title));

-- Cấp mã tiếp theo.
WITH mx AS (
  SELECT coalesce(max(substring("Code" from 'NV-(\d+)')::int), 0) AS m
  FROM "GoalTaskItems" WHERE "DocumentId" = '12660000-0000-0000-0000-000000001266' AND "Code" ~ '^NV-\d+$'),
seq AS (SELECT ord, row_number() OVER (ORDER BY ord) AS rn FROM _new)
UPDATE _new n SET item_id = gen_random_uuid(), code = 'NV-' || lpad((mx.m + seq.rn)::text, 2, '0')
FROM mx, seq WHERE seq.ord = n.ord;

INSERT INTO "GoalTaskItems"
  ("Id", "DocumentId", "ItemType", "Code", "Title", "Category", "Section", "Group", "IsOngoing", "IsGeneralTask",
   "StartDate", "DueDate", "LeadAgencyId", "LeadAgencyDisplayText", "AssignedAgencyId", "CoordinatingAgencyIds",
   "CoordinatingAgenciesDisplayText", "SubAgencyIds", "ResultTarget", "UnitId", "EvaluationType", "CalculationMethod",
   "CustomBaseline", "Deliverables", "AgencyDeliverables", "DynamicKPIs", "CreatedAt")
SELECT n.item_id, '12660000-0000-0000-0000-000000001266', 'Task', n.code, n.title, '', '', n.grp, TRUE, FALSE,
   '2026-07-14 00:00:00', '2030-12-31 12:00:00', 'fdd14728-fda5-41da-a232-cae702abfcc1', NULL, NULL, '[]'::jsonb,
   NULL, n.sub_ids, NULL, 'f50d1c29-8bc9-4b47-af16-2cc987917718', 'Qualitative', 'LatestValue',
   '{}'::jsonb,
   jsonb_build_array(jsonb_build_object('Id', replace(gen_random_uuid()::text, '-', ''), 'Title', n.deliverable,
      'DueDate', '2030-12-31T00:00:00Z', 'AttachmentUrl', NULL, 'CurrentStatus', 'NotStarted', 'AttachmentName', NULL,
      'DocumentNumber', NULL, 'PromulgationDate', NULL)),
   '{}'::jsonb, '{}'::jsonb, now() AT TIME ZONE 'utc'
FROM _new n;

-- Phân giao: Bộ KH&CN chủ trì và các đơn vị trực thuộc.
CREATE TEMP TABLE _pairs AS
SELECT item_id, 'fdd14728-fda5-41da-a232-cae702abfcc1'::uuid AS agency_id FROM _new
UNION SELECT n.item_id, s.id::uuid FROM _new n, jsonb_array_elements_text(n.sub_ids) s(id);
-- Phân giao chủ trì (như AgencyAssignments.SyncAsync): đơn vị cụ thể là chủ trì trực tiếp;
-- nhóm "Tất cả/Các đơn vị..." được mở rộng thành mọi đơn vị trực thuộc đang hoạt động của Bộ (giao qua nhóm).
CREATE TEMP TABLE _exec AS
SELECT item_id, agency_id, bool_and(via_group) AS via_group FROM (
  SELECT p.item_id, p.agency_id, FALSE AS via_group
  FROM _pairs p JOIN "Agencies" a ON a."Id" = p.agency_id
  WHERE NOT (a."Type" = 'Special' OR a."Code" LIKE 'ALL\_%')
  UNION ALL
  SELECT p.item_id, m."Id", TRUE
  FROM _pairs p JOIN "Agencies" grp ON grp."Id" = p.agency_id
  JOIN "Agencies" m ON m."ParentId" = grp."ParentId" AND m."Id" <> grp."Id" AND m."IsActive"
       AND NOT (m."Type" = 'Special' OR m."Code" LIKE 'ALL\_%')
  WHERE grp."Type" = 'Special' OR grp."Code" LIKE 'ALL\_%'
) x GROUP BY item_id, agency_id;

UPDATE "AgencyTaskExecutions" e SET
  "AssignedAt" = CASE WHEN NOT e."IsCoordinatingAssignment" THEN (now() AT TIME ZONE 'utc') ELSE e."AssignedAt" END,
  "IsLeadAssignment" = TRUE,
  "IsLeadGroupAssignment" = x.via_group
FROM _exec x WHERE e."GoalTaskId" = x.item_id AND e."AgencyId" = x.agency_id AND NOT e."IsLeadAssignment";

INSERT INTO "AgencyTaskExecutions"
  ("Id", "GoalTaskId", "AgencyId", "CalculatedStatus", "CompletionPercentage", "Deliverables", "AttachmentFileUrls",
   "CreatedAt", "UpdatedAt", "ApprovalStatus", "IsLeadAssignment", "IsCoordinatingAssignment",
   "IsLeadGroupAssignment", "IsCoordinatingGroupAssignment", "AssignedAt")
SELECT gen_random_uuid(), x.item_id, x.agency_id, 'NotStarted', 0, '[]'::jsonb, '[]'::jsonb,
   now() AT TIME ZONE 'utc', now() AT TIME ZONE 'utc', 1, TRUE, FALSE, x.via_group, FALSE, now() AT TIME ZONE 'utc'
FROM _exec x
WHERE NOT EXISTS (SELECT 1 FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId" = x.item_id AND e."AgencyId" = x.agency_id);

COMMIT;

-- Kết quả.
SELECT n.code AS ma, n.stt AS stt_excel, left(n.title, 90) AS nhiem_vu,
       coalesce((SELECT string_agg(btrim(a."Name"), '; ' ORDER BY s.o) FROM jsonb_array_elements_text(n.sub_ids) WITH ORDINALITY s(id, o)
                 JOIN "Agencies" a ON a."Id" = s.id::uuid), '(Bộ chủ trì)') AS don_vi_truc_thuoc,
       (SELECT count(*) FROM "AgencyTaskExecutions" e WHERE e."GoalTaskId" = n.item_id AND e."IsLeadAssignment") AS so_don_vi_chu_tri
FROM _new n ORDER BY n.ord;
