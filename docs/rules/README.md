# Bộ rule hệ thống theo dõi chiến lược

Ngày rà soát: 09/10/2026. Phạm vi: source ứng dụng `backend`, `frontend/src`, cấu hình dependency, migration, script và bộ kiểm thử; không đánh giá code sinh tự động, build output hoặc node_modules. Đây là chuẩn cho code mới và phần code được sửa; source legacy còn lệch được ghi riêng trong SOURCE-AUDIT.

| File | Nội dung |
| --- | --- |
| [UI-DESIGN.md](UI-DESIGN.md) | Font, cỡ chữ theo vai trò, màu, khoảng cách, bố cục |
| [CONTROLS.md](CONTROLS.md) | Button, input, select, date picker, modal, bảng và trạng thái dữ liệu |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Ranh giới BE/FE, nguồn logic chung, quy tắc tách module |
| [BUSINESS.md](BUSINESS.md) | Báo cáo năm, baseline, tiến độ, phê duyệt, ngày và API |
| [DEPENDENCIES.md](DEPENDENCIES.md) | Thư viện được sử dụng, adapter và cách kiểm chứng thay đổi |
| [SOURCE-AUDIT.md](SOURCE-AUDIT.md) | Phát hiện thực tế, việc cần gom chung, lộ trình áp dụng |

## Cách áp dụng

1. Xác định chức năng thuộc nghiệp vụ nào; đọc rule tương ứng và tìm nguồn logic chung.
2. Nếu logic đã có, import/gọi lại. Nếu chỉ khác hình dạng DTO, chuẩn hóa ở adapter thay vì copy công thức.
3. Nếu chưa có, tạo module chung khi có ít nhất hai nơi cùng quy tắc; chọn một chủ sở hữu, đầu vào/đầu ra rõ ràng.
4. Chuyển các caller liên quan và xóa bản sao đã được thay thế trong cùng đợt. Có test chứng minh kết quả giữa caller thống nhất nếu là nghiệp vụ.
5. Cập nhật bản đồ nguồn logic và SOURCE-AUDIT khi đóng được một điểm lệch.

Không áp dụng hàng loạt bằng tìm/thay thế cỡ chữ hoặc công thức. Các rule ghi **hiện có** là đường dẫn đã tồn tại; mục **đề xuất** chưa được triển khai. `npm run check:rules --prefix frontend` kiểm tra hàm formatter/status không bị định nghĩa lại và view không import Excel eager; không thay thế review tất cả rule.

## Tiêu chí review

- Một quy tắc chỉ có một nguồn thực thi trong cùng runtime; BE/FE chia sẻ hợp đồng, không sao chép tính toán chính thức.
- Cùng trạng thái phải có cùng tên và màu trên bảng, dashboard, modal và export.
- UI dùng thang chữ và control thống nhất; đủ trạng thái loading, empty, error, disabled.
- Không đổi nghiệp vụ để làm UI thuận tiện; không làm mất ghi chú, minh chứng, báo cáo hoặc quyền truy cập.
- Mọi ngoại lệ có lý do cụ thể, phạm vi và cách kiểm tra.
