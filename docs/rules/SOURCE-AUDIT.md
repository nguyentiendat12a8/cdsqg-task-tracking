# Rà soát source để áp dụng rule

Bảng dưới lưu phát hiện trước refactor; trạng thái áp dụng hiện tại ở cuối file. Ngày 09/10/2026. Đây là rà soát cấu trúc/static source và các nguồn logic liên quan, không phải xác nhận đã kiểm thử mọi workflow. Không chỉnh dữ liệu DB trong đợt viết rule này.

| Ưu tiên | Phát hiện / nguồn thực tế | Hướng xử lý |
| --- | --- | --- |
| P1 | `Program.cs` SeedInitialData chuyển mọi item khác targetDocId về văn bản 1266; NormalizeGoalTaskItemCodes có thể đổi mã khi startup | Tách seed khỏi sửa dữ liệu nghiệp vụ; migration/script riêng được review, không tự di chuyển dữ liệu khi boot |
| P1 | `useDocumentData.js` hardcode GUID 1266; năm 2026–2030 rải ở modal/filter | Metadata API/config context chung, kiểm tra khi chuyển văn bản/năm |
| P1 | `useDocumentFilters.js` lọc lại trang server; tiêu chí năm/scope/search khác `PlanningService.GetDocumentItemsAsync` | Server là nguồn filter trước pagination; FE chỉ draft/applied và trình bày; test totals/export |
| P1 | Phạm vi cơ quan nằm ở PlanningService, ApiPermissionFilter.CanReportAsync, DashboardController và heuristic FE | AgencyScopePolicy có mode quyền riêng; cùng fixture cơ quan con/chung/admin; không mở rộng quyền khi gom |
| P1 | `services/auth.js` isAdmin nhận Admin/1, DocumentDetailView còn nhận chuỗi '1'; Axios trong `stores/useTrackingStore.js` có interceptors riêng | Chuẩn hóa enum một lần, mọi caller dùng cùng auth client/error behavior |
| P2 | `features/document/presentation.js` và `features/dashboard/presentation.js` lặp formatDate/getStatusLabel/isGeneralTaskItem | shared formatters/statusPresentation; mapping đầy đủ + ngắn; không bỏ label approval/qualitative khi hợp nhất |
| P2 | Dashboard label viết tắt và màu ExpiringSoon purple khác các màn hình | Một map trạng thái cho bảng/card/chart/export, dùng palette UI-DESIGN |
| P2 | formatDateTime document tự gắn Z chuỗi không offset; nhiều view dùng new Date/toLocaleDateString | Phân biệt date-only/timestamp; timezone Asia/Ho_Chi_Minh; fixture qua nửa đêm và máy khác timezone |
| P2 | `style.css` ép font !important toàn bộ, focus global và tự thêm padding mọi overflow container | Token/style scoped theo control; giữ focus visible và không phá layout; kiểm tra trước khi gỡ override |
| P2 | Các view/modal có text-[10px]/[11px], bảng/form thiếu thang chữ nhất quán | Áp dụng thang chữ theo vai trò khi sửa từng component, không thay thế mù toàn source |
| P2 | Modal nghiệp vụ, SearchableSelect, DatePicker có implementation riêng | Checklist keyboard/screen reader/teleport focus; directive không thay thế test control |
| P2 | `GoalTaskItemController`, ExecutionController còn điều phối cache/workflow lớn; ProgressCalculator đã dùng chung | Giữ calculator làm nguồn; dần tách transaction/workflow service, không xóa raw/evidence để đơn giản hóa |
| P3 | `excelReports.js` import writer static; nhiều export tự dựng style | Kiểm tra import graph, lazy-load từ action, dùng worksheetStyle/excelRuntime; regression XLSX/XLS |
| P3 | package.json khai báo nhiều UI/Excel library song song | Kiểm kê dependency thực tế rồi đề xuất gỡ; không thay kit/nâng package trong đợt viết rule |

## Những phần đã có nền tảng dùng chung

ProgressCalculator xử lý baseline/tỷ lệ/hoàn thành/thời hạn/agency tổng hợp; submit và import dùng ExecutionService; ApiPermissionFilter là entry quyền; auth/config API tập trung; dashboard/document đã tách feature; Excel lazy engine; directive accessibility và confirm chung; PostgreSQL migration có phiên bản. Đây là các nguồn phải mở rộng thay vì tạo bản sao mới.

## Lộ trình triển khai

1. Khóa nghiệp vụ bằng fixture (total năm, baseline, quyền, scope/filter) và bỏ sửa dữ liệu nghiệp vụ khi startup.
2. Gom format/status/auth enum; chuyển dashboard/document rồi các modal/export, xóa hàm trùng đã thay thế.
3. Gom query/scope BE; bỏ filter lặp trên trang FE, chuẩn hóa metadata văn bản/năm.
4. Tạo design token và primitive Button/Input/Modal nếu kiểm kê cho thấy cùng hợp đồng; áp dụng từng màn hình, kiểm tra keyboard/mobile và không đổi workflow.
5. Dọn dependency/import graph sau khi chứng minh không còn caller; bổ sung lint/check tự động cho rule có thể máy kiểm tra.

Điều kiện hoàn thành mỗi bước: nguồn chính xác định rõ, caller đã chuyển, bản sao tương ứng đã xóa, kiểm thử liên quan đạt và audit cập nhật. Bộ rule là hướng dẫn review/agent; check:rules bổ sung kiểm tra tự động phạm vi formatter/status và Excel imports. Không tính việc tạo file rule là đã refactor toàn bộ BE/FE.

## Đã áp dụng trong source ngày 09/10/2026

- Bỏ sửa DocumentId/mã nhiệm vụ khi startup; GET chi tiết văn bản không còn đổi mã/lưu DB.
- DocumentItemQuery là nguồn filter/sort BE. FE nhận nguyên trang server; export lấy đủ các trang của cùng query, kiểm tra response phân trang.
- AgencyScopePolicy tách view scope và quyền report; PlanningService/ApiPermissionFilter dùng chung. Scope tổng hợp dashboard và import còn hợp đồng khác cần xét riêng, chưa ép thành predicate chung.
- shared/formatters dùng chung ngày lịch, timestamp UTC → Việt Nam, số, phần trăm, tên file. Dashboard/document, chi tiết, báo cáo, lịch sử, thông báo và user management đã chuyển caller.
- shared/statusPresentation là nguồn nhãn/màu execution/approval cho dashboard/document/chi tiết/báo cáo/export; trạng thái văn bản pháp lý giữ mapping riêng vì là nghiệp vụ khác.
- shared/roles chuẩn hóa Admin/1/'1'; bỏ nhận role 0 là Admin trên màn hình user.
- Tracking store chuyển Axios sang services/apiClient + fetchWithAuth; không xóa package Axios khi chưa audit dependency hoàn chỉnh.
- config/reporting tập trung GUID và năm mặc định; environment cho phép đổi documentId. Chưa lấy toàn bộ context năm từ metadata API.
- style.css dùng font/token, bỏ ép font !important và padding global cho scroll container. Select/date/confirm/baseline và form thêm/sửa/báo cáo dùng thang chữ/control chung.
- Fixture accessibility dùng cùng Vue runtime thay vì đường dẫn cache Vite hardcode. Test export thêm case 101 dòng qua hai trang.

## Còn cần kiểm kê theo workflow riêng

Không áp dụng cỡ chữ hàng loạt cho mọi text legacy: các card/dashboard và modal ít dùng còn cỡ 10–11px cần chuyển theo vai trò, có visual regression. Full metadata API, scope tổng hợp dashboard/import, tách các workflow transaction lớn và dọn package cần các thay đổi tiếp theo có test riêng. Chưa coi toàn bộ UI đạt screen reader chỉ từ test bàn phím hiện có.
