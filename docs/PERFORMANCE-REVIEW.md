# Rà soát hiệu năng — 13/10/2026

Phạm vi: tải trang và module FE, request dashboard/bộ lọc, biểu đồ/bản đồ, Excel, truy vấn danh mục/thống kê, planning, execution và thông báo. Rà soát source và kiểm thử local; chưa đo tải đồng thời hay p95 trên môi trường triển khai.

## Đã tối ưu

- Thống kê QPPL tạo lookup theo ID cơ quan một lần trong request. Thay việc mỗi cơ quan quét mọi văn bản bằng duyệt các văn bản liên quan. Một văn bản có cùng cơ quan ban hành và dự thảo chỉ xuất hiện một lần trong lookup; vẫn giữ thứ tự và các bộ lọc trước thống kê.
- Thống kê QPPL cấp Admin/cấp 2 chỉ SELECT ID, ID cơ quan, loại và lĩnh vực. Không tải trích yếu dài, ghi chú và JSON tệp khi chỉ cần đếm. Cấp 3 giữ dữ liệu đầy đủ cho widget văn bản mới nhất.
- GET danh sách/thống kê QPPL, danh mục cơ quan và danh sách thông báo không đưa entity vào ChangeTracker. Identity resolution giữ các tham chiếu cơ quan cha dùng chung trong danh mục. Các workflow ghi dữ liệu vẫn dùng tracking.
- Dashboard và thống kê QPPL hủy nhóm request cũ khi tải bộ lọc mới hoặc unmount. Giữ version guard cho kết quả/loading, tránh response cũ thay response mới. Không thêm cache xuyên người dùng/năm.

## Điểm đang có và giữ nguyên

- View, modal lớn, bản đồ và engine Excel lazy load. Excel reader/writer được tách để giữ tương thích XLS cũ, style, merge và dropdown của XLSX.
- Dashboard BE đã có AsSplitQuery cho collection includes. Không thay thuật toán ProgressCalculator, duyệt/submit/import, baseline hoặc quy tắc hoàn thành qua năm.
- Export tải đầy đủ trang, kiểm tra thiếu/trùng dữ liệu. Không tăng tốc bằng bỏ trang hay bỏ kiểm tra.

## Cần đo trước khi thay đổi tiếp

- Planning grid còn tải collection baseline/log/execution trước lọc/phân trang. Cần đo SQL và số dòng thực, rồi kiểm tra snapshot nhất quán khi tách truy vấn; không cắt lịch sử vì cần xác định hoàn thành và báo cáo năm.
- Chunk Excel khoảng 870 KB, bản đồ khoảng 504 KB trước gzip. Có thể nghiên cứu worker và tối giản asset; cần hồi quy Excel native/tiếng Việt/validation, địa giới và hai quần đảo trước khi thay thư viện hoặc hình học.
- Thông báo polling 5 giây. Thay bằng visibility scheduling/SSE cần kiểm tra độ trễ, reconnect, quyền và cập nhật sau thao tác; hiện giữ tần suất.
- Index DB/caching/response compression: cần EXPLAIN ANALYZE, tải thực tế và hợp đồng invalidation trước khi migration/cấu hình; không thêm index hoặc cache theo phỏng đoán.

## Kiểm chứng

- PostgreSQL: LegalStatsPostgresTests kiểm tra số liệu, issuer/drafter trùng, filter loại/lĩnh vực, cấp 2/cấp 3, ghi chú/tệp và ChangeTracker trống; schema test riêng được xóa sau test.
- Hồi quy BE: AnnualDashboardPostgresTests, ProgressCalculatorTests, DocumentQueryTests; kiểm tra thêm SecurityTests/ProvinceCatalogTests khi chỉnh GET danh mục.
- FE: performance-requests.browser kiểm tra request bị thay thế/unmount bị hủy, kết quả năm/bộ lọc mới nhất và loading. report-year.browser kiểm tra năm, export và control mobile. API/fetch trong browser test được mock.
- Build và check:rules. Không tuyên bố mức tăng tốc phần trăm vì chưa benchmark end-to-end với dữ liệu/tải production.

Kết quả: 37 test nghiệp vụ/query/PG ban đầu qua; mở rộng kiểm tra quyền/danh mục có 59 test qua và 1 lỗi ở ProvinceCatalogTests, dòng 48. Test yêu cầu seed giữ nguyên tên legacy `hÀ nỘi`, nhưng ProvinceCatalog.Seed hiện đã chuẩn hóa thành `Thành phố Hà Nội`. Assertion này chạy trước controller và không đi qua GET được tối ưu. Không sửa seed/dữ liệu hay thay kỳ vọng test trong đợt hiệu năng; cần thống nhất riêng quy tắc bảo toàn tên legacy. Browser cancellation và report-year, check:rules và build đều qua.
