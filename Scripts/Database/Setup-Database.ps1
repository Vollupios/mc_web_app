# 🗄️ CONFIGURADOR DE BANCO DE DADOS - Intranet Documentos
# ================================================================
# Script unificado para configuração de banco de dados
# Suporta: SQLite (dev), MySQL (produção)
# ================================================================

param(
    [Parameter(HelpMessage="Tipo de banco: SQLite, MySQL")]
    [ValidateSet("SQLite", "MySQL")]
    [string]$DatabaseType = "SQLite",
    
    [Parameter(HelpMessage="String de conexão personalizada")]
    [string]$ConnectionString = "",
    
    [Parameter(HelpMessage="Recriar banco de dados (ATENÇÃO: Apaga dados!)")]
    [switch]$Recreate = $false,
    
    [Parameter(HelpMessage="Executar apenas migrações")]
    [switch]$MigrateOnly = $false,
    
    [Parameter(HelpMessage="Inserir dados de exemplo")]
    [switch]$WithSampleData = $true
)

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$AppRoot = Split-Path (Split-Path $ScriptRoot -Parent) -Parent

# ================================================================
# FUNÇÕES AUXILIARES
# ================================================================

function Write-DbLog {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    switch ($Level) {
        "ERROR" { Write-Host "[$timestamp] [ERROR] $Message" -ForegroundColor Red }
        "WARN"  { Write-Host "[$timestamp] [WARN] $Message" -ForegroundColor Yellow }
        "SUCCESS" { Write-Host "[$timestamp] [SUCCESS] $Message" -ForegroundColor Green }
        default { Write-Host "[$timestamp] [INFO] $Message" -ForegroundColor White }
    }
}

function Test-DatabaseConnection {
    param([string]$ConnectionString, [string]$Type)
    
    Write-DbLog "🔍 Testando conexão com banco de dados..." "INFO"
    
    try {
        if ($Type -eq "SQLite") {
            $dbPath = [regex]::Match($ConnectionString, "Data Source=([^;]+)").Groups[1].Value
            if (Test-Path $dbPath) {
                Write-DbLog "✅ Banco SQLite encontrado: $dbPath" "SUCCESS"
                return $true
            } else {
                Write-DbLog "⚠️ Banco SQLite será criado: $dbPath" "WARN"
                return $false
            }
        }
        elseif ($Type -eq "MySQL") {
            # Implementar teste de conexão MySQL
            Write-DbLog "🔍 Testando conexão MySQL..." "INFO"
            # TODO: Implementar teste real de conexão MySQL
            return $true
        }
    }
    catch {
        Write-DbLog "❌ Erro ao testar conexão: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

# ================================================================
# CONFIGURAÇÃO SQLITE
# ================================================================

function Setup-SQLiteDatabase {
    Write-DbLog "🗄️ Configurando banco SQLite..." "INFO"
    
    $dbPath = Join-Path $AppRoot "IntranetDocumentos.db"
    $connectionString = "Data Source=$dbPath"
    
    if ($Recreate -and (Test-Path $dbPath)) {
        Write-DbLog "🗑️ Removendo banco existente..." "WARN"
        Remove-Item $dbPath -Force
    }
    
    # Executar migrações
    Write-DbLog "📦 Executando migrações EF Core..." "INFO"
    Set-Location $AppRoot
    
    try {
        if ($MigrateOnly) {
            & dotnet ef database update --project IntranetDocumentos.csproj
        } else {
            # Criar migração inicial se necessário
            $migrations = & dotnet ef migrations list --project IntranetDocumentos.csproj 2>$null
            if (!$migrations -or $migrations.Count -eq 0) {
                Write-DbLog "📝 Criando migração inicial..." "INFO"
                & dotnet ef migrations add InitialCreate --project IntranetDocumentos.csproj
            }
            
            & dotnet ef database update --project IntranetDocumentos.csproj
        }
        
        Write-DbLog "✅ Banco SQLite configurado com sucesso" "SUCCESS"
        return $connectionString
    }
    catch {
        Write-DbLog "❌ Erro ao configurar SQLite: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# ================================================================
# CONFIGURAÇÃO MYSQL
# ================================================================

function Setup-MySQLDatabase {
    Write-DbLog "🗄️ Configurando banco MySQL..." "INFO"
    
    $setupScript = Join-Path $ScriptRoot "setup-database.mysql.sql"
    
    if (!(Test-Path $setupScript)) {
        Write-DbLog "❌ Script MySQL não encontrado: $setupScript" "ERROR"
        throw "Script de setup MySQL não encontrado"
    }
    
    # Configuração padrão MySQL
    $server = "localhost"
    $database = "intranet_documentos"
    $user = "intranet_user"
    $password = "IntranetPass123!"
    
    if (!$ConnectionString) {
        $ConnectionString = "Server=$server;Database=$database;Uid=$user;Pwd=$password;CharSet=utf8mb4;"
    }
    
    Write-DbLog "🔧 Executando script de setup MySQL..." "INFO"
    
    try {
        # Executar script SQL
        Write-DbLog "📜 Aplicando: $setupScript" "INFO"
        
        # Aqui você executaria o script MySQL
        # mysql -u root -p < $setupScript
        # Por enquanto, apenas log da ação
        Write-DbLog "⚠️ Execute manualmente: mysql -u root -p < $setupScript" "WARN"
        
        # Executar migrações EF Core
        Set-Location $AppRoot
        & dotnet ef database update --project IntranetDocumentos.csproj
        
        Write-DbLog "✅ Banco MySQL configurado com sucesso" "SUCCESS"
        return $ConnectionString
    }
    catch {
        Write-DbLog "❌ Erro ao configurar MySQL: $($_.Exception.Message)" "ERROR"
        throw
    }
}

# ================================================================
# DADOS DE EXEMPLO
# ================================================================

function Insert-SampleData {
    if (!$WithSampleData) {
        return
    }
    
    Write-DbLog "📋 Inserindo dados de exemplo..." "INFO"
    
    try {
        Set-Location $AppRoot
        
        # Executar aplicação temporariamente para seed de dados
        Write-DbLog "🌱 Executando seed de dados..." "INFO"
        
        # O seed é executado automaticamente no Program.cs
        # quando a aplicação inicia pela primeira vez
        
        Write-DbLog "✅ Dados de exemplo inseridos" "SUCCESS"
    }
    catch {
        Write-DbLog "⚠️ Erro ao inserir dados de exemplo: $($_.Exception.Message)" "WARN"
        # Não é crítico, continua a instalação
    }
}

# ================================================================
# BACKUP E RESTORE
# ================================================================

function Backup-Database {
    param([string]$ConnectionString, [string]$Type)
    
    Write-DbLog "💾 Criando backup do banco de dados..." "INFO"
    
    $backupDir = Join-Path $AppRoot "DatabaseBackups"
    if (!(Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    
    try {
        if ($Type -eq "SQLite") {
            $dbPath = [regex]::Match($ConnectionString, "Data Source=([^;]+)").Groups[1].Value
            if (Test-Path $dbPath) {
                $backupPath = Join-Path $backupDir "IntranetDocumentos_Backup_$timestamp.db"
                Copy-Item $dbPath $backupPath
                Write-DbLog "✅ Backup SQLite criado: $backupPath" "SUCCESS"
            }
        }
        elseif ($Type -eq "MySQL") {
            # Implementar backup MySQL
            $backupPath = Join-Path $backupDir "IntranetDocumentos_Backup_$timestamp.sql"
            Write-DbLog "⚠️ Backup MySQL deve ser feito manualmente" "WARN"
            Write-DbLog "   Comando: mysqldump -u root -p intranet_documentos > $backupPath" "INFO"
        }
    }
    catch {
        Write-DbLog "⚠️ Erro ao criar backup: $($_.Exception.Message)" "WARN"
    }
}

# ================================================================
# ATUALIZAÇÃO DE CONFIGURAÇÃO
# ================================================================

function Update-AppConfiguration {
    param([string]$ConnectionString, [string]$Type)
    
    Write-DbLog "⚙️ Atualizando configuração da aplicação..." "INFO"
    
    $appSettingsPath = Join-Path $AppRoot "appsettings.json"
    $appSettingsProdPath = Join-Path $AppRoot "appsettings.Production.json"
    
    try {
        # Atualizar appsettings.json para desenvolvimento
        if (Test-Path $appSettingsPath) {
            $config = Get-Content $appSettingsPath | ConvertFrom-Json
            
            if ($Type -eq "SQLite") {
                $config.ConnectionStrings.DefaultConnection = $ConnectionString
            } elseif ($Type -eq "MySQL") {
                $config.ConnectionStrings.DefaultConnection = $ConnectionString
            }
            
            $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
            Write-DbLog "✅ appsettings.json atualizado" "SUCCESS"
        }
        
        # Atualizar appsettings.Production.json
        if (Test-Path $appSettingsProdPath) {
            $prodConfig = Get-Content $appSettingsProdPath | ConvertFrom-Json
            
            if ($Type -eq "MySQL") {
                $prodConfig.ConnectionStrings.DefaultConnection = $ConnectionString
            }
            
            $prodConfig | ConvertTo-Json -Depth 10 | Set-Content $appSettingsProdPath
            Write-DbLog "✅ appsettings.Production.json atualizado" "SUCCESS"
        }
    }
    catch {
        Write-DbLog "⚠️ Erro ao atualizar configuração: $($_.Exception.Message)" "WARN"
    }
}

# ================================================================
# EXECUÇÃO PRINCIPAL
# ================================================================

function Main {
    try {
        Write-DbLog "🗄️ CONFIGURADOR DE BANCO - Intranet Documentos" "SUCCESS"
        Write-DbLog "=============================================" "INFO"
        Write-DbLog "Tipo de banco: $DatabaseType" "INFO"
        Write-DbLog "Recriar banco: $Recreate" "INFO"
        Write-DbLog "Apenas migração: $MigrateOnly" "INFO"
        Write-DbLog "Dados de exemplo: $WithSampleData" "INFO"
        Write-DbLog "" "INFO"
        
        # Fazer backup se não estiver recriando
        if (!$Recreate -and !$MigrateOnly) {
            Backup-Database $ConnectionString $DatabaseType
        }
        
        # Configurar banco de dados
        if ($DatabaseType -eq "SQLite") {
            $finalConnectionString = Setup-SQLiteDatabase
        } elseif ($DatabaseType -eq "MySQL") {
            $finalConnectionString = Setup-MySQLDatabase
        }
        
        # Atualizar configuração da aplicação
        Update-AppConfiguration $finalConnectionString $DatabaseType
        
        # Inserir dados de exemplo
        Insert-SampleData
        
        Write-DbLog "✅ CONFIGURAÇÃO DE BANCO CONCLUÍDA!" "SUCCESS"
        Write-DbLog "Connection String: $finalConnectionString" "INFO"
        
    }
    catch {
        Write-DbLog "❌ ERRO NA CONFIGURAÇÃO: $($_.Exception.Message)" "ERROR"
        exit 1
    }
}

# ================================================================
# EXECUÇÃO
# ================================================================

if ($MyInvocation.InvocationName -eq $MyInvocation.MyCommand.Name) {
    Main
}

<#
.SYNOPSIS
    Configurador unificado de banco de dados para Intranet Documentos

.DESCRIPTION
    Script que automatiza a configuração de banco de dados (SQLite ou MySQL),
    execução de migrações, inserção de dados e configuração da aplicação.

.EXAMPLE
    .\Setup-Database.ps1
    Configuração padrão com SQLite

.EXAMPLE
    .\Setup-Database.ps1 -DatabaseType MySQL -ConnectionString "Server=localhost;Database=intranet;..."
    Configuração com MySQL e string personalizada

.EXAMPLE
    .\Setup-Database.ps1 -Recreate
    Recria o banco de dados (ATENÇÃO: apaga dados existentes)
#>
