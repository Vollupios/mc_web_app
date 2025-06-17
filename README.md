# 📁 Intranet Documentos

Sistema de gestão de documentos para intranet corporativa desenvolvido em ASP.NET Core MVC com Entity Framework Core e ASP.NET Core Identity.

## 🚀 Funcionalidades

### 👤 Autenticação e Autorização
- **Login seguro** com ASP.NET Core Identity
- **Três níveis de acesso:**
  - **Admin (TI):** Acesso total + gerenciamento de usuários
  - **Gestor:** Acesso a todos os documentos
  - **Usuário:** Acesso ao próprio departamento + área Geral

### 📂 Gestão de Documentos
- **Upload de arquivos** com validação de tipo e tamanho (máx. 10MB)
- **Organização por departamentos:** Pessoal, Fiscal, Contábil, Cadastro, Apoio, TI
- **Área Geral** acessível por todos os usuários
- **Download seguro** com controle de permissões
- **Visualização de detalhes** dos documentos
- **Exclusão** (próprios documentos ou admin)

### 🏢 Departamentos
- Pessoal
- Fiscal  
- Contábil
- Cadastro
- Apoio
- TI
- Geral (acesso universal)

### ⚙️ Administração (TI)
- **Criação de usuários** com atribuição de departamento e função
- **Edição de perfis** de usuários
- **Gerenciamento de permissões**
- **Exclusão de usuários** (exceto admin principal)

## 🏗️ Arquitetura

```
/IntranetDocumentos/
├── 📁 Controllers/           # Controladores MVC
│   ├── AccountController.cs  # Autenticação
│   ├── DocumentsController.cs # Gestão de documentos
│   └── AdminController.cs    # Administração (TI)
├── 📁 Data/                  # Contexto do banco
│   └── ApplicationDbContext.cs
├── 📁 Models/                # Modelos de dados
│   ├── ApplicationUser.cs    # Usuário (extends Identity)
│   ├── Document.cs           # Documento
│   ├── Department.cs         # Departamento
│   └── ViewModels/           # ViewModels
├── 📁 Services/              # Lógica de negócio
│   ├── IDocumentService.cs
│   └── DocumentService.cs
├── 📁 Views/                 # Interfaces do usuário
│   ├── Account/
│   ├── Documents/
│   ├── Admin/
│   └── Shared/
└── 📁 DocumentsStorage/      # Armazenamento físico
    ├── Pessoal/
    ├── Fiscal/
    ├── Contabil/
    ├── Cadastro/
    ├── Apoio/
    ├── TI/
    └── Geral/
```

## 🛠️ Tecnologias

- **ASP.NET Core 9.0** (MVC)
- **Entity Framework Core 9.0** (SQLite)
- **ASP.NET Core Identity** (Autenticação/Autorização)
- **Bootstrap 5** + **Bootstrap Icons** (Interface)
- **C# 12** (.NET 9)

## 📋 Pré-requisitos

- **.NET 9.0 SDK**
- **Visual Studio 2022** ou **VS Code**

## 🚀 Instalação e Configuração

### 1. Clone o repositório
```bash
git clone <url-do-repositorio>
cd IntranetDocumentos
```

### 2. Configuração do Banco de Dados
O projeto usa **SQLite** por padrão (arquivo `IntranetDocumentos.db`).
A configuração está em `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=IntranetDocumentos.db"
  }
}
```

### 3. Executar Migrations (já aplicadas)
```bash
dotnet ef database update
```

### 4. Executar a aplicação
```bash
dotnet run
```
**A aplicação estará disponível em:** `http://localhost:5098`

### 5. Primeiro Acesso
**Usuário Administrador Padrão:**
- **Email:** admin@empresa.com
- **Senha:** Admin123!

## 📱 Como Usar

### Para Administradores (TI)
1. Faça login com a conta admin
2. Acesse **Administração** no menu
3. Crie usuários e atribua departamentos/funções
4. Gerencie documentos de todos os departamentos

### Para Gestores
1. Faça login com sua conta
2. Visualize documentos de todos os departamentos
3. Envie documentos para qualquer departamento
4. Baixe e visualize documentos

### Para Usuários
1. Faça login com sua conta
2. Visualize documentos do seu departamento + Geral
3. Envie documentos para seu departamento ou Geral
4. Baixe documentos permitidos

## 🔒 Segurança

### Controle de Acesso
- **Autenticação obrigatória** para todas as funcionalidades
- **Autorização baseada em roles** (Admin, Gestor, Usuario)
- **Segregação por departamentos**
- **Validação de permissões** em cada operação

### Validações
- **Tipos de arquivo permitidos:** PDF, Word, Excel, PowerPoint, Imagens, Texto, ZIP/RAR
- **Tamanho máximo:** 10MB por arquivo
- **Validação de entrada** em todos os formulários
- **Proteção CSRF** com tokens

### Armazenamento
- **Arquivos fora da wwwroot** (não acessíveis diretamente)
- **Nomes únicos** (GUID) para evitar conflitos
- **Organização por pastas** de departamento

## 🔧 Personalização

### Adicionar Novo Departamento
1. Acesse o banco de dados
2. Insira na tabela `Departments`
3. Crie pasta correspondente em `DocumentsStorage/`

### Modificar Validações
Edite as constantes em:
- `DocumentService.cs` (lógica de negócio)
- `UploadViewModel.cs` (validações de formulário)

### Customizar Interface
- **CSS:** `wwwroot/css/site.css`
- **Layout:** `Views/Shared/_Layout.cshtml`
- **Bootstrap:** Já integrado com temas personalizáveis

## 📊 Banco de Dados

### Principais Tabelas
- **AspNetUsers** (Usuários + ApplicationUser)
- **Departments** (Departamentos)
- **Documents** (Metadados dos documentos)
- **AspNetRoles** (Funções)
- **AspNetUserRoles** (Relacionamento usuário-função)

### Relacionamentos
- **Usuário ↔ Departamento** (N:1)
- **Usuário ↔ Documento** (1:N) - uploader
- **Departamento ↔ Documento** (1:N) - categoria
- **Usuário ↔ Role** (N:N) - permissões

## 🐛 Troubleshooting

### Erro de Connection String
- Verifique se o SQL Server/LocalDB está rodando
- Confirme a string em `appsettings.json`

### Erro de Migrations
```bash
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erro de Permissões de Arquivo
- Verifique permissões da pasta `DocumentsStorage/`
- Execute como administrador se necessário

## 📞 Suporte

Para suporte técnico ou dúvidas:
- **Email:** suporte@empresa.com
- **TI:** Acesse a área de administração
- **Documentação:** Este README

## 📄 Licença

Este projeto é propriedade da empresa e destinado ao uso interno.

---

**Desenvolvido para gestão eficiente de documentos corporativos** 📁✨

## ✅ Status do Projeto

**Status:** ✅ **CONCLUÍDO E TESTADO**

### Implementações Realizadas
- ✅ **Estrutura MVC completa** com Controllers, Models, Views e Services
- ✅ **Autenticação e autorização** com ASP.NET Core Identity
- ✅ **Controle de acesso baseado em roles** (Admin, Gestor, Usuario)
- ✅ **Segregação por departamentos** com regras de negócio
- ✅ **Upload/Download seguro** de documentos com validações
- ✅ **Interface responsiva** com Bootstrap 5
- ✅ **Banco de dados SQLite** configurado e populado
- ✅ **Seed de dados inicial** (roles, departamentos, usuário admin)
- ✅ **Validações de segurança** (CSRF, tipos de arquivo, tamanhos)

### Testes Realizados
- ✅ **Compilação** sem erros
- ✅ **Migrations aplicadas** com sucesso
- ✅ **Aplicação executando** em `http://localhost:5098`
- ✅ **Seed de dados** funcionando (admin criado automaticamente)
- ✅ **Estrutura de pastas** para armazenamento criada
- ✅ **Interface web** acessível e funcional

### Credenciais de Teste
- **Admin**: admin@empresa.com / Admin123!
- **Departamento**: TI
- **Roles**: Admin (acesso total)

---

# 📚 Intranet de Documentos
