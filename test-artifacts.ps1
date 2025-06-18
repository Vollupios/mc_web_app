# Teste Completo de Build Artifacts - IntranetDocumentos
# ======================================================

param(
    [string]$ArtifactPath = "./artifacts/package",
    [string]$ExpectedVersion = "20250618094845-7f0e31c"
)

# Função para logging
function Write-TestLog {
    param([string]$Message, [string]$Type = "Info")
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    switch ($Type) {
        "Success" { Write-Host "[$timestamp] [PASS] $Message" -ForegroundColor Green }
        "Error"   { Write-Host "[$timestamp] [FAIL] $Message" -ForegroundColor Red }
        "Warning" { Write-Host "[$timestamp] [WARN] $Message" -ForegroundColor Yellow }
        default   { Write-Host "[$timestamp] [INFO] $Message" -ForegroundColor White }
    }
}

# Contador de testes
$TestResults = @{
    Passed = 0
    Failed = 0
    Total = 0
}

# Função de teste
function Test-Assert {
    param(
        [bool]$Condition,
        [string]$TestName,
        [string]$ErrorMessage = "Test failed"
    )
    
    $TestResults.Total++
    
    if ($Condition) {
        $TestResults.Passed++
        Write-TestLog "$TestName" "Success"
        return $true
    } else {
        $TestResults.Failed++
        Write-TestLog "$TestName - $ErrorMessage" "Error"
        return $false
    }
}

Write-Host "=== TESTE COMPLETO DE BUILD ARTIFACTS ===" -ForegroundColor Cyan
Write-Host "Artifact Path: $ArtifactPath" -ForegroundColor White
Write-Host "Expected Version: $ExpectedVersion" -ForegroundColor White

# =====================================
# 1. TESTES DE ESTRUTURA
# =====================================

Write-Host "`n[FASE 1] Testes de Estrutura Basica" -ForegroundColor Yellow

# Teste 1.1: Diretório existe
Test-Assert (Test-Path $ArtifactPath) "Artifact directory exists" "Directory not found: $ArtifactPath"

# Teste 1.2: Arquivos principais
$requiredFiles = @(
    "IntranetDocumentos.exe",
    "IntranetDocumentos.dll", 
    "appsettings.json",
    "deployment-info.json"
)

foreach ($file in $requiredFiles) {
    $filePath = Join-Path $ArtifactPath $file
    Test-Assert (Test-Path $filePath) "Required file: $file" "File not found: $file"
}

# Teste 1.3: Diretórios obrigatórios
$requiredDirs = @("wwwroot", "runtimes")

foreach ($dir in $requiredDirs) {
    $dirPath = Join-Path $ArtifactPath $dir
    Test-Assert (Test-Path $dirPath) "Required directory: $dir" "Directory not found: $dir"
}

# =====================================
# 2. TESTES DE METADADOS
# =====================================

Write-Host "`n[FASE 2] Testes de Metadados" -ForegroundColor Yellow

# Teste 2.1: deployment-info.json
$deploymentInfoPath = Join-Path $ArtifactPath "deployment-info.json"
if (Test-Path $deploymentInfoPath) {
    try {
        $deploymentInfo = Get-Content $deploymentInfoPath | ConvertFrom-Json
        Test-Assert ($deploymentInfo.version -eq $ExpectedVersion) "Version matches expected ($ExpectedVersion)" "Version mismatch: expected $ExpectedVersion, got $($deploymentInfo.version)"
        Test-Assert ($null -ne $deploymentInfo.buildDate) "Build date exists" "Build date is missing"
    } catch {
        Test-Assert $false "deployment-info.json is valid JSON" $_.Exception.Message
    }
}

# Teste 2.2: appsettings.json
$appSettingsPath = Join-Path $ArtifactPath "appsettings.json"
if (Test-Path $appSettingsPath) {
    try {
        $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
        Test-Assert ($null -ne $appSettings.ConnectionStrings.DefaultConnection) "Database connection string exists" "Missing connection string"
    } catch {
        Test-Assert $false "appsettings.json is valid JSON" $_.Exception.Message
    }
}

# =====================================
# 3. TESTES DE DEPENDÊNCIAS
# =====================================

Write-Host "`n[FASE 3] Testes de Dependencias" -ForegroundColor Yellow

# Teste 3.1: Dependências principais
$coreDependencies = @(
    "Microsoft.EntityFrameworkCore.dll",
    "Microsoft.Data.Sqlite.dll",
    "Microsoft.AspNetCore.Identity.EntityFrameworkCore.dll"
)

foreach ($dep in $coreDependencies) {
    $depPath = Join-Path $ArtifactPath $dep
    Test-Assert (Test-Path $depPath) "Core dependency: $dep" "Dependency missing: $dep"
}

# =====================================
# 4. TESTES DE EXECUTÁVEL
# =====================================

Write-Host "`n[FASE 4] Testes de Executabilidade" -ForegroundColor Yellow

# Teste 4.1: Tamanho do executável
$exePath = Join-Path $ArtifactPath "IntranetDocumentos.exe"
if (Test-Path $exePath) {
    $exeInfo = Get-Item $exePath
    $sizeMB = [math]::Round($exeInfo.Length / 1MB, 2)
    Test-Assert ($exeInfo.Length -gt 50KB) "Executable has reasonable size ($([math]::Round($exeInfo.Length / 1KB, 1)) KB)" "Executable too small: $sizeMB MB"
}

# Teste 4.2: Runtime config
$runtimeConfigPath = Join-Path $ArtifactPath "IntranetDocumentos.runtimeconfig.json"
if (Test-Path $runtimeConfigPath) {
    try {
        $runtimeConfig = Get-Content $runtimeConfigPath | ConvertFrom-Json
        Test-Assert ($runtimeConfig.runtimeOptions.tfm -eq "net9.0") "Runtime target framework is net9.0" "Wrong target framework: $($runtimeConfig.runtimeOptions.tfm)"
    } catch {
        Test-Assert $false "Runtime config is valid JSON" $_.Exception.Message
    }
}

# =======================================
# 5. TESTES WEB
# =======================================

Write-Host "`n[FASE 5] Testes Web" -ForegroundColor Yellow

# Teste 5.1: Arquivos estáticos
$wwwrootPath = Join-Path $ArtifactPath "wwwroot"
if (Test-Path $wwwrootPath) {
    $staticFiles = Get-ChildItem $wwwrootPath -Recurse -File
    Test-Assert ($staticFiles.Count -gt 0) "Static files exist ($($staticFiles.Count) files)" "No static files found"
}

# =====================================
# 6. TESTES DE SEGURANÇA BÁSICOS
# =====================================

Write-Host "`n[FASE 6] Testes de Seguranca Basicos" -ForegroundColor Yellow

# Teste 6.1: Verificar se não há arquivos sensíveis
$sensitiveFiles = @("*.key", "*.pfx", "*.p12", "*.pem", "secrets.json", ".env")
$foundSensitive = $false

foreach ($pattern in $sensitiveFiles) {
    $found = Get-ChildItem $ArtifactPath -Filter $pattern -Recurse -ErrorAction SilentlyContinue
    if ($found) {
        $foundSensitive = $true
        Write-TestLog "Found sensitive file: $($found.Name)" "Warning"
    }
}

Test-Assert (-not $foundSensitive) "No sensitive files in artifact" "Sensitive files found in artifact"

# Teste 6.2: Verificar permissões do executável
if (Test-Path $exePath) {
    try {
        $acl = Get-Acl $exePath
        # Verificação básica - o arquivo deve ser legível
        Test-Assert ($null -ne $acl) "Executable has proper ACL" "No ACL found for executable"
    } catch {
        Test-Assert $false "Can read executable permissions" $_.Exception.Message
    }
}

# =====================================
# RESULTADOS FINAIS
# =====================================

Write-Host "`n=== RESULTADOS ===" -ForegroundColor Cyan
Write-Host "Testes Passaram: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "Testes Falharam: $($TestResults.Failed)" -ForegroundColor Red
Write-Host "Total de Testes: $($TestResults.Total)" -ForegroundColor White

if ($TestResults.Failed -eq 0) {
    $successRate = 100
} else {
    $successRate = [math]::Round(($TestResults.Passed / $TestResults.Total) * 100, 1)
}

Write-Host "Taxa de Sucesso: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } else { "Yellow" })

if ($TestResults.Failed -eq 0) {
    Write-Host "`nARTIFACT APROVADO PARA DEPLOYMENT!" -ForegroundColor Green -BackgroundColor DarkGreen
    exit 0
} else {
    Write-Host "`nARTIFACT REPROVADO - CORRIJA OS PROBLEMAS ANTES DO DEPLOYMENT" -ForegroundColor Red -BackgroundColor DarkRed
    exit 1
}
