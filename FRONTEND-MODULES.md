# Frontend: tách module và tải Excel theo nhu cầu

Đã thực hiện ngày 08/10/2026. Giữ các chức năng và định dạng báo cáo hiện có.

## Cấu trúc

`frontend/src/features/dashboard/` gồm bộ lọc, truy vấn API, metrics, drilldown,
thống kê văn bản, các component trình bày và báo cáo. Báo cáo tách riêng phần gọi
API, sheet tổng hợp, sheet cơ quan và điều phối workbook. Năm khối thẻ cơ quan
dùng chung `AgencyProgressCard.vue`.

`frontend/src/features/document/` gồm dữ liệu/phân trang server, bộ lọc, trình
bày bảng và phân trang, modal thêm/sửa, workflow thêm/sửa/duyệt và export.
Các modal lớn được tải khi mở. View điều phối props/model của các component;
các component trình bày không tự gọi API.

Dashboard view giảm từ 3.060 xuống 568 dòng, document view từ 2.830 xuống 871
dòng. Logic được chuyển sang các module, không xóa các workflow nghiệp vụ.

## Excel

`excelRuntime.js` tải engine khi chọn import/export. Xuất và đọc `.xlsx` dùng
entry `xlsx.bundle.js` không kèm bảng mã file cũ. Đọc `.xls` hoặc định dạng chưa
xác định dùng entry đầy đủ. Tiếng Việt, màu, đường viền, ô gộp và nhiều sheet
được giữ lại.

`frontend/build/excelWriter.js` loại duy nhất dependency `cpexcel.js` khỏi entry
writer, cho cả build production và tối ưu dependency dev; không sửa node_modules.
Nếu entry thư viện thay đổi, build dừng để yêu cầu kiểm tra lại adapter. Không
được dùng writer này để đọc file XLS có bảng mã cũ.

Kích thước JS do `vite build` báo, kB chưa gzip:

| Chunk | Trước | Sau |
| --- | ---: | ---: |
| ExecutiveDashboard | 273,05 | 242,07 |
| DocumentDetailView | 194,29 | 124,19 |
| Excel dùng cho export / XLSX | 870,42 | 425,09 |

Writer gzip 147,73 kB, so với engine cũ 323,05 kB. Reader đầy đủ vẫn 869,94 kB
và còn cảnh báo chunk >500 kB; chỉ được tải khi import XLS cũ. Không tăng ngưỡng
cảnh báo để che phần này. Các số đo chunk không phải tổng dung lượng ứng dụng.

## Lỗi được sửa cùng đợt tách module

- Hai lời gọi `fetchDocumentData()` không tồn tại chuyển sang `loadData()`.
- Đổi số dòng/trang tải lại dữ liệu và về trang 1.
- Hủy timer debounce khi rời view; request cũ không ghi đè kết quả mới.
- Cache danh sách cơ quan trong vòng đời document view thay vì tải mỗi lần lọc.
- Hiển thị lỗi tải dữ liệu và nút thử lại; khóa nút export từ lúc tải module.
- Báo cáo dashboard dừng nếu không tải đủ dữ liệu cơ quan, thay vì xuất sheet
  rỗng và báo thành công. Hai request cho một sheet cơ quan chạy đồng thời.
- Modal thêm/sửa và drilldown dùng focus trap/Escape/khôi phục focus chung.

## Kiểm tra

Build production thành công. Kiểm thử Edge headless qua:

- Dashboard, chi tiết, drilldown, submit thêm/sửa và payload API.
- Đổi số dòng/trang, tìm kiếm với phản hồi về sai thứ tự, lỗi API và thử lại.
- Tải XLSX, đọc lại nội dung tiếng Việt, định dạng màu và ô gộp.
- Đọc file XLS, không tải engine Excel khi mở dashboard, không tải reader cũ
  khi xuất/đọc XLSX; không có lỗi runtime hoặc Vue warning.
- Menu mobile, sidebar desktop và hộp xác nhận bằng bàn phím.
- Xuất XLSX từ bản build production thực tế bằng engine nhỏ.

Chạy test (cần Playwright đã cài và Edge):

```powershell
# PLAYWRIGHT_PACKAGE trỏ tới package.json của Playwright đã cài trên máy.
$env:PLAYWRIGHT_PACKAGE = '<absolute-path-to-playwright/package.json>'
$env:CDSQG_VITE_CACHE_DIR = 'node_modules/.vite-refactor'
npm run dev -- --host 127.0.0.1 --port 5201 --strictPort
# Terminal khác:
npm run test:refactor
# Sau npm run build, chạy preview ở terminal riêng:
npm run preview -- --host 127.0.0.1 --port 5202 --strictPort
npm run test:excel-production
```

Test dùng API fixture, không gửi thao tác thêm/sửa vào database thật. Có thể
đổi base URL bằng `FRONTEND_TEST_URL`.

Tham khảo cơ chế import theo nhu cầu và encoding của
[SheetJS](https://docs.sheetjs.com/docs/getting-started/installation/frameworks/).
