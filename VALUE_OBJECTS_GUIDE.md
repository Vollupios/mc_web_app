# Value Objects - Guia de Implementação

## Visão Geral

Este documento descreve a implementação e uso dos Value Objects na aplicação Intranet Documentos. Os Value Objects são um padrão do Domain-Driven Design (DDD) que representam conceitos do domínio através de objetos imutáveis focados em valor ao invés de identidade.

## Value Objects Implementados

### 1. Email (Email.cs)
Representa e valida endereços de email.

```csharp
// Criação
var email = Email.Create("user@example.com");

// Validação automática
Email.TryCreate("invalid-email", out var emailVO); // retorna false

// Métodos úteis
email.GetDomain(); // "example.com"
email.GetLocalPart(); // "user"
email.IsFromDomain("example.com"); // true
```

**Integração no ApplicationUser:**
```csharp
user.SetEmail("user@company.com");
var emailVO = user.GetEmailValueObject();
```

### 2. FileSize (FileSize.cs)
Representa tamanhos de arquivo com formatação humana e operações aritméticas.

```csharp
// Criação
var size = FileSize.FromBytes(1024);
var sizeMB = FileSize.FromMegabytes(10);

// Formatação
size.ToHumanReadableString(); // "1.0 KB"
size.ToDetailedString(); // "1.0 KB (1,024 bytes)"

// Operações
var total = size1 + size2;
var difference = size2 - size1;

// Comparação
if (fileSize > maxSize) { /* arquivo muito grande */ }
```

**Integração no Document:**
```csharp
document.SetFileSize(FileSize.FromBytes(uploadedFile.Length));
var formattedSize = document.GetFormattedFileSize(); // "2.5 MB"
```

### 3. PhoneNumber (PhoneNumber.cs)
Representa e formata números de telefone brasileiros.

```csharp
// Criação
var phone = PhoneNumber.Create("11987654321");

// Validação e formatação
phone.ToFormattedString(); // "(11) 98765-4321"
phone.IsMobile(); // true
phone.GetAreaCode(); // "11"
```

**Integração no Ramal:**
```csharp
ramal.SetPhoneNumber(PhoneNumber.Create("1134567890"));
var formatted = ramal.GetFormattedPhoneNumber(); // "(11) 3456-7890"
```

### 4. DocumentChecksum (DocumentChecksum.cs)
Representa checksums/hashes de documentos para integridade.

```csharp
// Criação a partir de conteúdo
var checksum = DocumentChecksum.FromContent("file content");

// Criação a partir de arquivo
var checksum = DocumentChecksum.FromFile(filePath);

// Verificação
checksum.Verify("file content"); // true/false
checksum.VerifyFile(filePath); // true/false
```

### 5. Money (Money.cs)
Representa valores monetários com moeda.

```csharp
// Criação
var amount = Money.Create(100.50m, "BRL");

// Operações (mesma moeda)
var total = amount1 + amount2;
var tax = amount * 0.1m;

// Formatação
amount.ToString(); // "R$ 100,50" (para BRL)
```

### 6. StatusValue (StatusValue.cs)
Representa status/estados do sistema com validações de transição.

```csharp
// Status pré-definidos
var draft = StatusValue.Draft;
var published = StatusValue.Published;

// Validação de transições
draft.IsValidTransition(StatusValue.PendingReview); // true
draft.CanBeModified(); // true
published.CanBeModified(); // false
```

## Padrões de Uso

### 1. Criação e Validação
```csharp
// Pattern: Factory methods com validação
try 
{
    var email = Email.Create(userInput);
    // Use email safely
}
catch (ArgumentException ex)
{
    // Handle invalid input
}

// Pattern: TryCreate para validação sem exceção
if (PhoneNumber.TryCreate(userInput, out var phone))
{
    // Use phone safely
}
```

### 2. Integração com Entity Framework
Os Value Objects podem ser usados como propriedades computadas ou em métodos helper:

```csharp
public class Document
{
    public long FileSize { get; set; } // Stored as primitive
    
    // Helper methods using Value Objects
    public ValueObjects.FileSize GetFileSizeValueObject()
    {
        return ValueObjects.FileSize.FromBytes(FileSize);
    }
    
    public void SetFileSize(ValueObjects.FileSize fileSize)
    {
        FileSize = fileSize.Bytes;
    }
}
```

### 3. Uso em Controllers
```csharp
// Validação aprimorada
var fileSize = FileSize.FromBytes(uploadedFile.Length);
var maxSize = FileSize.FromMegabytes(10);

if (fileSize > maxSize)
{
    ModelState.AddModelError("File", 
        $"Arquivo muito grande: {fileSize.ToHumanReadableString()}. " +
        $"Máximo: {maxSize.ToHumanReadableString()}");
}
```

### 4. Uso em Views
```html
<!-- Antes: método helper manual -->
@GetFileSize(document.FileSize)

<!-- Depois: Value Object -->
@document.GetFormattedFileSize()
```

## Vantagens dos Value Objects

### 1. **Encapsulamento de Validação**
- Validação centralizada e consistente
- Impossível criar objetos inválidos
- Reduz duplicação de código de validação

### 2. **Expressividade do Domínio**
- Código mais legível e expressivo
- Operações específicas do domínio (ex: `email.IsFromDomain()`)
- Melhor documentação através do código

### 3. **Imutabilidade**
- Objetos imutáveis por design
- Thread-safe
- Reduz bugs relacionados a estado mutável

### 4. **Operações Específicas**
- Operações aritméticas para FileSize e Money
- Formatação contextual (ex: telefones brasileiros)
- Comparações type-safe

### 5. **Reutilização**
- Value Objects podem ser usados em múltiplos contextos
- Reduz duplicação de lógica de domínio
- Facilita testes unitários

## Testes Unitários

Exemplo de testes para Value Objects:

```csharp
[Fact]
public void FileSize_Should_Support_Arithmetic_Operations()
{
    // Arrange
    var size1 = FileSize.FromMegabytes(1);
    var size2 = FileSize.FromMegabytes(2);

    // Act
    var sum = size1 + size2;

    // Assert
    Assert.Equal(3, sum.Megabytes);
}
```

## Considerações de Performance

1. **Criação de Objetos**: Value Objects são pequenos e imutáveis, impacto mínimo
2. **Validação**: Validação ocorre apenas na criação, não a cada uso
3. **Comparação**: Implementação otimizada de `GetHashCode()` e `Equals()`
4. **Conversão**: Conversões implícitas facilitam interoperabilidade

## Próximos Passos

1. **Expandir Uso**: Integrar Value Objects em mais partes da aplicação
2. **Novos Value Objects**: Criar para outros conceitos (CPF, CNPJ, CEP, etc.)
3. **Persistência**: Considerar usar Value Objects diretamente no EF Core com conversores
4. **Validação**: Integrar com FluentValidation para validações mais complexas

## Arquivos Relacionados

- `Models/ValueObjects/ValueObject.cs` - Classe base abstrata
- `Models/ValueObjects/Email.cs` - Value Object para emails
- `Models/ValueObjects/FileSize.cs` - Value Object para tamanhos de arquivo
- `Models/ValueObjects/PhoneNumber.cs` - Value Object para telefones
- `Models/ValueObjects/DocumentChecksum.cs` - Value Object para checksums
- `Models/ValueObjects/Money.cs` - Value Object para valores monetários
- `Models/ValueObjects/StatusValue.cs` - Value Object para status/estados
- `Tests/ValueObjects/ValueObjectsTests.cs` - Testes unitários

Esta implementação segue as melhores práticas do DDD e melhora significativamente a qualidade e expressividade do código do domínio.
