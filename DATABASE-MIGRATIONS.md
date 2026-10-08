# Migration database

Schema được quản lý bởi EF migrations ở backend/Infrastructure/Data/Migrations.
Program.cs dùng Migrate; không còn EnsureCreated hay SQL sửa schema thủ công.
Migration lỗi sẽ dừng khởi động thay vì tiếp tục chạy với schema thiếu.

## Database mới

Đặt `CDSQG_MIGRATION_CONNECTION` rồi chạy:

```powershell
dotnet ef database update --project backend
```

Có thể review và áp dụng scripts/migrate.sql bằng psql. Script được sinh từ
migration và có kiểm tra lịch sử phiên bản. Không chỉnh sửa migration đã áp dụng.

## Database cũ chưa có lịch sử EF

1. Backup bằng pg_dump; thử khôi phục bản backup vào database riêng.
2. Chạy scripts/adopt-legacy-baseline.sql trên bản sao trước. Script kiểm tra 155
   cột/tên/kiểu của InitialSchema và chỉ ghi nhận phiên bản nếu tất cả khớp.
3. Nếu báo mismatch, đối chiếu model và schema thật để lập migration chuyển đổi
   riêng. Không bỏ qua kiểm tra và không dùng EnsureDeleted/EnsureCreated.
4. Kiểm tra thêm index, khóa ngoại và nullability trên schema cũ (script kiểm tra
   cột không chứng minh toàn bộ constraint đã tương đương).
5. Sau khi baseline thành công, áp dụng scripts/migrate.sql để thêm các cột reset.
   Kiểm tra đăng nhập, báo cáo, dashboard và dữ liệu đối chiếu trên bản sao.
6. Sau khi review kết quả và backup, mới áp dụng cùng quy trình lên database thật.

Các script chưa được chạy trên database thật trong đợt sửa này.

## Kiểm thử PostgreSQL

```powershell
$env:CDSQG_TEST_POSTGRES = 'Host=127.0.0.1;Port=55439;Database=postgres;Username=review'
dotnet test backend.Tests/backend.Tests.csproj
```

Dùng instance dành cho kiểm thử. Test tạo schema ngẫu nhiên, kiểm tra migration,
baseline, chạy lại migration, token hết hạn/dùng lại và hai request reset đồng thời,
rồi xóa đúng schema vừa tạo. Không đặt biến này trỏ database production.
Không có biến thì integration test được đánh dấu skipped rõ ràng.

Ngày giờ giữ `timestamp without time zone` và quy ước UTC hiện có để tương thích
schema cũ. Chuyển sang timestamptz cần một migration dữ liệu riêng.
