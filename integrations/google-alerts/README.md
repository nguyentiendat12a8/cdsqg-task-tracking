# Google Alerts → Google Sheets → hộp tiếp nhận AI

Luồng tự quét website đã tạm dừng. Không cần API AI để nhận thông báo vào Sheet. AI chỉ xử lý sau khi có khóa/model hợp lệ trên BE.

## Cài đặt một lần trên tài khoản Google của admin

1. Tạo Google Sheet riêng tư, chọn **Tiện ích mở rộng → Apps Script**, dán nội dung `Code.gs` trong thư mục này. Không công khai Sheet.
2. Trong **Project Settings → Script Properties**, đặt:
   - `CDSQG_API_BASE_URL`: URL HTTPS của backend đã triển khai, không phải URL frontend.
   - `CDSQG_ALERTS_BRIDGE_KEY`: chuỗi ngẫu nhiên tối thiểu 32 ký tự, tối đa 256; cùng giá trị biến môi trường BE `AiData__AlertsBridgeKey`. Không ghi khóa vào ô Sheet hoặc Git, không dùng API key của AI làm khóa này.
3. Chạy `setup()` trong trình soạn thảo và cấp quyền đọc Gmail, cập nhật Sheet, gọi backend và tạo lịch. Script tạo sheet `Google Alerts` và lịch chạy mỗi giờ. Chỉ xử lý thư gửi từ `googlealerts-noreply@google.com`; không gửi email hay đọc bài toàn văn.
4. Tạo các Google Alerts bằng chính tài khoản đó, chọn gửi email. Gợi ý tạo riêng theo nhóm nguồn/từ khóa (có thể tách thêm Alert để tăng độ phủ):
   - `site:desapublications.un.org "E-Government Survey"`
   - `site:publicadministration.un.org "Viet Nam"`
   - `site:nso.gov.vn "kinh tế số"`
   - `site:gso.gov.vn "kinh tế số"`
   - `site:moit.gov.vn "thương mại điện tử"`
5. Chạy `syncAlerts()` lần đầu, kiểm tra Sheet và tab **Google Alerts / Sheets**. Email được gắn nhãn `CDSQG-Alerts-Processed` sau khi xử lý; không xóa email. Có thể bỏ nhãn rồi chạy lại nếu cần đọc lại thư. Mỗi lượt tối đa 30 hội thoại và gửi 50 dòng để giới hạn quota.

Google Alerts không có chức năng ghi thẳng Excel. Apps Script là cầu nối email → Sheet, còn Sheets cho tải xuống `.xlsx` bằng **Tệp → Tải xuống → Microsoft Excel**. Nếu chưa cấu hình backend, script vẫn thu thông báo vào Sheet; cột Đồng bộ giữ trạng thái chờ. HTTP 401 cần kiểm tra khóa; 429/5xx chờ lần sau. Script không ghi phản hồi chứa khóa vào Sheet/log.

## Hợp đồng kết nối

`POST /api/ai-alerts-bridge`, header `X-Alerts-Key`, JSON array tối đa 100 dòng `{title,url}`. Khóa chỉ có quyền nạp tiêu đề/URL, không tải tài liệu riêng, gọi AI trực tiếp hay phê duyệt báo cáo. Backend kiểm tra URL HTTPS thuộc hostname chính thức và bỏ trùng URL. Bridge chưa đặt khóa hoặc khóa sai luôn trả 401. Việc gắn trạng thái “Đã đồng bộ” chỉ nghĩa là backend nhận tiêu đề, không phải đã duyệt số liệu.

Admin có thể nhập Excel dự phòng với cột `Tiêu đề` và `URL`; tối đa 100 dòng, 5 MB. Các dòng sai bị chặn để sửa trước khi nhập; không nhận URL ngoài nguồn cho phép. Không công khai Sheet bằng CSV để hệ thống đọc.

## Trạng thái xử lý

- **Chờ AI lọc tiêu đề:** chỉ gửi tiêu đề, không tải bài.
- **Có khả năng liên quan — chờ đọc:** AI chọn chỉ số; hàng đợi tải đúng URL đã nhận, không đi theo các liên kết trên trang.
- **Chưa rõ — cần admin xem:** không tự đọc; admin chọn chỉ số và bấm đọc.
- **AI đánh giá chưa liên quan:** không tự đọc; vẫn giữ trong hộp tin để admin có thể chọn đọc nếu AI bỏ sót.
- **Đã đọc:** kết quả bóc tách nằm trong Đề xuất và kiểm chứng; có thể không tìm thấy số liệu.
- **Lỗi xử lý:** admin lọc lại tiêu đề hoặc chọn chỉ số để đọc lại sau khi sửa cấu hình/nguồn.

Worker kiểm tra mỗi 30 giây khi BE hoạt động, mỗi lượt tối đa 5 mục. Tiêu đề được phân loại trước; bài liên quan được đọc ở lượt sau. Chưa có AI key/model thì dữ liệu nằm chờ. Apps Script và Google Alerts không thay thế bước kiểm chứng; AI vẫn có chi phí khi phân loại và đọc toàn văn.

## Admin tải tài liệu

Tab **Nạp tài liệu** nhận PDF có lớp chữ, DOCX, TXT UTF-8 tối đa 12 MB. Admin chọn chỉ số cần bóc tách. File không phải qua lọc tiêu đề vì admin chủ động chọn. Bản scan cần OCR trước; DOCX dẫn chứng theo số đoạn thay vì số trang in. File lưu riêng ngoài wwwroot, chỉ admin tải qua API có xác thực.

BE cần `AiData__UploadDirectory` trỏ tới thư mục ổ đĩa bền vững trong triển khai (ví dụ volume dữ liệu). Mặc định lưu `App_Data/ai-documents` dưới thư mục BE; phải backup cùng DB, không dùng ổ tạm nếu muốn giữ file sau redeploy. Bản trích văn bản và nguồn vẫn lưu DB, độc lập với file gốc.

## Giới hạn

Google Alerts phụ thuộc kết quả Google lập chỉ mục, không bảo đảm thu được mọi công bố, mọi PDF hoặc thông báo ngay lập tức. Bộ tách email dựa vào liên kết HTML nên cần kiểm tra khi Google đổi mẫu thư. Apps Script có quota và trigger chạy dưới tài khoản người đã cấp quyền, không phải dịch vụ vô hạn. File/upload và số liệu AI không tự trở thành báo cáo: admin duyệt mới tạo báo cáo như luồng hiện có.

Tài liệu chính thức: https://support.google.com/websearch/answer/4815696 ; https://developers.google.com/apps-script/guides/triggers/installable ; https://developers.google.com/apps-script/guides/services/quotas
