#!/usr/bin/env pwsh

# Script para verificar e criar usuário administrador
# Útil para desenvolvimento e primeiras configurações

param(
    [string]$Email = "admin@intranet.com",
    [SecureString]$Password,
    [switch]$CreateIfMissing
)

# Converter senha para string se fornecida, senão usar padrão
$PasswordString = if ($Password) { 
    [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
} else { 
    "Admin123!" 
}

Write-Host "=== Verificação do Usuário Administrador ===" -ForegroundColor Green
Write-Host "Email: $Email" -ForegroundColor Cyan

# Definir diretório do projeto
$ProjectDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Set-Location $ProjectDir

try {
    # Verificar se o usuário admin existe via aplicação
    Write-Host "`nVerificando usuário administrador..." -ForegroundColor Yellow
    
    dotnet run --no-build --verbosity quiet -- --check-admin-user "$Email" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Usuário administrador encontrado!" -ForegroundColor Green
        Write-Host "Email: $Email" -ForegroundColor White
        Write-Host "Status: Ativo" -ForegroundColor Green
        
        # Tentar obter informações adicionais
        $infoResult = dotnet run --no-build --verbosity quiet -- --admin-info "$Email" 2>&1
        if ($LASTEXITCODE -eq 0 -and $infoResult) {
            Write-Host "Informações: $infoResult" -ForegroundColor Cyan
        }
        
    } else {
        Write-Host "⚠ Usuário administrador não encontrado!" -ForegroundColor Yellow
        
        if ($CreateIfMissing) {
            Write-Host "`nCriando usuário administrador..." -ForegroundColor Yellow
            
            $createResult = dotnet run --no-build --verbosity quiet -- --create-admin-user "$Email" "$PasswordString" 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Usuário administrador criado com sucesso!" -ForegroundColor Green
                Write-Host "Email: $Email" -ForegroundColor White
                Write-Host "Senha: $PasswordString" -ForegroundColor White
                Write-Host ""
                Write-Host "IMPORTANTE: Altere a senha no primeiro login!" -ForegroundColor Red
            } else {
                Write-Host "✗ Erro ao criar usuário: $createResult" -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host ""
            Write-Host "Para criar o usuário administrador, execute:" -ForegroundColor Yellow
            Write-Host "  .\check-admin-user.ps1 -CreateIfMissing" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Ou com credenciais customizadas:" -ForegroundColor Yellow
            Write-Host "  .\check-admin-user.ps1 -Email 'seu@email.com' -Password 'SuaSenha123!' -CreateIfMissing" -ForegroundColor Cyan
        }
    }
    
} catch {
    Write-Host "✗ Erro durante a verificação: $($_.Exception.Message)" -ForegroundColor Red
    
    # Verificar se o problema é com o banco
    Write-Host "`nVerificando conectividade com banco..." -ForegroundColor Yellow
    $dbTest = dotnet ef database drop --dry-run 2>&1
    
    if ($dbTest -notmatch "Would drop database") {
        Write-Host "⚠ Problema detectado com o banco de dados" -ForegroundColor Yellow
        Write-Host "Execute: .\Setup-Database.ps1" -ForegroundColor Cyan
    }
    
    exit 1
}

Write-Host "`n=== Verificação Concluída ===" -ForegroundColor Green
