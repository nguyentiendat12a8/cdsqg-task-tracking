# Rule nghiệp vụ và hợp đồng dữ liệu

## Báo cáo năm và tiến độ

Chỉ báo cáo `PeriodYear`; không tháng/quý, không mục tiêu/nhiệm vụ con. Cơ quan vẫn phân cấp, sản phẩm đầu ra vẫn tồn tại. Số nhập là **tổng đã đạt đến năm báo cáo**, cả mã legacy Cumulative và LatestValue. Năm trước 15, năm nay tổng 35: nhập 35; không cộng thành 50. Chỉ tiêu năm nay 50 →70%.

Chọn bản approved mới nhất của đúng cơ quan/năm; màn hình tổng hiện tại chọn năm mới nhất rồi revision mới nhất trong năm. Sửa năm trước không thay đổi tổng năm sau. Pending/rejected không đổi kết quả chính thức; preview dùng cùng calculator nhưng ghi rõ trạng thái. Không tự chuyển đổi raw lịch sử từ increment sang total.

Baseline: custom đúng YYYY → TargetBaseline đúng năm →100 chỉ cho đơn vị phần trăm/legacy không đơn vị theo rule hiện tại. Đơn vị số lượng thiếu baseline: phần trăm null, không bịa 100. Custom phải dương, invariant decimal, năm 1900–9999. Một baseline dùng cho cả tỷ lệ và hoàn thành. Null hiển thị “—”, không đồng nghĩa 0.

Định lượng: actual/target×100, decimal, làm tròn 2 số; hoàn thành so giá trị chưa làm tròn. Nếu làm tròn thành 100 nhưng chưa đạt target, hiển thị 99.99. Không cap số thực tế; thanh progress có thể cap độ rộng 100% nhưng giữ tỷ lệ thật trong text. Định tính: sản phẩm NotStarted 0, Drafting 25, Reviewing 60, Submitted 85, Completed 100; lấy trung bình, tất cả sản phẩm phải complete. Không sản phẩm mới dùng trạng thái báo cáo 0/25/60/100.

Mục chung: đúng tập cơ quan đủ điều kiện, cùng năm, trung bình tỷ lệ clamped 0–100; chưa báo cáo góp 0. Hoàn thành chỉ khi tất cả cơ quan hoàn thành; không cộng số giữa cơ quan. Thời hạn/cảnh báo theo ProgressCalculator; không suy ra lại trong FE. Thời điểm hoàn thành hiện dùng LogDate, không phải ApprovedAt. Đây là quy tắc đang có; thay đổi cần nghiệp vụ xác nhận và test.

## Phê duyệt và ghi dữ liệu

Popup báo cáo: Admin chọn nhóm ALL_AGENCIES → cơ quan cấp gốc đang hoạt động loại Ministry/Province; ALL_MINISTRIES/ALL_MINISTRIES_DIRECT → Ministry; ALL_PROVINCES/ALL_PROVINCES_UBND → Province. Giao với phạm vi nhóm chủ trì của mục tiêu/nhiệm vụ để không tạo báo cáo ngoài phạm vi được giao. Lưu một ProgressLog riêng theo từng cơ quan, cùng năm/kết quả/trạng thái/sản phẩm và người thao tác; không lưu báo cáo vào ID nhóm. Toàn bộ lượt gửi dùng một transaction, lỗi một cơ quan rollback cả lượt. Nhóm doanh nghiệp chưa khai báo thành viên tạm không hỗ trợ. Đổi cơ quan/năm trong popup phải loading, nạp đúng báo cáo, reset sản phẩm/ghi chú/tệp nếu chưa có bản ghi, và chặn gửi nếu đang tải/lỗi tải; response cũ không được ghi đè lựa chọn mới.

Nhiệm vụ (riêng và chung) hiển thị trạng thái, không hiển thị tỷ lệ quy đổi của từng nhiệm vụ/cơ quan trong danh sách, lịch sử, duyệt, dashboard chi tiết và Excel. Mẫu Excel nhiệm vụ luôn dùng trạng thái, kể cả dữ liệu legacy có evaluationType không thống nhất. Tỷ lệ quy đổi định tính trong ProgressCalculator vẫn là dữ liệu nội bộ phục vụ quy tắc cảnh báo hiện hành; không dùng làm số kết quả người dùng nhập. Tỷ lệ số nhiệm vụ hoàn thành / tổng nhiệm vụ được giao của một đơn vị là chỉ số thống kê riêng và vẫn được hiển thị.

Theo yêu cầu người dùng ngày 15/10/2026, trang chủ và trung tâm báo cáo không có bộ lọc Năm báo cáo và không truyền `reportYear` khi tải thống kê, chi tiết hoặc xuất Excel; dùng báo cáo approved mới nhất theo cơ quan như danh sách mục tiêu/nhiệm vụ. Giữ nguyên `fromYear/toYear` để lọc năm thời hạn, độc lập với kỳ báo cáo. API vẫn hỗ trợ `reportYear` cho ngữ cảnh xem theo năm khi có yêu cầu rõ ràng. Các quy tắc dưới đây chỉ áp dụng khi caller truyền năm báo cáo cụ thể. Lấy báo cáo approved mới nhất đúng cơ quan/năm, baseline đúng năm. Nếu năm chưa có báo cáo: chỉ giữ kết quả từ năm trước khi mục tiêu đã đạt chỉ tiêu cuối cùng, hoặc nhiệm vụ đã hoàn thành mọi sản phẩm/trạng thái trực tiếp; không chuyển tiến độ dở dang. Chỉ tiêu cuối cùng lấy theo năm thời hạn (hoặc năm cuối của văn bản/cấu hình chỉ tiêu nếu thiếu thời hạn), không coi đạt chỉ tiêu năm là đạt toàn bộ mục tiêu. Không áp dụng giữ hoàn thành cho mục thực hiện thường xuyên. Chỉ xét báo cáo approved trước năm được chọn và revision mới nhất mỗi năm; không lấy kết quả năm tương lai. Các mục được giao vẫn hiện trong thống kê để theo dõi phần chưa thực hiện. Sản phẩm chưa có báo cáo trong năm không lấy trạng thái từ cache năm khác. Năm đã kết thúc xét thời hạn tại 31/12 của năm đó; năm hiện tại/tương lai xét ngày hiện tại. `fromYear/toYear` vẫn là bộ lọc năm thời hạn, độc lập năm báo cáo. Nếu ngữ cảnh xem có năm cụ thể, truy vấn chi tiết cơ quan và xuất Excel phải truyền cùng `reportYear`; GET không sửa cache.

Raw report, minh chứng, ghi chú, quyết định duyệt là dữ liệu gốc. Cache phần trăm/status/agency execution là dẫn xuất. Thay đổi baseline/report/duyệt/từ chối phải làm mới cache qua nguồn chung, không lưu công thức tại controller. Bản duyệt năm cũ không thay thế kết quả chính thức năm mới.

Admin vẫn phải được xác thực; role/agency/approvedBy không lấy từ query/body để cấp quyền. FE điều khiển khả năng thấy nút, BE bắt buộc kiểm tra quyền. Không trả password/hash/token recovery qua API. Quên mật khẩu hiện thông báo tính năng đang phát triển theo yêu cầu người dùng; không tự bật quy trình gửi email.

## Ngày, số và JSON

Ngày lịch (deadline/start date/date picker): `YYYY-MM-DD`, không đổi ngày vì timezone. Thời điểm (log/approval/audit): UTC ISO8601 có Z; UI hiển thị `Asia/Ho_Chi_Minh`, định dạng `HH:mm dd/MM/yyyy`. Chuẩn này cần migration/refactor riêng cho timestamp legacy; không gắn Z cho mọi chuỗi mơ hồ rồi coi đã đúng. Năm báo cáo là integer, độc lập năm của LogDate.

BE dùng decimal; API number cho actual/percentage, string invariant cho custom baseline theo hợp đồng hiện có. UI format số `vi-VN`, tối đa 2 số lẻ cho phần trăm. Không gửi số locale `1.234,5` vào decimal API. Enum mới dùng tên canonical; số legacy được normalize một lần ở adapter, không so `1`, `'1'`, `'Admin'` rải khắp view.

Response phân trang `{items,totalCount,totalPages}`; query filter/sort được server thực hiện trước pagination. Error cần mã/lời giải thích và field khi liên quan validation; 400 validation, 401 phiên đăng nhập, 403 quyền, 404 không tồn tại, 409 xung đột, 5xx lỗi server. Rule status này là chuẩn đích, không khẳng định mọi endpoint legacy đã theo.

## Database

Schema đổi qua EF migration có phiên bản, PostgreSQL thật cho migration/query/transaction. InMemory chỉ test logic không phụ thuộc relational. Khởi động chỉ migrate/seed idempotent, không reset quyền/phê duyệt hoặc di chuyển toàn bộ nhiệm vụ sang văn bản khác. Seed tra unique code trước display name; tên không phải khóa. Backup trước restore/destructive migration; xác minh item/agency/report/user và config thực tế sau restore. DB dump không chứa file minh chứng vật lý; cần lưu/restore file riêng.

Tham khảo chi tiết hiện hành: `ANNUAL-PROGRESS.md`, `CUMULATIVE-PROGRESS-PROPOSAL.md`, `DATABASE-MIGRATIONS.md` tại root. Nếu có mâu thuẫn, ưu tiên quyết định nghiệp vụ mới nhất của người dùng và cập nhật các tài liệu trong cùng thay đổi.

## Danh tính báo cáo tiến độ

Phân biệt ba đối tượng: phạm vi/cơ quan được giao (`LeadAgencyId` của mục tiêu), cơ quan thực hiện báo cáo (`AgencyId` của ProgressLog), người thao tác (`CreatedBy` từ danh tính đã xác thực). Không dùng tên cơ quan thay cho người báo cáo hoặc ghép tên cơ quan vào CreatedBy trong API lịch sử. Import của Admin cho mục tiêu chung phải chọn cơ quan cụ thể; không lưu báo cáo vào cơ quan nhóm ALL_*. Lịch sử hiển thị riêng cả ba. Bản cũ không ghi người thao tác thì không tự suy đoán để sửa audit. Form nạp báo cáo theo đúng task/cơ quan/năm và bỏ qua phản hồi tải đã cũ.

15/10/2026 (điều chỉnh theo yêu cầu người dùng): Trung tâm báo cáo có lại Năm báo cáo để thống kê cơ quan hoàn thành chỉ tiêu của năm được chọn; metrics, grid và Excel dùng cùng reportYear. Trang chủ vẫn dùng báo cáo approved mới nhất, không giới hạn năm báo cáo. Giữ Từ năm/Đến năm để lọc thời hạn ở cả hai màn hình. Quy tắc này thay thế phạm vi bỏ Năm báo cáo trên trung tâm báo cáo ở ghi chú trước.

15/10/2026 (phạm vi Năm báo cáo): Chỉ hiện và gửi reportYear khi bộ lọc đã áp dụng chọn duy nhất Goal (Chỉ mục tiêu). Chỉ nhiệm vụ hoặc Tất cả không gửi năm, dùng kết quả approved mới nhất và Excel không ghi Năm báo cáo. Giữ Từ năm/Đến năm ở mọi chế độ. Quy tắc này giới hạn lại ghi chú khôi phục Năm báo cáo trước đó.
