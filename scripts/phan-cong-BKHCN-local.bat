@echo off
chcp 65001 >nul
echo ===================================================
echo   PHAN CONG DON VI THUOC BO KH^&CN (QD 2629 + QD 1266) - DB LOCAL
echo   1) Gan don vi truc thuoc   2) Bo sung 16 nhiem vu 1266 con thieu
echo ===================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0phan-cong-BKHCN-local.ps1" %*
echo.
pause
