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

Nút xuất/nhập Excel và thêm mới dùng `components/ActionButton.vue` với kind `export`, `import`, `create`. Cùng chiều cao 36px, chữ 12px, bo 8px và icon 16px; thêm mới primary, nhập/xuất secondary. Giữ tên phạm vi dữ liệu trong nhãn; không tự khai báo icon/màu/padding ở caller. Component chuyển tiếp disabled, aria-busy, click và type submit đến button native.

Control một dòng cao 36px trên desktop/mobile theo yêu cầu người dùng. Textarea, card/tab nội dung nhiều dòng và icon độc lập không ép thành input 36px. Icon 16–20px; icon-only phải có aria-label. Button trong form có `type="button"` trừ submit. Primary xanh, secondary trắng viền slate-200, danger rose; disabled có style và chặn thực sự event.

Input có label liên kết `for/id`, help/error qua aria-describedby; required biểu thị bằng chữ hoặc dấu sao có giải thích. Number dùng `step` theo nghiệp vụ, hiển thị đơn vị ở suffix; kiểm tra min/max ở BE, FE chỉ hỗ trợ nhập. Không dùng `value || fallback` vì 0 là giá trị hợp lệ. Validation lỗi đặt gần field, submit lỗi đặt trong form bằng role=alert, không chỉ toast. Placeholder không thay thế label.

Select phải hỗ trợ Tab, Enter/Space, Arrow, Esc, active option và aria-expanded/aria-controls; lựa chọn hiện tại đọc được bằng screen reader. Date picker cần keyboard chọn ngày/tháng, trạng thái ngày chọn/hôm nay, focus không bị mất khi đổi tháng. Các component hiện hữu chưa được coi là đạt toàn bộ chỉ vì đã có directive.

## Modal

Header: tiêu đề 18px, mã/context 12px, mô tả 12px. Body scroll riêng; footer luôn thấy; max-height 90dvh với fallback 90vh. Width nhỏ 448px, thường 672px, bảng rộng tối đa 1024px, luôn giới hạn theo viewport. Padding 16px mobile /24px desktop. Có tên accessible qua aria-labelledby và nút đóng. Tránh role=dialog lồng nếu directive đã thiết lập.

Esc/backdrop chỉ đóng khi không đang lưu. Khi lưu: khóa submit để chống gửi lặp; giữ dữ liệu và không báo thành công trước response. Thành công mới emit saved và tải lại dữ liệu từ server. Đóng modal trả focus về control đã mở nó. Dropdown teleport phải nằm trong phạm vi focus được phép.

## Bảng và bộ lọc

Header 12px, cell 12px; số canh phải/tabular-nums, nội dung canh trái. Header sticky trong vùng scroll riêng; màn hình hẹp có scroll ngang hoặc card giữ đầy đủ thao tác. Hành động icon có tên. Sort biểu thị aria-sort. Phân trang dùng totalCount của server, không số item sau lọc trang hiện tại.

Filter draft tách applied; Apply/reset về trang 1. Server lọc và phân trang: FE không lọc lại dữ liệu của một trang theo quy tắc khác. Export ghi rõ phạm vi: trang hiện tại hoặc toàn bộ kết quả đã lọc; toàn bộ phải truy vấn đủ trang. Loading khóa export, response cũ không ghi đè response mới.

## Trạng thái dữ liệu bắt buộc

Loading lần đầu; refreshing không giật bố cục; empty chưa có dữ liệu; empty do filter kèm reset; lỗi API kèm retry; 403 thông báo quyền; pending approval nói rõ kết quả chính thức chưa đổi; submitting khóa thao tác lặp. Test tên cơ quan/nhiệm vụ dài, số lớn, null/0, nhiều lựa chọn, zoom 200%, 390px và desktop. Không dùng tooltip làm cách duy nhất để xem lỗi hay nội dung bắt buộc.


## Bố cục Excel xuất ra

Tiêu đề, thông tin đơn vị/ngày xuất, hướng dẫn và tên khối thống kê phải gộp trên các cột trống của dòng tương ứng. Dùng `fitPresentationRows` qua `styleWorksheet`; không gộp các ô dữ liệu có giá trị hoặc ô tiêu đề bảng. Dòng chỉ số dạng nhãn/giá trị được chia thành hai vùng gộp, giữ nguyên cả số 0. Bật wrap và chiều cao phù hợp cho nội dung dài. Bảng danh bạ/đầu mối chỉ có dữ liệu ở từng cột không cần gộp.

Mẫu báo cáo tiến độ có 8 cột: STT, Mã, Tên, Cơ quan chủ trì, Đơn vị tính, Tiến độ hiện tại (kết quả thực tế), Tiến độ thực hiện mới, Ghi chú. Cột G/H để nhập; năm báo cáo chọn khi import. Parser nhận diện theo tên cột, không theo vị trí cố định, giữ tương thích mẫu cũ. Số nhập là tổng đã đạt đến năm báo cáo; tỷ lệ hoàn thành chỉ tiêu là số dẫn xuất riêng.

## Định dạng số lượng Việt Nam (13/10/2026)

Số lượng hiển thị dùng `formatNumber` / `formatMeasuredValue`: dấu chấm hàng nghìn, dấu phẩy thập phân (20.000,5). Ô nhập chỉ tiêu/kết quả thực tế dùng `QuantityInput`, nhận định dạng Việt Nam và format khi blur; không làm tròn giá trị nhập. Payload API là number hoặc chuỗi số invariant theo DTO, không gửi dấu phân nhóm. Import Excel giữ ô số native; ô text định lượng được parse bằng `parseQuantityInput`. Năm, mã, số văn bản, số điện thoại và ngày không phải số lượng, không thêm dấu hàng nghìn. Biểu đồ dùng locale vi-VN.

## Mẫu Excel tiến độ nhiệm vụ (13/10/2026)

Import bỏ qua mục không nhập tiến độ mới, kể cả chỉ có ghi chú; không hiển thị trong preview, không đếm lỗi, không gửi API. Số 0 là giá trị hợp lệ. Nhiệm vụ có sản phẩm bỏ qua khi tất cả ô trạng thái mới đều trống; đã chọn một phần thì vẫn kiểm tra đủ trạng thái và ID, không suy đoán trạng thái của phần để trống.

Tiêu đề/tên cột theo loại đang xuất: Mục tiêu hoặc Nhiệm vụ. Không điền trước trạng thái mới. Mẫu nhiệm vụ thêm cột Sản phẩm đầu ra trong cùng bảng, mỗi sản phẩm một dòng; merge dọc STT/mã/tên/cơ quan/đơn vị tính/ghi chú theo nhiệm vụ. Trạng thái hiện tại và trạng thái mới riêng từng sản phẩm, dropdown 3 trạng thái ở cột H; nhiệm vụ không có sản phẩm chọn trực tiếp ở cột H. Mã sản phẩm ở cột ẩn J; không tạo sheet sản phẩm riêng cho mẫu mới. Mẫu mục tiêu giữ 8 cột. Dùng reportStatuses chung, validation Excel list/stop và kiểm tra lại khi import. Import nhóm dòng theo merge mã nhiệm vụ, đối chiếu đủ ID sản phẩm và tạo một báo cáo/nhiệm vụ; thiếu/trùng/sai mã hoặc trạng thái không hợp lệ phải báo lỗi. Tương thích mẫu cũ dùng sheet sản phẩm riêng. BE giữ metadata/minh chứng và không tin tên sản phẩm từ file.

Lịch sử báo cáo theo yêu cầu mới chỉ hiển thị dòng “Đơn vị thực hiện: tên người thao tác”; không kèm nguồn Import Excel hay tên cơ quan trong tên người. API vẫn giữ riêng cơ quan báo cáo và phạm vi giao. Bản cũ không ghi nhận người thao tác hiển thị rõ chưa ghi nhận, không suy đoán audit.
