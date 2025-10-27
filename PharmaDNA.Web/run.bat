@echo off
echo Starting PharmaDNA Web Application...
echo.

REM Check if .env file exists
if not exist ".env" (
    echo Warning: .env file not found!
    echo Please create a .env file based on .env.example
    echo.
    pause
)

REM Navigate to web directory
cd /d "%~dp0"

REM Restore dependencies
echo Restoring NuGet packages...
dotnet restore

REM Build project
echo.
echo Building project...
dotnet build

REM Run the application
echo.
echo Starting application...
echo Application will be available at:
echo - https://localhost:5001
echo - http://localhost:5000
echo.
dotnet run

pause

