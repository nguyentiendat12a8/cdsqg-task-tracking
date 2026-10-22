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

13/10/2026: Thêm bản đồ 34 tỉnh và KPI lãnh đạo; tỷ lệ hoàn thành số mục được cung cấp bởi BE, null khi chưa có mục. Seed ProvinceCatalog bổ sung thiếu và giữ ID cũ; danh mục tỉnh khóa tạo/sửa danh tính/xóa qua AgencyController và FE nhận IsFixedProvince. Không tự chuyển dữ liệu từ tỉnh cũ hoặc xóa cơ quan legacy.

13/10/2026: Sửa QPPL, lịch sử thông báo/nạp dữ liệu xuất trang hiện tại thành xuất toàn bộ theo query danh sách. Browser test đọc lại XLSX 101 dòng/2 trang, server giới hạn pageSize, giữ bộ lọc tìm kiếm, desktop/mobile, trang sau lỗi không tạo file thiếu. Kế hoạch đơn vị dùng API agencies không phân trang; báo cáo tổng hợp dùng tập đầy đủ trước slice; dashboard dùng endpoint agency-items không phân trang. Sửa thẻ div đóng thiếu sẵn có trong UrgeHistoryView để kiểm chứng luồng này.

15/10/2026: Theo yêu cầu trực tiếp của người dùng, bỏ bộ chọn Năm báo cáo ở dashboard/trung tâm báo cáo và tham số reportYear trong metrics, drilldown, grid và Excel. Hai màn hình dùng báo cáo approved mới nhất; giữ nguyên Từ năm/Đến năm (năm thời hạn), các bộ lọc khác và năm khi nhập báo cáo. Đây là thay đổi so với quy tắc xem theo năm trước đó, không thay đổi dữ liệu báo cáo hay API theo năm dành cho ngữ cảnh khác.

15/10/2026 (điều chỉnh theo yêu cầu người dùng): Trung tâm báo cáo có lại Năm báo cáo để thống kê cơ quan hoàn thành chỉ tiêu của năm được chọn; metrics, grid và Excel dùng cùng reportYear. Trang chủ vẫn dùng báo cáo approved mới nhất, không giới hạn năm báo cáo. Giữ Từ năm/Đến năm để lọc thời hạn ở cả hai màn hình. Quy tắc này thay thế phạm vi bỏ Năm báo cáo trên trung tâm báo cáo ở ghi chú trước.

15/10/2026 (phạm vi Năm báo cáo): Chỉ hiện và gửi reportYear khi bộ lọc đã áp dụng chọn duy nhất Goal (Chỉ mục tiêu). Chỉ nhiệm vụ hoặc Tất cả không gửi năm, dùng kết quả approved mới nhất và Excel không ghi Năm báo cáo. Giữ Từ năm/Đến năm ở mọi chế độ. Quy tắc này giới hạn lại ghi chú khôi phục Năm báo cáo trước đó.

15/10/2026 — Phân giao theo cơ quan:
- Một GoalTaskItem gốc; AgencyTaskExecution có khóa duy nhất (GoalTaskId, AgencyId), lưu IsLeadAssignment/IsCoordinatingAssignment độc lập và nguồn qua nhóm cho từng vai trò. Một cơ quan có thể có cả hai vai trò, danh sách vẫn chỉ một mục gốc. Không tạo mục tiêu/nhiệm vụ con trong GoalTaskItems.
- AgencyAssignments mở rộng nhóm bằng ReportingAgencyGroups khi tạo/sửa phân giao; chốt các cơ quan nhận giao tại thời điểm ghi. Sửa tên/chỉ tiêu không tự cập nhật thành viên nhóm. API refresh-assignments có Authorize Admin và guard trực tiếp; Admin đồng bộ có xác nhận từ tab Tiến độ các đơn vị. Bỏ phân giao chỉ tắt vai trò, không xóa lịch sử/ID/báo cáo/phê duyệt.
- Migration 20261015120000_PersistAgencyAssignments backfill chủ trì/phối hợp (trực tiếp và qua 5 nhóm đã có ánh xạ), giữ execution ID/cache và toàn bộ ProgressLog. Nhóm chưa có ánh xạ thành viên không tự gán tất cả cơ quan.
- Lọc chủ trì hoặc phối hợp dùng cùng assignmentRole=Lead/Coordinating trên list, metrics, drilldown, grid và Excel. Phối hợp không cấp thêm quyền gửi báo cáo chủ trì. Trạng thái chính thức luôn từ ProgressLog approved theo đúng cơ quan qua ProgressCalculator, không dùng cache của mục gốc hay cơ quan khác.
- SQL lọc phân giao bằng EXISTS trước Include báo cáo; index bắt đầu AgencyId cho từng vai trò. Danh sách sắp mã/tên, không tìm chữ/lọc trạng thái dẫn xuất, dùng COUNT/LIMIT/OFFSET tại DB. Lọc/sắp trạng thái hay tiến độ vẫn tính trên tập khớp cơ quan trước khi phân trang; không có truy vấn riêng từng dòng.
- Reset riêng tiến độ giữ bản ghi phân giao; xóa cơ quan có execution bị chặn để bảo toàn lịch sử. GET không tạo/cập nhật phân giao. Tab ma trận hiển thị cả cơ quan chưa gửi báo cáo và nhãn vai trò; đơn vị chưa báo cáo là NotReported.

19/10/2026 — Bổ sung nguồn Google Alerts RSS theo yêu cầu: bảng nguồn, lịch một lần/ngày theo giờ Việt Nam và Quét ngay; job bền, khóa hàng, unique pending scan và URL hash chống trùng. Dùng lại pipeline đọc/kiểm chứng AI và component hộp tiếp nhận, không tạo cơ chế phê duyệt riêng. Kiểm thử PostgreSQL cho lịch/job/trùng/AI tắt, phân loại tiêu đề và không tự công bố; kiểm tra browser desktop/mobile và quyền Admin. Đã kết nối nguồn Google Alerts thật trên local và bật lịch 08:00 Việt Nam; RSS vừa tạo trả 0 bài. Luồng có bài vẫn cần kiểm chứng với bài RSS thực tế. Bài ngoài allowlist và bài chỉ dẫn link đính kèm chưa tự tải.

Quản trị AI — đã gom file và RSS vào Nguồn dữ liệu chung, bỏ submenu RSS riêng nhưng giữ alias URL, chuyển mục tiêu áp dụng sang Xem thêm ở Kiểm chứng. Dùng chung header/control/palette và thẻ trên màn hình hẹp; nạp file vào hộp thoại. Cấu hình AI bố trí hai vùng kết nối/mô hình và một thanh lưu. Không đổi BE, schema, khóa API hoặc dữ liệu đã duyệt trong đợt chỉnh UI này.

21/10/2026 — Bổ sung chẩn đoán 429 có cấu trúc trong source: quota bằng 0, quota ngày, giới hạn theo phút, tín dụng và phản hồi không xác định; giữ thời gian thử lại do provider cung cấp, không tự retry 429 mù. Test bằng phản hồi giả lập và caller bóc tách; kết quả test không chứng minh quota thực tế trên production. Bản sửa cần được triển khai backend để các lỗi mới trên production có thông báo chi tiết; lỗi cũ đã lưu không thể phục hồi phần response từng bị bỏ.

22/10/2026 — Gom trạng thái MT/NV về ba loại theo yêu cầu người dùng: ProgressCalculator trả Status ba giá trị và DeadlineStatus riêng; ExecutionStatuses chuẩn hóa giá trị cũ cho filter/sort, ExecutionStatusCounts đếm thống nhất. statusPresentation/executionStatusSeries và ExecutionStatusBreakdown dùng chung cho các legend/biểu đồ/filter/Excel. Migration AddAgencyDisplayNames bổ sung hai trường tên hiển thị đã có trên CLR model nhưng thiếu migration; giữ nguyên text đã tồn tại ở DB cũ bằng ADD COLUMN IF NOT EXISTS, không sửa dữ liệu nghiệp vụ.

22/10/2026 — Trang chủ thiết kế lại theo yêu cầu người dùng, giữ ProvinceProgressMap nguyên bản. LeadershipOverview trình bày chỉ số unique/assignment riêng; leadershipPresentation chỉ adapter DTO và độ rộng thanh trực quan, không tính trạng thái/tỷ lệ nghiệp vụ. LeadershipWatchlist trình bày cảnh báo BE và phân bổ cơ quan; AgencyPerformanceBoard tìm/sắp/phân trang tập thống kê và mở drilldown hiện có. Bỏ tải thống kê QPPL không dùng và biểu đồ cột lớn khỏi trang chủ. DashboardHeader có draft/applied cho popup; đóng không áp dụng thay đổi. DocumentModeSwitcher dùng trigger/heading slot của OverlayPanel chung, giữ context/storage và hỗ trợ phím mũi tên/Enter/Esc. Theo yêu cầu co gọn mới, bỏ khối tròn lặp số liệu, rút gọn header/KPI, đưa bản đồ và cảnh báo sát đầu trang.
