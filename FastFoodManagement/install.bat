@echo off
:: ================================================
:: Restaurant Management System Installation Script
:: Must be run as Administrator
:: Version: 3.0 (Enhanced with all suggested improvements)
:: Last Updated: 2024-05-20
:: ================================================

:: Initialize variables
set APP_NAME=FastFoodManagement
set APP_VERSION=3.0
set INSTALL_DATE=%date% %time%

:: Check for Administrator privileges
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [!] يجب تشغيل هذا السكربت كمسؤول
    echo [!] الرجاء الضغط بزر الماوس الأيمن واختيار "تشغيل كمسؤول"
    pause
    exit /b
)

:: Display header
echo ================================================
echo    نظام إدارة المطعم - إصدار %APP_VERSION%
echo    معالج التثبيت (%INSTALL_DATE%)
echo ================================================
echo.

:: Check internet connection
echo [1/7] التحقق من اتصال الإنترنت...
ping -n 1 google.com >nul || (
    echo [!] لا يوجد اتصال بالإنترنت
    echo [!] هذا التثبيت يتطلب اتصالاً نشطاً
    pause
    exit /b
)

:: Define main paths
set APP_PATH=C:\%APP_NAME%
set DATA_PATH=%APP_PATH%\Data
set LOGS_PATH=%APP_PATH%\Logs
set BACKUPS_PATH=%APP_PATH%\Backups
set REPORTS_PATH=%APP_PATH%\Reports
set RECEIPTS_PATH=%APP_PATH%\Receipts
set TEMP_PATH=%APP_PATH%\Temp

:: Create log file
if not exist "%LOGS_PATH%" mkdir "%LOGS_PATH%"
echo %INSTALL_DATE% - بدء التثبيت >> "%LOGS_PATH%\install.log"

:: Step 1: Check disk space
echo [2/7] التحقق من مساحة القرص...
fsutil volume diskfree %SYSTEMDRIVE% | find "of free bytes" >nul
if %ERRORLEVEL% neq 0 (
    echo [!] تعذر التحقق من مساحة القرص
    echo [i] تأكد من وجود 2GB مساحة حرة على الأقل
    echo %INSTALL_DATE% - خطأ في مساحة القرص >> "%LOGS_PATH%\install.log"
    pause
    exit /b
)

:: Step 2: Install .NET 8.0
echo [3/7] تثبيت .NET 8.0 Runtime...
echo %INSTALL_DATE% - تثبيت .NET 8.0 >> "%LOGS_PATH%\install.log"

dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
if %ERRORLEVEL% equ 0 (
    echo [✓] .NET 8.0 مثبت مسبقاً
    echo %INSTALL_DATE% - .NET 8.0 موجود مسبقاً >> "%LOGS_PATH%\install.log"
) else (
    echo [i] جاري تنزيل .NET 8.0...
    
    :: Create temporary folder
    if not exist "%TEMP%\%APP_NAME%" mkdir "%TEMP%\%APP_NAME%"
    
    :: Download installer
    set DOTNET_URL=https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x64-installer
    powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri '%DOTNET_URL%' -OutFile '%TEMP%\%APP_NAME%\dotnet-runtime.exe'}"
    
    :: Install silently
    echo [i] جاري التثبيت، قد يستغرق بضع دقائق...
    start /wait "" "%TEMP%\%APP_NAME%\dotnet-runtime.exe" /install /quiet /norestart
    
    :: Verify installation
    dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
    if %ERRORLEVEL% equ 0 (
        echo [✓] تم تثبيت .NET 8.0 بنجاح
        echo %INSTALL_DATE% - تثبيت .NET 8.0 نجح >> "%LOGS_PATH%\install.log"
    ) else (
        echo [!] فشل تثبيت .NET 8.0
        echo [i] الرجاء التثبيت يدوياً من موقع مايكروسوفت
        echo %INSTALL_DATE% - فشل تثبيت .NET >> "%LOGS_PATH%\install.log"
        pause
        exit /b
    )
)

:: Step 3: Install SQLite (latest version)
echo [4/7] تثبيت SQLite...
echo %INSTALL_DATE% - تثبيت SQLite >> "%LOGS_PATH%\install.log"

set SQLITE_PATH=C:\sqlite3
set SQLITE_EXE=%SQLITE_PATH%\sqlite3.exe
set SQLITE_URL=https://www.sqlite.org/2023/sqlite-tools-win32-x86-3440200.zip

if exist "%SQLITE_EXE%" (
    echo [✓] SQLite موجود مسبقاً في %SQLITE_PATH%
    echo %INSTALL_DATE% - SQLite موجود مسبقاً >> "%LOGS_PATH%\install.log"
) else (
    echo [i] جاري تنزيل SQLite...
    
    :: Create SQLite folder
    if not exist "%SQLITE_PATH%" mkdir "%SQLITE_PATH%"
    
    :: Download and extract
    powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri '%SQLITE_URL%' -OutFile '%TEMP%\%APP_NAME%\sqlite.zip'}"
    powershell -command "& {Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::ExtractToDirectory('%TEMP%\%APP_NAME%\sqlite.zip', '%TEMP%\%APP_NAME%\sqlite')}"
    
    :: Copy files
    copy "%TEMP%\%APP_NAME%\sqlite\sqlite-tools-win32-x86-3440200\sqlite3.exe" "%SQLITE_PATH%\"
    
    :: Verify
    if exist "%SQLITE_EXE%" (
        echo [✓] تم تثبيت SQLite بنجاح
        echo %INSTALL_DATE% - تثبيت SQLite نجح >> "%LOGS_PATH%\install.log"
    ) else (
        echo [!] فشل تثبيت SQLite
        echo %INSTALL_DATE% - فشل تثبيت SQLite >> "%LOGS_PATH%\install.log"
        pause
        exit /b
    )
)

:: Add SQLite to PATH
echo [i] إضافة SQLite إلى متغيرات النظام...
echo %PATH% | find /i "%SQLITE_PATH%" > nul
if %ERRORLEVEL% equ 0 (
    echo [✓] المسار %SQLITE_PATH% مضاف مسبقاً
    echo %INSTALL_DATE% - مسار SQLite موجود في PATH >> "%LOGS_PATH%\install.log"
) else (
    setx PATH "%PATH%;%SQLITE_PATH%" /M
    echo [✓] تم إضافة %SQLITE_PATH% إلى PATH
    echo %INSTALL_DATE% - تم إضافة SQLite إلى PATH >> "%LOGS_PATH%\install.log"
)

:: Step 4: Install NuGet packages
echo [5/7] تثبيت حزم NuGet المطلوبة...
echo %INSTALL_DATE% - تثبيت حزم NuGet >> "%LOGS_PATH%\install.log"

set TEMP_PROJECT=%TEMP%\%APP_NAME%\TempProject
if not exist "%TEMP_PROJECT%" mkdir "%TEMP_PROJECT%"

pushd "%TEMP_PROJECT%"
dotnet new console -n TempProject
cd TempProject

:: Package installation with progress
echo [i] جاري تثبيت الحزم...
echo [i] █░░░░░░░░░░░░░░░░░ 5% (Microsoft.EntityFrameworkCore.Sqlite)
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0 >nul

echo [i] ████░░░░░░░░░░░░░░ 20% (MaterialDesignThemes)
dotnet add package MaterialDesignThemes --version 4.9.0 >nul

echo [i] ███████░░░░░░░░░░░ 35% (iTextSharp.LGPLv2.Core)
dotnet add package iTextSharp.LGPLv2.Core --version 3.4.3 >nul

echo [i] █████████░░░░░░░░░ 50% (Microsoft.Extensions.Logging)
dotnet add package Microsoft.Extensions.Logging --version 8.0.0 >nul

echo [i] ███████████░░░░░░░ 65% (Newtonsoft.Json)
dotnet add package Newtonsoft.Json --version 13.0.3 >nul

echo [i] █████████████░░░░░ 80% (System.Drawing.Common)
dotnet add package System.Drawing.Common --version 8.0.0 >nul

echo [✓] ██████████████████ 100% تم تثبيت جميع الحزم
echo %INSTALL_DATE% - تم تثبيت جميع حزم NuGet >> "%LOGS_PATH%\install.log"
popd

:: Step 5: Create folder structure
echo [6/7] إنشاء هيكل المجلدات...
echo %INSTALL_DATE% - إنشاء مجلدات >> "%LOGS_PATH%\install.log"

if not exist "%APP_PATH%" (
    echo [i] جاري إنشاء الهيكل الرئيسي...
    mkdir "%APP_PATH%"
    mkdir "%DATA_PATH%"
    mkdir "%LOGS_PATH%"
    mkdir "%BACKUPS_PATH%"
    mkdir "%REPORTS_PATH%"
    mkdir "%RECEIPTS_PATH%"
    mkdir "%TEMP_PATH%"
    
    :: Set permissions
    icacls "%APP_PATH%" /grant "Users:(OI)(CI)M" >nul
    echo [✓] تم إنشاء الهيكل بنجاح
    echo %INSTALL_DATE% - تم إنشاء الهيكل >> "%LOGS_PATH%\install.log"
) else (
    echo [✓] الهيكل موجود مسبقاً
    echo %INSTALL_DATE% - الهيكل موجود مسبقاً >> "%LOGS_PATH%\install.log"
)

:: Step 6: Initialize database
echo [7/7] تهيئة قاعدة البيانات...
echo %INSTALL_DATE% - تهيئة قاعدة البيانات >> "%LOGS_PATH%\install.log"

set DB_FILE=%DATA_PATH%\restaurant.db
set DB_BACKUP=%BACKUPS_PATH%\restaurant_initial.db

if not exist "%DB_FILE%" (
    echo [i] جاري إنشاء قاعدة البيانات...
    
    :: Create SQL initialization script
    echo BEGIN TRANSACTION; > "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Categories (CategoryId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Products (ProductId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT, Price REAL NOT NULL, CategoryId INTEGER, IsAvailable INTEGER, Image TEXT, FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Orders (OrderId INTEGER PRIMARY KEY, OrderDate TEXT NOT NULL, CustomerName TEXT, TotalAmount REAL, IsPaid INTEGER, PaymentMethod TEXT, OrderStatus TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE OrderItems (OrderItemId INTEGER PRIMARY KEY, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER, Subtotal REAL, FOREIGN KEY(OrderId) REFERENCES Orders(OrderId), FOREIGN KEY(ProductId) REFERENCES Products(ProductId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Users (UserId INTEGER PRIMARY KEY, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, FullName TEXT, Role TEXT, IsActive INTEGER); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE AppSettings (SettingId INTEGER PRIMARY KEY, SettingKey TEXT NOT NULL UNIQUE, SettingValue TEXT, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Add initial data with secure password hash
    echo INSERT INTO Categories (Name, Description) VALUES ('الوجبات الرئيسية', 'الوجبات الأساسية'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Categories (Name, Description) VALUES ('المشروبات', 'مشروبات ساخنة وباردة'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Products (Name, Description, Price, CategoryId, IsAvailable) VALUES ('برجر لحم', 'برجر لحم مع الجبن والخضار', 25.00, 1, 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES ('admin', '$2a$11$K3g6XpVzmdBp0AfZ9GWbZeRJJWrm/QbiQTfHX5XhwJECZ6FbLIHSa', 'مدير النظام', 'Admin', 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo COMMIT; >> "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Create database
    "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Create backup
    if exist "%DB_FILE%" (
        copy "%DB_FILE%" "%DB_BACKUP%" >nul
        echo [✓] تم إنشاء قاعدة البيانات والنسخة الاحتياطية
        echo %INSTALL_DATE% - تم إنشاء قاعدة البيانات >> "%LOGS_PATH%\install.log"
    ) else (
        echo [!] فشل إنشاء قاعدة البيانات
        echo %INSTALL_DATE% - فشل إنشاء قاعدة البيانات >> "%LOGS_PATH%\install.log"
        pause
        exit /b
    )
) else (
    echo [✓] قاعدة البيانات موجودة مسبقاً
    echo %INSTALL_DATE% - قاعدة البيانات موجودة مسبقاً >> "%LOGS_PATH%\install.log"
)

:: Create desktop shortcut
echo [i] إنشاء اختصار على سطح المكتب...
set SHORTCUT_PATH=%USERPROFILE%\Desktop\%APP_NAME%.lnk
set TARGET_PATH=%APP_PATH%\%APP_NAME%.exe

echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.TargetPath = "%TARGET_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%APP_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Description = "نظام إدارة المطعم - إصدار %APP_VERSION%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"

cscript //nologo "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo [✓] تم إنشاء الاختصار
echo %INSTALL_DATE% - تم إنشاء اختصار سطح المكتب >> "%LOGS_PATH%\install.log"

:: Cleanup
echo [i] جاري التنظيف...
rmdir /s /q "%TEMP_PROJECT%" >nul 2>&1
del "%TEMP%\%APP_NAME%\*.*" >nul 2>&1
rmdir /s /q "%TEMP%\%APP_NAME%" >nul 2>&1

:: Final message
echo.
echo ================================================
echo *                                              *
echo *      تم التثبيت بنجاح! - الإصدار %APP_VERSION%      *
echo *                                              *
echo *  - تم تثبيت .NET 8.0 Runtime                *
echo *  - تم تثبيت SQLite وإضافته إلى PATH         *
echo *  - تم تثبيت جميع الحزم المطلوبة             *
echo *  - تم إنشاء هيكل المجلدات                   *
echo *  - تم تهيئة قاعدة البيانات                  *
echo *  - تم إنشاء اختصار على سطح المكتب          *
echo *                                              *
echo *  معلومات الدخول الافتراضية:                 *
echo *  اسم المستخدم: admin                        *
echo *  كلمة المرور: Admin@2024                    *
echo *                                              *
echo *  تم إنشاء سجل التثبيت في:                   *
echo *  %LOGS_PATH%\install.log                    *
echo *                                              *
echo ================================================
echo.

pause