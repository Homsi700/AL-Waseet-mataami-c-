@echo off
echo ===================================================
echo Installing Restaurant Management System Requirements
echo ===================================================

echo.
echo Checking for .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please install .NET 8.0 SDK from the official website:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo After installation, run this file again.
    pause
    exit /b 1
)

echo .NET SDK found. Checking version...
dotnet --version
echo.

echo Installing required NuGet packages...
cd FastFoodManagement
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo Failed to install NuGet packages.
    pause
    exit /b 1
)

echo.
echo Creating database folder...
mkdir FastFoodDB 2>nul

echo.
echo Initializing database...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo Note: You may need to install Entity Framework Core tools:
    echo dotnet tool install --global dotnet-ef
    echo.
    echo Then run: dotnet ef database update
)

echo.
echo Installation completed successfully!
echo You can now run the project using run.bat
echo.
pause