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


Dashboard địa phương: ProvinceCatalog (BE) là danh mục cố định 34 tỉnh/thành và seed chỉ bổ sung thiếu; AgencyStatusSummaryDto trả tỷ lệ số mục hoàn thành. ProvinceProgressMap lazy load đường biên 34 tỉnh, chỉ hiển thị tỷ lệ BE và dùng drilldown chung.

13/10/2026: ProvinceCatalog chuẩn hóa tên để kiểm tra tồn tại (không dấu, hoa/thường, khoảng trắng và tiền tố UBND/tỉnh/TP/Thành phố). Seed chỉ thêm địa phương thiếu với tên ngắn, không sửa bản ghi production đã có. DisplayName cung cấp tên ngắn cho API danh mục/dashboard; giữ nguyên ID, mã và tên lưu trữ legacy.

13/10/2026: services/exportData.loadAllExportPages là nguồn FE tải đầy đủ trang cho xuất danh sách mục tiêu/nhiệm vụ, QPPL, lịch sử thông báo và lịch sử nạp dữ liệu. Caller giữ query đã áp dụng; loader chỉ thay pageNumber/pageSize, kiểm tra totalCount, ID trùng và lỗi trang sau, không xuất file thiếu dòng. QPPL/hai lịch sử lưu URL từ lần tải danh sách thành công để không xuất theo draft chưa áp dụng.

15/10/2026: Theo yêu cầu trực tiếp của người dùng, bỏ bộ chọn Năm báo cáo ở dashboard/trung tâm báo cáo và tham số reportYear trong metrics, drilldown, grid và Excel. Hai màn hình dùng báo cáo approved mới nhất; giữ nguyên Từ năm/Đến năm (năm thời hạn), các bộ lọc khác và năm khi nhập báo cáo. Đây là thay đổi so với quy tắc xem theo năm trước đó, không thay đổi dữ liệu báo cáo hay API theo năm dành cho ngữ cảnh khác.

15/10/2026 (điều chỉnh theo yêu cầu người dùng): Trung tâm báo cáo có lại Năm báo cáo để thống kê cơ quan hoàn thành chỉ tiêu của năm được chọn; metrics, grid và Excel dùng cùng reportYear. Trang chủ vẫn dùng báo cáo approved mới nhất, không giới hạn năm báo cáo. Giữ Từ năm/Đến năm để lọc thời hạn ở cả hai màn hình. Quy tắc này thay thế phạm vi bỏ Năm báo cáo trên trung tâm báo cáo ở ghi chú trước.

15/10/2026 (phạm vi Năm báo cáo): Chỉ hiện và gửi reportYear khi bộ lọc đã áp dụng chọn duy nhất Goal (Chỉ mục tiêu). Chỉ nhiệm vụ hoặc Tất cả không gửi năm, dùng kết quả approved mới nhất và Excel không ghi Năm báo cáo. Giữ Từ năm/Đến năm ở mọi chế độ. Quy tắc này giới hạn lại ghi chú khôi phục Năm báo cáo trước đó.
