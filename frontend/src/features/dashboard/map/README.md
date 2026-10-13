# Bản đồ địa phương trên dashboard

Nguồn: https://github.com/nguyenduy1133/Free-GIS-Data, tệp Post-2025/Provinces.geojson, tải 13/10/2026. Nguồn cho phép dùng công khai miễn phí, yêu cầu ghi nguồn; liên kết được hiển thị dưới bản đồ. Dữ liệu nguồn có đúng 34 feature, gồm các đảo. Asset provinces.json được chiếu và giảm số điểm để phục vụ dashboard, không phải bản đồ địa giới pháp lý. Không dùng tệp 63 tỉnh. Không tự gộp số liệu cơ quan mang tên tỉnh cũ sang tỉnh mới.

BE AgencyStatusSummaryDto trả GoalCompletionRate, TaskCompletionRate và CompletionRateOverall: số mục hoàn thành đúng/quá hạn chia số được giao, làm tròn 2 chữ số. Đây là tỷ lệ số mục hoàn thành, khác phần trăm tiến độ của một mục. Không có mục được giao trả null. FE chỉ nhận tỷ lệ, tô màu và ghép tên địa phương hiện tại; thiếu dữ liệu tỷ lệ hiển thị riêng, không bịa 0%.

Bản đồ lazy load, dữ liệu đóng gói local, không gọi dịch vụ bản đồ khi người dùng mở trang. Chọn tỉnh bằng chuột, bàn phím hoặc danh sách tìm kiếm; nút chi tiết dùng workflow drilldown hiện có. Bộ lọc và mục tiêu/nhiệm vụ được giữ theo dashboard.

Đã sửa nhãn bị sao chép sai của feature Ma=31 thành Đồng Tháp (đường biên nằm tại 105.18–106.82°E, 10.13–10.97°N), chuẩn hóa tên Hồ Chí Minh; kiểm tra đủ 34 tên duy nhất theo https://baochinhphu.vn/nghi-quyet-cua-quoc-hoi-ve-sap-xep-don-vi-hanh-chinh-cap-tinh-102250612191145158.htm. Không sử dụng các thuộc tính dân số/diện tích từ nguồn.
