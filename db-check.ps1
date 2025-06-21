param([string]$DatabaseFile = "IntranetDocumentos.db")

if (-not (Test-Path $DatabaseFile)) {
    Write-Host "Erro: Arquivo do banco nao encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "VERIFICACAO DO BANCO DE DADOS" -ForegroundColor Green
$FileInfo = Get-Item $DatabaseFile
Write-Host "Arquivo: $DatabaseFile"
Write-Host "Tamanho: $([math]::Round($FileInfo.Length / 1KB, 2)) KB"
Write-Host "Modificado: $($FileInfo.LastWriteTime)"

$LockFiles = Get-ChildItem -Filter "$($FileInfo.BaseName)*" | Where-Object { $_.Name -like "*-journal" -or $_.Name -like "*-wal" -or $_.Name -like "*-shm" }

Write-Host ""
if ($LockFiles.Count -gt 0) {
    Write-Host "ATENCAO: Arquivos de lock encontrados:" -ForegroundColor Yellow
    foreach ($LockFile in $LockFiles) {
        Write-Host "  $($LockFile.Name)"
    }
} else {
    Write-Host "OK: Nenhum arquivo de lock encontrado" -ForegroundColor Green
}

$Processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
Write-Host ""
if ($Processes.Count -gt 0) {
    Write-Host "ATENCAO: Processos .NET em execucao:" -ForegroundColor Yellow
    foreach ($Process in $Processes) {
        Write-Host "  PID: $($Process.Id)"
    }
} else {
    Write-Host "OK: Nenhum processo .NET em execucao" -ForegroundColor Green
}

Write-Host ""
Write-Host "RECOMENDACOES:" -ForegroundColor Cyan
Write-Host "1. Faca backups regulares: .\backup-database.ps1"
Write-Host "2. Pare a aplicacao graciosamente (Ctrl+C)"
Write-Host "3. Monitore o tamanho do banco regularmente"
