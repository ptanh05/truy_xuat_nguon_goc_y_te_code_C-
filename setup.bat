@echo off
echo ==========================================
echo    PHARMADNA SETUP SCRIPT
echo ==========================================
echo.

REM Check if .env file exists
if not exist "PharmaDNA.Web\.env" (
    echo Creating .env file from template...
    copy "env.example" "PharmaDNA.Web\.env"
    echo.
    echo ✅ .env file created!
    echo.
    echo ⚠️  IMPORTANT: Please edit PharmaDNA.Web\.env and configure your settings:
    echo    - DATABASE_URL (PostgreSQL connection string)
    echo    - PINATA_JWT (Pinata API token)
    echo    - PHARMADNA_RPC (Blockchain RPC URL)
    echo    - PHARMA_NFT_ADDRESS (Deployed contract address)
    echo    - OWNER_PRIVATE_KEY (Owner wallet private key)
    echo.
    pause
    exit /b 1
)

echo ✅ .env file found!

REM Install .NET dependencies
echo.
echo Installing .NET dependencies...
cd PharmaDNA.Web
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to restore .NET packages
    pause
    exit /b 1
)

REM Install Node.js dependencies
echo.
echo Installing Node.js dependencies...
npm install
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to install Node.js packages
    pause
    exit /b 1
)

REM Build Tailwind CSS
echo.
echo Building Tailwind CSS...
npm run build-css-prod
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to build Tailwind CSS
    pause
    exit /b 1
)

REM Build the project
echo.
echo Building the project...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to build project
    pause
    exit /b 1
)

cd ..

echo.
echo ==========================================
echo    SETUP COMPLETED SUCCESSFULLY!
echo ==========================================
echo.
echo Next steps:
echo 1. Edit PharmaDNA.Web\.env with your actual configuration
echo 2. Deploy smart contract (run saga-contract\deploy-pharmadna.bat)
echo 3. Update PHARMA_NFT_ADDRESS in .env file
echo 4. Run the application (run.bat or dotnet run)
echo.
echo For more information, see SETUP.md
echo.
pause
