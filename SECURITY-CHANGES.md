# Cấu hình và phân quyền sau sửa lỗi

- Tất cả API yêu cầu đăng nhập, trừ đăng nhập và hướng dẫn quên mật khẩu.
- API tài khoản, bảo trì/xóa dữ liệu, danh mục, văn bản, chỉ tiêu kế hoạch,
  gửi đôn đốc/thông báo và duyệt/từ chối báo cáo yêu cầu Admin.
- AgencyUser được gửi/import tiến độ trong phạm vi được giao; cập nhật kế hoạch,
  đầu mối và file của cơ quan mình hoặc đơn vị trực thuộc; đọc thông báo của mình.
- Vai trò, cơ quan, người báo cáo và người phê duyệt lấy từ tài khoản xác thực.
  Token của tài khoản bị khóa hoặc đã đổi vai trò/cơ quan sẽ bị từ chối.
- Khôi phục mật khẩu công khai không đổi mật khẩu, không trả mật khẩu, không tiết lộ
  tài khoản tồn tại. Quản trị viên xác minh chủ tài khoản rồi dùng API đặt lại mật khẩu.
  Chưa triển khai khôi phục tự phục vụ vì chưa có kênh email xác minh.
- HTML đôn đốc và rich text dùng allowlist khi hiển thị. Giữ định dạng văn bản cơ bản,
  bảng và link an toàn; loại bỏ script, sự kiện, style và nội dung nhúng.

## Cấu hình chạy

Production phải đặt `Jwt__SecretKey` bằng khóa ngẫu nhiên ít nhất 32 byte UTF-8.
Không còn dùng khóa dự phòng cố định. Development có thể dùng khóa tạm sinh lúc
khởi động; khi khởi động lại người dùng phải đăng nhập lại.

`DATABASE_URL` không rỗng ưu tiên hơn connection string localhost trong appsettings.
Có thể dùng `ConnectionStrings__DefaultConnection` thay thế. Không tự chuyển sang
InMemory khi thiếu cấu hình production. Muốn chạy InMemory cho thử nghiệm phải đặt
`ASPNETCORE_ENVIRONMENT=Development`, `AllowInMemoryDatabase=true`,
`UsePostgreSQL=false`, đồng thời để trống cả hai cấu hình kết nối.

Dashboard dùng `AsNoTracking` và split query để tránh nhân số hàng khi tải nhiều
collection. Kiểm thử dashboard xác nhận hành vi; chưa đo tốc độ trên PostgreSQL thật.

Các lỗi baseline và cộng dồn chưa thay đổi trong đợt này.


Cập nhật 08/10/2026: đã triển khai khôi phục qua link email, migration và accessibility. Xem PASSWORD-RECOVERY.md, DATABASE-MIGRATIONS.md và FIVE-ISSUES-IMPLEMENTED.md; các mô tả chưa hỗ trợ email ở trên là trạng thái trước nâng cấp.
