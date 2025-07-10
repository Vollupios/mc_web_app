# Deploy Intranet Documentos - Windows Server

## 📋 Pré-requisitos

### Software necessário:
1. **Windows Server 2019/2022** ou Windows 10/11
2. **.NET 9.0 Hosting Bundle** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
3. **MySQL Server 8.0+** - [Download](https://dev.mysql.com/downloads/mysql/)
4. **IIS (Internet Information Services)**

## 🚀 Instalação Rápida

### 1. Executar Script de Deploy
```powershell
# Execute como Administrador
.\Deploy-WindowsServer.ps1 -MySqlPassword "SuaSenhaSegura123!" -EmailPassword "SuaSenhaEmail456!"
```

### 2. Publicar Aplicação
```bash
# No ambiente de desenvolvimento
dotnet publish -c Release -o ./publish --self-contained false
```

### 3. Copiar Arquivos
Copie o conteúdo da pasta `publish` para `C:\inetpub\wwwroot\IntranetDocumentos\`

### 4. Configurar MySQL
```sql
-- Execute no MySQL como root
mysql -u root -p < setup-mysql.sql
```

### 5. Executar Migrations
```powershell
# Na pasta da aplicação
cd C:\inetpub\wwwroot\IntranetDocumentos
dotnet IntranetDocumentos.dll --environment Production
```

## ⚙️ Configuração Manual

### 1. Configurar IIS
1. Abrir **IIS Manager**
2. Criar **Application Pool**:
   - Nome: `IntranetDocumentos`
   - .NET CLR Version: `No Managed Code`
   - Managed Pipeline Mode: `Integrated`
3. Criar **Site**:
   - Site Name: `Intranet Documentos`
   - Physical Path: `C:\inetpub\wwwroot\IntranetDocumentos`
   - Port: `80`
   - Application Pool: `IntranetDocumentos`

### 2. Configurar Permissões
```powershell
# Permissões para Application Pool Identity
icacls "C:\IntranetData" /grant "IIS AppPool\IntranetDocumentos:(OI)(CI)F" /T
icacls "C:\inetpub\wwwroot\IntranetDocumentos" /grant "IIS AppPool\IntranetDocumentos:(OI)(CI)RX" /T
```

### 3. Configurar appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;User=app_user;Password=SuaSenhaSegura123!;Port=3306;SslMode=Preferred;CharSet=utf8mb4;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.suaempresa.com",
    "SmtpPort": 587,
    "SmtpUsername": "intranet@suaempresa.com",
    "SmtpPassword": "SuaSenhaEmail456!",
    "EnableSsl": true,
    "FromName": "Intranet Documentos",
    "FromEmail": "intranet@suaempresa.com"
  }
}
```

## 🔧 Estrutura de Pastas

```
C:\inetpub\wwwroot\IntranetDocumentos\    # Aplicação
├── IntranetDocumentos.dll
├── appsettings.json
├── appsettings.Production.json
├── web.config
└── wwwroot\

C:\IntranetData\                          # Dados
├── Documents\                            # Documentos uploadeados
├── Backups\                             # Backups automáticos
└── Logs\                                # Logs da aplicação
```

## 🗄️ Configuração MySQL

### Criar usuário e banco:
```sql
CREATE DATABASE IntranetDocumentos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'app_user'@'localhost' IDENTIFIED BY 'SuaSenhaSegura123!';
GRANT ALL PRIVILEGES ON IntranetDocumentos.* TO 'app_user'@'localhost';
FLUSH PRIVILEGES;
```

### Executar migrations:
```powershell
cd C:\inetpub\wwwroot\IntranetDocumentos
dotnet IntranetDocumentos.dll --environment Production
```

## 🔐 Configuração SSL (Recomendado)

### 1. Obter Certificado SSL
- Certificado de autoridade certificadora
- Certificado autoassinado para testes

### 2. Configurar HTTPS no IIS
1. Instalar certificado no Windows
2. No IIS Manager, adicionar binding HTTPS
3. Selecionar certificado
4. Configurar redirecionamento HTTP → HTTPS

### 3. Forçar HTTPS na aplicação
A aplicação já está configurada para redirecionar HTTP para HTTPS automaticamente.

## 📊 Monitoramento e Logs

### Event Viewer
- Abrir **Visualizador de Eventos**
- Navegar para **Logs do Windows** > **Aplicativo**
- Filtrar por fonte: `IntranetDocumentos`

### Logs de arquivo
- Logs da aplicação: `C:\IntranetData\Logs\`
- Logs do IIS: `C:\inetpub\logs\LogFiles\`

## 🔄 Backup e Manutenção

### Backup Automático
A aplicação possui backup automático configurado:
- **Banco de dados**: Backup diário automático
- **Arquivos**: Copie `C:\IntranetData\Documents\` regularmente

### Backup Manual
```sql
-- Backup do banco
mysqldump -u app_user -p IntranetDocumentos > backup.sql

-- Restore do banco  
mysql -u app_user -p IntranetDocumentos < backup.sql
```

## 🚨 Solução de Problemas

### Aplicação não inicia
1. Verificar logs no Event Viewer
2. Verificar se .NET 9.0 está instalado
3. Verificar permissões da Application Pool Identity

### Erro de conexão com MySQL
1. Verificar se MySQL está rodando
2. Testar conexão: `mysql -u app_user -p -h localhost`
3. Verificar string de conexão no appsettings.Production.json

### Upload de arquivos falha
1. Verificar permissões em `C:\IntranetData\Documents\`
2. Verificar se pasta existe
3. Verificar configuração de tamanho máximo no IIS

### Emails não são enviados
1. Verificar configurações SMTP no appsettings.Production.json
2. Testar conectividade com servidor SMTP
3. Verificar logs da aplicação

## 📞 Suporte

Para problemas técnicos:
1. Verificar logs no Event Viewer
2. Verificar logs da aplicação em `C:\IntranetData\Logs\`
3. Verificar configurações no `appsettings.Production.json`

## 🔄 Atualizações

Para atualizar a aplicação:
1. Parar o site no IIS
2. Fazer backup dos arquivos atuais
3. Copiar novos arquivos (preservar appsettings.Production.json)
4. Executar migrations se necessário
5. Reiniciar o site no IIS
