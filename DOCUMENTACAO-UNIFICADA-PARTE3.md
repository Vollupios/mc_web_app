# 📚 DOCUMENTAÇÃO UNIFICADA - Parte 3

## **7.1 Estrutura do Projeto**

### **📁 Organização de Pastas**

```text
IntranetDocumentos/
├── Controllers/          # Controladores MVC
│   ├── AccountController.cs
│   ├── DocumentsController.cs
│   ├── AdminController.cs
│   ├── AnalyticsController.cs
│   └── ...
├── Models/              # Entidades e ViewModels
│   ├── ApplicationUser.cs
│   ├── Document.cs
│   ├── Department.cs
│   └── ViewModels/
├── Views/               # Views Razor
│   ├── Shared/
│   ├── Documents/
│   ├── Account/
│   └── ...
├── Services/            # Lógica de negócio
│   ├── DocumentService.cs
│   ├── AnalyticsService.cs
│   ├── Security/
│   └── Background/
├── Data/                # Contexto Entity Framework
│   └── ApplicationDbContext.cs
├── Middleware/          # Middlewares customizados
│   ├── SecurityHeadersMiddleware.cs
│   └── SecurityAuditMiddleware.cs
├── Extensions/          # Métodos de extensão
├── Utils/               # Utilitários
├── Builders/            # Builder Pattern
├── wwwroot/             # Arquivos estáticos
├── DocumentsStorage/    # Armazenamento de documentos
└── DatabaseBackups/     # Backups automáticos
```text

### **🏗️ Padrões Arquiteturais**

#### **Repository Pattern**

```csharp
public interface IDocumentRepository
{
    Task<Document> GetByIdAsync(int id);
    Task<List<Document>> GetByDepartmentAsync(int departmentId);
    Task<Document> AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(int id);
}
```text

#### **Service Pattern**

```csharp
public interface IDocumentService
{
    Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user);
    Task SaveDocumentAsync(IFormFile file, ApplicationUser user, int departmentId);
    Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user);
    Task<Stream> GetDocumentStreamAsync(int documentId, ApplicationUser user);
}
```text

#### **Builder Pattern**

```csharp
public class DocumentBuilder : IBuilder<Document>
{
    private Document _document = new();
    
    public DocumentBuilder SetFileName(string fileName)
    {
        _document.OriginalFileName = fileName;
        return this;
    }
    
    public DocumentBuilder SetUploader(ApplicationUser user)
    {
        _document.UploaderId = user.Id;
        return this;
    }
    
    public Document Build() => _document;
}
```text

---

## **7.2 Padrões de Código**

### **🔧 Convenções de Nomenclatura**

#### **C# Code Style**

```csharp
// Classes: PascalCase
public class DocumentService { }

// Métodos: PascalCase
public async Task<Document> GetDocumentAsync(int id) { }

// Propriedades: PascalCase
public string OriginalFileName { get; set; }

// Variáveis locais: camelCase
var documentId = 123;

// Constantes: PascalCase
public const int MaxFileSize = 10485760;

// Interfaces: I + PascalCase
public interface IDocumentService { }
```text

#### **Async/Await Pattern**

```csharp
// Sempre usar Async suffix
public async Task<Document> GetDocumentAsync(int id)
{
    // ConfigureAwait(false) em bibliotecas
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// Exception handling
public async Task<Document> SaveDocumentAsync(Document document)
{
    try
    {
        return await _repository.AddAsync(document);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao salvar documento {DocumentId}", document.Id);
        throw;
    }
}
```text

### **📝 Logging Pattern**

```csharp
// Logs estruturados com contexto
_logger.LogInformation("📄 Upload iniciado - Arquivo: {FileName}, Usuário: {UserId}, Tamanho: {FileSize}", 
    fileName, userId, fileSize);

_logger.LogWarning("🔒 Acesso negado - Documento: {DocumentId}, Usuário: {UserId}, Motivo: {Reason}", 
    documentId, userId, "Departamento diferente");

_logger.LogError(ex, "❌ Erro crítico - Operação: {Operation}, Usuário: {UserId}", 
    operation, userId);
```text

### **🔒 Security Patterns**

```csharp
// Validação de entrada
public async Task<IActionResult> Download(int id)
{
    if (id <= 0)
        return BadRequest("ID inválido");
    
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Challenge();
    
    if (!await _documentService.CanUserAccessDocumentAsync(id, user))
        return Forbid();
    
    // ... resto da implementação
}

// Anti-forgery tokens
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Upload(UploadViewModel model) { }
```text

---

## **7.3 Testes**

### **🧪 Estrutura de Testes**

```text
Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── DocumentServiceTests.cs
│   │   ├── AnalyticsServiceTests.cs
│   │   └── UserRateLimitingServiceTests.cs
│   ├── Controllers/
│   │   ├── DocumentsControllerTests.cs
│   │   └── AccountControllerTests.cs
│   └── Utils/
├── IntegrationTests/
│   ├── DatabaseTests.cs
│   ├── AuthenticationTests.cs
│   └── FileUploadTests.cs
└── EndToEndTests/
    ├── LoginFlowTests.cs
    ├── DocumentUploadTests.cs
    └── SearchTests.cs
```text

### **🔧 Exemplo de Teste Unitário**

```csharp
[TestClass]
public class DocumentServiceTests
{
    private ApplicationDbContext _context;
    private DocumentService _documentService;
    private Mock<ILogger<DocumentService>> _mockLogger;
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<DocumentService>>();
        _documentService = new DocumentService(_context, _mockLogger.Object);
    }
    
    [TestMethod]
    public async Task CanUserAccessDocument_UserFromSameDepartment_ReturnsTrue()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "TI" };
        var user = new ApplicationUser { Id = "user1", DepartmentId = 1 };
        var document = new Document { Id = 1, DepartmentId = 1 };
        
        _context.Departments.Add(department);
        _context.Users.Add(user);
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _documentService.CanUserAccessDocumentAsync(1, user);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }
}
```text

### **🌐 Teste de Integração**

```csharp
[TestClass]
public class DocumentUploadIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public DocumentUploadIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task UploadDocument_ValidFile_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Test content"));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(fileContent, "File", "test.pdf");
        content.Add(new StringContent("1"), "DepartmentId");
        
        // Act
        var response = await _client.PostAsync("/Documents/Upload", content);
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
    }
}
```text

---

## **7.4 Build e Deploy**

### **🔧 Build Scripts**

#### **build-analytics.sh (Linux/Mac)**

```bash
#!/bin/bash
echo "🔧 Building Intranet Documentos..."

# Restore packages
dotnet restore IntranetDocumentos.csproj

# Build project
dotnet build IntranetDocumentos.csproj --configuration Release --no-restore

# Run tests
dotnet test Tests/ --no-build --verbosity normal

# Publish application
dotnet publish IntranetDocumentos.csproj --configuration Release --output ./publish

echo "✅ Build completed successfully!"
```text

#### **Deploy-WindowsServer.ps1**

```powershell
#!/usr/bin/env pwsh
param(
    [string]$TargetPath = "C:\inetpub\wwwroot\IntranetDocumentos",
    [string]$Configuration = "Release"
)

Write-Host "🚀 Iniciando deploy para Windows Server..." -ForegroundColor Green

# Stop IIS application pool
Write-Host "⏹️ Parando Application Pool..." -ForegroundColor Yellow
Stop-WebAppPool -Name "IntranetDocumentos" -ErrorAction SilentlyContinue

# Backup current deployment
if (Test-Path $TargetPath) {
    $backupPath = "$TargetPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Write-Host "💾 Criando backup em: $backupPath" -ForegroundColor Yellow
    Copy-Item -Path $TargetPath -Destination $backupPath -Recurse -Force
}

# Build and publish
Write-Host "🔧 Fazendo build da aplicação..." -ForegroundColor Yellow
dotnet publish IntranetDocumentos.csproj --configuration $Configuration --output $TargetPath --force

# Set permissions
Write-Host "🔒 Configurando permissões..." -ForegroundColor Yellow
icacls $TargetPath /grant "IIS_IUSRS:(OI)(CI)F" /T

# Start application pool
Write-Host "▶️ Iniciando Application Pool..." -ForegroundColor Yellow
Start-WebAppPool -Name "IntranetDocumentos"

Write-Host "✅ Deploy concluído com sucesso!" -ForegroundColor Green
```text

### **📦 CI/CD Pipeline (GitHub Actions)**

```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Restore dependencies
      run: dotnet restore IntranetDocumentos.csproj
    
    - name: Build
      run: dotnet build IntranetDocumentos.csproj --no-restore --configuration Release
    
    - name: Test
      run: dotnet test Tests/ --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish IntranetDocumentos.csproj --configuration Release --output ./publish
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: published-app
        path: ./publish
```text

---

## **8.1 Problemas Comuns**

### **🔴 Erro: "Failed to bind to address already in use"**

```bash
# Verificar processos na porta
sudo netstat -tulpn | grep :5000

# Finalizar processo
sudo kill -9 <PID>

# Ou usar porta diferente
dotnet run --urls "http://localhost:5001"
```text

### **🔴 Erro: "Connection string not found"**

```json
// Verificar appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntranetDocumentos;Uid=app_user;Pwd=senha;"
  }
}
```text

### **🔴 Erro: "Redis connection failed"**

```bash
# Verificar se Redis está rodando
redis-cli ping

# Se não estiver, iniciar Redis
redis-server

# Verificar logs
redis-cli info server
```text

### **🔴 Erro: "File upload size exceeded"**

```xml
<!-- web.config - Aumentar limite de upload -->
<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="52428800" /> <!-- 50MB -->
    </requestFiltering>
  </security>
</system.webServer>
```text

### **🔴 Erro: "Database migration failed"**

```bash
# Verificar conexão com banco
mysql -u app_user -p -e "SELECT 1;"

# Executar migrations manualmente
dotnet ef database update

# Em caso de erro, resetar migrations
dotnet ef database drop --force
dotnet ef database update
```text

---

## **8.2 Logs e Diagnóstico**

### **📊 Estrutura de Logs**

```text
Logs/
├── application-YYYYMMDD.log      # Logs da aplicação
├── security-YYYYMMDD.log         # Logs de segurança
├── performance-YYYYMMDD.log      # Logs de performance
└── errors-YYYYMMDD.log           # Logs de erro
```text

### **🔍 Comandos de Diagnóstico**

```powershell
# Verificar logs da aplicação
Get-Content -Path "Logs\application-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50

# Verificar eventos do Windows
Get-EventLog -LogName Application -Source "ASP.NET Core" -Newest 10

# Verificar status IIS
Get-WebAppPoolState -Name "IntranetDocumentos"
Get-WebApplication -Name "IntranetDocumentos"

# Verificar conectividade MySQL
Test-NetConnection -ComputerName localhost -Port 3306

# Verificar conectividade Redis
Test-NetConnection -ComputerName localhost -Port 6379
```text

### **📈 Monitoramento de Performance**

```csharp
// Performance counters
public class PerformanceMonitor
{
    public static void LogMetrics(ILogger logger)
    {
        var process = Process.GetCurrentProcess();
        var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
        var cpuTime = process.TotalProcessorTime.TotalMilliseconds;
        
        logger.LogInformation("📊 Memory: {MemoryMB}MB, CPU: {CpuTime}ms", 
            memoryUsage, cpuTime);
    }
}
```text

---

## **8.3 FAQ**

### **❓ Como adicionar um novo departamento?**

```sql
-- Via SQL
INSERT INTO Departments (Name) VALUES ('Novo Departamento');

-- Ou via interface administrativa
-- Admin → Departamentos → Adicionar Novo
```text

### **❓ Como resetar senha de usuário?**

```csharp
// Via código (admin)
var user = await _userManager.FindByEmailAsync("usuario@empresa.com");
var token = await _userManager.GeneratePasswordResetTokenAsync(user);
await _userManager.ResetPasswordAsync(user, token, "NovaSenha123!");
```text

### **❓ Como fazer backup manual?**

```bash
# Backup completo
./backup-database.ps1

# Ou comando direto
mysqldump -u app_user -p IntranetDocumentos > backup.sql
tar -czf DocumentsStorage_backup.tar.gz DocumentsStorage/
```text

### **❓ Como verificar se Redis está funcionando?**

```bash
# Teste básico
redis-cli ping
# Resposta esperada: PONG

# Ver chaves da aplicação
redis-cli keys "*IntranetDocumentos*"

# Monitorar comandos
redis-cli monitor
```text

### **❓ Como configurar HTTPS em produção?**

```powershell
# 1. Obter certificado SSL
# 2. Instalar no Windows Server
# 3. Configurar binding no IIS
New-WebBinding -Name "IntranetDocumentos" -Protocol https -Port 443 -SslFlags 1

# 4. Forçar HTTPS na aplicação (appsettings.json)
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:443"
      }
    }
  }
}
```text

---

## **9.1 Últimas Correções**

### **🔧 Versão 2.0 (16/07/2025)**

#### **✅ Principais Correções:**

- ✅ **Rota AdvancedSearch implementada** (DocumentsController)
- ✅ **Queries LINQ otimizadas** (AnalyticsService)  
- ✅ **Sintaxe SQL MySQL corrigida** (setup-mysql.sql)
- ✅ **Redis integrado** com rate limiting distribuído
- ✅ **Middlewares de segurança** implementados
- ✅ **VS Code configurado** para MySQL

#### **🔒 Melhorias de Segurança:**

- ✅ Rate limiting por usuário/email
- ✅ Headers de segurança (X-Frame-Options, CSP, etc.)
- ✅ Auditoria de segurança completa
- ✅ Upload de arquivos seguro
- ✅ Validação robusta de entrada

#### **⚡ Otimizações de Performance:**

- ✅ Cache Redis distribuído
- ✅ Queries MySQL otimizadas
- ✅ Índices de banco otimizados
- ✅ Streaming de arquivos
- ✅ Compressão de respostas

---

## **9.2 Roadmap**

### **🎯 Versão 2.1 (Próximos 30 dias)**

- 🔄 **Notificações em tempo real** (SignalR)
- 📱 **PWA** (Progressive Web App)
- 🔍 **Busca full-text** em conteúdo de documentos
- 📊 **Dashboard avançado** com mais métricas
- 🌍 **Multi-idioma** (i18n)

### **🎯 Versão 2.2 (Próximos 60 dias)**

- ☁️ **Integração nuvem** (Azure/AWS)
- 🤖 **OCR** para documentos escaneados
- 📧 **Sistema de aprovação** por email
- 📱 **App mobile** (React Native)
- 🔄 **Sincronização offline**

### **🎯 Versão 3.0 (Próximos 90 dias)**

- 🤖 **IA para categorização** automática
- 🔐 **Single Sign-On** (SAML/OAuth)
- 📊 **Business Intelligence** integrado
- 🌐 **Multi-tenant** support
- 🔄 **Microservices** architecture

---

## **9.3 Próximos Passos**

### **🚀 Deploy em Produção**

1. ✅ **Executar scripts de deploy** (`Deploy-WindowsServer.ps1`)
2. ✅ **Configurar SSL/HTTPS** obrigatório
3. ✅ **Configurar backup automático** (diário)
4. ✅ **Monitoramento** de performance
5. ✅ **Treinamento** dos usuários

### **🔧 Manutenção**

1. ✅ **Backup diário** automatizado
2. ✅ **Monitoramento** de logs
3. ✅ **Atualizações de segurança** mensais
4. ✅ **Review** de permissões trimestralmente
5. ✅ **Auditoria** de segurança semestral

### **📈 Melhorias Contínuas**

1. ✅ **Feedback** dos usuários
2. ✅ **Métricas** de uso
3. ✅ **Otimizações** de performance
4. ✅ **Novas funcionalidades** conforme demanda
5. ✅ **Atualizações** tecnológicas

---

## **🎯 CONCLUSÃO**

### **✅ Status Final do Projeto**

A aplicação **Intranet Documentos** está **100% funcional** e pronta para deploy em ambiente de produção. Todas as funcionalidades principais foram implementadas, testadas e documentadas.

### **🏆 Principais Conquistas**

- ✅ **Sistema completo** de gestão de documentos
- ✅ **Segurança enterprise-grade** implementada
- ✅ **Performance otimizada** com Redis
- ✅ **Busca avançada** funcional
- ✅ **Analytics completo** e dashboards
- ✅ **Documentação unificada** e abrangente

### **🚀 Pronto para Produção**

- ✅ **Scripts de deploy** automatizados
- ✅ **Configurações de segurança** validadas
- ✅ **Backup e restore** funcionais
- ✅ **Monitoramento** implementado
- ✅ **Documentação completa** para administradores

**A aplicação está pronta para servir uma organização empresarial com segurança, performance e escalabilidade! 🎉**

---

*Documentação compilada em 16 de Julho de 2025  
Versão 2.0 - Production Ready*
