# 🛠️ FERRAMENTAS DE DESENVOLVIMENTO - Intranet Documentos
# ================================================================
# Script unificado para tarefas de desenvolvimento
# ================================================================

param(
    [Parameter(Position=0, HelpMessage="Ação a executar")]
    [ValidateSet("run", "build", "test", "migrate", "seed", "clean", "watch", "publish", "check", "reset")]
    [string]$Action = "run",
    
    [Parameter(HelpMessage="Configuração: Debug, Release")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter(HelpMessage="Ambiente: Development, Production")]
    [string]$Environment = "Development",
    
    [Parameter(HelpMessage="Porta para desenvolvimento")]
    [int]$Port = 5000,
    
    [Parameter(HelpMessage="Executar em modo watch")]
    [switch]$Watch = $false
)

$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$AppRoot = Split-Path (Split-Path $ScriptRoot -Parent) -Parent

# ================================================================
# FUNÇÕES AUXILIARES
# ================================================================

function Write-DevLog {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    switch ($Level) {
        "ERROR" { Write-Host "[$timestamp] ❌ $Message" -ForegroundColor Red }
        "WARN"  { Write-Host "[$timestamp] ⚠️ $Message" -ForegroundColor Yellow }
        "SUCCESS" { Write-Host "[$timestamp] ✅ $Message" -ForegroundColor Green }
        "INFO" { Write-Host "[$timestamp] 🔧 $Message" -ForegroundColor Cyan }
        default { Write-Host "[$timestamp] 📋 $Message" -ForegroundColor White }
    }
}

function Test-DevPrerequisites {
    Write-DevLog "Verificando pré-requisitos..." "INFO"
    
    # Verificar .NET
    try {
        $dotnetVersion = dotnet --version
        Write-DevLog ".NET $dotnetVersion encontrado" "SUCCESS"
    }
    catch {
        Write-DevLog ".NET não encontrado!" "ERROR"
        return $false
    }
    
    # Verificar projeto
    $projectFile = Join-Path $AppRoot "IntranetDocumentos.csproj"
    if (!(Test-Path $projectFile)) {
        Write-DevLog "Arquivo de projeto não encontrado: $projectFile" "ERROR"
        return $false
    }
    
    Write-DevLog "Pré-requisitos verificados" "SUCCESS"
    return $true
}

# ================================================================
# AÇÕES DE DESENVOLVIMENTO
# ================================================================

function Invoke-Run {
    Write-DevLog "🚀 Iniciando aplicação..." "INFO"
    
    Set-Location $AppRoot
    
    $env:ASPNETCORE_ENVIRONMENT = $Environment
    $env:ASPNETCORE_URLS = "http://localhost:$Port;https://localhost:$($Port + 1)"
    
    if ($Watch) {
        Write-DevLog "Modo watch ativado - aplicação será recarregada automaticamente" "INFO"
        dotnet watch run --project IntranetDocumentos.csproj
    } else {
        dotnet run --project IntranetDocumentos.csproj
    }
}

function Invoke-Build {
    Write-DevLog "🔨 Compilando aplicação..." "INFO"
    
    Set-Location $AppRoot
    dotnet build IntranetDocumentos.csproj --configuration $Configuration --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-DevLog "Compilação concluída com sucesso" "SUCCESS"
    } else {
        Write-DevLog "Erro na compilação" "ERROR"
    }
}

function Invoke-Test {
    Write-DevLog "🧪 Executando testes..." "INFO"
    
    Set-Location $AppRoot
    
    $testProjects = Get-ChildItem -Path . -Filter "*.Tests.csproj" -Recurse
    
    if ($testProjects.Count -eq 0) {
        Write-DevLog "Nenhum projeto de teste encontrado" "WARN"
        return
    }
    
    foreach ($testProject in $testProjects) {
        Write-DevLog "Executando testes: $($testProject.Name)" "INFO"
        dotnet test $testProject.FullName --configuration $Configuration --verbosity minimal
    }
}

function Invoke-Migrate {
    Write-DevLog "📦 Executando migrações..." "INFO"
    
    Set-Location $AppRoot
    
    try {
        # Listar migrações pendentes
        $pendingMigrations = dotnet ef migrations list --project IntranetDocumentos.csproj --no-build 2>$null
        
        if ($pendingMigrations) {
            Write-DevLog "Aplicando migrações ao banco de dados..." "INFO"
            dotnet ef database update --project IntranetDocumentos.csproj
            Write-DevLog "Migrações aplicadas com sucesso" "SUCCESS"
        } else {
            Write-DevLog "Nenhuma migração pendente" "INFO"
        }
    }
    catch {
        Write-DevLog "Erro ao executar migrações: $($_.Exception.Message)" "ERROR"
    }
}

function Invoke-Seed {
    Write-DevLog "🌱 Inserindo dados de exemplo..." "INFO"
    
    # Os dados são inseridos automaticamente quando a aplicação inicia
    # através do DatabaseInitializer no Program.cs
    Write-DevLog "Execute a aplicação para inserir dados automaticamente" "INFO"
}

function Invoke-Clean {
    Write-DevLog "🧹 Limpando arquivos de build..." "INFO"
    
    Set-Location $AppRoot
    
    # Limpar bin e obj
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
    
    # Limpar cache do NuGet
    dotnet nuget locals all --clear
    
    Write-DevLog "Limpeza concluída" "SUCCESS"
}

function Invoke-Watch {
    Write-DevLog "👀 Iniciando modo watch..." "INFO"
    
    Set-Location $AppRoot
    $env:ASPNETCORE_ENVIRONMENT = $Environment
    
    dotnet watch run --project IntranetDocumentos.csproj
}

function Invoke-Publish {
    Write-DevLog "📦 Publicando aplicação..." "INFO"
    
    Set-Location $AppRoot
    $publishPath = Join-Path $AppRoot "publish"
    
    if (Test-Path $publishPath) {
        Remove-Item $publishPath -Recurse -Force
    }
    
    dotnet publish IntranetDocumentos.csproj `
        --configuration Release `
        --output $publishPath `
        --self-contained false
    
    if ($LASTEXITCODE -eq 0) {
        Write-DevLog "Aplicação publicada em: $publishPath" "SUCCESS"
    } else {
        Write-DevLog "Erro na publicação" "ERROR"
    }
}

function Invoke-Check {
    Write-DevLog "🔍 Verificando integridade do projeto..." "INFO"
    
    Set-Location $AppRoot
    
    # Verificar arquivos essenciais
    $essentialFiles = @(
        "IntranetDocumentos.csproj",
        "Program.cs",
        "appsettings.json",
        "Views/Shared/_Layout.cshtml"
    )
    
    foreach ($file in $essentialFiles) {
        if (Test-Path $file) {
            Write-DevLog "✓ $file" "SUCCESS"
        } else {
            Write-DevLog "✗ $file (não encontrado)" "ERROR"
        }
    }
    
    # Verificar dependências
    Write-DevLog "Verificando dependências..." "INFO"
    dotnet restore --verbosity minimal
    
    # Verificar compilação
    Write-DevLog "Verificando compilação..." "INFO"
    dotnet build --configuration Debug --verbosity minimal --no-restore
    
    Write-DevLog "Verificação concluída" "SUCCESS"
}

function Invoke-Reset {
    Write-DevLog "🔄 Resetando ambiente de desenvolvimento..." "WARN"
    
    $confirm = Read-Host "ATENÇÃO: Isso irá remover o banco de dados e arquivos de build. Continuar? (s/N)"
    if ($confirm -ne "s" -and $confirm -ne "S") {
        Write-DevLog "Reset cancelado" "INFO"
        return
    }
    
    Set-Location $AppRoot
    
    # Remover banco SQLite
    $dbFile = "IntranetDocumentos.db"
    if (Test-Path $dbFile) {
        Remove-Item $dbFile -Force
        Write-DevLog "Banco de dados removido" "SUCCESS"
    }
    
    # Limpar build
    Invoke-Clean
    
    # Recriar banco
    Invoke-Migrate
    
    Write-DevLog "Reset do ambiente concluído" "SUCCESS"
}

# ================================================================
# MENU INTERATIVO
# ================================================================

function Show-DevMenu {
    Clear-Host
    Write-Host "🛠️ FERRAMENTAS DE DESENVOLVIMENTO - Intranet Documentos" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ações disponíveis:" -ForegroundColor White
    Write-Host ""
    Write-Host "  1. run      - Executar aplicação" -ForegroundColor Green
    Write-Host "  2. build    - Compilar aplicação" -ForegroundColor Yellow
    Write-Host "  3. test     - Executar testes" -ForegroundColor Magenta
    Write-Host "  4. migrate  - Aplicar migrações" -ForegroundColor Blue
    Write-Host "  5. seed     - Inserir dados" -ForegroundColor Cyan
    Write-Host "  6. clean    - Limpar arquivos" -ForegroundColor Red
    Write-Host "  7. watch    - Modo watch" -ForegroundColor Green
    Write-Host "  8. publish  - Publicar app" -ForegroundColor Yellow
    Write-Host "  9. check    - Verificar projeto" -ForegroundColor Blue
    Write-Host " 10. reset    - Reset completo" -ForegroundColor Red
    Write-Host ""
    Write-Host "  0. Sair" -ForegroundColor Gray
    Write-Host ""
    
    $choice = Read-Host "Digite o número da ação"
    
    switch ($choice) {
        "1" { $global:Action = "run" }
        "2" { $global:Action = "build" }
        "3" { $global:Action = "test" }
        "4" { $global:Action = "migrate" }
        "5" { $global:Action = "seed" }
        "6" { $global:Action = "clean" }
        "7" { $global:Action = "watch" }
        "8" { $global:Action = "publish" }
        "9" { $global:Action = "check" }
        "10" { $global:Action = "reset" }
        "0" { exit 0 }
        default { 
            Write-Host "Opção inválida!" -ForegroundColor Red
            Start-Sleep 2
            Show-DevMenu
            return
        }
    }
}

# ================================================================
# EXECUÇÃO PRINCIPAL
# ================================================================

function Main {
    Write-DevLog "FERRAMENTAS DE DESENVOLVIMENTO - Intranet Documentos" "INFO"
    Write-DevLog "====================================================" "INFO"
    
    if (!(Test-DevPrerequisites)) {
        exit 1
    }
    
    # Se nenhuma ação foi especificada, mostrar menu
    if (!$Action -or $Action -eq "") {
        Show-DevMenu
    }
    
    Write-DevLog "Executando ação: $Action" "INFO"
    Write-DevLog "Configuração: $Configuration" "INFO"
    Write-DevLog "Ambiente: $Environment" "INFO"
    Write-DevLog "" "INFO"
    
    switch ($Action.ToLower()) {
        "run"     { Invoke-Run }
        "build"   { Invoke-Build }
        "test"    { Invoke-Test }
        "migrate" { Invoke-Migrate }
        "seed"    { Invoke-Seed }
        "clean"   { Invoke-Clean }
        "watch"   { Invoke-Watch }
        "publish" { Invoke-Publish }
        "check"   { Invoke-Check }
        "reset"   { Invoke-Reset }
        default   { 
            Write-DevLog "Ação inválida: $Action" "ERROR"
            Write-DevLog "Ações válidas: run, build, test, migrate, seed, clean, watch, publish, check, reset" "INFO"
        }
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
    Ferramentas unificadas de desenvolvimento para Intranet Documentos

.DESCRIPTION
    Script que centraliza todas as tarefas de desenvolvimento comuns como
    build, test, migrate, run, etc.

.PARAMETER Action
    Ação a executar: run, build, test, migrate, seed, clean, watch, publish, check, reset

.PARAMETER Configuration
    Configuração de build: Debug ou Release

.PARAMETER Environment
    Ambiente de execução: Development ou Production

.PARAMETER Port
    Porta para desenvolvimento (padrão: 5000)

.PARAMETER Watch
    Executar em modo watch (recarrega automaticamente)

.EXAMPLE
    .\Dev-Tools.ps1
    Mostra menu interativo

.EXAMPLE
    .\Dev-Tools.ps1 run
    Executa a aplicação

.EXAMPLE
    .\Dev-Tools.ps1 run -Watch -Port 3000
    Executa em modo watch na porta 3000

.EXAMPLE
    .\Dev-Tools.ps1 build -Configuration Release
    Compila em modo Release
#>
