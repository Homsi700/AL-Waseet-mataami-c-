@echo off
echo ===================================================
echo Building Restaurant Management System
echo ===================================================

echo.
echo Building the project...
cd FastFoodManagement
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build the project.
    pause
    exit /b 1
)

echo.
echo Project built successfully!
echo.
pause