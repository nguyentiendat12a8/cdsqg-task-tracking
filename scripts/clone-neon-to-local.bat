@echo off
echo ===================================================
echo   CLONE DB NEON VE LOCAL (thay the DB backend dang dung)
echo   Hay TAT backend truoc khi chay.
echo ===================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0clone-neon-to-local.ps1"
echo.
pause
