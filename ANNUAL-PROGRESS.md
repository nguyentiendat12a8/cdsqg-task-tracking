# Báo cáo năm và quy tắc tính tiến độ

Hệ thống chỉ nhận báo cáo theo năm (`PeriodYear`). Không còn trường tháng/quý hay cấu trúc mục tiêu/nhiệm vụ con. Phân cấp **cơ quan** và danh sách **sản phẩm đầu ra** vẫn được sử dụng.

## Bộ tính chung

`backend/Application/Services/ProgressCalculator.cs` là nguồn quy tắc nghiệp vụ:

- `ResolveBaseline`: chỉ tiêu tùy chỉnh `CustomBaseline["YYYY"]` → chỉ tiêu `TargetBaseline.Year` → mặc định 100 cho đơn vị `%`. Đơn vị số lượng thiếu chỉ tiêu trả về `null`, không giả định 100.
- `LatestApproved`: lấy báo cáo đã duyệt, đúng cơ quan; chọn năm báo cáo mới nhất rồi bản sửa mới nhất của năm đó. Báo cáo chờ duyệt/bị từ chối không thay đổi tiến độ chính thức.
- `Evaluate`: tính giá trị, tỷ lệ, trạng thái và cảnh báo cùng lúc. Tỷ lệ định lượng = giá trị / chỉ tiêu của đúng năm × 100. Hoàn thành khi giá trị thực tế đạt chỉ tiêu, không dùng số phần trăm đã làm tròn để quyết định.
- `EvaluateOverall`: với mục chung, lấy trung bình tỷ lệ của các bộ/tỉnh thuộc phạm vi giao việc trong cùng năm; cơ quan chưa báo cáo đóng góp 0%. Chỉ hoàn thành khi tất cả cơ quan thuộc phạm vi đã hoàn thành. Không cộng lẫn giá trị giữa cơ quan.
- `RefreshCachesAsync`: cập nhật cache phần trăm/cảnh báo trong log và tiến độ cơ quan sau khi báo cáo, phê duyệt, từ chối hoặc chỉnh sửa chỉ tiêu. Các màn hình đọc kết quả tính lại, không lấy cache làm quy tắc nghiệp vụ.

### Cách nhập số liệu

Giá trị nhập là **tổng kết quả đã đạt đến năm báo cáo**, dùng trực tiếp cho cả `LatestValue` và mã legacy `Cumulative`. Không cộng thêm các năm trước. Ví dụ năm 2026 đạt 15, đến năm 2027 đạt tổng 35: báo cáo 2027 nhập 35; chỉ tiêu 2027 là 50 thì tiến độ 70%. Bản sửa đã duyệt thay thế tổng của cùng năm và cơ quan. Sửa năm trước không thay đổi tổng đã báo cáo của năm sau.

Mục cụ thể mặc định dùng cơ quan được giao, nếu có; nếu không, dùng cơ quan chủ trì. Có thể truyền `agencyId` để xem từng cơ quan. Báo cáo cũ không có `AgencyId` chỉ được đối chiếu với cơ quan chủ trì của mục cụ thể.

### Định tính và thời hạn

Sản phẩm: chưa thực hiện 0%, đang soạn 25%, thẩm định 60%, đã trình 85%, hoàn thành 100%. Tiến độ là trung bình các sản phẩm; phải hoàn thành tất cả sản phẩm mới hoàn thành nhiệm vụ. Nếu không có sản phẩm, dùng trạng thái định tính của báo cáo: 0/25/60/100%.

Mục chưa hoàn thành và đã quá hạn được tính quá hạn kể cả khi chưa bắt đầu. Mục đã bắt đầu còn tối đa 30 ngày được tính sắp hết hạn. Thời điểm báo cáo đạt chỉ tiêu (`LogDate`) được dùng để phân biệt hoàn thành đúng/quá hạn. Với mục thường xuyên, thời hạn đánh giá là 31/12 của năm báo cáo.

Chỉnh sửa chỉ tiêu trong bảng năm thay thế chỉ tiêu tùy chỉnh của chính năm đó. Xóa chỉ tiêu tùy chỉnh trong hộp thiết lập khôi phục việc dùng chỉ tiêu năm thông thường. Sau khi lưu, giao diện tải lại tiến độ/trạng thái.

## Migration và dữ liệu

`20261008164528_AnnualReportingOnly` xóa mục con và bản ghi liên quan, xóa baseline/báo cáo có quý khác 0, chỉ giữ khóa baseline `YYYY`, bỏ cột tháng/quý/phân cấp nhiệm vụ và tạo khóa duy nhất `(GoalTaskId, Year)`. Schema legacy thiếu khóa/index tự tham chiếu được hỗ trợ. Migration chạy trong transaction; lỗi sẽ rollback và backend dừng khởi động.

Báo cáo cũ có `PeriodQuarter = 0` được giữ dưới dạng báo cáo năm. Schema cũ không lưu tháng riêng nên không thể nhận diện một báo cáo tháng từng bị ghi với quý 0 chỉ từ dữ liệu này. Database local đang dùng không có cấu hình tháng/quý hoặc mục con trước khi nâng cấp; 5 mục chính và 4 báo cáo được giữ lại.

Các file minh chứng vật lý không bị xóa. Cache tiến độ được vô hiệu hóa khi nâng cấp; bản ghi báo cáo năm và minh chứng trong log được giữ lại. `Down` chỉ phục hồi cấu trúc cột, **không phục hồi dữ liệu đã xóa**. Muốn khôi phục dữ liệu phải restore backup.

Khởi động ứng dụng không tự chuyển báo cáo đã duyệt về chờ duyệt. Các quyết định phê duyệt được giữ nguyên.

Backup local ngay trước khi áp dụng:
`C:\Users\84878\Documents\ChatGPT\CDSQG\scratch\cdsqg_before_annual_apply_20261008_235824.dump`.

Migration đã áp dụng cho PostgreSQL local `localhost:5433/cdsqg_db`. Không áp dụng thay đổi lên Neon.

## Kiểm thử

- `backend.Tests/ProgressCalculatorTests.cs`: baseline đúng năm, tùy chỉnh, cộng dồn, bản sửa, cách ly cơ quan, thiếu chỉ tiêu, ngưỡng làm tròn, sản phẩm, hạn và mục chung.
- `backend.Tests/PostgresIntegrationTests.cs`: nâng cấp schema chuẩn/legacy trên PostgreSQL thật, xóa mục con nhiều cấp và báo cáo quý, giữ báo cáo năm/phân cấp cơ quan, cập nhật cache.
- `backend.Tests/SecurityTests.cs`: từ chối trường tháng/quý và yêu cầu tạo mục con.
- `frontend/tests/annual.browser.mjs`: biểu mẫu baseline, truy vấn báo cáo năm, multipart payload và không có điều khiển tháng/quý.
- `frontend/tests/refactor.browser.mjs`: tạo/sửa/lọc/dashboard và export Excel.

Chạy kiểm thử PostgreSQL với `CDSQG_TEST_POSTGRES` trỏ đến database dành cho test. Test chỉ tạo/xóa schema ngẫu nhiên riêng.

