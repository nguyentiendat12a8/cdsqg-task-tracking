-- =========================================================================
-- SCRIPT CẬP NHẬT DỮ LIỆU THẬT TỪ EXCEL CHO 51 BỘ, NGÀNH VÀ ĐỊA PHƯƠNG
-- Dành cho môi trường Production (PostgreSQL)
-- Bao gồm: Kế hoạch CĐS 5 năm, VB phúc đáp, Cập nhật 1266 và Cán bộ đầu mối
-- =========================================================================

BEGIN;

-- 0. Dọn dẹp dữ liệu cán bộ đầu mối / kế hoạch thử nghiệm trên các bản ghi đặc biệt
UPDATE "Agencies"
SET "ContactPersons" = '[]'
WHERE "Code" IN ('ALL_MINISTRIES_DIRECT', 'ALL_AGENCIES', 'ALL_MINISTRIES', 'ALL_PROVINCES', 'ALL_PROVINCES_UBND')
   OR "Id" = '00000000-0000-0000-0000-000000009995';

-- Ministry: Bộ Quốc phòng (Code: Bộ Quốc phòng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 6005/BQP-BTL86 ngày 13/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 7076/KH-BQP ngày 22/9/2026 của Bộ Quốc phòng ban hành Kế hoạch Chuyển đổi số giai đoạn 2026-2030", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Tùng Hưng", "Position": "Thiếu tướng. Phó tư lệnh Bộ Tư lệnh 86", "Department": "", "Phone": "0983 043 325", "Email": ""}, {"Name": "Nguyễn Kiên- Đại tá. Trưởng phòng Phần mền và cơ sở dữ liệu. Bộ Tư lệnh 86", "Position": "", "Department": "", "Phone": "0985101126", "Email": ""}, {"Name": "Phạm Trọng Linh", "Position": "Thượng úy. Trợ lý Phòng phần mềm và Cơ sở dữ liệu. Bộ Tư lệnh 86", "Department": "", "Phone": "0879 610 286", "Email": ""}]'::jsonb
WHERE "Id" = '2c924b28-141c-47ef-8aef-60cab4f76a62' OR "Code" = 'Bộ Quốc phòng';

-- Ministry: Bộ Công an (Code: BCA)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '01a3d089-be05-4bfb-b966-032fe2a51a21' OR "Code" = 'BCA';

-- Ministry: Bộ Ngoại giao (Code: Bộ Ngoại giao)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'd6ba55c2-4311-4452-bc23-8284c71f0b56' OR "Code" = 'Bộ Ngoại giao';

-- Ministry: Bộ Nội vụ (Code: BNV)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 8775/BNV ngày 12/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Quyết định số 428/QĐ-BNV ngày 14/4/2026 ban hành kế hoạch chuyể đổi số của Bộ nội vụ giai đoạn 2026-2030", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Đỗ Chí Dũng", "Position": "Giám đốc", "Department": "TTCNTT", "Phone": "0903 239 068", "Email": ""}, {"Name": "Trịnh Tuấn Chung -Trưởng phòng", "Position": "", "Department": "TT CNTT", "Phone": "0903 007 068", "Email": ""}, {"Name": "Nguyễn Thu Hương", "Position": "Phó Trưởng phòng-TTCNTT", "Department": "", "Phone": "0983 341 213", "Email": ""}]'::jsonb
WHERE "Id" = '79136e0b-b8e5-4d38-86cd-2d82078ea586' OR "Code" = 'BNV';

-- Ministry: Bộ Tư pháp (Code: BTP)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '6261e456-7c69-4b12-805b-613b7c2afa1b' OR "Code" = 'BTP';

-- Ministry: Bộ Tài chính (Code: Bộ Tài chính)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 2213/CNTT ngày 12/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Quyết định số 3049/QĐ-BTC ngày 21/10/2026 ban hành Kế hoạch Chuyển đổi số của Bộ Tài chính giai đoạn 2026-2030", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Như Sơn", "Position": "Phó Cục trưởng-CNTT&CĐS", "Department": "", "Phone": "0913 382 138", "Email": "nguyennhuson@mof.gov.vn"}, {"Name": "Hoàng Vương Nam", "Position": "Chuyên viên P KH-TH", "Department": "", "Phone": "0917 232 297", "Email": "hoangvuongnam@mof.gov.vn"}]'::jsonb
WHERE "Id" = '9254ae61-d6e3-4903-a183-44aaaa0fa6a3' OR "Code" = 'Bộ Tài chính';

-- Ministry: Bộ Công Thương (Code: Bộ Công Thương)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 1870/TMĐT ngày 18/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Quyết định số 405/QĐ-BCT ngày 9/3/2026 ban hành kế hoạch Chuyển đổi số của Bộ Công Thương giai đoạn 2026-2030", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Hoàng Ninh", "Position": "Phó Cục trưởng-TMĐT&KTS", "Department": "", "Phone": "0912 524 948", "Email": "NinhH@moit.gov.vn"}, {"Name": "Đinh Anh Tuấn", "Position": "Trưởng phòng- CĐS", "Department": "Cục TMĐT", "Phone": "0934 595 333", "Email": "AnhDTuan@moit.gov.vn"}]'::jsonb
WHERE "Id" = 'f68a5877-09fd-4576-bfc1-798a2a4b7680' OR "Code" = 'Bộ Công Thương';

-- Ministry: Bộ Nông nghiệp và Môi trường (Code: Bộ Nông nghiệp và Môi trường)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 1675/CĐS ngày 24/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Quyết định số 3254/QĐ-BNNMT ngày 14/8/2026 ban hành Kế hoạch Chuyển đổi số của Bộ Nông nghiệp và Môi trường giai đoạn 2026-2030", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Lê Phú Hà", "Position": "Cục trưởng Cục CĐS", "Department": "", "Phone": "", "Email": "0989 080 937- lpha@mae.gov.vn"}, {"Name": "Bùi Mạnh Khôi", "Position": "Thông kê Cục CĐS", "Department": "Trưởng P Tài chính", "Phone": "0902 164 919", "Email": "bmkhoi@mae.gov.vn"}]'::jsonb
WHERE "Id" = '247ffd42-6903-49c9-8f30-81592c94ddaa' OR "Code" = 'Bộ Nông nghiệp và Môi trường';

-- Ministry: Bộ Xây dựng (Code: Bộ Xây dựng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 13014/BXD-TTCNTT ngày 18/8/2026", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "đang rà soát - dự thảo kế hoạch", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Ngọc Quang", "Position": "Phó giám đốc-TTCNTT", "Department": "", "Phone": "0989 089 689", "Email": "nguyenngocquang@moc.gov.vn"}, {"Name": "Trần Thị Thanh Hương", "Position": "Chuyên viên P CĐS TTCNTT", "Department": "", "Phone": "", "Email": "0973 031 435-huongtt@moc.gov.vn"}]'::jsonb
WHERE "Id" = '1c0f39ca-4a18-4ebc-a2cf-f9542256707b' OR "Code" = 'Bộ Xây dựng';

-- Ministry: Bộ Văn hóa, Thể thao và Du lịch (Code: Bộ Văn hóa, Thể thao và Du lịch)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 1120/CĐSVHTTDL ngày 12/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Quyết định số 4220/QĐ-BVHTTDL ngày 5/11/2025 ban hành Chiến lược Chuyển đổi số của Bộ Van hóa, Thể thao và Du lich giai đoạn 2025-2030", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Lê Mạnh Hùng", "Position": "Phó Giám đốc-TT CĐS", "Department": "", "Phone": "0905 188 881", "Email": "lemanhhung@cntt.gov.vn"}, {"Name": "Dương Anh Quân", "Position": "Trưởng phòng-TT CĐS", "Department": "", "Phone": "0915 091 580", "Email": "quanda@cntt.gov.vn"}, {"Name": "Nguyễn Việt Hà", "Position": "Chuyên viên TT CĐS", "Department": "", "Phone": "0963 608 069", "Email": "hanv@cntt.gov.vn"}, {"Name": "Nguyễn Thị Nga", "Position": "Chuyên viên TT CĐS", "Department": "", "Phone": "0973 223 407", "Email": "ngant@cntt.gov.vn"}]'::jsonb
WHERE "Id" = 'a9736803-0568-4ae3-8428-e96613b1293c' OR "Code" = 'Bộ Văn hóa, Thể thao và Du lịch';

-- Ministry: Bộ Khoa học và Công nghệ (Code: BKHCN)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Số 2227/QĐ-BKHCN ngày 24/4/2026", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Cục CĐSQG", "Position": "", "Department": "TTCNTT", "Phone": "", "Email": ""}]'::jsonb
WHERE "Id" = 'fdd14728-fda5-41da-a232-cae702abfcc1' OR "Code" = 'BKHCN';

-- Ministry: Bộ Giáo dục và Đào tạo (Code: BGD)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '852cd5a9-f1d4-45ba-81a8-d7a45fd802da' OR "Code" = 'BGD';

-- Ministry: Bộ Y tế (Code: BYT)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '886a0673-f633-4d45-8467-71cb59372c71' OR "Code" = 'BYT';

-- Ministry: Bộ Dân tộc và Tôn giáo (Code: Bộ Dân tộc và Tôn giáo)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '74beca5d-5812-4626-82ee-b75cb5b15aca' OR "Code" = 'Bộ Dân tộc và Tôn giáo';

-- Ministry: Ngân hàng Nhà nước Việt Nam (Code: Ngân hàng Nhà nước Việt Nam)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 7121/NHNN-CNTT ngày 7/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Quyết định số 1344/QĐ-NHNN ngày 23/6/2026 ban hành Kế hoạch chuyển đổi số của Ngân hàng Nhà nước giai đoạn 2026-2030\nĐang rà soát theo Cv 7121/NHNN", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Hoàng Minh Tiến", "Position": "P Cục trưởng", "Department": "", "Phone": "0912 928 948", "Email": "tien.hoangminh@sbv.gov.vn"}, {"Name": "Trương Thị Hương", "Position": "", "Department": "", "Phone": "Chuyên viên phòng Quản lý chính sách và CĐS- 0916 061 236", "Email": "huong.truong@sbv.gov.vn"}]'::jsonb
WHERE "Id" = 'd6b04db7-082a-450e-ac8e-66607cb70cf3' OR "Code" = 'Ngân hàng Nhà nước Việt Nam';

-- Ministry: Thanh tra Chính phủ (Code: Thanh tra Chính phủ)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'c6ff73ea-a1a0-4d01-9b60-e8350e037c0f' OR "Code" = 'Thanh tra Chính phủ';

-- Ministry: Văn phòng Chính phủ (Code: Văn phòng Chính phủ)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'a26759c0-8542-41b7-bc4f-ed864f6d3dd6' OR "Code" = 'Văn phòng Chính phủ';

-- Province: Tuyên Quang (Code: Tuyên Quang)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 2888/SKHCN-CĐS ngày 11/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Quyết định số 2310/QĐ-UBND ngày 19/9/2026 của UBND tỉnh Tuyên Quang điều chỉnh, bổ sung một số nội dung Quyết định số 1539/QĐ-UBND ngày 11/12/2025 của Ủy ban nhân dân tỉnh phê duyệt Chiến lược Chuyển đổi số tỉnh Tuyên Quang đến năm 2030", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Hồ Thị Phương Lan", "Position": "Trưởng phòng -CĐS", "Department": "", "Phone": "0915 160 266", "Email": ""}, {"Name": "Lê Thành Trung", "Position": "Chuyên viên P CĐS", "Department": "", "Phone": "0979 982 266", "Email": ""}]'::jsonb
WHERE "Id" = '9e862d3f-56e0-45fd-9d90-2f60e00775a3' OR "Code" = 'Tuyên Quang';

-- Province: Lào Cai (Code: Lào Cai)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 2650/SKHCN-CĐS ngày 14/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 399/KH-UBND ngày 21/8/2026 của UBND tỉnh Lào Cai Chuyển đổi số tỉnh Lào Cai giai đoạn 2026 - 2030", "IsUpdated1266": true, "Notes": "Văn bản báo cáo của Lào Cai không bao gồm nội dung rà soát", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Hông Quang", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0913 287 426", "Email": "nguyenhongquang@laocai.gov.vn"}, {"Name": "Phùng Mạnh Sang", "Position": "Chuyên viên P CĐS", "Department": "", "Phone": "0852 662 000", "Email": "phungmanhsang@laocai.gov.vn"}]'::jsonb
WHERE "Id" = 'c871e36b-6a0d-4113-a4aa-96d137b91df3' OR "Code" = 'Lào Cai';

-- Province: Thái Nguyên (Code: Thái Nguyên)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 380/KH-UBND ngày 26/8/2026 của UBND tỉnh Thái Nguyên thực hiện Quyết định số 1266/QĐ-TTg ngày 14 tháng 7 năm 2026 của Thủ tướng Chính phủ phê duyệt Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Thái Nguyên", "IsUpdated1266": true, "Notes": "Không có CV gửi đầu mối", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'fc5f9eb1-1d70-482e-9858-45faf246c4e3' OR "Code" = 'Thái Nguyên';

-- Province: Phú Thọ (Code: Phú Thọ)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'ebd29c42-8ff7-4447-b2f2-07bfdf89fcb1' OR "Code" = 'Phú Thọ';

-- Province: Thành phố Bắc Ninh (Code: Thành phố Bắc Ninh)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3556/SKHCN-CĐS ngày 15/8/2026", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Chỉ có dự thảo kế hoạch", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Văn Khánh", "Position": "Trưởng phòng CĐS", "Department": "", "Phone": "0886 088 666", "Email": "khanhnv107@bacninh.gov.vn"}, {"Name": "Nguyễn Văn Định", "Position": "Chuyên viên P CĐS", "Department": "", "Phone": "0866 866 953", "Email": "dinhnv@bacninh.gov.vn"}]'::jsonb
WHERE "Id" = '9f6aee61-d5b5-4580-80d1-d96cc6ee4acb' OR "Code" = 'Thành phố Bắc Ninh';

-- Province: Hưng Yên (Code: Hưng Yên)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 278/KH-UBND ngày 13/7/2026", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Đỗ Đình Quang", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0982 256 237", "Email": "ddquang@hungyen.gov.vn"}, {"Name": "Bùi Anh Lâm", "Position": "Trưởng phòng -CĐSBCVT", "Department": "", "Phone": "0963 688 686", "Email": "balam@hungyen.gov.vn"}]'::jsonb
WHERE "Id" = '3b7a3868-9ddd-4c46-96ac-0ca5226a2bff' OR "Code" = 'Hưng Yên';

-- Province: Thành phố Hải Phòng (Code: Thành phố Hải Phòng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3529/SKHCN-CNTT ngày 10/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "1167/QĐ-UBND ngày 31/3/2026 (chiến lược)", "IsUpdated1266": false, "Notes": "Chưa có kế hoạch", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Minh Kha", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0904 081 819", "Email": ""}, {"Name": "Dương Thị Tú Anh", "Position": "Phó Trưởng phòng CNTT", "Department": "", "Phone": "0979 831 231", "Email": ""}, {"Name": "Nguyễn Việt Anh", "Position": "Chuyên viên CNTT", "Department": "", "Phone": "0815 433 168", "Email": ""}]'::jsonb
WHERE "Id" = '55616a24-b122-48b8-aca8-73213c5a12b1' OR "Code" = 'Thành phố Hải Phòng';

-- Province: Ninh Bình (Code: Ninh Bình)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '4c16b6d4-96b9-4c45-94df-6d110a7b6083' OR "Code" = 'Ninh Bình';

-- Province: Quảng Trị (Code: Quảng Trị)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3362/SKHCN-CĐS ngày 10/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "số 1785/KH-UBND ngày 7/4/2026", "IsUpdated1266": true, "Notes": "Thực hiện Quyết định số 2629/QĐ-TTg ngày 01/12/2025 của Thủ tướng Chính phủ, UBND tỉnh Quảng trị đã ban hành Kế hoạch số 1785/KH-UBND ngày 07/4/2026 về chuyển đổi số tỉnh Quảng Trị giai đoạn 2026 - 2030 và Kế hoạch 3472/KH-UBND ngày 15/7/2026 về phát triển kinh tế số và xã hội số tỉnh Quảng Trị giai đoạn 2026 - 2030. Nhìn chung, các kế hoạch triển khai đã bám sát quan điểm, mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp của Trung ương, phù hợp với Nghị quyết số 57-NQ/TW, Luật Chuyển đổi số, các chương trình, kế hoạch có liên quan và phù hợp với điều kiện thực tiễn, tình hình phát triển kinh tế, xã hội, nguồn lực của tỉnh Quảng Trị; nội dung của 02 Kế hoạch do UBND tỉnh ban hành đã xác định rõ nhiệm vụ trọng tâm, trọng điểm giai đoạn 2026 -2030, nhiệm vụ ưu tiên hằng năm, sản phẩm đầu ra, tiến độ thực hiện, đơn vị chủ trì, đơn vị phối hợp, nguồn lực thực hiện và các chỉ tiêu theo dõi, đánh giá kết quả cụ thể. Các nhiệm vụ được triển khai đồng bộ, có tính liên thông, phát triển chuyển đổi số toàn diện trên cả 3 trụ cột là chính quyền số, kinh tế số, xã hội số, có tính đột phá, tạo động lực phát triển nhanh, bền vững trên tất cả các lĩnh vực của tỉnh Quảng Trị; đảm bảo phù hợp với các nội dung của Quyết định số 1266/QĐ-TTg ngày 14/7/2026 của Thủ tướng Chính phủ. (Theo CV số 3362 ngày 10/8/2026)", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Xuân Ngọc", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0935 596 599", "Email": ""}, {"Name": "Nguyễn Thành Lê", "Position": "Trưởng phòng", "Department": "", "Phone": "0912 049 773", "Email": ""}, {"Name": "Nguyễn Tiến Thành", "Position": "Chuyên viên CNTT", "Department": "", "Phone": "0982 576 767", "Email": ""}]'::jsonb
WHERE "Id" = 'd5f2554b-78fe-45e5-abc6-b250b9204167' OR "Code" = 'Quảng Trị';

-- Province: Thành phố Đà Nẵng (Code: Thành phố Đà Nẵng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'eb71a536-09c9-4154-8f79-ebab7a263aef' OR "Code" = 'Thành phố Đà Nẵng';

-- Province: Quảng Ngãi (Code: Quảng Ngãi)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4062/SHKCN-BCVT ngày 11/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 298/KH-UBND ngày 7/8/2026 Triển khai thực hiện Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Quảng Ngãi", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Quốc Huy Hoàng", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0982 142 211", "Email": "nqhhoang-skh@quangngai.gov.vn"}, {"Name": "Hà Thanh Tuấn", "Position": "Trưởng phòng", "Department": "", "Phone": "0905 257 799", "Email": "httuan-skh@quangngai.gov.vn"}, {"Name": "Đặng Bảo Hy", "Position": "Chuyên viên P BCVT&CĐS", "Department": "", "Phone": "0905 181 088", "Email": ""}]'::jsonb
WHERE "Id" = '3e718d84-8de0-41d3-b85d-1fdf4ddd77cf' OR "Code" = 'Quảng Ngãi';

-- Province: Gia Lai (Code: Gia Lai)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Quyết định số 4155/QĐ-UBND ngày 1/10/2026 của UBND tỉnh Gia Lai ban hành Kế hoạch chuyển đổi số tỉnh Gia Lai  \ngiai đoạn 2026 – 2030", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '6ee684fa-6005-41fa-92db-41e71b55ac73' OR "Code" = 'Gia Lai';

-- Province: Khánh Hòa (Code: Khánh Hoà)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '11505dcb-d397-4fc8-b747-9dccea6701c2' OR "Code" = 'Khánh Hoà';

-- Province: Lâm Đồng (Code: Lâm Đồng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4099/SKHCN-CĐS ngày 20/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 6388/KH-UBND ngày 8/5/2026", "IsUpdated1266": false, "Notes": "Văn bản báo cáo của Lâm Đồng không bao gồm nội dung rà soát", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Lê Thanh Liêm", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0941 777 799", "Email": "liemlt@lamdong.gov.vn"}, {"Name": "Võ Duy Phong", "Position": "Trưởng phòng CĐS", "Department": "", "Phone": "0911 364 567", "Email": "phongvd.skhcn@lamdong.gov.vn"}]'::jsonb
WHERE "Id" = '0313d44f-b102-439e-973c-69f45ac3561a' OR "Code" = 'Lâm Đồng';

-- Province: Đắk Lắk (Code: Đắk Lắk)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3960/SKHCN-CĐS ngày 4/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Có danh mục bổ sung nhiệm vụ của kế hoach số 95/KH-UBND ngày 25/3/2026", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Ra Lan Trương Thanh Hà", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0914 586 911", "Email": ""}, {"Name": "Lê An Pha", "Position": "Trưởng phòng CĐS", "Department": "", "Phone": "0836 907 788", "Email": ""}]'::jsonb
WHERE "Id" = '4ec1aa04-48c8-4845-8352-62c11b5e398e' OR "Code" = 'Đắk Lắk';

-- Province: Thành phố Hồ Chí Minh (Code: Thành phố Hồ Chí Minh)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 8071/SKHCN-CĐS ngày 15/8/2026", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "TP HCM đang rà soát", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Võ Minh Thành", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0918 804 410", "Email": "vmthanh.skhcn@tphcm.gov.vn"}, {"Name": "Nguyễn Trọng Ngân", "Position": "Trưởng phòng -CĐS", "Department": "", "Phone": "", "Email": "0982 971 791- ngtngan.skhcn@tphcm.gov.vn"}, {"Name": "Lê Hoài Nam", "Position": "Chuyên viên P CĐS", "Department": "", "Phone": "0908 270 305", "Email": "lhnam.skhcn@tphcm.gov.vn"}]'::jsonb
WHERE "Id" = 'a12f85d7-e93e-4d7e-bb3a-91bf6b02d938' OR "Code" = 'Thành phố Hồ Chí Minh';

-- Province: Thành phố Đồng Nai (Code: Thành phố Đồng Nai)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = 'b4861247-737e-476d-a6d6-dd192ad31ed1' OR "Code" = 'Thành phố Đồng Nai';

-- Province: Tây Ninh (Code: Tây Ninh)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 15054/UBND-VHXH ngày 18/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 482/KH_UBND ngày 2/2/2026", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Bùi Nguyên Khởi", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0889 691 777", "Email": "nguyenkhoi@tayninh.gov.vn"}, {"Name": "Tăng Thị Ngọc Em", "Position": "Trưởng phòng -CĐS", "Department": "", "Phone": "0946 700 369", "Email": "ngocem@tayninh.gov.vn"}]'::jsonb
WHERE "Id" = '5d4a284c-be41-45d3-943d-f506e797aaaf' OR "Code" = 'Tây Ninh';

-- Province: Thành phố Cần Thơ (Code: Thành phố Cần Thơ)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4349/SKHCN-CĐS ngày 19/8/2026", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Ngô Anh Tín", "Position": "Giám đốc SKHCN", "Department": "", "Phone": "0917 939 939", "Email": "anhtin@cantho.gov.vn"}, {"Name": "Lê Hồng Anh", "Position": "Quyền Trưởng phòng CĐS", "Department": "", "Phone": "0932 898 964", "Email": "lehonganh@cantho.gov.vn"}]'::jsonb
WHERE "Id" = 'a5c4fe47-5370-4f36-a92c-6a6518a6bf2b' OR "Code" = 'Thành phố Cần Thơ';

-- Province: Vĩnh Long (Code: Vĩnh Long)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '3335839b-d0c9-44bb-a2e2-74891e85f8b2' OR "Code" = 'Vĩnh Long';

-- Province: Đồng Tháp (Code: Đồng Tháp)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4304/SKH&CN-CĐS ngày 17/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 1109/KH-UBND ngày 15/7/2026", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Trần Văn Dũng", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0919 785 785", "Email": "tvdung@dong thap.gov.vn"}, {"Name": "Lê Minh Hiếu", "Position": "Chuyên viên P CĐS", "Department": "", "Phone": "0977 926 457", "Email": "leminhhieu@dongthap.gov.vn"}]'::jsonb
WHERE "Id" = '6cc5fe6c-cd52-48b6-997e-8dc341485d44' OR "Code" = 'Đồng Tháp';

-- Province: Cà Mau (Code: Cà Mau)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4325/SKHCN-CNSBC ngày 17/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 556/KH-UBND ngày 17/9/2026 ngày 17/9/2026 Chuyển đổi số trên địa bàn tỉnh Cà Mau giai đoạn 2026 - 2030, định hướng đến năm 2045", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Phạm Thống Nhất", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0913 892 892", "Email": "skhcn.ptnhat@gmail.com"}, {"Name": "Trần Quốc Toản", "Position": "Phó Trưởng phòng CNS&BC", "Department": "", "Phone": "0944 317 508", "Email": "skhcn.tqtoan@câmu.gov.vn"}]'::jsonb
WHERE "Id" = '0213129c-4559-49b6-8384-d5ff2f3c57d0' OR "Code" = 'Cà Mau';

-- Province: An Giang (Code: An Giang)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '5e7ebb11-f6f9-44fb-8b27-6c6e1d776e95' OR "Code" = 'An Giang';

-- Province: Thành phố Hà Nội (Code: Thành phố Hà Nội)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '065ba3fb-52b3-4c19-9fa5-edfeb5d55db3' OR "Code" = 'Thành phố Hà Nội';

-- Province: Thành phố Huế (Code: Thành phố Huế)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4105/SKHCN-CĐS ngày 14/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 469/KH-UBND ngày 14/8/2026 \nTriển khai Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 trên địa bàn thành phố Huế", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Xuân Sơn", "Position": "Giám đốc SKHCN", "Department": "", "Phone": "0914 202 345", "Email": "nxson.skhcn@hue.gov.vn"}, {"Name": "Phan Nữ Anh Thư", "Position": "Phó Trưởng phòng CĐS SKHCN", "Department": "", "Phone": "0905 979 772", "Email": "pnathu.skhcn@hue.gov.vn"}]'::jsonb
WHERE "Id" = '5c084ca5-c165-4b35-8474-3f4b91a02d17' OR "Code" = 'Thành phố Huế';

-- Province: Lai Châu (Code: Lai Châu)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3466/SKHCN-BCVTCNNTT ngày 13/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 576/KH-UBND ngày 23/1/2026", "IsUpdated1266": false, "Notes": "Văn bản báo cáo của Lai Châu không bao gồm nội dung rà soát", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Phạm Quang Cường", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0976 819 323", "Email": "cuongpq.sokhcn@laichau.gov.vn"}, {"Name": "Bùi Thị Lan", "Position": "Trưởng phòng, P Bưu chính, Viễn thông, Công nghệ thông tin", "Department": "", "Phone": "", "Email": "lanbt.sokhcn@laichau.gov.vn"}, {"Name": "Nguyễn Thị Mai Liên", "Position": "Chuyên viên P Bưu chính, Viễn thông, Công nghệ thông tin", "Department": "", "Phone": "0978 637 648", "Email": "lienntm.sokhcn@laichau.gov.vn"}]'::jsonb
WHERE "Id" = '928cf8e4-24a6-422b-90d8-04f2ec9f84d9' OR "Code" = 'Lai Châu';

-- Province: Điện Biên (Code: Điện Biên)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 5011/SKHCN-KĐC ngày 9/11/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Số 1898/KH-UBND ngày 17/3/2026", "IsUpdated1266": true, "Notes": "Ngày 17/3/2026, UBND tỉnh Điện Biên đã ban hành Kế hoạch số 1898/KH-UBND Chuyển đổi số tỉnh Điện Biên giai đoạn 2026 - 2030. Ngoài ra, tỉnh Điện Biên đã ban hành Nghị quyết số 17-NQ/TU ngày 27/02/2026 của Ban Chấp hành Đảng bộ tỉnh về đột phá phát triển khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2035; Kế hoạch số 2092/KH-UBND ngày 23/3/2026 của UBND tỉnh thực hiện Nghị quyết số 17-NQ/TU ngày 27/02/2026 của Ban Chấp hành Đảng bộ tỉnh về đột phá phát triển khoa học, công nghệ, đổi mới sáng tạo và chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2035 (Có Nghị quyết, Kế hoạch kèm theo).\n Các nội dung tại Nghị quyết, Kế hoạch bảo đảm bám sát quan điểm, mục tiêu, chỉ tiêu, nhiệm vụ, giải pháp của Quyết định số 1266/QĐ-TTg ngày 14/7/2026 của Thủ tướng Chính phủ phê duyệt Chiến lược quốc gia về chuyển đổi số giai đoạn 2026 - 2030, định hướng đến năm 2045 và phù hợp với Nghị quyết số 57-NQ/TW, Luật Chuyển đổi số và các chương trình, kế hoạch có liên quan; trong đó, xác định rõ nhiệm vụ trọng tâm giai đoạn 2026 - 2030, nhiệm vụ ưu tiên hằng năm, sản phẩm đầu ra, tiến độ thực hiện, đơn vị chủ trì, đơn vị phối hợp, nguồn lực thực hiện và chỉ tiêu theo dõi, đánh giá kết quả. (theo CV số 5011/SKHCN-KĐC ngày 9/11/2026)", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Phạm Thành Nam", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0912232669", "Email": "Nampt.skhcn@dien bien.gov.vn"}, {"Name": "Trần Thị Nga", "Position": "Chuyên viên P KHCN, ĐMST&CĐS", "Department": "", "Phone": "0947483618", "Email": "Ngatt.skhcn@dien bien.gov.vn"}]'::jsonb
WHERE "Id" = 'cc87b372-c45f-42c5-bdaf-c5c9c63564b4' OR "Code" = 'Điện Biên';

-- Province: Sơn La (Code: Sơn La)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 394/BC-SKHCN ngày 14/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Quyết định số 2364/QĐ-UBND ngày 16/9/2026 QĐ cập nhật bổ sung 1 số chỉ tiêu, nhiệm vụ...2026-2030", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Phạm Quốc Chính", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0983 002 065", "Email": "chinhpq.skhcn@sonla.gov.vn"}, {"Name": "Ngô Thị Hồng Hạnh", "Position": "", "Department": "Trưởng P CĐS", "Phone": "0912 109 204", "Email": "hanhntn.skhcn@sonla.gov.vn"}]'::jsonb
WHERE "Id" = '71890330-684e-4394-9543-94fdcbe231da' OR "Code" = 'Sơn La';

-- Province: Lạng Sơn (Code: Lạng Sơn)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 4629/BC-SKHCN ngày 14/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 338/KH-UBND ngày 1/9/2026  \nTriển khai thực hiện Chiến lược quốc gia về chuyển đổi số giai đoạn  2026 - 2030, định hướng đến năm 2045 trên địa bàn tỉnh Lạng Sơn", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Trần Hữu Giang", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0913 199 838", "Email": "thgiang@langson.gov.vn"}, {"Name": "Vũ Thùy Dung", "Position": "Phó Trưởng phòng CĐS SKHCN", "Department": "", "Phone": "0388 653 616", "Email": "vtdung@langson.gov.vn"}]'::jsonb
WHERE "Id" = 'e920d4d4-b95e-48c6-9bd1-0547d9c2a30c' OR "Code" = 'Lạng Sơn';

-- Province: Thành phố Quảng Ninh (Code: Thành phố Quảng Ninh)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3987/SKHCN-QL ngày 14/8/2026", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "Tỉnh đang tổng hợp", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Trung Tiến", "Position": "Phó giám đốc", "Department": "Sở KHCN", "Phone": "0913 388 266", "Email": ""}, {"Name": "Vũ Thị Kim Minh Huệ", "Position": "Phó Trưởng phòng Quản lý CNTT&CĐS", "Department": "", "Phone": "0912 773 828", "Email": ""}, {"Name": "Phạm Thị Trang", "Position": "Chuyên viên P QLCN&CĐS", "Department": "", "Phone": "0834 130 448", "Email": ""}]'::jsonb
WHERE "Id" = '8c25226f-7146-489f-9416-9353ae766507' OR "Code" = 'Thành phố Quảng Ninh';

-- Province: Thanh Hóa (Code: Thanh Hoá)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "Số 3730/SKHCN-CNTT ngày 14/8/2026", "HasReviewedPlan": true, "IsIssued": true, "DocumentNumber": "Kế hoạch số 282/KH-UBND ngày 4/8/2026 của UBND tỉnh Thanh Hóa triển khai thực hiện Chiến lược quốc gia về chuyển đổi số trên địa bàn tỉnh Thanh Hóa giai đoạn 2026 - 2030, định hướng đến năm 2045", "IsUpdated1266": true, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[{"Name": "Nguyễn Thị Thu Hà", "Position": "Phó Trưởng phòng Quản lý CNTT&CĐS", "Department": "", "Phone": "0982 401 828", "Email": "hantt.skhcn@thanhhoa.gov.vn"}]'::jsonb
WHERE "Id" = '6ae77e42-9e4b-4ee9-8676-07cf557c6f49' OR "Code" = 'Thanh Hoá';

-- Province: Nghệ An (Code: Nghệ An)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '09cfea01-ba49-4b6b-afa2-aac3b35a21b8' OR "Code" = 'Nghệ An';

-- Province: Hà Tĩnh (Code: Hà Tĩnh)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '665e4ffd-c45f-4676-926f-34d5f8b72f46' OR "Code" = 'Hà Tĩnh';

-- Province: Cao Bằng (Code: Cao Bằng)
UPDATE "Agencies"
SET "FiveYearPlan" = '{"ResponseDocument": "", "HasReviewedPlan": false, "IsIssued": false, "DocumentNumber": "", "IsUpdated1266": false, "Notes": "", "Period": "2026-2030", "Files": []}'::jsonb,
    "ContactPersons" = '[]'::jsonb
WHERE "Id" = '62de8530-3cb2-4a27-bf81-8f4f022200bb' OR "Code" = 'Cao Bằng';

COMMIT;
