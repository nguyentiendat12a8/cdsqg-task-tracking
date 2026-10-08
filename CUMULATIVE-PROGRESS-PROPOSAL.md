# Quy ước tổng kết quả theo năm — đã thống nhất ngày 09/10/2026

Số nhập là **tổng kết quả đã đạt đến năm báo cáo**, không phải phần tăng thêm của riêng năm đó.

- Năm 2026 đạt 15; đến năm 2027 tổng đạt 35: nhập 35 cho năm 2027, không nhập 20.
- Hệ thống dùng trực tiếp 35, không cộng 15 thành 50. Nếu chỉ tiêu năm 2027 là 50 thì tiến độ 70%.
- Chỉ dùng bản đã duyệt mới nhất của đúng năm, đúng mục và đúng cơ quan. Bản sửa thay thế số cũ; chờ duyệt chỉ là bản xem trước.
- Chỉnh sửa năm trước không làm thay đổi tổng đã báo cáo của năm sau.
- Baseline của năm là chỉ tiêu tổng cần đạt đến năm đó; cùng một baseline quyết định tỷ lệ và hoàn thành.
- Mã `Cumulative` và `LatestValue` được giữ để tương thích dữ liệu/API, đều dùng quy ước tổng năm này.

`ProgressCalculator` áp dụng chung cho submit, phê duyệt, dashboard, chi tiết và export. Không cộng giữa cơ quan. Mục chung tổng hợp tỷ lệ của các cơ quan theo quy tắc riêng.

Không tự chuyển đổi số đã nhập trong lịch sử: nếu một báo cáo cũ từng nhập phần tăng thêm, cần người phụ trách kiểm tra và gửi bản sửa tổng chính xác. Không xóa lịch sử, quyết định phê duyệt hoặc minh chứng.

Kiểm thử gồm: không cộng trùng năm trước; bản sửa cùng năm thay thế tổng; bản chờ duyệt không thay đổi kết quả chính thức; hai mã phương pháp cùng cho 35/50 = 70%; sửa năm trước không thay đổi năm sau; cache dùng cùng bộ tính.
