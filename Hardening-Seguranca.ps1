# 🔐 Implementação de Melhorias de Segurança
# Execute como Administrador

param(
    [string]$AppPath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [switch]$Force = $false
)

Write-Host "🛡️ IMPLEMENTANDO MELHORIAS DE SEGURANÇA CRÍTICAS" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$changesApplied = 0

try {
    # 1. BACKUP DE CONFIGURAÇÕES
    Write-Host "[1/8] 💾 Criando backup das configurações..." -ForegroundColor Yellow
    $backupPath = "C:\Backup\IntranetSecurity_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    
    Copy-Item "$AppPath\appsettings.Production.json" "$backupPath\appsettings.Production.json.bak"
    Copy-Item "$AppPath\web.config" "$backupPath\web.config.bak"
    Write-Host "   ✅ Backup criado em: $backupPath" -ForegroundColor Green

    # 2. VERIFICAR SENHA PADRÃO
    Write-Host "[2/8] 🔑 Verificando senha administrativa..." -ForegroundColor Yellow
    $appsettingsPath = "$AppPath\appsettings.Production.json"
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    
    if ($appsettings.AdminUser.Password -eq "Admin123!") {
        Write-Host "   ❌ CRÍTICO: Senha padrão detectada!" -ForegroundColor Red
        
        if ($Force) {
            # Gerar senha segura automaticamente
            $newPassword = -join ((33..126) | Get-Random -Count 16 | % {[char]$_})
            $appsettings.AdminUser.Password = $newPassword
            
            Write-Host "   ✅ Nova senha gerada: $newPassword" -ForegroundColor Green
            Write-Host "   📝 ANOTE ESTA SENHA COM SEGURANÇA!" -ForegroundColor Red
            $changesApplied++
        } else {
            Write-Host "   ⚠️  Use -Force para gerar nova senha automaticamente" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ✅ Senha administrativa não é padrão" -ForegroundColor Green
    }

    # 3. FORTALECER CONFIGURAÇÕES DE SENHA NO CÓDIGO
    Write-Host "[3/8] 🔐 Verificando política de senhas..." -ForegroundColor Yellow
    $programCs = "$AppPath\Program.cs"
    
    if (Test-Path $programCs) {
        $programContent = Get-Content $programCs -Raw
        
        if ($programContent -match "RequiredLength = 6" -or 
            $programContent -match "RequireNonAlphanumeric = false" -or
            $programContent -match "RequireUppercase = false") {
            Write-Host "   ⚠️  Política de senhas fraca detectada em Program.cs" -ForegroundColor Yellow
            Write-Host "   📝 Recompile a aplicação com as configurações seguras do guia" -ForegroundColor Yellow
        } else {
            Write-Host "   ✅ Política de senhas parece adequada" -ForegroundColor Green
        }
    }

    # 4. ATUALIZAR HEADERS DE SEGURANÇA
    Write-Host "[4/8] 🛡️ Configurando headers de segurança..." -ForegroundColor Yellow
    $webConfigPath = "$AppPath\web.config"
    [xml]$webConfig = Get-Content $webConfigPath

    # Verificar se já existe seção de headers
    $httpProtocol = $webConfig.configuration.location.'system.webServer'.httpProtocol
    if (-not $httpProtocol) {
        Write-Host "   ⚠️  Seção httpProtocol não encontrada no web.config" -ForegroundColor Yellow
    } else {
        $customHeaders = $httpProtocol.customHeaders
        if ($customHeaders) {
            # Headers de segurança críticos
            $securityHeaders = @{
                "Content-Security-Policy" = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;"
                "Strict-Transport-Security" = "max-age=31536000; includeSubDomains"
                "Permissions-Policy" = "camera=(), microphone=(), geolocation=()"
                "X-Content-Type-Options" = "nosniff"
                "X-Frame-Options" = "DENY"
                "X-XSS-Protection" = "1; mode=block"
            }

            foreach ($header in $securityHeaders.GetEnumerator()) {
                $existingHeader = $customHeaders.add | Where-Object { $_.name -eq $header.Key }
                if (-not $existingHeader) {
                    $newHeader = $webConfig.CreateElement("add")
                    $newHeader.SetAttribute("name", $header.Key)
                    $newHeader.SetAttribute("value", $header.Value)
                    $customHeaders.AppendChild($newHeader) | Out-Null
                    Write-Host "   ✅ Adicionado header: $($header.Key)" -ForegroundColor Green
                    $changesApplied++
                }
            }
        }
    }

    # 5. CONFIGURAR PERMISSÕES DE DIRETÓRIOS
    Write-Host "[5/8] 📁 Configurando permissões de diretórios..." -ForegroundColor Yellow
    $directories = @("C:\IntranetData\Documents", "C:\IntranetData\Backups", "C:\IntranetData\Temp")
    
    foreach ($dir in $directories) {
        if (Test-Path $dir) {
            # Remover permissões perigosas
            icacls $dir /remove "Everyone" /T 2>$null | Out-Null
            icacls $dir /remove "Users" /T 2>$null | Out-Null
            
            # Aplicar permissões restritivas
            icacls $dir /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null
            icacls $dir /grant "BUILTIN\Administrators:(OI)(CI)F" /T | Out-Null
            
            Write-Host "   ✅ Permissões configuradas para: $dir" -ForegroundColor Green
            $changesApplied++
        } else {
            Write-Host "   ⚠️  Diretório não encontrado: $dir" -ForegroundColor Yellow
        }
    }

    # 6. REMOVER HEADERS DESNECESSÁRIOS VIA IIS
    Write-Host "[6/8] 🔧 Removendo headers desnecessários..." -ForegroundColor Yellow
    
    try {
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        
        $headersToRemove = @("Server", "X-Powered-By", "X-AspNet-Version")
        foreach ($header in $headersToRemove) {
            try {
                Remove-WebConfigurationProperty -PSPath "IIS:\" -Filter "system.webServer/httpProtocol/customHeaders" -Name collection -AtElement @{name=$header} -ErrorAction SilentlyContinue
                Write-Host "   ✅ Removido header: $header" -ForegroundColor Green
            } catch {
                Write-Host "   ⚠️  Header $header não encontrado ou já removido" -ForegroundColor Yellow
            }
        }
        $changesApplied++
    } catch {
        Write-Host "   ⚠️  Módulo WebAdministration não disponível" -ForegroundColor Yellow
    }

    # 7. CONFIGURAR HTTPS REDIRECT
    Write-Host "[7/8] 🔒 Configurando redirecionamento HTTPS..." -ForegroundColor Yellow
    
    # Verificar se já existe regra de HTTPS
    if ($webConfig.configuration.location.'system.webServer'.rewrite) {
        Write-Host "   ✅ Seção de rewrite já existe" -ForegroundColor Green
    } else {
        Write-Host "   📝 Configure redirecionamento HTTPS manualmente no IIS" -ForegroundColor Yellow
    }

    # 8. SALVAR ALTERAÇÕES
    Write-Host "[8/8] 💾 Salvando alterações..." -ForegroundColor Yellow
    
    if ($changesApplied -gt 0) {
        # Salvar appsettings se modificado
        if ($appsettings.AdminUser.Password -ne "Admin123!") {
            $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
        }
        
        # Salvar web.config se modificado
        $webConfig.Save($webConfigPath)
        
        Write-Host "   ✅ Configurações salvas" -ForegroundColor Green
    }

    # RELATÓRIO FINAL
    Write-Host ""
    Write-Host "🎯 RELATÓRIO DE SEGURANÇA" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    Write-Host "✅ Alterações aplicadas: $changesApplied" -ForegroundColor Green
    Write-Host "📁 Backup salvo em: $backupPath" -ForegroundColor Blue
    
    if ($appsettings.AdminUser.Password -ne "Admin123!") {
        Write-Host "🔑 Nova senha administrativa gerada!" -ForegroundColor Red
        Write-Host "   Email: admin@empresa.com" -ForegroundColor White
        Write-Host "   Senha: $($appsettings.AdminUser.Password)" -ForegroundColor White
        Write-Host "   📝 SALVE ESTAS CREDENCIAIS COM SEGURANÇA!" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "📋 PRÓXIMOS PASSOS RECOMENDADOS:" -ForegroundColor Yellow
    Write-Host "1. Reiniciar IIS: iisreset" -ForegroundColor White
    Write-Host "2. Testar login com novas credenciais" -ForegroundColor White
    Write-Host "3. Verificar headers de segurança com ferramenta online" -ForegroundColor White
    Write-Host "4. Implementar melhorias de código conforme ANALISE-SEGURANCA.md" -ForegroundColor White
    Write-Host "5. Configurar monitoramento de logs de segurança" -ForegroundColor White

    Write-Host ""
    Write-Host "✅ HARDENING DE SEGURANÇA CONCLUÍDO!" -ForegroundColor Green

} catch {
    Write-Host "❌ ERRO: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "📁 Restaure o backup se necessário: $backupPath" -ForegroundColor Yellow
    exit 1
}

# Perguntar se deve reiniciar IIS
Write-Host ""
$restart = Read-Host "Deseja reiniciar o IIS agora? (s/N)"
if ($restart -eq "s" -or $restart -eq "S") {
    Write-Host "🔄 Reiniciando IIS..." -ForegroundColor Yellow
    iisreset
    Write-Host "✅ IIS reiniciado" -ForegroundColor Green
}

Write-Host ""
Write-Host "🛡️ Hardening de segurança concluído com sucesso!" -ForegroundColor Green
