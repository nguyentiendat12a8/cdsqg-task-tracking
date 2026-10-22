@echo off
setlocal
chcp 65001 >nul
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0seed_2629.ps1" %*
set "seedExitCode=%ERRORLEVEL%"
if "%~1"=="" pause
exit /b %seedExitCode%