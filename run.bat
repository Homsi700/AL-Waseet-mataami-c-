@echo off
echo ===================================================
echo تشغيل نظام إدارة المطعم
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
echo تشغيل التطبيق...
dotnet run
if %ERRORLEVEL% NEQ 0 (
    echo فشل في تشغيل التطبيق.
    pause
    exit /b 1
)

pause