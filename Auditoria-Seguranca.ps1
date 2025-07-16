# 🔍 Script de Verificação de Segurança
# Executa auditoria completa de segurança da aplicação

param(
    [string]$AppPath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [switch]$Detailed = $false,
    [switch]$ExportReport = $false
)

Write-Host "🔍 AUDITORIA DE SEGURANÇA - INTRANET DOCUMENTOS" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

$script:securityScore = 100
$script:criticalIssues = 0
$script:warningIssues = 0
$script:recommendations = @()

function Write-SecurityCheck {
    param(
        [string]$Description,
        [string]$Status,  # "PASS", "FAIL", "WARN"
        [string]$Details = "",
        [int]$Impact = 0  # Pontos a deduzir
    )
    
    $icon = switch ($Status) {
        "PASS" { "✅" }
        "FAIL" { "❌" }
        "WARN" { "⚠️ " }
    }
    
    $color = switch ($Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "WARN" { "Yellow" }
    }
    
    Write-Host "$icon $Description" -ForegroundColor $color
    if ($Details -and $Detailed) {
        Write-Host "   $Details" -ForegroundColor Gray
    }
    
    if ($Status -eq "FAIL") {
        $script:criticalIssues++
        $script:securityScore -= $Impact
    } elseif ($Status -eq "WARN") {
        $script:warningIssues++
        $script:securityScore -= ($Impact / 2)
    }
}

function Add-Recommendation {
    param([string]$Recommendation)
    $script:recommendations += $Recommendation
}

try {
    # 1. VERIFICAÇÕES DE CONFIGURAÇÃO
    Write-Host "📋 [1/8] Verificando Configurações Básicas..." -ForegroundColor Yellow
    Write-Host ""

    # 1.1 Verificar se aplicação existe
    if (-not (Test-Path $AppPath)) {
        Write-SecurityCheck "Aplicação encontrada" "FAIL" "Diretório não encontrado: $AppPath" 50
        throw "Aplicação não encontrada"
    } else {
        Write-SecurityCheck "Aplicação encontrada" "PASS" "Localizada em: $AppPath"
    }

    # 1.2 Verificar arquivos críticos
    $criticalFiles = @("appsettings.Production.json", "web.config", "IntranetDocumentos.dll")
    foreach ($file in $criticalFiles) {
        $filePath = Join-Path $AppPath $file
        if (Test-Path $filePath) {
            Write-SecurityCheck "Arquivo crítico: $file" "PASS"
        } else {
            Write-SecurityCheck "Arquivo crítico: $file" "FAIL" "Arquivo não encontrado" 10
        }
    }

    # 2. VERIFICAÇÕES DE AUTENTICAÇÃO
    Write-Host ""
    Write-Host "🔐 [2/8] Verificando Configurações de Autenticação..." -ForegroundColor Yellow
    Write-Host ""

    # 2.1 Verificar senha administrativa padrão
    $appsettingsPath = Join-Path $AppPath "appsettings.Production.json"
    if (Test-Path $appsettingsPath) {
        try {
            $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
            
            if ($appsettings.AdminUser.Password -eq "Admin123!") {
                Write-SecurityCheck "Senha administrativa" "FAIL" "Senha padrão detectada!" 30
                Add-Recommendation "Altere a senha administrativa padrão imediatamente"
            } elseif ($appsettings.AdminUser.Password.Length -lt 8) {
                Write-SecurityCheck "Senha administrativa" "WARN" "Senha muito curta (< 8 caracteres)" 15
                Add-Recommendation "Use senha com pelo menos 12 caracteres"
            } else {
                Write-SecurityCheck "Senha administrativa" "PASS" "Senha não é padrão"
            }

            # 2.2 Verificar configuração de email
            if ($appsettings.EmailSettings.SmtpPassword -match "CHANGE_THIS") {
                Write-SecurityCheck "Configuração de email" "WARN" "Senha de email padrão detectada" 5
                Add-Recommendation "Configure as credenciais de email SMTP"
            } else {
                Write-SecurityCheck "Configuração de email" "PASS"
            }

        } catch {
            Write-SecurityCheck "Arquivo appsettings.json" "FAIL" "Erro ao ler configurações: $($_.Exception.Message)" 20
        }
    }

    # 3. VERIFICAÇÕES DE REDE E WEB CONFIG
    Write-Host ""
    Write-Host "🌐 [3/8] Verificando Configurações Web..." -ForegroundColor Yellow
    Write-Host ""

    # 3.1 Verificar web.config
    $webConfigPath = Join-Path $AppPath "web.config"
    if (Test-Path $webConfigPath) {
        try {
            [xml]$webConfig = Get-Content $webConfigPath
            
            # Verificar headers de segurança
            $securityHeaders = @("X-Content-Type-Options", "X-Frame-Options", "Content-Security-Policy", "Strict-Transport-Security")
            $httpProtocol = $webConfig.configuration.location.'system.webServer'.httpProtocol
            
            if ($httpProtocol -and $httpProtocol.customHeaders) {
                $existingHeaders = $httpProtocol.customHeaders.add | ForEach-Object { $_.name }
                
                foreach ($header in $securityHeaders) {
                    if ($existingHeaders -contains $header) {
                        Write-SecurityCheck "Header de segurança: $header" "PASS"
                    } else {
                        Write-SecurityCheck "Header de segurança: $header" "WARN" "Header ausente" 5
                        Add-Recommendation "Adicione o header de segurança: $header"
                    }
                }
            } else {
                Write-SecurityCheck "Headers de segurança" "FAIL" "Seção de headers não encontrada" 25
                Add-Recommendation "Configure headers de segurança no web.config"
            }

        } catch {
            Write-SecurityCheck "Arquivo web.config" "FAIL" "Erro ao ler web.config: $($_.Exception.Message)" 15
        }
    }

    # 4. VERIFICAÇÕES DE PERMISSÕES
    Write-Host ""
    Write-Host "🔒 [4/8] Verificando Permissões de Diretórios..." -ForegroundColor Yellow
    Write-Host ""

    # 4.1 Verificar permissões dos diretórios de dados
    $dataDirectories = @("C:\IntranetData\Documents", "C:\IntranetData\Backups", "C:\IntranetData\Temp")
    
    foreach ($dir in $dataDirectories) {
        if (Test-Path $dir) {
            try {
                $acl = Get-Acl $dir
                $permissions = $acl.AccessToString
                
                # Verificar permissões perigosas
                if ($permissions -match "Everyone.*FullControl" -or $permissions -match "Users.*FullControl") {
                    Write-SecurityCheck "Permissões: $dir" "FAIL" "Permissões muito abertas detectadas" 20
                    Add-Recommendation "Restrinja as permissões do diretório: $dir"
                } elseif ($permissions -match "Everyone" -or $permissions -match "Users.*Modify") {
                    Write-SecurityCheck "Permissões: $dir" "WARN" "Permissões poderiam ser mais restritivas" 10
                    Add-Recommendation "Considere restringir mais as permissões de: $dir"
                } else {
                    Write-SecurityCheck "Permissões: $dir" "PASS"
                }
            } catch {
                Write-SecurityCheck "Permissões: $dir" "WARN" "Erro ao verificar permissões" 5
            }
        } else {
            Write-SecurityCheck "Diretório: $dir" "WARN" "Diretório não encontrado" 5
        }
    }

    # 5. VERIFICAÇÕES DE REDE
    Write-Host ""
    Write-Host "🌍 [5/8] Verificando Configurações de Rede..." -ForegroundColor Yellow
    Write-Host ""

    # 5.1 Verificar portas abertas
    try {
        $httpPort = netstat -an | findstr ":80 " | Measure-Object | Select-Object -ExpandProperty Count
        $httpsPort = netstat -an | findstr ":443 " | Measure-Object | Select-Object -ExpandProperty Count
        
        if ($httpPort -gt 0) {
            Write-SecurityCheck "Porta HTTP (80)" "WARN" "Porta HTTP aberta - recomenda-se apenas HTTPS" 10
            Add-Recommendation "Configure redirecionamento HTTP para HTTPS"
        }
        
        if ($httpsPort -gt 0) {
            Write-SecurityCheck "Porta HTTPS (443)" "PASS" "HTTPS disponível"
        } else {
            Write-SecurityCheck "Porta HTTPS (443)" "WARN" "HTTPS não detectado" 15
            Add-Recommendation "Configure certificado SSL/TLS"
        }
    } catch {
        Write-SecurityCheck "Verificação de portas" "WARN" "Erro ao verificar portas de rede" 5
    }

    # 6. VERIFICAÇÕES DE ARQUIVOS
    Write-Host ""
    Write-Host "📁 [6/8] Verificando Segurança de Arquivos..." -ForegroundColor Yellow
    Write-Host ""

    # 6.1 Verificar diretório de uploads
    $uploadsDir = "C:\IntranetData\Documents"
    if (Test-Path $uploadsDir) {
        # Verificar arquivos suspeitos
        $suspiciousExtensions = @("*.exe", "*.bat", "*.cmd", "*.ps1", "*.vbs", "*.js", "*.jar")
        $suspiciousFiles = @()
        
        foreach ($ext in $suspiciousExtensions) {
            $files = Get-ChildItem -Path $uploadsDir -Filter $ext -Recurse -ErrorAction SilentlyContinue
            $suspiciousFiles += $files
        }
        
        if ($suspiciousFiles.Count -gt 0) {
            Write-SecurityCheck "Arquivos suspeitos" "FAIL" "$($suspiciousFiles.Count) arquivos executáveis encontrados" 25
            Add-Recommendation "Remova arquivos executáveis do diretório de uploads"
            if ($Detailed) {
                $suspiciousFiles | ForEach-Object { Write-Host "   - $($_.FullName)" -ForegroundColor Red }
            }
        } else {
            Write-SecurityCheck "Arquivos suspeitos" "PASS" "Nenhum arquivo executável encontrado"
        }

        # Verificar tamanho dos arquivos
        $largeFiles = Get-ChildItem -Path $uploadsDir -Recurse -ErrorAction SilentlyContinue | 
                     Where-Object { $_.Length -gt 100MB }
        
        if ($largeFiles.Count -gt 0) {
            Write-SecurityCheck "Arquivos grandes" "WARN" "$($largeFiles.Count) arquivos > 100MB encontrados" 5
            Add-Recommendation "Verifique arquivos muito grandes por possível uso malicioso"
        } else {
            Write-SecurityCheck "Tamanho de arquivos" "PASS"
        }
    }

    # 7. VERIFICAÇÕES DE LOGS
    Write-Host ""
    Write-Host "📝 [7/8] Verificando Configurações de Log..." -ForegroundColor Yellow
    Write-Host ""

    # 7.1 Verificar logs de evento do Windows
    try {
        $logEvents = Get-EventLog -LogName Application -Source "IntranetDocumentos" -Newest 10 -ErrorAction SilentlyContinue
        if ($logEvents) {
            Write-SecurityCheck "Logs de aplicação" "PASS" "$($logEvents.Count) eventos recentes encontrados"
        } else {
            Write-SecurityCheck "Logs de aplicação" "WARN" "Nenhum log de aplicação encontrado" 10
            Add-Recommendation "Configure logging da aplicação no Event Log"
        }
    } catch {
        Write-SecurityCheck "Logs de evento" "WARN" "Erro ao acessar logs de evento" 5
    }

    # 7.2 Verificar logs de IIS
    $iisLogsPath = "C:\inetpub\logs\LogFiles"
    if (Test-Path $iisLogsPath) {
        $recentLogs = Get-ChildItem $iisLogsPath -Recurse -Filter "*.log" | 
                     Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-7) }
        
        if ($recentLogs.Count -gt 0) {
            Write-SecurityCheck "Logs do IIS" "PASS" "$($recentLogs.Count) logs recentes encontrados"
        } else {
            Write-SecurityCheck "Logs do IIS" "WARN" "Nenhum log recente do IIS encontrado" 10
        }
    } else {
        Write-SecurityCheck "Diretório de logs IIS" "WARN" "Diretório de logs do IIS não encontrado" 5
    }

    # 8. VERIFICAÇÕES DE BACKUP
    Write-Host ""
    Write-Host "💾 [8/8] Verificando Configurações de Backup..." -ForegroundColor Yellow
    Write-Host ""

    # 8.1 Verificar backups recentes
    $backupPath = "C:\IntranetData\Backups"
    if (Test-Path $backupPath) {
        $recentBackups = Get-ChildItem $backupPath -Filter "*.db" -ErrorAction SilentlyContinue | 
                        Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-7) }
        
        if ($recentBackups.Count -gt 0) {
            Write-SecurityCheck "Backups recentes" "PASS" "$($recentBackups.Count) backups na última semana"
        } else {
            Write-SecurityCheck "Backups recentes" "WARN" "Nenhum backup recente encontrado" 15
            Add-Recommendation "Configure backup automático da aplicação"
        }
    } else {
        Write-SecurityCheck "Diretório de backup" "WARN" "Diretório de backup não encontrado" 10
    }

    # RELATÓRIO FINAL
    Write-Host ""
    Write-Host "📊 RELATÓRIO DE SEGURANÇA" -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan
    
    $scoreColor = if ($script:securityScore -ge 80) { "Green" } 
                 elseif ($script:securityScore -ge 60) { "Yellow" } 
                 else { "Red" }
    
    Write-Host "🏆 Pontuação de Segurança: $script:securityScore/100" -ForegroundColor $scoreColor
    Write-Host "❌ Problemas Críticos: $script:criticalIssues" -ForegroundColor Red
    Write-Host "⚠️  Avisos: $script:warningIssues" -ForegroundColor Yellow
    
    # Classificação de risco
    $riskLevel = if ($script:securityScore -ge 90) { "BAIXO" }
                elseif ($script:securityScore -ge 70) { "MÉDIO" }
                elseif ($script:securityScore -ge 50) { "ALTO" }
                else { "CRÍTICO" }
    
    $riskColor = switch ($riskLevel) {
        "BAIXO" { "Green" }
        "MÉDIO" { "Yellow" }
        "ALTO" { "Red" }
        "CRÍTICO" { "Magenta" }
    }
    
    Write-Host "🚨 Nível de Risco: $riskLevel" -ForegroundColor $riskColor

    # Recomendações
    if ($script:recommendations.Count -gt 0) {
        Write-Host ""
        Write-Host "💡 RECOMENDAÇÕES:" -ForegroundColor Yellow
        $script:recommendations | ForEach-Object { Write-Host "   • $_" -ForegroundColor White }
    }

    # Próximos passos
    Write-Host ""
    Write-Host "📋 PRÓXIMOS PASSOS:" -ForegroundColor Yellow
    if ($script:criticalIssues -gt 0) {
        Write-Host "   1. 🚨 URGENTE: Corrija os problemas críticos imediatamente" -ForegroundColor Red
        Write-Host "   2. Execute: .\Hardening-Seguranca.ps1 -Force" -ForegroundColor White
    }
    if ($script:warningIssues -gt 0) {
        Write-Host "   3. ⚠️  Resolva os avisos de segurança" -ForegroundColor Yellow
    }
    Write-Host "   4. 📖 Consulte ANALISE-SEGURANCA.md para melhorias avançadas" -ForegroundColor White
    Write-Host "   5. 🔄 Execute esta auditoria semanalmente" -ForegroundColor White

    # Exportar relatório se solicitado
    if ($ExportReport) {
        $reportPath = "C:\Logs\SecurityAudit_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
        New-Item -ItemType Directory -Path (Split-Path $reportPath) -Force -ErrorAction SilentlyContinue | Out-Null
        
        $report = @"
RELATÓRIO DE AUDITORIA DE SEGURANÇA
Data: $(Get-Date)
Aplicação: Intranet Documentos
Caminho: $AppPath

RESUMO:
- Pontuação: $script:securityScore/100
- Nível de Risco: $riskLevel
- Problemas Críticos: $script:criticalIssues
- Avisos: $script:warningIssues

RECOMENDAÇÕES:
$($script:recommendations | ForEach-Object { "- $_" } | Out-String)
"@
        
        $report | Out-File -FilePath $reportPath -Encoding UTF8
        Write-Host ""
        Write-Host "📄 Relatório exportado para: $reportPath" -ForegroundColor Blue
    }

} catch {
    Write-Host "❌ ERRO DURANTE AUDITORIA: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ AUDITORIA DE SEGURANÇA CONCLUÍDA" -ForegroundColor Green
