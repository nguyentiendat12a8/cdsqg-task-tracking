# Chay 2 script phan cong don vi thuoc Bo KH&CN tren DB LOCAL, dung thu tu:
#   1. gan_don_vi_truc_thuoc_BKHCN_2629_1266.sql  (gan don vi truc thuoc cho muc tieu, nhiem vu da co + 3 danh muc dac biet)
#   2. bo_sung_nhiem_vu_1266_con_thieu.sql        (them 16 nhiem vu QD 1266 con thieu, gan dung don vi)
# - Chi chay tren localhost. Mat khau chi nhap khi chay, khong ghi ra dia.
# - Ca 2 script chay lai nhieu lan van an toan (bo qua phan da co).
param(
    [string]$Database = 'cdsqg_db',
    [string]$Port = '5433',
    [string]$User = 'postgres',
    [string]$PgBin = 'C:\Program Files\PostgreSQL\18\bin'
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8

function Read-Secret([string]$prompt) {
    $secure = Read-Host -Prompt $prompt -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

$psql = Join-Path $PgBin 'psql.exe'
if (-not (Test-Path $psql)) { throw "Khong tim thay psql.exe tai $PgBin. Truyen -PgBin '<thu muc bin PostgreSQL>'." }
$scripts = @(
    (Join-Path $PSScriptRoot 'gan_don_vi_truc_thuoc_BKHCN_2629_1266.sql'),
    (Join-Path $PSScriptRoot 'bo_sung_nhiem_vu_1266_con_thieu.sql')
)
foreach ($s in $scripts) { if (-not (Test-Path $s)) { throw "Thieu file $s" } }

Write-Host "DB dich: $Database tai 127.0.0.1:$Port (user $User)" -ForegroundColor Yellow
$env:PGPASSWORD = Read-Secret "Nhap MAT KHAU user '$User' cua PostgreSQL local (bo trong roi Enter neu khong co)"
$env:PGCLIENTENCODING = 'UTF8'
$common = @('-h', '127.0.0.1', '-p', $Port, '-U', $User, '-d', $Database, '-v', 'ON_ERROR_STOP=1', '-P', 'pager=off', '-X')

try {
    # Kiem tra ket noi va cot SubAgencyIds (do migration cua backend moi tao ra).
    $check = & $psql @common -tA -c "SELECT count(*) FROM information_schema.columns WHERE table_name = 'GoalTaskItems' AND column_name = 'SubAgencyIds';"
    if ($LASTEXITCODE -ne 0) { throw "Khong ket noi duoc DB (sai mat khau, port hoac ten DB?)." }
    if ("$check".Trim() -ne '1') {
        throw "DB chua co cot SubAgencyIds. Hay build va chay backend moi 1 lan (start-local.bat) de tu ap migration, tat backend roi chay lai."
    }

    $i = 0
    foreach ($s in $scripts) {
        $i++
        Write-Host ""
        Write-Host "[$i/2] Dang chay $(Split-Path $s -Leaf) ..." -ForegroundColor Cyan
        & $psql @common -f $s
        if ($LASTEXITCODE -ne 0) { throw "Script $(Split-Path $s -Leaf) loi; da rollback, DB khong doi o buoc nay." }
    }

    Write-Host ""
    Write-Host "Tong hop sau khi chay:" -ForegroundColor Green
    # Gui SQL qua stdin: PowerShell 5.1 lam mat dau " khi truyen tham so cho psql.
    @"
SELECT d."DocumentNumber" AS van_ban,
       count(*) FILTER (WHERE g."ItemType" = 'Goal') AS muc_tieu,
       count(*) FILTER (WHERE g."ItemType" = 'Task') AS nhiem_vu,
       count(*) FILTER (WHERE jsonb_array_length(g."SubAgencyIds") > 0) AS da_gan_don_vi_thuoc_Bo
FROM "GoalTaskItems" g JOIN "Documents" d ON d."Id" = g."DocumentId"
GROUP BY d."DocumentNumber" ORDER BY d."DocumentNumber";
"@ | & $psql @common
    Write-Host ""
    Write-Host "Xong. Khoi dong lai backend local de kiem tra tren giao dien." -ForegroundColor Green
}
finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
