param(
    [Parameter(Mandatory = $true)][string]$Database,
    [Parameter(Mandatory = $true)][string]$BackupFile,
    [string]$DbHost = 'localhost',
    [int]$Port = 5433,
    [string]$Username = 'postgres',
    [string]$MaintenanceDatabase = 'postgres',
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\18\bin',
    [Parameter(Mandatory = $true)][switch]$ReplaceDatabase
)
$ErrorActionPreference = 'Stop'
if (!$ReplaceDatabase) { throw 'Explicit -ReplaceDatabase is required.' }
if ($Database -in @('postgres', 'template0', 'template1')) { throw 'System databases cannot be replaced.' }
if ($MaintenanceDatabase -eq $Database) { throw 'Maintenance database must differ from the database being restored.' }
$restoreTool = Join-Path $PostgresBin 'pg_restore.exe'
$psqlTool = Join-Path $PostgresBin 'psql.exe'
if (!(Test-Path -LiteralPath $restoreTool) -or !(Test-Path -LiteralPath $psqlTool)) {
    throw 'PostgreSQL client tools not found. Set -PostgresBin.'
}
$archive = [IO.Path]::GetFullPath($BackupFile)
if (!(Test-Path -LiteralPath $archive -PathType Leaf)) { throw 'Backup file not found.' }
Push-Location -LiteralPath (Split-Path -Parent $archive)
try {
$archiveName = Split-Path -Leaf $archive
& $restoreTool --list $archiveName | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Backup archive cannot be read. Database untouched.' }
# Drop/recreate is required to also remove arbitrary NEW demo tables/schemas.
# Use psql variable quoting for identifiers and literals, never interpolate SQL.
# Stop backend and disconnect database GUI sessions before running this script.
$sql = @'
SELECT format('DROP DATABASE %I', :'target_db') \gexec
SELECT format('CREATE DATABASE %I TEMPLATE template0', :'target_db') \gexec
'@
$sql | & $psqlTool -X --host=$DbHost --port=$Port --username=$Username --dbname=$MaintenanceDatabase --set=ON_ERROR_STOP=1 --set="target_db=$Database"
if ($LASTEXITCODE -ne 0) { throw 'Database recreation failed. Check active connections and permissions.' }
& $restoreTool --host=$DbHost --port=$Port --username=$Username --dbname=$Database --single-transaction --exit-on-error $archiveName
if ($LASTEXITCODE -ne 0) { throw 'Restore failed. Keep the archive and retry after resolving the error; do not start backend.' }
Write-Host "Full database restored from: $archive"
} finally { Pop-Location }
