# Rà soát UI/UX laptop — 09/11/2026

## Kết luận

Khó dùng ở zoom 100% chủ yếu do phân bổ chiều cao và popover, không nên giải quyết bằng cách thu nhỏ toàn bộ chữ. Dùng hướng dẫn responsive của skill ui-styling trong repo UI UX PRO MAX SKILL; giữ font Be Vietnam Pro và thang chữ dễ đọc.

## Các điểm đã sửa

| Vấn đề | Cách xử lý |
| --- | --- |
| Bộ lọc dài bị khuất ở màn thấp | Đo khoảng trống trên/dưới nút; giữ popover cũ khi vừa, chỉ fallback khung phải nếu không vừa |
| Nút áp dụng nằm ngoài vùng nhìn | Field cuộn riêng; header/footer giữ trong giới hạn viewport |
| Select sát mép dưới vẫn mở ra ngoài | Tính khoảng trống thật, chọn hướng trên/dưới và giới hạn chiều cao danh sách |
| Thanh tìm kiếm ép nhiều control cùng hàng | Cho wrap, tìm kiếm co giãn, không ép rộng tối thiểu lớn hơn màn hình |
| Header tiêu đề và nhiều hành động cạnh tranh chỗ | Tách hàng ở breakpoint thích hợp; giảm khoảng trống trang trí trên laptop thấp |
| Vùng bảng trừ chiều cao cố định | Dùng vị trí thực của bảng để chừa vùng phân trang; cập nhật khi resize/header thay đổi |
| Popup có thể vượt vùng nhìn | Giới hạn theo dvh; form dài cuộn, không cần thu nhỏ trình duyệt |

## Kiểm chứng

Kiểm tra frontend local với API giả đúng hình dạng, 13 route ở 1920×1080, 1366×650, 1280×600, 1024×640 và 390×740. Laptop test chiều cao thực dụng đã trừ phần thanh trình duyệt. Giữ zoom 100%; bảng mục tiêu/nhiệm vụ có 10 dòng. Kiểm tra tọa độ panel, footer, select; cuộn nội dung không kéo footer; mở/đóng popup thêm mục; desktop lớn giữ popover. Mobile cho cuộn trang tự nhiên để tới phân trang. Không gọi lưu/tạo dữ liệu.

Đây là kiểm tra khả năng hiển thị và tương tác, chưa phải chứng nhận accessibility đầy đủ hoặc kiểm thử mọi tổ hợp dữ liệu production. Bảng có nhiều cột vẫn cuộn ngang trong vùng riêng; trang dài vẫn cuộn dọc. Mục tiêu là đọc và thao tác bình thường ở zoom 100%, không ép mọi thông tin vào cùng một khung hình.

## Khi kiểm tra thực tế

Ưu tiên thử lọc ở danh sách mục tiêu/nhiệm vụ, dashboard, báo cáo, QPPL, thông tin chuyển đổi số và quản trị tài khoản. Đã chạy thêm laptop 1366×650 với menu trái ghim mở. Khi kiểm tra thực tế, thử cả khi ghim/mở menu trái; mở select cuối bộ lọc và thay đổi kích thước cửa sổ. Kiểm tra các tên dài và dữ liệu thực trước khi triển khai diện rộng.
