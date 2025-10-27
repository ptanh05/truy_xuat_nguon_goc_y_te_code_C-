# Script to clean up unnecessary files from the root directory

Write-Host "🧹 Cleaning up unnecessary files..." -ForegroundColor Yellow

# Files to delete
$filesToDelete = @(
    "components.json",
    "next.config.mjs",
    "package.json",
    "package-lock.json",
    "postcss.config.mjs",
    "tailwind.config.ts",
    "tsconfig.json",
    "README.md"
)

# Directories to delete
$dirsToDelete = @(
    "app",
    "components",
    "hooks",
    "lib",
    "node_modules",
    "public",
    "styles"
)

# Delete files
foreach ($file in $filesToDelete) {
    if (Test-Path $file) {
        Write-Host "Deleting file: $file" -ForegroundColor Red
        Remove-Item -Path $file -Force
    }
}

# Delete directories
foreach ($dir in $dirsToDelete) {
    if (Test-Path $dir) {
        Write-Host "Deleting directory: $dir" -ForegroundColor Red
        Remove-Item -Path $dir -Recurse -Force
    }
}

Write-Host "✅ Cleanup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Remaining directories:" -ForegroundColor Cyan
Get-ChildItem -Directory | Select-Object Name
