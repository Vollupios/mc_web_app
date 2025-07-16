# 📚 DOCUMENTAÇÃO OFICIAL - Intranet Documentos

> **Sistema de Gestão de Documentos Corporativos**  
> **Versão:** 2.0 Production Ready  
> **Data:** 16 de Julho de 2025  
> **Status:** ✅ Totalmente Funcional

---

## 🎯 **NAVEGAÇÃO RÁPIDA**

| Seção | Descrição | Para quem |
|-------|-----------|-----------|
| **[🚀 Instalação Rápida](#-instalação-rápida)** | Setup em minutos | Administradores |
| **[📖 Visão Geral](#-visão-geral-do-sistema)** | O que é o sistema | Todos |
| **[🔧 Instalação Completa](#-instalação-e-deploy)** | Guia detalhado | DevOps/TI |
| **[🔒 Segurança](#-segurança)** | Configurações de segurança | Administradores |
| **[📊 Funcionalidades](#-funcionalidades)** | Recursos disponíveis | Usuários finais |
| **[🛠️ Desenvolvimento](#-desenvolvimento)** | Informações técnicas | Desenvolvedores |
| **[🆘 Troubleshooting](#-troubleshooting)** | Solução de problemas | Suporte técnico |

---

## 🚀 **INSTALAÇÃO RÁPIDA**

### **Para Administradores - Setup em 5 minutos**

#### **Windows Server (Produção)**
```batch
# 1. Execute como Administrador
deploy-quick.bat

# 2. Configure banco de dados
mysql -u root -p < setup-database.mysql.sql

# 3. Acesse a aplicação
# http://seu-servidor/
# Login: admin@intranet.com
# Senha: Admin@123
```

#### **Desenvolvimento Local**
```bash
# 1. Clone o repositório
git clone https://github.com/Vollupios/mc_web_app.git
cd mc_web_app

# 2. Execute a aplicação
dotnet run --project IntranetDocumentos.csproj

# 3. Acesse: http://localhost:5000
```

### **✅ Verificação Rápida**
- ✅ Aplicação carrega sem erros
- ✅ Login funciona
- ✅ Upload de documento funciona
- ✅ Busca avançada acessível

---

## 📖 **VISÃO GERAL DO SISTEMA**

### **🎯 O que é o Intranet Documentos?**

Sistema web corporativo para gestão centralizada de documentos, reuniões e ramais telefônicos, desenvolvido em ASP.NET Core com foco em **segurança**, **performance** e **facilidade de uso**.

### **👥 Quem usa o sistema?**

| Perfil | Acesso | Funcionalidades |
|--------|--------|-----------------|
| **👤 Funcionários** | Seu departamento + Geral | Upload, download, busca de documentos |
| **👔 Gestores** | Todos os departamentos | Gestão de reuniões, relatórios |
| **🔑 Administradores** | Total | Usuários, configurações, segurança |

### **🏢 Departamentos Suportados**
- **Pessoal** - Documentos de RH
- **Fiscal** - Documentos fiscais e tributários  
- **Contábil** - Documentos contábeis e financeiros
- **Cadastro** - Cadastros diversos
- **Apoio** - Apoio administrativo
- **TI** - Documentos técnicos
- **Geral** - Documentos acessíveis por todos

### **🔧 Tecnologias Utilizadas**
```
Frontend:     Bootstrap 5 + JavaScript + Razor Views
Backend:      ASP.NET Core 9.0 MVC
Database:     MySQL 8.0+
Cache:        Redis 6.0+ (opcional)
Web Server:   IIS (Produção) / Kestrel (Dev)
```

---

## 🔧 **INSTALAÇÃO E DEPLOY**

### **💻 Requisitos do Sistema**

#### **Windows Server (Produção)**
- **OS:** Windows Server 2019/2022
- **RAM:** 8GB mínimo (16GB recomendado)
- **Disco:** 100GB+ SSD
- **Processador:** 4+ cores
- **.NET:** ASP.NET Core Runtime 9.0

#### **MySQL Database**
- **Versão:** MySQL 8.0+
- **RAM:** 2GB mínimo
- **Conexões:** 100+ simultâneas

#### **Redis Cache (Opcional)**
- **Versão:** Redis 6.0+
- **RAM:** 512MB mínimo
- **Política:** allkeys-lru

### **🚀 Deploy Automatizado**

#### **1. Scripts de Deploy Prontos**
```powershell
# Deploy completo no Windows Server
.\Deploy-WindowsServer.ps1

# Configuração IIS
.\Configuracao-IIS.ps1

# Instalação Redis
.\Install-Redis-Windows.ps1

# Verificação pós-instalação
.\Verificacao-Pos-Instalacao.ps1
```

#### **2. Configuração MySQL**
```sql
-- Execute o script de setup
mysql -u root -p < setup-database.mysql.sql

-- Ou configuração manual:
CREATE DATABASE `IntranetDocumentos` CHARACTER SET utf8mb4;
CREATE USER `app_user`@`localhost` IDENTIFIED BY 'SuaSenhaSegura123!';
GRANT ALL PRIVILEGES ON `IntranetDocumentos`.* TO `app_user`@`localhost`;
FLUSH PRIVILEGES;
```

#### **3. Configuração da Aplicação**
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=SuaSenha;",
    "Redis": "localhost:6379"
  }
}
```

### **🌐 Configuração IIS**

#### **Application Pool**
```powershell
# Criar Application Pool
New-WebAppPool -Name "IntranetDocumentos"
Set-ItemProperty IIS:\AppPools\IntranetDocumentos managedRuntimeVersion ""
```

#### **Website**
```powershell
# Criar Website
New-Website -Name "IntranetDocumentos" -ApplicationPool "IntranetDocumentos" -PhysicalPath "C:\inetpub\wwwroot\IntranetDocumentos" -Port 80
```

#### **HTTPS (Recomendado)**
```powershell
# Configurar HTTPS
New-WebBinding -Name "IntranetDocumentos" -Protocol https -Port 443 -SslFlags 1
```

---

## 🔒 **SEGURANÇA**

### **🛡️ Recursos de Segurança Implementados**

#### **Autenticação e Autorização**
- ✅ **ASP.NET Core Identity** com senhas robustas
- ✅ **Role-based access** (Admin, Gestor, Usuario)
- ✅ **Department-based permissions**
- ✅ **Session management** seguro

#### **Proteção contra Ataques**
- ✅ **CSRF Protection** - Anti-forgery tokens
- ✅ **XSS Protection** - Encoding automático
- ✅ **SQL Injection** - Entity Framework parametrizado  
- ✅ **File Upload** - Validação rigorosa
- ✅ **Path Traversal** - Sanitização de caminhos

#### **Headers de Segurança**
```http
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
```

### **⚡ Rate Limiting**

#### **Limites Configurados**
- **Login**: 5 tentativas em 15 min → Bloqueio 30 min
- **Upload**: 20 uploads em 60 min por usuário
- **Global**: Distribuído via Redis entre servidores

#### **Configuração**
```json
{
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
```

### **📋 Auditoria**

#### **Logs de Segurança**
```json
{
  "Timestamp": "2025-07-16T14:52:39Z",
  "EventType": "LOGIN_ATTEMPT",
  "UserId": "user@empresa.com",
  "IpAddress": "192.168.1.100",
  "Success": true,
  "Details": {"LoginMethod": "Password"}
}
```

#### **Eventos Auditados**
- ✅ Tentativas de login (sucesso/falha)
- ✅ Uploads de documentos
- ✅ Downloads de documentos
- ✅ Alterações de dados
- ✅ Acessos negados

### **🔧 Scripts de Hardening**
```powershell
# Auditoria completa de segurança
.\Auditoria-Seguranca.ps1

# Hardening do Windows Server
.\Hardening-Seguranca.ps1
```

---

## 📊 **FUNCIONALIDADES**

### **📄 Sistema de Documentos**

#### **Upload de Documentos**
- ✅ **Tipos suportados**: PDF, Office, imagens, texto, ZIP
- ✅ **Tamanho máximo**: 10MB por arquivo
- ✅ **Organização**: Por departamentos
- ✅ **Controle de acesso**: Baseado em departamento/role
- ✅ **Versionamento**: Histórico de alterações

#### **🔍 Busca Avançada**
```
Filtros disponíveis:
✅ Termo de busca (nome do arquivo/conteúdo)
✅ Departamento específico
✅ Tipo de arquivo (PDF, Word, Excel, etc.)
✅ Período de data (início/fim)
✅ Usuário que fez upload
```

#### **📁 Organização de Arquivos**
```
DocumentsStorage/
├── Pessoal/
├── Fiscal/
├── Contabil/
├── Cadastro/
├── Apoio/
├── TI/
└── Geral/
```

### **📅 Sistema de Reuniões**

#### **Tipos de Reunião**
- **Ordinária** - Reuniões regulares programadas
- **Extraordinária** - Reuniões especiais eventuais
- **Emergencial** - Reuniões urgentes

#### **Funcionalidades**
- ✅ **Agendamento** com data/hora
- ✅ **Controle de participantes**
- ✅ **Notificações por email**
- ✅ **Status tracking** (Agendada, Em Andamento, Concluída)
- ✅ **Relatórios** e analytics

### **📞 Ramais Telefônicos**

#### **Funcionalidades**
- ✅ **Catálogo completo** de ramais
- ✅ **Organização por departamento**
- ✅ **Busca rápida** por nome/ramal/cargo
- ✅ **Informações de contato** (email, cargo)
- ✅ **Interface responsiva**

### **📊 Analytics e Relatórios**

#### **Dashboard Executivo**
- 📈 **Estatísticas de documentos** (total, mensais, por departamento)
- 📈 **Métricas de reuniões** (por tipo, status, departamento)
- 📈 **Atividade por usuário** (uploads, downloads, reuniões)
- 📈 **Performance do sistema** (Redis, MySQL, aplicação)

#### **Relatórios Disponíveis**
- **Documentos mais baixados**
- **Usuários mais ativos**
- **Atividade por departamento**
- **Estatísticas mensais**
- **Tipos de arquivo mais usados**

---

## 🛠️ **DESENVOLVIMENTO**

### **📁 Estrutura do Projeto**
```
IntranetDocumentos/
├── Controllers/          # Controladores MVC
├── Models/              # Entidades e ViewModels
├── Views/               # Views Razor
├── Services/            # Lógica de negócio
├── Data/                # Entity Framework
├── Middleware/          # Middlewares customizados
├── Extensions/          # Métodos de extensão
├── Builders/            # Builder Pattern
├── wwwroot/             # Arquivos estáticos
├── DocumentsStorage/    # Armazenamento documentos
└── DatabaseBackups/     # Backups automáticos
```

### **🏗️ Padrões Arquiteturais**

#### **Repository Pattern**
```csharp
public interface IDocumentRepository
{
    Task<Document> GetByIdAsync(int id);
    Task<List<Document>> GetByDepartmentAsync(int departmentId);
    Task<Document> AddAsync(Document document);
}
```

#### **Service Pattern**
```csharp
public interface IDocumentService  
{
    Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user);
    Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user);
    Task SaveDocumentAsync(IFormFile file, ApplicationUser user, int departmentId);
}
```

### **🔧 Convenções de Código**

#### **Nomenclatura**
```csharp
// Classes: PascalCase
public class DocumentService { }

// Métodos: PascalCase com Async suffix
public async Task<Document> GetDocumentAsync(int id) { }

// Propriedades: PascalCase
public string OriginalFileName { get; set; }

// Variáveis: camelCase
var documentId = 123;

// Interfaces: I + PascalCase
public interface IDocumentService { }
```

#### **Logging Estruturado**
```csharp
_logger.LogInformation("📄 Upload iniciado - Arquivo: {FileName}, Usuário: {UserId}", 
    fileName, userId);

_logger.LogWarning("🔒 Acesso negado - Documento: {DocumentId}, Usuário: {UserId}", 
    documentId, userId);
```

### **🧪 Testes**
```
Tests/
├── UnitTests/           # Testes unitários
├── IntegrationTests/    # Testes de integração  
└── EndToEndTests/       # Testes E2E
```

### **📦 Build e Deploy**
```powershell
# Build local
dotnet build IntranetDocumentos.csproj --configuration Release

# Publicar
dotnet publish --configuration Release --output ./publish

# Deploy automatizado
.\Deploy-WindowsServer.ps1
```

---

## 🆘 **TROUBLESHOOTING**

### **❓ Problemas Comuns**

#### **🔴 "Failed to bind to address already in use"**
```bash
# Verificar processos na porta
netstat -tulpn | grep :5000

# Usar porta diferente
dotnet run --urls "http://localhost:5001"
```

#### **🔴 "Connection string not found"**
```json
// Verificar appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=senha;"
  }
}
```

#### **🔴 "Redis connection failed"**
```bash
# Verificar Redis
redis-cli ping
# Resultado esperado: PONG

# Se não funcionar, Redis é opcional
# Aplicação usa MemoryCache como fallback
```

#### **🔴 "File upload size exceeded"**
```xml
<!-- web.config -->
<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="52428800" /> <!-- 50MB -->
    </requestFiltering>
  </security>
</system.webServer>
```

### **📊 Diagnóstico**

#### **Verificar Status da Aplicação**
```powershell
# Status IIS
Get-WebAppPoolState -Name "IntranetDocumentos"

# Logs da aplicação  
Get-Content "Logs\application-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50

# Conectividade MySQL
Test-NetConnection -ComputerName localhost -Port 3306

# Conectividade Redis
Test-NetConnection -ComputerName localhost -Port 6379
```

### **💾 Backup e Restore**

#### **Backup Automático**
```powershell
# Configurar backup diário
.\backup-database.ps1

# Agendador do Windows
schtasks /create /tn "Backup Intranet" /tr "C:\path\backup-database.ps1" /sc daily /st 02:00
```

#### **Backup Manual**
```bash
# Backup MySQL
mysqldump -u app_user -p IntranetDocumentos > backup.sql

# Backup arquivos
tar -czf DocumentsStorage_backup.tar.gz DocumentsStorage/
```

#### **Restore**
```bash
# Restore MySQL
mysql -u app_user -p IntranetDocumentos < backup.sql

# Restore arquivos
tar -xzf DocumentsStorage_backup.tar.gz
```

---

## 📝 **CHANGELOG E ROADMAP**

### **📅 Versão 2.0 (16/07/2025) - ATUAL**

#### **✅ Correções Implementadas**
- ✅ **Rota AdvancedSearch** implementada (DocumentsController)
- ✅ **Queries LINQ otimizadas** (AnalyticsService)
- ✅ **Sintaxe SQL MySQL** corrigida
- ✅ **Redis integrado** com rate limiting distribuído
- ✅ **Middlewares de segurança** implementados
- ✅ **VS Code configurado** para MySQL

#### **🔒 Melhorias de Segurança**
- ✅ Rate limiting por usuário/email
- ✅ Headers de segurança enterprise
- ✅ Auditoria completa implementada
- ✅ Upload seguro de arquivos
- ✅ Validação robusta de entrada

#### **⚡ Otimizações de Performance**
- ✅ Cache Redis distribuído
- ✅ Queries MySQL otimizadas
- ✅ Índices de banco otimizados
- ✅ Streaming de arquivos
- ✅ Fallback para MemoryCache

### **🎯 Roadmap Futuro**

#### **Versão 2.1 (Próximos 30 dias)**
- 🔄 **Notificações em tempo real** (SignalR)
- 📱 **PWA** (Progressive Web App)
- 🔍 **Busca full-text** em conteúdo
- 📊 **Dashboard avançado**
- 🌍 **Multi-idioma** (i18n)

#### **Versão 2.2 (Próximos 60 dias)**
- ☁️ **Integração nuvem** (Azure/AWS)
- 🤖 **OCR** para documentos escaneados
- 📧 **Aprovação por email**
- 📱 **App mobile**
- 🔄 **Sincronização offline**

#### **Versão 3.0 (Próximos 90 dias)**
- 🤖 **IA para categorização**
- 🔐 **Single Sign-On** (SAML/OAuth)
- 📊 **Business Intelligence**
- 🌐 **Multi-tenant**
- 🔄 **Microservices**

---

## 📞 **SUPORTE E CONTATO**

### **🔧 Suporte Técnico**
- **Documentação**: Esta documentação unificada
- **Issues**: [GitHub Issues](https://github.com/Vollupios/mc_web_app/issues)
- **Wiki**: [GitHub Wiki](https://github.com/Vollupios/mc_web_app/wiki)

### **📚 Recursos Adicionais**
- **Scripts de Deploy**: Disponíveis na pasta raiz
- **Configurações de Produção**: `appsettings.Production.json`
- **Logs**: Pasta `Logs/` (criada automaticamente)
- **Backups**: Pasta `DatabaseBackups/`

### **🎯 Status do Projeto**
**✅ PRODUÇÃO READY - Sistema 100% funcional e testado**

---

## ⚖️ **LICENÇA**

Este projeto está licenciado sob a [MIT License](LICENSE).

---

**📅 Última atualização:** 16 de Julho de 2025  
**🔢 Versão da documentação:** 2.0  
**✅ Status:** Documentação completa e unificada**
