#!/bin/bash

echo "=========================================="
echo "    PHARMADNA SETUP SCRIPT"
echo "=========================================="
echo

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
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

# Check if .env file exists
if [ ! -f "PharmaDNA.Web/.env" ]; then
    print_status "Creating .env file from template..."
    cp env.example PharmaDNA.Web/.env
    echo
    print_status "✅ .env file created!"
    echo
    print_warning "IMPORTANT: Please edit PharmaDNA.Web/.env and configure your settings:"
    echo "   - DATABASE_URL (PostgreSQL connection string)"
    echo "   - PINATA_JWT (Pinata API token)"
    echo "   - PHARMADNA_RPC (Blockchain RPC URL)"
    echo "   - PHARMA_NFT_ADDRESS (Deployed contract address)"
    echo "   - OWNER_PRIVATE_KEY (Owner wallet private key)"
    echo
    exit 1
fi

print_status "✅ .env file found!"

# Install .NET dependencies
echo
print_status "Installing .NET dependencies..."
cd PharmaDNA.Web
dotnet restore
if [ $? -ne 0 ]; then
    print_error "Failed to restore .NET packages"
    exit 1
fi

# Install Node.js dependencies
echo
print_status "Installing Node.js dependencies..."
npm install
if [ $? -ne 0 ]; then
    print_error "Failed to install Node.js packages"
    exit 1
fi

# Build Tailwind CSS
echo
print_status "Building Tailwind CSS..."
npm run build-css-prod
if [ $? -ne 0 ]; then
    print_error "Failed to build Tailwind CSS"
    exit 1
fi

# Build the project
echo
print_status "Building the project..."
dotnet build
if [ $? -ne 0 ]; then
    print_error "Failed to build project"
    exit 1
fi

cd ..

echo
echo "=========================================="
echo "    SETUP COMPLETED SUCCESSFULLY!"
echo "=========================================="
echo
echo "Next steps:"
echo "1. Edit PharmaDNA.Web/.env with your actual configuration"
echo "2. Deploy smart contract (run saga-contract/deploy-pharmadna.bat)"
echo "3. Update PHARMA_NFT_ADDRESS in .env file"
echo "4. Run the application (./run.sh or dotnet run)"
echo
echo "For more information, see SETUP.md"
echo
