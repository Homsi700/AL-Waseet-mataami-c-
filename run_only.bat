@echo off
echo ===================================================
echo Running Restaurant Management System (No Build)
echo ===================================================

echo.
echo Running the application...
cd FastFoodManagement
dotnet run --no-build
if %ERRORLEVEL% NEQ 0 (
    echo Failed to run the application.
    echo Make sure you have built the project first using build.bat
    pause
    exit /b 1
)

pause