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
