@echo off
echo ===================================================
echo Initializing Restaurant Management System Database
echo ===================================================

echo.
echo Checking for Entity Framework tools...
dotnet tool list --global | findstr "dotnet-ef"
if %ERRORLEVEL% NEQ 0 (
    echo Installing Entity Framework Core tools...
    dotnet tool install --global dotnet-ef
    if %ERRORLEVEL% NEQ 0 (
        echo Failed to install Entity Framework Core tools.
        pause
        exit /b 1
    )
)

echo.
echo Creating database migrations...
cd FastFoodManagement
dotnet ef migrations add InitialCreate
if %ERRORLEVEL% NEQ 0 (
    echo Failed to create database migrations.
    pause
    exit /b 1
)

echo.
echo Applying migrations to database...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo Failed to apply migrations to database.
    pause
    exit /b 1
)

echo.
echo Database initialized successfully!
echo.
pause