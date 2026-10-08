# Kết quả xử lý 5 vấn đề — 08/10/2026

## 3. Database

Đã thay SQL cập nhật schema trong Program.cs bằng hai EF migration có snapshot và
SQL idempotent. Kiểm thử trên PostgreSQL 18 riêng xác nhận tạo schema, chạy lại,
adopt baseline, từ chối kiểu cột sai và cập nhật schema recovery. Xem DATABASE-MIGRATIONS.md.
Chưa áp dụng lên database thật; database legacy cần backup và kiểm tra baseline trước.

## 5. Khôi phục tài khoản

Đã có gửi link SMTP, màn hình reset, hash token 256-bit, hạn 20 phút, chống dùng lại
và chống hai request reset đồng thời. Đổi mật khẩu vô hiệu phiên JWT cũ. Public API
không trả password/token và phản hồi chung cho tài khoản có/không tồn tại. Xem
PASSWORD-RECOVERY.md để cấu hình. Chưa có thông tin SMTP production trong repo.

## 7. UX dữ liệu

Đã có phản hồi chung cho lỗi mạng, 403, 429 và server lỗi trong fetchWithAuth.
Đã kiểm tra component với 200 cơ quan tên dài, tìm kiếm rỗng, bảng 1600 px trên mobile,
và report đang Pending (nút gửi bị khóa). Không tự gửi lại thao tác ghi khi lỗi.
Các fixture giả lập tình huống thực tế; chưa nghiệm thu với dữ liệu production.

## 8. Accessibility

Modal nghiệp vụ dùng directive quản lý focus, giữ Tab, Escape và phục hồi focus.
Modal con đóng riêng; alertdialog xác nhận có ưu tiên. SearchableSelect hỗ trợ
Enter/Space/mũi tên/Escape, vai trò combobox/listbox/option và aria-selected.
DatePicker hỗ trợ mở bằng bàn phím, mũi tên đổi ngày, nhãn ngày/tháng/năm.
Bảng có tên truy cập, scope column header và vùng cuộn focus được.
LoadingSpinner thông báo status cho công nghệ hỗ trợ.
Đã kiểm tra semantic role và bàn phím bằng Edge headless; chưa kiểm tra thủ công
với NVDA/JAWS hoặc toàn bộ bộ lọc nhiều cấp.

## 9. Cảnh báo build

Đã sửa các cảnh báo nullable và biến không dùng; tham chiếu EF Relational 9.0.2 rõ
ràng để loại xung đột 9.0.1/9.0.2. Đã sửa cảnh báo analyzer trong test.
Vite vẫn cảnh báo chunk Excel 870 KB; đây là thư viện export tách riêng, không phải
cảnh báo nullable/EF. EF CLI đang 9.0.0 cũng có thông báo cũ hơn runtime 9.0.2.

## Chạy kiểm tra

- `dotnet test backend.Tests/backend.Tests.csproj` với CDSQG_TEST_POSTGRES dành cho test.
- `npm --prefix frontend run build`.
- Các script Edge: frontend/tests/ui-review.mjs, security.browser.mjs,
  data-accessibility.mjs; đặt PLAYWRIGHT_PACKAGE tới package.json nơi có Playwright.

Không deploy, không chỉnh dữ liệu thật, không lưu secret SMTP trong source.
