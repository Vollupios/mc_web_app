# Script para recriar o banco de dados SQL Server
# Use este script quando o banco estiver corrompido ou para reset completo

Write-Host "=== Recreando Banco de Dados SQL Server ===" -ForegroundColor Green

# Parar processo do aplicativo se estiver rodando
Write-Host "Parando processos do IntranetDocumentos..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*IntranetDocumentos*"} | Stop-Process -Force -ErrorAction SilentlyContinue

# Aguardar um momento para liberação das conexões
Start-Sleep -Seconds 3

# Excluir banco de dados existente (CUIDADO: isso remove todos os dados!)
Write-Host "Removendo banco de dados existente..." -ForegroundColor Yellow
try {
    dotnet ef database drop --force
    Write-Host "Banco de dados removido com sucesso." -ForegroundColor Green
} catch {
    Write-Host "Banco não existia ou erro ao remover: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Recriar banco de dados aplicando migrações
Write-Host "Criando novo banco e aplicando migrações..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "Banco de dados criado e migrações aplicadas com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "ERRO: Falha ao criar banco de dados: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verificar se o banco foi criado verificando as tabelas
Write-Host "Verificando integridade do banco..." -ForegroundColor Yellow
try {
    dotnet run --no-build --verbosity quiet -- --verify-database 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Banco criado com sucesso e está funcionando!" -ForegroundColor Green
    } else {
        Write-Host "AVISO: Banco criado mas pode ter problemas de conectividade." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Não foi possível verificar a conectividade, mas o banco foi criado." -ForegroundColor Yellow
}

Write-Host "`nProcesso concluído! Você pode iniciar o aplicativo agora." -ForegroundColor Green
Write-Host "Use: dotnet run" -ForegroundColor Cyan

Write-Host "`nProcesso concluído! Você pode iniciar o aplicativo agora." -ForegroundColor Green
Write-Host "Use: dotnet run" -ForegroundColor Cyan
