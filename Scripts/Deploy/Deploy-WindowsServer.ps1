# Script de Deploy para Windows Server
# Execute como Administrador

param(
    [string]$SiteName = "Intranet Documentos",
    [string]$AppPoolName = "IntranetDocumentos",
    [string]$SitePath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [string]$DataPath = "C:\IntranetData",
    [int]$Port = 80,
    [string]$MySqlPassword = "",
    [string]$EmailPassword = ""
)

Write-Host "=== Deploy Intranet Documentos - Windows Server ===" -ForegroundColor Green

# Verificar se está executando como administrador
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Error "Este script deve ser executado como Administrador!"
    exit 1
}

# 1. Verificar .NET 9.0 Runtime
Write-Host "1. Verificando .NET 9.0 Runtime..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -like "9.*") {
        Write-Host "✓ .NET 9.0 encontrado: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Warning "⚠ .NET 9.0 não encontrado. Instale o .NET 9.0 Hosting Bundle"
        Write-Host "Download: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
    }
} catch {
    Write-Error "✗ .NET Runtime não encontrado. Instale o .NET 9.0 Hosting Bundle"
    exit 1
}

# 2. Verificar IIS
Write-Host "2. Verificando IIS..." -ForegroundColor Yellow
$iisFeature = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
if ($iisFeature.State -eq "Enabled") {
    Write-Host "✓ IIS está habilitado" -ForegroundColor Green
} else {
    Write-Host "Habilitando IIS..." -ForegroundColor Yellow
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent -All
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument -All
    Write-Host "✓ IIS habilitado" -ForegroundColor Green
}

# 3. Criar estrutura de pastas
Write-Host "3. Criando estrutura de pastas..." -ForegroundColor Yellow
$folders = @(
    $SitePath,
    "$DataPath\Documents",
    "$DataPath\Backups",
    "$DataPath\Logs"
)

foreach ($folder in $folders) {
    if (!(Test-Path $folder)) {
        New-Item -Path $folder -ItemType Directory -Force
        Write-Host "✓ Pasta criada: $folder" -ForegroundColor Green
    } else {
        Write-Host "✓ Pasta já existe: $folder" -ForegroundColor Green
    }
}

# 4. Configurar Application Pool
Write-Host "4. Configurando Application Pool..." -ForegroundColor Yellow
Import-Module WebAdministration

if (Get-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
    Remove-WebAppPool -Name $AppPoolName
}

New-WebAppPool -Name $AppPoolName -Force
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false

Write-Host "✓ Application Pool '$AppPoolName' configurado" -ForegroundColor Green

# 5. Configurar Site IIS
Write-Host "5. Configurando Site IIS..." -ForegroundColor Yellow

if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Remove-Website -Name $SiteName
}

New-Website -Name $SiteName -Port $Port -PhysicalPath $SitePath -ApplicationPool $AppPoolName
Write-Host "✓ Site '$SiteName' criado na porta $Port" -ForegroundColor Green

# 6. Configurar permissões
Write-Host "6. Configurando permissões..." -ForegroundColor Yellow

# Permissões para Application Pool Identity
$appPoolIdentity = "IIS AppPool\$AppPoolName"

# Permissões na pasta de dados
icacls $DataPath /grant "${appPoolIdentity}:(OI)(CI)F" /T

# Permissões na pasta do site (leitura/execução)
icacls $SitePath /grant "${appPoolIdentity}:(OI)(CI)RX" /T

Write-Host "✓ Permissões configuradas" -ForegroundColor Green

# 7. Configurar appsettings.Production.json
Write-Host "7. Configurando appsettings.Production.json..." -ForegroundColor Yellow

if ($MySqlPassword -and $EmailPassword) {
    $appsettingsPath = "$SitePath\appsettings.Production.json"
    if (Test-Path $appsettingsPath) {
        $content = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        # Atualizar senha do MySQL
        $content.ConnectionStrings.DefaultConnection = $content.ConnectionStrings.DefaultConnection -replace "CHANGE_THIS_PASSWORD", $MySqlPassword
        
        # Atualizar senha do email
        $content.EmailSettings.SmtpPassword = $EmailPassword
        
        $content | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        Write-Host "✓ Senhas atualizadas no appsettings.Production.json" -ForegroundColor Green
    }
} else {
    Write-Warning "⚠ Lembre-se de configurar as senhas no appsettings.Production.json"
    Write-Host "Arquivo: $SitePath\appsettings.Production.json" -ForegroundColor Cyan
}

# 8. Criar Event Source para logging
Write-Host "8. Configurando Event Source..." -ForegroundColor Yellow
try {
    if (![System.Diagnostics.EventLog]::SourceExists("IntranetDocumentos")) {
        [System.Diagnostics.EventLog]::CreateEventSource("IntranetDocumentos", "Application")
        Write-Host "✓ Event Source 'IntranetDocumentos' criado" -ForegroundColor Green
    } else {
        Write-Host "✓ Event Source 'IntranetDocumentos' já existe" -ForegroundColor Green
    }
} catch {
    Write-Warning "⚠ Não foi possível criar Event Source. Execute novamente como Administrador."
}

# 9. Configurar Firewall (opcional)
Write-Host "9. Configurando Firewall..." -ForegroundColor Yellow
try {
    $existingRule = Get-NetFirewallRule -DisplayName "HTTP Intranet" -ErrorAction SilentlyContinue
    if (!$existingRule) {
        New-NetFirewallRule -DisplayName "HTTP Intranet" -Direction Inbound -Port $Port -Protocol TCP -Action Allow
        Write-Host "✓ Regra de firewall criada para porta $Port" -ForegroundColor Green
    } else {
        Write-Host "✓ Regra de firewall já existe" -ForegroundColor Green
    }
} catch {
    Write-Warning "⚠ Não foi possível configurar firewall automaticamente"
}

Write-Host "`n=== Deploy Concluído ===" -ForegroundColor Green
Write-Host "Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Copie os arquivos da aplicação para: $SitePath" -ForegroundColor White
Write-Host "2. Configure MySQL e execute as migrations" -ForegroundColor White
Write-Host "3. Configure SMTP no appsettings.Production.json" -ForegroundColor White
Write-Host "4. Teste o site em: http://localhost:$Port" -ForegroundColor White

Write-Host "`nArquivos importantes:" -ForegroundColor Yellow
Write-Host "- Site: $SitePath" -ForegroundColor White
Write-Host "- Dados: $DataPath" -ForegroundColor White
Write-Host "- Logs: Visualizador de Eventos > Application" -ForegroundColor White
