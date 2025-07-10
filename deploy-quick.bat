@echo off
title Instalacao Rapida - Intranet Documentos
echo.
echo ========================================
echo  INSTALACAO RAPIDA - INTRANET DOCUMENTOS
echo ========================================
echo.
echo Este script instalara automaticamente:
echo - IIS e recursos necessarios
echo - .NET 9.0 Hosting Bundle (se necessario)
echo - MySQL (se necessario)
echo - Aplicacao Intranet Documentos
echo.
echo ATENCAO: Execute como Administrador!
echo.
pause

echo.
echo [1/5] Verificando pre-requisitos...
echo.

:: Verificar se esta rodando como admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERRO: Este script deve ser executado como Administrador!
    echo Clique com botao direito e selecione "Executar como administrador"
    pause
    exit /b 1
)

echo ✓ Executando como Administrador

:: Verificar se o PowerShell existe
powershell -Command "Get-Host" >nul 2>&1
if %errorLevel% neq 0 (
    echo ERRO: PowerShell nao encontrado!
    pause
    exit /b 1
)

echo ✓ PowerShell disponivel

echo.
echo [2/5] Configurando IIS...
echo.

:: Executar script de configuracao do IIS
if exist "Configuracao-IIS.ps1" (
    powershell -ExecutionPolicy Bypass -File "Configuracao-IIS.ps1"
    if %errorLevel% neq 0 (
        echo ERRO: Falha na configuracao do IIS
        pause
        exit /b 1
    )
    echo ✓ IIS configurado com sucesso
) else (
    echo AVISO: Script de configuracao do IIS nao encontrado
    echo Configurando IIS manualmente...
    
    :: Habilitar IIS via DISM
    dism /online /enable-feature /featurename:IIS-WebServerRole /all
    dism /online /enable-feature /featurename:IIS-HttpRedirect /all
    dism /online /enable-feature /featurename:IIS-NetFxExtensibility45 /all
    dism /online /enable-feature /featurename:IIS-ASPNET45 /all
    
    echo ✓ IIS habilitado
)

echo.
echo [3/5] Verificando .NET 9.0...
echo.

:: Verificar se .NET 9.0 esta instalado
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 9." >nul 2>&1
if %errorLevel% neq 0 (
    echo AVISO: .NET 9.0 Hosting Bundle nao encontrado!
    echo Por favor, baixe e instale:
    echo https://dotnet.microsoft.com/download/dotnet/9.0
    echo.
    echo Pressione qualquer tecla apos instalar o .NET 9.0...
    pause
) else (
    echo ✓ .NET 9.0 Hosting Bundle instalado
)

echo.
echo [4/5] Executando deploy da aplicacao...
echo.

:: Executar script principal de deploy
if exist "Deploy-WindowsServer.ps1" (
    set /p MYSQL_PASSWORD="Digite a senha do MySQL (sera criada): "
    if "!MYSQL_PASSWORD!"=="" set MYSQL_PASSWORD=IntranetSegura123!
    
    powershell -ExecutionPolicy Bypass -File "Deploy-WindowsServer.ps1" -MySqlPassword "!MYSQL_PASSWORD!"
    if %errorLevel% neq 0 (
        echo ERRO: Falha no deploy da aplicacao
        pause
        exit /b 1
    )
    echo ✓ Aplicacao implantada com sucesso
) else (
    echo ERRO: Script de deploy nao encontrado (Deploy-WindowsServer.ps1)
    pause
    exit /b 1
)

echo.
echo [5/5] Verificando instalacao...
echo.

:: Executar script de verificacao
if exist "Verificacao-Pos-Instalacao.ps1" (
    powershell -ExecutionPolicy Bypass -File "Verificacao-Pos-Instalacao.ps1"
) else (
    echo AVISO: Script de verificacao nao encontrado
    echo Verificando manualmente...
    
    :: Verificar se o site responde
    powershell -Command "try { Invoke-WebRequest -Uri 'http://localhost/IntranetDocumentos' -UseBasicParsing | Out-Null; Write-Host '✓ Site respondendo' } catch { Write-Host '✗ Site nao responde' }"
)

echo.
echo ========================================
echo  INSTALACAO CONCLUIDA!
echo ========================================
echo.
echo ✓ Aplicacao instalada em: C:\inetpub\wwwroot\IntranetDocumentos
echo ✓ URL de acesso: http://localhost/IntranetDocumentos
echo ✓ Login padrao: admin@empresa.com.br / Admin123!
echo.
echo PROXIMOS PASSOS:
echo 1. Editar C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json
echo 2. Configurar string de conexao MySQL
echo 3. Configurar configuracoes de email (SMTP)
echo 4. Acessar o sistema e alterar senha do admin
echo.
echo Para documentacao completa, consulte:
echo GUIA-UNIFICADO-FINAL.md
echo.
pause

:: Tentar abrir o navegador
start http://localhost/IntranetDocumentos

echo.
echo ==============================================
echo      Deploy concluido com sucesso!
echo ==============================================
echo.
echo Proximos passos:
echo 1. Configure a connection string no appsettings.Production.json
echo 2. Configure o SMTP no appsettings.Production.json  
echo 3. Teste a aplicacao no navegador
echo.
pause
