@echo off
echo ===================================================
echo تهيئة قاعدة بيانات نظام إدارة المطعم
echo ===================================================

echo.
echo التحقق من وجود أدوات Entity Framework...
dotnet tool list --global | findstr "dotnet-ef"
if %ERRORLEVEL% NEQ 0 (
    echo تثبيت أدوات Entity Framework Core...
    dotnet tool install --global dotnet-ef
    if %ERRORLEVEL% NEQ 0 (
        echo فشل في تثبيت أدوات Entity Framework Core.
        pause
        exit /b 1
    )
)

echo.
echo إنشاء هجرات قاعدة البيانات...
cd FastFoodManagement
dotnet ef migrations add InitialCreate
if %ERRORLEVEL% NEQ 0 (
    echo فشل في إنشاء هجرات قاعدة البيانات.
    pause
    exit /b 1
)

echo.
echo تطبيق الهجرات على قاعدة البيانات...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo فشل في تطبيق الهجرات على قاعدة البيانات.
    pause
    exit /b 1
)

echo.
echo تم تهيئة قاعدة البيانات بنجاح!
echo.
pause