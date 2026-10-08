# Khôi phục mật khẩu qua email

Đã có POST /api/auth/forgot-password và POST /api/auth/reset-password cùng màn hình
`/#reset-password?token=...`. Token ngẫu nhiên 256 bit, lưu hash SHA-256, hạn 20 phút,
chỉ dùng một lần bằng UPDATE có điều kiện. Reset thành công đổi SecurityStamp để
vô hiệu token đăng nhập cũ. API công khai không trả mật khẩu hoặc token reset.

## Cấu hình môi trường

- `PasswordRecovery__FrontendUrl`: URL HTTPS của frontend, ví dụ https://portal.example.gov.vn.
  Localhost HTTP được phép cho phát triển; URL lấy từ cấu hình, không từ Host header.
- `Smtp__Host`, `Smtp__Port` (mặc định 587).
- `Smtp__EnableSsl=true` dùng STARTTLS.
- `Smtp__Username`, `Smtp__Password`, `Smtp__From`.

Giữ password SMTP trong secret/environment; không commit vào appsettings.
Email tài khoản phải là địa chỉ đã được quản trị viên xác minh khi cấp tài khoản.
Không có chức năng công khai thay email. Email trùng nhiều tài khoản không tự gửi.

Chưa có cấu hình SMTP thật trong repo; phải cấu hình các giá trị trên và gửi thử
bằng tài khoản kiểm thử trước khi dùng production. SMTP lỗi thì token vừa sinh được
vô hiệu hóa, ghi loại lỗi vào log, phản hồi vẫn chung để không lộ tài khoản tồn tại.
Endpoint giới hạn 5 lượt/10 phút theo IP. Hệ thống nhiều instance cần rate limiter
dùng chung; proxy cần cấu hình forwarded headers chỉ với proxy tin cậy.

Người dùng đặt mật khẩu 12–128 ký tự. Token được bỏ khỏi address bar ngay khi mở
màn hình và chỉ giữ trong bộ nhớ component. Refresh lại trang cần mở lại link email.
Sau nâng cấp SecurityStamp, người dùng cần đăng nhập lại.

> Tạm dừng theo yêu cầu người dùng: giao diện và API chỉ báo **Tính năng đang phát triển**; không gửi email và không đổi mật khẩu. Source dịch vụ được giữ để phát triển sau.
