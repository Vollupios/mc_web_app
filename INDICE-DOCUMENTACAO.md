# 📚 Índice da Documentação - Intranet Documentos

## 🚀 Documentação Principal

### **👉 Para Instalação e Deploy - COMECE AQUI**

**[GUIA-UNIFICADO-FINAL.md](GUIA-UNIFICADO-FINAL.md)** - **GUIA COMPLETO DEFINITIVO**
> Guia unificado final que contém ABSOLUTAMENTE TUDO sobre:
> - Instalação rápida (1 comando)
> - Instalação manual detalhada
> - Configuração de produção
> - Deploy remoto
> - Verificação e diagnóstico  
> - Solução de problemas
> - Backup e manutenção
> - Scripts, arquivos e checklist completo

## 📖 Documentação Técnica

### Projeto e Funcionalidades

- **[README.md](README.md)** - Visão geral do projeto e funcionalidades
- **[STATUS-FINAL.md](STATUS-FINAL.md)** - Status atual, problemas corrigidos e estado do projeto

### ⚠️ Documentação Anterior (Obsoleta - Integrada ao Guia Final)

- ~~[GUIA-COMPLETO.md](GUIA-COMPLETO.md)~~ - ✅ **Substituído por GUIA-UNIFICADO-FINAL.md**
- ~~[INSTALL-GUIDE.md](INSTALL-GUIDE.md)~~ - ✅ **Integrado ao GUIA-UNIFICADO-FINAL.md**
- ~~[DEPLOY-GUIDE.md](DEPLOY-GUIDE.md)~~ - ✅ **Integrado ao GUIA-UNIFICADO-FINAL.md**
- ~~[PACKAGE-README.md](PACKAGE-README.md)~~ - ✅ **Integrado ao GUIA-UNIFICADO-FINAL.md**
- ~~[DEPLOYMENT-FILES.md](DEPLOYMENT-FILES.md)~~ - ✅ **Integrado ao GUIA-UNIFICADO-FINAL.md**

## 🛠️ Scripts de Instalação

### Execução Rápida

```batch
# Instalação automática (Execute como Administrador)
deploy-quick.bat
```

### Scripts PowerShell

- **`Deploy-WindowsServer.ps1`** - Script principal de deploy automático
- **`Configuracao-IIS.ps1`** - Configuração específica do IIS
- **`Publish-ToWindowsServer.ps1`** - Deploy remoto
- **`Verificacao-Pos-Instalacao.ps1`** - Verificação pós-instalação

### Configuração

- **`web.config`** - Configuração do IIS
- **`appsettings.Production.json`** - Configurações de produção
- **`setup-mysql.sql`** - Setup do banco MySQL

## 🎯 Fluxo de Uso da Documentação

### 1. **Primeira Vez** (Instalação)

```text
1. Ler GUIA-UNIFICADO-FINAL.md → Seção "Instalação Rápida"
2. Executar deploy-quick.bat como Admin
3. Configurar appsettings.Production.json
4. Executar Verificacao-Pos-Instalacao.ps1
5. Acessar aplicação
```

### 2. **Deploy Manual** (Controle Total)

```text
1. Ler GUIA-UNIFICADO-FINAL.md → Seção "Instalação Manual Detalhada"
2. Seguir passo a passo manual
3. Usar scripts PowerShell específicos
4. Configurar conforme necessário
```

### 3. **Deploy Remoto** (Produção)

```text
1. Ler GUIA-UNIFICADO-FINAL.md → Seção "Deploy Remoto"
2. Usar Publish-ToWindowsServer.ps1
3. Configurar certificados SSL
4. Implementar monitoramento
```

### 4. **Problemas** (Troubleshooting)

```text
1. Executar Verificacao-Pos-Instalacao.ps1
2. Consultar GUIA-UNIFICADO-FINAL.md → Seção "Solução de Problemas"
3. Verificar logs conforme orientações
4. Aplicar correções sugeridas
```

### 5. **Manutenção** (Operação)

```text
1. Consultar GUIA-UNIFICADO-FINAL.md → Seção "Backup e Manutenção"
2. Implementar rotinas de backup
3. Monitorar performance
4. Manter atualizações de segurança
```

## 📋 Checklist Rápido

### ✅ Pré-Instalação

- [ ] Windows Server 2019/2022
- [ ] Privilégios de Administrador
- [ ] Internet disponível

### ✅ Instalação

- [ ] Executado deploy-quick.bat
- [ ] Configurado appsettings.Production.json
- [ ] Verificado com Verificacao-Pos-Instalacao.ps1

### ✅ Funcionamento

- [ ] Site acessível via navegador
- [ ] Login admin funcionando
- [ ] Upload/download testado
- [ ] Backup funcionando

## 🆘 Suporte Rápido

### Comandos de Verificação

```powershell
# Verificação completa automatizada
.\Verificacao-Pos-Instalacao.ps1

# Status básico
Get-Website | Where-Object { $_.Name -eq "Intranet Documentos" }
Get-IISAppPool | Where-Object { $_.Name -eq "IntranetDocumentos" }

# Testar conectividade
Invoke-WebRequest -Uri "http://localhost" -Method HEAD
```

### Logs Importantes

- **Event Viewer**: Windows Logs → Application → IntranetDocumentos
- **IIS**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **App**: `C:\IntranetData\Logs\app-*.log`

## 🔗 Links Rápidos

| Ação | Arquivo | Comando |
|------|---------|---------|
| **Instalar** | deploy-quick.bat | Execute como Admin |
| **Configurar** | appsettings.Production.json | Editar senhas |
| **Verificar** | Verificacao-Pos-Instalacao.ps1 | PowerShell |
| **Documentar** | GUIA-COMPLETO.md | Ler seções relevantes |

---

## 🎉 Resumo

**✅ Tudo está unificado no [GUIA-COMPLETO.md](GUIA-COMPLETO.md)**

Este arquivo contém absolutamente TUDO que você precisa:

- Instalação rápida e manual
- Deploy remoto e local
- Configuração completa
- Solução de problemas
- Manutenção e operação
- Scripts e exemplos
- Checklist completo

**👉 Comece pelo GUIA-COMPLETO.md e tenha sucesso garantido!**
