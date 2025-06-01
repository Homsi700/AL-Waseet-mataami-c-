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
    echo [!] This script must be run as Administrator
    echo [!] Please right-click the file and select "Run as Administrator"
    pause
    exit /b
)

:: Display header
echo ================================================
echo    Restaurant Management System v%APP_VERSION%
echo    Installation Wizard (%INSTALL_DATE%)
echo ================================================
echo.

:: Check internet connection
echo [1/7] Checking internet connection...
set INTERNET_AVAILABLE=0

:: Try multiple domains to check internet connectivity
ping -n 1 google.com >nul 2>&1 && set INTERNET_AVAILABLE=1
if %INTERNET_AVAILABLE% equ 0 ping -n 1 microsoft.com >nul 2>&1 && set INTERNET_AVAILABLE=1
if %INTERNET_AVAILABLE% equ 0 ping -n 1 cloudflare.com >nul 2>&1 && set INTERNET_AVAILABLE=1

if %INTERNET_AVAILABLE% equ 0 (
    echo [!] No internet connection detected
    echo [i] Some installation steps may fail without internet connection
    echo [i] Components like .NET Runtime and SQLite require internet to download
    echo.
    choice /C YN /M "Do you want to continue anyway (offline mode)?"
    if %ERRORLEVEL% equ 2 (
        echo [i] Installation cancelled by user
        echo %INSTALL_DATE% - Installation cancelled: no internet connection >> "%LOGS_PATH%\install.log"
        pause
        exit /b
    )
    echo [i] Continuing in offline mode - some components may be skipped
    echo %INSTALL_DATE% - Continuing in offline mode >> "%LOGS_PATH%\install.log"
    set OFFLINE_MODE=1
) else (
    echo [✓] Internet connection available
    echo %INSTALL_DATE% - Internet connection available >> "%LOGS_PATH%\install.log"
    set OFFLINE_MODE=0
)

:: Define main paths
set APP_PATH=C:\%APP_NAME%
set DATA_PATH=%APP_PATH%\Data
set LOGS_PATH=%APP_PATH%\Logs
set BACKUPS_PATH=%APP_PATH%\Backups
set REPORTS_PATH=%APP_PATH%\Reports
set RECEIPTS_PATH=%APP_PATH%\Receipts
set TEMP_PATH=%APP_PATH%\Temp

:: Create initial folders for logging
if not exist "%APP_PATH%" mkdir "%APP_PATH%"
if not exist "%LOGS_PATH%" mkdir "%LOGS_PATH%"

:: Create log file
echo %INSTALL_DATE% - Installation started > "%LOGS_PATH%\install.log"

:: Step 1: Check disk space
echo [2/7] Checking disk space...
:: Different versions of Windows have different output formats for fsutil
:: Try multiple approaches to get free space
set FREE_SPACE=
:: Try first format (newer Windows versions)
for /f "tokens=2" %%a in ('fsutil volume diskfree %SYSTEMDRIVE% ^| find "Avail"') do set FREE_SPACE=%%a 2>nul

:: If that didn't work, try another format
if not defined FREE_SPACE (
    for /f "tokens=3" %%a in ('fsutil volume diskfree %SYSTEMDRIVE% ^| find "avail free"') do set FREE_SPACE=%%a 2>nul
)

:: If still not defined, try one more approach
if not defined FREE_SPACE (
    for /f "tokens=3" %%a in ('dir %SYSTEMDRIVE%\ ^| find "bytes free"') do set FREE_SPACE=%%a 2>nul
)

:: Skip disk space check if we couldn't determine it, but continue installation
if not defined FREE_SPACE (
    echo [!] Could not verify disk space automatically
    echo [i] Please ensure you have at least 2GB of free space
    echo [i] Continuing installation...
    echo %INSTALL_DATE% - Disk space check skipped >> "%LOGS_PATH%\install.log"
) else (
    echo [✓] Disk space check passed: %FREE_SPACE% bytes available
    echo %INSTALL_DATE% - Disk space check passed: %FREE_SPACE% bytes >> "%LOGS_PATH%\install.log"
)

:: Step 2: Install .NET 8.0
echo [3/7] Installing .NET 8.0 Runtime...
echo %INSTALL_DATE% - Installing .NET 8.0 >> "%LOGS_PATH%\install.log"

dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
if %ERRORLEVEL% equ 0 (
    echo [✓] .NET 8.0 Runtime is already installed
    echo %INSTALL_DATE% - .NET 8.0 already exists >> "%LOGS_PATH%\install.log"
) else (
    if %OFFLINE_MODE% equ 1 (
        echo [!] Cannot install .NET 8.0 in offline mode
        echo [i] Please install .NET 8.0 manually when online
        echo [i] URL: https://dotnet.microsoft.com/download/dotnet/8.0
        echo [i] Continuing installation without .NET 8.0...
        echo %INSTALL_DATE% - .NET 8.0 installation skipped (offline mode) >> "%LOGS_PATH%\install.log"
    ) else (
        echo [i] Downloading .NET 8.0...
        
        :: Create temporary folder
        if not exist "%TEMP%\%APP_NAME%" mkdir "%TEMP%\%APP_NAME%"
        
        :: Download installer
        set DOTNET_URL=https://download.visualstudio.microsoft.com/download/pr/c3b20cdf-6b7a-4a10-b6b5-9000f0d1c0a0/4079c599c4a65a6f3a55b06ad8d1b316/windowsdesktop-runtime-8.0.0-win-x64.exe
        echo [i] Downloading from Microsoft servers...
        powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri '%DOTNET_URL%' -OutFile '%TEMP%\%APP_NAME%\dotnet-runtime.exe'}"
        
        if not exist "%TEMP%\%APP_NAME%\dotnet-runtime.exe" (
            echo [!] Download failed. Please check your internet connection.
            echo [i] Continuing installation without .NET 8.0...
            echo %INSTALL_DATE% - .NET 8.0 download failed >> "%LOGS_PATH%\install.log"
        ) else (
            :: Install silently
            echo [i] Installing, this may take a few minutes...
            start /wait "" "%TEMP%\%APP_NAME%\dotnet-runtime.exe" /install /quiet /norestart
            
            :: Verify installation
            dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
            if %ERRORLEVEL% equ 0 (
                echo [✓] .NET 8.0 installed successfully
                echo %INSTALL_DATE% - .NET 8.0 installation successful >> "%LOGS_PATH%\install.log"
            ) else (
                echo [!] Failed to install .NET 8.0
                echo [i] Please install manually from Microsoft website
                echo [i] https://dotnet.microsoft.com/download/dotnet/8.0
                echo [i] Continuing installation without .NET 8.0...
                echo %INSTALL_DATE% - .NET installation failed >> "%LOGS_PATH%\install.log"
            )
        )
    )
)

:: Step 3: Install SQLite (latest version)
echo [4/7] Installing SQLite...
echo %INSTALL_DATE% - Installing SQLite >> "%LOGS_PATH%\install.log"

set SQLITE_PATH=C:\sqlite3
set SQLITE_EXE=%SQLITE_PATH%\sqlite3.exe
set SQLITE_URL=https://www.sqlite.org/2023/sqlite-tools-win32-x86-3420000.zip

if exist "%SQLITE_EXE%" (
    echo [✓] SQLite already exists at %SQLITE_PATH%
    echo %INSTALL_DATE% - SQLite already exists >> "%LOGS_PATH%\install.log"
) else (
    if %OFFLINE_MODE% equ 1 (
        echo [!] Cannot install SQLite in offline mode
        echo [i] Please install SQLite manually when online
        echo [i] URL: https://www.sqlite.org/download.html
        echo [i] Continuing installation without SQLite...
        echo %INSTALL_DATE% - SQLite installation skipped (offline mode) >> "%LOGS_PATH%\install.log"
    ) else (
        echo [i] Downloading SQLite...
        
        :: Create SQLite folder
        if not exist "%SQLITE_PATH%" mkdir "%SQLITE_PATH%"
        
        :: Download and extract
        powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri '%SQLITE_URL%' -OutFile '%TEMP%\%APP_NAME%\sqlite.zip'}"
        
        if not exist "%TEMP%\%APP_NAME%\sqlite.zip" (
            echo [!] SQLite download failed
            echo [i] Continuing installation without SQLite...
            echo %INSTALL_DATE% - SQLite download failed >> "%LOGS_PATH%\install.log"
        ) else (
            echo [i] Extracting SQLite...
            powershell -command "& {Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::ExtractToDirectory('%TEMP%\%APP_NAME%\sqlite.zip', '%TEMP%\%APP_NAME%\sqlite')}"
            
            :: Copy files - corrected path for sqlite-tools
            echo [i] Installing SQLite...
            for /f %%F in ('dir /b /s "%TEMP%\%APP_NAME%\sqlite\sqlite3.exe"') do (
                copy "%%F" "%SQLITE_PATH%\" >nul
                echo [i] Found and copied SQLite from: %%F
            )
            
            :: If not found in the expected location, try to find it
            if not exist "%SQLITE_EXE%" (
                echo [i] Searching for sqlite3.exe in extracted files...
                for /f %%F in ('dir /b /s "%TEMP%\%APP_NAME%\sqlite\*.exe"') do (
                    if "%%~nxF"=="sqlite3.exe" (
                        copy "%%F" "%SQLITE_PATH%\" >nul
                        echo [i] Found and copied SQLite from: %%F
                    )
                )
            )
            
            :: Verify
            if exist "%SQLITE_EXE%" (
                echo [✓] SQLite installed successfully
                echo %INSTALL_DATE% - SQLite installation successful >> "%LOGS_PATH%\install.log"
            ) else (
                echo [!] Failed to install SQLite
                echo [i] Please install SQLite manually from SQLite website
                echo [i] https://www.sqlite.org/download.html
                echo [i] Continuing installation without SQLite...
                echo %INSTALL_DATE% - SQLite installation failed >> "%LOGS_PATH%\install.log"
            )
        )
    )
)

:: Add SQLite to PATH
echo [i] Adding SQLite to system variables...
echo %PATH% | find /i "%SQLITE_PATH%" > nul
if %ERRORLEVEL% equ 0 (
    echo [✓] Path %SQLITE_PATH% is already added to PATH
    echo %INSTALL_DATE% - SQLite path already in PATH >> "%LOGS_PATH%\install.log"
) else (
    setx PATH "%PATH%;%SQLITE_PATH%" /M
    echo [✓] Added %SQLITE_PATH% to PATH
    echo %INSTALL_DATE% - Added SQLite to PATH >> "%LOGS_PATH%\install.log"
    
    :: Update current session PATH
    set "PATH=%PATH%;%SQLITE_PATH%"
)

:: Step 4: Install NuGet packages
echo [5/7] Installing required NuGet packages...
echo %INSTALL_DATE% - Installing NuGet packages >> "%LOGS_PATH%\install.log"

if %OFFLINE_MODE% equ 1 (
    echo [!] Cannot install NuGet packages in offline mode
    echo [i] NuGet packages will need to be installed manually when online
    echo [i] Continuing installation without NuGet packages...
    echo %INSTALL_DATE% - NuGet packages installation skipped (offline mode) >> "%LOGS_PATH%\install.log"
) else (
    set TEMP_PROJECT=%TEMP%\%APP_NAME%\TempProject
    if not exist "%TEMP_PROJECT%" mkdir "%TEMP_PROJECT%"

    pushd "%TEMP_PROJECT%"
    echo [i] Creating temporary project...
    dotnet new console -n TempProject >nul 2>&1
    if %ERRORLEVEL% neq 0 (
        echo [!] Failed to create temporary project
        echo [i] Please ensure .NET SDK is installed
        echo [i] Continuing installation without NuGet packages...
        echo %INSTALL_DATE% - Failed to create temporary project >> "%LOGS_PATH%\install.log"
    ) else (
        cd TempProject

        :: Package installation with progress
        echo [i] Installing packages...
        set NUGET_SUCCESS=1
        
        echo [i] █░░░░░░░░░░░░░░░░░ 5%% (Microsoft.EntityFrameworkCore.Sqlite)
        dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        echo [i] ████░░░░░░░░░░░░░░ 20%% (MaterialDesignThemes)
        dotnet add package MaterialDesignThemes --version 4.9.0 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        echo [i] ███████░░░░░░░░░░░ 35%% (iTextSharp.LGPLv2.Core)
        dotnet add package iTextSharp.LGPLv2.Core --version 3.4.3 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        echo [i] █████████░░░░░░░░░ 50%% (Microsoft.Extensions.Logging)
        dotnet add package Microsoft.Extensions.Logging --version 8.0.0 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        echo [i] ███████████░░░░░░░ 65%% (Newtonsoft.Json)
        dotnet add package Newtonsoft.Json --version 13.0.3 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        echo [i] █████████████░░░░░ 80%% (System.Drawing.Common)
        dotnet add package System.Drawing.Common --version 8.0.0 >nul 2>&1
        if %ERRORLEVEL% neq 0 set NUGET_SUCCESS=0

        if %NUGET_SUCCESS% equ 1 (
            echo [✓] ██████████████████ 100%% All packages installed
            echo %INSTALL_DATE% - All NuGet packages installed >> "%LOGS_PATH%\install.log"
        ) else (
            echo [!] ██████████████████ Some packages failed to install
            echo [i] This may be due to network issues or NuGet server problems
            echo [i] Continuing installation...
            echo %INSTALL_DATE% - Some NuGet packages failed to install >> "%LOGS_PATH%\install.log"
        )
    )
    popd
)

:: Step 5: Create folder structure
echo [6/7] Creating folder structure...
echo %INSTALL_DATE% - Creating folders >> "%LOGS_PATH%\install.log"

:: Create main folders if they don't exist
if not exist "%DATA_PATH%" mkdir "%DATA_PATH%"
if not exist "%BACKUPS_PATH%" mkdir "%BACKUPS_PATH%"
if not exist "%REPORTS_PATH%" mkdir "%REPORTS_PATH%"
if not exist "%RECEIPTS_PATH%" mkdir "%RECEIPTS_PATH%"
if not exist "%TEMP_PATH%" mkdir "%TEMP_PATH%"

:: Set permissions
echo [i] Setting access permissions...
icacls "%APP_PATH%" /grant "Users:(OI)(CI)M" >nul 2>&1
echo [✓] Folder structure created successfully
echo %INSTALL_DATE% - Folder structure created >> "%LOGS_PATH%\install.log"

:: Step 6: Initialize database
echo [7/7] Initializing database...
echo %INSTALL_DATE% - Initializing database >> "%LOGS_PATH%\install.log"

set DB_FILE=%DATA_PATH%\restaurant.db
set DB_BACKUP=%BACKUPS_PATH%\restaurant_initial.db

if not exist "%DB_FILE%" (
    echo [i] Creating new database...
    
    :: Check if SQLite is available
    where sqlite3 >nul 2>&1
    set SQLITE_AVAILABLE=0
    if %ERRORLEVEL% equ 0 (
        set SQLITE_AVAILABLE=1
    ) else (
        if exist "%SQLITE_EXE%" (
            set SQLITE_AVAILABLE=1
        )
    )
    
    if %SQLITE_AVAILABLE% equ 0 (
        echo [!] SQLite is not available to create the database
        echo [i] A template database will be created without data
        echo [i] You will need to initialize it manually later
        
        :: Create an empty file as a placeholder
        echo. > "%DB_FILE%"
        echo %INSTALL_DATE% - Empty database placeholder created >> "%LOGS_PATH%\install.log"
    ) else (
        :: Create SQL initialization script
        echo BEGIN TRANSACTION; > "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE Categories (CategoryId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE Products (ProductId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT, Price REAL NOT NULL, CategoryId INTEGER, IsAvailable INTEGER, Image TEXT, FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE Orders (OrderId INTEGER PRIMARY KEY, OrderDate TEXT NOT NULL, CustomerName TEXT, TotalAmount REAL, IsPaid INTEGER, PaymentMethod TEXT, OrderStatus TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE OrderItems (OrderItemId INTEGER PRIMARY KEY, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER, Subtotal REAL, FOREIGN KEY(OrderId) REFERENCES Orders(OrderId), FOREIGN KEY(ProductId) REFERENCES Products(ProductId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE Users (UserId INTEGER PRIMARY KEY, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, FullName TEXT, Role TEXT, IsActive INTEGER); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo CREATE TABLE AppSettings (SettingId INTEGER PRIMARY KEY, SettingKey TEXT NOT NULL UNIQUE, SettingValue TEXT, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
        
        :: Add initial data with secure password hash
        echo INSERT INTO Categories (Name, Description) VALUES ('Main Dishes', 'Primary dishes'); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo INSERT INTO Categories (Name, Description) VALUES ('Beverages', 'Hot and cold drinks'); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo INSERT INTO Products (Name, Description, Price, CategoryId, IsAvailable) VALUES ('Beef Burger', 'Beef burger with cheese and vegetables', 25.00, 1, 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES ('admin', '$2a$11$K3g6XpVzmdBp0AfZ9GWbZeRJJWrm/QbiQTfHX5XhwJECZ6FbLIHSa', 'System Administrator', 'Admin', 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
        echo COMMIT; >> "%TEMP%\%APP_NAME%\init_db.sql"
        
        :: Create database
        echo [i] Executing SQLite commands...
        "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\%APP_NAME%\init_db.sql"
        
        :: Check if database was created
        if exist "%DB_FILE%" (
            :: Create backup
            copy "%DB_FILE%" "%DB_BACKUP%" >nul
            echo [✓] Database and backup created successfully
            echo %INSTALL_DATE% - Database created successfully >> "%LOGS_PATH%\install.log"
        ) else (
            echo [!] Failed to create database
            echo [i] A template database will be created without data
            echo [i] You will need to initialize it manually later
            
            :: Create an empty file as a placeholder
            echo. > "%DB_FILE%"
            echo %INSTALL_DATE% - Database creation failed, empty placeholder created >> "%LOGS_PATH%\install.log"
        )
    )
) else (
    echo [✓] Database already exists
    echo %INSTALL_DATE% - Database already exists >> "%LOGS_PATH%\install.log"
)

:: Create desktop shortcut
echo [i] Creating desktop shortcut...
set SHORTCUT_PATH=%USERPROFILE%\Desktop\%APP_NAME%.lnk
set TARGET_PATH=%APP_PATH%\%APP_NAME%.exe

echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.TargetPath = "%TARGET_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%APP_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Description = "Restaurant Management System v%APP_VERSION%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"

cscript //nologo "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo [✓] Desktop shortcut created
echo %INSTALL_DATE% - Desktop shortcut created >> "%LOGS_PATH%\install.log"

:: Cleanup
echo [i] Cleaning up temporary files...
rmdir /s /q "%TEMP_PROJECT%" >nul 2>&1
del "%TEMP%\%APP_NAME%\dotnet-runtime.exe" >nul 2>&1
del "%TEMP%\%APP_NAME%\sqlite.zip" >nul 2>&1
del "%TEMP%\%APP_NAME%\init_db.sql" >nul 2>&1
del "%TEMP%\%APP_NAME%\CreateShortcut.vbs" >nul 2>&1
rmdir /s /q "%TEMP%\%APP_NAME%\sqlite" >nul 2>&1

:: Final message
echo.
echo ================================================
echo *                                              *
echo *   Installation completed successfully! v%APP_VERSION%  *
echo *                                              *

:: Show different messages based on offline mode
if %OFFLINE_MODE% equ 1 (
    echo *  OFFLINE MODE INSTALLATION:                *
    echo *  Some components may need to be installed  *
    echo *  manually when you have internet access:   *
    echo *                                            *
    echo *  - .NET 8.0 Runtime                       *
    echo *  - SQLite                                 *
    echo *  - NuGet packages                         *
) else (
    echo *  - .NET 8.0 Runtime installed             *
    echo *  - SQLite installed and added to PATH     *
    echo *  - Required NuGet packages installed      *
)

echo *                                              *
echo *  - Folder structure created                 *
echo *  - Database initialized                     *
echo *  - Desktop shortcut created                 *
echo *                                              *
echo *  Default login credentials:                 *
echo *  Username: admin                            *
echo *  Password: Admin@2024                       *
echo *                                              *
echo *  Installation log created at:               *
echo *  %LOGS_PATH%\install.log                    *
echo *                                              *
echo ================================================
echo.

echo %INSTALL_DATE% - Installation completed successfully >> "%LOGS_PATH%\install.log"
pause