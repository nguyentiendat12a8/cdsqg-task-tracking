# Áp dụng chuẩn vào source — 09/10/2026

## Thay đổi đã thực hiện

BE: startup/GET chi tiết không chuyển văn bản, renumber mã hoặc lưu cache. DocumentItemQuery xử lý filter/sort dùng chung; AgencyScopePolicy tách view/report với hợp đồng cũ. ProgressCalculator giữ nguyên quy tắc tổng năm đã thống nhất.

FE: shared formatters/status/roles/agencyPresentation; caller dashboard/document/chi tiết/báo cáo/lịch sử/thông báo/user đã chuyển. Store dùng apiClient/fetchWithAuth chung. Danh sách dùng nguyên trang/sort server; export lấy tất cả các trang của cùng query. REPORTING_DOCUMENT_ID/REPORTING_YEARS tập trung ở config.

UI: token font/cỡ chữ/chiều cao control; bỏ ép font !important và padding global cho vùng scroll. Select/date picker, confirm/baseline, form thêm/sửa và báo cáo áp dụng control/cỡ chữ theo vai trò, input mobile 16px. Không sửa raw report/minh chứng/quyết định duyệt.

## Kiểm chứng đã chạy

- 67 test BE đạt, không skip, gồm PostgreSQL integration và test scope/query mới.
- Build BE đạt 0 warning/0 error; build FE đạt, còn warning reader Excel legacy khoảng 870KB đã có trước.
- test:shared: null/0, ngày lịch, timestamp qua nửa đêm Việt Nam, enum role/status.
- check:rules: ngăn formatter/status canonical bị định nghĩa lại; ngăn view import Excel eager.
- test:annual: baseline năm, multipart/report query, viewport mobile.
- test:refactor: thêm/sửa/lọc/phân trang/retry và export 101 dòng qua hai trang; dashboard, Unicode/merge/style XLSX và reader XLS legacy.
- data-accessibility: 200 tên cơ quan dài, search empty, bàn phím select/date, bảng, Esc/focus, pending disabled và lỗi 403/429/500.
- security.browser: HTML độc hại, rich text, header xác thực/không lộ token sang host ngoài, 401 logout.
- test:excel-production: export Unicode/style với writer nhỏ ở production.

## Giới hạn còn ghi rõ

Rule mới đã có nguồn thực thi chính, nhưng không khẳng định mọi component legacy đạt tất cả quy chuẩn visual/screen reader. Full metadata năm từ API, typography chi tiết các card cũ, scope tổng hợp dashboard/import và workflow transaction lớn tiếp tục theo SOURCE-AUDIT. Không nâng/gỡ dependency trong đợt này.

## Dashboard: tab khối cơ quan — 13/10/2026

Địa phương / Bộ-Ngành / Khối khác nằm trong một vùng tab 36px, số lượng theo bộ lọc. Địa phương mở mặc định; bản đồ SVG và danh sách giữ nguyên. Panel chỉ mount lần đầu được mở rồi giữ bằng v-show để bảo toàn tìm kiếm/lựa chọn biểu đồ. Hỗ trợ ArrowLeft/Right/Home/End và trạng thái rỗng Khối khác. Không thay đổi API, phép tính hoặc dữ liệu BE.

Regression browser kiểm tra chỉ một panel hiện, giữ search bản đồ khi chuyển tab, keyboard, mobile không cuộn ngang toàn trang, drilldown và export. Build FE đạt; không công bố tỷ lệ giảm chiều dài trang khi chưa đo với dữ liệu thực.

### 13/10/2026 — Mật độ chung và Excel báo cáo năm

- Giữ bố cục cũ; density chung tại style.css: tiêu đề token 18px, panel 16–20px, gap nhóm 12–20px, control 36px. Body/form không giảm dưới chuẩn 14px. AppHeader và lề main gọn hơn.
- Danh sách mục tiêu/nhiệm vụ bật lại xuất mẫu báo cáo và import; lấy đủ trang theo bộ lọc đang áp dụng trước khi đối chiếu mã. Modal chọn năm 2026–2030, không cộng giá trị năm trước.
- Sửa nhận cột tiến độ mới, hướng dẫn cột J/K; chặn số âm, chuỗi số lỗi, trạng thái lạ, mã trùng. Năm bắt buộc, chống submit lặp. Nhãn chờ phê duyệt khớp BE (chỉ Admin duyệt ngay).
- Browser: test:progress-excel (API mock), test:density (9 màn hình, dữ liệu rỗng desktop/mobile), test:refactor, test:annual, shared và data-accessibility (200 nhãn dài).
- PostgreSQL: ProgressImportPostgresTests dùng schema phát sinh riêng, migration và submit thật; xác minh định lượng/định tính, mã không tồn tại, số âm, pending không thành dữ liệu chính thức và chặn pending lặp. Không ghi dữ liệu thử vào DB ứng dụng.

### 13/10/2026 — Chốt chữ mặc định 12px

Theo yêu cầu người dùng, nội dung, bảng, nút, label/dropdown cùng dùng 12px; thay các class 9–11px/13px bằng text-xs. Typography chính nằm ở token style.css (text-sm và text-base legacy cũng 12px). Các nút cùng weight 600; heading trang/modal 18px, nhóm 14px, thương hiệu 16px, KPI 28px. Input mobile 16px là ngoại lệ tránh tự zoom, font của tài liệu đính kèm giữ nguyên.

Kiểm chứng: test:density đo font-size 12px thực tế của control/label trên 9 view desktop/mobile; test:refactor đo chữ bảng và nút với dữ liệu thật dạng fixture; test:progress-excel, build và check:rules đạt. Rule check chặn thêm class text cỡ 9–11/13px.
