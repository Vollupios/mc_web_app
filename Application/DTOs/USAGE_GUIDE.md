# Guia de Uso dos DTOs - Intranet Documentos

## Introdução

Os DTOs (Data Transfer Objects) foram implementados para facilitar a transferência de dados entre as camadas da aplicação, seguindo os princípios da Clean Architecture. Este guia demonstra como utilizar os DTOs na prática.

## Estrutura dos DTOs

### 1. Documentos

```csharp
// Para criar um novo documento
var createDto = new DocumentCreateDTO
{
    OriginalFileName = "documento.pdf",
    StoredFileName = "guid_documento.pdf",
    ContentType = "application/pdf",
    FileSize = 1024000,
    Description = "Documento importante",
    DepartmentId = 1,
    FolderId = 5,
    UploaderId = "user123",
    Status = DocumentStatus.Draft,
    Version = 1
};

// Converter para entidade
var document = createDto.ToEntity();

// Converter entidade para DTO
var documentDto = document.ToDTO();
```

### 2. Pastas

```csharp
// Para criar uma nova pasta
var folderCreateDto = new FolderCreateDTO
{
    Name = "Contratos 2024",
    Description = "Contratos do ano de 2024",
    ParentFolderId = 1,
    DepartmentId = 2,
    Color = "#007bff",
    Icon = "bi-folder-fill",
    DisplayOrder = 1,
    IsSystemFolder = false,
    IsActive = true,
    CreatedById = "user123"
};

// Converter para entidade
var folder = folderCreateDto.ToEntity();

// Converter entidade para DTO
var folderDto = folder.ToDTO();
```

### 3. Usuários

```csharp
// Para criar um novo usuário
var userCreateDto = new UserCreateDTO
{
    UserName = "joao.silva",
    Email = "joao.silva@empresa.com",
    FirstName = "João",
    LastName = "Silva",
    DepartmentId = 1,
    IsActive = true,
    EmailConfirmed = true
};

// Converter para entidade
var user = userCreateDto.ToEntity();

// Converter entidade para DTO
var userDto = user.ToDTO();
```

### 4. Departamentos

```csharp
// Para criar um novo departamento
var departmentCreateDto = new DepartmentCreateDTO
{
    Name = "Recursos Humanos"
};

// Converter para entidade
var department = departmentCreateDto.ToEntity();

// Converter entidade para DTO
var departmentDto = department.ToDTO();
```

### 5. Ramais

```csharp
// Para criar um novo ramal
var ramalCreateDto = new RamalCreateDTO
{
    Numero = "1234",
    Nome = "João Silva",
    TipoFuncionario = TipoFuncionario.Normal,
    DepartmentId = 1,
    FotoPath = "/images/funcionarios/joao.jpg",
    Observacoes = "Atende das 8h às 17h",
    Ativo = true
};

// Converter para entidade
var ramal = ramalCreateDto.ToEntity();

// Converter entidade para DTO
var ramalDto = ramal.ToDTO();
```

### 6. Reuniões

```csharp
// Para criar uma nova reunião
var reuniaoCreateDto = new ReuniaoCreateDTO
{
    Titulo = "Reunião de Planejamento",
    DataReuniao = new DateTime(2024, 12, 25),
    HoraInicio = new TimeSpan(14, 0, 0),
    HoraFim = new TimeSpan(16, 0, 0),
    TipoReuniao = TipoReuniao.Interno,
    Sala = SalaReuniao.Sala1,
    Empresa = "Cliente XYZ",
    Status = StatusReuniao.Agendada,
    Observacoes = "Reunião importante",
    ResponsavelUserId = "user123"
};

// Converter para entidade
var reuniao = reuniaoCreateDto.ToEntity();

// Converter entidade para DTO
var reuniaoDto = reuniao.ToDTO();
```

## Uso em Controllers

### Exemplo com DocumentsController

```csharp
[HttpPost]
public async Task<IActionResult> Upload(DocumentCreateDTO dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        // Converter DTO para entidade
        var document = dto.ToEntity();
        
        // Salvar no banco
        await _documentService.CreateAsync(document);
        
        // Retornar DTO da entidade criada
        var resultDto = document.ToDTO();
        
        return Ok(resultDto);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = ex.Message });
    }
}

[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var document = await _documentService.GetByIdAsync(id);
    
    if (document == null)
        return NotFound();
    
    // Converter entidade para DTO
    var dto = document.ToDTO();
    
    return Ok(dto);
}
```

## Uso em Services

### Exemplo com DocumentService

```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    
    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<DocumentDTO> CreateAsync(DocumentCreateDTO dto)
    {
        // Validar DTO
        if (string.IsNullOrEmpty(dto.OriginalFileName))
            throw new ArgumentException("Nome do arquivo é obrigatório");
        
        // Converter para entidade
        var document = dto.ToEntity();
        
        // Salvar no repositório
        await _repository.AddAsync(document);
        await _repository.SaveChangesAsync();
        
        // Retornar DTO
        return document.ToDTO();
    }
    
    public async Task<List<DocumentDTO>> SearchAsync(DocumentSearchDTO searchDto)
    {
        var documents = await _repository.GetAllAsync();
        
        // Aplicar filtros usando extension method
        var filteredDocuments = documents
            .Where(d => d.MatchesSearchCriteria(searchDto))
            .ToList();
        
        // Converter para DTOs
        return filteredDocuments.Select(d => d.ToDTO()).ToList();
    }
}
```

## Uso de DTOs de Busca

### Exemplo de Busca de Documentos

```csharp
var searchDto = new DocumentSearchDTO
{
    FileName = "contrato",
    ContentType = "application/pdf",
    DepartmentId = 1,
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31),
    UploaderId = "user123"
};

var documents = await _documentService.SearchAsync(searchDto);
```

### Exemplo de Busca de Ramais

```csharp
var searchDto = new RamalSearchDTO
{
    Nome = "João",
    Numero = "12",
    DepartmentId = 1,
    TipoFuncionario = TipoFuncionario.Normal,
    Ativo = true
};

var ramais = await _ramalService.SearchAsync(searchDto);
```

## Validação com DTOs

### Usando Data Annotations

```csharp
public class DocumentCreateDTO
{
    [Required(ErrorMessage = "Nome do arquivo é obrigatório")]
    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Tipo de conteúdo é obrigatório")]
    public string ContentType { get; set; } = string.Empty;
    
    [Range(1, long.MaxValue, ErrorMessage = "Tamanho do arquivo deve ser maior que 0")]
    public long FileSize { get; set; }
    
    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string? Description { get; set; }
}
```

### Validação Custom

```csharp
public class DocumentValidator
{
    public static bool ValidateDocumentCreateDTO(DocumentCreateDTO dto, out List<string> errors)
    {
        errors = new List<string>();
        
        // Validar extensão de arquivo
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".png" };
        var extension = Path.GetExtension(dto.OriginalFileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            errors.Add($"Extensão {extension} não é permitida");
        }
        
        // Validar tamanho do arquivo (max 10MB)
        if (dto.FileSize > 10 * 1024 * 1024)
        {
            errors.Add("Arquivo muito grande. Máximo 10MB");
        }
        
        return errors.Count == 0;
    }
}
```

## Uso com Entity Framework

### Exemplo com DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentFolder> DocumentFolders { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Ramal> Ramais { get; set; }
    public DbSet<Reuniao> Reunioes { get; set; }
    
    // Método para buscar documentos com DTOs
    public async Task<List<DocumentDTO>> GetDocumentsByDepartmentAsync(int departmentId)
    {
        return await Documents
            .Where(d => d.DepartmentId == departmentId)
            .Include(d => d.Department)
            .Include(d => d.Uploader)
            .Include(d => d.Folder)
            .Select(d => d.ToDTO())
            .ToListAsync();
    }
}
```

## Melhores Práticas

### 1. Sempre use DTOs para transferência de dados entre camadas

```csharp
// ✅ Correto - usando DTO
public async Task<DocumentDTO> CreateDocumentAsync(DocumentCreateDTO dto)
{
    var document = dto.ToEntity();
    await _repository.AddAsync(document);
    return document.ToDTO();
}

// ❌ Incorreto - expondo entidade diretamente
public async Task<Document> CreateDocumentAsync(Document document)
{
    await _repository.AddAsync(document);
    return document;
}
```

### 2. Use DTOs específicos para cada operação

```csharp
// ✅ Correto - DTOs específicos
public class DocumentCreateDTO { /* propriedades para criação */ }
public class DocumentUpdateDTO { /* propriedades para atualização */ }
public class DocumentResponseDTO { /* propriedades para resposta */ }

// ❌ Incorreto - DTO genérico para tudo
public class DocumentDTO { /* todas as propriedades */ }
```

### 3. Implemente validação nos DTOs

```csharp
public class DocumentCreateDTO
{
    [Required]
    [FileExtensions(Extensions = "pdf,doc,docx,jpg,png")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 10 * 1024 * 1024)] // Max 10MB
    public long FileSize { get; set; }
}
```

### 4. Use AutoMapper para mapeamentos complexos (opcional)

```csharp
// Configuração do AutoMapper
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Document, DocumentDTO>()
            .ForMember(dest => dest.UploaderName, opt => opt.MapFrom(src => src.Uploader.UserName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name));
            
        CreateMap<DocumentCreateDTO, Document>()
            .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.LastModified, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

## Conclusão

Os DTOs implementados fornecem uma camada robusta de transferência de dados que:

- ✅ Separa claramente as responsabilidades entre camadas
- ✅ Facilita a validação de dados
- ✅ Melhora a manutenibilidade do código
- ✅ Permite evolução independente das entidades
- ✅ Facilita testes unitários
- ✅ Melhora a documentação da API

Use este guia como referência para implementar e utilizar os DTOs corretamente em sua aplicação.
