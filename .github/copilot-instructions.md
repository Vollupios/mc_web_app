<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Intranet Documentos - Instruções para Copilot

Este é um projeto ASP.NET Core MVC para gestão de documentos corporativos.

## Arquitetura e Padrões

- **Framework**: ASP.NET Core 9.0 MVC
- **ORM**: Entity Framework Core 9.0 com SQL Server
- **Autenticação**: ASP.NET Core Identity
- **Padrão**: Repository/Service pattern
- **Interface**: Bootstrap 5 com Bootstrap Icons

## Convenções do Projeto

### Estrutura de Pastas
- `Controllers/`: Controladores MVC (Account, Documents, Admin)
- `Models/`: Entidades e ViewModels
- `Services/`: Lógica de negócio (IDocumentService, DocumentService)
- `Data/`: Contexto EF Core
- `Views/`: Views Razor organizadas por controller
- `DocumentsStorage/`: Armazenamento físico de arquivos (fora da wwwroot)

### Roles e Permissões
- **Admin**: Acesso total + gerenciamento de usuários (TI)
- **Gestor**: Acesso a todos os documentos
- **Usuario**: Acesso ao próprio departamento + área Geral

### Departamentos
- Pessoal, Fiscal, Contábil, Cadastro, Apoio, TI
- **Geral**: Acessível por todos os usuários

## Padrões de Código

### Controllers
- Sempre usar `[Authorize]` nas ações que precisam autenticação
- Usar `[Authorize(Roles = "Admin")]` para ações administrativas
- Injetar dependências via construtor
- Validar permissões do usuário antes de operações

### Services
- Implementar interfaces para abstração
- Usar async/await para operações I/O
- Tratar exceções adequadamente
- Implementar validações de negócio

### Views
- Usar Bootstrap 5 para layout responsivo
- Incluir Bootstrap Icons para ícones
- Implementar validação client-side
- Usar TempData para mensagens de sucesso/erro

### Segurança
- Validar uploads de arquivo (tipo, tamanho)
- Armazenar arquivos fora da wwwroot
- Usar nomes únicos (GUID) para arquivos
- Verificar permissões antes de download/exclusão

## Convenções de Nomenclatura

### C#
- PascalCase para classes, métodos, propriedades
- camelCase para variáveis locais
- Prefixar interfaces com 'I'
- Sufixar ViewModels com 'ViewModel'

### HTML/CSS
- Usar classes Bootstrap padrão
- IDs em camelCase
- Classes CSS customizadas em kebab-case

## Banco de Dados

### Entidades Principais
- `ApplicationUser`: Extends IdentityUser
- `Department`: Departamentos da empresa
- `Document`: Metadados dos documentos

### Relacionamentos
- User -> Department (N:1)
- User -> Documents (1:N) - uploader
- Department -> Documents (1:N)

## Funcionalidades Específicas

### Upload de Documentos
- Máximo 10MB por arquivo
- Tipos permitidos: PDF, Office, imagens, texto, ZIP
- Organização por departamentos
- Validação de permissões por departamento

### Download de Documentos
- Streaming para arquivos grandes
- Controle de acesso baseado em departamento/role
- Log de downloads (se necessário)

## Tratamento de Erros

- Usar try-catch em operações críticas
- Retornar mensagens amigáveis ao usuário
- Log de erros para diagnóstico
- Páginas de erro customizadas

## Localização

- Interface em português brasileiro
- Mensagens de erro em português
- Formatos de data/hora brasileiros
- Validações em português
