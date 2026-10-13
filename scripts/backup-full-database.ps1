param(
    [Parameter(Mandatory = $true)][string]$Database,
    [Parameter(Mandatory = $true)][string]$BackupFile,
    [string]$DbHost = 'localhost',
    [int]$Port = 5433,
    [string]$Username = 'postgres',
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\18\bin'
)
$ErrorActionPreference = 'Stop'
$dumpTool = Join-Path $PostgresBin 'pg_dump.exe'
$restoreTool = Join-Path $PostgresBin 'pg_restore.exe'
if (!(Test-Path -LiteralPath $dumpTool) -or !(Test-Path -LiteralPath $restoreTool)) {
    throw 'PostgreSQL client tools not found. Set -PostgresBin.'
}
$destination = [IO.Path]::GetFullPath($BackupFile)
if (Test-Path -LiteralPath $destination) { throw 'Backup file already exists; choose a new filename.' }
$directory = Split-Path -Parent $destination
if (!(Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
Push-Location -LiteralPath $directory
try {
    $archiveName = Split-Path -Leaf $destination
    & $dumpTool --host=$DbHost --port=$Port --username=$Username --format=custom --file=$archiveName $Database
    if ($LASTEXITCODE -ne 0) { throw 'Backup failed. Do not use the incomplete file for restore.' }
    & $restoreTool --list $archiveName | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Backup archive cannot be read.' }
} finally { Pop-Location }
Write-Host "Full database backup saved: $destination"
Write-Host 'This includes database tables, data, constraints, indexes and sequences; uploaded files and server roles are separate.'
