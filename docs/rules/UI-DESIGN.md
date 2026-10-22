# Rule font và thiết kế

## Font

Giữ font hiện hành: `"Helvetica Neue", Helvetica, Arial, sans-serif`. Khai báo tại `frontend/src/style.css`, không khai báo font-family trong component. Không thêm webfont/CDN cho từng màn hình. `font-mono` chỉ dành cho đoạn code/chuỗi kỹ thuật; không dùng cho tên cơ quan, điện thoại hoặc nội dung nghiệp vụ. Font đã khai báo qua token trong style.css; không thêm override !important theo component.

## Thang chữ chuẩn (root 16px, dùng rem)

| Vai trò | Cỡ / line-height | Weight | Utility Tailwind |
| --- | --- | --- | --- |
| Tiêu đề trang (desktop và mobile) | 18 / 24px | 700 | ui-page-title |
| Tiêu đề modal / khối lớn | 18 / 24px | 700 | ui-title |
| Tiêu đề card / nhóm | 14 / 20px | 600 | h3/h4 hoặc ui-section-title |
| Nội dung, input, nút, ô bảng | 12 / 18px | 400; nút 600 | text-sm |
| Label form | 12 / 18px | 600 | text-sm font-semibold |
| Header bảng, badge, chú thích | 12 / 16px | 500–600 | text-xs |
| Số KPI chính | 28 / 36px | 700 | text-[1.75rem] leading-9 font-bold |
| Lỗi / thông báo quan trọng | 12 / 18px | 500 | text-sm font-medium |

Không tạo thêm cỡ 9, 10, 11, 13px trong UI mới. Chiều cao control một dòng thống nhất 36px, token --ui-control-height: 2.25rem. Theo yêu cầu người dùng 13/10/2026, chữ mặc định 12px cho cả nội dung chính, bảng, nút và form. Không dùng cỡ dưới 12px. Text trong input trên mobile dùng 16px (CSS theo vai trò input/textarea; không dựa vào text-base vì text-base đã về 12px) để tránh trình duyệt tự zoom. Không thu nhỏ chữ để ép bảng rộng vào màn hình. Không đổi root font-size theo viewport. Zoom 200% vẫn phải đọc và thao tác được.

## Màu và phân cấp

Nền trang slate-50; card trắng; nội dung chính slate-900; nội dung phụ slate-600; viền slate-200. Blue-600/700 cho hành động chính, rose-600/700 cho thao tác phá hủy/lỗi. Chỉ một hành động chính trong một nhóm thao tác. Không dùng text-slate-400 cho thông tin bắt buộc phải đọc; reserve cho placeholder/decorative và kiểm tra contrast thực tế.

Chuẩn trạng thái thực thi (22/10/2026): NotStarted — Chưa thực hiện (slate); InProgress — Đang thực hiện (blue); Completed — Đã hoàn thành (emerald). Cùng nhãn/màu trên bảng, biểu đồ, filter, lịch sử và Excel. Sắp hết hạn/quá hạn là cảnh báo thời hạn riêng, không chia thêm trạng thái. Chuẩn phê duyệt: Pending amber; Approved emerald; Rejected rose. Cảnh báo nghiệp vụ Green/Yellow/Red tách biệt màu trạng thái. Luôn kèm nhãn chữ, không chỉ dùng màu/icon.

## Khoảng cách và bề mặt

Dùng bước 4px: 4/8/12/16/24/32. Nhãn cách control 6–8px; field cách field 16px; nhóm cách nhóm 24px. Card/modal bo 16px; control bo 8px; badge bo tròn. Padding khối desktop 24px, mobile 16px; thanh hành động 16–24px. Shadow nhẹ cho card, shadow lớn cho modal; tránh gradient và nhiều viền lồng nhau không có mục đích.

Tiêu đề/mô tả dài phải wrap; mã/số/đơn vị có thể nowrap. Không truncate nội dung mà không có cách xem đầy đủ bằng bàn phím. Responsive giữ chức năng, ưu tiên wrap/scroll khu vực bảng thay vì gây cuộn ngang cả trang.



## Mật độ gọn (13/10/2026)

Root ứng dụng dùng `ui-density-compact`, quy định tập trung tại style.css. Giữ nguyên bố cục, body/form 12px, caption 12px, control 36px và số KPI 28px. Tiêu đề chung theo token 18px; heading trang/modal thống nhất 18px, nhóm 14px và thương hiệu 16px, không còn phụ thuộc class cỡ chữ legacy. Padding khối 20px →16px, 24px →20px; gap/space nhóm 16px →12px, 24px →20px. Lề nội dung 12px mobile/16px desktop. Không giảm root font-size, không ép chiều cao nội dung, không truncate tên hay thay đổi logic dữ liệu. Modal teleport ngoài root cần giữ chuẩn riêng hoặc mang class density nếu muốn áp dụng.

## Nguồn cỡ chữ (13/10/2026)

`--ui-font-body` và Tailwind `--text-sm`/`--text-base` cùng là 12px tại style.css; class legacy text-sm/text-xs/text-base không còn tạo ba cỡ chữ khác nhau cho nội dung. Heading trang/modal 18px, heading nhóm 14px, thương hiệu 16px, KPI 28px. Các control chuẩn dùng 12px/18px và weight 600. Input/textarea mobile 16px để tránh auto zoom; không đổi root 16px. Nhãn nhỏ 9–11px và 13px đã chuyển về text-xs (12px), không thêm arbitrary text size mới. Chữ trong tài liệu đính kèm giữ định dạng tài liệu, chỉ chuẩn hóa phần giao diện trình xem.

## Bảng màu toàn hệ thống (13/10/2026)

Token màu tại style.css: navy menu #18273B, primary #315EAD, nền trang #F5F6F8, card trắng, viền #E2E6EC, chữ chính #243247 và phụ #64748B. Tab mục tiêu và nhiệm vụ dùng cùng primary; bỏ bóng phát sáng menu. Các utility blue/purple dùng bảng màu primary chung; emerald/teal/rose/amber giữ ý nghĩa trạng thái với sắc độ dịu. Biểu đồ tiến độ đọc token CSS qua shared/themeColors.js, không tự định nghĩa màu trạng thái. Màu phân loại văn bản và mức hoàn thành bản đồ giữ bảng phân loại riêng.

22/10/2026 — Theo ảnh tham khảo người dùng, trang chủ có bề mặt pastel và màu nhấn theo khối: tổng số sky, trạng thái giữ slate/blue/emerald, thời hạn amber/rose, phân bổ violet, tiêu đề bảng/bản đồ blue. Phạm vi màu trang trí chỉ ở dashboard, không đổi palette toàn ứng dụng hoặc màu phân loại tỉnh. Giữ bố cục gọn và chiều cao control hiện có.

22/10/2026 — Theo yêu cầu tăng độ nổi bật, 8 thẻ KPI trang chủ dùng nền màu đậm sky/slate/blue/emerald và chữ trắng; ngoại lệ bề mặt card trắng/pastel chỉ trong KPI và header dashboard. Giữ nhãn trạng thái, màu bản đồ và palette toàn hệ thống.

22/10/2026 — Theo yêu cầu đồng bộ màu từ trang chủ ra toàn hệ thống: menu navy #213658, mục chọn #3b5fa8; primary blue theo token chung; header trang/khối/bảng/modal #f1f5fc, nền ứng dụng #f4f7fc, card trắng. Nguồn màu tại style.css; dùng ui-screen-heading/ui-section-heading thay cho màu cứng ở từng màn. Giữ nguyên màu nghiệp vụ của trạng thái, cảnh báo, bản đồ và phân loại văn bản. Quy định này mở rộng phạm vi palette dashboard trước đó theo yêu cầu mới của người dùng.

22/10/2026 — Icon thao tác dùng ui-action tại style.css: xem/sửa/cập nhật/giao/sao chép màu primary, menu thêm màu slate, thông báo/đặt mật khẩu màu amber, xóa màu rose. SVG kế thừa currentColor từ nút; nền/viền cùng tông nhạt, hover tăng nền, disabled dùng slate. Không thay màu nhãn trạng thái để biểu thị thao tác.
