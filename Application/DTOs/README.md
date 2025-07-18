# DTOs (Data Transfer Objects) - Intranet Documentos

## 📋 Visão Geral

Os DTOs (Data Transfer Objects) são objetos que carregam dados entre processos, especialmente entre a camada de aplicação e a camada de apresentação. Eles são uma implementação do padrão DTO que visa:

- **Desacoplamento**: Separar a estrutura interna dos dados da API externa
- **Validação**: Centralizar as regras de validação de entrada
- **Segurança**: Controlar quais dados são expostos
- **Versionamento**: Facilitar mudanças na API sem afetar modelos internos
- **Performance**: Reduzir a quantidade de dados transferidos

## 🏗️ Estrutura Organizacional

```text
Application/
├── DTOs/
│   ├── Common/             # DTOs base e utilitários
│   ├── Documents/          # DTOs para documentos e pastas
│   ├── Users/              # DTOs para usuários e autenticação
│   ├── Departments/        # DTOs para departamentos
│   ├── Ramais/             # DTOs para ramais telefônicos
│   ├── Reunioes/           # DTOs para reuniões
│   ├── Workflow/           # DTOs para fluxo de trabalho
│   └── Analytics/          # DTOs para analytics e relatórios
└── Mappers/
    └── DTOMapper.cs        # Mapeamento entre DTOs e Models
```

## 📦 Categorias de DTOs

### 1. **Common DTOs** (`/Common/`)
- `BaseDTO`: Classe base para todos os DTOs
- `OperationResultDTO`: Resposta padrão para operações
- `PaginationDTO`: Paginação de resultados
- `PagedResultDTO<T>`: Resultados paginados tipados

### 2. **Document DTOs** (`/Documents/`)
- `CreateDocumentDTO`: Criação de documentos
- `UpdateDocumentDTO`: Atualização de documentos
- `DocumentDTO`: Resposta completa de documento
- `DocumentDownloadDTO`: Download de documentos
- `MoveDocumentDTO`: Movimentação de documentos
- `DocumentSearchDTO`: Filtros de busca
- `DocumentStatisticsDTO`: Estatísticas de documentos

### 3. **Folder DTOs** (`/Documents/`)
- `CreateFolderDTO`: Criação de pastas
- `UpdateFolderDTO`: Atualização de pastas
- `FolderDTO`: Resposta completa de pasta
- `FolderTreeDTO`: Árvore de pastas
- `FolderNavigationDTO`: Navegação de pastas
- `BreadcrumbDTO`: Breadcrumbs de navegação

### 4. **User DTOs** (`/Users/`)
- `CreateUserDTO`: Criação de usuários
- `UpdateUserDTO`: Atualização de usuários
- `UserDTO`: Resposta de usuário
- `LoginDTO`: Login de usuário
- `LoginResponseDTO`: Resposta de login
- `ChangePasswordDTO`: Alteração de senha

### 5. **Department DTOs** (`/Departments/`)
- `CreateDepartmentDTO`: Criação de departamentos
- `UpdateDepartmentDTO`: Atualização de departamentos
- `DepartmentDTO`: Resposta de departamento
- `DepartmentStatisticsDTO`: Estatísticas de departamento
- `DepartmentActivityDTO`: Atividade do departamento

### 6. **Ramal DTOs** (`/Ramais/`)
- `CreateRamalDTO`: Criação de ramais
- `UpdateRamalDTO`: Atualização de ramais
- `RamalDTO`: Resposta de ramal
- `RamalSearchDTO`: Filtros de busca
- `RamalStatisticsDTO`: Estatísticas de ramais

### 7. **Reunião DTOs** (`/Reunioes/`)
- `CreateReuniaoDTO`: Criação de reuniões
- `UpdateReuniaoDTO`: Atualização de reuniões
- `ReuniaoDTO`: Resposta de reunião
- `ParticipanteDTO`: Participantes da reunião
- `CalendarioReuniaoDTO`: Calendário de reuniões
- `ReuniaoStatisticsDTO`: Estatísticas de reuniões

### 8. **Workflow DTOs** (`/Workflow/`)
- `WorkflowActionDTO`: Ações de workflow
- `DocumentWorkflowDTO`: Workflow de documentos
- `WorkflowHistoryDTO`: Histórico de workflow
- `WorkflowDashboardDTO`: Dashboard de workflow
- `WorkflowStatisticsDTO`: Estatísticas de workflow

### 9. **Analytics DTOs** (`/Analytics/`)
- `DashboardAnalyticsDTO`: Dashboard principal
- `DocumentAnalyticsDTO`: Analytics de documentos
- `ReuniaoAnalyticsDTO`: Analytics de reuniões
- `DepartmentAnalyticsDTO`: Analytics de departamentos
- `UserAnalyticsDTO`: Analytics de usuários
- `SystemAnalyticsDTO`: Analytics do sistema

## 🎯 Padrões de Nomenclatura

### Convenções de Nomes:
- **Create**: `Create{Entity}DTO` - Para criação de entidades
- **Update**: `Update{Entity}DTO` - Para atualização de entidades
- **Response**: `{Entity}DTO` - Para respostas de API
- **Search**: `{Entity}SearchDTO` - Para filtros de busca
- **Statistics**: `{Entity}StatisticsDTO` - Para estatísticas
- **Action**: `{Action}{Entity}DTO` - Para ações específicas

### Sufixos Comuns:
- `DTO` - Data Transfer Object
- `Response` - Resposta de API
- `Request` - Requisição de API
- `Filter` - Filtros de busca
- `Stats` - Estatísticas
- `Summary` - Resumo de dados

## 🔧 Características dos DTOs

### 1. **Validação**
```csharp
public class CreateDocumentDTO
{
    [Required(ErrorMessage = "O arquivo é obrigatório")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "Selecione um departamento")]
    public int DepartmentId { get; set; }

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    public string? Description { get; set; }
}
```

### 2. **Mapeamento**
```csharp
public static DocumentDTO ToDTO(this Document document)
{
    return new DocumentDTO
    {
        Id = document.Id,
        OriginalFileName = document.OriginalFileName,
        // ... outros campos
    };
}
```

### 3. **Propriedades Calculadas**
```csharp
public class DocumentDTO
{
    public long FileSize { get; set; }
    public string FileSizeFormatted => FormatBytes(FileSize);
    
    public DateTime? DueDate { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now;
}
```

### 4. **Enumerações**
```csharp
public enum DocumentStatus
{
    Active = 1,
    Archived = 2,
    Deleted = 3,
    Pending = 4,
    InReview = 5,
    Approved = 6,
    Rejected = 7
}
```

## 📊 Vantagens dos DTOs

### 1. **Segurança**
- Controla exposição de dados sensíveis
- Previne over-posting e mass assignment
- Validação centralizada

### 2. **Performance**
- Reduz payload de rede
- Projeta apenas dados necessários
- Cacheable em diferentes camadas

### 3. **Manutenibilidade**
- Separação clara de responsabilidades
- Facilita refatoração
- Versionamento de API

### 4. **Testabilidade**
- Objetos simples e testáveis
- Mocking facilitado
- Validação isolada

## 🔄 Fluxo de Dados

```text
Controller → Service → Repository → Entity
    ↓           ↓          ↓           ↓
   DTO    →   DTO    →   DTO    →   Model
    ↓           ↓          ↓           ↓
Response  ←  Response ←  Response ←  DTO
```

## 🛠️ Uso Prático

### 1. **Em Controllers**
```csharp
[HttpPost]
public async Task<IActionResult> CreateDocument(CreateDocumentDTO dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _documentService.CreateAsync(dto);
    return Ok(result);
}
```

### 2. **Em Services**
```csharp
public async Task<OperationResultDTO<DocumentDTO>> CreateAsync(CreateDocumentDTO dto)
{
    var document = dto.ToEntity(userId, fileName, checksum);
    await _repository.AddAsync(document);
    
    return new OperationResultDTO<DocumentDTO>
    {
        Success = true,
        Data = document.ToDTO()
    };
}
```

### 3. **Em Repositories**
```csharp
public async Task<PagedResultDTO<DocumentDTO>> SearchAsync(DocumentSearchDTO search)
{
    var query = _context.Documents.AsQueryable();
    
    if (!string.IsNullOrEmpty(search.Query))
        query = query.Where(d => d.OriginalFileName.Contains(search.Query));
    
    var documents = await query.ToListAsync();
    
    return new PagedResultDTO<DocumentDTO>
    {
        Items = documents.ToDTO(),
        Pagination = new PaginationDTO { /* ... */ }
    };
}
```

## 🔍 Boas Práticas

### 1. **Nomenclatura**
- Use nomes descritivos e consistentes
- Prefixe com o tipo de operação (Create, Update, etc.)
- Sufixe sempre com "DTO"

### 2. **Validação**
- Use Data Annotations para validações simples
- Implemente IValidatableObject para validações complexas
- Mensagens de erro em português

### 3. **Propriedades**
- Use propriedades calculadas para dados derivados
- Formate dados para apresentação
- Inclua metadados relevantes

### 4. **Estrutura**
- Agrupe DTOs por contexto/funcionalidade
- Mantenha DTOs simples e focados
- Use herança quando apropriado

### 5. **Mapeamento**
- Centralize lógica de mapeamento
- Use extension methods para conversões
- Mantenha mapeamentos simples

## 🎨 Exemplos de Uso

### Criação de Documento
```csharp
var createDto = new CreateDocumentDTO
{
    File = uploadedFile,
    DepartmentId = 1,
    Description = "Documento importante",
    IsPublic = false,
    Tags = new List<string> { "importante", "fiscal" }
};

var result = await _documentService.CreateAsync(createDto);
```

### Busca de Documentos
```csharp
var searchDto = new DocumentSearchDTO
{
    Query = "relatório",
    DepartmentId = 1,
    UploadDateFrom = DateTime.Now.AddMonths(-1),
    Page = 1,
    PageSize = 20,
    SortBy = "UploadDate",
    SortOrder = "desc"
};

var results = await _documentService.SearchAsync(searchDto);
```

### Estatísticas
```csharp
var stats = await _analyticsService.GetDocumentStatisticsAsync();
Console.WriteLine($"Total de documentos: {stats.TotalDocuments}");
Console.WriteLine($"Armazenamento usado: {stats.TotalStorageFormatted}");
```

---

## 📚 Recursos Adicionais

- **Validação**: Data Annotations, FluentValidation
- **Mapeamento**: AutoMapper, Extension Methods
- **Serialização**: System.Text.Json, Newtonsoft.Json
- **Documentação**: Swagger/OpenAPI

---

*Este documento é parte da arquitetura Clean Architecture + DDD do sistema Intranet Documentos.*
