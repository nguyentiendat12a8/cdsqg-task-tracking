# Theo dõi kế hoạch chuyển đổi số

Menu **Tiện ích → Thông tin chuyển đổi số**. Không còn tab Google Sheets trong dữ liệu Internet AI.

## Thu thập độc lập

Cấu hình mẫu lấy từ các tab NGUON_THEO_DOI, TU_KHOA, CAU_HINH, HUONG_DAN trong sheet người dùng cung cấp ngày 02/11/2026: 85 nguồn (51 bộ/tỉnh và 34 Sở KH&CN), 21 cụm tìm kiếm. Có thể thêm, sửa, xóa hoặc bật/tắt trên web. Quét mặc định mỗi 30 phút, 12 cặp nguồn × từ khóa mỗi lượt, tìm trong 365 ngày, lưu con trỏ luân phiên trong PostgreSQL. Máy chủ phải hoạt động để chạy lịch; khởi động lại tiếp tục lượt đến hạn. Khóa DB ngăn quét trùng giữa nhiều instance. Lỗi nguồn không xóa bài cũ.

Nguồn tìm kiếm RSS là Google News với truy vấn giới hạn site của nguồn chính thức; không cần Google Sheets, Apps Script hoặc tài khoản Google để điều phối. Tìm kiếm RSS không bảo đảm bao phủ toàn bộ lịch sử. URL Google News được giữ làm link trung gian, không coi đó là bằng chứng đã xác minh văn bản gốc. Chống trùng URL chuẩn hóa và bộ/tỉnh + tiêu đề chuẩn hóa. Bài cũ nhập từ sheet được giữ nguyên.

## Đọc tiêu đề khi được yêu cầu

Thu thập không gọi AI. Admin bấm **Đọc** để tạo DigitalPlanRead riêng, worker lấy đúng tiêu đề và gợi ý nguồn gửi một yêu cầu độc lập tới cấu hình AI hiện có; không nạp lịch sử hội thoại hoặc mục tiêu 1266/2629. Kết quả lưu theo hash tiêu đề + bộ/tỉnh, click tiếp xem kết quả đã lưu. Lỗi cho thử lại, không tự lặp yêu cầu lỗi/quota. Tiêu đề thay đổi có kết quả mới, lịch sử cũ giữ lại.

Related chỉ cho tin nói rõ ban hành/phê duyệt kế hoạch chuyển đổi số hằng năm hoặc khoảng đúng 5 năm tính cả năm đầu/cuối. Kiểm tra năm và trích dẫn có thật trong tiêu đề; thiếu căn cứ ghi NeedsReview. Dự thảo, triển khai kế hoạch có sẵn, sai phạm vi hoặc không rõ chủ thể không báo Related. Kết quả chỉ là nhận diện tiêu đề, người dùng xem nguồn để xác minh pháp lý. Khi Related tạo thông báo trong hệ thống cho người bấm đọc, một lần cho mỗi kết quả, mở chi tiết qua #digital-news?article=ID.

Không tạo AiInboxItem, đề xuất bóc số liệu, ProgressLog hoặc tự duyệt kết quả mục tiêu. Migration cộng thêm bảng/cột; chỉ dừng các job Sheet cũ còn chờ và chưa có lease đang chạy, giữ lịch sử AI và mọi dữ liệu bài đã có.

## Kiểm chứng

DigitalPlanMonitoringTests: nguồn mẫu, an toàn truy vấn, kế hoạch hằng năm/5 năm, thiếu bằng chứng, migration PostgreSQL, chống trùng, lịch/con trỏ, không tự gọi AI, đọc thủ công/cache và thông báo một lần. digital-news.browser.mjs: desktop/mobile, cấu hình và chi tiết, vai trò đọc, menu riêng, phân trang/sort/filter.

## Tiêu chí đánh giá cấu hình được (02/11/2026)

Cấu hình theo dõi → Tiêu chí đánh giá: mỗi dòng một cụm từ liên quan/hành động/loại trừ, chọn kế hoạch hằng năm và/hoặc 5 năm. Khớp cụm từ không phân biệt hoa thường/dấu, theo ranh giới từ; cần ít nhất một cụm liên quan và một cụm hành động, không được có cụm loại trừ. AI đọc ngữ nghĩa tiêu đề, không nhận URL hoặc nội dung bài. Năm, dẫn chứng nguyên văn và việc đây là kế hoạch vẫn được kiểm tra ở backend.

Cấu hình lưu database, mỗi lượt đọc chụp lại cấu hình. Cache theo bài + tiêu đề + hash tiêu chí. Đổi tiêu chí không tự gọi AI, lần Đọc sau tạo kết quả riêng; dữ liệu kết quả cũ không bị ghi đè. Kết quả cũ trước migration được gắn bộ tiêu chí mặc định, không bị biến thành Chưa đọc. Từ khóa thu thập tin vẫn độc lập.

## Khung kiến trúc số

Loại Architecture nhận diện ban hành/phê duyệt khung kiến trúc số, khung kiến trúc chính phủ số, khung kiến trúc chính quyền số. Có nhóm từ khóa riêng và checkbox riêng; không đòi hỏi năm hay thời kỳ của kế hoạch. Chỉ nói xây dựng khung, ban hành kế hoạch xây dựng khung hoặc dự thảo không đủ để báo đã ban hành chính khung kiến trúc. Chỉ đọc tiêu đề, lưu snapshot tiêu chí, không tự đọc lại bài.

Default mới bật Architecture. Migration chỉ thêm Architecture khi chưa có, điền ArchitectureKeywords khi trống; giữ từ khóa và các checkbox người dùng đã nhập/tắt. Thêm hai cụm thu thập chính phủ số/chính quyền số nếu chưa có (tổng mặc định 23 cụm); cụm khung kiến trúc số đã có từ sheet giữ nguyên. Bài/kết quả đọc cũ được giữ; bộ tiêu chí mở rộng có cache riêng, người dùng bấm Đọc để đánh giá theo phạm vi mới.

## Tiêu chí thống nhất (thay thế cách tách loại trước đây)

Giao diện và API hiện chỉ có ba nhóm: RelatedKeywords, ActionKeywords, ExcludedKeywords. Từ khóa khung kiến trúc số được gộp vào RelatedKeywords. Không còn checkbox hoặc giới hạn theo loại: AI tự phân loại Annual/FiveYear/Architecture, backend kiểm tra năm và dẫn chứng theo loại nhận diện. Mọi loại đều phải khớp ít nhất một cụm liên quan, một hành động và không có cụm loại trừ.

Migration UnifiedDigitalPlanCriteria ghép các cụm cũ theo thứ tự, loại trùng không phân biệt hoa thường, giữ nguyên hành động/loại trừ. Không sửa bài, lịch thu thập hay lịch sử kết quả; cấu hình đổi có hash riêng, chỉ bấm Đọc mới đánh giá lại. Adapter Parse vẫn đọc snapshot cũ có ArchitectureKeywords, bỏ các cờ bật/tắt cũ khi xử lý yêu cầu còn chờ. Cấu hình mới lưu đúng ba trường.

## Khoảng thời gian đăng bài

Lịch quét có PublishedFrom/PublishedTo dạng ngày. Chưa cấu hình ngày từ thì dùng ngày 1 của tháng hiện tại theo UTC+7; ngày đến để trống là ngày hiện tại. Lưu ngày cụ thể sẽ giữ mốc đó cho các lượt tiếp theo. Truy vấn RSS dùng after/before mở rộng biên một ngày, backend kiểm tra PublishedAt theo ngày Việt Nam và lấy cả ngày đầu/cuối. Bài không có thời gian đăng đáng tin cậy không được thêm; không dùng thời gian nạp để thay thế. LookbackDays còn lưu để tương thích dữ liệu cũ, không còn dùng làm bộ lọc thu thập. Thay đổi khoảng ngày không xóa bài và kết quả đã lưu.

## Tập trung hai chủ đề và từ gần nghĩa

Bộ mặc định hiện bật 15 truy vấn theo văn bản chuyển đổi số và kiến trúc số/điện tử. Bao gồm kế hoạch, đề án, chương trình, nghị quyết, chiến lược và các cách gọi chính phủ số/chính quyền số/chính phủ điện tử/chính quyền điện tử. Các truy vấn mặc định rộng về tỷ trọng kinh tế số, chỉ số, dịch vụ công, AI, hồ sơ… được tắt. Giữ nguyên các dòng người dùng đã sửa nội dung hoặc tự thêm. Bỏ RequiredTerms mặc định bắt buộc “chuyển đổi số” ở các nguồn seed để không loại mất bài kiến trúc không chứa cụm đó; không bỏ điều kiện riêng do người dùng thay đổi.

Từ khóa đánh giá được bổ sung biến thể cùng hành động thông qua/công bố, vẫn giữ từ riêng và chỉ đọc tiêu đề. DigitalDocument là nhãn cho đề án/chương trình/nghị quyết/chiến lược hoặc kế hoạch không thuộc kỳ năm/5 năm; không bắt buộc năm. Annual/FiveYear vẫn kiểm tra đúng thời kỳ nếu AI chọn loại đó. Không chỉ dựa vào từ khóa để kết luận đã ban hành.

Thông báo ưu tiên route digital-news trước luồng duyệt tiến độ (tránh nhầm khi tiêu đề chứa “phê duyệt”). Bấm thông báo mở chính xác bài và kết quả qua result ID; thông báo cũ được tra bằng NotificationId. Hoạt động từ màn khác, khi đang ở danh sách, và khi bấm lại cùng thông báo. Kết quả lịch sử vẫn mở được sau đổi tiêu chí; không gọi AI lại.

## Lọc tiêu đề theo từ khóa thu thập trước khi lưu

Từ khóa thu thập không chỉ tạo truy vấn RSS: tiêu đề trả về phải chứa cụm tìm kiếm và ít nhất một điều kiện đi kèm (phân cách dấu phẩy, hoặc không yêu cầu điều kiện khi để trống), không phân biệt hoa thường/dấu. Phải có dấu hiệu văn bản/kiến trúc thuộc phạm vi theo dõi. Bộ lọc DigitalNewsCollectionFilter không đọc DigitalPlanCriteria và không gọi AI. Tiêu chí AI và từ khóa loại trừ AI vẫn chỉ dùng khi bấm Đọc. Vì vậy tin dự thảo nếu khớp truy vấn vẫn có thể xuất hiện để AI đánh giá sau; bộ lọc không khẳng định đã ban hành.

Truy vấn RSS dùng intitle cho cụm tìm kiếm và các điều kiện, rồi kiểm tra lại tại backend để không tin mù quáng kết quả từ Google News. Các truy vấn mặc định chủ đề rộng (chuyển đổi số/chính phủ số/chính quyền số) được thêm điều kiện ban hành/phê duyệt/thông qua/công bố thay vì chỉ cần nhắc tên loại văn bản. Dòng đã sửa điều kiện riêng được giữ nguyên.

Danh sách áp dụng cùng bộ lọc theo các từ khóa thu thập đang bật TRƯỚC đếm/phân trang. Các bài nhiễu cũ không còn hiện trong danh sách, nhưng không xóa bản ghi, kết quả và thông báo cũ; đường dẫn chi tiết kết quả vẫn hoạt động. Đổi tiêu chí AI không làm đổi tập bài thu thập/hiển thị. Đổi từ khóa thu thập mới ảnh hưởng tập bài này.

## Nới mức thu thập theo chủ đề

Bộ lọc hiện nhận diện lõi chủ đề của từ khóa thu thập (chuyển đổi số/chính phủ số/chính quyền số + loại văn bản; hoặc kiến trúc số/điện tử), không ép khớp nguyên cụm dài. Các điều kiện hành động ban hành/phê duyệt/thông qua/công bố không bắt buộc ở bước quét hoặc trong tiêu đề; AI mới đánh giá việc ban hành khi bấm Đọc. Điều kiện nội dung riêng ngoài bốn hành động vẫn được giữ. Từ khóa tùy chỉnh không thuộc nhóm lõi vẫn dùng cụm do người dùng nhập.

Truy vấn không còn intitle hoặc bắt buộc hành động: tìm cụm chủ đề và các cách gọi văn bản trên website nguồn. Vẫn kiểm tra lại tiêu đề để chặn tin hoàn toàn ngoài chủ đề. Có thể giữ tin hội nghị/triển khai nếu rõ ràng nhắc văn bản CĐS; chưa coi đó là đã ban hành.

Mỗi lượt đi chéo qua nguồn và nhóm từ khóa, tiếp tục đủ mọi cặp theo con trỏ thay vì dành nhiều truy vấn liên tiếp chỉ cho một nguồn. Phạm vi ngày đăng không tự mở rộng.

Trạng thái NeedsReview (Cần xem nguồn) cũng tạo thông báo cho người bấm Đọc, tiêu đề phân biệt với tin đã nhận diện phù hợp. Mỗi kết quả chỉ thông báo một lần, mở đúng kết quả qua liên kết article/result; xem lại cache không gửi trùng. NotRelevant/Error không tạo thông báo này.
