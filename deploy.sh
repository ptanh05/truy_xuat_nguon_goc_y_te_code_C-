#!/bin/bash

# PharmaDNA Deployment Script
# This script deploys the PharmaDNA application

set -e

echo "🚀 Starting PharmaDNA deployment..."

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
    print_error ".env file not found in PharmaDNA.Web directory"
    print_warning "Please copy env.example to PharmaDNA.Web/.env and configure it"
    exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

print_status "Building Docker images..."
docker-compose build

print_status "Starting services..."
docker-compose up -d

print_status "Waiting for services to be ready..."
sleep 30

# Check if services are running
if docker-compose ps | grep -q "Up"; then
    print_status "✅ Services are running successfully!"
    print_status "🌐 Application is available at:"
    print_status "   - HTTP: http://localhost:5000"
    print_status "   - HTTPS: https://localhost:5001"
    print_status "🗄️  Database is available at: localhost:5432"
else
    print_error "❌ Some services failed to start. Check logs with: docker-compose logs"
    exit 1
fi

print_status "🎉 Deployment completed successfully!"

# Show logs
print_status "📋 Recent logs:"
docker-compose logs --tail=20
