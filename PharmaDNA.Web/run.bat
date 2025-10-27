@echo off
echo ==========================================
echo    PHARMADNA WEB APPLICATION
echo ==========================================
echo.

REM Check if .env file exists
if not exist ".env" (
    echo ❌ .env file not found!
    echo.
    echo Please run setup.bat first to create .env file
    echo or copy env.example to .env and configure it manually.
    echo.
    pause
    exit /b 1
)

echo ✅ .env file found!

REM Navigate to web directory
cd /d "%~dp0"

REM Validate configuration
echo.
echo Validating configuration...
dotnet run --no-build --configuration Release -- --validate-config
if %ERRORLEVEL% neq 0 (
    echo ❌ Configuration validation failed!
    echo Please check your .env file and ensure all required variables are set.
    echo.
    pause
    exit /b 1
)

REM Restore dependencies
echo.
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to restore packages
    pause
    exit /b 1
)

REM Build project
echo.
echo Building project...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed
    pause
    exit /b 1
)

REM Run the application
echo.
echo ==========================================
echo    STARTING APPLICATION
echo ==========================================
echo.
echo 🌐 Application will be available at:
echo    - https://localhost:5001 (HTTPS)
echo    - http://localhost:5000 (HTTP)
echo.
echo 📚 API Documentation:
echo    - https://localhost:5001/api-docs (Swagger UI)
echo.
echo 🔍 Health Check:
echo    - https://localhost:5001/api/health
echo.
echo Press Ctrl+C to stop the application
echo.
dotnet run --configuration Release

pause

