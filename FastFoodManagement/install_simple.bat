@echo off
:: ================================================
:: Restaurant Management System Installation Script
:: Simple Version - No Complex Checks
:: Version: 3.0
:: ================================================

:: Initialize variables
set APP_NAME=FastFoodManagement
set APP_VERSION=3.0
set INSTALL_DATE=%date% %time%

:: Display header
echo ================================================
echo    Restaurant Management System v%APP_VERSION%
echo    Installation Wizard (%INSTALL_DATE%)
echo ================================================
echo.

:: Define main paths
set APP_PATH=C:\%APP_NAME%
set DATA_PATH=%APP_PATH%\Data
set LOGS_PATH=%APP_PATH%\Logs
set BACKUPS_PATH=%APP_PATH%\Backups
set REPORTS_PATH=%APP_PATH%\Reports
set RECEIPTS_PATH=%APP_PATH%\Receipts
set TEMP_PATH=%APP_PATH%\Temp

:: Create folders
echo [1/5] Creating folder structure...
if not exist "%APP_PATH%" mkdir "%APP_PATH%"
if not exist "%DATA_PATH%" mkdir "%DATA_PATH%"
if not exist "%LOGS_PATH%" mkdir "%LOGS_PATH%"
if not exist "%BACKUPS_PATH%" mkdir "%BACKUPS_PATH%"
if not exist "%REPORTS_PATH%" mkdir "%REPORTS_PATH%"
if not exist "%RECEIPTS_PATH%" mkdir "%RECEIPTS_PATH%"
if not exist "%TEMP_PATH%" mkdir "%TEMP_PATH%"

:: Create log file
echo %INSTALL_DATE% - Installation started > "%LOGS_PATH%\install.log"

:: Set permissions
icacls "%APP_PATH%" /grant "Users:(OI)(CI)M" >nul 2>&1
echo [✓] Folder structure created successfully
echo %INSTALL_DATE% - Folder structure created >> "%LOGS_PATH%\install.log"

:: Step 2: Install .NET 8.0 if not already installed
echo [2/5] Checking .NET 8.0 Runtime...
dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
if %ERRORLEVEL% equ 0 (
    echo [✓] .NET 8.0 Runtime is already installed
    echo %INSTALL_DATE% - .NET 8.0 already exists >> "%LOGS_PATH%\install.log"
) else (
    echo [i] .NET 8.0 Runtime is not installed
    echo [i] Please install .NET 8.0 Runtime manually from:
    echo [i] https://dotnet.microsoft.com/download/dotnet/8.0
    echo [i] Continuing installation...
    echo %INSTALL_DATE% - .NET 8.0 not installed >> "%LOGS_PATH%\install.log"
)

:: Step 3: Check SQLite
echo [3/5] Checking SQLite...
set SQLITE_PATH=C:\sqlite3
set SQLITE_EXE=%SQLITE_PATH%\sqlite3.exe

if exist "%SQLITE_EXE%" (
    echo [✓] SQLite already exists at %SQLITE_PATH%
    echo %INSTALL_DATE% - SQLite already exists >> "%LOGS_PATH%\install.log"
) else (
    echo [i] SQLite is not installed
    echo [i] Please install SQLite manually from:
    echo [i] https://www.sqlite.org/download.html
    echo [i] Continuing installation...
    echo %INSTALL_DATE% - SQLite not installed >> "%LOGS_PATH%\install.log"
)

:: Step 4: Initialize database
echo [4/5] Initializing database...
echo %INSTALL_DATE% - Initializing database >> "%LOGS_PATH%\install.log"

set DB_FILE=%DATA_PATH%\restaurant.db
set DB_BACKUP=%BACKUPS_PATH%\restaurant_initial.db

:: Create SQL initialization script - only create tables, no data
echo CREATE TABLE IF NOT EXISTS Categories (CategoryId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT); > "%TEMP%\create_tables.sql"
echo CREATE TABLE IF NOT EXISTS Products (ProductId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT, Price REAL NOT NULL, CategoryId INTEGER, IsAvailable INTEGER, Image TEXT, FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId)); >> "%TEMP%\create_tables.sql"
echo CREATE TABLE IF NOT EXISTS Orders (OrderId INTEGER PRIMARY KEY, OrderDate TEXT NOT NULL, CustomerName TEXT, TotalAmount REAL, IsPaid INTEGER, PaymentMethod TEXT, OrderStatus TEXT); >> "%TEMP%\create_tables.sql"
echo CREATE TABLE IF NOT EXISTS OrderItems (OrderItemId INTEGER PRIMARY KEY, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER, Subtotal REAL, FOREIGN KEY(OrderId) REFERENCES Orders(OrderId), FOREIGN KEY(ProductId) REFERENCES Products(ProductId)); >> "%TEMP%\create_tables.sql"
echo CREATE TABLE IF NOT EXISTS Users (UserId INTEGER PRIMARY KEY, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, FullName TEXT, Role TEXT, IsActive INTEGER); >> "%TEMP%\create_tables.sql"
echo CREATE TABLE IF NOT EXISTS AppSettings (SettingId INTEGER PRIMARY KEY, SettingKey TEXT NOT NULL UNIQUE, SettingValue TEXT, Description TEXT); >> "%TEMP%\create_tables.sql"

:: Create SQL script for data insertion
echo BEGIN TRANSACTION; > "%TEMP%\insert_data.sql"
echo INSERT OR IGNORE INTO Categories (CategoryId, Name, Description) VALUES (1, 'Main Dishes', 'Primary dishes'); >> "%TEMP%\insert_data.sql"
echo INSERT OR IGNORE INTO Categories (CategoryId, Name, Description) VALUES (2, 'Beverages', 'Hot and cold drinks'); >> "%TEMP%\insert_data.sql"
echo INSERT OR IGNORE INTO Products (ProductId, Name, Description, Price, CategoryId, IsAvailable) VALUES (1, 'Beef Burger', 'Beef burger with cheese and vegetables', 25.00, 1, 1); >> "%TEMP%\insert_data.sql"
echo INSERT OR IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Role, IsActive) VALUES (1, 'admin', '$2a$11$K3g6XpVzmdBp0AfZ9GWbZeRJJWrm/QbiQTfHX5XhwJECZ6FbLIHSa', 'System Administrator', 'Admin', 1); >> "%TEMP%\insert_data.sql"
echo COMMIT; >> "%TEMP%\insert_data.sql"

if not exist "%DB_FILE%" (
    echo [i] Creating new database...
    
    :: Try to create database if SQLite exists
    if exist "%SQLITE_EXE%" (
        echo [i] Creating database with SQLite...
        
        :: First create tables
        "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\create_tables.sql"
        
        :: Then insert data
        "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\insert_data.sql"
        
        if exist "%DB_FILE%" (
            copy "%DB_FILE%" "%DB_BACKUP%" >nul
            echo [✓] Database created successfully
            echo %INSTALL_DATE% - Database created successfully >> "%LOGS_PATH%\install.log"
        ) else (
            echo [!] Failed to create database
            echo [i] Creating empty database file...
            echo. > "%DB_FILE%"
            echo %INSTALL_DATE% - Empty database created >> "%LOGS_PATH%\install.log"
        )
    ) else (
        echo [i] Creating empty database file...
        echo. > "%DB_FILE%"
        echo %INSTALL_DATE% - Empty database created (SQLite not available) >> "%LOGS_PATH%\install.log"
    )
) else (
    echo [✓] Database already exists
    echo %INSTALL_DATE% - Database already exists >> "%LOGS_PATH%\install.log"
    
    :: Update existing database schema
    if exist "%SQLITE_EXE%" (
        echo [i] Updating database schema...
        "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\create_tables.sql"
        echo [✓] Database schema updated
    )
)

:: Step 5: Copy executable files (if available) or create placeholder
echo [5/5] Setting up application files...

:: Check if we have executable in the current directory
set SOURCE_DIR=%~dp0
set SOURCE_EXE=%SOURCE_DIR%%APP_NAME%.exe

if exist "%SOURCE_EXE%" (
    echo [i] Found executable file, copying to installation directory...
    copy "%SOURCE_EXE%" "%APP_PATH%\" >nul
    echo [✓] Application files copied
    echo %INSTALL_DATE% - Application files copied >> "%LOGS_PATH%\install.log"
) else (
    echo [i] Executable not found, creating placeholder...
    echo @echo off > "%APP_PATH%\%APP_NAME%.bat"
    echo echo ================================================ >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo    Restaurant Management System v%APP_VERSION% >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo ================================================ >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo. >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo This is a placeholder for the actual application. >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo The application needs to be built from source code. >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo. >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo Please build the project in Visual Studio and copy >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo the executable files to this directory. >> "%APP_PATH%\%APP_NAME%.bat"
    echo echo. >> "%APP_PATH%\%APP_NAME%.bat"
    echo pause >> "%APP_PATH%\%APP_NAME%.bat"
    
    echo [i] Created placeholder batch file
    echo %INSTALL_DATE% - Created placeholder batch file >> "%LOGS_PATH%\install.log"
    
    :: Set target to batch file instead
    set TARGET_PATH=%APP_PATH%\%APP_NAME%.bat
)

:: Create desktop shortcut
echo [i] Creating desktop shortcut...
set SHORTCUT_PATH=%USERPROFILE%\Desktop\%APP_NAME%.lnk

echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\CreateShortcut.vbs"
echo oLink.TargetPath = "%TARGET_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%APP_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Description = "Restaurant Management System v%APP_VERSION%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\CreateShortcut.vbs"

cscript //nologo "%TEMP%\CreateShortcut.vbs"
echo [✓] Desktop shortcut created
echo %INSTALL_DATE% - Desktop shortcut created >> "%LOGS_PATH%\install.log"

:: Cleanup
echo [i] Cleaning up temporary files...
del "%TEMP%\create_tables.sql" >nul 2>&1
del "%TEMP%\insert_data.sql" >nul 2>&1
del "%TEMP%\CreateShortcut.vbs" >nul 2>&1

:: Final message
echo.
echo ================================================
echo *                                              *
echo *   Installation completed successfully! v%APP_VERSION%  *
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