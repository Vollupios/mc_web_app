# 📦 Pacote de Instalação - Intranet Documentos

Este pacote contém todos os arquivos necessários para instalar a **Intranet Documentos** em um Windows Server.

## 📁 Conteúdo do Pacote

### Scripts de Instalação

- `deploy-quick.bat` - **Instalação rápida automatizada** (Execute como Admin)
- `Deploy-WindowsServer.ps1` - Script principal de deploy e configuração
- `Configuracao-IIS.ps1` - Configuração específica do IIS e Application Pool
- `Publish-ToWindowsServer.ps1` - Script para publicação remota
- `Verificacao-Pos-Instalacao.ps1` - Verificação pós-instalação

### Configuração

- `appsettings.Production.json` - Configurações para produção
- `web.config` - Configuração do IIS
- `setup-mysql.sql` - Script para configurar banco MySQL

### Documentação

- `INSTALL-GUIDE.md` - **Guia completo de instalação**
- `DEPLOY-GUIDE.md` - Guia detalhado de deployment
- `README.md` - Documentação do projeto

## 🚀 Instalação Rápida (Recomendado)

### Pré-requisitos

1. **Windows Server 2019/2022** ou Windows 10/11 Pro
2. **Privilégios de Administrador**
3. **Conexão com internet** (para download de dependências)

### Passos

1. **Extrair** o pacote para `C:\Deploy\IntranetDocumentos`
2. **Executar como Administrador**:

   ```batch
   deploy-quick.bat
   ```

3. **Seguir** as instruções do instalador
4. **Configurar** as senhas no arquivo `appsettings.Production.json`
5. **Testar** acessando `http://localhost`

## 🛠️ Instalação Manual

### 1. Preparar Ambiente

```powershell
# Execute como Administrador
.\Deploy-WindowsServer.ps1
```

### 2. Configurar IIS

```powershell
# Configure IIS e Application Pool
.\Configuracao-IIS.ps1
```

### 3. Configurar Banco de Dados

- Instale **MySQL 8.0+** ou **MariaDB 10.5+**
- Execute o script: `setup-mysql.sql`

### 4. Configurar Aplicação

- Edite `appsettings.Production.json`
- Configure connection string
- Configure SMTP para emails

### 5. Verificar Instalação

```powershell
# Verificar se tudo está funcionando
.\Verificacao-Pos-Instalacao.ps1
```

## ⚙️ Configurações Importantes

### Connection String

```json
"DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=app_user;Password=SUA_SENHA;Port=3306;SslMode=Preferred;CharSet=utf8mb4;"
```

### Email SMTP

```json
"EmailSettings": {
  "SmtpServer": "smtp.empresa.com",
  "SmtpPort": 587,
  "SmtpUsername": "intranet@empresa.com",
  "SmtpPassword": "SUA_SENHA_EMAIL",
  "EnableSsl": true
}
```

### Diretórios de Dados

- **Documentos**: `C:\IntranetData\Documents`
- **Backups**: `C:\IntranetData\Backups`
- **Logs**: `C:\IntranetData\Logs`

## 🔐 Segurança

### Usuário Padrão

- **Email**: `admin@empresa.com`
- **Senha**: `Admin123!`
- **⚠️ ALTERE** esta senha na primeira utilização!

### Configurações de Segurança

- HTTPS configurado automaticamente
- Headers de segurança no `web.config`
- Políticas de senha configuráveis
- Controle de acesso por departamento

## 📋 Checklist Pós-Instalação

- [ ] ✅ Aplicação acessível via navegador
- [ ] ✅ Login administrativo funcionando
- [ ] ✅ Upload de documentos testado
- [ ] ✅ Download de documentos testado
- [ ] ✅ Emails sendo enviados (opcional)
- [ ] ✅ Backup automático configurado
- [ ] ✅ HTTPS configurado (recomendado)
- [ ] ✅ Senhas padrão alteradas
- [ ] ✅ Usuários e departamentos criados

## 🐛 Problemas Comuns

### Erro 500.30

**Causa**: .NET 9.0 não instalado ou mal configurado
**Solução**:

1. Instalar [.NET 9.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Reiniciar IIS: `iisreset`

### Erro de Conexão com Banco

**Causa**: MySQL não acessível ou senha incorreta
**Solução**:

1. Verificar se MySQL está rodando
2. Testar: `mysql -u app_user -p`
3. Verificar firewall (porta 3306)

### Upload de Arquivos Falha

**Causa**: Permissões insuficientes
**Solução**:

```cmd
icacls "C:\IntranetData" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

## 📞 Suporte

### Logs para Diagnóstico

- **Event Viewer**: Windows Logs → Application → IntranetDocumentos
- **IIS Logs**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **Application Logs**: `C:\IntranetData\Logs\`

### Comandos Úteis

```powershell
# Status do Application Pool
Get-IISAppPool -Name "IntranetDocumentos"

# Reiniciar Application Pool
Restart-WebAppPool -Name "IntranetDocumentos"

# Verificar processos
Get-Process -Name "w3wp"

# Testar conectividade
Test-NetConnection -ComputerName localhost -Port 80
```

## 📚 Mais Informações

Para informações detalhadas, consulte:

- **INSTALL-GUIDE.md** - Guia completo de instalação
- **DEPLOY-GUIDE.md** - Guia avançado de deployment  
- **README.md** - Documentação do projeto

---

## 🏆 Instalação Bem-sucedida

Após a instalação, você terá:

- ✅ Sistema de gestão de documentos corporativos
- ✅ Controle de acesso por departamento
- ✅ Sistema de agendamento de reuniões
- ✅ Backup automático configurado
- ✅ Interface moderna e responsiva
- ✅ Analytics e relatórios
- ✅ Sistema de notificações por email

**Acesse**: `http://seu-servidor` e comece a usar!

---

## Tecnologias Utilizadas

Desenvolvido com ASP.NET Core 9.0 + MySQL + Bootstrap 5
