# 📚 DOCUMENTAÇÃO UNIFICADA - Intranet Documentos

> **Sistema de Gestão de Documentos Corporativos**  
> **Versão:** 2.0 (com Redis, Segurança Enterprise e Busca Avançada)  
> **Data:** 16 de Julho de 2025  
> **Status:** ✅ Produção Ready

---

## 🎯 **ÍNDICE GERAL DA DOCUMENTAÇÃO**

### **📖 1. INFORMAÇÕES GERAIS**

- [1.1 Visão Geral do Sistema](#11-visão-geral-do-sistema)
- [1.2 Arquitetura e Tecnologias](#12-arquitetura-e-tecnologias)
- [1.3 Funcionalidades Principais](#13-funcionalidades-principais)
- [1.4 Requisitos do Sistema](#14-requisitos-do-sistema)

### **🚀 2. INSTALAÇÃO E DEPLOY**

- [2.1 Instalação Rápida (Desenvolvimento)](#21-instalação-rápida-desenvolvimento)
- [2.2 Deploy Windows Server (Produção)](#22-deploy-windows-server-produção)
- [2.3 Configuração MySQL](#23-configuração-mysql)
- [2.4 Configuração Redis](#24-configuração-redis)
- [2.5 Configuração IIS](#25-configuração-iis)

### **🔧 3. CONFIGURAÇÃO E ADMINISTRAÇÃO**

- [3.1 Configurações da Aplicação](#31-configurações-da-aplicação)
- [3.2 Usuários e Permissões](#32-usuários-e-permissões)
- [3.3 Departamentos](#33-departamentos)
- [3.4 Backup e Restore](#34-backup-e-restore)

---

**📋 Para acessar as demais seções da documentação, consulte:**

- DOCUMENTACAO-UNIFICADA-PARTE2.md (Seções 4-6: Segurança, Performance, Funcionalidades)
- DOCUMENTACAO-UNIFICADA-PARTE3.md (Seções 7-9: Desenvolvimento, Troubleshooting, Changelog)

---

## **1.1 Visão Geral do Sistema**

### **🎯 Objetivo**

Sistema web para gestão centralizada de documentos corporativos, reuniões e ramais telefônicos, desenvolvido em ASP.NET Core MVC com foco em segurança, performance e escalabilidade.

### **👥 Público-Alvo**

- **Funcionários**: Upload, download e busca de documentos
- **Gestores**: Acesso amplo e gestão de reuniões
- **Administradores**: Gestão completa do sistema

### **🏢 Ambiente Corporativo**

- **Departamentos**: Pessoal, Fiscal, Contábil, Cadastro, Apoio, TI
- **Área Geral**: Documentos acessíveis por todos
- **Controle de Acesso**: Baseado em departamento e role

---

## **1.2 Arquitetura e Tecnologias**

### **🏗️ Stack Tecnológico**

```text
Frontend:     Bootstrap 5 + Razor Views + JavaScript
Backend:      ASP.NET Core 9.0 MVC
Database:     MySQL 8.0+
Cache:        Redis 6.0+
Web Server:   IIS (Produção) / Kestrel (Desenvolvimento)
```text

### **📁 Arquitetura de Camadas**

```text
┌─────────────────────┐
│   Presentation      │ Controllers + Views (MVC)
├─────────────────────┤
│   Business Logic    │ Services + Validators
├─────────────────────┤
│   Data Access       │ Entity Framework Core
├─────────────────────┤
│   Infrastructure    │ MySQL + Redis + File System
└─────────────────────┘
```text

### **🔧 Padrões Implementados**

- **Repository Pattern**: Abstração de acesso a dados
- **Service Layer**: Lógica de negócio centralizada
- **Dependency Injection**: Injeção de dependências nativa
- **Builder Pattern**: Construção de ViewModels complexos

---

## **1.3 Funcionalidades Principais**

### **📄 Gestão de Documentos**

- ✅ Upload de arquivos (PDF, Office, imagens, etc.)
- ✅ Download seguro com controle de acesso
- ✅ Busca avançada com filtros múltiplos
- ✅ Organização por departamentos
- ✅ Versionamento e histórico
- ✅ Workflow de aprovação

### **🔍 Busca Avançada**

- ✅ Busca por termo (nome/conteúdo)
- ✅ Filtro por departamento
- ✅ Filtro por tipo de arquivo
- ✅ Filtro por período de data
- ✅ Resultados paginados

### **📅 Sistema de Reuniões**

- ✅ Agendamento de reuniões
- ✅ Tipos: Ordinária, Extraordinária, Emergencial
- ✅ Notificações por email
- ✅ Controle de participantes
- ✅ Relatórios e analytics

### **📞 Ramais Telefônicos**

- ✅ Catálogo de ramais por departamento
- ✅ Busca rápida de contatos
- ✅ Gestão centralizada
- ✅ Interface responsiva

### **📊 Analytics e Relatórios**

- ✅ Dashboard executivo
- ✅ Estatísticas de documentos
- ✅ Métricas de reuniões
- ✅ Atividade por departamento
- ✅ Usuários mais ativos

---

## **1.4 Requisitos do Sistema**

### **💻 Servidor Windows (Produção)**

```text
Sistema:      Windows Server 2019/2022
RAM:          Mínimo 8GB (Recomendado 16GB)
Disco:        100GB+ SSD
Processador:  4+ cores
Rede:         1Gbps
```text

### **🗄️ Banco de Dados MySQL**

```text
Versão:       MySQL 8.0+
RAM:          Mínimo 2GB
Conexões:     100+ simultâneas
InnoDB:       Buffer Pool 128MB+
```text

### **🔴 Redis Cache**

```text
Versão:       Redis 6.0+
RAM:          Mínimo 512MB
Persistência: RDB + AOF
Política:     allkeys-lru
```text

### **🌐 IIS (Produção)**

```text
Versão:       IIS 10+
.NET:         ASP.NET Core Runtime 9.0
Módulos:      ASP.NET Core Module v2
SSL:          Certificado válido
```text

---

## **2.1 Instalação Rápida (Desenvolvimento)**

### **🔧 Pré-requisitos**

```bash
# 1. Instalar .NET 9.0 SDK
dotnet --version  # Verificar versão

# 2. Instalar MySQL
mysql --version   # Verificar instalação

# 3. Instalar Redis (opcional para desenvolvimento)
redis-server --version
```text

### **⚡ Setup Rápido**

```bash
# 1. Clonar repositório
git clone https://github.com/Vollupios/mc_web_app.git
cd mc_web_app

# 2. Configurar banco de dados
mysql -u root -p < setup-database.mysql.sql

# 3. Configurar connection string
# Editar appsettings.json com credenciais do MySQL

# 4. Executar migrations
dotnet ef database update

# 5. Iniciar aplicação
dotnet run --project IntranetDocumentos.csproj
```text

### **🌐 Acesso Padrão**

```text
URL:      http://localhost:5000
Admin:    admin@intranet.com
Senha:    Admin@123
```text

---

## **2.2 Deploy Windows Server (Produção)**

### **📦 Scripts de Deploy Automatizado**

```powershell
# 1. Executar script principal de deploy
.\Deploy-WindowsServer.ps1

# 2. Configurar IIS
.\Configuracao-IIS.ps1

# 3. Publicar aplicação
.\Publish-ToWindowsServer.ps1

# 4. Verificar instalação
.\Verificacao-Pos-Instalacao.ps1
```text

### **🔧 Deploy Manual**

```powershell
# 1. Publicar aplicação
dotnet publish -c Release -o C:\inetpub\wwwroot\IntranetDocumentos

# 2. Configurar IIS
# - Criar Application Pool (.NET Core)
# - Criar Website apontando para pasta publicada
# - Configurar bindings (HTTP/HTTPS)

# 3. Configurar permissões
icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "IIS_IUSRS:F"
```text

---

## **2.3 Configuração MySQL**

### **📄 Arquivo de Setup**

Use o arquivo `setup-database.mysql.sql`:

```sql
-- Executar como root
mysql -u root -p < setup-database.mysql.sql
```text

### **🔧 Configuração Manual**

```sql
-- 1. Criar banco de dados
CREATE DATABASE `IntranetDocumentos` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- 2. Criar usuário da aplicação
CREATE USER `app_user`@`localhost` IDENTIFIED BY 'SuaSenhaSegura123!';

-- 3. Conceder permissões
GRANT ALL PRIVILEGES ON `IntranetDocumentos`.* TO `app_user`@`localhost`;
FLUSH PRIVILEGES;
```text

### **⚙️ Configurações de Produção**

```sql
-- Otimizações para produção
SET GLOBAL max_allowed_packet = 52428800; -- 50MB
SET GLOBAL innodb_buffer_pool_size = 134217728; -- 128MB
SET GLOBAL innodb_log_file_size = 67108864; -- 64MB
```text

---

## **2.4 Configuração Redis**

### **🔧 Instalação Windows Server**

```powershell
# Executar script automatizado
.\Install-Redis-Windows.ps1

# Ou instalação manual:
# 1. Download Redis for Windows
# 2. Instalar como serviço
# 3. Configurar redis.conf
# 4. Iniciar serviço
```text

### **⚙️ Configuração de Produção**

```conf
# redis.conf para produção
maxmemory 512mb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000
appendonly yes
```text

### **🧪 Teste de Funcionamento**

```bash
# Verificar conexão
redis-cli ping
# Resultado esperado: PONG

# Ver chaves da aplicação
redis-cli keys "*"

# Monitorar comandos
redis-cli monitor
```text

---

## **2.5 Configuração IIS**

### **🔧 Configuração Automática**

```powershell
# Script completo de configuração IIS
.\Configuracao-IIS.ps1
```text

### **📝 Configuração Manual**

#### **1. Criar Application Pool**

```powershell
# Via PowerShell
New-WebAppPool -Name "IntranetDocumentos" -Force
Set-ItemProperty IIS:\AppPools\IntranetDocumentos processModel.identityType ApplicationPoolIdentity
Set-ItemProperty IIS:\AppPools\IntranetDocumentos managedRuntimeVersion ""
```text

#### **2. Criar Website**

```powershell
# Via PowerShell
New-Website -Name "IntranetDocumentos" -ApplicationPool "IntranetDocumentos" -PhysicalPath "C:\inetpub\wwwroot\IntranetDocumentos" -Port 80
```text

#### **3. Configurar HTTPS**

```powershell
# Adicionar binding HTTPS
New-WebBinding -Name "IntranetDocumentos" -Protocol https -Port 443 -SslFlags 1
```text

### **🔒 Headers de Segurança (web.config)**

```xml
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <add name="X-Frame-Options" value="DENY" />
      <add name="X-XSS-Protection" value="1; mode=block" />
      <add name="X-Content-Type-Options" value="nosniff" />
      <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
      <add name="Permissions-Policy" value="geolocation=(), microphone=(), camera=()" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```text

---

## **3.1 Configurações da Aplicação**

### **📄 appsettings.json (Desenvolvimento)**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=SuaSenha;",
    "Redis": "localhost:6379"
  },
  "Redis": {
    "InstanceName": "IntranetDocumentos",
    "Database": 0
  },
  "RateLimiting": {
    "LoginAttempts": {
      "MaxAttempts": 5,
      "WindowMinutes": 15,
      "LockoutMinutes": 30
    },
    "UploadAttempts": {
      "MaxAttempts": 20,
      "WindowMinutes": 60
    }
  }
}
```text

### **📄 appsettings.Production.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=PRODUCTION_PASSWORD;",
    "Redis": "localhost:6379"
  },
  "Redis": {
    "InstanceName": "IntranetDocumentos_Prod",
    "Database": 0,
    "ConnectTimeout": 10000,
    "SyncTimeout": 10000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```text

### **⚙️ Configurações Avançadas**

```json
{
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".png"],
    "StoragePath": "DocumentsStorage"
  },
  "Email": {
    "SmtpServer": "smtp.empresa.com",
    "SmtpPort": 587,
    "UseSSL": true,
    "FromEmail": "noreply@empresa.com"
  },
  "Security": {
    "RequireHttps": true,
    "CookieSecure": true,
    "EnableAuditLogging": true
  }
}
```text

---

## **3.2 Usuários e Permissões**

### **👥 Tipos de Usuário**

#### **🔑 Admin (Administrador)**

```text
Acesso: Total ao sistema
Permissões:
- ✅ Gestão de usuários
- ✅ Todos os departamentos
- ✅ Configurações do sistema
- ✅ Logs e auditoria
- ✅ Backup e restore
```text

#### **👔 Gestor**

```text
Acesso: Amplo (exceto administração)
Permissões:
- ✅ Todos os documentos
- ✅ Todas as reuniões
- ✅ Analytics completo
- ❌ Gestão de usuários
- ❌ Configurações do sistema
```text

#### **👤 Usuario (Funcionário)**

```text
Acesso: Departamento próprio + Geral
Permissões:
- ✅ Documentos do seu departamento
- ✅ Documentos da área Geral
- ✅ Reuniões que participa
- ❌ Outros departamentos
- ❌ Administração
```text

### **🏢 Departamentos**

```text
1. Pessoal       - Documentos de RH e pessoal
2. Fiscal        - Documentos fiscais e tributários
3. Contábil      - Documentos contábeis e financeiros
4. Cadastro      - Documentos de cadastros diversos
5. Apoio         - Documentos de apoio administrativo
6. TI            - Documentos técnicos e de TI
7. Geral         - Documentos acessíveis por todos
```text

### **🔧 Criação de Usuários**

```sql
-- Via interface administrativa ou SQL direto
INSERT INTO AspNetUsers (UserName, Email, DepartmentId, EmailConfirmed)
VALUES ('usuario@empresa.com', 'usuario@empresa.com', 1, 1);

-- Adicionar role
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id 
FROM AspNetUsers u, AspNetRoles r 
WHERE u.Email = 'usuario@empresa.com' AND r.Name = 'Usuario';
```text

---

## **3.3 Departamentos**

### **📊 Estrutura de Departamentos**

```sql
-- Departamentos padrão
INSERT INTO Departments (Name) VALUES 
('Pessoal'),
('Fiscal'), 
('Contábil'),
('Cadastro'),
('Apoio'),
('TI'),
('Geral');
```text

### **🔒 Controle de Acesso por Departamento**

```csharp
// Regras de negócio
public bool CanUserAccessDocument(Document doc, ApplicationUser user)
{
    // Admin: acesso total
    if (user.IsInRole("Admin")) return true;
    
    // Gestor: acesso total
    if (user.IsInRole("Gestor")) return true;
    
    // Usuario: apenas seu departamento + Geral
    if (doc.DepartmentId == null) return true; // Geral
    if (doc.DepartmentId == user.DepartmentId) return true;
    
    return false;
}
```text

### **📁 Organização de Arquivos**

```text
DocumentsStorage/
├── Pessoal/
├── Fiscal/
├── Contabil/
├── Cadastro/
├── Apoio/
├── TI/
└── Geral/
```text

---

## **3.4 Backup e Restore**

### **🔄 Backup Automático**

```powershell
# Script de backup automático
.\backup-database.ps1

# Configuração no agendador de tarefas do Windows
schtasks /create /tn "Backup IntranetDocumentos" /tr "C:\Path\backup-database.ps1" /sc daily /st 02:00
```text

### **💾 Backup Manual**

```bash
# Backup MySQL
mysqldump -u app_user -p IntranetDocumentos > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup Redis (se usado)
redis-cli BGSAVE

# Backup arquivos
tar -czf DocumentsStorage_backup.tar.gz DocumentsStorage/
```text

### **🔄 Restore**

```bash
# Restore MySQL
mysql -u app_user -p IntranetDocumentos < backup_20250716_120000.sql

# Restore arquivos
tar -xzf DocumentsStorage_backup.tar.gz
```text

### **☁️ Backup em Nuvem (Opcional)**

```powershell
# Upload para Azure Blob Storage
az storage blob upload-batch --destination backups --source ./DatabaseBackups/ --account-name storageaccount

# Upload para AWS S3
aws s3 sync ./DatabaseBackups/ s3://bucket-backups/intranet/
```text

---

*Continua na parte 2...*
