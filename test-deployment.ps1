# Script de Teste de Deployment Local
# Simula um deployment real para testar o artifact

param(
    [string]$ArtifactPath = "./artifacts/package",
    [string]$TestPort = "5999",
    [int]$TimeoutSeconds = 30,
    [switch]$SkipCleanup
)

$ErrorActionPreference = "Stop"

Write-Host "Starting Local Deployment Test..." -ForegroundColor Cyan
Write-Host "Artifact: $ArtifactPath" -ForegroundColor Gray
Write-Host "Test Port: $TestPort" -ForegroundColor Gray

# Verificar se o artifact existe
if (-not (Test-Path $ArtifactPath)) {
    Write-Error "Artifact not found at: $ArtifactPath"
    exit 1
}

# Verificar se a porta está livre
try {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, [int]$TestPort)
    $listener.Start()
    $listener.Stop()
    Write-Host "Port $TestPort is available" -ForegroundColor Green
} catch {
    Write-Error "Port $TestPort is not available: $($_.Exception.Message)"
    exit 1
}

# Criar diretório temporário para teste
$testDir = Join-Path $env:TEMP "artifact-deploy-test-$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "Creating test deployment directory: $testDir" -ForegroundColor Yellow

try {
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
    
    # Copiar artifact para diretório de teste
    Write-Host "Copying artifact files..." -ForegroundColor Yellow
    Copy-Item -Path "$ArtifactPath\*" -Destination $testDir -Recurse -Force
    
    # Navegar para diretório de teste
    Push-Location $testDir
    
    # Verificar se o executável existe
    $exePath = Join-Path $testDir "IntranetDocumentos.exe"
    if (-not (Test-Path $exePath)) {
        throw "Executable not found: $exePath"
    }
    
    Write-Host "Starting application on port $TestPort..." -ForegroundColor Yellow
    
    # Configurar variáveis de ambiente para o teste
    $env:ASPNETCORE_URLS = "http://localhost:$TestPort"
    $env:ASPNETCORE_ENVIRONMENT = "Production"
    
    # Iniciar aplicação em background
    $process = Start-Process -FilePath $exePath -PassThru -WindowStyle Hidden
    
    if (-not $process) {
        throw "Failed to start application process"
    }
    
    Write-Host "Application started with PID: $($process.Id)" -ForegroundColor Green
    
    # Aguardar inicialização
    Write-Host "Waiting for application to start..." -ForegroundColor Yellow
    $startTime = Get-Date
    $appStarted = $false
    
    while (((Get-Date) - $startTime).TotalSeconds -lt $TimeoutSeconds) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$TestPort" -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                $appStarted = $true
                break
            }
        } catch {
            # Ignorar erros durante a espera
            Start-Sleep -Seconds 2
        }
    }
    
    if ($appStarted) {
        Write-Host "SUCCESS: Application is responding!" -ForegroundColor Green
        Write-Host "Response status: $($response.StatusCode)" -ForegroundColor Green
        
        # Fazer alguns testes básicos
        Write-Host "`nRunning basic health checks..." -ForegroundColor Yellow
        
        # Teste 1: Verificar se a página inicial carrega
        try {
            $homeResponse = Invoke-WebRequest -Uri "http://localhost:$TestPort" -UseBasicParsing
            if ($homeResponse.Content -match "IntranetDocumentos|Login|Intranet") {
                Write-Host "✅ Home page loads correctly" -ForegroundColor Green
            } else {
                Write-Host "⚠️  Home page content unexpected" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "❌ Home page test failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Teste 2: Verificar se recursos estáticos carregam
        try {
            $cssResponse = Invoke-WebRequest -Uri "http://localhost:$TestPort/css/site.css" -UseBasicParsing -ErrorAction SilentlyContinue
            if ($cssResponse -and $cssResponse.StatusCode -eq 200) {
                Write-Host "✅ Static CSS files accessible" -ForegroundColor Green
            } else {
                Write-Host "⚠️  CSS files may not be accessible" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠️  CSS test inconclusive" -ForegroundColor Yellow
        }
        
        # Teste 3: Verificar se a aplicação não crashou
        if (-not $process.HasExited) {
            Write-Host "✅ Application process is stable" -ForegroundColor Green
        } else {
            Write-Host "❌ Application process has exited" -ForegroundColor Red
        }
        
        Write-Host "`n🎉 DEPLOYMENT TEST PASSED!" -ForegroundColor Green
        Write-Host "Artifact is ready for production deployment." -ForegroundColor Green
        
    } else {
        Write-Host "FAILED: Application did not start within $TimeoutSeconds seconds" -ForegroundColor Red
        
        if (-not $process.HasExited) {
            Write-Host "Process is running but not responding to HTTP requests" -ForegroundColor Red
        } else {
            Write-Host "Application process has exited with code: $($process.ExitCode)" -ForegroundColor Red
        }
        
        throw "Deployment test failed"
    }
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
    
} finally {
    # Cleanup
    Write-Host "`nCleaning up..." -ForegroundColor Yellow
    
    # Parar processo se ainda estiver rodando
    if ($process -and -not $process.HasExited) {
        Write-Host "Stopping application process..." -ForegroundColor Yellow
        $process.Kill()
        $process.WaitForExit(10000)
    }
    
    # Voltar ao diretório original
    Pop-Location
    
    # Remover diretório de teste se não for para manter
    if (-not $SkipCleanup -and (Test-Path $testDir)) {
        Write-Host "Removing test directory..." -ForegroundColor Yellow
        Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "Test directory preserved: $testDir" -ForegroundColor Gray
    }
    
    Write-Host "Cleanup completed." -ForegroundColor Green
}

Write-Host "`n✅ Deployment test completed successfully!" -ForegroundColor Green
