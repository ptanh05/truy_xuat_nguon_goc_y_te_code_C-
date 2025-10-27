@echo off
echo Installing Tailwind CSS for PharmaDNA Web Application...

REM Install Node.js dependencies
echo Installing Node.js dependencies...
npm install

REM Build Tailwind CSS
echo Building Tailwind CSS...
npm run build-css-prod

echo.
echo ✅ Tailwind CSS installation completed!
echo.
echo To start development with Tailwind CSS watching for changes:
echo npm run build-css
echo.
echo To build for production:
echo npm run build-css-prod
echo.
pause
