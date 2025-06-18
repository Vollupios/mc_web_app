# Script de Teste de Build Artifacts - Versao Simples
param(
    [string]$ArtifactPath = "./artifacts/package",
    [string]$Version = "20250618094845-7f0e31c"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting Build Artifact Tests..." -ForegroundColor Cyan
Write-Host "Artifact Path: $ArtifactPath" -ForegroundColor Gray
Write-Host "Expected Version: $Version" -ForegroundColor Gray

# Contadores
$PassedTests = 0
$FailedTests = 0

function Test-FileExists {
    param($FilePath, $TestName)
    if (Test-Path $FilePath) {
        Write-Host "PASS: $TestName" -ForegroundColor Green
        $script:PassedTests++
        return $true
    } else {
        Write-Host "FAIL: $TestName - File not found: $FilePath" -ForegroundColor Red
        $script:FailedTests++
        return $false
    }
}

Write-Host "`n=== FASE 1: Testes de Estrutura ===" -ForegroundColor Yellow

# Teste 1: Verificar diretorio do artifact
Test-FileExists $ArtifactPath "Artifact directory exists"

# Teste 2: Verificar arquivos principais
$requiredFiles = @(
    "IntranetDocumentos.exe",
    "IntranetDocumentos.dll",
    "appsettings.json",
    "deployment-info.json"
)

foreach ($file in $requiredFiles) {
    $filePath = Join-Path $ArtifactPath $file
    Test-FileExists $filePath "Required file: $file"
}

# Teste 3: Verificar diretorios
$requiredDirs = @("wwwroot", "runtimes")
foreach ($dir in $requiredDirs) {
    $dirPath = Join-Path $ArtifactPath $dir
    Test-FileExists $dirPath "Required directory: $dir"
}

Write-Host "`n=== FASE 2: Testes de Metadados ===" -ForegroundColor Yellow

# Teste 4: Verificar deployment-info.json
$deployInfoPath = Join-Path $ArtifactPath "deployment-info.json"
if (Test-Path $deployInfoPath) {
    try {
        $deployInfo = Get-Content $deployInfoPath | ConvertFrom-Json
        if ($deployInfo.version -eq $Version) {
            Write-Host "PASS: Version matches expected ($Version)" -ForegroundColor Green
            $PassedTests++
        } else {
            Write-Host "FAIL: Version mismatch. Expected: $Version, Found: $($deployInfo.version)" -ForegroundColor Red
            $FailedTests++
        }
        
        if (![string]::IsNullOrEmpty($deployInfo.buildDate)) {
            Write-Host "PASS: Build date exists ($($deployInfo.buildDate))" -ForegroundColor Green
            $PassedTests++
        } else {
            Write-Host "FAIL: Build date missing" -ForegroundColor Red
            $FailedTests++
        }
    } catch {
        Write-Host "FAIL: deployment-info.json invalid JSON: $($_.Exception.Message)" -ForegroundColor Red
        $FailedTests++
    }
}

Write-Host "`n=== FASE 3: Testes de Dependencias ===" -ForegroundColor Yellow

# Teste 5: Verificar dependencias principais
$coreDependencies = @(
    "Microsoft.EntityFrameworkCore.dll",
    "Microsoft.Data.Sqlite.dll",
    "Microsoft.AspNetCore.Identity.EntityFrameworkCore.dll"
)

foreach ($dll in $coreDependencies) {
    $dllPath = Join-Path $ArtifactPath $dll
    Test-FileExists $dllPath "Core dependency: $dll"
}

Write-Host "`n=== FASE 4: Testes de Executabilidade ===" -ForegroundColor Yellow

# Teste 6: Verificar tamanho do executavel
$exePath = Join-Path $ArtifactPath "IntranetDocumentos.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length
    if ($exeSize -gt 100KB) {
        Write-Host "PASS: Executable has reasonable size ($([math]::Round($exeSize/1KB, 2)) KB)" -ForegroundColor Green
        $PassedTests++
    } else {
        Write-Host "FAIL: Executable too small ($exeSize bytes)" -ForegroundColor Red
        $FailedTests++
    }
}

# Teste 7: Verificar runtime config
$runtimeConfigPath = Join-Path $ArtifactPath "IntranetDocumentos.runtimeconfig.json"
if (Test-Path $runtimeConfigPath) {
    try {
        $runtimeConfig = Get-Content $runtimeConfigPath | ConvertFrom-Json
        if ($runtimeConfig.runtimeOptions.tfm -eq "net9.0") {
            Write-Host "PASS: Runtime target framework is net9.0" -ForegroundColor Green
            $PassedTests++
        } else {
            Write-Host "FAIL: Wrong target framework: $($runtimeConfig.runtimeOptions.tfm)" -ForegroundColor Red
            $FailedTests++
        }
    } catch {
        Write-Host "FAIL: Runtime config invalid: $($_.Exception.Message)" -ForegroundColor Red
        $FailedTests++
    }
}

Write-Host "`n=== FASE 5: Testes Web ===" -ForegroundColor Yellow

# Teste 8: Verificar arquivos estaticos
$wwwrootPath = Join-Path $ArtifactPath "wwwroot"
if (Test-Path $wwwrootPath) {
    $staticFiles = Get-ChildItem $wwwrootPath -Recurse -File -ErrorAction SilentlyContinue
    if ($staticFiles.Count -gt 0) {
        Write-Host "PASS: Static files exist ($($staticFiles.Count) files)" -ForegroundColor Green
        $PassedTests++
    } else {
        Write-Host "FAIL: No static files found in wwwroot" -ForegroundColor Red
        $FailedTests++
    }
}

Write-Host "`n=== RESULTADOS ===" -ForegroundColor Cyan
Write-Host "Testes Passaram: $PassedTests" -ForegroundColor Green
Write-Host "Testes Falharam: $FailedTests" -ForegroundColor Red

$totalTests = $PassedTests + $FailedTests
$successRate = if ($totalTests -gt 0) { [math]::Round(($PassedTests / $totalTests) * 100, 2) } else { 0 }

Write-Host "Taxa de Sucesso: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })

if ($FailedTests -eq 0) {
    Write-Host "`nARTIFACT APROVADO PARA DEPLOYMENT!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nARTIFACT TEM PROBLEMAS - Verifique os erros acima" -ForegroundColor Red
    exit 1
}
