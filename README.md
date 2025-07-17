# 📚 Intranet Documentos

> **Sistema de Gestão de Documentos Corporativos**  
> Desenvolvido em ASP.NET Core MVC com foco em segurança, performance e escalabilidade

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-orange.svg)](https://www.microsoft.com/sql-server/)
[![SQLite](https://img.shields.io/badge/SQLite-3.0+-blue.svg)](https://www.sqlite.org/)
[![Redis](https://img.shields.io/badge/Redis-7.0+-red.svg)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 🎯 **Visão Geral**

Sistema web corporativo para gestão centralizada de documentos, reuniões e ramais telefônicos, com controle de acesso baseado em departamentos e roles de usuário.

### **✨ Principais Funcionalidades**

- 📄 **Gestão de Documentos**: Upload, download, organização por departamentos
- 🔍 **Busca Avançada**: Pesquisa full-text com filtros inteligentes  
- 👥 **Controle de Acesso**: Baseado em departamentos e roles (Admin/Gestor/Usuario)
- 📅 **Sistema de Reuniões**: Agendamento e controle de reuniões
- 📞 **Ramais Telefônicos**: Catálogo de ramais internos
- 📊 **Analytics**: Dashboard com estatísticas e relatórios
- 🔴 **Cache Redis**: Performance otimizada com cache distribuído
- 🔒 **Segurança Enterprise**: Rate limiting, hardening, auditoria

---

## 🚀 **Instalação Rápida**

### **Para Desenvolvimento:**

```bash
# 1. Clonar repositório
git clone https://github.com/Vollupios/mc_web_app.git
cd IntranetDocumentos

# 2. Executar instalação automatizada
.\Scripts\install-quick.bat
```text

### **Para Produção (Windows Server):**

```powershell
# Executar instalador principal
.\Scripts\Install-IntranetDocumentos.ps1 -InstallType Production
```text

---

## 📖 **Documentação**

### **📚 Documentação Completa:**

- [**Parte 1: Informações Gerais e Instalação**](DOCUMENTACAO-UNIFICADA.md)
- [**Parte 2: Segurança, Performance e Funcionalidades**](DOCUMENTACAO-UNIFICADA-PARTE2.md)  
- [**Parte 3: Desenvolvimento e Troubleshooting**](DOCUMENTACAO-UNIFICADA-PARTE3.md)

### **🔧 Scripts e Automação:**

- [**Guia de Scripts Unificados**](SCRIPTS-UNIFICADOS.md)
- [**Status dos Scripts**](STATUS-SCRIPTS-FINAL.md)

### **📋 Status do Projeto:**

- [**Correções SQL**](STATUS-CORRECOES-SQL.md)
- [**Implementação Redis**](REDIS-IMPLEMENTADO.md)
- [**Correções de Rotas**](STATUS-CORRECAO-ROTA.md)

---

## 🏗️ **Arquitetura**

### **Stack Tecnológico:**

- **Backend**: ASP.NET Core 9.0 MVC
- **Frontend**: Bootstrap 5 + Bootstrap Icons
- **Database**: SQL Server 2019+ (Produção) / SQLite (Desenvolvimento)
- **Cache**: Redis 7.0+
- **ORM**: Entity Framework Core 9.0
- **Auth**: ASP.NET Core Identity

### **Estrutura do Projeto:**

```texttext
📁 IntranetDocumentos/
├── 📁 Controllers/          # Controladores MVC
├── 📁 Models/              # Entidades e ViewModels  
├── 📁 Services/            # Lógica de negócio
├── 📁 Views/               # Views Razor
├── 📁 Scripts/             # 🚀 Scripts de instalação/deploy
├── 📁 Data/                # Contexto EF Core
├── 📁 Middleware/          # Middlewares customizados
├── 📁 Extensions/          # Extensões de funcionalidade
└── 📁 DocumentsStorage/    # Armazenamento de arquivos
```text

---

## 👥 **Roles e Permissões**

| Role | Permissões | Departamentos |
|------|------------|---------------|
| **Admin** | Acesso total + gestão de usuários | Todos + TI |
| **Gestor** | Acesso a todos os documentos | Todos |
| **Usuario** | Acesso ao próprio departamento | Próprio + Geral |

### **Departamentos:**

- Pessoal, Fiscal, Contábil, Cadastro, Apoio, TI
- **Geral**: Acessível por todos os usuários

---

## 🔒 **Segurança**

- ✅ **Autenticação**: ASP.NET Core Identity
- ✅ **Autorização**: Role-based + Department-based
- ✅ **Rate Limiting**: Proteção contra ataques
- ✅ **Validação de Upload**: Tipos e tamanhos de arquivo
- ✅ **Headers de Segurança**: HSTS, CSP, X-Frame-Options
- ✅ **Hardening**: Scripts automatizados de segurança
- ✅ **Auditoria**: Logs de segurança e acesso

---

## ⚡ **Performance**

- 🔴 **Redis Cache**: Cache distribuído para sessões e dados
- 📊 **Analytics Otimizadas**: Queries LINQ otimizadas
- 🗄️ **EF Core**: Queries eficientes com lazy loading
- 📁 **File Storage**: Armazenamento otimizado fora da wwwroot
- 🚀 **Streaming**: Download de arquivos grandes via streaming

---

## 🛠️ **Desenvolvimento**

### **Pré-requisitos:**

- .NET 9.0 SDK
- SQL Server 2019+ ou SQLite
- Redis (opcional para desenvolvimento)
- Visual Studio 2022+ ou VS Code

### **Comandos Úteis:**

```bash
# Executar aplicação
dotnet run

# Executar migrações
dotnet ef database update

# Executar testes
dotnet test

# Scripts de desenvolvimento
.\Scripts\Development\Dev-Tools.ps1
```text

---

## 📊 **Status Atual**

### **✅ Funcionalidades Implementadas:**

- [x] Sistema de documentos completo
- [x] Busca avançada funcionando
- [x] Redis cache integrado
- [x] Sistema de reuniões
- [x] Ramais telefônicos
- [x] Analytics e dashboard
- [x] Segurança enterprise
- [x] Scripts de deploy automatizados

### **🚀 Próximos Passos:**

- [ ] Notificações em tempo real
- [ ] API REST para integração
- [ ] App mobile (futuro)
- [ ] Backup automatizado em nuvem

---

## 👨‍💻 **Contribuição**

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## � **Correções Aplicadas**

### ✅ Scripts Unificados e Organizados

- Criada estrutura `/Scripts/` para organização
- Corrigidos warnings PSScriptAnalyzer nos scripts PowerShell
- Removidos scripts duplicados e desnecessários

### ✅ Workflow Restrito a Admins

- Acesso ao WorkflowController limitado apenas a usuários Admin
- Menu e colunas de workflow ocultados para usuários não-admin

### ✅ Visualização de Documentos Corrigida

- PDFs e imagens agora são exibidos no navegador (inline)
- Outros tipos de arquivo são baixados automaticamente
- Corrigida codificação UTF-8 para nomes de arquivos com acentos

### ✅ Dashboard de Analytics Funcionando

- Corrigido erro LINQ no cálculo de tempo médio de reuniões
- Dashboard agora carrega corretamente via menu "Dados > Dashboard"
- Exibe estatísticas de documentos, reuniões e departamentos

### 📁 Arquivos Removidos

Consulte `ANALISE-ARQUIVOS-DUPLICADOS.md` para lista completa dos 27 arquivos removidos.

---

## �📄 **Licença**

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 📞 **Suporte**

- **Documentação**: Consulte os arquivos `.md` na raiz do projeto
- **Issues**: Use o sistema de issues do GitHub
- **Scripts**: Pasta `/Scripts/` com automações completas

---

**🎉 Desenvolvido com ❤️ para gestão corporativa eficiente!**
