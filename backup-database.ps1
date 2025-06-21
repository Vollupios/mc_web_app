# Script para backup manual do banco SQLite
# Executa backup completo do banco de dados da Intranet de Documentos

param(
    [string]$BackupPath = "DatabaseBackups",
    [switch]$Compress
)

# Configurações
$DatabaseFile = "IntranetDocumentos.db"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupName = "backup_$Timestamp"

# Verifica se o banco existe
if (-not (Test-Path $DatabaseFile)) {
    Write-Error "Arquivo do banco de dados '$DatabaseFile' não encontrado!"
    exit 1
}

# Cria diretório de backup se não existir
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "Diretório de backup criado: $BackupPath" -ForegroundColor Green
}

try {
    Write-Host "Iniciando backup do banco de dados..." -ForegroundColor Yellow
    
    # Para o processo do .NET se estiver rodando
    $DotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    $StoppedProcesses = @()
    
    foreach ($Process in $DotnetProcesses) {
        if ($Process.ProcessName -eq "dotnet") {
            Write-Host "Parando processo dotnet (ID: $($Process.Id))..." -ForegroundColor Yellow
            Stop-Process -Id $Process.Id -Force
            $StoppedProcesses += $Process.Id
            Start-Sleep -Seconds 2
        }
    }
      if ($Compress -or $true) {
        # Backup comprimido (padrão)
        $BackupFile = Join-Path $BackupPath "$BackupName.zip"
        
        # Cria arquivo ZIP com o banco
        Compress-Archive -Path $DatabaseFile -DestinationPath $BackupFile -Force
        
        Write-Host "Backup comprimido criado: $BackupFile" -ForegroundColor Green
    } else {
        # Backup simples (cópia)
        $BackupFile = Join-Path $BackupPath "$BackupName.db"
        Copy-Item -Path $DatabaseFile -Destination $BackupFile -Force
        
        Write-Host "Backup criado: $BackupFile" -ForegroundColor Green
    }
    
    # Informações do backup
    $BackupInfo = Get-Item $BackupFile
    Write-Host "Tamanho: $([math]::Round($BackupInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "Data: $($BackupInfo.CreationTime)" -ForegroundColor Cyan
    
    # Limpeza de backups antigos (mantém últimos 30 dias)
    Write-Host "Limpando backups antigos..." -ForegroundColor Yellow
    $CutoffDate = (Get-Date).AddDays(-30)
    $OldBackups = Get-ChildItem -Path $BackupPath -Filter "backup_*" | Where-Object { $_.CreationTime -lt $CutoffDate }
    
    foreach ($OldBackup in $OldBackups) {
        Remove-Item $OldBackup.FullName -Force
        Write-Host "Removido backup antigo: $($OldBackup.Name)" -ForegroundColor Gray
    }
    
    Write-Host "Backup concluído com sucesso!" -ForegroundColor Green
    
} catch {
    Write-Error "Erro durante o backup: $($_.Exception.Message)"
    exit 1
} finally {
    # Opcional: Reiniciar os processos parados (comentado por segurança)
    # foreach ($ProcessId in $StoppedProcesses) {
    #     Write-Host "Reiniciando aplicação..." -ForegroundColor Yellow
    #     # Aqui você pode adicionar comando para reiniciar a aplicação se necessário
    # }
}

Write-Host ""
Write-Host "=== INFORMAÇÕES DO BACKUP ===" -ForegroundColor Magenta
Write-Host "Arquivo: $BackupFile" -ForegroundColor White
Write-Host "Para restaurar: substitua o arquivo '$DatabaseFile' pelo backup desejado" -ForegroundColor White
Write-Host "Localização dos backups: $(Resolve-Path $BackupPath)" -ForegroundColor White
