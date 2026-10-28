# Thông tin chuyển đổi số – đồng bộ Google Sheets

Menu: Tiện ích → Thông tin chuyển đổi số (`#digital-news`). Danh sách dùng chung, không phụ thuộc văn bản 1266/2629. Người dùng đã đăng nhập được xem, tìm theo tên bài/bộ tỉnh, lọc bộ/tỉnh và mở link nguồn. Chỉ Admin được lưu lịch và đồng bộ ngay; các endpoint này có Authorize(Roles="Admin"). Không gọi AI, không cập nhật tiến độ mục tiêu.

## Nguồn khởi tạo

Migration `DigitalTransformationNewsSheetSync` tạo bảng DigitalNewsItems và DigitalNewsSyncSettings, không sửa dữ liệu nghiệp vụ cũ. Cấu hình mặc định bật mỗi 2 giờ, lấy trang tính gid=731708878 tại sheet do người dùng cung cấp:
https://docs.google.com/spreadsheets/d/1fOGT8aBm6X5ul2pk8p7zxzixEMklpqLVXEgsSCFrEDI/edit?gid=731708878

Nguồn được đọc qua endpoint CSV công khai trên docs.google.com; không cần lưu API key. Sheet cần cho phép bất kỳ ai có liên kết được xem. Link có gid đọc đúng tab; không có gid đọc tab đầu tiên. CSV lấy dữ liệu bảng, không đọc công thức/chỉ dẫn trong sheet như lệnh thực thi. Không tải nội dung bài báo, kể cả các link Google News đang có trong sheet; chỉ hiển thị liên kết nguồn.

## Cấu hình và quy tắc nhập

- Admin mở Cấu hình đồng bộ để thay URL, chu kỳ nguyên 1–168 giờ và bật/tắt lịch. Lưu thay đổi có bật lịch sẽ lên lịch lượt gần nhất; không thay đổi dữ liệu đã nhập.
- Cột bắt buộc: Tên bộ/tỉnh, Tên bài viết, Link nguồn bài viết. STT trên web theo danh sách đã phân trang, không dùng STT của sheet làm ID.
- Link HTTP/HTTPS hợp lệ là định danh bản ghi; bỏ fragment và tham số utm_. Dòng lặp link không thêm trùng. Đổi tiêu đề/bộ tỉnh trên cùng link sẽ cập nhật bản ghi cũ. Không xóa bài trên web khi sheet xóa dòng. Các hàng trống bị bỏ qua; hàng thiếu nội dung/link không an toàn được đếm bỏ qua. Sai header, CSV hỏng hoặc lỗi tải không làm mất danh sách hiện tại.
- Worker kiểm tra hạn mỗi phút; sau lượt xong đặt lần tới theo giờ cấu hình. Backend cần hoạt động; khi bật lại sẽ quét bù một lượt nếu đã đến hạn. Không dùng cron của trình duyệt. Chu kỳ không phụ thuộc việc người dùng mở màn hình.
- Khóa lease ở PostgreSQL ngăn hai backend/lượt thủ công chạy trùng. Network chạy ngoài transaction; kiểm tra token trong transaction trước khi commit. Giới hạn 30 giây tải sheet, 5 MB CSV, 20.000 dòng. Thất bại hiển thị lý do và lịch thử tiếp; có thể bấm Đồng bộ ngay.

## Kiểm chứng 28/10/2026

Đọc nguồn thật: 80 bản ghi hợp lệ được nạp vào database local; lượt kế tiếp 0 thêm mới, 80 đã có. Số mục tiêu/nhiệm vụ và nguồn RSS cũ giữ nguyên. 8 kiểm thử chức năng đạt, bao gồm PostgreSQL thật: lặp lượt, cập nhật/xóa dòng nguồn, lịch, chạy chồng, lỗi CSV và lọc danh sách. Playwright 1440/390px cho Admin và người xem đạt.

Chưa deploy production. Lần deploy backend cần chạy migration như cơ chế khởi động hiện có, sau đó worker sẽ nhập dữ liệu từ sheet theo cấu hình.
