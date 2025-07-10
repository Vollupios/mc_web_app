# 🚀 Guia de Instalação - Intranet Documentos

Este guia fornece instruções passo a passo para instalar a aplicação **Intranet Documentos** em um Windows Server com IIS.

## 📋 Pré-requisitos

### Software Necessário
- ✅ **Windows Server 2019/2022** ou Windows 10/11 Pro
- ✅ **IIS (Internet Information Services)** com ASP.NET Core Module V2
- ✅ **.NET 9.0 Hosting Bundle** ([Download aqui](https://dotnet.microsoft.com/download/dotnet/9.0))
- ✅ **MySQL Server 8.0+** ou **MariaDB 10.5+**

### Hardware Recomendado
- **CPU**: 2+ cores
- **RAM**: 4GB+ (recomendado 8GB)
- **Disco**: 20GB+ espaço livre
- **Rede**: Conexão estável com a internet

## 🛠️ Instalação Rápida

### Opção 1: Script Automatizado (Recomendado)

1. **Execute como Administrador**:
   ```batch
   deploy-quick.bat
   ```

2. **Siga as instruções** do script para completar a instalação.

### Opção 2: Instalação Manual

#### Passo 1: Preparar o Ambiente

1. **Instalar .NET 9.0 Hosting Bundle**:
   - Baixe e instale o [.NET 9.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Reinicie o servidor após a instalação

2. **Configurar IIS**:
   ```powershell
   # Execute como Administrador
   .\Configuracao-IIS.ps1
   ```

#### Passo 2: Instalar MySQL

1. **Download e Instalação**:
   - Baixe o [MySQL Server](https://dev.mysql.com/downloads/mysql/)
   - Instale seguindo o assistente
   - Configure usuário `root` com senha segura

2. **Criar Banco de Dados**:
   ```sql
   -- Execute no MySQL Workbench ou linha de comando
   source setup-mysql.sql
   ```

#### Passo 3: Deploy da Aplicação

1. **Publicar aplicação**:
   ```powershell
   # Na máquina de desenvolvimento
   .\Publish-ToWindowsServer.ps1 -TargetServer "SEU-SERVIDOR"
   ```

2. **Ou copiar manualmente**:
   - Compile o projeto: `dotnet publish -c Release`
   - Copie os arquivos para `C:\inetpub\wwwroot\IntranetDocumentos`

#### Passo 4: Configuração

1. **Editar `appsettings.Production.json`**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=app_user;Password=SUA_SENHA_AQUI;Port=3306;SslMode=Preferred;CharSet=utf8mb4;"
     },
     "EmailSettings": {
       "SmtpServer": "seu-smtp.empresa.com",
       "SmtpPort": 587,
       "SmtpUsername": "intranet@empresa.com",
       "SmtpPassword": "SUA_SENHA_EMAIL_AQUI",
       "EnableSsl": true
     }
   }
   ```

2. **Configurar Permissões**:
   ```cmd
   icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "IIS_IUSRS:(OI)(CI)RX" /T
   icacls "C:\IntranetData" /grant "IIS_IUSRS:(OI)(CI)F" /T
   ```

3. **Iniciar Site**:
   - Abra **IIS Manager**
   - Inicie o **Application Pool**: `IntranetDocumentos`
   - Inicie o **Site**: `Intranet Documentos`

## 🔧 Configuração Avançada

### Configurar HTTPS

1. **Instalar Certificado SSL**:
   ```powershell
   # Com certificado existente
   .\Configuracao-IIS.ps1 -CertificateThumbprint "SEU_THUMBPRINT_AQUI"
   ```

2. **Ou gerar certificado auto-assinado** (apenas desenvolvimento):
   ```powershell
   New-SelfSignedCertificate -DnsName "intranet.empresa.com" -CertStoreLocation "cert:\LocalMachine\My"
   ```

### Configurar Firewall

```powershell
# Permitir HTTP/HTTPS
New-NetFirewallRule -DisplayName "Intranet HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "Intranet HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
```

### Backup Automático

O sistema já inclui backup automático configurado em:
- **Frequência**: Diário (24h)
- **Local**: `C:\IntranetData\Backups\Auto`
- **Retenção**: 30 dias

## 🧪 Teste da Instalação

### 1. Verificar Aplicação
- Acesse: `http://seu-servidor/` ou `https://seu-servidor/`
- Login padrão: `admin@empresa.com` / `Admin123!`

### 2. Testar Funcionalidades
- ✅ Login/Logout
- ✅ Upload de documentos
- ✅ Download de documentos
- ✅ Gestão de usuários (Admin)
- ✅ Agendamento de reuniões

### 3. Verificar Logs
- **IIS Logs**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **Application Logs**: Event Viewer → Windows Logs → Application
- **Custom Logs**: `C:\IntranetData\Logs\`

## 🐛 Solução de Problemas

### Erro 500.30 - ASP.NET Core app failed to start

**Solução**:
1. Verificar se .NET 9.0 está instalado
2. Verificar permissões do Application Pool
3. Verificar connection string
4. Verificar logs no Event Viewer

### Erro de Conexão com Banco

**Solução**:
1. Verificar se MySQL está executando
2. Testar conexão: `mysql -u app_user -p`
3. Verificar firewall do MySQL (porta 3306)
4. Verificar string de conexão

### Upload de Arquivos Falha

**Solução**:
1. Verificar permissões em `C:\IntranetData\Documents`
2. Verificar tamanho máximo no `web.config`
3. Verificar espaço em disco

### Performance Lenta

**Solução**:
1. Aumentar RAM do servidor
2. Otimizar queries do banco
3. Configurar cache do IIS
4. Verificar antivírus em tempo real

## 📞 Suporte

### Logs Importantes
- **Event Viewer**: Windows Logs → Application → Source: "IntranetDocumentos"
- **IIS Logs**: `%SystemDrive%\inetpub\logs\LogFiles`
- **Application Logs**: `C:\IntranetData\Logs\`

### Comandos Úteis

```powershell
# Verificar status do Application Pool
Get-IISAppPool -Name "IntranetDocumentos"

# Reiniciar Application Pool
Restart-WebAppPool -Name "IntranetDocumentos"

# Verificar processo .NET
Get-Process -Name "w3wp" | Where-Object {$_.ProcessName -eq "w3wp"}

# Verificar connection string
Get-Content "C:\inetpub\wwwroot\IntranetDocumentos\appsettings.Production.json"
```

## 📚 Documentação Adicional

- [DEPLOY-GUIDE.md](DEPLOY-GUIDE.md) - Guia completo de deployment
- [README.md](README.md) - Documentação do projeto
- [Arquivos de Script](.) - Scripts PowerShell auxiliares

---

## ✅ Checklist de Instalação

- [ ] .NET 9.0 Hosting Bundle instalado
- [ ] IIS configurado com ASP.NET Core Module
- [ ] MySQL/MariaDB instalado e configurado
- [ ] Banco de dados criado (setup-mysql.sql)
- [ ] Application Pool criado e configurado
- [ ] Site IIS criado e funcionando
- [ ] Arquivos da aplicação deployados
- [ ] Connection string configurada
- [ ] Permissões de arquivo configuradas
- [ ] HTTPS configurado (opcional)
- [ ] Firewall configurado
- [ ] Teste de login realizado
- [ ] Backup automático funcionando

**🎉 Instalação Concluída com Sucesso!**
