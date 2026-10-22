param(
    [string]$Database = 'cdsqg_db',
    [string]$DbHost = 'localhost',
    [int]$Port = 5433,
    [string]$Username = 'postgres',
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\18\bin',
    [switch]$AssignmentsOnly
)

$ErrorActionPreference = 'Stop'
$psqlTool = Join-Path $PostgresBin 'psql.exe'

if (!(Test-Path -LiteralPath $psqlTool)) {
    throw "Không tìm thấy psql.exe tại: $PostgresBin"
}

$sqlFile = Join-Path $PSScriptRoot 'seed_2629_data.sql'
if (!(Test-Path -LiteralPath $sqlFile)) {
    throw "Không tìm thấy file seed: $sqlFile"
}

if (-not $env:PGPASSWORD) {
    $env:PGPASSWORD = "1"
}
$env:PGCLIENTENCODING = "UTF8"

Write-Host "Đang xử lý QĐ 2629/QĐ-TTg trên database $Database (${DbHost}:${Port})..." -ForegroundColor Cyan

Push-Location $PSScriptRoot
try {
    $backendProject = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../backend/backend.csproj'))
    # Isolate both obj and bin: copied static-web-assets caches contain absolute source paths.
    $maintenanceArtifacts = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../scratch/seed-maintenance-artifacts'))
    & dotnet build $backendProject --artifacts-path $maintenanceArtifacts --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'Build backend thất bại; chưa nạp lại dữ liệu SQL.' }
    if (-not $AssignmentsOnly) {
        & $psqlTool -X --host=$DbHost --port=$Port --username=$Username --dbname=$Database --set=ON_ERROR_STOP=1 -f seed_2629_data.sql
    }
    if ($LASTEXITCODE -eq 0) {
        # Use the same assignment writer as application create/edit; no duplicate SQL expansion.
        $backendProject = Join-Path $PSScriptRoot '../backend/backend.csproj'
        Push-Location (Split-Path $backendProject)
        $previousConnection = $env:ConnectionStrings__DefaultConnection
        $previousDatabaseUrl = $env:DATABASE_URL
        $seedConnection = [System.Data.Common.DbConnectionStringBuilder]::new()
        $seedConnection['Host'] = $DbHost; $seedConnection['Port'] = $Port
        $seedConnection['Database'] = $Database; $seedConnection['Username'] = $Username
        $seedConnection['Password'] = $env:PGPASSWORD
        try {
            $env:ConnectionStrings__DefaultConnection = $seedConnection.ConnectionString
            Remove-Item Env:DATABASE_URL -ErrorAction SilentlyContinue
            & dotnet run --project $backendProject --no-build --artifacts-path $maintenanceArtifacts --launch-profile http -- --backfill-missing-assignments
            if ($LASTEXITCODE -ne 0) { throw 'Không khởi tạo được phân giao sau seed.' }
        } finally { $env:ConnectionStrings__DefaultConnection = $previousConnection; $env:DATABASE_URL = $previousDatabaseUrl; Pop-Location }

        if ($AssignmentsOnly) { Write-Host ">>> Đã khởi tạo các phân giao còn thiếu; không nạp lại SQL." -ForegroundColor Green }
        else { Write-Host ">>> NẠP DỮ LIỆU VÀ KHỞI TẠO PHÂN GIAO THÀNH CÔNG!" -ForegroundColor Green }
    } else {
        throw "Có lỗi khi nạp dữ liệu. Exit code: $LASTEXITCODE"
    }
} finally {
    Pop-Location
}
