@echo off
REM ================================================================
REM 🚀 INSTALAÇÃO RÁPIDA - Intranet Documentos
REM ================================================================
REM Script batch para instalação rápida no Windows
REM Versão: 2.0 Production Ready
REM Data: 16 de Julho de 2025
REM ================================================================

setlocal EnableDelayedExpansion

REM ================================================================
REM CONFIGURAÇÕES
REM ================================================================

set "APP_NAME=Intranet Documentos"
set "APP_VERSION=2.0"
set "SCRIPT_DIR=%~dp0"
set "APP_ROOT=%SCRIPT_DIR%"
set "LOG_FILE=%APP_ROOT%Logs\install-quick-%date:~-4,4%%date:~-10,2%%date:~-7,2%-%time:~0,2%%time:~3,2%%time:~6,2%.log"

REM Criar pasta de logs
if not exist "%APP_ROOT%Logs" mkdir "%APP_ROOT%Logs"

REM ================================================================
REM BANNER E INFORMAÇÕES
REM ================================================================

echo.
echo ================================================================
echo 🚀 %APP_NAME% v%APP_VERSION% - INSTALAÇÃO RÁPIDA
echo ================================================================
echo.
echo 📋 Este script irá:
echo    ✅ Verificar pré-requisitos
echo    ✅ Configurar banco de dados SQLite
echo    ✅ Instalar dependências
echo    ✅ Executar a aplicação
echo.
echo 💡 Para instalação avançada, use:
echo    Scripts\Install-IntranetDocumentos.ps1
echo.

REM ================================================================
REM VERIFICAR PRIVILÉGIOS
REM ================================================================

echo 🔍 Verificando privilégios...
net session >nul 2>&1
if errorlevel 1 (
    echo ❌ ERRO: Este script deve ser executado como Administrador!
    echo.
    echo 💡 Clique com botão direito no script e selecione "Executar como administrador"
    pause
    exit /b 1
)
echo ✅ Privilégios de administrador confirmados

REM ================================================================
REM VERIFICAR .NET
REM ================================================================

echo 🔍 Verificando .NET...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET não encontrado!
    echo.
    echo 🔽 Baixe e instale o .NET 8.0 de:
    echo    https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

for /f "tokens=*" %%a in ('dotnet --version 2^>nul') do set "DOTNET_VERSION=%%a"
echo ✅ .NET %DOTNET_VERSION% encontrado

REM ================================================================
REM VERIFICAR ARQUIVOS
REM ================================================================

echo 🔍 Verificando arquivos da aplicação...
if not exist "%APP_ROOT%IntranetDocumentos.csproj" (
    echo ❌ Arquivo de projeto não encontrado!
    echo    Esperado: %APP_ROOT%IntranetDocumentos.csproj
    pause
    exit /b 1
)
echo ✅ Arquivos da aplicação encontrados

REM ================================================================
REM CONFIGURAR BANCO DE DADOS
REM ================================================================

echo.
echo 🗄️ Configurando banco de dados...

if exist "%APP_ROOT%Scripts\Database\Setup-Database.ps1" (
    echo 📜 Executando configuração de banco via PowerShell...
    powershell.exe -ExecutionPolicy Bypass -File "%APP_ROOT%Scripts\Database\Setup-Database.ps1" -DatabaseType SQLite
    if errorlevel 1 (
        echo ⚠️ Erro na configuração do banco, mas continuando...
    ) else (
        echo ✅ Banco de dados configurado
    )
) else (
    echo 📦 Executando migrações EF Core...
    cd /d "%APP_ROOT%"
    dotnet ef database update --project IntranetDocumentos.csproj
    if errorlevel 1 (
        echo ⚠️ Erro nas migrações, mas continuando...
    ) else (
        echo ✅ Migrações executadas
    )
)

REM ================================================================
REM RESTAURAR DEPENDÊNCIAS
REM ================================================================

echo.
echo 📦 Restaurando dependências...
cd /d "%APP_ROOT%"
dotnet restore IntranetDocumentos.csproj
if errorlevel 1 (
    echo ❌ Erro ao restaurar dependências!
    pause
    exit /b 1
)
echo ✅ Dependências restauradas

REM ================================================================
REM BUILD DA APLICAÇÃO
REM ================================================================

echo.
echo 🔨 Compilando aplicação...
dotnet build IntranetDocumentos.csproj --configuration Release
if errorlevel 1 (
    echo ❌ Erro na compilação!
    pause
    exit /b 1
)
echo ✅ Aplicação compilada

REM ================================================================
REM VERIFICAÇÕES FINAIS
REM ================================================================

echo.
echo 🔍 Verificações finais...

REM Verificar se banco foi criado
if exist "%APP_ROOT%IntranetDocumentos.db" (
    echo ✅ Banco de dados SQLite criado
) else (
    echo ⚠️ Banco de dados não encontrado, mas continuando...
)

REM Verificar pasta de documentos
if not exist "%APP_ROOT%DocumentsStorage" mkdir "%APP_ROOT%DocumentsStorage"
echo ✅ Pasta de documentos preparada

REM ================================================================
REM INICIAR APLICAÇÃO
REM ================================================================

echo.
echo ================================================================
echo 🚀 INICIANDO APLICAÇÃO
echo ================================================================
echo.
echo 🌐 A aplicação será iniciada em:
echo    HTTP:  http://localhost:5000
echo    HTTPS: https://localhost:5001
echo.
echo 👤 Login padrão:
echo    Email: admin@intranet.com
echo    Senha: Admin@123
echo.
echo 📚 Documentação completa:
echo    DOCUMENTACAO-OFICIAL-UNIFICADA.md
echo.
echo 💡 Para parar a aplicação, pressione Ctrl+C
echo.

REM Aguardar confirmação
set /p "START_APP=Iniciar aplicação agora? (S/n): "
if /i "%START_APP%"=="n" (
    echo.
    echo ✅ Instalação concluída!
    echo 💡 Para iniciar manualmente: dotnet run --project IntranetDocumentos.csproj
    pause
    exit /b 0
)

REM Iniciar aplicação
echo.
echo 🚀 Iniciando %APP_NAME%...
echo.
dotnet run --project IntranetDocumentos.csproj

REM ================================================================
REM FINALIZAÇÃO
REM ================================================================

echo.
echo ================================================================
echo ✅ INSTALAÇÃO RÁPIDA CONCLUÍDA
echo ================================================================
echo.
echo 📋 Log da instalação: %LOG_FILE%
echo 📚 Documentação: DOCUMENTACAO-OFICIAL-UNIFICADA.md
echo 💻 Repositório: https://github.com/Vollupios/mc_web_app
echo.
echo 🎉 Obrigado por usar o %APP_NAME%!
echo.
pause
