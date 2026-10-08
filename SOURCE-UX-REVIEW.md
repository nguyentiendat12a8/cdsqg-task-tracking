# Đánh giá source và UI/UX — 08/10/2026

## Đánh giá hiện tại

Hệ thống có đủ các nhóm nghiệp vụ: mục tiêu/nhiệm vụ, báo cáo tiến độ, phê duyệt,
đôn đốc, văn bản, cơ quan và tài khoản. Backend đã tách controller, service và
entity; frontend dùng Vue và các component tái sử dụng. Tuy vậy controller/service
và một số view chứa nhiều logic, gây khó kiểm thử và khó giữ các báo cáo nhất quán.

Phạm vi kiểm tra gồm cấu trúc source, các luồng đã rà soát trong cuộc trò chuyện,
shell ứng dụng, điều hướng, xác thực, dashboard, rich text và hộp xác nhận.
Kiểm tra UI lần này dùng API giả lập rỗng ở 390×844 và 1440×900; không phải nghiệm
thu mọi màn hình với dữ liệu thực. Chưa benchmark PostgreSQL hay kiểm thử tải.

## Đã cải thiện trong đợt này

| Vấn đề | Cải thiện |
|---|---|
| Tải toàn bộ view ngay khi mở | Tải view theo nhu cầu; entry JS giảm từ khoảng 1,79 MB xuống 175 KB |
| Sidebar chiếm chỗ trên điện thoại | Menu drawer, nút mở/đóng, đóng bằng nền hoặc chọn mục |
| Hộp xác nhận thiếu focus bàn phím | Alertdialog, focus nút hủy mặc định, giữ Tab trong dialog, Escape, phục hồi focus |
| Hai yêu cầu xác nhận có thể làm promise cũ treo | Hủy yêu cầu trước khi mở xác nhận mới |
| Không tìm thấy nhiệm vụ trong thông báo nhưng mở mục đầu tiên | Bỏ fallback mở nhầm nhiệm vụ |
| localStorage hỏng làm ứng dụng không khởi động | Bắt lỗi JSON và bỏ phiên lưu không hợp lệ |
| Dictionary baseline rỗng/thiếu kỳ bỏ qua baseline thường | Fallback khi không tìm được baseline tùy chỉnh hợp lệ |
| Phân trang tài khoản có thể nhận size=0 hoặc quá lớn | Chuẩn hóa page >=1, size trong 1–100 |
| Khó nhận biết focus/không hỗ trợ giảm chuyển động | Focus rõ ràng, tôn trọng prefers-reduced-motion |

## Các vấn đề còn cần xử lý

1. **Tiến độ cộng dồn:** vẫn cần thống nhất quy ước số nhập và thực hiện
   `CUMULATIVE-PROGRESS-PROPOSAL.md`. Không thay đổi quy tắc nghiệp vụ này trong
   đợt cải thiện UI; cần đồng bộ cả submit, duyệt và dashboard.
2. **Baseline và thời gian:** cần một bộ tính tiến độ dùng chung cho tháng/quý/năm,
   baseline tùy chỉnh và thời hạn, kèm kiểm thử nghiệp vụ.
3. **Database:** cập nhật schema thủ công trong Program.cs cần chuyển sang migration
   có phiên bản. Cần kiểm thử bằng PostgreSQL, không chỉ EF InMemory.
4. **Phân quyền:** filter đang áp dụng chính sách Admin cho nhiều thao tác ghi;
   cần đối chiếu ma trận quyền nghiệp vụ của cấp 1/2/3 trước nghiệm thu toàn hệ thống.
5. **Khôi phục tài khoản:** hiện liên hệ Admin; tự phục vụ cần kênh email xác minh
   và token một lần. Không trả mật khẩu qua API.
6. **Frontend:** view dashboard và chi tiết lớn; nên tách dữ liệu, lọc, export và
   trình bày thành module nhỏ. Thư viện Excel vẫn khoảng 870 KB ở chunk riêng.
7. **UX dữ liệu:** cần kiểm tra với dữ liệu dài/thật, trạng thái rỗng, lỗi API,
   nhiều cơ quan, màn hình bảng rộng, thông báo lỗi 403 và thao tác chờ phê duyệt.
8. **Accessibility:** mới cải thiện shell và hộp xác nhận; các modal nghiệp vụ,
   date picker, searchable select và bảng vẫn cần kiểm tra bàn phím/screen reader.
9. **Cảnh báo build:** còn nullable và xung đột EF Core Relational 9.0.1/9.0.2.

## Kiểm chứng

- Frontend production build thành công; bundle đã tách theo view.
- Backend: 27 kiểm thử hiện có đạt.
- Edge headless: menu mobile, sidebar desktop, dialog bàn phím đạt; không có
  pageerror với API giả lập rỗng.
- Ảnh kiểm tra lưu ở scratch/ui-mobile-review.png và scratch/ui-desktop-review.png.

Các cải thiện đã nằm trong source local; không deploy và không sửa dữ liệu thật.
