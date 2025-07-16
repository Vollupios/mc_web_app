#!/usr/bin/env pwsh

# Script para iniciar a aplicação Intranet Documentos
# Verifica o build e executa a aplicação

# Definir diretório do projeto (relativo ao script ou absoluto)
$ProjectDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Set-Location $ProjectDir

Write-Host "=== Iniciando Intranet Documentos ===" -ForegroundColor Green
Write-Host "Diretório: $(Get-Location)" -ForegroundColor Cyan
Write-Host "Projeto: IntranetDocumentos.csproj" -ForegroundColor Cyan

Write-Host "`nVerificando status atual do projeto..." -ForegroundColor Yellow
dotnet build IntranetDocumentos.csproj --verbosity minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build bem-sucedido" -ForegroundColor Green
    
    # Verificar se o banco está acessível
    Write-Host "`nVerificando conectividade com banco de dados..." -ForegroundColor Yellow
    $dbCheck = dotnet ef database drop --dry-run 2>&1
    
    if ($dbCheck -match "Would drop database" -or $dbCheck -match "does not exist") {
        Write-Host "✓ Banco de dados acessível" -ForegroundColor Green
    } else {
        Write-Host "⚠ Problemas com banco - executando migrações..." -ForegroundColor Yellow
        dotnet ef database update
    }
    
    Write-Host "`n=== Executando aplicação ===" -ForegroundColor Green
    Write-Host "URL: https://localhost:7168" -ForegroundColor Cyan
    Write-Host "Pressione Ctrl+C para parar`n" -ForegroundColor Yellow
    
    dotnet run --project IntranetDocumentos.csproj
} else {
    Write-Host "✗ Build falhou" -ForegroundColor Red
    Write-Host "Verifique os erros acima e tente novamente." -ForegroundColor Yellow
    exit 1
}
