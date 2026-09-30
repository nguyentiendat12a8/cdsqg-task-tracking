# ==============================================================================
# Script Reset Tiến Độ, Báo Cáo, Đôn Đốc & Thông Báo Hệ Thống CDSQG
# Giữ nguyên danh sách Mục tiêu, Nhiệm vụ đã khởi tạo & Cài đặt hệ thống.
# ==============================================================================

Param(
    [string]$ApiUrl = "http://localhost:5000/api/masterdata/reset-progress"
)

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Đang thực hiện Reset tiến độ, báo cáo & thông báo hệ thống..." -ForegroundColor Yellow
Write-Host "==================================================================" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $ApiUrl -Method Post -ContentType "application/json" -ErrorAction Stop
    if ($response.success) {
        Write-Host " SUCCESS: $($response.message)" -ForegroundColor Green
    } else {
        Write-Host " RESPONSE: $($response | ConvertTo-Json)" -ForegroundColor Yellow
    }
} catch {
    Write-Host " Lỗi khi kết nối đến API backend ($ApiUrl): $_" -ForegroundColor Red
    Write-Host " Hãy đảm bảo Backend (dotnet run) đang chạy ở http://localhost:5000" -ForegroundColor Yellow
}
