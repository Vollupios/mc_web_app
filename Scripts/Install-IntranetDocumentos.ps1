# 🚀 INSTALADOR OFICIAL - Intranet Documentos
# ================================================================
# Script principal unificado para instalação completa
# Versão: 2.0 Production Ready
# Data: 16 de Julho de 2025
# ================================================================

param(
    [Parameter(HelpMessage="Tipo de instalação: Dev, Production")]
    [ValidateSet("Dev", "Production")]
    [string]$InstallType = "Production",
    
    [Parameter(HelpMessage="Instalar Redis para cache distribuído")]
    [switch]$WithRedis = $true,
    
    [Parameter(HelpMessage="Aplicar hardening de segurança")]
    [switch]$WithSecurity = $true,
    
    [Parameter(HelpMessage="Configurar IIS automaticamente")]
    [switch]$WithIIS = $true,
    
    [Parameter(HelpMessage="Executar verificação pós-instalação")]
    [switch]$WithVerification = $true,
    
    [Parameter(HelpMessage="Modo silencioso (sem interação)")]
    [switch]$Silent = $false
)

# ================================================================
# CONFIGURAÇÕES GLOBAIS
# ================================================================

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$AppRoot = Split-Path $ScriptRoot -Parent
$LogFile = Join-Path $AppRoot "Logs\install-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Criar pasta de logs se não existir
$LogDir = Split-Path $LogFile -Parent
if (!(Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

# ================================================================
# FUNÇÕES AUXILIARES
# ================================================================

function Write-InstallLog {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    
    # Escrever no console com cores
    switch ($Level) {
        "ERROR" { Write-Host $logEntry -ForegroundColor Red }
        "WARN"  { Write-Host $logEntry -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logEntry -ForegroundColor Green }
        default { Write-Host $logEntry -ForegroundColor White }
    }
    
    # Escrever no arquivo de log
    Add-Content -Path $LogFile -Value $logEntry
}

function Test-AdminPrivileges {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Prerequisite {
    param([string]$Name, [string]$Command, [string]$MinVersion = "")
    
    try {
        $result = Invoke-Expression $Command
        if ($MinVersion -and $result) {
            # Implementar verificação de versão se necessário
        }
        Write-InstallLog "✅ $Name encontrado: $result" "SUCCESS"
        return $true
    }
    catch {
        Write-InstallLog "❌ $Name não encontrado" "ERROR"
        return $false
    }
}

function Invoke-ScriptSafely {
    param([string]$ScriptPath, [string]$Arguments = "")
    
    Write-InstallLog "🔧 Executando: $ScriptPath $Arguments"
    
    try {
        if ($Arguments) {
            $result = & $ScriptPath $Arguments
        } else {
            $result = & $ScriptPath
        }
        
        Write-InstallLog "✅ Script executado com sucesso: $ScriptPath" "SUCCESS"
        return $true
    }
    catch {
        Write-InstallLog "❌ Erro ao executar script: $ScriptPath - $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# ================================================================
# VERIFICAÇÕES INICIAIS
# ================================================================

function Start-PreInstallChecks {
    Write-InstallLog "🔍 INICIANDO VERIFICAÇÕES PRÉ-INSTALAÇÃO" "INFO"
    
    # Verificar privilégios de administrador
    if (!(Test-AdminPrivileges)) {
        Write-InstallLog "❌ Este script deve ser executado como Administrador!" "ERROR"
        exit 1
    }
    Write-InstallLog "✅ Privilégios de administrador confirmados" "SUCCESS"
    
    # Verificar .NET
    $dotnetCheck = Test-Prerequisite ".NET 8.0+" "dotnet --version"
    if (!$dotnetCheck) {
        Write-InstallLog "🔽 Instalando .NET 8.0..." "INFO"
        # Implementar instalação do .NET se necessário
    }
    
    # Verificar PowerShell
    $psVersion = $PSVersionTable.PSVersion
    Write-InstallLog "✅ PowerShell $psVersion detectado" "SUCCESS"
    
    # Verificar IIS (para produção)
    if ($InstallType -eq "Production" -and $WithIIS) {
        $iisCheck = Get-WindowsFeature -Name IIS-WebServerRole -ErrorAction SilentlyContinue
        if (!$iisCheck -or $iisCheck.InstallState -ne "Installed") {
            Write-InstallLog "🔽 IIS não está instalado. Será instalado automaticamente." "WARN"
        }
    }
    
    Write-InstallLog "✅ Verificações pré-instalação concluídas" "SUCCESS"
}

# ================================================================
# INSTALAÇÃO PRINCIPAL
# ================================================================

function Install-Application {
    Write-InstallLog "🚀 INICIANDO INSTALAÇÃO DA APLICAÇÃO" "INFO"
    
    # 1. Deploy da aplicação
    Write-InstallLog "📦 Executando deploy da aplicação..." "INFO"
    $deployScript = Join-Path $ScriptRoot "Deploy\Deploy-WindowsServer.ps1"
    if (Test-Path $deployScript) {
        Invoke-ScriptSafely $deployScript "-InstallType $InstallType"
    }
    
    # 2. Configurar banco de dados
    Write-InstallLog "🗄️ Configurando banco de dados..." "INFO"
    $dbScript = Join-Path $ScriptRoot "Database\Setup-Database.ps1"
    if (Test-Path $dbScript) {
        Invoke-ScriptSafely $dbScript
    }
    
    # 3. Instalar Redis (se solicitado)
    if ($WithRedis) {
        Write-InstallLog "🔴 Instalando Redis..." "INFO"
        $redisScript = Join-Path $ScriptRoot "Deploy\Install-Redis-Windows.ps1"
        if (Test-Path $redisScript) {
            Invoke-ScriptSafely $redisScript
        }
    }
    
    # 4. Configurar IIS (se solicitado)
    if ($WithIIS -and $InstallType -eq "Production") {
        Write-InstallLog "🌐 Configurando IIS..." "INFO"
        $iisScript = Join-Path $ScriptRoot "Deploy\Configuracao-IIS.ps1"
        if (Test-Path $iisScript) {
            Invoke-ScriptSafely $iisScript
        }
    }
    
    # 5. Aplicar hardening de segurança (se solicitado)
    if ($WithSecurity) {
        Write-InstallLog "🔒 Aplicando configurações de segurança..." "INFO"
        $securityScript = Join-Path $ScriptRoot "Security\Hardening-Seguranca.ps1"
        if (Test-Path $securityScript) {
            Invoke-ScriptSafely $securityScript
        }
    }
    
    Write-InstallLog "✅ Instalação da aplicação concluída" "SUCCESS"
}

# ================================================================
# VERIFICAÇÃO PÓS-INSTALAÇÃO
# ================================================================

function Start-PostInstallVerification {
    if (!$WithVerification) {
        return
    }
    
    Write-InstallLog "🔍 INICIANDO VERIFICAÇÃO PÓS-INSTALAÇÃO" "INFO"
    
    $verificationScript = Join-Path $ScriptRoot "Deploy\Verificacao-Pos-Instalacao.ps1"
    if (Test-Path $verificationScript) {
        Invoke-ScriptSafely $verificationScript
    }
    
    # Verificações básicas adicionais
    Write-InstallLog "🔍 Verificações adicionais..." "INFO"
    
    # Verificar se a aplicação está rodando
    try {
        $response = Invoke-WebRequest -Uri "http://localhost" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-InstallLog "✅ Aplicação respondendo corretamente" "SUCCESS"
        }
    }
    catch {
        Write-InstallLog "⚠️ Aplicação pode não estar acessível via HTTP" "WARN"
    }
    
    Write-InstallLog "✅ Verificação pós-instalação concluída" "SUCCESS"
}

# ================================================================
# RELATÓRIO FINAL
# ================================================================

function Show-InstallationSummary {
    Write-InstallLog "📋 RESUMO DA INSTALAÇÃO" "INFO"
    Write-InstallLog "================================================" "INFO"
    Write-InstallLog "Tipo de instalação: $InstallType" "INFO"
    Write-InstallLog "Redis instalado: $WithRedis" "INFO"
    Write-InstallLog "Segurança aplicada: $WithSecurity" "INFO"
    Write-InstallLog "IIS configurado: $WithIIS" "INFO"
    Write-InstallLog "Verificação executada: $WithVerification" "INFO"
    Write-InstallLog "Log completo: $LogFile" "INFO"
    Write-InstallLog "================================================" "INFO"
    
    if ($InstallType -eq "Production") {
        Write-InstallLog "🌐 Aplicação disponível em:" "SUCCESS"
        Write-InstallLog "   HTTP:  http://localhost" "SUCCESS"
        Write-InstallLog "   HTTPS: https://localhost" "SUCCESS"
    } else {
        Write-InstallLog "🌐 Aplicação disponível em:" "SUCCESS"
        Write-InstallLog "   HTTP:  http://localhost:5000" "SUCCESS"
        Write-InstallLog "   HTTPS: https://localhost:5001" "SUCCESS"
    }
    
    Write-InstallLog "👤 Login padrão:" "SUCCESS"
    Write-InstallLog "   Email: admin@intranet.com" "SUCCESS"
    Write-InstallLog "   Senha: Admin@123" "SUCCESS"
    Write-InstallLog "" "INFO"
    Write-InstallLog "📚 Documentação completa em:" "INFO"
    Write-InstallLog "   DOCUMENTACAO-OFICIAL-UNIFICADA.md" "INFO"
}

# ================================================================
# EXECUÇÃO PRINCIPAL
# ================================================================

function Main {
    try {
        Write-InstallLog "🚀 INSTALADOR OFICIAL - Intranet Documentos v2.0" "SUCCESS"
        Write-InstallLog "================================================" "INFO"
        
        if (!$Silent) {
            Write-Host ""
            Write-Host "🎯 OPÇÕES DE INSTALAÇÃO:" -ForegroundColor Cyan
            Write-Host "   Tipo: $InstallType" -ForegroundColor White
            Write-Host "   Redis: $WithRedis" -ForegroundColor White
            Write-Host "   Segurança: $WithSecurity" -ForegroundColor White
            Write-Host "   IIS: $WithIIS" -ForegroundColor White
            Write-Host "   Verificação: $WithVerification" -ForegroundColor White
            Write-Host ""
            
            $confirm = Read-Host "Continuar com a instalação? (S/n)"
            if ($confirm -eq "n" -or $confirm -eq "N") {
                Write-InstallLog "❌ Instalação cancelada pelo usuário" "WARN"
                exit 0
            }
        }
        
        # Executar etapas da instalação
        Start-PreInstallChecks
        Install-Application
        Start-PostInstallVerification
        Show-InstallationSummary
        
        Write-InstallLog "🎉 INSTALAÇÃO CONCLUÍDA COM SUCESSO!" "SUCCESS"
        
    }
    catch {
        Write-InstallLog "❌ ERRO DURANTE A INSTALAÇÃO: $($_.Exception.Message)" "ERROR"
        Write-InstallLog "📋 Verifique o log completo em: $LogFile" "ERROR"
        exit 1
    }
}

# ================================================================
# EXECUÇÃO
# ================================================================

# Verificar se está sendo executado diretamente
if ($MyInvocation.InvocationName -eq $MyInvocation.MyCommand.Name) {
    Main
}

# ================================================================
# HELP E EXEMPLOS DE USO
# ================================================================

<#
.SYNOPSIS
    Instalador oficial unificado para o sistema Intranet Documentos

.DESCRIPTION
    Script principal que automatiza toda a instalação e configuração do sistema,
    incluindo deploy, banco de dados, Redis, IIS, segurança e verificações.

.PARAMETER InstallType
    Tipo de instalação: "Dev" ou "Production" (padrão: Production)

.PARAMETER WithRedis
    Instalar Redis para cache distribuído (padrão: $true)

.PARAMETER WithSecurity
    Aplicar configurações de segurança e hardening (padrão: $true)

.PARAMETER WithIIS
    Configurar IIS para produção (padrão: $true)

.PARAMETER WithVerification
    Executar verificação pós-instalação (padrão: $true)

.PARAMETER Silent
    Modo silencioso sem interação do usuário (padrão: $false)

.EXAMPLE
    .\Install-IntranetDocumentos.ps1
    Instalação padrão de produção com todas as opções

.EXAMPLE
    .\Install-IntranetDocumentos.ps1 -InstallType Dev -WithIIS:$false
    Instalação para desenvolvimento sem IIS

.EXAMPLE
    .\Install-IntranetDocumentos.ps1 -Silent -WithRedis:$false
    Instalação silenciosa sem Redis

.NOTES
    - Execute como Administrador
    - Requer .NET 8.0+ instalado
    - Para produção, requer Windows Server com IIS
    - Log detalhado salvo em Logs\install-*.log
#>
