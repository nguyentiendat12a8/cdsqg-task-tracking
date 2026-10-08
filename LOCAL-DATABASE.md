# Bản sao Neon local

Đã phục hồi ngày 08/10/2026 vào PostgreSQL 18, chỉ lắng nghe loopback.

```text
Host=127.0.0.1;Port=55439;Database=cdsqg_neon_20261008_225107;Username=review
```

File sao lưu: `C:\Users\84878\Documents\ChatGPT\CDSQG\scratch\cdsqg_neon_20261008_225107.dump`.
Cluster: `C:\Users\84878\Documents\ChatGPT\CDSQG\scratch\pg-review`.

Restore đủ 12 bảng. Đối chiếu sau restore khớp với Neon: Agencies 106, Users 5,
GoalTaskItems 82, ProgressLogs 1, AgencyTaskExecutions 1.
Không thay đổi dữ liệu Neon. Mật khẩu Neon đã được xóa khỏi file kết nối tạm.

Bản sao giữ schema gốc, chưa áp dụng migration mới. Trước khi chạy backend tự
migrate trên bản sao này, thực hiện quy trình nhận baseline trong
DATABASE-MIGRATIONS.md và kiểm tra schema tương thích.

Khởi động cluster nếu đã tắt (PowerShell):

```powershell
& 'C:\Program Files\PostgreSQL\18\bin\pg_ctl.exe' -D 'C:\Users\84878\Documents\ChatGPT\CDSQG\scratch\pg-review' -l 'C:\Users\84878\Documents\ChatGPT\CDSQG\scratch\pg-review.log' -o '-p 55439 -h 127.0.0.1' start
```

Đây là cluster phát triển riêng sử dụng trust authentication trên loopback.
Không mở cổng này ra mạng.
