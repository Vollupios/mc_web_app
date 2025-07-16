# 📚 Guia Unificado Final - Intranet Documentos

**Sistema de Gestão de Documentos Corporativos**  
*ASP.NET Core 9.0 + MySQL + Bootstrap 5*

---

## 🎯 COMEÇAR AQUI - Instalação Rápida

### Para Usuários que Querem Instalar Rapidamente

```batch
# 1. Extrair arquivos para C:\Deploy\IntranetDocumentos\
# 2. Executar como Administrador:
deploy-quick.bat

# 3. Configurar senhas em:
# C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json

# 4. Verificar instalação:
.\Verificacao-Pos-Instalacao.ps1
```

**⏱️ Tempo estimado: 10-15 minutos**

---

## 📋 Índice Completo

### 🚀 Instalação

1. [Instalação Rápida](#-instalação-rápida-1-comando)
2. [Pré-requisitos](#-pré-requisitos)
3. [Instalação Manual](#-instalação-manual-detalhada)

### ⚙️ Configuração

4. [Configuração de Produção](#-configuração-de-produção)
5. [Deploy Remoto](#-deploy-remoto)
6. [Configurações Avançadas](#-configurações-avançadas)

### 🔧 Manutenção

7. [Verificação e Diagnóstico](#-verificação-e-diagnóstico)
8. [Solução de Problemas](#-solução-de-problemas)
9. [Backup e Manutenção](#-backup-e-manutenção)

### 📖 Referência

10. [Scripts e Arquivos](#-scripts-e-arquivos)
11. [Checklist de Verificação](#-checklist-de-verificação)
12. [Segurança e Hardening](#-segurança-e-hardening)
13. [Contato e Suporte](#-contato-e-suporte)

---

## 🚀 Instalação Rápida (1 Comando)

### Método Automatizado

**1. Preparar Ambiente**

```batch
# Baixar e extrair para:
C:\Deploy\IntranetDocumentos\
```

**2. Executar Instalação**

```batch
# Clique com botão direito → "Executar como administrador"
deploy-quick.bat
```

**3. Configurar Aplicação**

- Edite `C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json`
- Configure:
  - String de conexão MySQL
  - Configurações de email (SMTP)
  - Chaves de segurança

**4. Verificar Instalação**

```powershell
# Execute para verificar se tudo está funcionando
.\Verificacao-Pos-Instalacao.ps1
```

**5. Acessar Sistema**

- URL: `http://localhost/IntranetDocumentos`
- Login padrão: `admin@empresa.com.br` / `Admin123!`

---

## 📋 Pré-requisitos

### Sistema Operacional

- ✅ **Windows Server 2019/2022** (recomendado)
- ✅ **Windows 10/11 Pro** (desenvolvimento/teste)

### Software Obrigatório

- ✅ **IIS (Internet Information Services)**
- ✅ **.NET 9.0 Hosting Bundle** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **MySQL Server 8.0+** ou **MariaDB 10.5+**

### Hardware Recomendado

- **CPU**: 2+ cores (4+ recomendado)
- **RAM**: 4GB mínimo (8GB+ recomendado)
- **Disco**: 20GB+ espaço livre
- **Rede**: Conexão estável

### Permissões

- ✅ **Administrador** no Windows Server
- ✅ **Escrita** em `C:\inetpub\wwwroot`
- ✅ **Criação** de diretórios em `C:\IntranetData`

---

## 🛠️ Instalação Manual Detalhada

### Passo 1: Instalar .NET 9.0

```powershell
# Baixar e instalar .NET 9.0 Hosting Bundle
# Link: https://dotnet.microsoft.com/download/dotnet/9.0

# Verificar instalação
dotnet --info
```

### Passo 2: Configurar IIS

```powershell
# Execute como Administrador
.\Configuracao-IIS.ps1

# Ou manualmente:
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
```

### Passo 3: Instalar MySQL

```batch
# Download MySQL Server 8.0+
# https://dev.mysql.com/downloads/mysql/

# Configurar usuário e senha durante instalação
# Anotar senha do root para configuração posterior
```

### Passo 4: Criar Banco de Dados

```sql
-- Execute no MySQL Workbench ou linha de comando
mysql -u root -p < setup-mysql.sql

-- Ou manualmente:
CREATE DATABASE IntranetDocumentos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'intranet_user'@'localhost' IDENTIFIED BY 'SuaSenhaSegura123!';
GRANT ALL PRIVILEGES ON IntranetDocumentos.* TO 'intranet_user'@'localhost';
FLUSH PRIVILEGES;
```

### Passo 5: Deploy da Aplicação

```powershell
# No ambiente de desenvolvimento (ou usar arquivos já compilados)
dotnet publish -c Release -o ./publish --self-contained false

# Copiar arquivos para IIS
xcopy .\publish C:\inetpub\wwwroot\IntranetDocumentos\ /E /I /H

# Ou usar script automatizado
.\Deploy-WindowsServer.ps1 -MySqlPassword "SuaSenhaSegura123!"
```

### Passo 6: Configurar Aplicação

```json
// Editar C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=intranet_user;Password=SuaSenhaSegura123!;AllowUserVariables=true;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "sistema@suaempresa.com.br",
    "SenderPassword": "suasenhaapp",
    "SenderName": "Sistema Intranet"
  }
}
```

### Passo 7: Executar Migrations

```powershell
# Na pasta da aplicação
cd C:\inetpub\wwwroot\IntranetDocumentos
dotnet IntranetDocumentos.dll --environment Production

# Aguardar criação das tabelas no banco
```

### Passo 8: Configurar Site no IIS

```powershell
# Criar site no IIS
New-WebSite -Name "IntranetDocumentos" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\IntranetDocumentos"

# Configurar Application Pool
New-WebAppPool -Name "IntranetDocumentosPool"
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentosPool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\Sites\IntranetDocumentos" -Name "applicationPool" -Value "IntranetDocumentosPool"
```

---

## ⚙️ Configuração de Produção

### Configuração de Segurança

```json
// appsettings.Production.json - Seção Security
{
  "Security": {
    "RequireHttps": true,
    "DataProtectionKeysPath": "C:\\Keys\\IntranetDocumentos",
    "AllowedHosts": "localhost;intranet.empresa.com.br",
    "CookieSecurePolicy": "Always"
  }
}
```

### Configuração de Armazenamento

```json
// appsettings.Production.json - Caminhos
{
  "Storage": {
    "DocumentsPath": "C:\\IntranetData\\Documents",
    "BackupPath": "C:\\IntranetData\\Backups",
    "TempPath": "C:\\IntranetData\\Temp",
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".png", ".zip"]
  }
}
```

### Configuração de Logging

```json
// appsettings.Production.json - Logging
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "IntranetDocumentos": "Information"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Warning"
      },
      "SourceName": "IntranetDocumentos",
      "LogName": "Application"
    }
  }
}
```

### Configuração de Performance

```xml
<!-- web.config -->
<system.webServer>
  <staticContent>
    <remove fileExtension=".woff" />
    <mimeMap fileExtension=".woff" mimeType="font/woff" />
    <remove fileExtension=".woff2" />
    <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="365.00:00:00" />
  </staticContent>
  
  <httpCompression>
    <dynamicTypes>
      <add mimeType="text/*" enabled="true" />
      <add mimeType="application/javascript" enabled="true" />
      <add mimeType="application/json" enabled="true" />
    </dynamicTypes>
  </httpCompression>
</system.webServer>
```

---

## 🌐 Deploy Remoto

### Deploy para Servidor Remoto

```powershell
# Configure as variáveis no script
$ServerIP = "192.168.1.100"
$Username = "Administrator"
$Password = "SenhaDoServidor"

# Execute o deploy remoto
.\Publish-ToWindowsServer.ps1 -ServerIP $ServerIP -Username $Username -Password $Password
```

### Deploy via FTP

```powershell
# Configurar FTP no script
$FtpServer = "ftp.suaempresa.com.br"
$FtpUsername = "deploy"
$FtpPassword = "senhaFTP"

# Publicar via FTP
.\Deploy-FTP.ps1 -Server $FtpServer -Username $FtpUsername -Password $FtpPassword
```

### Deploy via Azure DevOps

```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    configuration: 'Release'

- task: DotNetCoreCLI@2
  displayName: 'Publish'
  inputs:
    command: 'publish'
    configuration: 'Release'
    arguments: '--output $(Build.ArtifactStagingDirectory)'

- task: IISWebAppDeploymentOnMachineGroup@0
  displayName: 'Deploy to IIS'
  inputs:
    WebSiteName: 'IntranetDocumentos'
    Package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
```

---

## 🔍 Verificação e Diagnóstico

### Script de Verificação Automática

```powershell
# Execute para verificar toda a instalação
.\Verificacao-Pos-Instalacao.ps1

# Saída esperada:
# ✅ IIS: Configurado e funcionando
# ✅ .NET 9.0: Instalado
# ✅ MySQL: Conectando
# ✅ Aplicação: Respondendo
# ✅ Permissões: Configuradas
```

### Verificação Manual

```powershell
# 1. Verificar .NET
dotnet --info

# 2. Verificar IIS
Get-WindowsFeature -Name IIS-* | Where-Object InstallState -eq Installed

# 3. Verificar MySQL
mysql -u intranet_user -p -e "SHOW DATABASES;"

# 4. Verificar aplicação
Invoke-WebRequest -Uri "http://localhost/IntranetDocumentos" -UseBasicParsing

# 5. Verificar logs
Get-EventLog -LogName Application -Source "IntranetDocumentos" -Newest 10
```

### Verificação de Performance

```powershell
# Monitorar recursos
Get-Counter "\Process(w3wp)\% Processor Time"
Get-Counter "\Memory\Available MBytes"
Get-Counter "\Web Service(IntranetDocumentos)\Current Connections"

# Verificar tempos de resposta
Measure-Command { Invoke-WebRequest "http://localhost/IntranetDocumentos" }
```

---

## 🚨 Solução de Problemas

### Problema: Aplicação não inicia

**Sintomas**: Erro 500, página em branco, timeout

**Diagnóstico**:

```powershell
# Verificar logs detalhados
Get-EventLog -LogName Application -Source "IIS AspNetCore Module V2" -Newest 5

# Verificar processo
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
```

**Soluções**:

1. **Verificar .NET Hosting Bundle**

   ```powershell
   # Reinstalar se necessário
   dotnet --info | Select-String "Host"
   ```

2. **Verificar permissões**

   ```powershell
   # Dar permissões ao IIS_IUSRS
   icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "IIS_IUSRS:(OI)(CI)F" /T
   ```

3. **Verificar web.config**

   ```xml
   <!-- Verificar se está configurado corretamente -->
   <aspNetCore processPath="dotnet" arguments=".\IntranetDocumentos.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
   ```

### Problema: Erro de conexão com MySQL

**Sintomas**: "Unable to connect to database"

**Diagnóstico**:

```sql
-- Testar conexão
mysql -u intranet_user -p -h localhost

-- Verificar usuário
SELECT User, Host FROM mysql.user WHERE User = 'intranet_user';

-- Verificar privilégios
SHOW GRANTS FOR 'intranet_user'@'localhost';
```

**Soluções**:

1. **Recriar usuário**

   ```sql
   DROP USER 'intranet_user'@'localhost';
   CREATE USER 'intranet_user'@'localhost' IDENTIFIED BY 'SuaSenhaSegura123!';
   GRANT ALL PRIVILEGES ON IntranetDocumentos.* TO 'intranet_user'@'localhost';
   FLUSH PRIVILEGES;
   ```

2. **Verificar string de conexão**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=intranet_user;Password=SuaSenhaSegura123!;AllowUserVariables=true;CharSet=utf8mb4;"
     }
   }
   ```

### Problema: Upload de arquivos falha

**Sintomas**: Erro ao fazer upload, "Path not found"

**Diagnóstico**:

```powershell
# Verificar diretórios
Test-Path "C:\IntranetData\Documents"
Get-Acl "C:\IntranetData\Documents"
```

**Soluções**:

1. **Criar diretórios**

   ```powershell
   New-Item -ItemType Directory -Path "C:\IntranetData\Documents" -Force
   New-Item -ItemType Directory -Path "C:\IntranetData\Backups" -Force
   New-Item -ItemType Directory -Path "C:\IntranetData\Temp" -Force
   ```

2. **Configurar permissões**

   ```powershell
   icacls "C:\IntranetData" /grant "IIS_IUSRS:(OI)(CI)F" /T
   icacls "C:\IntranetData" /grant "BUILTIN\Users:(OI)(CI)M" /T
   ```

### Problema: Emails não são enviados

**Sintomas**: Notificações não chegam, erro SMTP

**Diagnóstico**:

```powershell
# Testar SMTP
Test-NetConnection -ComputerName "smtp.gmail.com" -Port 587
```

**Soluções**:

1. **Verificar configurações SMTP**

   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "EnableSsl": true,
       "UseDefaultCredentials": false,
       "SenderEmail": "sistema@empresa.com.br",
       "SenderPassword": "suasenhaapp"
     }
   }
   ```

2. **Gmail: Configurar senha de app**
   - Ativar autenticação de 2 fatores
   - Gerar senha de aplicativo
   - Usar senha de app no lugar da senha normal

---

## 💾 Backup e Manutenção

### Backup Automático

```powershell
# Script de backup (executar diariamente)
.\backup-daily.ps1

# Configurar tarefa agendada
$Trigger = New-ScheduledTaskTrigger -Daily -At "02:00AM"
$Action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Scripts\backup-daily.ps1"
Register-ScheduledTask -TaskName "IntranetBackup" -Trigger $Trigger -Action $Action -RunLevel Highest
```

### Backup Manual

```powershell
# Backup do banco de dados
mysqldump -u root -p IntranetDocumentos > "C:\Backups\db_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"

# Backup dos arquivos
Compress-Archive -Path "C:\IntranetData\Documents" -DestinationPath "C:\Backups\files_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"

# Backup da aplicação
Compress-Archive -Path "C:\inetpub\wwwroot\IntranetDocumentos" -DestinationPath "C:\Backups\app_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
```

### Manutenção Preventiva

```powershell
# Limpeza de logs antigos (executar semanalmente)
Get-ChildItem "C:\inetpub\wwwroot\IntranetDocumentos\logs" -Filter "*.log" | 
Where-Object CreationTime -lt (Get-Date).AddDays(-30) | 
Remove-Item -Force

# Limpeza de arquivos temporários
Get-ChildItem "C:\IntranetData\Temp" | 
Where-Object CreationTime -lt (Get-Date).AddDays(-7) | 
Remove-Item -Force -Recurse

# Verificação de integridade do banco
mysql -u root -p -e "CHECK TABLE IntranetDocumentos.Documents;"
```

### Monitoramento

```powershell
# Verificar espaço em disco
Get-WmiObject -Class Win32_LogicalDisk | 
Select-Object DeviceID, @{Name="Size(GB)";Expression={[math]::Round($_.Size/1GB,2)}}, @{Name="FreeSpace(GB)";Expression={[math]::Round($_.FreeSpace/1GB,2)}}

# Verificar performance da aplicação
Get-Counter "\Web Service(IntranetDocumentos)\Get Requests/sec"
Get-Counter "\Web Service(IntranetDocumentos)\Post Requests/sec"
Get-Counter "\ASP.NET Apps v4.0.30319(IntranetDocumentos)\Requests/Sec"

# Verificar conexões ativas
netstat -an | findstr :80 | findstr ESTABLISHED
```

---

## 📁 Scripts e Arquivos

### Scripts de Instalação

| Script | Descrição | Uso |
|--------|-----------|-----|
| `deploy-quick.bat` | Instalação automática completa | Execute como Admin |
| `Deploy-WindowsServer.ps1` | Deploy principal PowerShell | `.\Deploy-WindowsServer.ps1 -MySqlPassword "senha"` |
| `Configuracao-IIS.ps1` | Configuração específica do IIS | `.\Configuracao-IIS.ps1` |
| `Publish-ToWindowsServer.ps1` | Deploy remoto | `.\Publish-ToWindowsServer.ps1 -ServerIP "IP"` |
| `Verificacao-Pos-Instalacao.ps1` | Verificação pós-instalação | `.\Verificacao-Pos-Instalacao.ps1` |

### Arquivos de Configuração

| Arquivo | Descrição | Localização |
|---------|-----------|-------------|
| `appsettings.Production.json` | Configurações de produção | Raiz da aplicação |
| `web.config` | Configuração do IIS | Raiz da aplicação |
| `setup-mysql.sql` | Setup do banco MySQL | Scripts de instalação |

### Estrutura de Diretórios

```
C:\inetpub\wwwroot\IntranetDocumentos\     # Aplicação
C:\IntranetData\Documents\                 # Documentos
C:\IntranetData\Backups\                   # Backups
C:\IntranetData\Temp\                      # Arquivos temporários
C:\Keys\IntranetDocumentos\                # Chaves de proteção de dados
C:\Scripts\                                # Scripts de manutenção
C:\Logs\                                   # Logs personalizados
```

---

## ✅ Checklist de Verificação

### Pré-instalação

- [ ] Windows Server 2019/2022 instalado
- [ ] IIS habilitado e configurado
- [ ] .NET 9.0 Hosting Bundle instalado
- [ ] MySQL Server 8.0+ instalado
- [ ] Permissões de administrador disponíveis
- [ ] Firewall configurado (portas 80, 443, 3306)

### Durante a Instalação

- [ ] Scripts executados como administrador
- [ ] Banco de dados criado com sucesso
- [ ] Usuário MySQL criado e configurado
- [ ] Aplicação publicada em `C:\inetpub\wwwroot\IntranetDocumentos`
- [ ] Site IIS criado e configurado
- [ ] Application Pool configurado

### Pós-instalação

- [ ] `appsettings.Production.json` configurado
- [ ] Conexão com banco de dados funcionando
- [ ] Migrations executadas
- [ ] Site acessível via browser
- [ ] Login admin funcionando
- [ ] Upload de documentos funcionando
- [ ] Emails sendo enviados (se configurado)
- [ ] Logs sendo gravados

### Produção

- [ ] Backup automático configurado
- [ ] Monitoramento implementado
- [ ] SSL/TLS configurado (se necessário)
- [ ] DNS configurado (se necessário)
- [ ] Usuários criados e testados
- [ ] Departamentos configurados
- [ ] Permissões verificadas

---

## 🔐 Segurança e Hardening

### 🚨 Avaliação de Segurança

A segurança da aplicação é **crítica** para proteger dados corporativos. Esta seção aborda as principais vulnerabilidades e suas correções.

#### Análise Completa

Para uma análise detalhada de segurança, consulte:

- **[ANALISE-SEGURANCA.md](ANALISE-SEGURANCA.md)** - Análise completa de vulnerabilidades
- **Scripts de correção** incluídos no pacote

### ⚡ Hardening Rápido

Execute o script automatizado para aplicar as correções mais críticas:

```powershell
# Execute como Administrador
.\Hardening-Seguranca.ps1 -Force
```

**Este script:**

- ✅ Altera senhas padrão automaticamente
- ✅ Configura headers de segurança
- ✅ Aplica permissões restritivas
- ✅ Remove headers desnecessários
- ✅ Cria backup das configurações

### 🔍 Auditoria Contínua

Execute auditoria de segurança regularmente:

```powershell
# Auditoria básica
.\Auditoria-Seguranca.ps1

# Auditoria detalhada com relatório
.\Auditoria-Seguranca.ps1 -Detailed -ExportReport
```

### 🛡️ Melhorias Críticas

#### 1. **Política de Senhas Forte**

**❌ Configuração Atual (Insegura):**

```csharp
options.Password.RequiredLength = 6;              // Muito baixo
options.Password.RequireNonAlphanumeric = false;  // Sem símbolos
options.Password.RequireUppercase = false;        // Sem maiúsculas
```

**✅ Configuração Recomendada (Segura):**

```csharp
options.Password.RequiredLength = 12;             // Mínimo 12 caracteres
options.Password.RequireNonAlphanumeric = true;   // Símbolos obrigatórios
options.Password.RequireUppercase = true;         // Maiúsculas obrigatórias
options.Password.RequireLowercase = true;         // Minúsculas obrigatórias
options.Password.RequiredUniqueChars = 6;         // Caracteres únicos
```

#### 2. **Headers de Segurança**

Adicione ao `web.config`:

```xml
<httpProtocol>
  <customHeaders>
    <!-- Proteção XSS e Clickjacking -->
    <add name="X-Content-Type-Options" value="nosniff" />
    <add name="X-Frame-Options" value="DENY" />
    <add name="X-XSS-Protection" value="1; mode=block" />
    
    <!-- Content Security Policy -->
    <add name="Content-Security-Policy" 
         value="default-src 'self'; script-src 'self' 'unsafe-inline';" />
    
    <!-- HSTS para HTTPS -->
    <add name="Strict-Transport-Security" 
         value="max-age=31536000; includeSubDomains" />
    
    <!-- Remover headers que expõem informações -->
    <remove name="Server" />
    <remove name="X-Powered-By" />
  </customHeaders>
</httpProtocol>
```

#### 3. **Validação de Upload Rigorosa**

Implemente validação avançada de arquivos:

```csharp
public class SecureFileValidator
{
    private static readonly string[] AllowedExtensions = 
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt"
        // ❌ NUNCA: .exe, .bat, .ps1, .js, .vbs
    };

    public bool ValidateFile(IFormFile file)
    {
        // 1. Validar extensão
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return false;

        // 2. Validar MIME type real
        if (!ValidateMimeType(file))
            return false;

        // 3. Verificar assinatura de arquivo (magic bytes)
        if (!ValidateFileSignature(file))
            return false;

        return true;
    }
}
```

### 🔒 Configurações de Produção Seguras

#### Cookies Seguros

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;              // Protege contra XSS
    options.Cookie.SameSite = SameSiteMode.Strict; // Protege contra CSRF
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS apenas
    options.ExpireTimeSpan = TimeSpan.FromHours(2);  // Sessão mais curta
    options.SlidingExpiration = false;           // Não renovar automaticamente
});
```

#### Rate Limiting

```csharp
// Middleware para prevenir ataques de força bruta
public class RateLimitingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        
        if (IsRateLimited(clientIp, context.Request.Path))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        await next(context);
    }
}
```

### 📊 Monitoramento de Segurança

#### Logs Críticos para Monitorar

1. **Tentativas de login falhadas múltiplas**
2. **Upload de arquivos com extensões suspeitas**
3. **Tentativas de acesso não autorizado**
4. **Alterações em configurações administrativas**
5. **Downloads em massa de documentos**

#### Configuração de Alertas

```csharp
public class SecurityAuditService
{
    public async Task LogSecurityEventAsync(string eventType, string details)
    {
        // 1. Log crítico para monitoramento
        _logger.LogCritical("SECURITY EVENT: {EventType} - {Details}", 
            eventType, details);

        // 2. Email para administradores em eventos críticos
        if (IsCriticalEvent(eventType))
        {
            await _emailService.SendSecurityAlertAsync(eventType, details);
        }

        // 3. Salvar em tabela de auditoria
        await SaveToAuditLogAsync(eventType, details);
    }
}
```

### 🛠️ Scripts de Segurança

| Script | Função | Uso |
|--------|--------|-----|
| `Hardening-Seguranca.ps1` | Aplicar correções automáticas | `.\Hardening-Seguranca.ps1 -Force` |
| `Auditoria-Seguranca.ps1` | Verificar estado de segurança | `.\Auditoria-Seguranca.ps1 -Detailed` |
| `Backup-Configuracoes.ps1` | Backup antes de alterações | `.\Backup-Configuracoes.ps1` |

### ✅ Checklist de Segurança

#### ⚡ **Crítico (Implementar Imediatamente)**

- [ ] **Senha administrativa alterada** (não usar "Admin123!")
- [ ] **Headers de segurança configurados** no web.config
- [ ] **Permissões de diretório restringidas** (sem "Everyone")
- [ ] **HTTPS configurado** e HTTP redirecionado
- [ ] **Validação de upload implementada** (tipos de arquivo)

#### 🔒 **Alta Prioridade**

- [ ] **Política de senhas fortalecida** (12+ caracteres, complexidade)
- [ ] **Rate limiting implementado** (anti força bruta)
- [ ] **Logs de segurança ativos** (tentativas de login, uploads)
- [ ] **Cookies seguros configurados** (HttpOnly, Secure, SameSite)
- [ ] **Backup automático configurado** com criptografia

#### 📋 **Média Prioridade**

- [ ] **Auditoria regular de segurança** (semanal)
- [ ] **Monitoramento de logs** automatizado
- [ ] **Alertas de segurança** por email configurados
- [ ] **Antivírus integrado** para uploads
- [ ] **2FA implementado** para administradores

### 🚨 Resposta a Incidentes

#### Em Caso de Suspeita de Comprometimento

1. **Imediato:**

   ```powershell
   # Executar auditoria de emergência
   .\Auditoria-Seguranca.ps1 -Detailed -ExportReport
   
   # Verificar logs suspeitos
   Get-EventLog -LogName Security -Newest 100 | Where-Object {$_.EntryType -eq "FailureAudit"}
   ```

2. **Investigação:**
   - Verificar uploads recentes suspeitos
   - Analisar logs de acesso não autorizado
   - Verificar alterações em configurações

3. **Contenção:**
   - Desabilitar contas comprometidas
   - Alterar todas as senhas
   - Revisar permissões de acesso

4. **Recuperação:**
   - Restaurar backups se necessário
   - Aplicar patches de segurança
   - Reforçar monitoramento

### 📞 Suporte de Segurança

Para questões críticas de segurança:

1. **Execute auditoria:** `.\Auditoria-Seguranca.ps1 -ExportReport`
2. **Aplique hardening:** `.\Hardening-Seguranca.ps1 -Force`
3. **Consulte:** `ANALISE-SEGURANCA.md` para detalhes técnicos
4. **Contate:** Equipe de desenvolvimento com relatório de auditoria

**⚠️ Importante:** Implemente as correções de segurança em ambiente de teste antes de aplicar em produção.
