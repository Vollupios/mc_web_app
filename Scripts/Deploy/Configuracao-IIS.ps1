# Configuracao-IIS.ps1
# Script para configurar IIS e Application Pool para a Intranet Documentos

param(
    [string]$SiteName = "Intranet Documentos",
    [string]$AppPoolName = "IntranetDocumentos",
    [string]$SitePath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [int]$Port = 80,
    [int]$HttpsPort = 443,
    [string]$CertificateThumbprint = ""
)

# Verificar se esta executando como administrador
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Error "Este script deve ser executado como Administrador!"
    exit 1
}

Write-Host "=== Configuracao IIS - Intranet Documentos ===" -ForegroundColor Green

# Importar modulo WebAdministration
Import-Module WebAdministration

# 1. Criar Application Pool
Write-Host "1. Configurando Application Pool..." -ForegroundColor Yellow
if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
    Write-Host "Application Pool '$AppPoolName' ja existe - atualizando configuracoes" -ForegroundColor Cyan
    Remove-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
}

New-WebAppPool -Name $AppPoolName -Force

# Configurar Application Pool
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""  # No Managed Code for .NET Core
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"  # Nunca parar por inatividade
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.maxProcesses" -Value 1
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"  # Desabilitar restart periodico
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.loadUserProfile" -Value $true
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.memory" -Value 2097152  # 2GB limite de memoria

# Configuracoes de arquivo e request
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "recycling.logEventOnRecycle" -Value "Time,Memory,PrivateMemory"

Write-Host "✓ Application Pool configurado: $AppPoolName" -ForegroundColor Green

# 2. Criar/Atualizar Site
Write-Host "2. Configurando Site..." -ForegroundColor Yellow
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Write-Host "Site '$SiteName' ja existe - removendo para recriar" -ForegroundColor Cyan
    Remove-Website -Name $SiteName
}

# Criar site HTTP
New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName

Write-Host "✓ Site criado: $SiteName (Porta $Port)" -ForegroundColor Green

# 3. Configurar HTTPS se certificado fornecido
if ($CertificateThumbprint) {
    Write-Host "3. Configurando HTTPS..." -ForegroundColor Yellow
    try {
        New-WebBinding -Name $SiteName -IP "*" -Port $HttpsPort -Protocol https
        
        # Associar certificado
        $cert = Get-ChildItem -Path "Cert:\LocalMachine\My\$CertificateThumbprint" -ErrorAction Stop
        New-Item -Path "IIS:\SslBindings\0.0.0.0!$HttpsPort" -Value $cert -Force
        
        Write-Host "✓ HTTPS configurado na porta $HttpsPort" -ForegroundColor Green
    } catch {
        Write-Warning "Erro ao configurar HTTPS: $($_.Exception.Message)"
        Write-Host "Configure HTTPS manualmente apos a instalacao" -ForegroundColor Yellow
    }
} else {
    Write-Host "3. HTTPS nao configurado (certificado nao fornecido)" -ForegroundColor Yellow
}

# 4. Configurar modulos necessarios
Write-Host "4. Configurando modulos IIS..." -ForegroundColor Yellow

# Garantir que o ASP.NET Core Module esta instalado
$aspNetCoreModule = Get-WebGlobalModule | Where-Object { $_.Name -eq "AspNetCoreModuleV2" }
if (-not $aspNetCoreModule) {
    Write-Warning "ASP.NET Core Module V2 nao encontrado!"
    Write-Host "Instale o .NET Core Hosting Bundle: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
}

# 5. Configurar Request Filtering
Write-Host "5. Configurando Request Filtering..." -ForegroundColor Yellow

# Permitir uploads grandes
Set-WebConfigurationProperty -Filter "system.webServer/security/requestFiltering/requestLimits" -Name "maxAllowedContentLength" -Value 104857600 -PSPath "IIS:\" -Location "$SiteName"

# Configurar timeouts
Set-WebConfigurationProperty -Filter "system.webServer/asp" -Name "requestTimeout" -Value "00:10:00" -PSPath "IIS:\" -Location "$SiteName"

Write-Host "✓ Request Filtering configurado (100MB max upload)" -ForegroundColor Green

# 6. Configurar Compression
Write-Host "6. Configurando Compression..." -ForegroundColor Yellow
Set-WebConfigurationProperty -Filter "system.webServer/urlCompression" -Name "doStaticCompression" -Value $true -PSPath "IIS:\" -Location "$SiteName"
Set-WebConfigurationProperty -Filter "system.webServer/urlCompression" -Name "doDynamicCompression" -Value $true -PSPath "IIS:\" -Location "$SiteName"

Write-Host "✓ Compression configurada" -ForegroundColor Green

# 7. Configurar permissoes do Application Pool
Write-Host "7. Configurando permissoes..." -ForegroundColor Yellow

$appPoolIdentity = "IIS AppPool\$AppPoolName"

# Permissoes para o site
icacls $SitePath /grant "${appPoolIdentity}:(OI)(CI)RX" /T

# Permissoes para logs
$logsPath = "$SitePath\logs"
if (Test-Path $logsPath) {
    icacls $logsPath /grant "${appPoolIdentity}:(OI)(CI)F" /T
}

# Permissoes para dados
$dataPaths = @(
    "C:\IntranetData\Documents",
    "C:\IntranetData\Backups"
)

foreach ($path in $dataPaths) {
    if (Test-Path $path) {
        icacls $path /grant "${appPoolIdentity}:(OI)(CI)F" /T
    }
}

Write-Host "✓ Permissoes configuradas para: $appPoolIdentity" -ForegroundColor Green

# 8. Verificar configuracao
Write-Host "8. Verificando configuracao..." -ForegroundColor Yellow

$site = Get-Website -Name $SiteName
$appPool = Get-IISAppPool -Name $AppPoolName

Write-Host "✓ Site: $($site.Name) - Estado: $($site.State)" -ForegroundColor Green
Write-Host "✓ App Pool: $($appPool.Name) - Estado: $($appPool.State)" -ForegroundColor Green
Write-Host "✓ Caminho fisico: $($site.PhysicalPath)" -ForegroundColor Green

# 9. Iniciar services
Write-Host "9. Iniciando services..." -ForegroundColor Yellow
Start-WebAppPool -Name $AppPoolName
Start-Website -Name $SiteName

Write-Host "✓ Application Pool e Site iniciados" -ForegroundColor Green

Write-Host "`n🎉 Configuracao IIS concluida!" -ForegroundColor Green
Write-Host "📍 Site: http://localhost:$Port" -ForegroundColor Cyan
if ($CertificateThumbprint) {
    Write-Host "📍 HTTPS: https://localhost:$HttpsPort" -ForegroundColor Cyan
}

Write-Host "`n💡 Proximos passos:" -ForegroundColor Yellow
Write-Host "   1. Fazer deploy da aplicacao" -ForegroundColor White
Write-Host "   2. Configurar connection string" -ForegroundColor White
Write-Host "   3. Testar no navegador" -ForegroundColor White
Write-Host "   4. Configurar certificado SSL se necessario" -ForegroundColor White
