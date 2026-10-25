-- ==========================================================================
-- SEED SCRIPT: AI GOAL KEYWORDS CHO QUYẾT ĐỊNH 1266 VÀ 2629
-- Idempotent script: An toàn chạy trên cả Neon Production và Local PostgreSQL
-- ==========================================================================

-- 1. Thêm cột DocumentId nếu chưa có
ALTER TABLE "AiGoalKeywords" ADD COLUMN IF NOT EXISTS "DocumentId" uuid NOT NULL DEFAULT '12660000-0000-0000-0000-000000001266';

-- 2. Cập nhật các bản ghi hiện tại về Document 1266 nếu đang null/trống
UPDATE "AiGoalKeywords" SET "DocumentId" = '12660000-0000-0000-0000-000000001266' WHERE "DocumentId" IS NULL;

-- 3. Đổi ràng buộc duy nhất từ GoalCode sang (DocumentId, GoalCode)
DROP INDEX IF EXISTS "IX_AiGoalKeywords_GoalCode";
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AiGoalKeywords_DocumentId_GoalCode" ON "AiGoalKeywords" ("DocumentId", "GoalCode");

-- 4. Làm sạch KeywordsEn cho Quyết định 1266 (chỉ MT-12 và MT-25 giữ lại tiếng Anh, 24 mục tiêu nội địa chuyển về rỗng)
UPDATE "AiGoalKeywords"
SET "KeywordsEn" = ''
WHERE "DocumentId" = '12660000-0000-0000-0000-000000001266'
  AND "GoalCode" NOT IN ('MT-12', 'MT-25');

-- 5. Nạp / Đồng bộ 26 mục tiêu QĐ 1266
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000001', '12660000-0000-0000-0000-000000001266', 'MT-01', '100% hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa', 'số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử, bưu chính công ích', '', '%', 'Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000002', '12660000-0000-0000-0000-000000001266', 'MT-02', '99% người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến', 'đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng', '', '%', 'Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000003', '12660000-0000-0000-0000-000000001266', 'MT-03', '100% bộ, ngành, địa phương sử dụng thống nhất, hiệu quả các nền tảng số dùng chung quốc gia phục vụ chỉ đạo, điều hành và cung cấp dịch vụ công', 'nền tảng số dùng chung, bộ ngành địa phương, chỉ đạo điều hành, dịch vụ công, trục liên thông văn bản, ndxp', '', '%', 'Xác định tỷ lệ bộ, ngành, địa phương sử dụng nền tảng số dùng chung quốc gia phục vụ chỉ đạo điều hành và dịch vụ công (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000004', '12660000-0000-0000-0000-000000001266', 'MT-04', '100% hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu', 'kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, tích hợp dữ liệu', '', '%', 'Xác định tỷ lệ hệ thống thông tin có kết nối, chia sẻ dữ liệu qua nền tảng tích hợp, chia sẻ dữ liệu (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000005', '12660000-0000-0000-0000-000000001266', 'MT-05', '100% cơ sở dữ liệu dùng chung quốc gia, ngành, lĩnh vực, cấp tỉnh được chuẩn hóa, kết nối, phân quyền và sẵn sàng khai thác cho AI', 'cơ sở dữ liệu dùng chung, csdl quốc gia, csdl chuyên ngành, chuẩn hóa dữ liệu, sẵn sàng khai thác ai, trí tuệ nhân tạo', '', '%', 'Xác định tỷ lệ cơ sở dữ liệu dùng chung quốc gia, ngành, địa phương được chuẩn hóa và sẵn sàng kết nối, khai thác AI (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000006', '12660000-0000-0000-0000-000000001266', 'MT-06', '100% trung tâm điều hành thông minh cấp tỉnh được nâng cấp theo hướng tích hợp AI phục vụ phân tích, dự báo, cảnh báo, giám sát và hỗ trợ ra quyết định', 'trung tâm điều hành thông minh, ioc, cấp tỉnh, tích hợp ai, dự báo cảnh báo, giám sát ra quyết định', '', '%', 'Xác định tỷ lệ trung tâm điều hành thông minh (IOC) cấp tỉnh được nâng cấp tích hợp AI (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000007', '12660000-0000-0000-0000-000000001266', 'MT-07', '100% cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử theo quy định', 'cán bộ công chức viên chức, hồ sơ điện tử, quản lý cán bộ, cơ sở dữ liệu cán bộ công chức, cbccvc, bộ nội vụ', '', '%', 'Xác định tỷ lệ cán bộ, công chức, viên chức trong cơ quan nhà nước có hồ sơ điện tử (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000008', '12660000-0000-0000-0000-000000001266', 'MT-08', '70% số chỉ tiêu thống kê thuộc các hệ thống chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính', 'chỉ tiêu thống kê, dữ liệu hành chính, hệ thống chỉ tiêu, tổng cục thống kê, cục thống kê, số liệu hành chính', '', '%', 'Xác định tỷ lệ chỉ tiêu thống kê được biên soạn từ nguồn dữ liệu hành chính (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000009', '12660000-0000-0000-0000-000000001266', 'MT-09', '100% cán bộ lãnh đạo, cán bộ, công chức, viên chức trong cơ quan nhà nước được đào tạo, bồi dưỡng, cập nhật kiến thức, kỹ năng số', 'đào tạo kỹ năng số, bồi dưỡng kiến thức số, cán bộ công chức, chuyển đổi số công vụ, tập huấn kỹ năng số', '', '%', 'Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng cập nhật kỹ năng số (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000010', '12660000-0000-0000-0000-000000001266', 'MT-10', '100% cán bộ chuyên trách về chuyển đổi số, công nghệ thông tin được đào tạo, bồi dưỡng, cập nhật công nghệ số', 'chuyên trách cntt, chuyên trách chuyển đổi số, bồi dưỡng công nghệ số, cán bộ công nghệ thông tin', '', '%', 'Xác định tỷ lệ cán bộ chuyên trách chuyển đổi số, CNTT được đào tạo, bồi dưỡng nâng cao công nghệ số (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000011', '12660000-0000-0000-0000-000000001266', 'MT-11', '100% cán bộ, công chức, viên chức có tài khoản và được đào tạo sử dụng nền tảng trợ lý AI công vụ', 'trợ lý ai công vụ, trợ lý ảo công vụ, tài khoản trợ lý ai, đào tạo trợ lý ảo, ai phục vụ cán bộ', '', '%', 'Xác định tỷ lệ cán bộ, công chức, viên chức có tài khoản và được tập huấn sử dụng trợ lý AI công vụ (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000012', '12660000-0000-0000-0000-000000001266', 'MT-12', 'Việt Nam thuộc nhóm 50 nước dẫn đầu trên thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)', 'chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí', 'e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam', 'rank', 'Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000013', '12660000-0000-0000-0000-000000001266', 'MT-13', 'Quy mô kinh tế số đạt tối thiểu 30% GDP', 'kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, giá trị gia tăng', '', '%', 'Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000014', '12660000-0000-0000-0000-000000001266', 'MT-14', 'Hỗ trợ tối thiểu 500.000 doanh nghiệp nhỏ và vừa chuyển đổi số', 'doanh nghiệp nhỏ và vừa, sme, chuyển đổi số doanh nghiệp, smedx, hỗ trợ doanh nghiệp', '', 'doanh nghiệp', 'Xác định số lượng doanh nghiệp nhỏ và vừa (SME) được hỗ trợ chuyển đổi số (đơn vị: doanh nghiệp).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000015', '12660000-0000-0000-0000-000000001266', 'MT-15', 'Phát triển, thúc đẩy và đưa vào sử dụng tối thiểu 5 sàn dữ liệu, bảo đảm an toàn, tuân thủ quy định của pháp luật', 'sàn dữ liệu, trung tâm giao dịch dữ liệu, thị trường dữ liệu, data exchange, sàn giao dịch dữ liệu', '', 'sàn', 'Xác định số lượng sàn dữ liệu / trung tâm giao dịch dữ liệu được đưa vào vận hành (đơn vị: sàn).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000016', '12660000-0000-0000-0000-000000001266', 'MT-16', 'Tỷ trọng doanh thu thương mại điện tử so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng đạt từ 15 - 17%', 'thương mại điện tử, tmđt, bán lẻ, doanh thu dịch vụ tiêu dùng, sách trắng tmđt, tỷ trọng b2c', '', '%', 'Xác định tỷ trọng doanh thu thương mại điện tử (B2C) so với tổng mức bán lẻ hàng hóa và doanh thu dịch vụ tiêu dùng của cả nước (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000017', '12660000-0000-0000-0000-000000001266', 'MT-17', 'Phấn đấu đạt 45% số doanh nghiệp nhỏ và vừa hoạt động trong lĩnh vực thương mại tham gia các sàn thương mại điện tử lớn trong và ngoài nước', 'sme thương mại, sàn thương mại điện tử, tham gia sàn tmđt, shopee, lazada, tiki, alibaba, amazon, bán hàng trực tuyến', '', '%', 'Xác định tỷ lệ doanh nghiệp nhỏ và vừa ngành thương mại tham gia các sàn thương mại điện tử (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000018', '12660000-0000-0000-0000-000000001266', 'MT-18', 'Giá trị thanh toán không dùng tiền mặt gấp 30 lần GDP', 'thanh toán không dùng tiền mặt, thanh toán điện tử, ngân hàng nhà nước, gdp, lần gdp, giao dịch phi tiền mặt', '', 'lần GDP', 'Xác định tỷ lệ giá trị thanh toán không dùng tiền mặt so với GDP (số lần GDP).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000019', '12660000-0000-0000-0000-000000001266', 'MT-19', 'Trên 40% doanh nghiệp nhỏ và vừa tiếp cận, sử dụng nền tảng “mô hình như một dịch vụ”, tác nhân AI hoặc giải pháp AI dùng chung', 'mô hình như một dịch vụ, maas, tác nhân ai, ai agent, giải pháp ai dùng chung, sme ứng dụng ai', '', '%', 'Xác định tỷ lệ doanh nghiệp nhỏ và vừa tiếp cận hoặc sử dụng giải pháp AI / mô hình như một dịch vụ (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000020', '12660000-0000-0000-0000-000000001266', 'MT-20', 'Hạ tầng mạng băng rộng cáp quang phủ 100% hộ gia đình', 'cáp quang, băng rộng cố định, ftth, hộ gia đình, thuê bao internet, phủ cáp quang', '', '%', 'Xác định tỷ lệ hộ gia đình có kết nối mạng băng rộng cáp quang (FTTH) (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000021', '12660000-0000-0000-0000-000000001266', 'MT-21', 'Tỷ lệ dân số từ 15 tuổi trở lên có tài khoản giao dịch tại ngân hàng hoặc tổ chức được phép khác đạt tối thiểu 95%', 'tài khoản giao dịch, ngân hàng, thanh toán, 15 tuổi, tổ chức tín dụng, tài khoản thanh toán', '', '%', 'Xác định tỷ lệ dân số từ 15 tuổi trở lên có tài khoản thanh toán / giao dịch tại ngân hàng (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000022', '12660000-0000-0000-0000-000000001266', 'MT-22', 'Tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân đạt trên 70%', 'chữ ký số, chữ ký điện tử cá nhân, 18 tuổi, chứng thư số, ký số cá nhân, vnpt smartca, viettel ca', '', '%', 'Xác định tỷ lệ dân số từ 18 tuổi trở lên có chữ ký số hoặc chữ ký điện tử cá nhân (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000023', '12660000-0000-0000-0000-000000001266', 'MT-23', 'Phấn đấu tỷ lệ người dân được lập sổ sức khỏe điện tử đạt 100%', 'sổ sức khỏe điện tử, hồ sơ sức khỏe điện tử, y tế số, vneid sức khỏe, bộ y tế, bảo hiểm y tế', '', '%', 'Xác định tỷ lệ người dân được lập sổ / hồ sơ sức khỏe điện tử (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000024', '12660000-0000-0000-0000-000000001266', 'MT-24', 'Đào tạo, bồi dưỡng, tập huấn kỹ năng số cơ bản cho tối thiểu 10.000.000 lượt người trong độ tuổi lao động', 'kỹ năng số cơ bản, độ tuổi lao động, tập huấn kỹ năng số, phổ cập kỹ năng số, lượt người', '', 'lượt người', 'Xác định số lượt người trong độ tuổi lao động được đào tạo, bồi dưỡng kỹ năng số cơ bản (đơn vị: lượt người).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000025', '12660000-0000-0000-0000-000000001266', 'MT-25', 'Việt Nam thuộc nhóm 30 nước dẫn đầu thế giới về chính phủ điện tử/chính phủ số (theo báo cáo đánh giá của Liên hợp quốc)', 'chính phủ điện tử, chính phủ số, egdi, liên hợp quốc, xếp hạng, thứ hạng, un, vị trí', 'e-government, egdi, survey, united nations, un, ranking, rank, index, vietnam, viet nam', 'rank', 'Xác định thứ hạng Chính phủ điện tử (EGDI - E-Government Development Index) của Việt Nam theo báo cáo Liên Hợp Quốc (UN E-Government Survey). Đơn vị là thứ hạng (rank).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b1000000-0000-0000-0000-000000000026', '12660000-0000-0000-0000-000000001266', 'MT-26', 'Quy mô kinh tế số đạt tối thiểu 50% GDP, kinh tế số trở thành một trong những động lực phát triển chủ yếu của nền kinh tế', 'kinh tế số, gdp, quy mô kinh tế số, tỷ trọng kinh tế số, 50% gdp, động lực phát triển', '', '%', 'Xác định tỷ trọng quy mô kinh tế số so với GDP của Việt Nam (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();

-- 6. Nạp / Đồng bộ 22 mục tiêu QĐ 2629
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000001', '26290000-0000-0000-0000-000000002629', 'MT-01', 'Tỉ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện.', 'dịch vụ công trực tuyến toàn trình, dvctt toàn trình, thủ tục hành chính đủ điều kiện, tthc đủ điều kiện, dịch vụ công toàn trình, cung cấp trực tuyến toàn trình', '', '%', 'Xác định tỷ lệ dịch vụ công trực tuyến toàn trình trên tổng số thủ tục hành chính có đủ điều kiện (%). Chỉ lấy số liệu thực tế đã công bố, không lấy ngưỡng mục tiêu.', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000002', '26290000-0000-0000-0000-000000002629', 'MT-02', 'Tỉ lệ thông tin, giấy tờ, tài liệu trong các thủ tục hành chính liên quan đến hoạt động sản xuất, kinh doanh chỉ cung cấp một lần cho cơ quan hành chính nhà nước.', 'chỉ cung cấp một lần, sản xuất kinh doanh, thủ tục hành chính doanh nghiệp, giấy tờ tài liệu một lần, chia sẻ thông tin thủ tục, cung cấp một lần', '', '%', 'Xác định tỷ lệ thông tin, giấy tờ trong TTHC về sản xuất kinh doanh chỉ phải cung cấp một lần cho cơ quan nhà nước (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000003', '26290000-0000-0000-0000-000000002629', 'MT-03', 'Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.', 'đánh giá hài lòng, mức độ hài lòng, dịch vụ công trực tuyến, người dân doanh nghiệp, cổng dịch vụ công quốc gia, chỉ số hài lòng', '', '%', 'Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000004', '26290000-0000-0000-0000-000000002629', 'MT-04', 'Tỉ lệ các nền tảng số quốc gia, nền tảng số dùng chung của ngành, lĩnh vực phục vụ Chính phủ số được hoàn thành và đưa vào sử dụng thống nhất từ Trung ương đến địa phương theo đúng kế hoạch.', 'nền tảng số quốc gia, nền tảng số dùng chung, chính phủ số, hoàn thành đưa vào sử dụng, sử dụng thống nhất, nền tảng ngành lĩnh vực', '', '%', 'Xác định tỷ lệ nền tảng số quốc gia, nền tảng dùng chung phục vụ Chính phủ số hoàn thành và đưa vào sử dụng thống nhất (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000005', '26290000-0000-0000-0000-000000002629', 'MT-05', 'Tỉ lệ hồ sơ công việc tại cấp bộ, tỉnh, cấp xã được xử lý trên môi trường điện tử (trừ hồ sơ công việc thuộc phạm vi bí mật nhà nước) và sử dụng chữ ký số cá nhân để giải quyết công việc.', 'hồ sơ công việc, môi trường điện tử, cấp bộ, cấp tỉnh, cấp xã, chữ ký số cá nhân, xử lý văn bản điện tử, hệ thống quản lý văn bản', '', '%', 'Xác định tỷ lệ hồ sơ công việc tại cấp bộ, tỉnh, xã được xử lý trên môi trường điện tử và có dùng chữ ký số cá nhân (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000006', '26290000-0000-0000-0000-000000002629', 'MT-06', 'Tỉ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo.', 'chế độ báo cáo, hệ thống thông tin báo cáo, hệ thống báo cáo quốc gia, báo cáo điện tử, số hóa chế độ báo cáo', '', '%', 'Xác định tỷ lệ chế độ báo cáo của các cơ quan được thực hiện trên Hệ thống thông tin báo cáo (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000007', '26290000-0000-0000-0000-000000002629', 'MT-07', 'Tỉ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao cho các bộ, ngành, địa phương được theo dõi, giám sát, đánh giá trên môi trường điện tử.', 'nhiệm vụ chính phủ giao, thủ tướng chính phủ giao, theo dõi giám sát đánh giá, môi trường điện tử, hệ thống theo dõi nhiệm vụ', '', '%', 'Xác định tỷ lệ nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao được theo dõi, giám sát trên môi trường điện tử (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000008', '26290000-0000-0000-0000-000000002629', 'MT-08', 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 3 mức độ trưởng thành về quản trị dữ liệu.', 'trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 3, mức độ trưởng thành dữ liệu, đánh giá quản trị dữ liệu', '', '%', 'Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 3 về mức độ trưởng thành quản trị dữ liệu (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000009', '26290000-0000-0000-0000-000000002629', 'MT-09', 'Tỉ lệ cán bộ, công chức, viên chức được đào tạo kỹ năng số cơ bản.', 'đào tạo kỹ năng số, kỹ năng số cơ bản, cán bộ công chức viên chức, bồi dưỡng kỹ năng số, phổ cập kỹ năng số công chức', '', '%', 'Xác định tỷ lệ cán bộ, công chức, viên chức được đào tạo, bồi dưỡng kỹ năng số cơ bản (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000010', '26290000-0000-0000-0000-000000002629', 'MT-10', 'Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.', 'chuyên trách cntt, chuyên trách chuyển đổi số, chứng chỉ quản trị dữ liệu, đào tạo quản trị dữ liệu cơ bản', '', '%', 'Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000011', '26290000-0000-0000-0000-000000002629', 'MT-11', 'Tỉ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến.', 'đánh giá hài lòng dvctt, mức độ hài lòng người dân, cổng dịch vụ công, sự hài lòng dịch vụ công trực tuyến', '', '%', 'Xác định tỷ lệ người dân, doanh nghiệp đánh giá hài lòng khi sử dụng dịch vụ công trực tuyến (giai đoạn 2028-2030) (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000012', '26290000-0000-0000-0000-000000002629', 'MT-12', 'Tỉ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, được AI hỗ trợ “cá thể hóa”, thông báo hoặc gợi ý hành động theo các mốc sự kiện quan trọng trong cuộc đời.', 'dịch vụ công thiết yếu, dịch vụ công chủ động, ai cá thể hóa, gợi ý hành động, mốc sự kiện cuộc đời, trợ lý ai dịch vụ công', '', '%', 'Xác định tỷ lệ dịch vụ công trực tuyến thiết yếu được triển khai ở mức chủ động, có AI hỗ trợ cá thể hóa theo mốc sự kiện (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000013', '26290000-0000-0000-0000-000000002629', 'MT-13', 'Tỉ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng chuẩn.', 'dữ liệu mở, open data, dữ liệu mở đúng chuẩn, cổng dữ liệu quốc gia, data.gov.vn, cung cấp dữ liệu mở', '', '%', 'Xác định tỷ lệ cơ quan nhà nước cung cấp dữ liệu mở đúng tiêu chuẩn, định dạng mở (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000014', '26290000-0000-0000-0000-000000002629', 'MT-14', 'Tỉ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa.', 'số hóa hồ sơ, số hóa kết quả, thủ tục hành chính, tthc, hồ sơ trực tuyến, dịch vụ công, lưu trữ điện tử', '', '%', 'Xác định tỷ lệ hồ sơ, kết quả giải quyết thủ tục hành chính được số hóa (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000015', '26290000-0000-0000-0000-000000002629', 'MT-15', 'Tỉ lệ khai thác, sử dụng lại thông tin, dữ liệu đã được số hóa trong giải quyết thủ tục hành chính, dịch vụ công.', 'khai thác dữ liệu số hóa, sử dụng lại thông tin, tái sử dụng dữ liệu, số hóa trong tthc, không yêu cầu nộp lại giấy tờ', '', '%', 'Xác định tỷ lệ khai thác, tái sử dụng thông tin, dữ liệu đã số hóa trong giải quyết TTHC, dịch vụ công (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000016', '26290000-0000-0000-0000-000000002629', 'MT-16', 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu.', 'trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 4, mức độ trưởng thành dữ liệu cấp 4', '', '%', 'Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 4 mức độ trưởng thành về quản trị dữ liệu (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000017', '26290000-0000-0000-0000-000000002629', 'MT-17', 'Tỉ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu.', 'trưởng thành về quản trị dữ liệu, quản trị dữ liệu cấp độ 5, mức độ trưởng thành dữ liệu tối ưu cấp 5', '', '%', 'Xác định tỷ lệ bộ, ngành, địa phương đạt cấp độ 5 mức độ trưởng thành về quản trị dữ liệu (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000018', '26290000-0000-0000-0000-000000002629', 'MT-18', 'Tỉ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu.', 'kết nối chia sẻ dữ liệu, hệ thống thông tin, nền tảng chia sẻ dữ liệu quốc gia, ndxp, lgsp, dịch vụ chia sẻ dữ liệu', '', '%', 'Xác định tỷ lệ hệ thống thông tin được kết nối, chia sẻ dữ liệu qua dịch vụ chia sẻ dữ liệu (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000019', '26290000-0000-0000-0000-000000002629', 'MT-19', 'Tỉ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI để phục vụ công tác chỉ đạo, điều hành.', 'ứng dụng ai, trí tuệ nhân tạo, chỉ đạo điều hành, trợ lý ảo, ai phục vụ điều hành, cơ quan tổ chức sử dụng ai', '', '%', 'Xác định tỷ lệ cơ quan, tổ chức sử dụng ít nhất 01 ứng dụng AI phục vụ công tác chỉ đạo, điều hành (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000020', '26290000-0000-0000-0000-000000002629', 'MT-20', 'Tỉ lệ đội ngũ lãnh đạo của từng cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học, kỹ thuật, chuyển đổi số phù hợp với yêu cầu nhiệm vụ.', 'đội ngũ lãnh đạo, lãnh đạo chuyển đổi số, chuyên môn kỹ thuật, kinh nghiệm khoa học kỹ thuật chuyển đổi số, cán bộ lãnh đạo', '', '%', 'Xác định tỷ lệ đội ngũ lãnh đạo của cơ quan, tổ chức có chuyên môn, kinh nghiệm về khoa học kỹ thuật, chuyển đổi số (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000021', '26290000-0000-0000-0000-000000002629', 'MT-21', 'Tỉ lệ cán bộ chuyên trách về công nghệ thông tin, chuyển đổi số của bộ, ngành, địa phương được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản.', 'chuyên trách cntt chuyển đổi số, chứng chỉ quản trị dữ liệu cơ bản, đào tạo chuyên trách quản trị dữ liệu', '', '%', 'Xác định tỷ lệ cán bộ chuyên trách CNTT, CĐS được đào tạo đạt chứng chỉ quản trị dữ liệu cơ bản (mục tiêu 2028-2030) (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();
INSERT INTO "AiGoalKeywords" ("Id", "DocumentId", "GoalCode", "GoalTitle", "KeywordsVi", "KeywordsEn", "ExpectedUnit", "ExtractionPrompt", "CreatedAt", "UpdatedAt")
VALUES ('b2000000-0000-0000-0000-000000000022', '26290000-0000-0000-0000-000000002629', 'MT-22', 'Tỉ lệ các hệ thống thông tin phục vụ phát triển Chính phủ số phải được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ trước khi đưa vào vận hành, sử dụng; được kiểm tra, đánh giá an ninh mạng định kỳ.', 'an ninh mạng theo cấp độ, an toàn thông tin theo cấp độ, phê duyệt hồ sơ cấp độ, kiểm tra an ninh mạng định kỳ, bảo đảm an toàn thông tin', '', '%', 'Xác định tỷ lệ hệ thống thông tin Chính phủ số được phê duyệt hồ sơ đề xuất bảo đảm an ninh mạng theo cấp độ và kiểm tra định kỳ (%).', NOW(), NOW())
ON CONFLICT ("DocumentId", "GoalCode") DO UPDATE SET
  "GoalTitle" = EXCLUDED."GoalTitle",
  "KeywordsVi" = EXCLUDED."KeywordsVi",
  "KeywordsEn" = EXCLUDED."KeywordsEn",
  "ExpectedUnit" = EXCLUDED."ExpectedUnit",
  "ExtractionPrompt" = EXCLUDED."ExtractionPrompt",
  "UpdatedAt" = NOW();

-- Hoàn tất kiểm tra số lượng
SELECT "DocumentId", count(*) FROM "AiGoalKeywords" GROUP BY "DocumentId";