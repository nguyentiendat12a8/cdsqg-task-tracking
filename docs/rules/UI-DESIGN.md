# Rule font và thiết kế

## Font

Giữ font hiện hành: `"Helvetica Neue", Helvetica, Arial, sans-serif`. Khai báo tại `frontend/src/style.css`, không khai báo font-family trong component. Không thêm webfont/CDN cho từng màn hình. `font-mono` chỉ dành cho đoạn code/chuỗi kỹ thuật; không dùng cho tên cơ quan, điện thoại hoặc nội dung nghiệp vụ. Font đã khai báo qua token trong style.css; không thêm override !important theo component.

## Thang chữ chuẩn (root 16px, dùng rem)

| Vai trò | Cỡ / line-height | Weight | Utility Tailwind |
| --- | --- | --- | --- |
| Tiêu đề trang | 24 / 32px | 700 | text-2xl font-bold |
| Tiêu đề trang trên mobile | 20 / 28px | 700 | text-xl font-bold |
| Tiêu đề modal / khối lớn | 20 / 28px | 700 | text-xl font-bold |
| Tiêu đề card / nhóm | 16 / 24px | 600 | text-base font-semibold |
| Nội dung, input, nút, ô bảng | 14 / 20px | 400; nút 600 | text-sm |
| Label form | 14 / 20px | 600 | text-sm font-semibold |
| Header bảng, badge, chú thích | 12 / 16px | 500–600 | text-xs |
| Số KPI chính | 28 / 36px | 700 | text-[1.75rem] leading-9 font-bold |
| Lỗi / thông báo quan trọng | 14 / 20px | 500 | text-sm font-medium |

Không tạo thêm cỡ 9, 10, 11, 13px trong UI mới. Chiều cao control một dòng thống nhất 36px, token --ui-control-height: 2.25rem. Chữ 12px chỉ là thông tin phụ, không dùng làm nội dung bảng chính hoặc form dài. Text trong input trên mobile dùng 16px (`text-base sm:text-sm`) để tránh trình duyệt tự zoom. Không thu nhỏ chữ để ép bảng rộng vào màn hình. Không đổi root font-size theo viewport. Zoom 200% vẫn phải đọc và thao tác được.

## Màu và phân cấp

Nền trang slate-50; card trắng; nội dung chính slate-900; nội dung phụ slate-600; viền slate-200. Blue-600/700 cho hành động chính, rose-600/700 cho thao tác phá hủy/lỗi. Chỉ một hành động chính trong một nhóm thao tác. Không dùng text-slate-400 cho thông tin bắt buộc phải đọc; reserve cho placeholder/decorative và kiểm tra contrast thực tế.

Chuẩn trạng thái thực thi: NotStarted slate; InProgressOnTime emerald; InProgressOverdue rose; ExpiringSoon amber; CompletedOnTime blue; CompletedOverdue orange. Chuẩn phê duyệt: Pending amber; Approved emerald; Rejected rose. Cảnh báo nghiệp vụ Green/Yellow/Red tách biệt màu trạng thái. Luôn kèm nhãn chữ, không chỉ dùng màu/icon. Đây là chuẩn mới; mapping legacy còn lệch được ghi trong SOURCE-AUDIT.

## Khoảng cách và bề mặt

Dùng bước 4px: 4/8/12/16/24/32. Nhãn cách control 6–8px; field cách field 16px; nhóm cách nhóm 24px. Card/modal bo 16px; control bo 8px; badge bo tròn. Padding khối desktop 24px, mobile 16px; thanh hành động 16–24px. Shadow nhẹ cho card, shadow lớn cho modal; tránh gradient và nhiều viền lồng nhau không có mục đích.

Tiêu đề/mô tả dài phải wrap; mã/số/đơn vị có thể nowrap. Không truncate nội dung mà không có cách xem đầy đủ bằng bàn phím. Responsive giữ chức năng, ưu tiên wrap/scroll khu vực bảng thay vì gây cuộn ngang cả trang.


