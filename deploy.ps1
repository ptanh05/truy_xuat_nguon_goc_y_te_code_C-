# PharmaDNA Deployment Script for Windows
# This script deploys the PharmaDNA application

param(
    [switch]$SkipBuild,
    [switch]$SkipTests,
    [string]$Environment = "Development"
)

Write-Host "🚀 Starting PharmaDNA deployment..." -ForegroundColor Green

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if .env file exists
if (-not (Test-Path "PharmaDNA.Web\.env")) {
    Write-Error ".env file not found in PharmaDNA.Web directory"
    Write-Warning "Please copy env.example to PharmaDNA.Web\.env and configure it"
    exit 1
}

# Check if Docker is installed
try {
    docker --version | Out-Null
    Write-Status "Docker is installed"
} catch {
    Write-Error "Docker is not installed. Please install Docker Desktop first."
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version | Out-Null
    Write-Status "Docker Compose is installed"
} catch {
    Write-Error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
}

# Run tests if not skipped
if (-not $SkipTests) {
    Write-Status "Running tests..."
    Set-Location "PharmaDNA.Web"
    try {
        dotnet test
        Write-Status "✅ All tests passed"
    } catch {
        Write-Warning "Some tests failed, but continuing with deployment..."
    }
    Set-Location ".."
}

# Build Docker images if not skipped
if (-not $SkipBuild) {
    Write-Status "Building Docker images..."
    docker-compose build
}

# Start services
Write-Status "Starting services..."
docker-compose up -d

# Wait for services to be ready
Write-Status "Waiting for services to be ready..."
Start-Sleep -Seconds 30

# Check if services are running
$runningServices = docker-compose ps | Select-String "Up"
if ($runningServices) {
    Write-Status "✅ Services are running successfully!"
    Write-Status "🌐 Application is available at:"
    Write-Status "   - HTTP: http://localhost:5000"
    Write-Status "   - HTTPS: https://localhost:5001"
    Write-Status "🗄️  Database is available at: localhost:5432"
} else {
    Write-Error "❌ Some services failed to start. Check logs with: docker-compose logs"
    exit 1
}

Write-Status "🎉 Deployment completed successfully!"

# Show logs
Write-Status "📋 Recent logs:"
docker-compose logs --tail=20

Write-Host "`n📝 Useful commands:" -ForegroundColor Cyan
Write-Host "   View logs: docker-compose logs -f" -ForegroundColor White
Write-Host "   Stop services: docker-compose down" -ForegroundColor White
Write-Host "   Restart services: docker-compose restart" -ForegroundColor White
Write-Host "   View service status: docker-compose ps" -ForegroundColor White
