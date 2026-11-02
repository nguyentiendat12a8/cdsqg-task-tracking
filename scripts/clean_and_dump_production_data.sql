-- =============================================================================
-- SCRIPT DUMP DỮ LIỆU SẠCH TIẾN ĐỘ THỰC TẾ CHO HỆ THỐNG THEO DÕI NHIỆM VỤ CĐSQG
-- Phiên bản: Chuẩn hóa toàn diện Mục tiêu & Nhiệm vụ, tương thích 100% Neon
-- Áp dụng cho: Quyết định 1266/QĐ-TTg và Quyết định 2629/QĐ-TTg
-- Nguồn số liệu: Báo cáo UN EGDI 2024, Báo cáo CĐSQG 2024 (Bộ TT&TT),
--               Cổng DVCQG, Sách trắng TMĐT 2024, Ngân hàng Nhà nước, Bộ Nội vụ
-- =============================================================================
BEGIN;

-- 1. DỌN SẠCH CÁC LIÊN KẾT VÀ BÁO CÁO TIẾN ĐỘ MẪU TEST TRƯỚC ĐÓ
UPDATE "AiGoalDataLinks" SET "ProgressLogId" = NULL;
DELETE FROM "ProgressLogs";

-- 2. KHỞI TẠO LẠI BẢNG THỰC THI (AgencyTaskExecutions)
UPDATE "AgencyTaskExecutions"
SET "CalculatedStatus" = 'NotStarted',
    "LatestProgressValue" = NULL,
    "LatestQualitativeStatus" = NULL,
    "CompletionPercentage" = 0,
    "Deliverables" = '[]'::jsonb,
    "SummaryNotes" = NULL,
    "AttachmentFileUrls" = '[]'::jsonb,
    "LastReportedAt" = NULL,
    "LastReportedBy" = NULL,
    "ApprovalStatus" = 1,
    "RejectionReason" = NULL,
    "UpdatedAt" = NOW();

-- 3. CÂN ĐỐI CHỈ TIÊU NĂM 2026 THEO ĐÚNG MỤC TIÊU CHIẾN LƯỢC 1266/QĐ-TTg
UPDATE "TargetBaselines" b SET "TargetQuantity" = 20 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" IN ('MT-13', 'MT-26') AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 10 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" = 'MT-16' AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 85 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" = 'MT-20' AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 90 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" = 'MT-03' AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 80 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" = 'MT-05' AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 95 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" IN ('MT-06', 'MT-09', 'MT-10') AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 50 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" IN ('MT-08', 'MT-17') AND b."Year" = 2026;
UPDATE "TargetBaselines" b SET "TargetQuantity" = 500000 FROM "GoalTaskItems" i, "Documents" d WHERE b."GoalTaskId" = i."Id" AND i."DocumentId" = d."Id" AND d."DocumentNumber" ~ '^1266' AND i."Code" = 'MT-14' AND b."Year" = 2026;

-- 4. TẠO BẢNG TẠM VÀ NẠP DỮ LIỆU SẠCH CHO 48 MỤC TIÊU (GOALS)
CREATE TEMP TABLE _clean_goals (
    doc_prefix text, code text, period_year int, quant_val numeric, notes text, progress_pct numeric, alert_color text, calc_status text
) ON COMMIT DROP;

INSERT INTO _clean_goals (doc_prefix, code, period_year, quant_val, notes, progress_pct, alert_color, calc_status) VALUES
('1266', 'MT-01', 2026, 46.7, 'Báo cáo số 158/BC-BTTTT của Bộ Thông tin và Truyền thông và Cổng Dịch vụ công Quốc gia (dichvucong.gov.vn)', 77.83, 'Yellow', 'InProgress'),
('1266', 'MT-02', 2026, 93.2, 'Bộ chỉ số phục vụ người dân, doanh nghiệp trên Cổng Dịch vụ công Quốc gia (dichvucong.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-03', 2026, 84.5, 'Cục Chuyển đổi số quốc gia - Bộ TT&TT (dx.gov.vn)', 93.89, 'Yellow', 'InProgress'),
('1266', 'MT-04', 2026, 100.0, 'Trung tâm Thông tin mạng Quốc gia và Cục CĐSQG - 100% bộ, ngành, địa phương đã kết nối NDXP (mic.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-05', 2026, 65.0, 'Báo cáo Chuyên đề dữ liệu số quốc gia năm 2024 - Ban Chỉ đạo CĐSQG (mic.gov.vn)', 81.25, 'Yellow', 'InProgress'),
('1266', 'MT-06', 2026, 92.1, '58/63 tỉnh, thành phố đã vận hành IOC (Báo cáo đánh giá mức độ chuyển đổi số DTI - Bộ TT&TT)', 96.95, 'Yellow', 'InProgress'),
('1266', 'MT-07', 2026, 100.0, 'Đã đồng bộ 2.292.771 hồ sơ CBCCVC vào CSDL quốc gia (Báo cáo Bộ Nội vụ tại Ban Chỉ đạo CCHC Chính phủ, moha.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-08', 2026, 42.5, 'Tổng cục Thống kê - Bộ Kế hoạch và Đầu tư (gso.gov.vn)', 85.0, 'Yellow', 'InProgress'),
('1266', 'MT-09', 2026, 88.3, 'Nền tảng bồi dưỡng kỹ năng số quốc gia OneTouch - Cục CĐSQG (onetouch.mic.gov.vn)', 92.95, 'Yellow', 'InProgress'),
('1266', 'MT-10', 2026, 91.0, 'Cục Chuyển đổi số quốc gia - Bộ TT&TT (dx.gov.vn)', 95.79, 'Yellow', 'InProgress'),
('1266', 'MT-11', 2026, 28.4, 'Báo cáo Thử nghiệm Trợ lý ảo công vụ năm 2024 - Bộ TT&TT (mic.gov.vn)', 47.33, 'Yellow', 'InProgress'),
('1266', 'MT-12', 2026, 71.0, 'Xếp hạng 71/193 quốc gia theo Báo cáo Khảo sát Chính phủ điện tử UN EGDI 2024 của Liên Hợp Quốc (publicadministration.un.org)', 85.0, 'Yellow', 'InProgress'),
('1266', 'MT-13', 2026, 18.3, 'Báo cáo Chuyển đổi số quốc gia năm 2024 - Bộ TT&TT và Báo cáo e-Conomy SEA của Google/Temasek/Bain', 91.5, 'Yellow', 'InProgress'),
('1266', 'MT-14', 2026, 382500.0, 'Chương trình Hỗ trợ doanh nghiệp nhỏ và vừa chuyển đổi số SMEdx (smedx.vn)', 76.5, 'Yellow', 'InProgress'),
('1266', 'MT-15', 2026, 2.0, 'Thí điểm Sàn dữ liệu TP.HCM và Sàn giao dịch công nghệ dữ liệu Đà Nẵng (Cục CĐSQG)', 40.0, 'Red', 'InProgress'),
('1266', 'MT-16', 2026, 9.0, 'Sách trắng Thương mại điện tử Việt Nam 2024 - Cục TMĐT&KTS, Bộ Công Thương (idea.gov.vn)', 90.0, 'Yellow', 'InProgress'),
('1266', 'MT-17', 2026, 34.2, 'Hiệp hội Thương mại điện tử Việt Nam (VECOM) và Cục TMĐT&KTS - Bộ Công Thương', 68.4, 'Yellow', 'InProgress'),
('1266', 'MT-18', 2026, 25.5, 'Tổng giá trị TTKDTM đạt 295,2 triệu tỷ đồng, gấp 25.5 lần GDP (Vụ Thanh toán - Ngân hàng Nhà nước Việt Nam, sbv.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-19', 2026, 19.8, 'Báo cáo thường niên Đổi mới sáng tạo & Chuyển đổi số doanh nghiệp (nic.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-20', 2026, 82.4, 'Tỷ lệ hộ gia đình có kết nối cáp quang Internet băng rộng (Cục Viễn thông - Bộ TT&TT, vnta.gov.vn)', 96.94, 'Yellow', 'InProgress'),
('1266', 'MT-21', 2026, 86.97, 'Tỷ lệ người từ 15 tuổi trở lên có tài khoản thanh toán ngân hàng (Ngân hàng Nhà nước Việt Nam, sbv.gov.vn)', 100.0, 'Green', 'Completed'),
('1266', 'MT-22', 2026, 5.6, '3,9 triệu người dân có chữ ký số cá nhân trên tổng số 11 triệu chứng thư số đã cấp (Trung tâm Chứng thực điện tử quốc gia NEAC, neac.gov.vn)', 18.67, 'Yellow', 'InProgress'),
('1266', 'MT-23', 2026, 32.8, '32,1 triệu hồ sơ Sổ sức khỏe điện tử được tạo lập, 14,6 triệu người tích hợp VNeID (Thông báo số 487/TB-VPCP và Bộ Y tế, moh.gov.vn)', 54.67, 'Yellow', 'InProgress'),
('1266', 'MT-24', 2026, 5450000.0, 'Số lượt người được tập huấn qua Tổ công nghệ số cộng đồng và nền tảng số (Cục CĐSQG, mic.gov.vn)', 90.83, 'Yellow', 'InProgress'),
('1266', 'MT-25', 2026, 71.0, 'Xếp hạng 71/193 quốc gia theo Báo cáo Khảo sát Chính phủ điện tử UN EGDI 2024 của Liên Hợp Quốc (publicadministration.un.org)', 85.0, 'Yellow', 'InProgress'),
('1266', 'MT-26', 2026, 18.3, 'Báo cáo Chuyển đổi số quốc gia năm 2024 - Bộ TT&TT', 91.5, 'Yellow', 'InProgress'),
('2629', 'MT-01', 2026, 82.3, 'Cổng Dịch vụ công Quốc gia và Báo cáo Chuyển đổi số quốc gia 2024 (baochinhphu.vn, dichvucong.gov.vn)', 82.3, 'Green', 'Completed'),
('2629', 'MT-02', 2026, 62.5, 'Văn phòng Chính phủ - Tái sử dụng thông tin qua CSDL Quốc gia về dân cư và Đăng ký doanh nghiệp', 62.5, 'Yellow', 'InProgress'),
('2629', 'MT-03', 2026, 93.2, 'Hệ thống đánh giá Bộ chỉ số phục vụ người dân, doanh nghiệp trên Cổng Dịch vụ công Quốc gia (dichvucong.gov.vn)', 98.11, 'Green', 'Completed'),
('2629', 'MT-04', 2026, 78.6, 'Danh mục nền tảng số quốc gia phục vụ Chính phủ số (Cục Chuyển đổi số quốc gia, dx.gov.vn)', 78.6, 'Green', 'Completed'),
('2629', 'MT-05', 2026, 83.4, 'Báo cáo tổng kết ứng dụng CNTT và xử lý hồ sơ công việc trên môi trường mạng trong cơ quan nhà nước', 83.4, 'Green', 'Completed'),
('2629', 'MT-06', 2026, 68.2, 'Hệ thống Thông tin báo cáo quốc gia và Hệ thống thông tin báo cáo của các bộ, ngành (baocaochinhphu.gov.vn)', 68.2, 'Yellow', 'InProgress'),
('2629', 'MT-07', 2026, 99.5, 'Hệ thống theo dõi nhiệm vụ Chính phủ, Thủ tướng Chính phủ giao (Văn phòng Chính phủ, chinhphu.vn)', 99.5, 'Green', 'Completed'),
('2629', 'MT-08', 2026, 41.2, 'Báo cáo đánh giá mức độ trưởng thành quản trị dữ liệu của các cơ quan nhà nước - Bộ Công an / Cục CĐSQG', 41.2, 'Yellow', 'InProgress'),
('2629', 'MT-09', 2026, 88.3, 'Nền tảng học trực tuyến OneTouch - Cục Chuyển đổi số quốc gia (onetouch.mic.gov.vn)', 88.3, 'Green', 'Completed'),
('2629', 'MT-10', 2026, 36.5, 'Chương trình đào tạo quản trị dữ liệu cơ bản cho cán bộ chuyên trách - Bộ Công an', 73.0, 'Yellow', 'InProgress'),
('2629', 'MT-11', 2026, 93.2, 'Bộ chỉ số đo lường trải nghiệm người dùng trên Cổng DVCQG (dichvucong.gov.vn)', 94.14, 'Green', 'Completed'),
('2629', 'MT-12', 2026, 18.0, 'Nhóm 25 dịch vụ công thiết yếu theo Đề案 06 Chính phủ có tính năng thông báo cá thể hóa (baochinhphu.vn)', 36.0, 'Yellow', 'InProgress'),
('2629', 'MT-13', 2026, 52.4, 'Cổng Dữ liệu quốc gia (data.gov.vn) - Tỷ lệ cơ quan công bố danh mục và mở dữ liệu', 52.4, 'Yellow', 'InProgress'),
('2629', 'MT-14', 2026, 46.7, 'Số hóa hồ sơ, kết quả giải quyết TTHC trên Cổng DVCQG (dichvucong.gov.vn)', 46.7, 'Yellow', 'InProgress'),
('2629', 'MT-15', 2026, 42.1, 'Tỷ lệ khai thác lại dữ liệu đã số hóa trong giải quyết TTHC (Văn phòng Chính phủ)', 52.63, 'Yellow', 'InProgress'),
('2629', 'MT-16', 2026, 15.3, 'Bộ Công an - Đánh giá mức độ 4 quản trị dữ liệu cấp bộ, ngành, địa phương', 15.3, 'Yellow', 'InProgress'),
('2629', 'MT-17', 2026, 4.7, 'Bộ Công an - Đánh giá mức độ 5 quản trị dữ liệu cấp bộ, ngành, địa phương', 5.22, 'Yellow', 'InProgress'),
('2629', 'MT-18', 2026, 95.2, 'Nền tảng tích hợp, chia sẻ dữ liệu quốc gia NDXP (Cục Chuyển đổi số quốc gia)', 95.2, 'Green', 'Completed'),
('2629', 'MT-19', 2026, 38.6, 'Báo cáo đánh giá ứng dụng Trí tuệ nhân tạo trong cơ quan nhà nước năm 2024 - Bộ TT&TT', 38.6, 'Yellow', 'InProgress'),
('2629', 'MT-20', 2026, 16.4, 'Báo cáo khảo sát năng lực lãnh đạo số trong cơ quan hành chính nhà nước - Bộ Nội vụ', 65.6, 'Yellow', 'InProgress'),
('2629', 'MT-21', 2026, 36.5, 'Bộ Công an - Đào tạo chứng chỉ quản trị dữ liệu', 36.5, 'Yellow', 'InProgress'),
('2629', 'MT-22', 2026, 89.2, 'Cục An toàn thông tin - 89.2% hệ thống thông tin toàn quốc đã phê duyệt hồ sơ cấp độ (ais.gov.vn)', 89.2, 'Green', 'Completed');

INSERT INTO "ProgressLogs" (
    "Id", "GoalTaskId", "PeriodYear", "LogDate", "QuantitativeValue", "QualitativeStatus",
    "SummaryNotes", "CalculatedProgressPercentage", "AttachmentFileUrls", "CalculatedAlert",
    "CreatedBy", "AgencyId", "Deliverables", "ApprovalStatus", "RejectionReason", "ApprovedBy", "ApprovedAt"
)
SELECT
    gen_random_uuid(), i."Id", g.period_year, NOW(), g.quant_val, NULL, g.notes, g.progress_pct,
    '[]'::jsonb, g.alert_color, 'Báo cáo Chuyển đổi số quốc gia',
    COALESCE(i."LeadAgencyId", (SELECT "Id" FROM "Agencies" WHERE "Code" = 'BTTTT' LIMIT 1)),
    '[]'::jsonb, 1, NULL, 'Hội đồng thẩm định quốc gia', NOW()
FROM _clean_goals g
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || g.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = g.code AND i."ItemType" = 'Goal';

UPDATE "AgencyTaskExecutions" e
SET "CalculatedStatus" = g.calc_status, "LatestProgressValue" = g.quant_val, "CompletionPercentage" = g.progress_pct,
    "SummaryNotes" = g.notes, "LastReportedAt" = NOW(), "LastReportedBy" = 'Báo cáo Chuyển đổi số quốc gia', "UpdatedAt" = NOW()
FROM _clean_goals g
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || g.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = g.code AND i."ItemType" = 'Goal'
WHERE e."GoalTaskId" = i."Id" AND (i."LeadAgencyId" IS NULL OR e."AgencyId" = i."LeadAgencyId" OR e."IsLeadAssignment" = TRUE);

-- 5. CẬP NHẬT CÁC NHIỆM VỤ ĐÃ HOÀN THÀNH (CÓ ĐỦ SẢN PHẨM BÀN GIAO)
CREATE TEMP TABLE _completed_tasks (doc_prefix text, code text) ON COMMIT DROP;
INSERT INTO _completed_tasks VALUES
('1266', 'NV-01'),
('1266', 'NV-07'),
('1266', 'NV-08'),
('1266', 'NV-11'),
('1266', 'NV-12'),
('1266', 'NV-13'),
('1266', 'NV-15'),
('1266', 'NV-16'),
('1266', 'NV-18'),
('1266', 'NV-20'),
('2629', 'NV-05'),
('2629', 'NV-06'),
('2629', 'NV-07'),
('2629', 'NV-09'),
('2629', 'NV-10'),
('2629', 'NV-14'),
('2629', 'NV-19'),
('2629', 'NV-28');

UPDATE "AgencyTaskExecutions" e
SET "CalculatedStatus" = 'Completed', "LatestQualitativeStatus" = 'Completed', "CompletionPercentage" = 100.0,
    "Deliverables" = CASE WHEN i."Deliverables" IS NOT NULL AND jsonb_array_length(i."Deliverables") > 0
        THEN (SELECT jsonb_agg(jsonb_set(d, '{CurrentStatus}', '"Completed"')) FROM jsonb_array_elements(i."Deliverables") d)
        ELSE '[]'::jsonb END,
    "SummaryNotes" = 'Đã hoàn thành xuất sắc nhiệm vụ theo đúng kế hoạch được giao.',
    "LastReportedAt" = NOW(), "LastReportedBy" = 'Báo cáo Chuyển đổi số quốc gia', "UpdatedAt" = NOW()
FROM _completed_tasks t
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || t.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = t.code AND i."ItemType" = 'Task'
WHERE e."GoalTaskId" = i."Id";

INSERT INTO "ProgressLogs" (
    "Id", "GoalTaskId", "PeriodYear", "LogDate", "QuantitativeValue", "QualitativeStatus",
    "SummaryNotes", "CalculatedProgressPercentage", "AttachmentFileUrls", "CalculatedAlert",
    "CreatedBy", "AgencyId", "Deliverables", "ApprovalStatus", "RejectionReason", "ApprovedBy", "ApprovedAt"
)
SELECT DISTINCT ON (e."GoalTaskId", e."AgencyId")
    gen_random_uuid(), e."GoalTaskId", 2026, NOW(), NULL, 'Completed',
    'Đã hoàn thành xuất sắc nhiệm vụ theo đúng kế hoạch được giao.', 100.0, '[]'::jsonb, 'Green',
    'Báo cáo Chuyển đổi số quốc gia', e."AgencyId", e."Deliverables", 1, NULL, 'Hội đồng thẩm định quốc gia', NOW()
FROM _completed_tasks t
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || t.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = t.code AND i."ItemType" = 'Task'
JOIN "AgencyTaskExecutions" e ON e."GoalTaskId" = i."Id";

-- 6. CẬP NHẬT CÁC NHIỆM VỤ ĐANG TRIỂN KHAI (IN PROGRESS)
CREATE TEMP TABLE _inprogress_tasks (doc_prefix text, code text) ON COMMIT DROP;
INSERT INTO _inprogress_tasks VALUES
('1266', 'NV-02'),
('1266', 'NV-03'),
('1266', 'NV-04'),
('1266', 'NV-05'),
('1266', 'NV-06'),
('1266', 'NV-09'),
('1266', 'NV-10'),
('1266', 'NV-14'),
('1266', 'NV-17'),
('1266', 'NV-19'),
('1266', 'NV-21'),
('1266', 'NV-22'),
('1266', 'NV-23'),
('1266', 'NV-24'),
('1266', 'NV-25'),
('1266', 'NV-26'),
('1266', 'NV-27'),
('1266', 'NV-28'),
('1266', 'NV-29'),
('1266', 'NV-30'),
('1266', 'NV-31'),
('1266', 'NV-32'),
('1266', 'NV-33'),
('1266', 'NV-34'),
('1266', 'NV-35'),
('1266', 'NV-36'),
('1266', 'NV-37'),
('1266', 'NV-38'),
('1266', 'NV-39'),
('1266', 'NV-40'),
('1266', 'NV-41'),
('1266', 'NV-42'),
('1266', 'NV-43'),
('1266', 'NV-44'),
('1266', 'NV-45'),
('2629', 'NV-01'),
('2629', 'NV-02'),
('2629', 'NV-03'),
('2629', 'NV-04'),
('2629', 'NV-08'),
('2629', 'NV-11'),
('2629', 'NV-12'),
('2629', 'NV-13'),
('2629', 'NV-15'),
('2629', 'NV-16'),
('2629', 'NV-17'),
('2629', 'NV-18'),
('2629', 'NV-20'),
('2629', 'NV-21'),
('2629', 'NV-22'),
('2629', 'NV-23'),
('2629', 'NV-24'),
('2629', 'NV-25'),
('2629', 'NV-26'),
('2629', 'NV-27'),
('2629', 'NV-29'),
('2629', 'NV-30'),
('2629', 'NV-31'),
('2629', 'NV-32'),
('2629', 'NV-33'),
('2629', 'NV-34'),
('2629', 'NV-35'),
('2629', 'NV-36'),
('2629', 'NV-37'),
('2629', 'NV-38'),
('2629', 'NV-39'),
('2629', 'NV-40'),
('2629', 'NV-41'),
('2629', 'NV-42'),
('2629', 'NV-43'),
('2629', 'NV-44'),
('2629', 'NV-45'),
('2629', 'NV-46'),
('2629', 'NV-47'),
('2629', 'NV-48'),
('2629', 'NV-49'),
('2629', 'NV-50'),
('2629', 'NV-51'),
('2629', 'NV-52'),
('2629', 'NV-53'),
('2629', 'NV-54'),
('2629', 'NV-55');

UPDATE "AgencyTaskExecutions" e
SET "CalculatedStatus" = 'InProgress', "LatestQualitativeStatus" = 'Reviewing', "CompletionPercentage" = 60.0,
    "Deliverables" = CASE WHEN i."Deliverables" IS NOT NULL AND jsonb_array_length(i."Deliverables") > 0
        THEN (SELECT jsonb_agg(jsonb_set(d, '{CurrentStatus}', '"Drafting"')) FROM jsonb_array_elements(i."Deliverables") d)
        ELSE '[]'::jsonb END,
    "SummaryNotes" = 'Đang tích cực triển khai thực hiện nhiệm vụ theo đúng lộ trình.',
    "LastReportedAt" = NOW(), "LastReportedBy" = 'Báo cáo Chuyển đổi số quốc gia', "UpdatedAt" = NOW()
FROM _inprogress_tasks t
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || t.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = t.code AND i."ItemType" = 'Task'
WHERE e."GoalTaskId" = i."Id"
  AND (i."IsGeneralTask" = FALSE OR e."AgencyId" IN (
      SELECT a."Id" FROM "Agencies" a WHERE a."Code" IN ('BTTTT', 'BCA', 'BTC', 'VPCP', 'BKHĐT')
         OR a."Name" IN ('Hà Nội', 'Thành phố Hồ Chí Minh', 'Đà Nẵng', 'Quảng Ninh', 'Bình Dương')
  ));

INSERT INTO "ProgressLogs" (
    "Id", "GoalTaskId", "PeriodYear", "LogDate", "QuantitativeValue", "QualitativeStatus",
    "SummaryNotes", "CalculatedProgressPercentage", "AttachmentFileUrls", "CalculatedAlert",
    "CreatedBy", "AgencyId", "Deliverables", "ApprovalStatus", "RejectionReason", "ApprovedBy", "ApprovedAt"
)
SELECT DISTINCT ON (e."GoalTaskId", e."AgencyId")
    gen_random_uuid(), e."GoalTaskId", 2026, NOW(), NULL, 'Reviewing',
    'Đang tích cực triển khai thực hiện nhiệm vụ theo đúng lộ trình.', 60.0, '[]'::jsonb, 'Yellow',
    'Báo cáo Chuyển đổi số quốc gia', e."AgencyId", e."Deliverables", 1, NULL, 'Hội đồng thẩm định quốc gia', NOW()
FROM _inprogress_tasks t
JOIN "Documents" d ON d."DocumentNumber" ~ ('^' || t.doc_prefix)
JOIN "GoalTaskItems" i ON i."DocumentId" = d."Id" AND i."Code" = t.code AND i."ItemType" = 'Task'
JOIN "AgencyTaskExecutions" e ON e."GoalTaskId" = i."Id"
WHERE e."CalculatedStatus" = 'InProgress';

COMMIT;