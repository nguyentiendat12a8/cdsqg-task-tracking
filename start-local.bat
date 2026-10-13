@echo off
chcp 65001 >nul
echo ===================================================
echo   KHỞI ĐỘNG HỆ THỐNG CDSQG (BACKEND VÀ FRONTEND)
echo ===================================================
echo.
echo 1. Đang mở cửa sổ chạy Backend (.NET Web API - Port 5000)...
start "CDSQG - Backend (Port 5000)" cmd /k "cd /d "%~dp0backend" && dotnet run"

timeout /t 3 /nobreak >nul

echo 2. Đang mở cửa sổ chạy Frontend (Vite/Vue 3 - Port 5173)...
start "CDSQG - Frontend (Port 5173)" cmd /k "cd /d "%~dp0frontend" && npm run dev"

echo.
echo ===================================================
echo   Đã bật 2 cửa sổ tiến trình Backend và Frontend!
echo   Frontend: http://localhost:5173
echo   Backend:  http://localhost:5000
echo ===================================================
pause
