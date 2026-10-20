from pathlib import Path
p=Path('backend/Application/AI/AiAlertService.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('var feed=id.HasValue?await db.AiAlertFeeds.FindAsync([id.Value],ct)??throw new KeyNotFoundException():new AiAlertFeed();','''await using var tx=await db.Database.BeginTransactionAsync(ct);
        var feed=id.HasValue?await db.AiAlertFeeds.FromSqlInterpolated($"SELECT * FROM \\"AiAlertFeeds\\" WHERE \\"Id\\"={id.Value} FOR UPDATE").SingleOrDefaultAsync(ct)??throw new KeyNotFoundException():new AiAlertFeed();'''.replace('\\"','\\"'))
s=s.replace('await db.SaveChangesAsync(ct);return feed;','await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return feed;');p.write_text(s,encoding='utf-8')
p=Path('backend.Tests/SecurityTests.cs');s=p.read_text(encoding='utf-8-sig').replace('[InlineData("MasterData", "ResetProgressData", "DELETE")]', '[InlineData("MasterData", "ResetProgressData", "DELETE")]\n    [InlineData("AiAlerts", "Create", "POST")]\n    [InlineData("AiAlerts", "Scan", "POST")]\n    [InlineData("AiAlerts", "List", "GET")]');p.write_text(s,encoding='utf-8')
p=Path('backend.Tests/AiAlertsTests.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('Assert.NotNull(await service.EnqueueAsync(feed.Id,"Manual","Admin",default)); // Manual works with schedule off.', '''await using var secondDb=new AppDbContext(options);
            var otherService=new AiAlertService(secondDb,feedReader,inbox);
            var parallel=await Task.WhenAll(service.EnqueueAsync(feed.Id,"Manual","Admin",default),otherService.EnqueueAsync(feed.Id,"Manual","Admin",default));
            Assert.Single(parallel.Where(s=>s!=null)); // Manual works with schedule off; concurrent requests produce one job.
            var pending=parallel.Single(s=>s!=null)!;
            await new AiAlertService(db,new BrokenFeedReader(),inbox).RunAsync(pending.Id,default);
            await db.Entry(pending).ReloadAsync();Assert.Equal("Error",pending.Status);Assert.Null(pending.LeaseUntil);
            Assert.Equal(0,await db.ProgressLogs.CountAsync());''');i=s.index('    private sealed class FeedReader:');s=s[:i]+'''    private sealed class BrokenFeedReader:IAiAlertFeedReader
    {
        public Task<List<AlertFeedEntry>> ReadAsync(string url,CancellationToken ct)=>throw new InvalidOperationException("RSS test failure");
    }
'''+s[i:];p.write_text(s,encoding='utf-8')
p=Path('docs/AI-PUBLIC-DATA.md');s=p.read_text(encoding='utf-8-sig');a=s.index('## Chế độ hiện hành');b=s.index('## Phạm vi giai đoạn đầu',a)
s=s[:a]+'''## Chế độ hiện hành (19/10/2026)

Hai đầu vào dùng chung kho AI: admin nạp PDF/DOCX/TXT, hoặc Google Alerts RSS nhận tiêu đề. RSS có lịch hằng ngày theo giờ Việt Nam và Quét ngay. AI chỉ đánh giá tiêu đề; chỉ bài liên quan mới được tải và đọc. Tiêu đề chưa rõ chờ admin xem. Không có cấu hình AI thì vẫn nhận RSS và giữ hàng đợi, không gọi AI.

Quét website diện rộng vẫn tắt: `AiCollectionWorker` không đăng ký, không khởi động lại lịch `AiDataSources` cũ. RSS chỉ đọc chính URL bài được Alerts thông báo trong danh sách nguồn cho phép; không tự theo liên kết trong bài. Google Sheets/Gmail Apps Script không cần dùng cho luồng mới; hàng đợi bridge cũ chưa gắn RSS vẫn không tự xử lý.

'''+s[b:]
s=s.replace('Chỉ số hạng dùng đơn vị RANK/Hạng/Thứ hạng; hạng càng nhỏ càng tốt.', 'Hai loại đơn vị người dùng là phần trăm và số lượng. MT-12/MT-25 dùng SL; hệ thống giữ cách đánh giá top thứ hạng, không đổi giá trị thực tế 71 thành tỷ lệ.')
a=s.index('Chỉ Admin được truy cập menu');b=s.index('\n\n## Cấu hình',a);s=s[:a]+'''Chỉ Admin truy cập UI và API. Nhóm Quản trị dữ liệu AI có Tài liệu AI, Nguồn Google Alerts, Kiểm chứng số liệu và Cấu hình AI. Nguồn Google Alerts có ba tab: nguồn RSS, tiêu đề đã nhận, lịch sử quét. Tài liệu AI vẫn gồm nạp file, lịch sử xử lý, mục tiêu áp dụng. Kiểm chứng hiển thị số liệu, nguồn, trang, mục tiêu liên kết; ghi chú tùy chọn, xác nhận kiểm chứng bắt buộc trước khi duyệt. Không tự tạo lại mục tiêu đã xóa.'''+s[b:]
s=s.replace('`AddAiProviderConfiguration` bằng', '`AddAiProviderConfiguration`, `AddAiAlertRssSources` bằng')
s+='''

## Google Alerts RSS: cấu hình và vận hành

1. Trong Google Alerts, tạo cảnh báo theo chủ đề, chọn Deliver to → RSS feed. Dùng truy vấn có `site:` để giới hạn các website uy tín tương ứng. Sao chép URL biểu tượng RSS (dạng `https://www.google.com/alerts/feeds/<id>/<id>`).
2. Vào Quản trị dữ liệu AI → Nguồn Google Alerts → Thêm nguồn RSS, đặt tên và URL. Mỗi nguồn được bật/tắt lịch, đặt giờ HH:mm theo Việt Nam (UTC+7); mặc định 08:00, lịch tắt khi mới tạo. Không thay URL RSS bằng API key.
3. Quét ngay xếp một job bền trong DB, không giữ HTTP request chờ AI. Worker kiểm tra mỗi 30 giây. Lịch chạy một lần/ngày; sau downtime chỉ quét bù một lượt. Manual không thay đổi mốc lịch ngày tiếp theo.
4. Lần đầu nhận tối đa 200 entry hiện có trong RSS. Lần sau nhận toàn bộ entry hiện còn trong RSS và bỏ trùng URL chuẩn hóa, không chỉ cắt theo ngày để tránh bỏ tin đến muộn. RSS không bảo đảm Google tìm đủ mọi bài; entry đã rời feed trong thời gian hệ thống ngừng có thể không nhận được.
5. Chỉ nhận bài HTTPS thuộc `AiMetricCatalog.AllowedHosts`. Link google.com/url được bóc tham số URL, không truy cập bộ chuyển hướng của Google. Bài ngoài allowlist hoặc tiêu đề trống/quá dài được ghi số lượng bỏ qua trong lượt quét, không gọi AI.
6. Phân loại mỗi tiêu đề mới một lần: liên quan → đọc URL bài; chưa rõ → NeedsReview chờ admin chọn chỉ số hoặc đánh giá lại tiêu đề; không liên quan → NotRelevant, không tải nội dung. Nội dung link được kiểm tra lại domain và IP công khai, giới hạn tải, redirect, PDF có chữ. Chỉ đọc bài/link PDF chính xác, không tự tìm file đính kèm trong bài.
7. `AiAlertFeeds` giữ cấu hình và lịch; `AiAlertScans` giữ người gọi, trigger, trạng thái, số đã nhận/mới/trùng/bỏ qua/lỗi. `AiInboxItems.AlertFeedId` và PublishedAt ghi nguồn RSS và ngày bài nếu feed cung cấp. Unique URL hash chống trùng giữa nguồn; khóa hàng và unique pending scan chống quét song song; lease phục hồi sau khởi động lại. Tắt lịch chỉ dừng nhận RSS tự động, không hủy các tài liệu đang chờ AI.
8. Đọc bài thất bại giữ Error và có nút đọc/đánh giá lại, không tự thử vô hạn hoặc ghi báo cáo. Admin vẫn cần duyệt số liệu. Lịch chỉ chạy khi BE hoạt động; hosting ngủ hoặc tắt máy local sẽ trễ giờ quét.

Kiểm thử: schema PostgreSQL tạm cho migration, lịch theo giờ VN, manual/scheduled trùng nhau, URL trùng giữa feed, phân loại tiêu đề → đọc đúng bài liên quan, uncertain/unrelated không fetch, không tạo ProgressLog. Parser Atom/RSS chặn DTD/XXE và giới hạn kích thước; browser test nguồn, giờ quét, job pending, tiêu đề, lịch sử và mobile. Chưa kiểm thử feed tài khoản thật nếu chưa cung cấp URL RSS.
''';p.write_text(s,encoding='utf-8')
