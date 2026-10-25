# Khai thác dữ liệu công khai bằng AI

## Chế độ hiện hành (19/10/2026)

Hai đầu vào dùng chung kho AI: admin nạp PDF/DOCX/TXT, hoặc Google Alerts RSS nhận tiêu đề. RSS có lịch hằng ngày theo giờ Việt Nam và Quét ngay. AI chỉ đánh giá tiêu đề; chỉ bài liên quan mới được tải và đọc. Tiêu đề chưa rõ chờ admin xem. Không có cấu hình AI thì vẫn nhận RSS và giữ hàng đợi, không gọi AI.

Quét website diện rộng vẫn tắt: `AiCollectionWorker` không đăng ký, không khởi động lại lịch `AiDataSources` cũ. RSS chỉ đọc chính URL bài được Alerts thông báo trong danh sách nguồn cho phép; không tự theo liên kết trong bài. Google Sheets/Gmail Apps Script không cần dùng cho luồng mới; hàng đợi bridge cũ chưa gắn RSS vẫn không tự xử lý.

## Phạm vi giai đoạn đầu

Ba chuỗi số liệu phục vụ năm mục tiêu: EGDI (MT-12, MT-25), tỷ trọng kinh tế số/GDP (MT-13, MT-26), tỷ trọng doanh thu TMĐT/bán lẻ và dịch vụ tiêu dùng (MT-16). MT-27 đã được người dùng xóa vì trùng; loại khỏi danh mục AI, không tự tạo lại mục tiêu. Excel đánh giá bảo lưu 27 mục tiêu trong thư mục `tài liệu phát triển`, gồm nhóm ưu tiên 6, có điều kiện 11, hỗ trợ minh chứng 10.

## Lưu trữ và duyệt

- `AiDataSources`: nguồn chính thức, URL, lịch, trạng thái và khóa xử lý liên tiến trình.
- `AiSourceDocuments`: bản chụp văn bản theo trang, URL, thời điểm tải, SHA-256, model và lỗi.
- `AiDataSuggestions`: số liệu AI nguyên gốc, năm, đơn vị, phương pháp, trích dẫn và quyết định kiểm chứng. Giá trị admin hiệu chỉnh nằm ở `AcceptedValue`; không sửa giá trị/trích dẫn ban đầu.
- `AiGoalDataLinks`: nối **ID mục tiêu** với ID số liệu AI. Khi được duyệt, lưu thêm ID báo cáo đã tạo. Đổi tên/mã mục tiêu không làm mất liên kết đã có.

Thu thập chỉ tạo đề xuất chờ duyệt. **Admin duyệt sẽ tạo một lần báo cáo đã phê duyệt cho từng mục tiêu liên kết và đúng năm số liệu**, thông qua `ExecutionService`, cập nhật cache/tiến độ theo quy tắc dùng chung. Lịch sử báo cáo cũ và kho AI được giữ nguyên. Số liệu cùng năm mới được duyệt sẽ là bản báo cáo mới; số liệu năm cũ không đổi năm của báo cáo hiện hành. Không sao chép số liệu UN giữa các năm không công bố.

Duyệt, tạo báo cáo, lưu liên kết báo cáo và thay phiên bản AI hiện hành dùng chung giao dịch Serializable: lỗi ở một mục tiêu sẽ rollback toàn bộ. Duyệt lại cùng đề xuất bị chặn. Từ chối không tạo báo cáo. Phải có mục tiêu liên kết, đơn vị phù hợp, cấu hình định lượng và phân giao chủ trì hợp lệ. Hai loại đơn vị người dùng là phần trăm và số lượng. MT-12/MT-25 dùng SL; hệ thống giữ cách đánh giá top thứ hạng, không đổi giá trị thực tế 71 thành tỷ lệ. Không tự thay đổi cấu hình mục tiêu để ép nhận số liệu.

## Màn quản trị

Chỉ Admin truy cập UI và API. Nhóm Quản trị dữ liệu AI có ba mục: **Nguồn dữ liệu**, **Kiểm chứng số liệu**, **Cấu hình AI**.

Nguồn dữ liệu gồm ba tab: Dữ liệu đầu vào (chung tài liệu tải lên và bài RSS, lọc theo loại nguồn và trạng thái bằng API trước phân trang), Nguồn Google Alerts (cấu hình RSS/lịch và Quét ngay), Lịch sử xử lý (chọn AI đọc/bóc tách hoặc thu thập RSS). Nạp tài liệu là hộp thoại; tiếp nhận vẫn được khi AI chưa sẵn sàng. Các trạng thái, đọc lại và xem đề xuất dùng cùng hộp tiếp nhận; RSS có thêm bước phân loại tiêu đề trước khi đọc nội dung. Không gửi bài không liên quan vào bước đọc toàn văn.

Mục tiêu áp dụng là phần tham chiếu Xem thêm của Kiểm chứng số liệu, nhóm theo ba chỉ số và chỉ nối các mục tiêu còn tồn tại. Màn kiểm chứng giữ số liệu, dẫn chứng, nguồn/trang, cảnh báo và mục tiêu liên kết; ghi chú tùy chọn, xác nhận kiểm chứng bắt buộc trước khi duyệt. Nguồn tải lên và RSS đều tạo đề xuất vào cùng kho AI và đi qua cùng nghiệp vụ phê duyệt/báo cáo. Không tự tạo lại mục tiêu đã xóa.

Giao diện dùng chung AiPageHeader, style dưới features/ai và control hiện hữu. Bảng đầu vào, nguồn RSS và đề xuất chuyển sang thẻ khi vùng nội dung hẹp; không ẩn thao tác. Giữ tương thích URL #ai-alerts, mở tab nguồn RSS trong workspace hợp nhất. UI không đổi trạng thái nghiệp vụ, giá trị số liệu hoặc quyết định phê duyệt.

## Cấu hình và triển khai

1. Áp dụng migrations `AddAiPublicData`, `AddAiPublishedReports`, `AddAiAlertsInbox`, `AddAiProviderConfiguration`, `AddAiAlertRssSources` bằng quy trình triển khai BE hiện có; không chạy reset dữ liệu.
2. Admin vào **Quản trị dữ liệu AI → Cấu hình AI** (`#ai-settings`): nhập Base URL, model, API key; Kiểm tra kết nối gửi yêu cầu JSON Schema ngắn (có tính phí), không lưu. Bật đọc và Lưu để áp dụng ngay cho hàng đợi, không cần restart. Để trống khóa khi sửa sẽ giữ khóa cũ; có lựa chọn xóa khóa khi tắt AI. Khóa không trả lại qua GET, không lưu localStorage/FE; chỉ lưu ciphertext trong `AiProviderConfigurations`, có thời điểm/người lưu.
3. Nếu chưa có bản cấu hình trong DB, dùng fallback môi trường BE: `AiData__Enabled`, `AiData__BaseUrl`, `AiData__ApiKey`, `AiData__Model`. Một khi đã lưu trên giao diện, cấu hình DB là nguồn ưu tiên. API chỉ nhận HTTPS hostname công khai, kiểm tra địa chỉ DNS lúc kết nối và không theo redirect.
4. Từ migration `20261025143854_PersistAiProtectionKeys`, ASP.NET Core Data Protection lưu keyring trong bảng `DataProtectionKeys` của PostgreSQL, cùng vòng đời DB qua restart/redeploy; không cần persistent disk cho khóa mã hóa. Giữ nguyên ApplicationName `CDSQG.AiCredentials` và purpose `CDSQG.AiProvider.ApiKey.v1`. Startup chạy migration trước khi worker/API dùng khóa, rồi nhập keyring cũ một cách idempotent nếu `App_Data/ai-keys` hoặc `AiData__ProtectionKeyDirectory` vẫn còn; không xóa file cũ hay ciphertext. Biến thư mục cũ chỉ phục vụ chuyển đổi, không còn là nơi ghi key mới. Nếu keyring cũ đã mất, khôi phục backup hoặc nhập lại API key một lần sau nâng cấp. Giao diện báo lỗi giải mã rõ ràng; lưu model/URL với ô API key trống sẽ không ghi mất ciphertext lỗi.
   - Có thể nâng cấp riêng không chạy HTTP/worker/seed: trong thư mục backend chạy `dotnet run -- --migrate-ai-keyring`. Backend bình thường cũng tự áp dụng migration và nhập khóa lúc khởi động.
   - Backup/restore cần giữ cả `DataProtectionKeys` và `AiProviderConfigurations`; không xóa/reset bảng khóa khi deploy.
   - Có thể mã hóa độc lập keyring mới bằng chứng chỉ PFX trong secret môi trường: `AiData__KeyEncryptionCertificateBase64`, `AiData__KeyEncryptionCertificatePassword`. Dùng cùng chứng chỉ/private key qua mọi lần deploy và instance; không sinh lại mỗi lần khởi động, không commit PFX hoặc mật khẩu. Khi chưa cấu hình chứng chỉ, provider EF lưu key XML không bọc mã hóa độc lập: cần giới hạn quyền DB/backup và mã hóa ổ lưu trữ. Các khóa legacy nhập nguyên trạng, không tự chuyển đổi mã hóa. Không xóa chứng chỉ cũ khi keyring còn khóa phụ thuộc nó.
   - File tài liệu gốc vẫn cần `AiData__UploadDirectory` trên ổ lưu trữ bền vững riêng; thay đổi này chỉ giải quyết vòng đời khóa API, không thay cơ chế lưu tài liệu.
5. Các giới hạn bộ đọc: `AiData__MaxDocumentsPerRun` mặc định 3 (giới hạn 1–5), `AiData__MaxChunksPerDocument` mặc định 3 (giới hạn 1–8). Mỗi chunk tối đa 12.000 ký tự; timeout API 90 giây. Chu kỳ worker 30 giây; lịch nguồn tối thiểu 24 giờ. Khóa nguồn hết hạn sau 2 giờ để phục hồi tiến trình bị dừng.
6. Ba nguồn mẫu cũ được giữ để bảo lưu lịch sử, không bật thu thập tự động. Google Alerts RSS dùng lịch riêng; không dùng URL mẫu 2024/2022 như nguồn tự khám phá.
7. Build và triển khai cả BE/FE. Không cần lưu API key ở GitHub nếu runtime BE đã quản lý biến môi trường.

## Giới hạn và kiểm chứng

Chỉ HTTPS và danh sách hostname chính thức được phép; kiểm tra lại redirect, phân giải địa chỉ công khai và kết nối tới IP đã kiểm tra. Tải tối đa 12 MB, PDF tối đa 350 trang/900.000 ký tự; PdfPig 0.1.16 đọc PDF có lớp chữ, không OCR bản scan. Luồng tự tìm bài trên website vẫn tạm dừng; link bài chính xác từ RSS được xử lý. Bộ đọc chỉ gửi các đoạn có từ khóa trong ngân sách, có thể bỏ sót tài liệu dài.

AI không có công cụ; nội dung nguồn được xem là dữ liệu, không thi hành chỉ dẫn. BE kiểm tra năm, miền giá trị, đơn vị, phạm vi Việt Nam, trang, trích dẫn nguyên văn và sự xuất hiện giá trị. Các kiểm tra này không chứng minh số liệu đúng ngữ nghĩa: admin vẫn đối chiếu kỳ năm, số thực tế so với mục tiêu/dự báo, phạm vi và phương pháp; chênh lệch giữa nguồn cùng năm có cảnh báo. Không coi tỷ trọng GDP, GRDP, tăng trưởng và doanh thu ICT là cùng chỉ số. EGDI thường công bố hai năm một lần.

Kiểm thử tính năng RSS dùng schema PostgreSQL tạm và provider giả lập, không gọi API AI thật hoặc thay đổi báo cáo ứng dụng.

## Sửa luồng đọc PDF (17/10/2026)
- PDF dùng ContentOrderTextExtractor giữ ranh giới chữ/cột số; rà tất cả trang trước khi chọn các đoạn trong ngân sách cấu hình. Ưu tiên bảng chỉ số; vẫn giới hạn số đoạn gửi AI, chưa bảo đảm bao phủ mọi cách diễn đạt.
- Schema ràng buộc scope là Việt Nam; không nhận nhãn mơ hồ Toàn quốc. Server gắn số trang theo trang nguồn thực sự gửi AI; quote và giá trị vẫn phải khớp nguồn.
- Đọc lại thực thi bóc tách lại; không bị cache bỏ qua, không tạo trùng dẫn chứng/báo cáo.
- Lịch sử phân biệt Extracted, EvidenceRejected (kèm nguyên nhân), NoData; hàng đợi ReadNoData không báo như đã có đề xuất.
- Thử thật PDF UN 2024 qua Gemini: Việt Nam hạng EGDI 71 năm 2024, dẫn chứng PDF 137 và 204, Pending, liên kết MT-12/MT-25. Chưa phê duyệt; số liệu lịch sử trước giai đoạn theo dõi.


## Google Alerts RSS: cấu hình và vận hành

1. Trong Google Alerts, tạo cảnh báo theo chủ đề, chọn Deliver to → RSS feed. Dùng truy vấn có `site:` để giới hạn các website uy tín tương ứng. Sao chép URL biểu tượng RSS (dạng `https://www.google.com/alerts/feeds/<id>/<id>`).
2. Vào Quản trị dữ liệu AI → Nguồn dữ liệu → Nguồn Google Alerts → Thêm nguồn RSS, đặt tên và URL. Mỗi nguồn được bật/tắt lịch, đặt giờ HH:mm theo Việt Nam (UTC+7); mặc định 08:00, lịch tắt khi mới tạo. Không thay URL RSS bằng API key.
3. Quét ngay xếp một job bền trong DB, không giữ HTTP request chờ AI. Worker kiểm tra mỗi 30 giây. Lịch chạy một lần/ngày; sau downtime chỉ quét bù một lượt. Manual không thay đổi mốc lịch ngày tiếp theo.
4. Lần đầu nhận tối đa 200 entry hiện có trong RSS. Lần sau nhận toàn bộ entry hiện còn trong RSS và bỏ trùng URL chuẩn hóa, không chỉ cắt theo ngày để tránh bỏ tin đến muộn. RSS không bảo đảm Google tìm đủ mọi bài; entry đã rời feed trong thời gian hệ thống ngừng có thể không nhận được.
5. Chỉ nhận bài HTTPS thuộc `AiMetricCatalog.AllowedHosts`. Link google.com/url được bóc tham số URL, không truy cập bộ chuyển hướng của Google. Bài ngoài allowlist hoặc tiêu đề trống/quá dài được ghi số lượng bỏ qua trong lượt quét, không gọi AI.
6. Phân loại mỗi tiêu đề mới một lần: liên quan → đọc URL bài; chưa rõ → NeedsReview chờ admin chọn chỉ số hoặc đánh giá lại tiêu đề; không liên quan → NotRelevant, không tải nội dung. Nội dung link được kiểm tra lại domain và IP công khai, giới hạn tải, redirect, PDF có chữ. Chỉ đọc bài/link PDF chính xác, không tự tìm file đính kèm trong bài.
7. `AiAlertFeeds` giữ cấu hình và lịch; `AiAlertScans` giữ người gọi, trigger, trạng thái, số đã nhận/mới/trùng/bỏ qua/lỗi. `AiInboxItems.AlertFeedId` và PublishedAt ghi nguồn RSS và ngày bài nếu feed cung cấp. Unique URL hash chống trùng giữa nguồn; khóa hàng và unique pending scan chống quét song song; lease phục hồi sau khởi động lại. Tắt lịch chỉ dừng nhận RSS tự động, không hủy các tài liệu đang chờ AI.
8. Đọc bài thất bại giữ Error và có nút đọc/đánh giá lại, không tự thử vô hạn hoặc ghi báo cáo. Admin vẫn cần duyệt số liệu. Lịch chỉ chạy khi BE hoạt động; hosting ngủ hoặc tắt máy local sẽ trễ giờ quét.

Kiểm thử: schema PostgreSQL tạm cho migration, lịch theo giờ VN, manual/scheduled trùng nhau, URL chuẩn hóa trùng, phân loại tiêu đề → đọc đúng bài liên quan, uncertain/unrelated không fetch, không tạo ProgressLog. Parser Atom/RSS chặn DTD/XXE và giới hạn kích thước; browser test nguồn, giờ quét, job pending, tiêu đề, lịch sử và mobile. Ngày 19/10 đã đọc nguồn RSS tài khoản người dùng bằng chính bộ đọc hệ thống: HTTP thành công, 0 entry khi vừa tạo. Luồng có bài được kiểm thử bằng feed/provider giả lập; chưa chứng minh đọc AI trên bài thật của RSS này.

### Mã đầu vào và cách nạp
Hai khối Nạp tĩnh (tài liệu) và Nạp động (Google Alerts) dẫn vào danh sách và kiểm chứng chung. Mỗi đầu vào có số thứ tự dài do PostgreSQL sequence cấp, unique trong database; UI hiển thị TL-000001 hoặc RSS-000002. Mã không đổi khi đọc lại, lọc hay chuyển trang. Migration cấp mã cho dữ liệu cũ theo thời gian tiếp nhận, giữ nguyên ID, trạng thái và liên kết. Số có thể có khoảng trống khi giao dịch hủy hoặc bỏ qua bản trùng; không phải STT theo trang.

Cập nhật: danh sách tách tab Nạp trực tiếp và Google Alerts, lọc tại server theo loại nguồn. Cấu hình RSS/lịch quét trong mục mở rộng tại tab Google Alerts. Mã TL-ddMMyyyy-001 / RSS-ddMMyyyy-002 dùng ngày tiếp nhận giờ Việt Nam và số chạy chung tối thiểu 3 chữ số, không cắt số trên 999, không đặt lại theo ngày. Nạp thành công mở chi tiết trạng thái; file trùng thông báo và mở bản đã tồn tại.

Xử lý lại: popup đóng sau khi API xác nhận xếp hàng; lỗi giữ popup. Đọc AI thử tối đa 3 lần khi HTTP 502/503/504 (chờ 1 rồi 2 giây), không tự retry lỗi 401/429. Thông báo nhà cung cấp và timeout được lưu an toàn tại inbox/lịch sử; không lưu body lỗi hoặc khóa API.

Đọc lại tạo AiInboxItem mới, mã mới, dùng chung file gốc bất biến; mỗi lần xử lý có AiSourceDocument mới và bộ đề xuất riêng. Bản cũ và quyết định duyệt không đổi. RSS đi cùng luồng đề xuất/kiểm chứng/báo cáo như tài liệu tải lên. Nút Xem kết quả và dẫn chứng lọc theo đầu vào, hiển thị mọi trạng thái để không ẩn dẫn chứng đã duyệt.

Nguồn đối soát: AiSourceDocument.InboxItemId liên kết chính xác bản đầu vào. API kiểm chứng trả mã/số đầu vào, loại upload/RSS, thời điểm tiếp nhận/đọc, tên feed cùng tên và URL nguồn. UI dùng AiSourceIdentity trên bảng, card và modal. Migration nối bản cũ theo URL file chính xác; RSS cũ chỉ nối khi duy nhất, không đoán bản nếu có nhiều lần đọc.

Demo RSS thật: hỗ trợ thêm kênh chính thức https://www.nso.gov.vn/feed/ và từ khóa tìm kiếm s (tối đa 100 ký tự, không tham số khác). Cùng worker/khử trùng/phân loại/đọc/kiểm chứng; ghi origin RSS trực tiếp. Nguồn demo chạy thủ công, không tự ghi báo cáo. Google Alerts vẫn theo dõi mới, RSS NSO có thể trả bài lưu trữ phù hợp từ khóa.

Hàng đợi đọc chung: upload, đọc lại và RSS đều đi qua AiInboxQueueGate. Database chỉ cho một tác vụ giữ khóa, xử lý một đầu vào/lần. Hoàn tất hoặc lỗi đều đặt NextReadAt = thời điểm kết thúc +60 giây. Worker kiểm tra 5 giây/lần nên có thể bắt đầu muộn hơn tối đa một chu kỳ. Khóa và thời gian nghỉ tồn tại qua restart; lease hết hạn sau 2 giờ để phục hồi khi tiến trình bị ngắt đột ngột. Thu thập RSS không gọi AI nên vẫn tiếp nhận bài vào hàng đợi.

Khắc phục tác vụ kẹt: chống trùng cùng document/chỉ số/năm/giá trị không phụ thuộc cách AI diễn đạt phương pháp; khi lưu lỗi phải tháo các insert thất bại khỏi tracker trước khi ghi lỗi. Trạng thái cuối và giải phóng lease inbox dùng ExecuteUpdate độc lập, tránh lặp lại insert lỗi. Không giới hạn tổng thời gian một tài liệu theo yêu cầu người dùng; giữ timeout từng HTTP request và cơ chế nghỉ 60 giây giữa hai tài liệu.

Xóa nguồn RSS là xóa mềm DeletedAt, tắt lịch và chặn quét/sửa; khóa dòng database đồng bộ với enqueue. Danh sách Nguồn đã xóa cho phép khôi phục, lịch vẫn tắt sau khôi phục. Không xóa bài viết, snapshot, đề xuất, báo cáo hoặc lịch sử. Không xóa được nguồn đang Queued/Running.

Danh sách đầu vào nạp trực tiếp và RSS phân trang server cố định 5 bản ghi/trang, lọc trước đếm và phân trang; xếp ReceivedAt giảm dần rồi SequenceNumber giảm dần để ổn định khi nhận cùng lúc. STT desktop/mobile nối tiếp theo pageSize server trả về.

Lịch sử AI đọc tài liệu và lịch sử thu thập RSS cùng phân trang server 5 bản ghi, mới nhất trước theo RetrievedAt/QueuedAt và khóa Id để ổn định khi trùng thời gian. STT nối tiếp theo pageSize.


## Mở rộng internet theo mục tiêu và đơn vị (25/10/2026)

- Nguồn RSS cũ mặc định tiếp tục theo dõi 3 chỉ số của 5 mục tiêu ưu tiên. Nguồn mới/sửa nguồn có thể chọn tối đa 10 chỉ số/mục tiêu định lượng và 100 đơn vị. Mục tiêu được định danh bằng ID, kèm quyết định; không dùng mã MT trùng giữa các quyết định để suy ra cùng chỉ tiêu.
- Có thể dùng Google Alerts, Google News RSS, RSS cổng .gov.vn và các nguồn báo chí được phép. Tạo nhanh Google News theo Đảng Cộng sản/Nhân Dân/VOV/VTV trên giao diện; URL bài vẫn được kiểm tra host HTTPS, DNS công khai, không theo redirect và không crawl link con. Danh sách host không phải sự xác nhận mọi số liệu trong bài đều đúng.
- Chỉ số EGDI/GDP/thương mại điện tử ưu tiên giữ định nghĩa toàn quốc. Mục tiêu khác lấy phạm vi từ các đơn vị chủ trì đã được phân giao; nếu không lọc đơn vị thì dùng toàn bộ phân giao hợp lệ. AI nhận danh sách phạm vi giới hạn trong cùng lượt bóc tách, không gọi riêng cho từng đơn vị.
- Mỗi đề xuất lưu TargetAgencyId, Scope, năm, giá trị/đơn vị, đoạn trích và trang nguồn. Đoạn trích theo đơn vị chỉ được chứa một đơn vị trong tập phạm vi, phải có tên đơn vị và giá trị thật; năm phải xuất hiện trong bản chụp. Bỏ dự báo/mục tiêu và kỳ tháng/quý. Không lấy GRDP địa phương làm GDP quốc gia.
- Không ghi báo cáo trước kiểm chứng. Khi duyệt, kiểm tra lại phân giao chủ trì và thời gian thực hiện; báo cáo gắn đúng AgencyId. Khóa unique kết quả hiện hành theo chỉ số–năm–đơn vị, riêng dữ liệu quốc gia. Duyệt kết quả một đơn vị không thay kết quả đơn vị khác. Dữ liệu không có mục tiêu/phân giao hợp lệ chỉ để tham khảo.
- URL trùng vẫn bỏ qua khi nhận. Các bài có cùng dẫn chứng/giá trị được cảnh báo trùng; số khác nhau cùng chỉ số/năm/đơn vị được cảnh báo đối chiếu. Lưu lịch sử nguồn và quyết định cũ, không cộng các bài đăng lại thành nhiều kết quả.
- Migration `InternetGoalAgencyTracking` thêm cấu hình JSON mặc định [] cho RSS cũ và TargetAgencyId nullable; giữ nguyên đề xuất/lịch sử đang có. Luồng file nạp trực tiếp giữ riêng.

### Bỏ Google News (25/10/2026)
Theo yêu cầu sử dụng, đã bỏ trình tạo Google News RSS và chặn URL Google News ở backend. Thêm nguồn bằng URL Google Alerts hoặc RSS trực tiếp được phép; vẫn chọn mục tiêu/đơn vị như trước. Dẫn chứng và lịch sử hiện có được giữ nguyên.

### Phạm vi văn bản trên các màn AI
Danh sách upload, RSS, tiêu đề chưa đọc, lịch sử quét/đọc và kiểm chứng nhận documentId từ mode thanh trên. AiDocumentScope phân loại bằng ID mục tiêu; chỉ số quốc gia và nguồn cũ mặc định thuộc 1266. Nguồn cấu hình mục tiêu nhiều văn bản xuất hiện trong các văn bản tương ứng. Tạo/sửa nguồn trong mode chỉ nhận mục tiêu của văn bản đó; 2629 cần chọn mục tiêu cụ thể. Không xóa hay gán lại dữ liệu khi đổi mode.

### RSS đọc theo Thiết lập từ khóa của văn bản (thay thế cấu hình mục tiêu/đơn vị từng nguồn)
Nguồn RSS chỉ lưu tên, URL, văn bản hiện tại và lịch quét. Không cần chọn mục tiêu hay đơn vị. DocumentId của nguồn được lưu độc lập; cùng URL có thể dùng cho hai văn bản, chống trùng bài theo URL + văn bản. Migration RssDocumentKeywordContext giữ lịch sử, suy ra văn bản cho nguồn cũ có mục tiêu cùng văn bản; nguồn mặc định vẫn thuộc 1266.
Mỗi lần đánh giá tiêu đề, ContextAsync lấy tất cả mục tiêu định lượng có Thiết lập từ khóa của đúng văn bản: tên, từ khóa Việt/Anh, đơn vị và chỉ dẫn bóc tách. Tiêu đề có thể khớp nhiều mục tiêu. Bài liên quan được đọc nội dung để tạo nhiều đề xuất độc lập có dẫn chứng; đơn vị được đối chiếu từ nội dung và phân giao, không bị giới hạn theo nguồn. Chưa cấu hình từ khóa thì báo rõ, không tự đánh dấu bài không liên quan. Cập nhật từ khóa được dùng ở lần phân loại/đọc tiếp theo; không tự xử lý lại lịch sử. Không có API hội thoại nối tiếp: mỗi request dựng messages mới, không tích lũy context qua các đợt quét.
