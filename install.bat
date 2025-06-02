@echo off
echo ===================================================
echo تثبيت متطلبات نظام إدارة المطعم
echo ===================================================

echo.
echo التحقق من وجود .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK غير مثبت. يرجى تثبيت .NET 8.0 SDK من الموقع الرسمي:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo بعد التثبيت، قم بإعادة تشغيل هذا الملف.
    pause
    exit /b 1
)

echo .NET SDK موجود. التحقق من الإصدار...
dotnet --version
echo.

echo تثبيت حزم NuGet المطلوبة...
cd FastFoodManagement
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo فشل في تثبيت حزم NuGet.
    pause
    exit /b 1
)

echo.
echo إنشاء مجلد قاعدة البيانات...
mkdir FastFoodDB 2>nul

echo.
echo تهيئة قاعدة البيانات...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo ملاحظة: قد تحتاج إلى تثبيت أدوات Entity Framework Core:
    echo dotnet tool install --global dotnet-ef
    echo.
    echo ثم قم بتشغيل: dotnet ef database update
)

echo.
echo تم الانتهاء من التثبيت بنجاح!
echo يمكنك الآن تشغيل المشروع باستخدام ملف run.bat
echo.
pause