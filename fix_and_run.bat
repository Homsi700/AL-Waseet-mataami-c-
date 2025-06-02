@echo off
echo ===================================================
echo Fixing and Running Restaurant Management System
echo ===================================================

echo.
echo Restoring NuGet packages...
cd FastFoodManagement
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo Failed to restore NuGet packages.
    pause
    exit /b 1
)

echo.
echo Building the project...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build the project.
    echo Check the error messages above for details.
    pause
    exit /b 1
)

echo.
echo Running the application...
dotnet run
if %ERRORLEVEL% NEQ 0 (
    echo Failed to run the application.
    pause
    exit /b 1
)

pause