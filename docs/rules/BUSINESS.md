# Rule nghiệp vụ và hợp đồng dữ liệu

## Báo cáo năm và tiến độ

Chỉ báo cáo `PeriodYear`; không tháng/quý, không mục tiêu/nhiệm vụ con. Cơ quan vẫn phân cấp, sản phẩm đầu ra vẫn tồn tại. Số nhập là **tổng đã đạt đến năm báo cáo**, cả mã legacy Cumulative và LatestValue. Năm trước 15, năm nay tổng 35: nhập 35; không cộng thành 50. Chỉ tiêu năm nay 50 →70%.

Chọn bản approved mới nhất của đúng cơ quan/năm; màn hình tổng hiện tại chọn năm mới nhất rồi revision mới nhất trong năm. Sửa năm trước không thay đổi tổng năm sau. Pending/rejected không đổi kết quả chính thức; preview dùng cùng calculator nhưng ghi rõ trạng thái. Không tự chuyển đổi raw lịch sử từ increment sang total.

Baseline: custom đúng YYYY → TargetBaseline đúng năm →100 chỉ cho đơn vị phần trăm/legacy không đơn vị theo rule hiện tại. Đơn vị số lượng thiếu baseline: phần trăm null, không bịa 100. Custom phải dương, invariant decimal, năm 1900–9999. Một baseline dùng cho cả tỷ lệ và hoàn thành. Null hiển thị “—”, không đồng nghĩa 0.

Định lượng: actual/target×100, decimal, làm tròn 2 số; hoàn thành so giá trị chưa làm tròn. Nếu làm tròn thành 100 nhưng chưa đạt target, hiển thị 99.99. Không cap số thực tế; thanh progress có thể cap độ rộng 100% nhưng giữ tỷ lệ thật trong text. Định tính: sản phẩm NotStarted 0, Drafting 25, Reviewing 60, Submitted 85, Completed 100; lấy trung bình, tất cả sản phẩm phải complete. Không sản phẩm mới dùng trạng thái báo cáo 0/25/60/100.

Mục chung: đúng tập cơ quan đủ điều kiện, cùng năm, trung bình tỷ lệ clamped 0–100; chưa báo cáo góp 0. Hoàn thành chỉ khi tất cả cơ quan hoàn thành; không cộng số giữa cơ quan. Thời hạn/cảnh báo theo ProgressCalculator; không suy ra lại trong FE. Thời điểm hoàn thành hiện dùng LogDate, không phải ApprovedAt. Đây là quy tắc đang có; thay đổi cần nghiệp vụ xác nhận và test.

## Phê duyệt và ghi dữ liệu

Raw report, minh chứng, ghi chú, quyết định duyệt là dữ liệu gốc. Cache phần trăm/status/agency execution là dẫn xuất. Thay đổi baseline/report/duyệt/từ chối phải làm mới cache qua nguồn chung, không lưu công thức tại controller. Bản duyệt năm cũ không thay thế kết quả chính thức năm mới.

Admin vẫn phải được xác thực; role/agency/approvedBy không lấy từ query/body để cấp quyền. FE điều khiển khả năng thấy nút, BE bắt buộc kiểm tra quyền. Không trả password/hash/token recovery qua API. Quên mật khẩu hiện thông báo tính năng đang phát triển theo yêu cầu người dùng; không tự bật quy trình gửi email.

## Ngày, số và JSON

Ngày lịch (deadline/start date/date picker): `YYYY-MM-DD`, không đổi ngày vì timezone. Thời điểm (log/approval/audit): UTC ISO8601 có Z; UI hiển thị `Asia/Ho_Chi_Minh`, định dạng `HH:mm dd/MM/yyyy`. Chuẩn này cần migration/refactor riêng cho timestamp legacy; không gắn Z cho mọi chuỗi mơ hồ rồi coi đã đúng. Năm báo cáo là integer, độc lập năm của LogDate.

BE dùng decimal; API number cho actual/percentage, string invariant cho custom baseline theo hợp đồng hiện có. UI format số `vi-VN`, tối đa 2 số lẻ cho phần trăm. Không gửi số locale `1.234,5` vào decimal API. Enum mới dùng tên canonical; số legacy được normalize một lần ở adapter, không so `1`, `'1'`, `'Admin'` rải khắp view.

Response phân trang `{items,totalCount,totalPages}`; query filter/sort được server thực hiện trước pagination. Error cần mã/lời giải thích và field khi liên quan validation; 400 validation, 401 phiên đăng nhập, 403 quyền, 404 không tồn tại, 409 xung đột, 5xx lỗi server. Rule status này là chuẩn đích, không khẳng định mọi endpoint legacy đã theo.

## Database

Schema đổi qua EF migration có phiên bản, PostgreSQL thật cho migration/query/transaction. InMemory chỉ test logic không phụ thuộc relational. Khởi động chỉ migrate/seed idempotent, không reset quyền/phê duyệt hoặc di chuyển toàn bộ nhiệm vụ sang văn bản khác. Seed tra unique code trước display name; tên không phải khóa. Backup trước restore/destructive migration; xác minh item/agency/report/user và config thực tế sau restore. DB dump không chứa file minh chứng vật lý; cần lưu/restore file riêng.

Tham khảo chi tiết hiện hành: `ANNUAL-PROGRESS.md`, `CUMULATIVE-PROGRESS-PROPOSAL.md`, `DATABASE-MIGRATIONS.md` tại root. Nếu có mâu thuẫn, ưu tiên quyết định nghiệp vụ mới nhất của người dùng và cập nhật các tài liệu trong cùng thay đổi.
