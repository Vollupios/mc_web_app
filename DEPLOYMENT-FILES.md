# 📦 Arquivos de Instalação Criados

## Scripts PowerShell (.ps1)
- `Deploy-WindowsServer.ps1` - Script principal de deploy automático
- `Configuracao-IIS.ps1` - Configuração específica do IIS e Application Pool  
- `Publish-ToWindowsServer.ps1` - Script para publicação remota
- `Verificacao-Pos-Instalacao.ps1` - Verificação e diagnóstico pós-instalação

## Scripts Batch (.bat)
- `deploy-quick.bat` - Instalação rápida automatizada (execute como Admin)

## Configuração
- `web.config` - Configuração do IIS para ASP.NET Core
- `appsettings.Production.json` - Configurações de produção (atualizado)
- `setup-mysql.sql` - Script SQL para configurar banco MySQL

## Documentação
- `INSTALL-GUIDE.md` - Guia completo de instalação passo a passo
- `PACKAGE-README.md` - Resumo do pacote de instalação
- `DEPLOYMENT-FILES.md` - Este arquivo (lista de arquivos criados)

## Projeto Atualizado
- `IntranetDocumentos.csproj` - Dependências atualizadas para .NET 9
- `Program.cs` - Configurações de produção e logging do Windows

## Como Usar

### Instalação Automática (Recomendado)
```batch
# Execute como Administrador
deploy-quick.bat
```

### Instalação Manual
```powershell
# 1. Deploy completo
.\Deploy-WindowsServer.ps1

# 2. Configurar IIS
.\Configuracao-IIS.ps1

# 3. Verificar instalação
.\Verificacao-Pos-Instalacao.ps1
```

### Publicação Remota
```powershell
# Para publicar em servidor remoto
.\Publish-ToWindowsServer.ps1 -TargetServer "SEU-SERVIDOR"
```

## Ordem de Execução Recomendada

1. **`deploy-quick.bat`** OU **`Deploy-WindowsServer.ps1`**
2. Configurar MySQL (usar `setup-mysql.sql`)
3. Editar `appsettings.Production.json` (senhas e configurações)
4. **`Configuracao-IIS.ps1`** (se não feito automaticamente)
5. **`Verificacao-Pos-Instalacao.ps1`** (verificar se tudo está OK)

## Pré-requisitos

- Windows Server 2019/2022 ou Windows 10/11 Pro
- Privilégios de Administrador
- .NET 9.0 Hosting Bundle (baixado automaticamente)
- MySQL 8.0+ ou MariaDB 10.5+
- IIS habilitado

## Status do Projeto

✅ **Pronto para instalação em Windows Server**

- Dependências atualizadas
- Scripts de deploy criados
- Documentação completa
- Configurações de produção
- Verificação automática
- Suporte a IIS com web.config
- Logging para Event Viewer do Windows
- Estrutura de diretórios automatizada

---

**Desenvolvido para ASP.NET Core 9.0 + MySQL + IIS**
