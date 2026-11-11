-- =============================================================================
-- GÁN ĐƠN VỊ TRỰC THUỘC BỘ KH&CN VÀO MỤC TIÊU, NHIỆM VỤ QĐ 2629 & QĐ 1266 (bản 4)
-- Nguồn: Phan_cong_nhiem_vu_QD2629.xlsx, Phan_cong_thuc_hien_QD1266.xlsx (11/11/2026)
--
-- YÊU CẦU: backend đã cập nhật (migration 20261111120000_MultipleSubAgencyAssignments và hỗ trợ
-- nhóm đơn vị thuộc Bộ). Script dừng ngay nếu chưa có cột SubAgencyIds.
--
-- - Mỗi mục có thể giao nhiều đơn vị trực thuộc; đơn vị đứng đầu là đơn vị đầu mối (theo thứ tự trong Excel).
-- - Cơ quan đồng chủ trì (ví dụ Bộ Công an ở 2629 NV-31, Bộ Nội vụ ở NV-65) được giữ nguyên.
-- - Tạo (nếu chưa có) 3 danh mục đặc biệt thuộc Bộ, ghi đúng như Quyết định: "Tất cả cơ quan, đơn vị thuộc Bộ",
--   "Các đơn vị trong Bộ", "Các đơn vị quản lý cơ sở dữ liệu quốc gia, cơ sở dữ liệu chuyên ngành".
--   Giao cho các danh mục này = giao cho tất cả đơn vị trực thuộc Bộ (mỗi đơn vị đều thấy và báo cáo được).
-- - Quy đổi tên: "Trung tâm Công nghệ thông tin" = Trung tâm Chuyển đổi số; "Cục Thông tin, Thống kê" =
--   Cục Thông tin, Thống kê và Đánh giá khoa học; "Vụ Kinh tế và Xã hội số" = Vụ Kinh tế số và Xã hội số.
-- - Chỉ ghi vào mục CHƯA giao đơn vị trực thuộc hoặc đã giao đúng như Excel; mục đã giao khác sẽ bỏ qua.
-- - Giữ nguyên báo cáo tiến độ, phê duyệt, minh chứng. Chạy lại nhiều lần vẫn an toàn.
-- - 16 nhiệm vụ QĐ 1266 chưa có trên hệ thống: chạy tiếp bo_sung_nhiem_vu_1266_con_thieu.sql.
-- =============================================================================

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'GoalTaskItems' AND column_name = 'SubAgencyIds') THEN
    RAISE EXCEPTION 'Chưa có cột SubAgencyIds. Hãy deploy backend mới (tự chạy migration) rồi chạy lại script.';
  END IF;
END $$;

DROP TABLE IF EXISTS _opt, _plan, _items, _skip, _pairs, _exec;
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

-- Tùy chọn: đặt FALSE nếu KHÔNG muốn gán cho nhiệm vụ chung (chủ trì là nhóm 'Các bộ, ngành...').
CREATE TEMP TABLE _opt AS SELECT TRUE AS include_general;

CREATE TEMP TABLE _plan (item_id uuid, ord int, agency_id uuid, is_general boolean, label text, agency text);
INSERT INTO _plan VALUES
  ('5bafeb2c-cd2c-4878-9a4f-62f8e2c8f4ef', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg MT-03', 'Cục Chuyển đổi số quốc gia'),
  ('2903c94b-d906-4a20-bb1e-1052e8a7dd99', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg MT-04', 'Cục Chuyển đổi số quốc gia'),
  ('7513352d-4413-43d7-9537-171906197ee0', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg MT-11', 'Cục Chuyển đổi số quốc gia'),
  ('1b71f52e-040c-4b6a-8364-207eb714b897', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg MT-12', 'Cục Chuyển đổi số quốc gia'),
  ('27bda09d-f965-49b6-bdb4-d858e8977d9d', 0, '99796db2-f6c1-4cdd-9a1f-bad99f87f57b', FALSE, '2629/QĐ-TTg MT-19', 'Viện Công nghệ số và Chuyển đổi số quốc gia'),
  ('9868baec-b3eb-4291-b432-51454bc94b23', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-01', 'Cục Chuyển đổi số quốc gia'),
  ('fa63607c-575f-4c54-875f-ad7d6005ebaf', 0, '99796db2-f6c1-4cdd-9a1f-bad99f87f57b', FALSE, '2629/QĐ-TTg NV-02', 'Viện Công nghệ số và Chuyển đổi số quốc gia'),
  ('cd89318d-8c42-4e6b-a47e-9908b7a5c287', 0, '8f223f67-a611-4469-abcc-873762604c68', TRUE , '2629/QĐ-TTg NV-04', 'Ủy ban Tiêu chuẩn Đo lường Chất lượng Quốc gia'),
  ('6445c767-d46d-4224-91d5-5fe9c553235d', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-07', 'Cục Chuyển đổi số quốc gia'),
  ('07d6a4f3-6dba-438f-bb48-d87cac9d2821', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-08', 'Cục Chuyển đổi số quốc gia'),
  ('8b2f6080-5d1a-4f1d-88d1-4e8b164450a5', 0, '99796db2-f6c1-4cdd-9a1f-bad99f87f57b', FALSE, '2629/QĐ-TTg NV-09', 'Viện Công nghệ số và Chuyển đổi số quốc gia'),
  ('41dd40b3-c5d9-47fa-821a-a97bc7a00380', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-10', 'Cục Chuyển đổi số quốc gia'),
  ('999e3504-73fe-497b-83c8-d6b1a222c9d5', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-31', 'Cục Chuyển đổi số quốc gia'),
  ('0c7af26e-85b8-4d60-9ff1-506b484379f8', 0, '8d12b457-744c-4ca9-aa05-179319933390', FALSE, '2629/QĐ-TTg NV-35', 'Cục Bưu điện Trung ương'),
  ('a9da2207-c32a-4d1c-b5f2-aff802bee0b3', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-40', 'Cục Chuyển đổi số quốc gia'),
  ('1f05c598-a6f4-461d-a00a-e3b931638aed', 0, 'ebca9dab-20c2-42d9-92e5-9487a5bc743c', FALSE, '2629/QĐ-TTg NV-41', 'Cục Thông tin, Thống kê và Đánh giá khoa học'),
  ('10da417c-afeb-41bf-bb21-86441fd221da', 0, '8d12b457-744c-4ca9-aa05-179319933390', FALSE, '2629/QĐ-TTg NV-54', 'Cục Bưu điện Trung ương'),
  ('a487e931-4964-4bce-bcb2-d5e220b77757', 0, '8d12b457-744c-4ca9-aa05-179319933390', FALSE, '2629/QĐ-TTg NV-57', 'Cục Bưu điện Trung ương'),
  ('44e1674a-1b9b-476a-8d9d-825b65c80ea7', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-65', 'Cục Chuyển đổi số quốc gia'),
  ('02e7efc9-589c-4e3f-84b0-09bfcaacb0b4', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-66', 'Cục Chuyển đổi số quốc gia'),
  ('84864b0d-a066-4e8c-8824-b3b9f43fefe1', 0, '829d700f-50fd-4416-952d-2826c0101dc6', FALSE, '2629/QĐ-TTg NV-69', 'Vụ Hợp tác quốc tế'),
  ('63160d54-f3e5-4980-b354-830f28e83327', 0, '829d700f-50fd-4416-952d-2826c0101dc6', FALSE, '2629/QĐ-TTg NV-70', 'Vụ Hợp tác quốc tế'),
  ('b82ebb39-56f9-4639-b9e5-1bc9d9e80c7f', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-71', 'Cục Chuyển đổi số quốc gia'),
  ('551e5a71-0dbd-474c-9e82-8bb5ea608d33', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-74', 'Cục Chuyển đổi số quốc gia'),
  ('e500f716-3261-4d05-a3e4-f316982b37f0', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '2629/QĐ-TTg NV-75', 'Cục Chuyển đổi số quốc gia'),
  ('f2745612-1ae2-4358-96e4-eebf5fe8236f', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-05', 'Trung tâm Chuyển đổi số'),
  ('87a596fe-81bb-4b87-8b92-a5e7a2a42f1f', 0, 'ebca9dab-20c2-42d9-92e5-9487a5bc743c', TRUE , '2629/QĐ-TTg NV-06', 'Cục Thông tin, Thống kê và Đánh giá khoa học'),
  ('30f0f42b-3aa4-4324-85cc-b5d59e32e614', 0, 'b0c11111-0000-4000-8000-000000000002', TRUE , '2629/QĐ-TTg NV-12', 'Các đơn vị trong Bộ'),
  ('9b10c20e-6a5c-4309-8249-acec5b751405', 0, '7a2325e6-26f4-4c40-83d2-7b0a357a3a30', TRUE , '2629/QĐ-TTg NV-15', 'Văn phòng Bộ'),
  ('70f92e86-a9e8-4d46-b4e4-e3de938c72a7', 0, 'b0c11111-0000-4000-8000-000000000003', TRUE , '2629/QĐ-TTg NV-16', 'Các đơn vị quản lý cơ sở dữ liệu quốc gia, cơ sở dữ liệu chuyên ngành'),
  ('4ee8219b-ad40-46f6-8441-e789a22bc0bd', 0, 'ebca9dab-20c2-42d9-92e5-9487a5bc743c', TRUE , '2629/QĐ-TTg NV-17', 'Cục Thông tin, Thống kê và Đánh giá khoa học'),
  ('fa484656-6144-4b85-891b-0d746567a554', 0, 'ebca9dab-20c2-42d9-92e5-9487a5bc743c', TRUE , '2629/QĐ-TTg NV-18', 'Cục Thông tin, Thống kê và Đánh giá khoa học'),
  ('35dc6334-157d-479e-9d5f-de7aca593525', 0, '99796db2-f6c1-4cdd-9a1f-bad99f87f57b', TRUE , '2629/QĐ-TTg NV-34', 'Viện Công nghệ số và Chuyển đổi số quốc gia'),
  ('fd114209-7e61-462a-823a-094129eca1b0', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-44', 'Trung tâm Chuyển đổi số'),
  ('4703d362-1b13-4a6b-bf33-3d955cf4e75f', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-45', 'Trung tâm Chuyển đổi số'),
  ('920af6b2-96b9-4ae8-b92b-37d807e4e926', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-46', 'Trung tâm Chuyển đổi số'),
  ('0f65dc5c-64ec-4e03-9179-28e7c44308c0', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-47', 'Trung tâm Chuyển đổi số'),
  ('dfa16a4d-86db-4706-82da-3d646121beec', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-48', 'Trung tâm Chuyển đổi số'),
  ('d1b99783-7e6d-4e5c-9595-3ef30011b4a0', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-49', 'Trung tâm Chuyển đổi số'),
  ('b9f48252-6c0d-4e27-b47a-c6f4bb2318f4', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-50', 'Trung tâm Chuyển đổi số'),
  ('3dabc52b-9022-4fe7-9416-b3c89aa21c69', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-51', 'Trung tâm Chuyển đổi số'),
  ('f36ef1d0-20a8-4480-8391-4fcf5a75054d', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '2629/QĐ-TTg NV-55', 'Trung tâm Chuyển đổi số'),
  ('1e5947a2-8c29-4c83-9a60-365a06149d66', 0, 'b0c11111-0000-4000-8000-000000000002', TRUE , '2629/QĐ-TTg NV-63', 'Các đơn vị trong Bộ'),
  ('a7730379-6b32-42f8-9018-65ae9309ac48', 0, '328d2158-5f4d-4a07-b55e-d672829c575f', TRUE , '2629/QĐ-TTg NV-64', 'Học viện Chiến lược Khoa học và Công nghệ'),
  ('0de70307-3d70-46ce-852b-4d1bfbb5af71', 0, '8a9183c9-9551-4424-8652-ec02e110eeb9', TRUE , '2629/QĐ-TTg NV-67', 'Vụ Tổ chức cán bộ'),
  ('882ce7e9-5f80-4ee1-8e90-a8a1254f824d', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-02', 'Cục Chuyển đổi số quốc gia'),
  ('f101699f-994b-4e27-856f-20b8273c47de', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-03', 'Cục Chuyển đổi số quốc gia'),
  ('6e884b6d-da22-4c04-8b67-50fdca1b9252', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-06', 'Cục Chuyển đổi số quốc gia'),
  ('653190c6-d1d6-4509-826c-964f9df1d600', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-09', 'Cục Chuyển đổi số quốc gia'),
  ('8dd36dda-b7fa-42b6-ac19-c72a29b6afe4', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-10', 'Cục Chuyển đổi số quốc gia'),
  ('f860a36b-1b97-4b11-aedc-4de835ba4740', 0, '2569481b-98f8-4eb2-8320-5ab6c9dd81ba', FALSE, '1266/QĐ-TTg MT-11', 'Cục Công nghệ số và Trí tuệ nhân tạo'),
  ('6b635a7b-7eb6-4085-a5f8-de2a7d3f7626', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-12', 'Cục Chuyển đổi số quốc gia'),
  ('5c6117e1-9dae-4700-8b52-48248401b9f8', 0, '30cae5a1-e56f-4923-98b6-d0193b0c2228', FALSE, '1266/QĐ-TTg MT-13', 'Học viện Công nghệ Bưu chính Viễn thông'),
  ('82b64e30-8704-44b5-a4c5-bab677d08820', 0, '44ac2721-e150-4eb6-9d23-f2ed20e9869c', FALSE, '1266/QĐ-TTg MT-14', 'Vụ Kinh tế số và Xã hội số'),
  ('f8b7b41c-8723-487b-8298-c23c623b1e2e', 0, '2569481b-98f8-4eb2-8320-5ab6c9dd81ba', FALSE, '1266/QĐ-TTg MT-19', 'Cục Công nghệ số và Trí tuệ nhân tạo'),
  ('88343d65-ef4a-4ce2-bef4-43e5c6cbe142', 0, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', FALSE, '1266/QĐ-TTg MT-20', 'Cục Viễn thông'),
  ('85884a8c-979c-44c9-addc-c44dae086268', 0, '1624bfd6-0bf0-4eda-b532-315fbfac6cd3', FALSE, '1266/QĐ-TTg MT-22', 'Trung tâm Chứng thực điện tử quốc gia'),
  ('2d451174-7887-4bc5-8edf-6aa2fc634421', 0, '44ac2721-e150-4eb6-9d23-f2ed20e9869c', TRUE , '1266/QĐ-TTg MT-24', 'Vụ Kinh tế số và Xã hội số'),
  ('c534e9e3-0a16-48cf-828a-cce4ae79c44e', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg MT-25', 'Cục Chuyển đổi số quốc gia'),
  ('b5f4f976-d6fd-4b77-ad22-8ebd31ae918d', 0, '30cae5a1-e56f-4923-98b6-d0193b0c2228', FALSE, '1266/QĐ-TTg MT-26', 'Học viện Công nghệ Bưu chính Viễn thông'),
  ('bacd8feb-b0bc-46ef-be99-036f3ab67aa8', 0, 'cf070596-f6af-4ad2-a4d6-1104ae3acec3', TRUE , '1266/QĐ-TTg NV-01', 'Vụ Pháp chế'),
  ('049b298d-ccdd-486a-94e3-189752836a17', 0, 'cf070596-f6af-4ad2-a4d6-1104ae3acec3', TRUE , '1266/QĐ-TTg NV-07', 'Vụ Pháp chế'),
  ('202d6a08-e49c-4a97-99ed-16b32f6723ac', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg NV-02', 'Cục Chuyển đổi số quốc gia'),
  ('202d6a08-e49c-4a97-99ed-16b32f6723ac', 1, '44ac2721-e150-4eb6-9d23-f2ed20e9869c', FALSE, '1266/QĐ-TTg NV-02', 'Vụ Kinh tế số và Xã hội số'),
  ('0c1004f3-22e6-4327-b503-8ab6337053a2', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg NV-08', 'Cục Chuyển đổi số quốc gia'),
  ('a34cc576-da46-4bb0-9020-f6e51febe75a', 0, '8f223f67-a611-4469-abcc-873762604c68', TRUE , '1266/QĐ-TTg NV-09', 'Ủy ban Tiêu chuẩn Đo lường Chất lượng Quốc gia'),
  ('00f0ded0-9a90-4f33-91de-b9ef3fd293d9', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-12', 'Trung tâm Chuyển đổi số'),
  ('00f0ded0-9a90-4f33-91de-b9ef3fd293d9', 1, '7a2325e6-26f4-4c40-83d2-7b0a357a3a30', TRUE , '1266/QĐ-TTg NV-12', 'Văn phòng Bộ'),
  ('a2d4a9a6-2590-404c-b191-d9e40089b1a4', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-13', 'Trung tâm Chuyển đổi số'),
  ('7a12add2-d5d7-4aae-afa0-ba76393142df', 0, '8d12b457-744c-4ca9-aa05-179319933390', FALSE, '1266/QĐ-TTg NV-14', 'Cục Bưu điện Trung ương'),
  ('513becfb-13d1-4462-82ec-45fb030ad5d2', 0, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', FALSE, '1266/QĐ-TTg NV-15', 'Cục Viễn thông'),
  ('9891a16d-f5cb-4b16-b38f-790b1c860226', 0, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', FALSE, '1266/QĐ-TTg NV-16', 'Cục Viễn thông'),
  ('673069bb-312d-4d80-b9a9-a67fcef3f5ea', 0, 'f3c57913-55ff-4a5b-a2cf-4d04e5b1a67d', FALSE, '1266/QĐ-TTg NV-17', 'Trung tâm Internet Việt Nam'),
  ('673069bb-312d-4d80-b9a9-a67fcef3f5ea', 1, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', FALSE, '1266/QĐ-TTg NV-17', 'Cục Viễn thông'),
  ('8991fa83-a26d-458f-8a2d-d8663ef60b9a', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', TRUE , '1266/QĐ-TTg NV-19', 'Cục Chuyển đổi số quốc gia'),
  ('20ead92c-c875-4b64-b5b9-ade481e3ca5e', 0, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', TRUE , '1266/QĐ-TTg NV-20', 'Cục Viễn thông'),
  ('1a581306-5003-4cbd-b25c-827d39a845e9', 0, 'b0c11111-0000-4000-8000-000000000001', TRUE , '1266/QĐ-TTg NV-21', 'Tất cả cơ quan, đơn vị thuộc Bộ'),
  ('10c12a1d-b15e-497f-b356-08aebb677f8d', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-22', 'Trung tâm Chuyển đổi số'),
  ('87847870-4051-4d99-b1bb-e793576d6c1f', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-23', 'Trung tâm Chuyển đổi số'),
  ('34b27cf4-50a2-43b6-ab61-883be6fb3fdd', 0, '7a2325e6-26f4-4c40-83d2-7b0a357a3a30', TRUE , '1266/QĐ-TTg NV-24', 'Văn phòng Bộ'),
  ('d4a766e2-0aee-4147-83d8-2a187d527003', 0, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg NV-27', 'Cục Chuyển đổi số quốc gia'),
  ('4aa1fe8c-001c-472a-a277-e6939ba8c072', 0, '8f223f67-a611-4469-abcc-873762604c68', TRUE , '1266/QĐ-TTg NV-28', 'Ủy ban Tiêu chuẩn Đo lường Chất lượng Quốc gia'),
  ('06836a51-dcd1-465d-8ae2-e3a02d519551', 0, '2569481b-98f8-4eb2-8320-5ab6c9dd81ba', TRUE , '1266/QĐ-TTg NV-29', 'Cục Công nghệ số và Trí tuệ nhân tạo'),
  ('5d1fdbb8-0626-4bfc-aeec-1bebaebf2452', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-30', 'Trung tâm Chuyển đổi số'),
  ('b1938d8c-5b21-48fd-8b18-3d3ab4215cf8', 0, '99796db2-f6c1-4cdd-9a1f-bad99f87f57b', TRUE , '1266/QĐ-TTg NV-31', 'Viện Công nghệ số và Chuyển đổi số quốc gia'),
  ('b6c928f1-7d78-44a5-8afe-508d198e76ed', 0, '3d860a72-0b43-4883-8e73-b75d46802de1', TRUE , '1266/QĐ-TTg NV-32', 'Trung tâm Chuyển đổi số'),
  ('130f2179-7668-4ef0-b941-423403afe70b', 0, '8a9183c9-9551-4424-8652-ec02e110eeb9', TRUE , '1266/QĐ-TTg NV-36', 'Vụ Tổ chức cán bộ'),
  ('2f8900a6-a21a-4f17-8cfd-99c516a13f70', 0, '7afd4a4d-bd02-48e5-b829-dc007ad34795', FALSE, '1266/QĐ-TTg NV-45', 'Vụ Khoa học kỹ thuật và công nghệ'),
  ('5855ebe6-f37b-48ce-bc13-8354bab264a3', 0, '2569481b-98f8-4eb2-8320-5ab6c9dd81ba', FALSE, '1266/QĐ-TTg NV-46', 'Cục Công nghệ số và Trí tuệ nhân tạo'),
  ('ab042620-ba29-4fdf-a0fa-9222365a4487', 0, '44ac2721-e150-4eb6-9d23-f2ed20e9869c', TRUE , '1266/QĐ-TTg NV-47', 'Vụ Kinh tế số và Xã hội số'),
  ('2eb7e930-f95e-4480-87fd-5a18fc531980', 0, 'f3c57913-55ff-4a5b-a2cf-4d04e5b1a67d', FALSE, '1266/QĐ-TTg NV-48', 'Trung tâm Internet Việt Nam'),
  ('32a7f51f-1c49-49b3-81aa-b3f7ebf995e7', 0, 'abc484c9-e1e8-4c68-99e2-c7e3e4f9230a', FALSE, '1266/QĐ-TTg NV-50', 'Cục Viễn thông'),
  ('32a7f51f-1c49-49b3-81aa-b3f7ebf995e7', 1, '2569481b-98f8-4eb2-8320-5ab6c9dd81ba', FALSE, '1266/QĐ-TTg NV-50', 'Cục Công nghệ số và Trí tuệ nhân tạo'),
  ('32a7f51f-1c49-49b3-81aa-b3f7ebf995e7', 2, 'ddfd2ca9-4596-412c-b854-d4a7839043d5', FALSE, '1266/QĐ-TTg NV-50', 'Cục Chuyển đổi số quốc gia'),
  ('32a7f51f-1c49-49b3-81aa-b3f7ebf995e7', 3, '8f223f67-a611-4469-abcc-873762604c68', FALSE, '1266/QĐ-TTg NV-50', 'Ủy ban Tiêu chuẩn Đo lường Chất lượng Quốc gia'),
  ('6628085c-25b6-4a21-9010-36941c9587a8', 0, '829d700f-50fd-4416-952d-2826c0101dc6', TRUE , '1266/QĐ-TTg NV-52', 'Vụ Hợp tác quốc tế'),
  ('ef5b2740-a2e9-467a-9976-48e74d8b4dad', 0, '8a9183c9-9551-4424-8652-ec02e110eeb9', TRUE , '1266/QĐ-TTg NV-54', 'Vụ Tổ chức cán bộ'),
  ('5dd52c15-9fd3-440f-a488-7df52fddfad9', 0, 'f3c57913-55ff-4a5b-a2cf-4d04e5b1a67d', FALSE, '1266/QĐ-TTg NV-55', 'Trung tâm Internet Việt Nam');

DELETE FROM _plan WHERE is_general AND NOT (SELECT include_general FROM _opt);
DELETE FROM _plan p WHERE NOT EXISTS (SELECT 1 FROM "GoalTaskItems" g WHERE g."Id" = p.item_id);

DO $$ BEGIN
  IF EXISTS (SELECT 1 FROM _plan p LEFT JOIN "Agencies" a ON a."Id" = p.agency_id WHERE a."Id" IS NULL OR a."ParentId" IS NULL) THEN
    RAISE EXCEPTION 'Có đơn vị không tồn tại hoặc không phải đơn vị trực thuộc. Không thay đổi gì.';
  END IF;
END $$;

CREATE TEMP TABLE _items AS
SELECT item_id, bool_or(is_general) AS is_general, min(label) AS label,
       jsonb_agg(to_jsonb(agency_id::text) ORDER BY ord) AS ids,
       string_agg(agency, '; ' ORDER BY ord) AS agencies
FROM _plan GROUP BY item_id;

-- Mục đã giao đơn vị trực thuộc khác với Excel: bỏ qua, không ghi đè.
CREATE TEMP TABLE _skip AS
SELECT i.*, g."SubAgencyIds" AS current_ids FROM _items i JOIN "GoalTaskItems" g ON g."Id" = i.item_id
WHERE jsonb_array_length(g."SubAgencyIds") > 0 AND g."SubAgencyIds" <> i.ids;
DELETE FROM _items WHERE item_id IN (SELECT item_id FROM _skip);
DELETE FROM _plan WHERE item_id IN (SELECT item_id FROM _skip);

-- 1) Ghi danh sách đơn vị trực thuộc (cơ quan đồng chủ trì giữ nguyên).
UPDATE "GoalTaskItems" g SET "SubAgencyIds" = i.ids
FROM _items i WHERE g."Id" = i.item_id AND g."SubAgencyIds" IS DISTINCT FROM i.ids;

-- 2) Phân giao.
CREATE TEMP TABLE _pairs AS SELECT item_id, agency_id FROM _plan;
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
SELECT 'Đã gán' AS ket_qua, i.label AS ma, i.agencies AS don_vi_truc_thuoc,
       concat_ws('; ', CASE WHEN i.is_general THEN 'Nhiệm vụ chung' END, 'Đồng chủ trì: ' || co."Name") AS ghi_chu
FROM _items i JOIN "GoalTaskItems" g ON g."Id" = i.item_id LEFT JOIN "Agencies" co ON co."Id" = g."AssignedAgencyId"
UNION ALL
SELECT 'Bỏ qua (đã giao đơn vị khác)', s.label, s.agencies, s.current_ids::text FROM _skip s
ORDER BY 1, 2;
