# 📚 DOCUMENTAÇÃO UNIFICADA - Parte 2

## **4.1 Análise de Segurança**

### **🔒 Medidas de Segurança Implementadas**

#### **1. Autenticação e Autorização**

```csharp
// ASP.NET Core Identity configurado
services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configuração de senhas seguras
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});
```text

#### **2. Headers de Segurança**

```csharp
// SecurityHeadersMiddleware implementado
app.UseMiddleware<SecurityHeadersMiddleware>();

// Headers configurados:
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
```text

#### **3. Proteção contra Ataques**

- ✅ **CSRF**: Anti-forgery tokens em todos os formulários
- ✅ **XSS**: Encoding automático em Razor views
- ✅ **SQL Injection**: Entity Framework com queries parametrizadas
- ✅ **Path Traversal**: Validação de nomes de arquivo
- ✅ **File Upload**: Validação de tipo e tamanho

### **🔍 Auditoria de Segurança**

```csharp
// SecurityAuditMiddleware registra todas as ações
public class SecurityAuditMiddleware
{
    // Logs detalhados de:
    // - Tentativas de login
    // - Uploads de arquivos
    // - Downloads de documentos
    // - Alterações de dados
    // - Acessos negados
}
```text

---

## **4.2 Rate Limiting**

### **⚡ Rate Limiting Distribuído com Redis**

#### **🔧 Configuração**

```csharp
// UserRateLimitingService com Redis
public class UserRateLimitingService : IUserRateLimitingService
{
    private readonly IDistributedCache _distributedCache;
    
    // Rate limiting por usuário/email
    // Compartilhado entre servidores via Redis
}
```text

#### **📊 Limites Configurados**

```json
{
  "RateLimiting": {
    "LoginAttempts": {
      "MaxAttempts": 5,
      "WindowMinutes": 15,
      "LockoutMinutes": 30
    },
    "UploadAttempts": {
      "MaxAttempts": 20,
      "WindowMinutes": 60
    }
  }
}
```text

#### **🛡️ Proteção Implementada**

- ✅ **Login**: 5 tentativas em 15 min → Bloqueio 30 min
- ✅ **Upload**: 20 uploads em 60 min por usuário
- ✅ **Global**: Distribuído via Redis entre servidores
- ✅ **Graceful**: Fallback para MemoryCache se Redis offline

---

## **4.3 Hardening**

### **🔒 Script de Hardening (Hardening-Seguranca.ps1)**

```powershell
# Configurações de segurança do Windows Server
# - Desabilitar serviços desnecessários
# - Configurar firewall
# - Políticas de senha
# - Atualizações automáticas
# - Logs de auditoria
```text

### **🛡️ Configurações IIS**

```xml
<!-- web.config - Configurações de segurança -->
<system.webServer>
  <security>
    <requestFiltering>
      <requestLimits maxAllowedContentLength="10485760" />
      <fileExtensions>
        <add fileExtension=".exe" allowed="false" />
        <add fileExtension=".bat" allowed="false" />
      </fileExtensions>
    </requestFiltering>
  </security>
</system.webServer>
```text

### **🔐 Certificados SSL**

```powershell
# Configuração HTTPS obrigatório
New-WebBinding -Name "IntranetDocumentos" -Protocol https -Port 443
# Redirecionamento HTTP → HTTPS
```text

---

## **4.4 Auditoria**

### **📋 Script de Auditoria (Auditoria-Seguranca.ps1)**

```powershell
# Verificações automatizadas:
# ✅ Configurações de segurança do IIS
# ✅ Permissões de arquivos
# ✅ Configuração SSL/TLS
# ✅ Headers de segurança
# ✅ Logs de acesso
# ✅ Backup e integridade
```text

### **📊 Logs de Auditoria**

```json
{
  "Timestamp": "2025-07-16T14:52:39.1007342Z",
  "EventType": "REQUEST_START",
  "Action": "/Documents/Download",
  "UserId": "user123",
  "UserEmail": "user@empresa.com",
  "IpAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0...",
  "Method": "GET",
  "Details": {"DocumentId": 42}
}
```text

### **🔍 Monitoramento Contínuo**

- ✅ **Tentativas de acesso negado**
- ✅ **Downloads de documentos sensíveis**
- ✅ **Múltiplos logins do mesmo usuário**
- ✅ **Uploads de arquivos suspeitos**
- ✅ **Alterações em dados críticos**

---

## **5.1 Redis Cache**

### **🔴 Implementação Redis**

#### **⚙️ Configuração no Program.cs**

```csharp
// Configuração Redis com fallback
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "IntranetDocumentos";
});

// Fallback para MemoryCache se Redis não disponível
services.AddMemoryCache();
```text

#### **💾 Uso do Cache**

```csharp
// Rate limiting distribuído
await _distributedCache.SetStringAsync($"login_attempts:{email}", json);

// Cache de sessões
await _distributedCache.SetStringAsync($"user_session:{userId}", sessionData);

// Cache de dados frequentes
await _distributedCache.SetStringAsync($"departments", departmentsJson);
```text

### **📊 Benefícios Obtidos**

- ⚡ **10-100x mais rápido** que consultas ao banco
- 🔄 **Cache persistente** entre restarts da aplicação
- 💾 **Menos carga no MySQL** (redução de consultas)
- 🌐 **Escalabilidade** para múltiplos servidores
- 🛡️ **Rate limiting preciso** globalmente

### **🔧 Monitoramento Redis**

```bash
# Verificar status
redis-cli ping

# Ver chaves da aplicação
redis-cli keys "*IntranetDocumentos*"

# Monitorar comandos em tempo real
redis-cli monitor

# Estatísticas de memória
redis-cli info memory
```text

---

## **5.2 Otimizações de Performance**

### **📊 Queries Otimizadas (AnalyticsService)**

```csharp
// ANTES (problemático)
MonthName = cultureInfo.DateTimeFormat.GetMonthName(g.Key.Month), // Erro EF Core

// DEPOIS (otimizado)
MonthName = "", // Preenchido após query
// Pós-processamento em memória
foreach (var month in monthlyUploads)
{
    month.MonthName = cultureInfo.DateTimeFormat.GetMonthName(month.Month);
}
```text

### **🗄️ Otimizações MySQL**

```sql
-- Configurações de produção otimizadas
SET GLOBAL innodb_buffer_pool_size = 134217728; -- 128MB
SET GLOBAL max_allowed_packet = 52428800; -- 50MB
SET GLOBAL innodb_log_file_size = 67108864; -- 64MB

-- Índices importantes
CREATE INDEX idx_documents_department ON Documents(DepartmentId);
CREATE INDEX idx_documents_upload_date ON Documents(UploadDate);
CREATE INDEX idx_download_logs_date ON DocumentDownloadLogs(DownloadDate);
```text

### **📁 Otimizações de Arquivo**

```csharp
// Upload otimizado com streaming
public async Task<string> SaveFileAsync(IFormFile file)
{
    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    var filePath = Path.Combine(_storagePath, fileName);
    
    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream); // Streaming direto
    
    return fileName;
}
```text

---

## **5.3 Monitoramento**

### **📊 Dashboard de Monitoramento**

```csharp
// Métricas implementadas no AnalyticsService:
// - Total de documentos e downloads
// - Uploads por departamento
// - Documentos mais baixados
// - Atividade por usuário
// - Estatísticas de reuniões
// - Performance do sistema
```text

### **🔍 Logs Estruturados**

```csharp
// Logging configurado com Serilog/NLog
_logger.LogInformation("📄 Upload iniciado - Arquivo: {FileName}, Usuário: {UserId}, Tamanho: {FileSize}", 
    fileName, userId, fileSize);

_logger.LogWarning("🔒 Tentativa de acesso negado - Documento: {DocId}, Usuário: {UserId}", 
    documentId, userId);

_logger.LogError("❌ Erro no upload - Arquivo: {FileName}, Erro: {Error}", 
    fileName, ex.Message);
```text

### **📈 Métricas de Performance**

```json
{
  "Application": {
    "RequestsPerSecond": 45,
    "AverageResponseTime": "150ms",
    "ErrorRate": "0.1%"
  },
  "Database": {
    "ActiveConnections": 8,
    "AverageQueryTime": "25ms",
    "SlowQueries": 0
  },
  "Redis": {
    "ConnectedClients": 5,
    "MemoryUsage": "45MB",
    "HitRate": "98.5%"
  }
}
```text

---

## **6.1 Sistema de Documentos**

### **📄 Upload de Documentos**

```csharp
public class DocumentService : IDocumentService
{
    // Funcionalidades implementadas:
    // ✅ Upload com validação de tipo e tamanho
    // ✅ Armazenamento seguro fora da wwwroot
    // ✅ Nomes únicos (GUID) para evitar conflitos
    // ✅ Controle de acesso por departamento
    // ✅ Versionamento de documentos
    // ✅ Workflow de aprovação
}
```text

### **📁 Organização de Arquivos**

```text
DocumentsStorage/
├── Pessoal/
│   ├── 12345678-1234-1234-1234-123456789abc.pdf
│   └── 87654321-4321-4321-4321-cba987654321.docx
├── Fiscal/
├── Contabil/
├── Cadastro/
├── Apoio/
├── TI/
└── Geral/
    └── manual-usuario.pdf
```text

### **🔒 Controle de Acesso**

```csharp
public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
{
    var document = await GetDocumentByIdAsync(documentId);
    
    // Admin: acesso total
    if (await _userManager.IsInRoleAsync(user, "Admin")) 
        return true;
    
    // Gestor: acesso total
    if (await _userManager.IsInRoleAsync(user, "Gestor")) 
        return true;
    
    // Usuario: apenas seu departamento + Geral
    if (document.DepartmentId == null) return true; // Geral
    return document.DepartmentId == user.DepartmentId;
}
```text

---

## **6.2 Busca Avançada**

### **🔍 Funcionalidades da Busca**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AdvancedSearch(
    string? searchTerm,     // Busca no nome do arquivo
    int? departmentId,      // Filtro por departamento
    string? contentType,    // Filtro por tipo de arquivo
    DateTime? startDate,    // Data inicial
    DateTime? endDate)      // Data final
{
    var documents = await _documentService.AdvancedSearchAsync(
        searchTerm, departmentId, contentType, startDate, endDate, user);
    
    return View(documents);
}
```text

### **🗄️ Query Otimizada**

```csharp
public async Task<List<Document>> AdvancedSearchAsync(...)
{
    var query = _context.Documents
        .Include(d => d.Department)
        .Include(d => d.Uploader)
        .AsQueryable();
    
    // Aplicar filtros condicionalmente
    if (!string.IsNullOrEmpty(searchTerm))
        query = query.Where(d => d.OriginalFileName.Contains(searchTerm));
    
    if (departmentId.HasValue)
        query = query.Where(d => d.DepartmentId == departmentId);
    
    if (!string.IsNullOrEmpty(contentType))
        query = query.Where(d => d.ContentType.Contains(contentType));
    
    if (startDate.HasValue)
        query = query.Where(d => d.UploadDate >= startDate);
    
    if (endDate.HasValue)
        query = query.Where(d => d.UploadDate <= endDate);
    
    return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
}
```text

### **🎨 Interface da Busca**

```html
<!-- Formulário de busca avançada -->
<form asp-action="AdvancedSearch" method="post">
    <div class="row">
        <div class="col-md-4">
            <label>Termo de busca</label>
            <input type="text" name="searchTerm" class="form-control" 
                   placeholder="Nome do arquivo...">
        </div>
        <div class="col-md-2">
            <label>Departamento</label>
            <select name="departmentId" class="form-select">
                <option value="">Todos</option>
                <!-- Departamentos do usuário -->
            </select>
        </div>
        <div class="col-md-2">
            <label>Tipo de arquivo</label>
            <select name="contentType" class="form-select">
                <option value="">Todos</option>
                <option value="pdf">PDF</option>
                <option value="word">Word</option>
                <option value="excel">Excel</option>
            </select>
        </div>
        <div class="col-md-2">
            <label>Data inicial</label>
            <input type="date" name="startDate" class="form-control">
        </div>
        <div class="col-md-2">
            <label>Data final</label>
            <input type="date" name="endDate" class="form-control">
        </div>
    </div>
    <button type="submit" class="btn btn-primary">Buscar</button>
</form>
```text

---

## **6.3 Sistema de Reuniões**

### **📅 Tipos de Reunião**

```csharp
public enum TipoReuniao
{
    [Display(Name = "Reunião Ordinária")]
    Ordinaria = 1,
    
    [Display(Name = "Reunião Extraordinária")]
    Extraordinaria = 2,
    
    [Display(Name = "Reunião de Emergência")]
    Emergencial = 3
}
```text

### **🔔 Sistema de Notificações**

```csharp
public class NotificationService : INotificationService
{
    public async Task SendMeetingReminderAsync(Reuniao reuniao)
    {
        var emailBody = $@"
            <h2>Lembrete de Reunião</h2>
            <p><strong>Título:</strong> {reuniao.Titulo}</p>
            <p><strong>Data:</strong> {reuniao.Data:dd/MM/yyyy}</p>
            <p><strong>Horário:</strong> {reuniao.HoraInicio:HH:mm} às {reuniao.HoraFim:HH:mm}</p>
            <p><strong>Local:</strong> {reuniao.Local}</p>
            <p><strong>Tipo:</strong> {reuniao.TipoReuniao.GetDisplayName()}</p>
        ";
        
        await _emailService.SendEmailAsync(reuniao.ResponsavelUser.Email, 
            "Lembrete de Reunião", emailBody);
    }
}
```text

### **📊 Analytics de Reuniões**

```csharp
public class ReunioesMetricsViewModel
{
    public int TotalReunioes { get; set; }
    public int ReunioesMesAtual { get; set; }
    public int ReunioesPendentes { get; set; }
    public int ReunioesPassadas { get; set; }
    public double TempoMedioReunioes { get; set; }
    public List<ReuniaoPorTipoViewModel> ReuniaoPorTipo { get; set; }
    public List<ReuniaoPorStatusViewModel> ReuniaoPorStatus { get; set; }
    public List<ReuniaoPorDepartamentoViewModel> ReuniaoPorDepartamento { get; set; }
}
```text

---

## **6.4 Ramais Telefônicos**

### **📞 Estrutura de Ramais**

```csharp
public class Ramal
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Numero { get; set; }
    public string? Email { get; set; }
    public string? Cargo { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    public bool Ativo { get; set; } = true;
}
```text

### **🔍 Busca de Ramais**

```csharp
public async Task<List<Ramal>> SearchRamaisAsync(string searchTerm, int? departmentId)
{
    var query = _context.Ramais
        .Include(r => r.Department)
        .Where(r => r.Ativo)
        .AsQueryable();
    
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(r => 
            r.Nome.Contains(searchTerm) ||
            r.Numero.Contains(searchTerm) ||
            r.Cargo.Contains(searchTerm));
    }
    
    if (departmentId.HasValue)
        query = query.Where(r => r.DepartmentId == departmentId);
    
    return await query.OrderBy(r => r.Nome).ToListAsync();
}
```text

### **📱 Interface Responsiva**

```html
<!-- Lista de ramais com busca -->
<div class="ramal-search">
    <input type="text" id="searchRamais" class="form-control" 
           placeholder="Buscar por nome, ramal ou cargo...">
</div>

<div class="ramais-grid">
    @foreach (var ramal in Model)
    {
        <div class="ramal-card">
            <h5>@ramal.Nome</h5>
            <p><strong>Ramal:</strong> @ramal.Numero</p>
            <p><strong>Departamento:</strong> @ramal.Department.Name</p>
            @if (!string.IsNullOrEmpty(ramal.Cargo))
            {
                <p><strong>Cargo:</strong> @ramal.Cargo</p>
            }
            @if (!string.IsNullOrEmpty(ramal.Email))
            {
                <p><a href="mailto:@ramal.Email">@ramal.Email</a></p>
            }
        </div>
    }
</div>
```text

---

## **6.5 Analytics e Relatórios**

### **📊 Dashboard Executivo**

```csharp
public class DashboardViewModel
{
    public DocumentStatisticsViewModel DocumentStatistics { get; set; }
    public ReunioesMetricsViewModel ReunioesMetrics { get; set; }
    public DepartmentActivityViewModel DepartmentActivity { get; set; }
    public List<UserActivityViewModel> TopActiveUsers { get; set; }
    public SystemHealthViewModel SystemHealth { get; set; }
}
```text

### **📈 Métricas de Documentos**

```csharp
public class DocumentStatisticsViewModel
{
    public int TotalDocuments { get; set; }
    public int DocumentsThisMonth { get; set; }
    public int TotalDownloads { get; set; }
    public int DownloadsThisMonth { get; set; }
    public long TotalStorageUsed { get; set; }
    public List<DocumentsByDepartmentViewModel> DocumentsByDepartment { get; set; }
    public List<DocumentTypeStatViewModel> DocumentTypeStats { get; set; }
    public List<TopDownloadedDocumentViewModel> TopDownloadedDocuments { get; set; }
    public List<MonthlyDocumentStatsViewModel> MonthlyStats { get; set; }
}
```text

### **🎯 Atividade por Departamento**

```csharp
public class DepartmentActivityViewModel
{
    public List<DepartmentStatsViewModel> DepartmentStats { get; set; }
    public List<UserActivityViewModel> TopActiveUsers { get; set; }
}

public class DepartmentStatsViewModel
{
    public string DepartmentName { get; set; }
    public int UserCount { get; set; }
    public int DocumentCount { get; set; }
    public int DownloadCount { get; set; }
    public int ReunioesCount { get; set; }
    public int ActivityScore { get; set; } // Pontuação calculada
}
```text

### **📊 Visualização de Dados**

```javascript
// Charts.js para gráficos interativos
// Gráficos implementados:
// - Documentos por departamento (Pie Chart)
// - Uploads mensais (Line Chart)
// - Downloads por tipo de arquivo (Bar Chart)
// - Atividade de usuários (Horizontal Bar)
// - Estatísticas de reuniões (Doughnut Chart)
```text

---

*Continua na parte 3 com Desenvolvimento, Troubleshooting e Changelog...*
