# Guia de Localização - Intranet Documentos

## Configuração Atual

A aplicação está configurada para usar **português do Brasil (pt-BR)** como idioma padrão e único da interface do usuário.

### Estrutura de Localização

```
Resources/
├── SharedResource.resx          (Inglês - fallback)
└── SharedResource.pt-BR.resx    (Português - padrão)
```

## Como Usar Localização nas Views

### 1. Injeção do Localizer

Adicione no topo da view:

```csharp
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<SharedResource> Localizer
```

### 2. Usando Strings Localizadas

```html
<!-- Uso básico -->
<h1>@Localizer["Navigation.Home"]</h1>

<!-- Com parâmetros -->
<span>@Localizer["Validation.Required", "Nome"]</span>

<!-- Em atributos -->
<input placeholder="@Localizer["EnterEmail"]" />
```

## Como Usar Localização nos Controllers

### 1. Injeção por Dependência

```csharp
public class MeuController : Controller
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public MeuController(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult MinhaAction()
    {
        var mensagem = _localizer["Messages.Success"];
        TempData["Success"] = mensagem.Value;
        return View();
    }
}
```

## Estrutura de Chaves de Localização

### Navegação
- `Navigation.Home` - Início
- `Navigation.Documents` - Documentos
- `Navigation.Meetings` - Reuniões
- `Navigation.Extensions` - Ramais
- `Navigation.Analytics` - Relatórios
- `Navigation.Admin` - Administração

### Ações Comuns
- `Common.Save` - Salvar
- `Common.Cancel` - Cancelar
- `Common.Delete` - Excluir
- `Common.Edit` - Editar
- `Common.Create` - Criar
- `Common.Upload` - Enviar
- `Common.Download` - Baixar
- `Common.Search` - Pesquisar

### Documentos
- `Documents.Title` - Documentos
- `Documents.Upload` - Enviar Documento
- `Documents.FileName` - Nome do Arquivo
- `Documents.Description` - Descrição
- `Documents.Department` - Departamento

### Reuniões
- `Meetings.Title` - Reuniões
- `Meetings.Schedule` - Agendar Reunião
- `Meetings.Subject` - Assunto
- `Meetings.DateTime` - Data e Hora
- `Meetings.Location` - Local

### Mensagens
- `Messages.Success` - Operação realizada com sucesso!
- `Messages.Error` - Ocorreu um erro. Tente novamente.
- `Messages.ValidationError` - Por favor, corrija os erros abaixo.

### Validação
- `Validation.Required` - O campo {0} é obrigatório.
- `Validation.EmailFormat` - Digite um e-mail válido.
- `Validation.MinLength` - O campo {0} deve ter pelo menos {1} caracteres.

## Adicionando Novas Strings

1. **Abra o arquivo** `Resources/SharedResource.pt-BR.resx`

2. **Adicione uma nova entrada:**
```xml
<data name="MinhaNovaChave" xml:space="preserve">
  <value>Meu texto em português</value>
</data>
```

3. **Use na view ou controller:**
```csharp
@Localizer["MinhaNovaChave"]
```

## Boas Práticas

### 1. Convenção de Nomenclatura
- Use ponto (.) para separar categorias: `Navigation.Home`
- Use PascalCase: `Documents.FileName`
- Seja descritivo: `Login.InvalidCredentials`

### 2. Organização por Contexto
```
Navigation.*    - Itens de menu e navegação
Common.*        - Ações e botões comuns
Messages.*      - Mensagens de feedback
Validation.*    - Mensagens de validação
Documents.*     - Funcionalidades de documentos
Meetings.*      - Funcionalidades de reuniões
```

### 3. Parâmetros em Strings
```xml
<!-- No .resx -->
<data name="Welcome.User" xml:space="preserve">
  <value>Bem-vindo, {0}!</value>
</data>
```

```csharp
// Na view ou controller
@Localizer["Welcome.User", Model.UserName]
```

## Configuração Técnica

### Program.cs
```csharp
// Configuração de localização
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Clear(); // Força pt-BR sempre
});
```

### Middleware
```csharp
// No pipeline de middleware
app.UseRequestLocalization();
```

## Extending para Múltiplos Idiomas (Futuro)

Se no futuro quiser adicionar suporte a múltiplos idiomas:

1. **Adicione novos arquivos .resx:**
   - `SharedResource.en-US.resx` (Inglês)
   - `SharedResource.es-ES.resx` (Espanhol)

2. **Atualize Program.cs:**
```csharp
var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US"),
    new CultureInfo("es-ES")
};
```

3. **Adicione seletor de idioma na interface**

## Notas Importantes

- ✅ **Interface**: Sempre em português do Brasil
- ✅ **Código**: Variáveis e métodos em inglês
- ✅ **Comentários**: Podem ser em português para melhor compreensão da equipe
- ✅ **Banco de dados**: Nomes de tabelas e campos em inglês
- ✅ **Logs**: Mensagens de sistema em português para facilitar suporte
