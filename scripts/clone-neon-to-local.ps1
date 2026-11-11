# Clone Neon (production) database into the local database the backend uses.
# - Neon is only read (pg_dump); nothing is written to Neon.
# - The current local database is backed up first, then kept under a *_old_<timestamp> name.
# - The Neon connection string is asked for at run time and never written to disk.
param(
    [string]$LocalDatabase = 'cdsqg_db',
    [string]$LocalPort = '5433',
    [string]$PgBin = 'C:\Program Files\PostgreSQL\18\bin',
    [string]$BackupDirectory = (Join-Path $env:USERPROFILE 'cdsqg-db-backups')
)
$ErrorActionPreference = 'Stop'

function Read-Secret([string]$prompt) {
    $secure = Read-Host -Prompt $prompt -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

function Parse-Connection([string]$value) {
    $value = $value.Trim().Trim('"').Trim("'")
    if ($value -match '^postgres(ql)?://') {
        $uri = [Uri]$value
        $userInfo = $uri.UserInfo.Split(':', 2)
        return @{
            Host     = $uri.Host
            Port     = $(if ($uri.Port -gt 0) { "$($uri.Port)" } else { '5432' })
            Database = [Uri]::UnescapeDataString($uri.AbsolutePath.TrimStart('/'))
            User     = [Uri]::UnescapeDataString($userInfo[0])
            Password = $(if ($userInfo.Count -gt 1) { [Uri]::UnescapeDataString($userInfo[1]) } else { '' })
        }
    }
    # Npgsql style: Host=...;Port=...;Database=...;Username=...;Password=...
    $map = @{}
    foreach ($part in $value.Split(';')) {
        $kv = $part.Split('=', 2)
        if ($kv.Count -eq 2) { $map[$kv[0].Trim().ToLowerInvariant()] = $kv[1].Trim() }
    }
    return @{
        Host     = $(if ($map['host']) { $map['host'] } else { $map['server'] })
        Port     = $(if ($map['port']) { $map['port'] } else { '5432' })
        Database = $map['database']
        User     = $(if ($map['username']) { $map['username'] } else { $map['user id'] })
        Password = $map['password']
    }
}

$names = @('PGHOST','PGPORT','PGDATABASE','PGUSER','PGPASSWORD','PGSSLMODE','PGCHANNELBINDING','PGCONNECT_TIMEOUT')
$saved = @{}
foreach ($n in $names) { $saved[$n] = [Environment]::GetEnvironmentVariable($n, 'Process') }

function Use-Conn($c, [string]$db, [string]$ssl) {
    $env:PGHOST = $c.Host; $env:PGPORT = $c.Port; $env:PGDATABASE = $db
    $env:PGUSER = $c.User; $env:PGPASSWORD = $c.Password
    $env:PGSSLMODE = $ssl; $env:PGCHANNELBINDING = $(if ($ssl -eq 'require') { 'require' } else { 'disable' })
    $env:PGCONNECT_TIMEOUT = '20'
}
function Invoke-Pg([string]$exe, [string[]]$arguments, [string]$errorMessage) {
    & (Join-Path $PgBin $exe) @arguments
    if ($LASTEXITCODE -ne 0) { throw $errorMessage }
}
# SQL is sent through stdin: Windows PowerShell 5.1 strips double quotes from native arguments.
function Sql([string]$sql) {
    $sql | & (Join-Path $PgBin 'psql.exe') -X -w -q -v ON_ERROR_STOP=1 -f -
    if ($LASTEXITCODE -ne 0) { throw "SQL loi: $sql" }
}
function SqlValue([string]$sql) {
    $out = & (Join-Path $PgBin 'psql.exe') -X -w -At -v ON_ERROR_STOP=1 -c $sql
    if ($LASTEXITCODE -ne 0) { throw "SQL loi: $sql" }
    return ($out | Out-String).Trim()
}

try {
    if (-not (Test-Path -LiteralPath (Join-Path $PgBin 'pg_dump.exe'))) { throw "Khong tim thay pg_dump trong $PgBin" }
    New-Item -ItemType Directory -Force -Path $BackupDirectory | Out-Null
    $stamp = Get-Date -Format 'yyyyMMdd_HHmmss'

    # ---- 1. Local target: the same DATABASE_URL the backend uses -------------------------
    $localRaw = [Environment]::GetEnvironmentVariable('DATABASE_URL', 'Process')
    if (-not $localRaw) { $localRaw = [Environment]::GetEnvironmentVariable('DATABASE_URL', 'User') }
    if (-not $localRaw) { $localRaw = [Environment]::GetEnvironmentVariable('DATABASE_URL', 'Machine') }
    if ($localRaw) {
        $local = Parse-Connection $localRaw
    } else {
        # Default: the local PostgreSQL 18 database the backend is using on this machine.
        $local = @{ Host = '127.0.0.1'; Port = $LocalPort; Database = $LocalDatabase; User = 'postgres'; Password = '' }
        $local.Password = Read-Secret "Nhap MAT KHAU user 'postgres' cua PostgreSQL tren may nay (khong phai Neon; bo trong roi Enter neu khong co)"
    }
    $localRaw = $null
    if ($local.Host -notin @('localhost','127.0.0.1','::1')) { throw "DB dich khong phai local ($($local.Host)). Dung lai de an toan." }
    if ($local.Database -notmatch '^[A-Za-z0-9_]+$') { throw "Ten DB local khong hop le: $($local.Database)" }
    $target = $local.Database
    Write-Host "DB local se bi thay the: $target tai $($local.Host):$($local.Port)" -ForegroundColor Yellow
    Use-Conn $local 'postgres' 'disable'
    & (Join-Path $PgBin 'psql.exe') -X -w -q -At -c 'SELECT 1;' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Khong dang nhap duoc PostgreSQL local (sai mat khau user postgres?). Chua thay doi gi.' }

    # ---- 2. Source: Neon ---------------------------------------------------------------
    $neonRaw = Read-Secret 'Dan chuoi ket noi Neon (postgresql://...)'
    $neon = Parse-Connection $neonRaw
    $neonRaw = $null
    if ($neon.Host -notlike '*.neon.tech') { throw 'Chuoi ket noi nguon khong phai Neon.' }
    $neon.Host = $neon.Host.Replace('-pooler.', '.')   # pg_dump should not go through the pooler

    Use-Conn $neon $neon.Database 'require'
    Write-Host "`n[1/5] Kiem tra ket noi Neon..." -ForegroundColor Cyan
    Sql "SELECT current_database() AS db, current_setting('server_version') AS version, pg_size_pretty(pg_database_size(current_database())) AS size;"

    $neonDump = Join-Path $BackupDirectory "neon_$stamp.dump"
    $recent = Get-ChildItem -LiteralPath $BackupDirectory -Filter 'neon_*.dump' -ErrorAction SilentlyContinue |
        Where-Object { $_.Length -gt 0 -and $_.LastWriteTime -gt (Get-Date).AddHours(-3) } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($recent -and (Read-Host "Da co ban tai Neon luc $($recent.LastWriteTime.ToString('HH:mm')) ($([math]::Round($recent.Length/1MB,1)) MB). Dung lai ban nay? (Y/N)") -match '^[Yy]') {
        $neonDump = $recent.FullName
    }
    if (-not (Test-Path -LiteralPath $neonDump)) {
    Write-Host "[2/5] Dang tai du lieu Neon ve $neonDump ..." -ForegroundColor Cyan
    Invoke-Pg 'pg_dump.exe' @('-w','-Fc','--verbose','--lock-wait-timeout=60s','--no-owner','--no-acl','--file',$neonDump) 'Tai du lieu Neon that bai. Chua dong gi vao DB local.'
    } else { Write-Host "[2/5] Dung lai ban tai Neon: $neonDump" -ForegroundColor Cyan }
    Invoke-Pg 'pg_restore.exe' @('--list',$neonDump) 'File dump Neon khong hop le.' | Out-Null

    # ---- 3. Back up the current local DB ----------------------------------------------
    Use-Conn $local 'postgres' 'disable'
    $exists = SqlValue "SELECT 1 FROM pg_database WHERE datname = '$target';"
    if ($exists -eq '1') {
        $localDump = Join-Path $BackupDirectory "local_${target}_before_$stamp.dump"
        Write-Host "[3/5] Sao luu DB local hien tai ve $localDump ..." -ForegroundColor Cyan
        Use-Conn $local $target 'disable'
        Invoke-Pg 'pg_dump.exe' @('-w','-Fc','--file',$localDump) 'Sao luu DB local that bai. Dung lai, chua thay doi gi.'
    } else {
        Write-Host "[3/5] DB local $target chua ton tai, bo qua sao luu." -ForegroundColor Cyan
    }

    # ---- 4. Restore into a fresh DB (old one untouched until this succeeds) ------------
    $newDb = "${target}_new_$stamp"
    Write-Host "[4/5] Dang dong du lieu vao DB tam $newDb ..." -ForegroundColor Cyan
    Use-Conn $local 'postgres' 'disable'
    Invoke-Pg 'createdb.exe' @('-w','--template','template0',$newDb) 'Khong tao duoc DB tam.'
    Use-Conn $local $newDb 'disable'
    & (Join-Path $PgBin 'pg_restore.exe') -w --exit-on-error --single-transaction --no-owner --no-acl --dbname $newDb $neonDump
    if ($LASTEXITCODE -ne 0) {
        Use-Conn $local 'postgres' 'disable'
        Sql "DROP DATABASE IF EXISTS `"$newDb`";"
        throw 'Dong du lieu that bai. DB local cu van giu nguyen.'
    }

    # ---- 5. Swap names ------------------------------------------------------------------
    Write-Host "[5/5] Thay the DB local..." -ForegroundColor Cyan
    Use-Conn $local 'postgres' 'disable'
    $oldDb = $null
    if ($exists -eq '1') {
        $oldDb = "${target}_old_$stamp"
        Sql "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$target' AND pid <> pg_backend_pid();"
        Sql "ALTER DATABASE `"$target`" RENAME TO `"$oldDb`";"
    }
    Sql "ALTER DATABASE `"$newDb`" RENAME TO `"$target`";"

    Use-Conn $local $target 'disable'
    Write-Host "`nSo dong mot so bang chinh trong DB local moi:" -ForegroundColor Green
    Sql @"
SELECT 'Agencies' AS bang, count(*) FROM "Agencies"
UNION ALL SELECT 'Users', count(*) FROM "Users"
UNION ALL SELECT 'GoalTaskItems', count(*) FROM "GoalTaskItems"
UNION ALL SELECT 'ProgressLogs', count(*) FROM "ProgressLogs";
"@

    Write-Host "`nXONG. DB local '$target' gio la ban sao cua Neon." -ForegroundColor Green
    Write-Host "File dump Neon : $neonDump"
    if ($oldDb) {
        Write-Host "DB local cu duoc giu lai voi ten: $oldDb (xoa trong pgAdmin khi khong can nua)"
        Write-Host "File sao luu DB local cu: $localDump"
    }
    Write-Host 'Khoi dong lai backend; backend se tu chay migration con thieu khi khoi dong.'
}
finally {
    foreach ($n in $names) { [Environment]::SetEnvironmentVariable($n, $saved[$n], 'Process') }
    $neon = $null; $local = $null
}
