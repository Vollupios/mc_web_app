# 📁 Scripts - Intranet Documentos

Pasta unificada com todos os scripts de instalação, deploy, desenvolvimento e manutenção do sistema.

---

## 📋 **ÍNDICE DE SCRIPTS**

### 🚀 **INSTALAÇÃO PRINCIPAL**

| Script | Descrição | Uso |
|--------|-----------|-----|
| **[install-quick.bat](install-quick.bat)** | 🎯 **Instalação ultra-rápida** | `install-quick.bat` |
| **[Install-IntranetDocumentos.ps1](Install-IntranetDocumentos.ps1)** | 📋 **Instalador completo** | `.\Install-IntranetDocumentos.ps1` |

### 📁 **ORGANIZE POR CATEGORIAS**

#### 🚀 **Deploy (/Deploy/)**
- **Deploy-WindowsServer.ps1** - Deploy completo para Windows Server
- **Configuracao-IIS.ps1** - Configuração automática do IIS
- **Install-Redis-Windows.ps1** - Instalação do Redis
- **Publish-ToWindowsServer.ps1** - Publicação para servidor remoto
- **Verificacao-Pos-Instalacao.ps1** - Verificações pós-instalação

#### 🗄️ **Banco de Dados (/Database/)**
- **Setup-Database.ps1** - Configurador unificado de BD
- **backup-database.ps1** - Backup automático
- **recreate-database.ps1** - Recriar banco (desenvolvimento)
- **fix-database.ps1/.sh** - Correção de problemas no BD
- **setup-database.mysql.sql** - Script MySQL
- **setup-mysql.sql** - Configuração MySQL alternativa
- **check-departments.sql** - Verificação de departamentos

#### 🔒 **Segurança (/Security/)**
- **Hardening-Seguranca.ps1** - Hardening do sistema
- **Auditoria-Seguranca.ps1** - Auditoria de segurança

#### 🛠️ **Desenvolvimento (/Development/)**
- **Dev-Tools.ps1** - Ferramentas unificadas de desenvolvimento
- **run-app.ps1** - Executar aplicação
- **start-app.ps1** - Inicialização alternativa
- **check-admin-user.ps1** - Verificar usuário admin
- **build-analytics.sh** - Build de analytics
- **test-analytics.sh** - Testes de analytics
- **fix-markdown.sh** - Correção de markdown

---

## 🎯 **GUIA DE USO RÁPIDO**

### **Para Administradores - Primeira Instalação**

```batch
# Opção 1: Instalação ultra-rápida (Windows)
install-quick.bat

# Opção 2: Instalação completa com opções
powershell -ExecutionPolicy Bypass -File Install-IntranetDocumentos.ps1
```

### **Para Desenvolvedores**

```powershell
# Ferramentas de desenvolvimento
.\Development\Dev-Tools.ps1

# Executar aplicação em modo development
.\Development\Dev-Tools.ps1 run -Watch

# Build da aplicação
.\Development\Dev-Tools.ps1 build -Configuration Release
```

### **Para DevOps/Produção**

```powershell
# Deploy completo para Windows Server
.\Deploy\Deploy-WindowsServer.ps1

# Configurar IIS
.\Deploy\Configuracao-IIS.ps1

# Aplicar hardening de segurança
.\Security\Hardening-Seguranca.ps1
```

---

## 🔧 **INSTALAÇÃO DETALHADA**

### **1. 🚀 Instalação Rápida (Recomendado)**

```batch
# Execute como Administrador
install-quick.bat
```

**O que faz:**
- ✅ Verifica pré-requisitos (.NET, privilégios)
- ✅ Configura banco SQLite automaticamente
- ✅ Restaura dependências e compila
- ✅ Inicia aplicação em modo desenvolvimento
- ✅ Abre automaticamente no navegador

### **2. 📋 Instalação Completa com Opções**

```powershell
# Instalação padrão de produção
.\Install-IntranetDocumentos.ps1

# Desenvolvimento sem IIS
.\Install-IntranetDocumentos.ps1 -InstallType Dev -WithIIS:$false

# Produção sem Redis
.\Install-IntranetDocumentos.ps1 -WithRedis:$false

# Instalação silenciosa
.\Install-IntranetDocumentos.ps1 -Silent
```

**Parâmetros disponíveis:**
- `InstallType`: Dev ou Production
- `WithRedis`: Instalar Redis (padrão: true)
- `WithSecurity`: Aplicar hardening (padrão: true)
- `WithIIS`: Configurar IIS (padrão: true)
- `WithVerification`: Verificar instalação (padrão: true)
- `Silent`: Sem interação (padrão: false)

---

## 🗄️ **BANCO DE DADOS**

### **Configuração Automática**

```powershell
# SQLite (desenvolvimento)
.\Database\Setup-Database.ps1 -DatabaseType SQLite

# MySQL (produção)
.\Database\Setup-Database.ps1 -DatabaseType MySQL

# Recriar banco (ATENÇÃO: apaga dados!)
.\Database\Setup-Database.ps1 -Recreate
```

### **Backup e Manutenção**

```powershell
# Backup manual
.\Database\backup-database.ps1

# Verificar integridade
.\Database\check-departments.sql
```

---

## 🔒 **SEGURANÇA**

### **Hardening Automático**

```powershell
# Aplicar todas as configurações de segurança
.\Security\Hardening-Seguranca.ps1

# Auditoria de segurança
.\Security\Auditoria-Seguranca.ps1
```

**Configurações aplicadas:**
- Headers de segurança HTTP
- Rate limiting distribuído
- Configurações de SSL/TLS
- Permissões de arquivos
- Firewall básico
- Logs de auditoria

---

## 🛠️ **DESENVOLVIMENTO**

### **Menu Interativo**

```powershell
# Abre menu com todas as opções
.\Development\Dev-Tools.ps1
```

### **Comandos Diretos**

```powershell
# Executar em modo development
.\Development\Dev-Tools.ps1 run

# Modo watch (recarrega automaticamente)
.\Development\Dev-Tools.ps1 run -Watch

# Build
.\Development\Dev-Tools.ps1 build

# Testes
.\Development\Dev-Tools.ps1 test

# Migrações
.\Development\Dev-Tools.ps1 migrate

# Limpeza
.\Development\Dev-Tools.ps1 clean

# Reset completo
.\Development\Dev-Tools.ps1 reset
```

---

## 🌐 **DEPLOY EM PRODUÇÃO**

### **Windows Server + IIS**

```powershell
# Deploy completo
.\Deploy\Deploy-WindowsServer.ps1

# Apenas IIS
.\Deploy\Configuracao-IIS.ps1

# Verificação pós-deploy
.\Deploy\Verificacao-Pos-Instalacao.ps1
```

### **Redis Cache**

```powershell
# Instalar Redis no Windows
.\Deploy\Install-Redis-Windows.ps1
```

---

## 📋 **VERIFICAÇÕES E DIAGNÓSTICO**

### **Verificação Pós-Instalação**

```powershell
# Verificação completa
.\Deploy\Verificacao-Pos-Instalacao.ps1
```

**Verifica:**
- ✅ Aplicação respondendo
- ✅ Banco de dados conectado
- ✅ Redis funcionando (se instalado)
- ✅ IIS configurado (se aplicável)
- ✅ Permissões de arquivos
- ✅ Logs sem erros

### **Diagnóstico de Problemas**

```powershell
# Verificar integridade do projeto
.\Development\Dev-Tools.ps1 check

# Verificar usuário admin
.\Development\check-admin-user.ps1

# Auditoria de segurança
.\Security\Auditoria-Seguranca.ps1
```

---

## 📝 **LOGS E DEBUGGING**

### **Localização dos Logs**

```
Logs/
├── install-*.log           # Logs de instalação
├── application-*.log       # Logs da aplicação
├── security-*.log          # Logs de segurança
└── deploy-*.log           # Logs de deploy
```

### **Verificar Logs**

```powershell
# Ver último log de instalação
Get-Content (Get-ChildItem Logs\install-*.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName

# Monitorar logs da aplicação
Get-Content Logs\application-latest.log -Wait
```

---

## 🆘 **RESOLUÇÃO DE PROBLEMAS**

### **Problemas Comuns**

#### **"Acesso negado" / "Access Denied"**
```powershell
# Execute como Administrador
# Clique direito no PowerShell → "Executar como administrador"
```

#### **".NET não encontrado"**
```powershell
# Instale .NET 8.0+ de: https://dotnet.microsoft.com/download
```

#### **"Não é possível executar scripts"**
```powershell
# Execute uma vez:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

#### **Erro de conexão com banco**
```powershell
# Reconfigure o banco:
.\Database\Setup-Database.ps1 -Recreate
```

#### **Aplicação não inicia**
```powershell
# Verificar dependências:
.\Development\Dev-Tools.ps1 check

# Reset completo:
.\Development\Dev-Tools.ps1 reset
```

---

## 📞 **SUPORTE**

### **Documentação Completa**
📚 **[DOCUMENTACAO-OFICIAL-UNIFICADA.md](../DOCUMENTACAO-OFICIAL-UNIFICADA.md)**

### **Links Úteis**
- 🐙 **Repositório:** https://github.com/Vollupios/mc_web_app
- 📋 **Issues:** https://github.com/Vollupios/mc_web_app/issues
- 📖 **Wiki:** https://github.com/Vollupios/mc_web_app/wiki

### **Informações do Sistema**
- **Versão:** 2.0 Production Ready
- **Framework:** ASP.NET Core 9.0
- **Banco:** SQLite (dev) / MySQL (prod)
- **Cache:** Redis (opcional)
- **Web Server:** Kestrel (dev) / IIS (prod)

---

**🎯 Para qualquer dúvida, consulte primeiro a [documentação oficial](../DOCUMENTACAO-OFICIAL-UNIFICADA.md) que contém informações detalhadas sobre instalação, configuração e uso do sistema.**
