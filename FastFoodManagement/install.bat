@echo off
:: Restaurant Management System Installation Script
:: Must be run as Administrator
:: Update Date: 2024-05-15
:: Version: 2.0 - Updated for .NET 8.0 with additional improvements

echo ================================================
echo    Restaurant Management System (.NET 8.0)
echo             Installation Wizard
echo ================================================
echo.

:: Check for Administrator privileges
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [!] This script must be run as Administrator
    echo [!] Please right-click the file and select "Run as Administrator"
    pause
    exit /b
)

:: Define main paths
set APP_NAME=FastFoodManagement
set APP_PATH=C:\%APP_NAME%
set DATA_PATH=%APP_PATH%\Data
set LOGS_PATH=%APP_PATH%\Logs
set BACKUPS_PATH=%APP_PATH%\Backups
set REPORTS_PATH=%APP_PATH%\Reports
set RECEIPTS_PATH=%APP_PATH%\Receipts
set TEMP_PATH=%APP_PATH%\Temp

:: Step 1: Install .NET 8.0 SDK and Runtime
echo.
echo [1/6] Installing .NET 8.0...
echo.

:: Check if .NET 8.0 is already installed
dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
if %ERRORLEVEL% equ 0 (
    echo [✓] .NET 8.0 Runtime is already installed
) else (
    echo [i] Downloading and installing .NET 8.0 Runtime...
    
    :: Create temporary folder for downloads
    if not exist "%TEMP%\%APP_NAME%" mkdir "%TEMP%\%APP_NAME%"
    
    :: Download the installer
    powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri 'https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x64-installer' -OutFile '%TEMP%\%APP_NAME%\dotnet-runtime-8.0.0-win-x64.exe'}"
    
    :: Run silently
    echo [i] Installing, this may take a few minutes...
    start /wait "" "%TEMP%\%APP_NAME%\dotnet-runtime-8.0.0-win-x64.exe" /install /quiet /norestart
    
    :: Verify installation
    dotnet --list-runtimes | find "Microsoft.WindowsDesktop.App 8.0" >nul
    if %ERRORLEVEL% equ 0 (
        echo [✓] .NET 8.0 installed successfully
    ) else (
        echo [!] Failed to install .NET 8.0
        echo [i] Please install .NET 8.0 manually from Microsoft website
        echo [i] https://dotnet.microsoft.com/download/dotnet/8.0
        pause
        exit /b
    )
)

:: Step 2: Install SQLite
echo.
echo [2/6] Installing SQLite...
echo.

:: Setup SQLite path
set SQLITE_PATH=C:\sqlite3
set SQLITE_EXE=%SQLITE_PATH%\sqlite3.exe

:: Check if sqlite3.exe exists
if exist "%SQLITE_EXE%" (
    echo [✓] SQLite already exists at %SQLITE_PATH%
) else (
    echo [i] Downloading and installing SQLite...
    
    :: Create SQLite folder if it doesn't exist
    if not exist "%SQLITE_PATH%" mkdir "%SQLITE_PATH%"
    
    :: Download SQLite
    powershell -command "& {$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri 'https://www.sqlite.org/2023/sqlite-tools-win32-x86-3420000.zip' -OutFile '%TEMP%\%APP_NAME%\sqlite.zip'}"
    
    :: Extract the file
    powershell -command "& {Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::ExtractToDirectory('%TEMP%\%APP_NAME%\sqlite.zip', '%TEMP%\%APP_NAME%\sqlite')}"
    
    :: Copy files to the required folder
    copy "%TEMP%\%APP_NAME%\sqlite\sqlite-tools-win32-x86-3420000\sqlite3.exe" "%SQLITE_PATH%\" >nul
    
    :: Verify copy
    if exist "%SQLITE_EXE%" (
        echo [✓] SQLite installed successfully
    ) else (
        echo [!] Failed to install SQLite
        echo [i] Please install SQLite manually from SQLite website
        echo [i] https://www.sqlite.org/download.html
        pause
        exit /b
    )
)

:: Add SQLite to PATH variable
echo [i] Adding SQLite to system variables...

:: Check if path is already added
echo %PATH% | find /i "%SQLITE_PATH%" > nul
if %ERRORLEVEL% equ 0 (
    echo [✓] Path %SQLITE_PATH% is already added to PATH
) else (
    :: Add path to PATH permanently
    setx PATH "%PATH%;%SQLITE_PATH%" /M
    echo [✓] Path %SQLITE_PATH% added to PATH
)

:: Step 3: Install additional NuGet packages
echo.
echo [3/6] Installing additional NuGet packages...
echo.

:: Check for dotnet
where dotnet >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [!] dotnet tool not found
    echo [i] Please make sure .NET SDK is installed
    pause
    exit /b
)

:: Create temporary project folder
set TEMP_PROJECT=%TEMP%\%APP_NAME%\TempProject
if not exist "%TEMP_PROJECT%" mkdir "%TEMP_PROJECT%"

:: Create temporary project
pushd "%TEMP_PROJECT%"
dotnet new console -n TempProject
cd TempProject

:: Install required packages
echo [i] Installing Microsoft.EntityFrameworkCore.Sqlite...
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0 >nul

echo [i] Installing Microsoft.EntityFrameworkCore.Tools...
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0 >nul

echo [i] Installing MaterialDesignThemes...
dotnet add package MaterialDesignThemes --version 4.9.0 >nul

echo [i] Installing iTextSharp.LGPLv2.Core...
dotnet add package iTextSharp.LGPLv2.Core --version 3.4.3 >nul

echo [i] Installing System.Drawing.Common...
dotnet add package System.Drawing.Common --version 8.0.0 >nul

echo [i] Installing Microsoft.Extensions.Logging...
dotnet add package Microsoft.Extensions.Logging --version 8.0.0 >nul

echo [i] Installing Newtonsoft.Json...
dotnet add package Newtonsoft.Json --version 13.0.3 >nul

echo [✓] All NuGet packages installed successfully

:: Return to original folder
popd

:: Step 4: Create folder structure
echo.
echo [4/6] Creating folder structure...
echo.

:: Create main folders
if not exist "%APP_PATH%" (
    echo [i] Creating main folders...
    mkdir "%APP_PATH%"
    mkdir "%DATA_PATH%"
    mkdir "%LOGS_PATH%"
    mkdir "%BACKUPS_PATH%"
    mkdir "%REPORTS_PATH%"
    mkdir "%RECEIPTS_PATH%"
    mkdir "%TEMP_PATH%"
    
    :: Set access permissions
    echo [i] Setting access permissions...
    icacls "%APP_PATH%" /grant "Users:(OI)(CI)M" >nul
    
    echo [✓] Folder structure created successfully
) else (
    echo [✓] Folder structure already exists
)

:: Step 5: Create initial database
echo.
echo [5/6] Creating initial database...
echo.

set DB_FILE=%DATA_PATH%\restaurant.db

if not exist "%DB_FILE%" (
    echo [i] Creating new database...
    
    :: Create SQL file for initial structure
    echo -- Creating initial database tables > "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Categories (CategoryId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Products (ProductId INTEGER PRIMARY KEY, Name TEXT NOT NULL, Description TEXT, Price REAL NOT NULL, CategoryId INTEGER, IsAvailable INTEGER, Image TEXT, FOREIGN KEY(CategoryId) REFERENCES Categories(CategoryId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Orders (OrderId INTEGER PRIMARY KEY, OrderDate TEXT NOT NULL, CustomerName TEXT, TotalAmount REAL, IsPaid INTEGER, PaymentMethod TEXT, OrderStatus TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE OrderItems (OrderItemId INTEGER PRIMARY KEY, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER, Subtotal REAL, FOREIGN KEY(OrderId) REFERENCES Orders(OrderId), FOREIGN KEY(ProductId) REFERENCES Products(ProductId)); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE Users (UserId INTEGER PRIMARY KEY, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, FullName TEXT, Role TEXT, IsActive INTEGER); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo CREATE TABLE AppSettings (SettingId INTEGER PRIMARY KEY, SettingKey TEXT NOT NULL UNIQUE, SettingValue TEXT, Description TEXT); >> "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Add initial data
    echo -- Adding initial data >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Categories (Name, Description) VALUES ('Main Dishes', 'Main course dishes'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Categories (Name, Description) VALUES ('Beverages', 'Cold and hot drinks'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Categories (Name, Description) VALUES ('Desserts', 'Sweets and desserts'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Products (Name, Description, Price, CategoryId, IsAvailable) VALUES ('Beef Burger', 'Beef burger with cheese and vegetables', 25.00, 1, 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Products (Name, Description, Price, CategoryId, IsAvailable) VALUES ('Chicken Burger', 'Chicken burger with special sauce', 22.00, 1, 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Products (Name, Description, Price, CategoryId, IsAvailable) VALUES ('Cola', 'Cold carbonated drink', 5.00, 2, 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES ('admin', '$2a$11$K3g6XpVzmdBp0AfZ9GWbZeRJJWrm/QbiQTfHX5XhwJECZ6FbLIHSa', 'System Administrator', 'Admin', 1); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO AppSettings (SettingKey, SettingValue, Description) VALUES ('RestaurantName', 'Al-Waseet Restaurant', 'Restaurant name'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    echo INSERT INTO AppSettings (SettingKey, SettingValue, Description) VALUES ('TaxRate', '15', 'Tax rate percentage'); >> "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Create database and execute commands
    "%SQLITE_EXE%" "%DB_FILE%" < "%TEMP%\%APP_NAME%\init_db.sql"
    
    :: Verify database creation
    if exist "%DB_FILE%" (
        echo [✓] Initial database created successfully
    ) else (
        echo [!] Failed to create database
        pause
        exit /b
    )
) else (
    echo [✓] Database already exists
)

:: Step 6: Create desktop shortcut
echo.
echo [6/6] Creating desktop shortcut...
echo.

set SHORTCUT_PATH=%USERPROFILE%\Desktop\%APP_NAME%.lnk
set TARGET_PATH=%APP_PATH%\%APP_NAME%.exe

:: Create VBScript file to create shortcut
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.TargetPath = "%TARGET_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%APP_PATH%" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Description = "Restaurant Management System" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.IconLocation = "%APP_PATH%\icon.ico" >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\%APP_NAME%\CreateShortcut.vbs"

:: Execute the file
cscript //nologo "%TEMP%\%APP_NAME%\CreateShortcut.vbs"

echo [✓] Desktop shortcut created

:: Clean up temporary files
echo.
echo [i] Cleaning up temporary files...
rmdir /s /q "%TEMP_PROJECT%" >nul 2>&1
del "%TEMP%\%APP_NAME%\dotnet-runtime-8.0.0-win-x64.exe" >nul 2>&1
del "%TEMP%\%APP_NAME%\sqlite.zip" >nul 2>&1
del "%TEMP%\%APP_NAME%\init_db.sql" >nul 2>&1
del "%TEMP%\%APP_NAME%\CreateShortcut.vbs" >nul 2>&1
rmdir /s /q "%TEMP%\%APP_NAME%\sqlite" >nul 2>&1

:: Display completion message
echo.
echo ================================================
echo *                                              *
echo *      Installation completed successfully!    *
echo *                                              *
echo *  - .NET 8.0 Runtime installed               *
echo *  - SQLite installed and added to PATH       *
echo *  - Required NuGet packages installed        *
echo *  - Folder structure created                 *
echo *  - Initial database created                 *
echo *  - Desktop shortcut created                 *
echo *                                              *
echo *  You can now run the Restaurant Management  *
echo *  System                                     *
echo *                                              *
echo *  Note: Default admin credentials are:       *
echo *  Username: admin                            *
echo *  Password: Admin123!                        *
echo *                                              *
echo ================================================
echo.

pause