# 🔴 Script de Instalação do Redis para Windows Server
# Execute como Administrador

Write-Host "🔴 Instalando Redis para Intranet Documentos..." -ForegroundColor Red

# Verificar se está executando como Admin
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Error "❌ Este script deve ser executado como Administrador!"
    exit 1
}

# Configurações
$redisVersion = "5.0.14.1"
$redisUrl = "https://github.com/microsoftarchive/redis/releases/download/win-3.0.504/Redis-x64-3.0.504.msi"
$redisPath = "C:\Program Files\Redis"
$redisConfig = "C:\Program Files\Redis\redis.windows-service.conf"
$downloadPath = "$env:TEMP\Redis-installer.msi"

try {
    Write-Host "📥 Baixando Redis..." -ForegroundColor Yellow
    
    # Baixar Redis
    Invoke-WebRequest -Uri $redisUrl -OutFile $downloadPath -UseBasicParsing
    
    Write-Host "📦 Instalando Redis..." -ForegroundColor Yellow
    
    # Instalar Redis
    Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", $downloadPath, "/quiet", "/norestart" -Wait
    
    # Aguardar instalação
    Start-Sleep -Seconds 10
    
    if (Test-Path $redisPath) {
        Write-Host "✅ Redis instalado com sucesso em: $redisPath" -ForegroundColor Green
    } else {
        throw "Falha na instalação do Redis"
    }
    
    # Configurar Redis
    Write-Host "⚙️ Configurando Redis..." -ForegroundColor Yellow
    
    if (Test-Path $redisConfig) {
        # Backup da configuração original
        Copy-Item $redisConfig "$redisConfig.backup"
        
        # Configurações personalizadas para a aplicação
        $configContent = @"
# Redis Configuration for Intranet Documentos
port 6379
bind 127.0.0.1
timeout 0
save 900 1
save 300 10
save 60 10000
rdbcompression yes
dbfilename dump.rdb
dir ./
maxmemory 256mb
maxmemory-policy allkeys-lru
appendonly yes
appendfsync everysec
"@
        
        Set-Content -Path $redisConfig -Value $configContent -Encoding UTF8
        Write-Host "✅ Configuração do Redis atualizada" -ForegroundColor Green
    }
    
    # Configurar como serviço do Windows
    Write-Host "🔧 Configurando serviço do Windows..." -ForegroundColor Yellow
    
    $serviceName = "Redis"
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    
    if ($service) {
        Write-Host "🔄 Reiniciando serviço Redis..." -ForegroundColor Yellow
        Restart-Service -Name $serviceName -Force
    } else {
        Write-Host "⚠️ Serviço Redis não encontrado. Configurando manualmente..." -ForegroundColor Yellow
        
        # Instalar como serviço
        $redisServerExe = "$redisPath\redis-server.exe"
        if (Test-Path $redisServerExe) {
            & $redisServerExe --service-install --service-name Redis --port 6379
            Start-Service -Name Redis
        }
    }
    
    # Verificar se o serviço está rodando
    Start-Sleep -Seconds 5
    $redisService = Get-Service -Name "Redis" -ErrorAction SilentlyContinue
    
    if ($redisService -and $redisService.Status -eq "Running") {
        Write-Host "✅ Serviço Redis está rodando!" -ForegroundColor Green
    } else {
        Write-Warning "⚠️ Serviço Redis pode não estar rodando corretamente"
    }
    
    # Testar conexão
    Write-Host "🧪 Testando conexão com Redis..." -ForegroundColor Yellow
    
    $redisCliPath = "$redisPath\redis-cli.exe"
    if (Test-Path $redisCliPath) {
        $testResult = & $redisCliPath ping 2>$null
        if ($testResult -eq "PONG") {
            Write-Host "✅ Redis respondeu com PONG - Conexão OK!" -ForegroundColor Green
        } else {
            Write-Warning "⚠️ Redis não respondeu ao ping"
        }
    }
    
    # Configurar Firewall (opcional)
    Write-Host "🔥 Configurando Firewall..." -ForegroundColor Yellow
    
    $firewallRule = Get-NetFirewallRule -DisplayName "Redis Server" -ErrorAction SilentlyContinue
    if (-not $firewallRule) {
        New-NetFirewallRule -DisplayName "Redis Server" -Direction Inbound -Port 6379 -Protocol TCP -Action Allow
        Write-Host "✅ Regra de firewall criada para porta 6379" -ForegroundColor Green
    } else {
        Write-Host "ℹ️ Regra de firewall já existe" -ForegroundColor Cyan
    }
    
    # Limpar arquivo de download
    if (Test-Path $downloadPath) {
        Remove-Item $downloadPath -Force
    }
    
    Write-Host "" -ForegroundColor White
    Write-Host "🎉 REDIS INSTALADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "🔴 Informações importantes:" -ForegroundColor Yellow
    Write-Host "   📍 Caminho: $redisPath" -ForegroundColor White
    Write-Host "   🌐 Porta: 6379" -ForegroundColor White
    Write-Host "   🔧 Configuração: $redisConfig" -ForegroundColor White
    Write-Host "   💾 Memória máxima: 256MB" -ForegroundColor White
    Write-Host "   📊 Política de cache: allkeys-lru" -ForegroundColor White
    Write-Host "" -ForegroundColor White
    Write-Host "🔴 Próximo passo: Configure a string de conexão Redis na aplicação:" -ForegroundColor Yellow
    Write-Host '   "Redis": "localhost:6379"' -ForegroundColor Cyan
    Write-Host "" -ForegroundColor White
    
} catch {
    Write-Error "❌ Erro na instalação do Redis: $($_.Exception.Message)"
    exit 1
}

Write-Host "✅ Script de instalação do Redis concluído!" -ForegroundColor Green
