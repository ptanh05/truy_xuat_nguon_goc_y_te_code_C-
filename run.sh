#!/bin/bash

echo "=========================================="
echo "    PHARMADNA WEB APPLICATION"
echo "=========================================="
echo

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Check if .env file exists
if [ ! -f "PharmaDNA.Web/.env" ]; then
    print_error ".env file not found!"
    echo
    echo "Please run ./setup.sh first to create .env file"
    echo "or copy env.example to PharmaDNA.Web/.env and configure it manually."
    echo
    exit 1
fi

print_status ".env file found!"

# Navigate to web directory
cd PharmaDNA.Web

# Validate configuration
echo
print_status "Validating configuration..."
dotnet run --no-build --configuration Release -- --validate-config
if [ $? -ne 0 ]; then
    print_error "Configuration validation failed!"
    echo "Please check your .env file and ensure all required variables are set."
    echo
    exit 1
fi

# Restore dependencies
echo
print_status "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    print_error "Failed to restore packages"
    exit 1
fi

# Build project
echo
print_status "Building project..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    print_error "Build failed"
    exit 1
fi

# Run the application
echo
echo "=========================================="
echo "    STARTING APPLICATION"
echo "=========================================="
echo
print_info "🌐 Application will be available at:"
echo "   - https://localhost:5001 (HTTPS)"
echo "   - http://localhost:5000 (HTTP)"
echo
print_info "📚 API Documentation:"
echo "   - https://localhost:5001/api-docs (Swagger UI)"
echo
print_info "🔍 Health Check:"
echo "   - https://localhost:5001/api/health"
echo
echo "Press Ctrl+C to stop the application"
echo
dotnet run --configuration Release
