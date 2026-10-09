# Rule kiến trúc và logic chung

## Một nguồn nghiệp vụ, không một file khổng lồ

BE quyết định giá trị/tỷ lệ/trạng thái, baseline, phạm vi cơ quan và quyền. FE nhận DTO chính thức; không suy ra hoàn thành từ phần trăm làm tròn, không tự cộng báo cáo. Hai runtime không thể import cùng hàm C#; chia sẻ hợp đồng API, enum và fixture nghiệp vụ. Nếu cần preview FE, ưu tiên API preview; bản tính tạm phải ghi rõ chưa duyệt và có fixture so sánh với BE.

| Quy tắc | Nguồn hiện có phải dùng | Ghi chú |
| --- | --- | --- |
| Baseline, tiến độ, thời hạn, cảnh báo | `backend/Application/Services/ProgressCalculator.cs` | Evaluate, EvaluateOverall, ResolveBaseline, LatestApproved |
| Submit/import tiến độ | `backend/Application/Services/ExecutionService.cs` | import đi qua submit; không công thức riêng |
| Danh sách/lọc server | `backend/Application/Services/PlanningService.cs` | dùng DocumentItemQuery và AgencyScopePolicy |
| Quyền API | `backend/Api/Security/ApiPermissionFilter.cs` | danh tính từ token đã xác thực; FE không cấp quyền |
| Database config | `backend/Application/Services/DatabaseConfiguration.cs` | không tự resolve config ở mỗi service |
| API URL và auth request | `frontend/src/config/api.js`, `services/auth.js`, `services/apiClient.js` | getApiUrl/fetchWithAuth; Tracking store đã chuyển fetch |
| Excel engine | `frontend/src/utils/excelRuntime.js` | dynamic import, writer/reader riêng |
| Excel style/export | `frontend/src/utils/worksheetStyle.js`, `excelExport.js` | feature dựng dữ liệu, adapter ghi workbook |
| Confirm và accessibility | `frontend/src/services/confirm.js`, `directives/` | component không copy event/focus loop |

## Module chung đã tạo (09/10/2026)

`frontend/src/shared/formatters.js`: formatDate (ngày lịch), formatDateTime, formatNumber, formatPercent, formatFileName. Không thêm DOM/API dependency; phân biệt ngày lịch và thời điểm.

`frontend/src/shared/statusPresentation.js`: mapping enum → label đầy đủ, label ngắn, badge, thứ tự; execution/approval/qualitative tách namespace. Một mapping cho chart/table/export.

`frontend/src/shared/agencyPresentation.js`: adapter hiển thị phạm vi dựa trên cờ/code BE, không biến heuristic tên cơ quan thành rule quyền.

`backend/Application/Services/AgencyScopePolicy.cs`: quyết định scope chung cho list/dashboard/report và write permission, có mode rõ (view/report/assign). Không dùng một predicate cho mọi quyền khi nghiệp vụ khác.

`backend/Application/Services/DocumentItemQuery.cs`: chuẩn hóa tiêu chí search/year/scope/status, sort, pagination/export. Không thêm repository abstraction chỉ để bọc DbSet.

`frontend/src/config/reporting.js`: tập trung default documentId/năm và override VITE_REPORTING_DOCUMENT_ID; lấy metadata API đầy đủ là bước tiếp theo. Năm mục tiêu và năm báo cáo là hai khái niệm riêng.

## Ranh giới module

View điều phối route/context/dialog, không chứa công thức nghiệp vụ hoặc định nghĩa workbook. Feature composable giữ request/filter/workflow của feature. Component trình bày nhận props, emit event; không tự tải dữ liệu toàn hệ thống. Shared utils thuần, không import view/feature; feature có thể import shared, chiều ngược lại không được.

BE controller bind DTO, kiểm tra quyền và điều phối service; service thực thi workflow/transaction; calculator thuần tính toán; DbContext/migration phụ trách persistence. Không thay đổi cache trong GET. Cache không phải nguồn chân lý. Logic phải cùng dữ liệu/agency/year giữa submit, approve, reject, list, dashboard và export.

Không chia sẻ state mutable giữa người dùng/request. Request FE dùng abort/version để chống stale response; cache metadata phải có cách invalidate khi metadata đổi. Không hide lỗi bằng catch trả danh sách rỗng.

## Khi gom hàm trùng

So sánh semantics trước: null, 0, timezone, agency scope, năm, pending, baseline. Viết hợp đồng và fixture cho những khác biệt có ý nghĩa, đưa hàm về chủ sở hữu, chuyển caller, xóa bản sao. Mapping DTO khác nhau thì viết adapter nhỏ. Không gom hàm chỉ vì có đoạn code giống nhau nếu một bên làm việc với mục định tính, bên kia văn bản pháp lý.

