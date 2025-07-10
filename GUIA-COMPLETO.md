# 📚 Guia Completo de Instalação e Deploy - Intranet Documentos

**Sistema de Gestão de Documentos Corporativos**  
*ASP.NET Core 9.0 + MySQL + Bootstrap 5*

---

## 📋 Índice

1. [Visão Geral](#-visão-geral)
2. [Pré-requisitos](#-pré-requisitos)
3. [Instalação Rápida](#-instalação-rápida)
4. [Instalação Manual Detalhada](#-instalação-manual-detalhada)
5. [Configuração de Produção](#-configuração-de-produção)
6. [Deploy Remoto](#-deploy-remoto)
7. [Verificação e Diagnóstico](#-verificação-e-diagnóstico)
8. [Configurações Avançadas](#-configurações-avançadas)
9. [Solução de Problemas](#-solução-de-problemas)
10. [Manutenção e Backup](#-manutenção-e-backup)
11. [Arquivos de Instalação](#-arquivos-de-instalação)
12. [Status e Checklist](#-status-e-checklist)

---

## 🎯 Visão Geral

A **Intranet Documentos** é um sistema completo de gestão de documentos corporativos que oferece:

### Funcionalidades Principais

- ✅ **Gestão de Documentos** - Upload, download e organização por departamentos
- ✅ **Controle de Acesso** - Permissões baseadas em departamentos e roles
- ✅ **Sistema de Reuniões** - Agendamento e controle de salas/veículos
- ✅ **Analytics** - Relatórios de uso e downloads
- ✅ **Backup Automático** - Sistema de backup configurável
- ✅ **Notificações** - Sistema de emails e alertas
- ✅ **Interface Responsiva** - Bootstrap 5 moderno

### Tecnologias Utilizadas

- **Backend**: ASP.NET Core 9.0 MVC
- **Banco de Dados**: MySQL 8.0+ / MariaDB 10.5+
- **ORM**: Entity Framework Core
- **Autenticação**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5 + Bootstrap Icons
- **Servidor Web**: IIS com ASP.NET Core Module V2

### Arquitetura

- **Padrão**: Repository/Service Pattern
- **Segurança**: Role-based access control
- **Organização**: Multi-departamento (Pessoal, Fiscal, Contábil, TI, etc.)
- **Armazenamento**: Arquivos físicos fora da wwwroot

---

## 📋 Pré-requisitos

### Sistema Operacional

- ✅ **Windows Server 2019/2022** (recomendado)
- ✅ **Windows 10/11 Pro** (para desenvolvimento/teste)

### Software Obrigatório

- ✅ **IIS (Internet Information Services)** com recursos:
  - ASP.NET Core Module V2
  - URL Rewrite Module
  - Request Filtering
- ✅ **.NET 9.0 Hosting Bundle** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- ✅ **MySQL Server 8.0+** ou **MariaDB 10.5+**

### Hardware Recomendado

- **CPU**: 2+ cores (4+ cores recomendado)
- **RAM**: 4GB mínimo (8GB+ recomendado)
- **Disco**: 20GB+ espaço livre
- **Rede**: Conexão estável com internet

### Permissões Necessárias

- ✅ **Privilégios de Administrador** no Windows Server
- ✅ **Acesso de escrita** em `C:\inetpub\wwwroot`
- ✅ **Permissões** para criar diretórios em `C:\IntranetData`

---

## 🚀 Instalação Rápida

### Método 1: Script Automatizado (Recomendado)

**Para usuários que preferem instalação automática:**

1. **Extrair Arquivos**

   ```cmd
   # Extrair o pacote para:
   C:\Deploy\IntranetDocumentos\
   ```

2. **Executar Instalação**

   ```batch
   # Clique com botão direito → "Executar como administrador"
   deploy-quick.bat
   ```

3. **Configurar Senhas**
   - Edite `C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json`
   - Configure conexão MySQL e SMTP

4. **Verificar Instalação**

   ```powershell
   .\Verificacao-Pos-Instalacao.ps1
   ```

5. **Acessar Sistema**
   - URL: `http://seu-servidor`
   - Login: `admin@empresa.com`
   - Senha: `Admin123!`

### Método 2: PowerShell Completo

```powershell
# Execute como Administrador
.\Deploy-WindowsServer.ps1

# Configure IIS especificamente
.\Configuracao-IIS.ps1

# Verifique a instalação
.\Verificacao-Pos-Instalacao.ps1
```

---

## 🛠️ Instalação Manual Detalhada

### Passo 1: Preparar o Ambiente

#### 1.1 Instalar .NET 9.0 Hosting Bundle

```powershell
# Verificar versão atual (opcional)
dotnet --version

# Baixar e instalar .NET 9.0 Hosting Bundle
# URL: https://dotnet.microsoft.com/download/dotnet/9.0
# Arquivo: dotnet-hosting-9.0.x-win.exe

# Após instalação, reiniciar IIS
iisreset
```

#### 1.2 Habilitar IIS e Recursos

```powershell
# Habilitar IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering -All

# Verificar ASP.NET Core Module
Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
```

### Passo 2: Configurar MySQL

#### 2.1 Instalar MySQL Server

```sql
-- Baixar MySQL Server 8.0+
-- URL: https://dev.mysql.com/downloads/mysql/

-- Ou MariaDB 10.5+
-- URL: https://mariadb.org/download/

-- Configurar usuário root com senha segura
-- Habilitar conexões TCP/IP na porta 3306
```

#### 2.2 Criar Banco de Dados

```sql
-- Conectar como root
mysql -u root -p

-- Executar script de setup
source C:\Deploy\IntranetDocumentos\setup-mysql.sql
```

### Passo 3: Configurar IIS

#### 3.1 Criar Application Pool

```powershell
# Importar módulo WebAdministration
Import-Module WebAdministration

# Criar Application Pool
New-WebAppPool -Name "IntranetDocumentos" -Force

# Configurar Application Pool
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentos" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentos" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentos" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentos" -Name "processModel.idleTimeout" -Value "00:00:00"
```

#### 3.2 Criar Site IIS

```powershell
# Criar site
New-Website -Name "Intranet Documentos" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\IntranetDocumentos" -ApplicationPool "IntranetDocumentos"

# Configurar Request Filtering
Set-WebConfigurationProperty -Filter "system.webServer/security/requestFiltering/requestLimits" -Name "maxAllowedContentLength" -Value 104857600 -PSPath "IIS:\" -Location "Intranet Documentos"
```

### Passo 4: Deploy da Aplicação

#### 4.1 Criar Estrutura de Diretórios

```powershell
# Diretórios principais
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\IntranetDocumentos" -Force
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\IntranetDocumentos\logs" -Force

# Diretórios de dados
$dataPaths = @(
    "C:\IntranetData\Documents",
    "C:\IntranetData\Backups",
    "C:\IntranetData\Logs",
    "C:\IntranetData\Documents\Geral",
    "C:\IntranetData\Documents\Pessoal",
    "C:\IntranetData\Documents\Fiscal",
    "C:\IntranetData\Documents\Contabil",
    "C:\IntranetData\Documents\Cadastro",
    "C:\IntranetData\Documents\Apoio",
    "C:\IntranetData\Documents\TI"
)

foreach ($path in $dataPaths) {
    New-Item -ItemType Directory -Path $path -Force
}
```

#### 4.2 Copiar Arquivos da Aplicação

```powershell
# Compilar e publicar (na máquina de desenvolvimento)
dotnet publish IntranetDocumentos.csproj --configuration Release --output .\publish

# Copiar arquivos para o servidor
Copy-Item ".\publish\*" -Destination "C:\inetpub\wwwroot\IntranetDocumentos" -Recurse -Force
```

#### 4.3 Configurar Permissões

```powershell
# Permissões para IIS
icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "IIS_IUSRS:(OI)(CI)RX" /T
icacls "C:\inetpub\wwwroot\IntranetDocumentos\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Permissões para dados
icacls "C:\IntranetData" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Permissões para Application Pool
$appPoolIdentity = "IIS AppPool\IntranetDocumentos"
icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "${appPoolIdentity}:(OI)(CI)RX" /T
icacls "C:\IntranetData" /grant "${appPoolIdentity}:(OI)(CI)F" /T
```

---

## ⚙️ Configuração de Produção

### Configurar appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=app_user;Password=SUA_SENHA_SEGURA;Port=3306;SslMode=Preferred;CharSet=utf8mb4;ConnectionTimeout=30;CommandTimeout=120;"
  },
  "AdminUser": {
    "Email": "admin@sua-empresa.com",
    "Password": "SuaSenhaAdmin123!"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.sua-empresa.com",
    "SmtpPort": 587,
    "SmtpUsername": "intranet@sua-empresa.com",
    "SmtpPassword": "SuaSenhaEmail123!",
    "EnableSsl": true,
    "FromName": "Intranet Documentos",
    "FromEmail": "intranet@sua-empresa.com"
  },
  "DocumentSettings": {
    "StoragePath": "C:\\IntranetData\\Documents",
    "MaxFileSize": 104857600,
    "BackupPath": "C:\\IntranetData\\Backups",
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"]
  },
  "BackupSettings": {
    "AutoBackupEnabled": true,
    "BackupIntervalHours": 24,
    "MaxBackupFiles": 30,
    "BackupPath": "C:\\IntranetData\\Backups\\Auto"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "EventLog": {
      "LogLevel": {
        "Default": "Information"
      },
      "SourceName": "IntranetDocumentos"
    }
  },
  "AllowedHosts": "*"
}
```

### Configurar web.config

O arquivo `web.config` já está otimizado com:

- ASP.NET Core Module V2
- Headers de segurança
- Configurações de upload (100MB)
- Redirecionamento HTTPS
- Compressão e cache

---

## 🌐 Deploy Remoto

### Script de Publicação Remota

```powershell
# Sintaxe completa
.\Publish-ToWindowsServer.ps1 -TargetServer "SEU-SERVIDOR" -TargetPath "C:\inetpub\wwwroot\IntranetDocumentos" -Credential $cred

# Exemplo prático
$credential = Get-Credential
.\Publish-ToWindowsServer.ps1 -TargetServer "192.168.1.100" -Credential $credential -RestartSite
```

### Deploy via CI/CD

Para integração com Azure DevOps ou GitHub Actions:

```yaml
# Exemplo Azure DevOps Pipeline
- task: DotNetCoreCLI@2
  displayName: 'Publish Application'
  inputs:
    command: 'publish'
    publishWebProjects: true
    arguments: '--configuration Release --output $(Build.ArtifactStagingDirectory)'

- task: PowerShell@2
  displayName: 'Deploy to Windows Server'
  inputs:
    filePath: 'Deploy-WindowsServer.ps1'
    arguments: '-TargetServer $(TargetServer) -MySqlPassword $(MySqlPassword)'
```

---

## 🔍 Verificação e Diagnóstico

### Script de Verificação Automática

```powershell
# Executar verificação completa
.\Verificacao-Pos-Instalacao.ps1

# Verificação com parâmetros customizados
.\Verificacao-Pos-Instalacao.ps1 -SiteUrl "https://intranet.empresa.com" -SitePath "D:\Sites\Intranet"
```

### Verificações Manuais

#### 1. Verificar .NET Runtime

```powershell
dotnet --version
# Deve retornar: 9.x.x
```

#### 2. Verificar IIS e Application Pool

```powershell
Get-Website | Where-Object { $_.Name -eq "Intranet Documentos" }
Get-IISAppPool | Where-Object { $_.Name -eq "IntranetDocumentos" }
```

#### 3. Testar Conectividade HTTP

```powershell
Invoke-WebRequest -Uri "http://localhost" -Method HEAD
```

#### 4. Verificar Logs

```powershell
# Event Viewer
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IntranetDocumentos'} -MaxEvents 10

# Logs da aplicação
Get-Content "C:\IntranetData\Logs\app-*.log" | Select-Object -Last 20
```

---

## 🔧 Configurações Avançadas

### HTTPS e Certificados SSL

#### Certificado Auto-assinado (Desenvolvimento)

```powershell
# Criar certificado auto-assinado
$cert = New-SelfSignedCertificate -DnsName "intranet.empresa.com" -CertStoreLocation "cert:\LocalMachine\My"

# Configurar HTTPS no IIS
.\Configuracao-IIS.ps1 -CertificateThumbprint $cert.Thumbprint -HttpsPort 443
```

#### Certificado Comercial

```powershell
# Importar certificado comercial
Import-PfxCertificate -FilePath "certificado.pfx" -CertStoreLocation "cert:\LocalMachine\My" -Password $securePassword

# Configurar binding HTTPS
New-WebBinding -Name "Intranet Documentos" -IP "*" -Port 443 -Protocol https
New-Item -Path "IIS:\SslBindings\0.0.0.0!443" -Value $cert -Force
```

### Configurar Firewall

```powershell
# Permitir HTTP/HTTPS
New-NetFirewallRule -DisplayName "Intranet HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "Intranet HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Permitir MySQL (se necessário)
New-NetFirewallRule -DisplayName "MySQL" -Direction Inbound -Protocol TCP -LocalPort 3306 -Action Allow
```

### Configurar Load Balancer

Para ambientes de alta disponibilidade:

```xml
<!-- web.config para load balancer -->
<system.webServer>
  <rewrite>
    <rules>
      <rule name="Force HTTPS" stopProcessing="true">
        <match url=".*" />
        <conditions>
          <add input="{HTTP_X_FORWARDED_PROTO}" pattern="^http$" />
        </conditions>
        <action type="Redirect" url="https://{HTTP_HOST}/{R:0}" redirectType="Permanent" />
      </rule>
    </rules>
  </rewrite>
</system.webServer>
```

---

## 🐛 Solução de Problemas

### Problemas Comuns e Soluções

#### Erro 500.30 - ASP.NET Core app failed to start

**Sintomas:**

- Página em branco ou erro 500.30
- Site não carrega

**Soluções:**

```powershell
# 1. Verificar .NET Runtime
dotnet --version

# 2. Verificar Application Pool
Get-IISAppPool -Name "IntranetDocumentos"
Restart-WebAppPool -Name "IntranetDocumentos"

# 3. Verificar logs
Get-WinEvent -FilterHashtable @{LogName='Application'; Level=2} -MaxEvents 5

# 4. Verificar permissões
icacls "C:\inetpub\wwwroot\IntranetDocumentos" /verify
```

#### Erro de Conexão com Banco de Dados

**Sintomas:**

- "Cannot connect to MySQL server"
- Timeout de conexão

**Soluções:**

```sql
-- 1. Verificar se MySQL está rodando
services.msc # Procurar MySQL80

-- 2. Testar conexão
mysql -u app_user -p -h localhost

-- 3. Verificar firewall
telnet localhost 3306

-- 4. Verificar usuário e permissões
SELECT User, Host FROM mysql.user WHERE User = 'app_user';
SHOW GRANTS FOR 'app_user'@'localhost';
```

#### Upload de Arquivos Falha

**Sintomas:**

- Erro ao fazer upload
- "Request entity too large"

**Soluções:**

```powershell
# 1. Verificar permissões
icacls "C:\IntranetData\Documents" /verify

# 2. Verificar tamanho máximo no IIS
Get-WebConfigurationProperty -Filter "system.webServer/security/requestFiltering/requestLimits" -Name "maxAllowedContentLength" -PSPath "IIS:\"

# 3. Verificar espaço em disco
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, FreeSpace, Size
```

#### Performance Lenta

**Sintomas:**

- Site carrega lentamente
- Timeouts frequentes

**Soluções:**

```powershell
# 1. Verificar uso de recursos
Get-Process -Name "w3wp" | Select-Object CPU, WorkingSet

# 2. Verificar logs de erro
Get-WinEvent -FilterHashtable @{LogName='Application'; Level=3} -MaxEvents 10

# 3. Otimizar Application Pool
Set-ItemProperty -Path "IIS:\AppPools\IntranetDocumentos" -Name "recycling.periodicRestart.memory" -Value 1048576

# 4. Verificar antivírus
# Excluir diretórios da aplicação do scan em tempo real
```

### Logs e Diagnóstico

#### Localização dos Logs

```powershell
# Logs do Windows Event Viewer
# Applications and Services Logs > IntranetDocumentos

# Logs da aplicação
C:\IntranetData\Logs\app-YYYYMMDD.log

# Logs do IIS
C:\inetpub\logs\LogFiles\W3SVC1\

# Logs de erro do ASP.NET Core
C:\inetpub\wwwroot\IntranetDocumentos\logs\
```

#### Comandos Úteis para Diagnóstico

```powershell
# Status geral
Get-Website | Where-Object { $_.Name -eq "Intranet Documentos" }
Get-IISAppPool | Where-Object { $_.Name -eq "IntranetDocumentos" }

# Processos relacionados
Get-Process -Name "w3wp" | Where-Object { $_.ProcessName -eq "w3wp" }

# Verificar portas
netstat -an | findstr :80
netstat -an | findstr :443

# Testar DNS/conectividade
nslookup intranet.empresa.com
Test-NetConnection -ComputerName localhost -Port 80

# Verificar certificados SSL
Get-ChildItem -Path "cert:\LocalMachine\My" | Where-Object { $_.Subject -like "*intranet*" }
```

---

## 🔄 Manutenção e Backup

### Backup Automático

O sistema inclui backup automático configurado por padrão:

```json
"BackupSettings": {
  "AutoBackupEnabled": true,
  "BackupIntervalHours": 24,
  "MaxBackupFiles": 30,
  "BackupPath": "C:\\IntranetData\\Backups\\Auto"
}
```

### Backup Manual

```powershell
# Backup do banco de dados
mysqldump -u app_user -p IntranetDocumentos > backup_$(Get-Date -Format 'yyyyMMdd').sql

# Backup dos arquivos
Compress-Archive -Path "C:\IntranetData\Documents" -DestinationPath "C:\IntranetData\Backups\documents_$(Get-Date -Format 'yyyyMMdd').zip"
```

### Manutenção Regular

#### Limpeza de Logs

```powershell
# Limpar logs antigos (manter últimos 30 dias)
Get-ChildItem "C:\IntranetData\Logs\*.log" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item

# Limpar backups antigos (manter últimos 30)
Get-ChildItem "C:\IntranetData\Backups\*.zip" | Sort-Object CreationTime -Descending | Select-Object -Skip 30 | Remove-Item
```

#### Monitoramento de Performance

```powershell
# Script de monitoramento
Get-Process -Name "w3wp" | Select-Object @{Name="CPU%";Expression={$_.CPU}}, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}}

# Verificar espaço em disco
Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DriveType -eq 3 } | Select-Object DeviceID, @{Name="Size(GB)";Expression={[math]::Round($_.Size/1GB,2)}}, @{Name="FreeSpace(GB)";Expression={[math]::Round($_.FreeSpace/1GB,2)}}
```

#### Atualizações de Segurança

```powershell
# Verificar atualizações do Windows
Get-WindowsUpdate

# Verificar atualizações .NET
# Consultar: https://dotnet.microsoft.com/download/dotnet/9.0

# Atualizar certificados SSL (quando necessário)
# Renovar certificados antes do vencimento
```

---

## 📦 Arquivos de Instalação

### Scripts PowerShell (.ps1)

| Arquivo | Descrição | Uso |
|---------|-----------|-----|
| `Deploy-WindowsServer.ps1` | Script principal de deploy automático | Deploy inicial completo |
| `Configuracao-IIS.ps1` | Configuração específica do IIS e Application Pool | Configuração de IIS |
| `Publish-ToWindowsServer.ps1` | Script para publicação remota | Deploy remoto |
| `Verificacao-Pos-Instalacao.ps1` | Verificação e diagnóstico pós-instalação | Verificação de status |

### Scripts Batch (.bat)

| Arquivo | Descrição | Uso |
|---------|-----------|-----|
| `deploy-quick.bat` | Instalação rápida automatizada | Instalação simples |

### Configuração

| Arquivo | Descrição | Configuração |
|---------|-----------|--------------|
| `web.config` | Configuração do IIS para ASP.NET Core | Headers segurança, uploads, HTTPS |
| `appsettings.Production.json` | Configurações de produção | Banco, email, paths, logging |
| `setup-mysql.sql` | Script SQL para configurar banco MySQL | Criação de banco e usuário |

### Documentação

| Arquivo | Descrição |
|---------|-----------|
| `GUIA-COMPLETO.md` | Este guia completo unificado |
| `README.md` | Documentação principal do projeto |
| `STATUS-FINAL.md` | Status atual e problemas corrigidos |

### Projeto

| Arquivo | Descrição |
|---------|-----------|
| `IntranetDocumentos.csproj` | Dependências atualizadas para .NET 9 |
| `Program.cs` | Configurações de produção e logging |

---

## ✅ Status e Checklist

### Estado Atual do Projeto

#### ✅ Problemas Corrigidos

- **Warnings de Compilação**: CA1416 EventLog Windows (suprimido)
- **Dependências**: Atualizadas para .NET 9 compatível
- **Markdown Lint**: MD036 corrigido em documentação
- **Scripts PowerShell**: Sintaxe validada, variáveis não utilizadas removidas

#### ✅ Melhorias Implementadas

- **Scripts de Deploy**: Automatização completa
- **Configurações de Segurança**: Headers, HTTPS, CORS
- **Logging**: Event Viewer Windows + arquivos
- **Estrutura de Diretórios**: Criação automática
- **Validação**: Configuração e dependências

#### ✅ Testes Realizados

- **Build Debug/Release**: Sucesso sem warnings
- **Publicação**: Funcionando corretamente
- **Scripts PowerShell**: Sintaxe válida
- **Estrutura de Arquivos**: Completa

### Checklist de Instalação

#### Pré-Instalação

- [ ] Windows Server 2019/2022 disponível
- [ ] Privilégios de Administrador
- [ ] Internet disponível para downloads
- [ ] MySQL Server instalado (opcional se usar script)

#### Instalação

- [ ] Executado `deploy-quick.bat` como Admin
- [ ] OU executado scripts PowerShell manualmente
- [ ] Configurado `appsettings.Production.json`
- [ ] Configurado banco MySQL
- [ ] Verificado com `Verificacao-Pos-Instalacao.ps1`

#### Pós-Instalação

- [ ] Site acessível via navegador
- [ ] Login administrativo funcionando
- [ ] Upload de documentos testado
- [ ] Download de documentos testado
- [ ] Emails sendo enviados (se configurado)
- [ ] HTTPS configurado (recomendado)
- [ ] Backup automático funcionando
- [ ] Senhas padrão alteradas
- [ ] Usuários e departamentos criados

#### Configuração Avançada

- [ ] Firewall configurado
- [ ] Certificado SSL válido instalado
- [ ] Monitoramento configurado
- [ ] Backup manual testado
- [ ] Procedimentos de manutenção documentados

### Comandos de Verificação Rápida

```powershell
# Verificação completa automatizada
.\Verificacao-Pos-Instalacao.ps1

# Verificações manuais básicas
Get-Website | Where-Object { $_.Name -eq "Intranet Documentos" }
Get-IISAppPool | Where-Object { $_.Name -eq "IntranetDocumentos" }
Test-NetConnection -ComputerName localhost -Port 80

# Verificar aplicação
Invoke-WebRequest -Uri "http://localhost" -Method HEAD

# Verificar logs recentes
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IntranetDocumentos'} -MaxEvents 5
```

---

## 🎯 Conclusão

A **Intranet Documentos** está **100% pronta para instalação em Windows Server**. Este guia fornece:

### ✅ Cobertura Completa

- **Instalação Rápida**: Para usuários que querem simplicidade
- **Instalação Manual**: Para usuários que precisam de controle total
- **Deploy Remoto**: Para ambientes corporativos
- **Configuração Avançada**: Para cenários complexos
- **Solução de Problemas**: Para resolução de issues
- **Manutenção**: Para operação contínua

### ✅ Scripts Automatizados

- Deploy completo em um comando
- Verificação automática de todos os componentes
- Configuração de IIS otimizada
- Estrutura de diretórios automática

### ✅ Documentação Completa

- Todos os cenários cobertos
- Exemplos práticos
- Solução de problemas detalhada
- Comandos prontos para uso

### 🚀 Próximos Passos

1. **Baixar** todos os arquivos para o Windows Server
2. **Executar** `deploy-quick.bat` como Administrador
3. **Configurar** senhas em `appsettings.Production.json`
4. **Verificar** com `Verificacao-Pos-Instalacao.ps1`
5. **Acessar** a aplicação e começar a usar!

---

**Desenvolvido com ASP.NET Core 9.0 + MySQL + Bootstrap 5**  
*Sistema pronto para ambiente corporativo de produção*

📞 **Suporte**: Consulte os logs e use os scripts de diagnóstico inclusos  
📚 **Documentação**: Todos os recursos documentados neste guia  
🔧 **Manutenção**: Scripts automatizados para operação contínua  

**🎉 Instalação Garantida com Sucesso!**
