# Intranet Documentos - Sistema de Gestão de Documentos

**Sistema web desenvolvido em ASP.NET Core MVC para gestão de documentos corporativos**

---

## 📚 Documentação e Instalação

### 🚀 **GUIA FINAL UNIFICADO**

**👉 [GUIA-UNIFICADO-FINAL.md](GUIA-UNIFICADO-FINAL.md) - COMECE AQUI**

> **Guia completo e definitivo que contém TUDO:**
>
> - ⚡ Instalação rápida (1 comando)
> - 🛠️ Instalação manual detalhada  
> - ⚙️ Configuração de produção
> - 🌐 Deploy remoto
> - 🔍 Verificação e diagnóstico
> - 🚨 Solução de problemas
> - 💾 Backup e manutenção
> - 📁 Scripts e arquivos
> - ✅ Checklist completo

### 📖 **Documentação Técnica**

- **[INDICE-DOCUMENTACAO.md](INDICE-DOCUMENTACAO.md)** - Índice de toda documentação
- **[STATUS-FINAL.md](STATUS-FINAL.md)** - Status atual e problemas corrigidos

### ⚡ **Instalação Rápida (TL;DR)**

```batch
# Execute como Administrador no Windows Server
deploy-quick.bat
```

---

Sistema web desenvolvido em ASP.NET Core MVC para gestão de documentos corporativos da Marcos Contabilidade.

---

## � Documentação Completa

### 🚀 **Para Instalação e Deploy**

👉 **[GUIA-COMPLETO.md](GUIA-COMPLETO.md)** - Guia unificado completo de instalação e deploy

### 📖 **Documentação Técnica**

- [README.md](README.md) - Este arquivo (visão geral do projeto)
- [STATUS-FINAL.md](STATUS-FINAL.md) - Status atual e problemas corrigidos

---

## 🚀 Funcionalidades

### 📄 **Gestão de Documentos**

- Upload de documentos por departamento
- Controle de acesso baseado em roles e departamentos
- Download seguro de arquivos
- Tipos suportados: PDF, Office, imagens, texto, ZIP
- Limite de 10MB por arquivo

### 🤝 **Sistema de Reuniões**

- Agendamento de reuniões internas e externas
- Controle de salas e veículos
- Gestão de participantes
- Status de reuniões (Agendada, Em Andamento, Concluída, Cancelada)

### 📞 **Lista de Ramais**

- Cadastro de ramais corporativos
- Organização por departamentos
- Fotos dos funcionários
- Status ativo/inativo
- Fotos dos funcionários
- Status ativo/inativo

### 📧 **Sistema de Email e Notificações**

- Configuração SMTP para diferentes provedores (Gmail, Outlook, Office365)
- Envio de emails administrativos para grupos de usuários
- Notificações automáticas para novos documentos
- Lembretes de reuniões
- Pré-visualização de emails antes do envio

### 👥 **Controle de Usuários**

- Sistema de autenticação com ASP.NET Core Identity
- Três níveis de acesso: Admin, Gestor, Usuario
- Controle por departamentos

### 🔧 Administração

- Área administrativa para gerenciamento
- Backup automático do banco de dados
- Logs de atividades

## 🏗️ Tecnologias

- **Framework**: ASP.NET Core 9.0 MVC
- **Banco de Dados**: SQLite com Entity Framework Core 9.0
- **Autenticação**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5 + Bootstrap Icons
- **Arquitetura**: Repository/Service Pattern

## 📁 Estrutura do Projeto

```
IntranetDocumentos/
├── Controllers/           # Controladores MVC
├── Models/               # Entidades e ViewModels
├── Services/             # Lógica de negócio
├── Data/                 # Contexto Entity Framework
├── Views/                # Views Razor
├── wwwroot/              # Arquivos estáticos
├── DocumentsStorage/     # Armazenamento de arquivos
├── DatabaseBackups/      # Backups automáticos
└── Migrations/           # Migrações do banco
```

## 🏢 Departamentos

- **Pessoal**: Recursos humanos
- **Fiscal**: Questões fiscais
- **Contábil**: Contabilidade
- **Cadastro**: Cadastros diversos
- **Apoio**: Suporte geral
- **TI**: Tecnologia da informação
- **Geral**: Acessível por todos os usuários

## 👤 Roles e Permissões

### Admin (TI)

- Acesso total ao sistema
- Gerenciamento de usuários
- Área administrativa
- Acesso a todos os departamentos

### Gestor

- Acesso a todos os documentos
- Gerenciamento de reuniões
- Visualização de relatórios

### Usuario

- Acesso ao próprio departamento
- Acesso à área Geral
- Upload e download de documentos

## 🚀 Como Executar

### Pré-requisitos

- .NET 9.0 SDK
- Visual Studio Code ou Visual Studio

### Passos

1. **Clone o repositório**

   ```bash
   git clone <repository-url>
   cd mc_web_app-main
   ```

2. **Execute a aplicação**

   ```bash
   # Opção 1: Comando direto
   dotnet run --project IntranetDocumentos.csproj
   
   # Opção 2: Script PowerShell
   .\run-app.ps1
   ```

3. **Acesse a aplicação**
   - HTTP: <http://localhost:5098>
   - HTTPS: <https://localhost:7168>

### Login Padrão

- **Email**: <admin@intranet.com>
- **Senha**: Admin123!

## 🗄️ Banco de Dados

O sistema utiliza SQLite com Entity Framework Core. O banco é criado automaticamente na primeira execução com dados de exemplo.

### Scripts Disponíveis

- `run-app.ps1` - Inicia a aplicação
- `backup-database.ps1` - Backup manual do banco
- `recreate-database.ps1` - Recria o banco (desenvolvimento)
- `start-app.ps1` - Script alternativo de inicialização

## 🔒 Segurança

- Autenticação baseada em cookies
- Validação de tipos de arquivo
- Armazenamento seguro fora da wwwroot
- Controle de acesso por departamento
- Logs de download de documentos

## 📋 CI/CD

O projeto inclui workflows do GitHub Actions para:

- Build automático
- Testes unitários
- Análise de segurança
- Deploy automático (quando configurado)

## 🛠️ Desenvolvimento

### Comandos Úteis

```bash
# Build do projeto
dotnet build IntranetDocumentos.csproj

# Executar testes
dotnet test

# Criar nova migração
dotnet ef migrations add <NomeMigracao> --project IntranetDocumentos.csproj

# Aplicar migrações
dotnet ef database update --project IntranetDocumentos.csproj
```

### Estrutura de Pastas Importantes

- `/DocumentsStorage/` - Arquivos de documentos (fora da wwwroot por segurança)
- `/DatabaseBackups/` - Backups automáticos do banco
- `/wwwroot/images/` - Imagens do sistema e fotos de usuários

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença [MIT](LICENSE).

## 📞 Suporte

Para suporte, entre em contato com a equipe de TI da Marcos Contabilidade.
