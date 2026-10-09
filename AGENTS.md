# Quy tắc làm việc trong repository

Đọc [bộ rule](docs/rules/README.md) trước khi sửa source. Yêu cầu trực tiếp của người dùng được ưu tiên; ghi rõ ngoại lệ và lý do trong mô tả thay đổi.

- UI: [UI-DESIGN](docs/rules/UI-DESIGN.md), [CONTROLS](docs/rules/CONTROLS.md).
- BE/FE và hàm dùng chung: [ARCHITECTURE](docs/rules/ARCHITECTURE.md).
- Nghiệp vụ và hợp đồng API: [BUSINESS](docs/rules/BUSINESS.md).
- Thư viện, build và kiểm thử: [DEPENDENCIES](docs/rules/DEPENDENCIES.md).
- Các điểm chưa tuân thủ và thứ tự xử lý: [SOURCE-AUDIT](docs/rules/SOURCE-AUDIT.md).

Không tạo thêm bản sao của logic đã có. Tìm hàm/component hiện hữu trước; dùng adapter nếu DTO khác hình dạng. Không gom hai workflow chỉ vì cùng tên: phải cùng đầu vào, kết quả và quy tắc nghiệp vụ. BE là nguồn tính tiến độ và phân quyền; FE chỉ trình bày kết quả, kiểm tra sơ bộ và cung cấp trải nghiệm tương tác.

Không ghi mật khẩu, connection string production, token hoặc dữ liệu tài khoản vào tài liệu/log. Thay đổi schema dùng migration có phiên bản. Restore/migration phải đối chiếu dữ liệu; không reset quyết định phê duyệt khi khởi động.

Chỉ chạy kiểm thử liên quan: thay đổi nghiệp vụ dùng unit test và PostgreSQL cho phần phụ thuộc DB; sửa UI kiểm tra trình duyệt ở desktop/mobile và build. Không yêu cầu chạy mọi test cho chỉnh tài liệu thuần túy. Không coi file rule là bằng chứng source đã tuân thủ.
