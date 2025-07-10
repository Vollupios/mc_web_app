# Script de Publicação para Windows Server
# Execute na máquina de desenvolvimento

param(
    [Parameter(Mandatory=$true)]
    [string]$TargetServer,
    
    [string]$TargetPath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [string]$BackupPath = "C:\Deploy\Backup",
    [PSCredential]$Credential,
    [switch]$SkipBackup,
    [switch]$RestartSite
)

$ErrorActionPreference = "Stop"

Write-Host "=== Publicação Intranet Documentos ===" -ForegroundColor Green
Write-Host "Servidor de destino: $TargetServer" -ForegroundColor Cyan
Write-Host "Caminho de destino: $TargetPath" -ForegroundColor Cyan

# 1. Limpar e compilar o projeto
Write-Host "`n1. Limpando e compilando o projeto..." -ForegroundColor Yellow
try {
    Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue
    
    dotnet clean IntranetDocumentos.csproj --configuration Release
    dotnet restore IntranetDocumentos.csproj
    dotnet build IntranetDocumentos.csproj --configuration Release --no-restore
    
    Write-Host "✓ Compilação concluída com sucesso" -ForegroundColor Green
} catch {
    Write-Error "✗ Erro na compilação: $($_.Exception.Message)"
    exit 1
}

# 2. Publicar o projeto
Write-Host "`n2. Publicando o projeto..." -ForegroundColor Yellow
$publishPath = ".\publish"
try {
    Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
    
    dotnet publish IntranetDocumentos.csproj `
        --configuration Release `
        --output $publishPath `
        --no-build `
        --verbosity normal
    
    Write-Host "✓ Publicação concluída: $publishPath" -ForegroundColor Green
} catch {
    Write-Error "✗ Erro na publicação: $($_.Exception.Message)"
    exit 1
}

# 3. Verificar arquivos essenciais
Write-Host "`n3. Verificando arquivos essenciais..." -ForegroundColor Yellow
$essentialFiles = @(
    "$publishPath\IntranetDocumentos.dll",
    "$publishPath\web.config",
    "$publishPath\appsettings.json",
    "$publishPath\appsettings.Production.json"
)

foreach ($file in $essentialFiles) {
    if (!(Test-Path $file)) {
        Write-Error "✗ Arquivo essencial não encontrado: $file"
        exit 1
    }
}
Write-Host "✓ Todos os arquivos essenciais estão presentes" -ForegroundColor Green

# 4. Conectar ao servidor de destino
Write-Host "`n4. Conectando ao servidor de destino..." -ForegroundColor Yellow
try {
    if ($Credential) {
        $session = New-PSSession -ComputerName $TargetServer -Credential $Credential
    } else {
        $session = New-PSSession -ComputerName $TargetServer
    }
    Write-Host "✓ Conectado ao servidor: $TargetServer" -ForegroundColor Green
} catch {
    Write-Error "✗ Erro ao conectar ao servidor: $($_.Exception.Message)"
    exit 1
}

try {
    # 5. Backup da versão atual (se existir)
    if (!$SkipBackup) {
        Write-Host "`n5. Realizando backup da versão atual..." -ForegroundColor Yellow
        Invoke-Command -Session $session -ScriptBlock {
            param($TargetPath, $BackupPath)
            
            if (Test-Path $TargetPath) {
                $backupFolder = "$BackupPath\$(Get-Date -Format 'yyyyMMdd_HHmmss')"
                New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
                Copy-Item "$TargetPath\*" -Destination $backupFolder -Recurse -Force
                Write-Host "✓ Backup realizado em: $backupFolder" -ForegroundColor Green
            } else {
                Write-Host "ℹ Primeira instalação - sem backup necessário" -ForegroundColor Cyan
            }
        } -ArgumentList $TargetPath, $BackupPath
    }

    # 6. Parar o site/aplicação
    Write-Host "`n6. Parando aplicação..." -ForegroundColor Yellow
    Invoke-Command -Session $session -ScriptBlock {
        param($TargetPath)
        
        # Parar o App Pool se existir
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        $appPool = Get-IISAppPool | Where-Object { $_.Applications.VirtualDirectories.PhysicalPath -eq $TargetPath } | Select-Object -First 1
        if ($appPool) {
            Stop-WebAppPool -Name $appPool.Name -ErrorAction SilentlyContinue
            Write-Host "✓ App Pool parado: $($appPool.Name)" -ForegroundColor Green
        }
        
        # Aguardar processos liberarem arquivos
        Start-Sleep -Seconds 3
    } -ArgumentList $TargetPath

    # 7. Criar estrutura de diretórios
    Write-Host "`n7. Criando estrutura de diretórios..." -ForegroundColor Yellow
    Invoke-Command -Session $session -ScriptBlock {
        param($TargetPath)
        
        New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
        New-Item -ItemType Directory -Path "$TargetPath\logs" -Force | Out-Null
        
        # Criar diretórios de dados
        $dataPaths = @(
            "C:\IntranetData\Documents",
            "C:\IntranetData\Backups",
            "C:\IntranetData\Documents\Geral",
            "C:\IntranetData\Documents\Pessoal",
            "C:\IntranetData\Documents\Fiscal",
            "C:\IntranetData\Documents\Contabil",
            "C:\IntranetData\Documents\Cadastro",
            "C:\IntranetData\Documents\Apoio",
            "C:\IntranetData\Documents\TI"
        )
        
        foreach ($path in $dataPaths) {
            New-Item -ItemType Directory -Path $path -Force | Out-Null
        }
        
        Write-Host "✓ Estrutura de diretórios criada" -ForegroundColor Green
    } -ArgumentList $TargetPath

    # 8. Copiar arquivos publicados
    Write-Host "`n8. Copiando arquivos para o servidor..." -ForegroundColor Yellow
    Copy-Item "$publishPath\*" -Destination $TargetPath -ToSession $session -Recurse -Force
    Write-Host "✓ Arquivos copiados com sucesso" -ForegroundColor Green

    # 9. Configurar permissões
    Write-Host "`n9. Configurando permissões..." -ForegroundColor Yellow
    Invoke-Command -Session $session -ScriptBlock {
        param($TargetPath)
        
        # Permissões para IIS_IUSRS
        icacls $TargetPath /grant "IIS_IUSRS:(OI)(CI)RX" /T
        icacls "$TargetPath\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
        
        # Permissões para diretório de dados
        icacls "C:\IntranetData" /grant "IIS_IUSRS:(OI)(CI)F" /T
        
        Write-Host "✓ Permissões configuradas" -ForegroundColor Green
    } -ArgumentList $TargetPath

    # 10. Reiniciar aplicação
    if ($RestartSite) {
        Write-Host "`n10. Reiniciando aplicação..." -ForegroundColor Yellow
        Invoke-Command -Session $session -ScriptBlock {
            param($TargetPath)
            
            # Reiniciar App Pool
            Import-Module WebAdministration -ErrorAction SilentlyContinue
            $appPool = Get-IISAppPool | Where-Object { $_.Applications.VirtualDirectories.PhysicalPath -eq $TargetPath } | Select-Object -First 1
            if ($appPool) {
                Start-WebAppPool -Name $appPool.Name
                Write-Host "✓ App Pool reiniciado: $($appPool.Name)" -ForegroundColor Green
            }
        } -ArgumentList $TargetPath
    }

    Write-Host "`n🎉 Deploy concluído com sucesso!" -ForegroundColor Green
    Write-Host "📍 Local: $TargetServer`:$TargetPath" -ForegroundColor Cyan
    
} finally {
    # Limpar sessão
    if ($session) {
        Remove-PSSession $session
    }
}

# Limpar arquivos temporários
Write-Host "`n11. Limpando arquivos temporários..." -ForegroundColor Yellow
Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✓ Limpeza concluída" -ForegroundColor Green

Write-Host "`n✅ Processo de deploy finalizado!" -ForegroundColor Green
Write-Host "💡 Próximos passos:" -ForegroundColor Cyan
Write-Host "   1. Configurar connection string no appsettings.Production.json" -ForegroundColor White
Write-Host "   2. Configurar SMTP no appsettings.Production.json" -ForegroundColor White
Write-Host "   3. Testar a aplicação no navegador" -ForegroundColor White
Write-Host "   4. Verificar logs em caso de problemas" -ForegroundColor White
