# Sửa ánh xạ phân giao seed 2629

Seed cũ quy đổi tên cơ quan không nhận diện được thành `ALL_AGENCIES`, dẫn tới giao nhầm mục của Văn phòng Chính phủ, Bộ KH&CN… cho mọi bộ và địa phương.

Với cơ sở dữ liệu đã có dữ liệu, chạy backend bằng cấu hình kết nối của môi trường đó với tham số `--repair-2629-assignment-mapping`. Đây là lệnh bảo trì riêng, không mở HTTP và không chạy lại seed SQL. Không dùng seed SQL để sửa dữ liệu đang vận hành vì luồng nhập mới thay thế mục theo quyết định.

Lệnh chỉ xử lý quyết định có ID `26290000-0000-0000-0000-000000002629`, nhận diện ánh xạ seed cũ hoặc ánh xạ đích đã sửa; bỏ qua mục đã đổi chủ trì thủ công. Danh mục cơ quan còn thiếu được đối chiếu mã/tên trước khi thêm; gặp tên chưa xác định hoặc danh mục mơ hồ thì giao dịch rollback. Ngoại lệ so với lựa chọn cũ “không thêm cơ quan” của script: bổ sung đúng các cơ quan được nêu trong phụ lục để không phải giao thay cho tất cả cơ quan.

Phân giao dùng lại `AgencyAssignments.SyncAsync`, chỉ cập nhật vai trò chủ trì, giữ vai trò phối hợp, ID execution, số liệu, báo cáo, minh chứng và quyết định phê duyệt. Phân giao nhầm được tắt vai trò, không xóa lịch sử. Chạy lặp không tạo trùng. Không tự sửa snapshot hiện có khi khởi động hay đọc dữ liệu.

Đã kiểm tra bằng PostgreSQL: giao riêng, nhóm bộ/ngành không gồm tỉnh, giao hai cơ quan, bỏ qua phân công thủ công, giữ lịch sử và chạy lặp. Trên dữ liệu local ngày 23/10/2026, Bộ Công an nhận 8 mục tiêu, 46 nhiệm vụ của chương trình 2629 sau khi sửa; lịch sử báo cáo giữ nguyên.
