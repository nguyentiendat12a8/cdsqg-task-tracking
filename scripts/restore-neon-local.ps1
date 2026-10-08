param(
    [string]$ConnectionFile = (Join-Path $PSScriptRoot '../scratch/neon-restore.env'),
    [string]$DatabaseName = ('cdsqg_neon_' + (Get-Date -Format 'yyyyMMdd_HHmmss')),
    [string]$BackupDirectory = (Join-Path $PSScriptRoot '../scratch')
)
$ErrorActionPreference = 'Stop'
if ($DatabaseName -notmatch '^cdsqg_neon_[a-z0-9_]+$') { throw 'Invalid destination database name.' }
$pgBin = 'C:\Program Files\PostgreSQL\18\bin'
$backupDir = [IO.Path]::GetFullPath($BackupDirectory)
$dump = Join-Path $backupDir ($DatabaseName + '.dump')
if (Test-Path -LiteralPath $dump) { throw 'Backup file already exists; refusing to overwrite.' }
$line = Get-Content -LiteralPath $ConnectionFile | Where-Object { $_ -match '^DATABASE_URL=' } | Select-Object -First 1
$value = ($line -replace '^DATABASE_URL=', '').Trim().Trim('"').Trim("'")
if ([string]::IsNullOrWhiteSpace($value)) { throw 'DATABASE_URL is missing.' }
$uri = [Uri]$value
if ($uri.Scheme -notin 'postgresql','postgres' -or $uri.Host -notlike '*.neon.tech') { throw 'Expected a Neon PostgreSQL connection.' }
$userInfo = $uri.UserInfo.Split(':',2)
if ($userInfo.Count -ne 2) { throw 'Connection must include username and password.' }
$names = @('PGHOST','PGPORT','PGDATABASE','PGUSER','PGPASSWORD','PGSSLMODE','PGCHANNELBINDING','PGCONNECT_TIMEOUT')
$saved = @{}
foreach ($name in $names) { $saved[$name] = [Environment]::GetEnvironmentVariable($name,'Process') }
try {
    # libpq credentials are passed through process environment, never command arguments.
    $env:PGHOST = $uri.Host.Replace('-pooler.','.')
    $env:PGPORT = if ($uri.Port -gt 0) { "$($uri.Port)" } else { '5432' }
    $env:PGDATABASE = [Uri]::UnescapeDataString($uri.AbsolutePath.TrimStart('/'))
    $env:PGUSER = [Uri]::UnescapeDataString($userInfo[0])
    $env:PGPASSWORD = [Uri]::UnescapeDataString($userInfo[1])
    $env:PGSSLMODE = 'require'
    $env:PGCHANNELBINDING = 'require'
    $env:PGCONNECT_TIMEOUT = '20'
    & "$pgBin\psql.exe" -X -w -v ON_ERROR_STOP=1 -c 'SELECT current_database(), current_setting(''server_version'') AS version, pg_size_pretty(pg_database_size(current_database())) AS size;'
    if ($LASTEXITCODE -ne 0) { throw 'Cannot connect to Neon.' }
    & "$pgBin\pg_dump.exe" -w -Fc --no-owner --no-acl --file $dump
    if ($LASTEXITCODE -ne 0) { throw 'Neon backup failed; no restore performed.' }
    & "$pgBin\pg_restore.exe" --list $dump | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Invalid backup archive.' }

    $env:PGHOST = '127.0.0.1'; $env:PGPORT = '55439'; $env:PGDATABASE = 'postgres'; $env:PGUSER = 'review'
    $env:PGPASSWORD = ''; $env:PGSSLMODE = 'disable'; $env:PGCHANNELBINDING = 'disable'
    & "$pgBin\createdb.exe" -w --template template0 $DatabaseName
    if ($LASTEXITCODE -ne 0) { throw 'Cannot create local destination; no existing database was overwritten.' }
    $env:PGDATABASE = $DatabaseName
    & "$pgBin\pg_restore.exe" -w --exit-on-error --single-transaction --no-owner --no-acl --dbname $DatabaseName $dump
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed; transaction rolled back. Backup is retained.' }
    & "$pgBin\psql.exe" -X -w -v ON_ERROR_STOP=1 -c 'SELECT table_name FROM information_schema.tables WHERE table_schema=''public'' AND table_type=''BASE TABLE'' ORDER BY table_name;'
    if ($LASTEXITCODE -ne 0) { throw 'Restore verification failed.' }
    Write-Output "LOCAL_DATABASE=$DatabaseName"
    Write-Output "BACKUP_FILE=$dump"
    Write-Output 'LOCAL_CONNECTION=Host=127.0.0.1;Port=55439;Username=review;Database=<LOCAL_DATABASE>'
} finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name,$saved[$name],'Process') }
    $value = $null; $userInfo = $null; $line = $null
}
