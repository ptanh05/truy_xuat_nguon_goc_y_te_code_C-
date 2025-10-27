@echo off
echo ==========================================
echo    STARTING PHARMADNA APPLICATION
echo ==========================================
echo.

REM Add dotnet to PATH
PATH=%PATH%;C:\Program Files\dotnet

REM Navigate to project directory
cd /d "%~dp0PharmaDNA.Web"

echo Building project...
dotnet build -v q

echo.
echo Starting application...
echo Application will be available at: http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run --urls "http://localhost:5000"

pause
