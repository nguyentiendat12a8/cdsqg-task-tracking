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

25/10/2026 — Đã xử lý vòng đời khóa AI qua redeploy: keyring chuyển từ filesystem ephemeral sang PostgreSQL bằng provider chính thức, có migration/nhập keyring cũ và test restart/multi-instance. API/UI phân biệt khóa chưa lưu với khóa không giải mã được; chặn lưu khóa trống đè ciphertext lỗi.

25/10/2026 — Đã mở rộng RSS ngoài bộ 5 mục tiêu mặc định: nguồn cổng .gov.vn và báo chí có allowlist, cấu hình mục tiêu theo ID/đơn vị cho mỗi feed, kiểm chứng đúng AgencyId, chống trùng kết quả hiện hành riêng theo metric–năm–đơn vị. Kiểm thử nguồn giả mạo, phạm vi quốc gia/địa phương, migration và nguồn cũ. Nạp file giữ riêng, không tự xuất bản kết quả internet.

25/10/2026: đã bỏ phạm vi mục tiêu/đơn vị thủ công ở RSS; danh mục AI dùng chung Thiết lập từ khóa, tách nguồn bằng DocumentId. Kiểm thử PostgreSQL chứng minh một bài tạo hai đề xuất, không tự ghi báo cáo; request mới không mang lịch sử hội thoại và cấu hình từ khóa cập nhật được nạp ở lần đọc kế tiếp.

28/10/2026: thêm Tiện ích → Thông tin chuyển đổi số. Đồng bộ sheet phía backend có lease, URL hash duy nhất, migration additive; không phụ thuộc browser và không ghi tiến độ mục tiêu. Xem docs/DIGITAL-NEWS.md về kiểm thử và giới hạn môi trường.

- 02/11/2026: Tiện ích thông tin chuyển đổi số đã tách khỏi AI Internet và đồng bộ Sheet. Nguồn/từ khóa/lịch, hàng đợi đánh giá tiêu đề, kết quả và thông báo lưu PostgreSQL; đọc thủ công không ghi đề xuất số liệu hoặc tiến độ. Chủ sở hữu và kiểm thử: docs/DIGITAL-PLAN-MONITORING.md.


## 08/11/2026 — Rà soát UX thành phần dùng chung

Áp dụng hướng dẫn UI UX Pro Max trong thư mục tham khảo local (được gitignore), giữ palette và mật độ đã thống nhất.
- OverlayPanel hỗ trợ chế độ anchored cho menu Excel: bỏ ép vị trí đầu màn hình trên mobile, giữ trong viewport; focus thao tác đầu tiên sau khi hiển thị, Esc trả focus và đóng khi Tab ra ngoài. Bộ lọc hiện hữu giữ chế độ mặc định.
- DocumentItemsTable: tiêu đề cột sắp xếp hỗ trợ Enter/Space, aria-sort; tên mục là nút mở chi tiết, không phụ thuộc hover tooltip. Mobile chỉ cố định cột mã.
- accessibleData: các header có cursor-pointer và không chứa control được hỗ trợ bàn phím qua event delegation; giữ scope và semantics của bảng.
- Sidebar mở nhãn khi điều hướng bàn phím, giữ focus trên nút hiện hành, có tên vùng điều hướng.
- Kiểm tra: document-ux.browser (Goal/Task ở 1440 và 390), utilities-header.browser, reports-layout.browser, digital-news.browser, dashboard-scope.browser; sidebar kiểm tra focus riêng. Build và check:rules qua. Chưa triển khai production.

Còn cần rà soát theo nghiệp vụ: thống nhất nghĩa các nhãn chưa cập nhật/chưa báo cáo/không áp dụng; không thay nhãn bằng suy đoán dữ liệu trong đợt sửa tương tác này.


## 08/11/2026 — Typography theo UI UX Pro Max

Người dùng cho phép thay quy định font/mật độ cũ. Tự lưu Be Vietnam Pro 400/500/600/700 và OFL từ google/fonts; font-display swap, fallback Segoe UI/Arial. Thang token: body 14px desktop, 16px mobile; bảng 14px, caption 13px; heading trang/modal 20px, nhóm 16px. Control 40px desktop, tối thiểu 44px mobile; nút mobile được wrap. Giữ root 16px, palette và logic dữ liệu. Gỡ các cỡ caption 10–11px còn trong màn đầu mối về token text-xs.

Đã kiểm tra 13 route bằng system-theme.browser ở 1440/390; document-ux, utilities-header, reports-layout, digital-news; typography.browser xác minh font load, cỡ chữ thực tế và CSS zoom 200% trên màn mục tiêu. Các kiểm tra không thay thế audit accessibility toàn diện. Chuẩn hiện hành nằm đầu UI-DESIGN/CONTROLS, phần cũ giữ làm lịch sử.


## 09/11/2026 — Laptop không cần thu nhỏ toàn trang

Nguyên nhân: bộ lọc cao cố định so với vùng nhìn; popover/select định vị bằng chiều cao ước lượng; thanh tìm kiếm không wrap; bảng dùng phép trừ chiều cao cố định bất kể header thực tế. Sửa tại OverlayPanel, SearchableSelect, DocumentFilters, accessibleData/accessibleDialog và style chung. Giữ bộ lọc kiểu cũ nếu đủ chỗ theo phản hồi người dùng; fallback cạnh phải chỉ khi không vừa. Chừa footer, cuộn field riêng, giới hạn popup theo dvh. Header của mục tiêu/nhiệm vụ/báo cáo xuống hàng theo breakpoint, giảm padding trên laptop thấp. Không thay schema, filter semantics hay dữ liệu.

Kiểm chứng local: laptop-layout.browser.mjs nạp từng route hoàn chỉnh với API giả, kiểm tra 13 màn ở năm viewport. Bộ lọc, footer và select phải ở trong vùng nhìn; cuộn field không dịch footer; desktop rộng không bật fallback. Thêm popup mục tiêu/nhiệm vụ và bảng 10 dòng để kiểm tra chiều cao/phân trang. Kiểm tra này không phải chứng nhận toàn bộ màn nghiệp vụ với mọi dữ liệu production.


## 10/11/2026 — Bảng báo cáo tiến độ theo cơ quan

Gộp thanh nhóm quyết định của tab mục tiêu đủ 13 cột (bản cũ mới phủ 12), đưa nhãn giai đoạn vào cùng cell; sửa empty colspan tương ứng. Chỉnh header hai tầng sticky, khoảng cách cell, dòng xen kẽ và vùng cuộn riêng cho mục tiêu/nhiệm vụ. Chú giải trắng: dữ liệu gốc/kết quả 2025; vàng: nhập trong Excel; xám có nhãn: không áp dụng/không điền. Ô hợp lệ trống hiển thị Chưa nhập; số 0 giữ nguyên. shared/progressReportCells.js dùng ApplicableYears từ API, bắt buộc 1266 không có kết quả 2025; dùng chung cho UI và Excel để không điền số liệu vào ô loại trừ. Không thay dữ liệu đã lưu. Test progress-report-table.browser.mjs kiểm tra 2629 giai đoạn 1/2, 1266, zero/null, colspan và file Excel ở desktop/mobile.

10/11/2026 — Theo phản hồi tiếp theo: bỏ chú giải và chữ Chưa nhập/Chưa có/Không áp dụng trong ô; giữ dấu — như cũ, phân biệt bằng nền vàng nhạt và xám đậm hơn. Giữ aria-label và quy tắc năm áp dụng cho UI/Excel.


## 10/11/2026 — Excel báo cáo: màu xanh và khóa ô

unifiedProgressReportExcel dùng header xanh đồng nhất, dòng giai đoạn cùng màu header, ô nhập xanh nhạt. Chiều cao dòng tính theo nội dung wrap có dư khoảng cho dấu tiếng Việt. Ô không áp dụng để trống; ô nhập trống hiện —; số 0 và số liệu có sẵn giữ nguyên. Mục tiêu: mở khóa ước tính 2026/kế hoạch 2027–2030 theo applicableYears và ghi chú; dữ liệu gốc/kết quả quyết định/2025 bị khóa. Nhiệm vụ: mở khóa trạng thái và ghi chú. Không dùng mật khẩu; worksheet protection nhằm chống sửa nhầm, không phải cơ chế bảo mật dữ liệu.

progressWorkbook chỉ bật protection cho sheet có metadata !reportEditableCells. Sau khi writer xuất XLSX, bổ sung style unlocked và sheetProtection vào OOXML vì thư viện không xuất trạng thái khóa riêng từng ô. Giữ validation trạng thái và không ảnh hưởng workbook khác. report-workbook-protection.test.mjs kiểm tra màu, placeholder, chiều cao, style count, lock từng ô, validation và opt-in; kiểm tra XML độc lập xác nhận cột cố định khóa, ô báo cáo mở. Parser bỏ qua dấu — ở ghi chú để không lưu placeholder.


10/11/2026 — Cập nhật Excel theo phản hồi: protection có password verifier theo mật khẩu người dùng cung cấp (không ghi plaintext vào source/tài liệu). Mở khóa kết quả 2025 chỉ khi applicableYears cho phép; parser đọc riêng cột 2025 và 2026, UI preview hiện năm và bulk payload giữ periodYear từng dòng. Backend bulk hiện đã hỗ trợ năm từng item nên không đổi BE. Ô không áp dụng dùng fill F8FAFC như dữ liệu cố định và để trống. Mã ghép chuẩn hóa xuống dòng, wrap và tính chiều cao; cột số La Mã căn giữa ở hai sheet. Sheet nhiệm vụ đổi F=Sản phẩm, G=Trạng thái, H=Ghi chú; validation/unlocked metadata và hướng dẫn theo cột G. Parser theo header giữ nhận mẫu cũ. Kiểm tra e2e import, khóa/style từng ô, password hash độc lập bằng openpyxl, tách kết quả 2025/2026 và giá trị zero.


11/11/2026 — ProgressReportsView xuất Excel không bật isLoading của bảng nữa; dùng isExporting riêng và chặn gửi lặp. Chốt agencyId/agencyName tại lúc bấm để tên file khớp dữ liệu tải; không thay tab/page/list hiện hành. progress-export-state.browser.mjs trì hoãn API xuất để kiểm tra table DOM giữ nguyên, không có frame navigation, download thành công và nhánh lỗi trả lại trạng thái nút.


### 11/11/2026 — Đối chiếu trước khi nhập báo cáo Excel

- So sánh kết quả theo năm, kế hoạch và trạng thái/ghi chú với snapshot API hiện tại; chỉ gửi các bản ghi thay đổi. Chuẩn hóa trạng thái tương đương và giá trị số để không đếm lại dữ liệu giữ nguyên.
- Bảng xem trước theo cột năm/trạng thái/ghi chú, tô ô thay đổi và hiện giá trị cũ → mới; không có thay đổi thì không cho xác nhận.
- Kiểm chứng: fixture chỉ sửa hai kết quả 2025, không gửi lại kết quả 2026/kế hoạch/nhiệm vụ; kiểm tra trình duyệt laptop/mobile và build frontend.


### 11/11/2026 — Phạm vi cơ quan trong báo cáo tiến độ

- Báo cáo dùng AgencyAssignments.Filter theo phân giao chủ trì thay vì tìm tên cơ quan trong nhãn hiển thị. Cùng nguồn cho danh sách/phân trang và fetchAll của xuất/nhập Excel.
- FE bỏ phản hồi cũ khi đổi cơ quan/tab; không hiển thị dữ liệu cơ quan trước trong lúc tải.
- Kiểm chứng: sáu test AgencyProgressReportTests, có trường hợp cơ quan chỉ có tên trong nhãn hoặc chỉ phối hợp phải bị loại; kiểm tra phản hồi sai thứ tự trên trình duyệt.


### 11/11/2026 — Mã nhiệm vụ trùng giữa hai quyết định khi nhập Excel

- Đối chiếu ID nhiệm vụ/ID quyết định ở cột ẩn và quyết định của khối thay vì lấy dòng đầu tiên cùng mã NV. File cũ dùng mã + khối quyết định; nếu không duy nhất thì bỏ qua và cảnh báo, không đoán.
- Chuẩn hóa xuống dòng ghi chú để tránh báo thay đổi do CRLF/LF.
- Kiểm chứng: xuất/nhập nguyên file có hai NV-03 khác quyết định không sinh cập nhật; sửa một trạng thái chỉ highlight đúng nhiệm vụ của quyết định đó.


### 11/11/2026 — Tách báo cáo tiến độ trong Bộ

- Tách tab đơn vị thuộc Bộ thành submenu Báo cáo tiến độ trong Bộ (#reports-progress-internal), tái sử dụng ProgressReportsView theo chế độ unitsOnly, remount khi đổi màn để không giữ nhầm cơ quan/tab.
- Giữ chọn đơn vị, mục tiêu/nhiệm vụ và luồng nhập/xuất hiện có; báo cáo thông thường chỉ còn hai tab.
- Kiểm chứng build và trình duyệt laptop/mobile: deep link, reload, chuyển về báo cáo cơ quan và đúng bộ chọn đơn vị.


### 11/11/2026 — Xác nhận đơn vị nhập báo cáo Excel

- Popup nhập có đơn vị đích độc lập, mặc định lấy từ màn danh sách; đổi đơn vị xóa preview cũ để đối chiếu lại. Request đọc/ghi dùng cùng đơn vị đích.
- API tiến độ kiểm tra phạm vi và phân giao của đơn vị đích kể cả Admin; không âm thầm ghi sang đơn vị mặc định. API kế hoạch kiểm tra đơn vị chủ trì nếu request có ReportingAgencyId; đơn vị thuộc Bộ không sửa kế hoạch dùng chung.
- Không hiển thị thành công khi API trả lỗi từng dòng. Kiểm chứng unit/backend, PostgreSQL và trình duyệt desktop/mobile: mặc định, đổi đơn vị, đối chiếu lại và payload.


### 11/11/2026 — Đồng bộ cấu trúc báo cáo web và Excel

- Web mục tiêu bỏ ĐVT (Excel giữ); preview có STT, mã, tên, hai giai đoạn, kết quả thực hiện, kế hoạch năm và ghi chú như Excel, chia khối theo quyết định. Highlight cũ → mới chỉ ở ô thay đổi.
- Web/preview/Excel nhiệm vụ bỏ cơ quan phối hợp. Excel còn 7 cột: STT, mã, tên, hạn, sản phẩm, trạng thái, ghi chú; dropdown ở F, ghi chú G, metadata ẩn H/I. Khóa ô/validation và chiều cao theo cột mới.
- Parser tìm cột theo tiêu đề, giữ tương thích file cũ còn cơ quan phối hợp. Kiểm chứng bảo vệ XML, export/import số liệu, mã trùng hai quyết định, file cũ và trình duyệt desktop/mobile.


### 11/11/2026 — Sửa bố cục preview nhập báo cáo

- Loại Ghi chú khỏi hàng tiêu đề phụ của mục tiêu (đã rowspan ở hàng trên), giữ 12 cột khớp dữ liệu/khối quyết định. Cột mã tối thiểu 112px, chỉ xuống dòng giữa các mã ghép.
- Hai ô tổng đếm mục tiêu/nhiệm vụ thay đổi theo dòng preview, bao gồm thay đổi kế hoạch; không đếm một mục tiêu nhiều lần theo năm.
- Popup giới hạn theo dynamic viewport, vùng preview có min-height/min-width 0 và cuộn riêng, footer xác nhận cố định ngoài vùng cuộn; bảng rộng cuộn ngang bên trong.
- Kiểm chứng browser 1366×650, 390×844, 360×640, 844×390: một tiêu đề Ghi chú, 12/7 cột, mã không gãy, hai ô tổng, popup/footer nằm trong viewport và không tràn ngang trang; build thành công.


### 11/11/2026 — Sửa HTTP 400 khi xác nhận nhập tiến độ

- Payload nhiệm vụ chuyển trạng thái chữ sang mã TextStatusEnum của API (1/2/3/4) qua adapter trong statusPresentation; không gửi Completed/Drafting dưới dạng chuỗi cho enum mặc định của ASP.NET. Giá trị null cho mục tiêu giữ nguyên.
- Dùng parseApiError cho lỗi tiến độ/kế hoạch để đọc ValidationProblemDetails thay vì mất chi tiết lỗi.
- Kiểm chứng mapping tất cả trạng thái và trình duyệt bốn kích thước: payload nhiệm vụ numeric, đúng đơn vị đích và đúng năm kết quả; build frontend.


### 11/11/2026 — Hiển thị lỗi nhập và bỏ vai trò tài khoản khỏi payload

- Tái hiện lỗi JSON userRole số 1 không chuyển được sang string của DTO, dẫn đến HTTP 400 trước khi controller xử lý. FE không gửi userRole/userAgencyId trong bulk import; ApiPermissionFilter lấy thông tin từ claims như hiện có.
- Đăng ký Vue3Toastify và stylesheet ở bootstrap để toast thực sự hiển thị; thêm lỗi tồn tại ngay trong popup nhập với role alert, giữ preview khi thất bại. Tên lỗi validation nhận diện đơn vị nhập, vai trò và dòng/trường cụ thể.
- Kiểm chứng JSON contract .NET và browser bốn viewport: API 400 hiện cả toast và alert, không báo thành công, thử lại được, payload không chứa vai trò giả định; build frontend.
