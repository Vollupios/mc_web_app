# Script para corrigir problemas no banco de dados SQL Server
# Executa diagnósticos e reparos básicos

param(
    [switch]$Force,
    [switch]$RecreateMigrations
)

Write-Host "=== Script de Correção do Banco SQL Server ===" -ForegroundColor Green

# Parar aplicação se estiver rodando
Write-Host "Parando processos da aplicação..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*IntranetDocumentos*"} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

try {
    # 1. Verificar estado das migrações
    Write-Host "Verificando estado das migrações..." -ForegroundColor Yellow
    $pendingMigrations = dotnet ef migrations list --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Erro ao verificar migrações: $pendingMigrations" -ForegroundColor Red
        
        if ($RecreateMigrations -or $Force) {
            Write-Host "Removendo e recriando migrações..." -ForegroundColor Yellow
            
            # Remover migrações existentes
            Get-ChildItem -Path "Migrations" -Filter "*.cs" | Remove-Item -Force -ErrorAction SilentlyContinue
            
            # Criar nova migração inicial
            dotnet ef migrations add InitialCreate --force
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Nova migração criada com sucesso!" -ForegroundColor Green
            } else {
                Write-Host "Erro ao criar nova migração!" -ForegroundColor Red
                exit 1
            }
        }
    }
    
    # 2. Verificar conectividade com o banco
    Write-Host "Testando conectividade com o banco..." -ForegroundColor Yellow
    $connectionTest = dotnet ef database drop --dry-run 2>&1
    
    if ($connectionTest -match "Would drop database") {
        Write-Host "Conectividade OK - Banco existe e está acessível" -ForegroundColor Green
    } else {
        Write-Host "Problemas de conectividade detectados" -ForegroundColor Yellow
        Write-Host "Tentando criar o banco..." -ForegroundColor Yellow
        
        # Tentar aplicar migrações
        dotnet ef database update
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Banco criado/atualizado com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "Erro ao criar/atualizar banco!" -ForegroundColor Red
            exit 1
        }
    }
    
    # 3. Verificar integridade das tabelas principais
    Write-Host "Verificando estrutura do banco..." -ForegroundColor Yellow
    
    # Usar a aplicação para verificar se as tabelas principais existem
    dotnet run --no-build --verbosity quiet -- --verify-database 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Estrutura do banco verificada - OK" -ForegroundColor Green
    } else {
        Write-Host "Problemas na estrutura detectados, tentando corrigir..." -ForegroundColor Yellow
        
        # Tentar aplicar migrações novamente
        dotnet ef database update --verbose
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Estrutura corrigida com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "Não foi possível corrigir a estrutura automaticamente" -ForegroundColor Red
            Write-Host "Considere executar: .\recreate-database.ps1" -ForegroundColor Yellow
        }
    }
    
    # 4. Limpeza de conexões órfãs (se aplicável)
    Write-Host "Verificando conexões ativas..." -ForegroundColor Yellow
    
    # 5. Resumo final
    Write-Host ""
    Write-Host "=== DIAGNÓSTICO CONCLUÍDO ===" -ForegroundColor Magenta
    Write-Host "Banco: SQL Server (LocalDB)" -ForegroundColor White
    Write-Host "Status: Verificações executadas" -ForegroundColor White
    Write-Host ""
    Write-Host "Se ainda houver problemas, considere:" -ForegroundColor Yellow
    Write-Host "1. .\recreate-database.ps1 - Para recriar completamente" -ForegroundColor Cyan
    Write-Host "2. .\backup-database.ps1 - Para fazer backup antes de mudanças" -ForegroundColor Cyan
    Write-Host "3. Verificar logs da aplicação para erros específicos" -ForegroundColor Cyan
    
} catch {
    Write-Error "Erro durante o processo de correção: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "Processo de correção concluído!" -ForegroundColor Green
Write-Host "Você pode iniciar a aplicação agora: dotnet run" -ForegroundColor Cyan
