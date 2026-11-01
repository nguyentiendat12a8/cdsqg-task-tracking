# Bản đồ địa phương trên dashboard

Nguồn: https://github.com/nguyenduy1133/Free-GIS-Data, tệp Post-2025/Provinces.geojson, tải 13/10/2026. Nguồn cho phép dùng công khai miễn phí, yêu cầu ghi nguồn; liên kết được hiển thị dưới bản đồ. Dữ liệu nguồn có đúng 34 feature, gồm các đảo. Asset provinces.json được chiếu và giảm số điểm để phục vụ dashboard, không phải bản đồ địa giới pháp lý. Không dùng tệp 63 tỉnh. Không tự gộp số liệu cơ quan mang tên tỉnh cũ sang tỉnh mới.

BE AgencyStatusSummaryDto trả GoalCompletionRate, TaskCompletionRate và CompletionRateOverall: số mục hoàn thành đúng/quá hạn chia số được giao, làm tròn 2 chữ số. Đây là tỷ lệ số mục hoàn thành, khác phần trăm tiến độ của một mục. Không có mục được giao trả null. FE chỉ nhận tỷ lệ, tô màu và ghép tên địa phương hiện tại; thiếu dữ liệu tỷ lệ hiển thị riêng, không bịa 0%.

Bản đồ lazy load, dữ liệu đóng gói local, không gọi dịch vụ bản đồ khi người dùng mở trang. Chọn tỉnh bằng chuột, bàn phím hoặc danh sách tìm kiếm; nút chi tiết dùng workflow drilldown hiện có. Bộ lọc và mục tiêu/nhiệm vụ được giữ theo dashboard.

Đã sửa nhãn bị sao chép sai của feature Ma=31 thành Đồng Tháp (đường biên nằm tại 105.18–106.82°E, 10.13–10.97°N), chuẩn hóa tên Hồ Chí Minh; kiểm tra đủ 34 tên duy nhất theo https://baochinhphu.vn/nghi-quyet-cua-quoc-hoi-ve-sap-xep-don-vi-hanh-chinh-cap-tinh-102250612191145158.htm. Không sử dụng các thuộc tính dân số/diện tích từ nguồn.

01/11/2026 — Tối giản hình học để giảm dung lượng tải: bỏ các điểm trùng liên tiếp và đơn giản hóa Douglas–Peucker với dung sai 0,2 đơn vị viewBox (dưới 0,5 px khi phóng to tối đa 4 lần). Bỏ 251 vòng suy biến dưới 3 điểm phân biệt (không có diện tích, không hiển thị). Tệp giảm từ khoảng 490 KB xuống 146 KB (gzip 70 KB → 48 KB); giữ nguyên viewBox, mã/tên 34 tỉnh và thứ tự feature. Đã đối chiếu ảnh render trước/sau ở cỡ gốc và phóng 6 lần.
