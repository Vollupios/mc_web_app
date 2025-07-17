# Arquitetura Orientada a Objetos - Intranet Documentos

## 📐 Estrutura Reorganizada (Clean Architecture + DDD)

```
📁 IntranetDocumentos/
├── 🏛️ Domain/                          # Camada de Domínio (Core Business)
│   ├── 📦 Entities/                    # Entidades de Domínio
│   │   ├── ApplicationUser.cs          # Usuário do sistema
│   │   ├── Department.cs               # Departamento
│   │   ├── Document.cs                 # Documento
│   │   ├── DocumentFolder.cs           # Pasta de documentos
│   │   ├── DocumentDownloadLog.cs      # Log de downloads
│   │   ├── Ramal.cs                    # Ramal telefônico
│   │   └── Reuniao.cs                  # Reunião
│   ├── 💎 ValueObjects/                # Objetos de Valor
│   │   └── [Value Objects existentes]
│   └── 🔌 Interfaces/                  # Contratos do Domínio
│       ├── IDocumentRepository.cs      # Repositório de documentos
│       ├── IDocumentFolderRepository.cs # Repositório de pastas
│       ├── IUserRepository.cs          # Repositório de usuários
│       ├── IDepartmentRepository.cs    # Repositório de departamentos
│       ├── IDocumentWriter.cs          # Interface de escrita
│       ├── IDocumentReader.cs          # Interface de leitura
│       ├── IDocumentSecurity.cs        # Interface de segurança
│       └── IDocumentDownloader.cs      # Interface de download
│
├── 🚀 Application/                     # Camada de Aplicação (Use Cases)
│   ├── 🔧 Services/                    # Serviços de Aplicação
│   │   ├── DocumentService.cs          # Orquestração de documentos
│   │   ├── DocumentFolderService.cs    # Orquestração de pastas
│   │   └── AnalyticsService.cs         # Serviços de analytics
│   └── 📄 DTOs/                        # Data Transfer Objects
│       ├── DocumentDtos.cs             # DTOs para documentos
│       └── FolderDtos.cs               # DTOs para pastas
│
├── 🏗️ Infrastructure/                  # Camada de Infraestrutura
│   ├── 💾 Data/                        # Contexto de dados
│   │   └── ApplicationDbContext.cs     # EF Core Context
│   ├── 📚 Repositories/                # Implementações de repositórios
│   │   ├── DocumentRepository.cs       # Repository concreto
│   │   ├── DocumentFolderRepository.cs # Repository de pastas
│   │   ├── DocumentWriter.cs           # Implementação de escrita
│   │   ├── DocumentReader.cs           # Implementação de leitura
│   │   ├── DocumentSecurity.cs         # Implementação de segurança
│   │   └── DocumentDownloader.cs       # Implementação de download
│   └── 🌐 ExternalServices/            # Serviços externos
│
├── 🎨 Presentation/                    # Camada de Apresentação
│   └── 📋 ViewModels/                  # ViewModels para Views
│       ├── AnalyticsViewModels.cs      # ViewModels de analytics
│       ├── DocumentTreeViewModels.cs   # ViewModels de árvore
│       └── [Outros ViewModels...]
│
└── 🎯 Web Layer/                       # Camada Web (Controllers/Views)
    ├── Controllers/                    # Controladores MVC
    └── Views/                          # Views Razor
```

## 🎯 Princípios SOLID Aplicados

### 1. **Single Responsibility Principle (SRP)**
- ✅ **IDocumentWriter**: Apenas operações de escrita
- ✅ **IDocumentReader**: Apenas operações de leitura  
- ✅ **IDocumentSecurity**: Apenas validações de segurança
- ✅ **DocumentRepository**: Apenas acesso a dados de documentos

### 2. **Open/Closed Principle (OCP)**
- ✅ **Interfaces abstratas**: Extensíveis sem modificação
- ✅ **Repository Pattern**: Novos repositórios sem alterar existentes
- ✅ **Strategy Pattern**: Diferentes estratégias de processamento

### 3. **Liskov Substitution Principle (LSP)**
- ✅ **Implementações intercambiáveis**: Qualquer implementação de `IDocumentRepository` pode substituir outra
- ✅ **Hierarquia bem definida**: Entidades seguem contratos bem definidos

### 4. **Interface Segregation Principle (ISP)**
- ✅ **Interfaces específicas**: `IDocumentWriter` ≠ `IDocumentReader`
- ✅ **Responsabilidades isoladas**: Cliente só depende do que usa

### 5. **Dependency Inversion Principle (DIP)**
- ✅ **Dependência de abstrações**: Controllers dependem de interfaces
- ✅ **Injeção de dependência**: IoC Container gerencia dependências

## 🏗️ Padrões de Design Implementados

### 📚 **Repository Pattern**
```csharp
// Interface no Domain
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int id);
    Task<IEnumerable<Document>> GetByDepartmentAsync(int? departmentId);
    // ...
}

// Implementação na Infrastructure
public class DocumentRepository : IDocumentRepository
{
    // Implementação concreta com EF Core
}
```

### 🎭 **Strategy Pattern**
```csharp
// Diferentes estratégias para processamento de documentos
public interface IFileProcessor
{
    Task<ProcessResult> ProcessAsync(IFormFile file);
}

public class ImageFileProcessor : IFileProcessor { }
public class DocumentFileProcessor : IFileProcessor { }
```

### 🏭 **Factory Pattern**
```csharp
public class FileProcessorFactory
{
    public IFileProcessor CreateProcessor(string fileType)
    {
        return fileType.ToLower() switch
        {
            "image" => new ImageFileProcessor(),
            "document" => new DocumentFileProcessor(),
            _ => new GenericFileProcessor()
        };
    }
}
```

### 📦 **DTO Pattern**
```csharp
// Separação entre entidades de domínio e dados de transporte
public class CreateDocumentDto
{
    public IFormFile File { get; set; }
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
}
```

## 🔗 Benefícios da Nova Estrutura

### 🎯 **Separação de Responsabilidades**
- **Domain**: Regras de negócio puras
- **Application**: Orquestração de use cases
- **Infrastructure**: Detalhes técnicos
- **Presentation**: Interface com usuário

### 🧪 **Testabilidade**
- Interfaces permitem mocking fácil
- Testes unitários isolados por camada
- Testes de integração focados

### 📈 **Manutenibilidade**
- Código organizado por responsabilidade
- Fácil localização de funcionalidades
- Evolução independente das camadas

### 🔌 **Extensibilidade**
- Novos repositórios sem quebrar código existente
- Novas estratégias de processamento
- Integração com novos serviços externos

### ♻️ **Reutilização**
- DTOs reutilizáveis entre controllers
- Serviços de domínio compartilhados
- Repositórios base para diferentes entidades

## 🚀 Próximos Passos

1. ✅ **Estrutura criada** - Pastas e arquivos organizados
2. 🔄 **Migração gradual** - Atualizar referências nos controllers
3. 🧪 **Testes** - Criar testes para nova estrutura
4. 📚 **Documentação** - Atualizar documentação técnica
5. 🔧 **Refinamento** - Ajustes baseados no uso

## 🎯 Conclusão

A nova arquitetura seguindo princípios OOP e Clean Architecture proporciona:

- ✅ **Código mais limpo e organizdo**
- ✅ **Melhor testabilidade e manutenibilidade**
- ✅ **Separação clara de responsabilidades**
- ✅ **Facilidade para evolução e extensão**
- ✅ **Conformidade com padrões da indústria**

Esta estrutura está preparada para crescimento e facilita o desenvolvimento colaborativo seguindo as melhores práticas de engenharia de software.
