@echo off
echo ===================================================
echo بناء نظام إدارة المطعم
echo ===================================================

echo.
echo بناء المشروع...
cd FastFoodManagement
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo فشل في بناء المشروع.
    pause
    exit /b 1
)

echo.
echo تم بناء المشروع بنجاح!
echo.
pause