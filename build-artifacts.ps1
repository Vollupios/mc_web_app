# Build and Package Script for IntranetDocumentos
# This script creates build artifacts that can be used by deployment workflows

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "./artifacts",
    [switch]$IncludeDatabase,
    [switch]$CreateZip
)

Write-Host "🚀 Starting build process..." -ForegroundColor Green

# Generate version
$version = (Get-Date -Format "yyyyMMddHHmmss") + "-" + (git rev-parse --short HEAD 2>$null -or "local")
Write-Host "📦 Version: $version" -ForegroundColor Cyan

# Clean previous artifacts
if (Test-Path $OutputPath) {
    Write-Host "🧹 Cleaning previous artifacts..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$publishPath = Join-Path $OutputPath "publish"
$packagePath = Join-Path $OutputPath "package"

Write-Host "🔨 Restoring dependencies..." -ForegroundColor Yellow
dotnet restore IntranetDocumentos.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Restore failed!" -ForegroundColor Red
    exit 1
}

Write-Host "🔨 Building application..." -ForegroundColor Yellow
dotnet build IntranetDocumentos.csproj --no-restore --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "📦 Publishing application..." -ForegroundColor Yellow
dotnet publish IntranetDocumentos.csproj --no-build --configuration $Configuration --output $publishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Creating deployment package..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $packagePath -Force | Out-Null

# Copy published files
Copy-Item -Path "$publishPath\*" -Destination $packagePath -Recurse -Force

# Copy additional configuration files
$additionalFiles = @(
    "appsettings.json",
    "appsettings.Production.json",
    "README.md",
    "LICENSE"
)

foreach ($file in $additionalFiles) {
    if (Test-Path $file) {
        Copy-Item -Path $file -Destination $packagePath -Force
        Write-Host "  ✅ Added: $file" -ForegroundColor Green
    }
}

# Include database if requested
if ($IncludeDatabase) {
    Write-Host "💾 Including database files..." -ForegroundColor Yellow
    $dbFiles = Get-ChildItem -Path "." -Filter "*.db*" -File
    if ($dbFiles) {
        $dbPath = Join-Path $packagePath "database"
        New-Item -ItemType Directory -Path $dbPath -Force | Out-Null
        foreach ($dbFile in $dbFiles) {
            Copy-Item -Path $dbFile.FullName -Destination $dbPath -Force
            Write-Host "  ✅ Added: $($dbFile.Name)" -ForegroundColor Green
        }
    }
}

# Create deployment info
$deploymentInfo = @{
    version = $version
    buildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    configuration = $Configuration
    dotnetVersion = (dotnet --version)
    buildMachine = $env:COMPUTERNAME
    buildUser = $env:USERNAME
} | ConvertTo-Json -Depth 2

$deploymentInfo | Out-File -FilePath (Join-Path $packagePath "deployment-info.json") -Encoding UTF8

# Create installation script
$installScript = @"
# Installation Script for IntranetDocumentos v$version
# Generated on $(Get-Date)

Write-Host "🚀 Installing IntranetDocumentos v$version..." -ForegroundColor Green

# Stop existing service/application
Write-Host "⏹️  Stopping existing application..." -ForegroundColor Yellow
# Add your stop commands here

# Backup current installation
Write-Host "💾 Creating backup..." -ForegroundColor Yellow
# Add your backup commands here

# Deploy new version
Write-Host "📦 Deploying new version..." -ForegroundColor Yellow
# Add your deployment commands here

# Update database
Write-Host "🗄️  Updating database..." -ForegroundColor Yellow
dotnet IntranetDocumentos.dll --migrate

# Start application
Write-Host "▶️  Starting application..." -ForegroundColor Yellow
# Add your start commands here

Write-Host "✅ Installation completed successfully!" -ForegroundColor Green
Write-Host "📊 Application should be available at: http://localhost:5000" -ForegroundColor Cyan
"@

$installScript | Out-File -FilePath (Join-Path $packagePath "install.ps1") -Encoding UTF8

# Create ZIP package if requested
if ($CreateZip) {
    Write-Host "🗜️ Creating ZIP package..." -ForegroundColor Yellow
    $zipPath = Join-Path $OutputPath "IntranetDocumentos-$version.zip"
    
    if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) {
        Compress-Archive -Path "$packagePath\*" -DestinationPath $zipPath -Force
        Write-Host "  ✅ ZIP created: $zipPath" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ Compress-Archive not available, skipping ZIP creation" -ForegroundColor Yellow
    }
}

# Generate artifact summary
Write-Host "`n📋 Build Artifact Summary:" -ForegroundColor Cyan
Write-Host "  Version: $version" -ForegroundColor White
Write-Host "  Configuration: $Configuration" -ForegroundColor White
Write-Host "  Output Path: $OutputPath" -ForegroundColor White
Write-Host "  Package Path: $packagePath" -ForegroundColor White

$packageSize = (Get-ChildItem -Path $packagePath -Recurse | Measure-Object -Property Length -Sum).Sum
Write-Host "  Package Size: $([math]::Round($packageSize / 1MB, 2)) MB" -ForegroundColor White

$fileCount = (Get-ChildItem -Path $packagePath -Recurse -File).Count
Write-Host "  File Count: $fileCount" -ForegroundColor White

Write-Host "`n🎉 Build completed successfully!" -ForegroundColor Green
Write-Host "📦 Artifacts are ready for deployment or upload to workflow storage." -ForegroundColor Green

# Display next steps
Write-Host "`n🔄 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Test the package locally: cd '$packagePath'; dotnet IntranetDocumentos.dll" -ForegroundColor Gray
Write-Host "  2. Deploy to staging: ./install.ps1" -ForegroundColor Gray
Write-Host "  3. Upload to workflow storage or deployment system" -ForegroundColor Gray
