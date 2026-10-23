# Script xóa dữ liệu bài viết RSS đã quét để phục vụ demo
$ErrorActionPreference = "Stop"

$psql = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
if (-not (Test-Path $psql)) {
    $psql = "psql"
}

$env:PGPASSWORD = "1"
$sqlFile = Join-Path $PSScriptRoot "clear-rss-demo.sql"

Write-Host "Đang xóa dữ liệu bài viết RSS và lịch sử quét..." -ForegroundColor Cyan
Get-Content $sqlFile | & $psql -U postgres -p 5432 -d cdsqg_db

Write-Host "Xóa thành công! Bạn có thể bắt đầu demo quét RSS mới." -ForegroundColor Green
