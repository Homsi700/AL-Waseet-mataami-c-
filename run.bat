@echo off
echo ===================================================
echo تشغيل نظام إدارة المطعم
echo ===================================================

echo.
echo بناء وتشغيل المشروع...
cd FastFoodManagement
dotnet run --no-build
if %ERRORLEVEL% NEQ 0 (
    echo فشل في تشغيل التطبيق.
    echo يمكنك محاولة بناء المشروع أولاً باستخدام: dotnet build
    pause
    exit /b 1
)

pause