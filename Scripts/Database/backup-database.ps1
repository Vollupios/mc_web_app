# Script para backup manual do banco SQL Server
# Executa backup completo do banco de dados da Intranet de Documentos

param(
    [string]$BackupPath = "DatabaseBackups",
    [string]$ConnectionString = "",
    [string]$DatabaseName = "IntranetDocumentosDb",
    [switch]$Compress
)

# Obter connection string do appsettings.json se não fornecida
if ([string]::IsNullOrEmpty($ConnectionString)) {
    if (Test-Path "appsettings.json") {
        $appSettings = Get-Content "appsettings.json" | ConvertFrom-Json
        $ConnectionString = $appSettings.ConnectionStrings.DefaultConnection
        Write-Host "Connection string obtida do appsettings.json" -ForegroundColor Cyan
    } else {
        Write-Error "appsettings.json não encontrado e connection string não fornecida!"
        exit 1
    }
}

# Cria diretório de backup se não existir
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "Diretório de backup criado: $BackupPath" -ForegroundColor Green
}

try {
    Write-Host "Iniciando backup do banco SQL Server..." -ForegroundColor Yellow
    Write-Host "Database: $DatabaseName" -ForegroundColor Cyan
    
    # Para o processo do .NET se estiver rodando para liberar conexões
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
    
    # Usar o DatabaseBackupService da aplicação via dotnet run
    Write-Host "Executando backup via aplicação..." -ForegroundColor Yellow
    
    # Executar backup via comando da aplicação
    $backupResult = dotnet run --no-build --verbosity quiet -- --backup-database 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Backup executado com sucesso via aplicação!" -ForegroundColor Green
        
        # Encontrar o arquivo de backup mais recente
        $latestBackup = Get-ChildItem -Path $BackupPath -Filter "*.db" | Sort-Object CreationTime -Descending | Select-Object -First 1
        
        if ($latestBackup) {
            Write-Host "Backup encontrado: $($latestBackup.Name)" -ForegroundColor Green
            
            # Comprimir se solicitado
            if ($Compress) {
                $zipFile = Join-Path $BackupPath "$($latestBackup.BaseName).zip"
                Compress-Archive -Path $latestBackup.FullName -DestinationPath $zipFile -Force
                Write-Host "Backup comprimido criado: $($zipFile)" -ForegroundColor Green
                
                # Remover arquivo não comprimido
                Remove-Item $latestBackup.FullName -Force
                $backupFile = $zipFile
            } else {
                $backupFile = $latestBackup.FullName
            }
            
            # Informações do backup
            $BackupInfo = Get-Item $backupFile
            Write-Host "Tamanho: $([math]::Round($BackupInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
            Write-Host "Data: $($BackupInfo.CreationTime)" -ForegroundColor Cyan
        }
    } else {
        Write-Error "Falha no backup via aplicação: $backupResult"
        exit 1
    }
    
    # Limpeza de backups antigos (mantém últimos 30 dias)
    Write-Host "Limpando backups antigos..." -ForegroundColor Yellow
    $CutoffDate = (Get-Date).AddDays(-30)
    $OldBackups = Get-ChildItem -Path $BackupPath -Filter "*_Backup_*" | Where-Object { $_.CreationTime -lt $CutoffDate }
    
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
Write-Host "Localização dos backups: $(Resolve-Path $BackupPath)" -ForegroundColor White
Write-Host "Para restaurar: use o DatabaseBackupService da aplicação ou scripts de restore" -ForegroundColor White
Write-Host "Banco: SQL Server - $DatabaseName" -ForegroundColor White
