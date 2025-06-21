# Script para verificar integridade do banco SQLite
# Executa verificações de integridade e mostra estatísticas

param(
    [string]$DatabaseFile = "IntranetDocumentos.db"
)

# Verifica se o banco existe
if (-not (Test-Path $DatabaseFile)) {
    Write-Error "Arquivo do banco de dados '$DatabaseFile' não encontrado!"
    exit 1
}

# Verifica se o SQLite está disponível
$SqliteCommand = Get-Command sqlite3 -ErrorAction SilentlyContinue
if (-not $SqliteCommand) {
    Write-Warning "SQLite3 não encontrado. Instalando via winget..."
    try {
        winget install sqlite.sqlite
        Write-Host "SQLite3 instalado com sucesso!" -ForegroundColor Green
    } catch {
        Write-Error "Erro ao instalar SQLite3. Instale manualmente: https://sqlite.org/download.html"
        exit 1
    }
}

Write-Host "=== VERIFICAÇÃO DE INTEGRIDADE DO BANCO ===" -ForegroundColor Magenta
Write-Host "Arquivo: $DatabaseFile" -ForegroundColor Cyan

# Informações do arquivo
$FileInfo = Get-Item $DatabaseFile
Write-Host "Tamanho: $([math]::Round($FileInfo.Length / 1KB, 2)) KB" -ForegroundColor Cyan
Write-Host "Modificado: $($FileInfo.LastWriteTime)" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== VERIFICAÇÃO DE INTEGRIDADE ===" -ForegroundColor Yellow

# Verifica integridade
try {
    $IntegrityResult = sqlite3 $DatabaseFile "PRAGMA integrity_check;"
    if ($IntegrityResult -eq "ok") {
        Write-Host "✓ Integridade: OK" -ForegroundColor Green
    } else {
        Write-Host "✗ Problemas de integridade encontrados:" -ForegroundColor Red
        Write-Host $IntegrityResult -ForegroundColor Red
    }
} catch {
    Write-Error "Erro ao verificar integridade: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "=== ESTATÍSTICAS DE TABELAS ===" -ForegroundColor Yellow

# Lista tabelas e registros
try {
    $Tables = sqlite3 $DatabaseFile ".tables"
    Write-Host "Tabelas encontradas:" -ForegroundColor Cyan
    
    foreach ($Table in $Tables -split '\s+') {
        if ($Table -and $Table.Trim()) {
            $Table = $Table.Trim()
            try {                $Count = sqlite3 $DatabaseFile "SELECT COUNT(*) FROM [$Table];"
                Write-Host "  ${Table}: $Count registros" -ForegroundColor White            } catch {
                Write-Host "  ${Table}: Erro ao contar registros" -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Error "Erro ao listar tabelas: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "=== INFORMAÇÕES ADICIONAIS ===" -ForegroundColor Yellow

# Verifica se há locks no arquivo
try {
    $LockFiles = Get-ChildItem -Path (Split-Path $DatabaseFile) -Filter "$($FileInfo.BaseName)*" | Where-Object { $_.Name -like "*-journal" -or $_.Name -like "*-wal" -or $_.Name -like "*-shm" }
    
    if ($LockFiles.Count -gt 0) {
        Write-Host "⚠ Arquivos de lock encontrados:" -ForegroundColor Yellow
        foreach ($LockFile in $LockFiles) {
            Write-Host "  $($LockFile.Name)" -ForegroundColor Yellow
        }
        Write-Host "Isso pode indicar que o banco está sendo usado por outro processo." -ForegroundColor Yellow
    } else {
        Write-Host "✓ Nenhum arquivo de lock encontrado" -ForegroundColor Green
    }
} catch {
    Write-Warning "Erro ao verificar locks: $($_.Exception.Message)"
}

# Verifica processos usando o arquivo
try {
    $Processes = Get-Process | Where-Object { $_.MainModule.ModuleName -like "*dotnet*" }
    if ($Processes.Count -gt 0) {
        Write-Host "⚠ Processos .NET em execução:" -ForegroundColor Yellow
        foreach ($Process in $Processes) {
            Write-Host "  PID: $($Process.Id) - $($Process.ProcessName)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✓ Nenhum processo .NET em execução" -ForegroundColor Green
    }
} catch {
    Write-Warning "Erro ao verificar processos: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "=== RECOMENDAÇÕES ===" -ForegroundColor Magenta
Write-Host "1. Faça backups regulares do banco de dados" -ForegroundColor White
Write-Host "2. Evite interromper a aplicação durante operações de escrita" -ForegroundColor White
Write-Host "3. Use o sistema de backup integrado da aplicação" -ForegroundColor White
Write-Host "4. Monitore o tamanho do arquivo de banco" -ForegroundColor White
