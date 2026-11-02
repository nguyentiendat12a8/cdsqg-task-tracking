# Sửa cơ quan chủ trì của mục tiêu 2629 đã nạp bằng seed cũ

Lỗi được xác nhận trên local ngày 02/11/2026: 14/22 mục tiêu có tên hiển thị một cơ quan, nhưng LeadAgencyId trỏ ALL_AGENCIES và có 51 đơn vị được giao. Seed hiện tại đã ánh xạ đúng; không chạy lại seed lên database có báo cáo.

File `repair_2629_goal_lead_assignments.sql` là PostgreSQL thuần, dùng được trong pgAdmin hoặc psql trên local/production. Script không chứa thông tin kết nối. Chạy toàn bộ file trong một phiên mới, ngoài transaction đang mở.

1. Giữ `INSERT INTO _r2629_options VALUES (false);` để xem trước. Kiểm tra các dòng REPAIR và cơ quan expected_lead; SKIP là thay đổi riêng được giữ nguyên.
2. Lưu bản sao lưu database trước khi áp dụng production. Đổi duy nhất dòng đó thành `VALUES (true);`, rồi chạy lại toàn bộ file. Script tự kiểm tra và COMMIT khi hợp lệ; nếu báo lỗi thì chạy ROLLBACK và xử lý nguyên nhân, không chạy lẻ phần UPDATE.
3. Để kiểm tra lại, chạy ở chế độ false: các mục đã sửa phải hiện ALREADY_CORRECT.

Có thể chạy từ CMD:

```
psql -h HOST -p PORT -U USER -d DATABASE -W -v ON_ERROR_STOP=1 -f "scripts/repair_2629_goal_lead_assignments.sql"
```

Phạm vi chỉ gồm 22 mục tiêu seed của văn bản 2629 (hỗ trợ mã MT-xx và MT-2629-xx). Xác nhận danh mục cơ quan duy nhất/đang hoạt động, đủ 22 mục tiêu, đúng tên gốc, không có cơ quan chủ trì thứ hai. Dừng khi thiếu/không rõ danh mục hoặc thiếu bản ghi thực hiện của cơ quan đúng. Bỏ qua cơ quan đã được đổi thủ công.

Chỉ sửa LeadAgencyId, IsGeneralTask và hai cờ vai trò chủ trì trên AgencyTaskExecutions. Không xóa bản ghi phân giao; các đơn vị bị giao nhầm chỉ ngừng vai trò chủ trì. Giữ vai trò phối hợp, kế hoạch năm, kết quả, báo cáo, tệp, trạng thái phê duyệt và lịch sử. Báo cáo đã gửi dưới đơn vị cũ vẫn thuộc đơn vị đó, không tự chuyển cho đơn vị đúng. Script so sánh dữ liệu trước/sau trong transaction và dừng nếu có thay đổi ngoài phạm vi. Không sửa nhiệm vụ NV hoặc văn bản 1266.

Script không tự áp dụng lên production. Không chạy lại seed vì seed có thể xóa/tạo lại dữ liệu.
