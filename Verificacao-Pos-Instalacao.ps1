# Verificacao-Pos-Instalacao.ps1
# Script para verificar se a instalação da Intranet Documentos está funcionando corretamente

param(
    [string]$SiteUrl = "http://localhost",
    [string]$SitePath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [string]$DataPath = "C:\IntranetData",
    [string]$AppPoolName = "IntranetDocumentos",
    [string]$SiteName = "Intranet Documentos"
)

$ErrorActionPreference = "Continue"
$issues = @()
$warnings = @()

Write-Host "=== Verificação Pós-Instalação - Intranet Documentos ===" -ForegroundColor Green
Write-Host "Data/Hora: $(Get-Date)" -ForegroundColor Cyan
Write-Host

# Função para adicionar issue
function Add-Issue($message, $severity = "Error") {
    if ($severity -eq "Error") {
        $script:issues += $message
        Write-Host "❌ $message" -ForegroundColor Red
    } else {
        $script:warnings += $message
        Write-Host "⚠️  $message" -ForegroundColor Yellow
    }
}

# Função para sucesso
function Add-Success($message) {
    Write-Host "✅ $message" -ForegroundColor Green
}

# 1. Verificar .NET Runtime
Write-Host "1. Verificando .NET Runtime..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "9.*") {
        Add-Success ".NET 9.0 Runtime encontrado: $dotnetVersion"
    } else {
        Add-Issue ".NET 9.0 não encontrado. Versão atual: $dotnetVersion"
    }
} catch {
    Add-Issue ".NET Runtime não está instalado ou não está no PATH"
}

# 2. Verificar IIS
Write-Host "`n2. Verificando IIS e Application Pool..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    
    # Verificar Application Pool
    $appPool = Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    if ($appPool) {
        if ($appPool.State -eq "Started") {
            Add-Success "Application Pool '$AppPoolName' está rodando"
        } else {
            Add-Issue "Application Pool '$AppPoolName' não está rodando. Estado: $($appPool.State)"
        }
    } else {
        Add-Issue "Application Pool '$AppPoolName' não encontrado"
    }
    
    # Verificar Site
    $site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
    if ($site) {
        if ($site.State -eq "Started") {
            Add-Success "Site '$SiteName' está rodando"
        } else {
            Add-Issue "Site '$SiteName' não está rodando. Estado: $($site.State)"
        }
    } else {
        Add-Issue "Site '$SiteName' não encontrado"
    }
    
} catch {
    Add-Issue "Erro ao verificar IIS: $($_.Exception.Message)"
}

# 3. Verificar arquivos da aplicação
Write-Host "`n3. Verificando arquivos da aplicação..." -ForegroundColor Yellow
$requiredFiles = @(
    "$SitePath\IntranetDocumentos.dll",
    "$SitePath\web.config",
    "$SitePath\appsettings.json",
    "$SitePath\appsettings.Production.json"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Add-Success "Arquivo encontrado: $(Split-Path $file -Leaf)"
    } else {
        Add-Issue "Arquivo não encontrado: $file"
    }
}

# 4. Verificar estrutura de diretórios
Write-Host "`n4. Verificando estrutura de diretórios..." -ForegroundColor Yellow
$requiredDirs = @(
    "$DataPath\Documents",
    "$DataPath\Backups",
    "$DataPath\Documents\Geral",
    "$DataPath\Documents\Pessoal",
    "$DataPath\Documents\Fiscal",
    "$DataPath\Documents\Contabil",
    "$DataPath\Documents\Cadastro",
    "$DataPath\Documents\Apoio",
    "$DataPath\Documents\TI",
    "$SitePath\logs"
)

foreach ($dir in $requiredDirs) {
    if (Test-Path $dir) {
        Add-Success "Diretório encontrado: $(Split-Path $dir -Leaf)"
    } else {
        Add-Issue "Diretório não encontrado: $dir" "Warning"
    }
}

# 5. Verificar permissões
Write-Host "`n5. Verificando permissões..." -ForegroundColor Yellow
try {
    # Testar escrita no diretório de logs
    $testFile = "$SitePath\logs\test.txt"
    try {
        "test" | Out-File $testFile -Force
        Remove-Item $testFile -Force
        Add-Success "Permissões de escrita OK para logs"
    } catch {
        Add-Issue "Sem permissão de escrita no diretório de logs"
    }
    
    # Testar escrita no diretório de dados
    $testDataFile = "$DataPath\Documents\test.txt"
    try {
        "test" | Out-File $testDataFile -Force
        Remove-Item $testDataFile -Force
        Add-Success "Permissões de escrita OK para dados"
    } catch {
        Add-Issue "Sem permissão de escrita no diretório de dados"
    }
    
} catch {
    Add-Issue "Erro ao verificar permissões: $($_.Exception.Message)" "Warning"
}

# 6. Verificar conectividade HTTP
Write-Host "`n6. Verificando conectividade HTTP..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $SiteUrl -Method HEAD -TimeoutSec 10 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Add-Success "Site responde corretamente na URL: $SiteUrl"
    } else {
        Add-Issue "Site retornou status: $($response.StatusCode)"
    }
} catch {
    Add-Issue "Erro ao acessar $SiteUrl : $($_.Exception.Message)"
}

# 7. Verificar logs de erro recentes
Write-Host "`n7. Verificando logs de erro recentes..." -ForegroundColor Yellow
try {
    # Verificar Event Log
    $recentEvents = Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IntranetDocumentos'; Level=2; StartTime=(Get-Date).AddHours(-1)} -MaxEvents 5 -ErrorAction SilentlyContinue
    if ($recentEvents) {
        Add-Issue "Encontrados $($recentEvents.Count) erros no Event Log na última hora" "Warning"
        foreach ($logEvent in $recentEvents) {
            Write-Host "   - $($logEvent.TimeCreated): $($logEvent.LevelDisplayName): $($logEvent.Message.Substring(0, [Math]::Min(100, $logEvent.Message.Length)))..." -ForegroundColor Gray
        }
    } else {
        Add-Success "Nenhum erro recente encontrado no Event Log"
    }
} catch {
    Add-Issue "Não foi possível verificar Event Log: $($_.Exception.Message)" "Warning"
}

# 8. Verificar configuração
Write-Host "`n8. Verificando configuração..." -ForegroundColor Yellow
try {
    $configFile = "$SitePath\appsettings.Production.json"
    if (Test-Path $configFile) {
        $config = Get-Content $configFile | ConvertFrom-Json
        
        # Verificar connection string
        if ($config.ConnectionStrings.DefaultConnection -like "*CHANGE_THIS_PASSWORD*") {
            Add-Issue "Connection string contém senha padrão - deve ser configurada"
        } else {
            Add-Success "Connection string parece estar configurada"
        }
        
        # Verificar email settings
        if ($config.EmailSettings.SmtpPassword -like "*CHANGE_THIS*") {
            Add-Issue "Configuração de email contém senha padrão" "Warning"
        } else {
            Add-Success "Configuração de email parece estar configurada"
        }
    }
} catch {
    Add-Issue "Erro ao verificar configuração: $($_.Exception.Message)" "Warning"
}

# 9. Verificar MySQL/MariaDB
Write-Host "`n9. Verificando banco de dados..." -ForegroundColor Yellow
try {
    # Tentar conectar usando mysql client se disponível
    mysql --version 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Add-Success "MySQL client encontrado"
        
        # Tentar testar conexão básica
        try {
            mysql -u root -e "SELECT 1;" 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Add-Success "Conexão básica com MySQL OK"
            } else {
                Add-Issue "Não foi possível conectar ao MySQL com usuário root" "Warning"
            }
        } catch {
            Add-Issue "Erro ao testar conexão MySQL" "Warning"
        }
    } else {
        Add-Issue "MySQL client não encontrado no PATH" "Warning"
    }
} catch {
    Add-Issue "Erro ao verificar MySQL: $($_.Exception.Message)" "Warning"
}

# 10. Resultado final
Write-Host "`n" + "="*60 -ForegroundColor Cyan
Write-Host "RESULTADO DA VERIFICAÇÃO" -ForegroundColor Cyan
Write-Host "="*60 -ForegroundColor Cyan

if ($issues.Count -eq 0) {
    Write-Host "`n🎉 SUCESSO! Nenhum problema crítico encontrado." -ForegroundColor Green
} else {
    Write-Host "`n❌ PROBLEMAS ENCONTRADOS ($($issues.Count)):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "   • $issue" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠️  AVISOS ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   • $warning" -ForegroundColor Yellow
    }
}

# Recomendações
Write-Host "`n💡 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
if ($issues.Count -gt 0) {
    Write-Host "   1. Corrija os problemas críticos listados acima" -ForegroundColor White
    Write-Host "   2. Execute este script novamente para verificar" -ForegroundColor White
} else {
    Write-Host "   1. Acesse a aplicação: $SiteUrl" -ForegroundColor White
    Write-Host "   2. Faça login com: admin@empresa.com / Admin123!" -ForegroundColor White
    Write-Host "   3. Teste upload/download de documentos" -ForegroundColor White
    Write-Host "   4. Configure usuários e departamentos" -ForegroundColor White
}

Write-Host "`n📋 RELATÓRIO SALVO EM:" -ForegroundColor Cyan
$reportPath = "$env:TEMP\IntranetDocumentos_Verification_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
$report = @"
=== RELATÓRIO DE VERIFICAÇÃO - INTRANET DOCUMENTOS ===
Data: $(Get-Date)
Site URL: $SiteUrl
Site Path: $SitePath

PROBLEMAS CRÍTICOS: $($issues.Count)
$(if ($issues.Count -gt 0) { ($issues | ForEach-Object { "• $_" }) -join "`n" } else { "Nenhum" })

AVISOS: $($warnings.Count)
$(if ($warnings.Count -gt 0) { ($warnings | ForEach-Object { "• $_" }) -join "`n" } else { "Nenhum" })

STATUS: $(if ($issues.Count -eq 0) { "✅ SUCESSO" } else { "❌ REQUER ATENÇÃO" })
"@

$report | Out-File $reportPath -Encoding UTF8
Write-Host "   $reportPath" -ForegroundColor White

Write-Host "`n🔧 Para suporte adicional, consulte:" -ForegroundColor Cyan
Write-Host "   • INSTALL-GUIDE.md - Guia de instalação" -ForegroundColor White
Write-Host "   • DEPLOY-GUIDE.md - Guia de deployment" -ForegroundColor White
Write-Host "   • Event Viewer - Logs do sistema" -ForegroundColor White
