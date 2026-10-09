# Rule control và trạng thái tương tác

## Component hiện có cần tái sử dụng

| Control | Nguồn hiện có | Quy tắc |
| --- | --- | --- |
| Select tìm kiếm, multi-select | `frontend/src/components/SearchableSelect.vue` | options `{value,label}`; giá trị đơn null, nhiều là mảng; không copy dropdown |
| Ngày | `frontend/src/components/DatePicker.vue` | ngày lịch `YYYY-MM-DD`; không tự đổi UTC |
| Xác nhận | `frontend/src/components/ConfirmModal.vue`, `services/confirm.js` | dùng cho thao tác phá hủy; không window.confirm |
| Modal | `directives/accessibleDialog.js` | focus trap, restore focus, Esc; cần kiểm tra hành vi với dropdown teleport |
| Popover | `components/OverlayPanel.vue` | dùng chung thay vì tự viết positioning trong view |
| Loading | `components/LoadingSpinner.vue` | kèm mô tả hành động, không spinner đơn độc |
| Lỗi API chung | `components/ApiFeedback.vue`, `services/auth.js` | 401 logout; 403 giải thích quyền; lỗi form giữ dữ liệu |
| Minh chứng / file | `utils/fileViewer.js` | giữ link tải/xem; không phát sinh URL xử lý riêng |

## Button và input

Control một dòng cao 36px trên desktop/mobile theo yêu cầu người dùng. Textarea, card/tab nội dung nhiều dòng và icon độc lập không ép thành input 36px. Icon 16–20px; icon-only phải có aria-label. Button trong form có `type="button"` trừ submit. Primary xanh, secondary trắng viền slate-200, danger rose; disabled có style và chặn thực sự event.

Input có label liên kết `for/id`, help/error qua aria-describedby; required biểu thị bằng chữ hoặc dấu sao có giải thích. Number dùng `step` theo nghiệp vụ, hiển thị đơn vị ở suffix; kiểm tra min/max ở BE, FE chỉ hỗ trợ nhập. Không dùng `value || fallback` vì 0 là giá trị hợp lệ. Validation lỗi đặt gần field, submit lỗi đặt trong form bằng role=alert, không chỉ toast. Placeholder không thay thế label.

Select phải hỗ trợ Tab, Enter/Space, Arrow, Esc, active option và aria-expanded/aria-controls; lựa chọn hiện tại đọc được bằng screen reader. Date picker cần keyboard chọn ngày/tháng, trạng thái ngày chọn/hôm nay, focus không bị mất khi đổi tháng. Các component hiện hữu chưa được coi là đạt toàn bộ chỉ vì đã có directive.

## Modal

Header: tiêu đề 20px, mã/context 12px, mô tả 14px. Body scroll riêng; footer luôn thấy; max-height 90dvh với fallback 90vh. Width nhỏ 448px, thường 672px, bảng rộng tối đa 1024px, luôn giới hạn theo viewport. Padding 16px mobile /24px desktop. Có tên accessible qua aria-labelledby và nút đóng. Tránh role=dialog lồng nếu directive đã thiết lập.

Esc/backdrop chỉ đóng khi không đang lưu. Khi lưu: khóa submit để chống gửi lặp; giữ dữ liệu và không báo thành công trước response. Thành công mới emit saved và tải lại dữ liệu từ server. Đóng modal trả focus về control đã mở nó. Dropdown teleport phải nằm trong phạm vi focus được phép.

## Bảng và bộ lọc

Header 12px, cell 14px; số canh phải/tabular-nums, nội dung canh trái. Header sticky trong vùng scroll riêng; màn hình hẹp có scroll ngang hoặc card giữ đầy đủ thao tác. Hành động icon có tên. Sort biểu thị aria-sort. Phân trang dùng totalCount của server, không số item sau lọc trang hiện tại.

Filter draft tách applied; Apply/reset về trang 1. Server lọc và phân trang: FE không lọc lại dữ liệu của một trang theo quy tắc khác. Export ghi rõ phạm vi: trang hiện tại hoặc toàn bộ kết quả đã lọc; toàn bộ phải truy vấn đủ trang. Loading khóa export, response cũ không ghi đè response mới.

## Trạng thái dữ liệu bắt buộc

Loading lần đầu; refreshing không giật bố cục; empty chưa có dữ liệu; empty do filter kèm reset; lỗi API kèm retry; 403 thông báo quyền; pending approval nói rõ kết quả chính thức chưa đổi; submitting khóa thao tác lặp. Test tên cơ quan/nhiệm vụ dài, số lớn, null/0, nhiều lựa chọn, zoom 200%, 390px và desktop. Không dùng tooltip làm cách duy nhất để xem lỗi hay nội dung bắt buộc.

