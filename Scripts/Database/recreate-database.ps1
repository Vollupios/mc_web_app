# Script para recriar o banco de dados SQLite
# Use este script quando o banco estiver corrompido

Write-Host "=== Recreando Banco de Dados SQLite ===" -ForegroundColor Green

# Parar processo do aplicativo se estiver rodando
Write-Host "Parando processos do IntranetDocumentos..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*IntranetDocumentos*"} | Stop-Process -Force -ErrorAction SilentlyContinue

# Aguardar um momento para liberação do arquivo
Start-Sleep -Seconds 2

# Remover banco corrompido se existir
if (Test-Path "IntranetDocumentos.db") {
    Write-Host "Removendo banco corrompido..." -ForegroundColor Yellow
    Remove-Item "IntranetDocumentos.db" -Force -ErrorAction SilentlyContinue
}

# Remover possível arquivo de backup/lock
if (Test-Path "IntranetDocumentos.db-shm") {
    Remove-Item "IntranetDocumentos.db-shm" -Force -ErrorAction SilentlyContinue
}

if (Test-Path "IntranetDocumentos.db-wal") {
    Remove-Item "IntranetDocumentos.db-wal" -Force -ErrorAction SilentlyContinue
}

# Recriar banco de dados aplicando migrações
Write-Host "Aplicando migrações..." -ForegroundColor Yellow
dotnet ef database update

# Verificar se banco foi criado com sucesso
if (Test-Path "IntranetDocumentos.db") {
    $fileSize = (Get-Item "IntranetDocumentos.db").Length
    Write-Host "Banco criado com sucesso! Tamanho: $fileSize bytes" -ForegroundColor Green
    
    if ($fileSize -gt 50000) {
        Write-Host "Banco parece estar íntegro (tamanho > 50KB)" -ForegroundColor Green
    } else {
        Write-Host "AVISO: Banco muito pequeno, pode estar corrompido!" -ForegroundColor Red
    }
} else {
    Write-Host "ERRO: Falha ao criar banco de dados!" -ForegroundColor Red
    exit 1
}

Write-Host "`nProcesso concluído! Você pode iniciar o aplicativo agora." -ForegroundColor Green
Write-Host "Use: dotnet run" -ForegroundColor Cyan
