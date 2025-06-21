# Sistema de Analytics - Intranet Documentos

## Visão Geral

O sistema de analytics foi implementado para fornecer insights detalhados sobre o uso da plataforma de documentos, reuniões e atividade por departamento.

## Funcionalidades Implementadas

### 📊 Dashboard Principal
- **Localização**: `/Analytics/Dashboard`
- **Acesso**: Admin e Gestor
- **Características**:
  - Cards com métricas principais (total de documentos, downloads, reuniões, departamentos)
  - Gráfico de atividade por departamento (Chart.js)
  - Top documentos mais baixados
  - Tabelas detalhadas por departamento e tipo de reunião

### 📈 Estatísticas de Documentos
- **Localização**: `/Analytics/DocumentStatistics`
- **Dados Coletados**:
  - Total de documentos por departamento
  - Estatísticas de download
  - Armazenamento utilizado
  - Documentos mais populares

### 🎯 Métricas de Reuniões
- **Localização**: `/Analytics/MeetingMetrics`
- **Análises**:
  - Reuniões por tipo (Interna, Externa, Online)
  - Status das reuniões (Agendada, Concluída, Cancelada)
  - Distribuição por departamento
  - Tempo médio de reuniões
  - Histórico mensal

### 🏢 Atividade por Departamento
- **Localização**: `/Analytics/DepartmentActivity`
- **Recursos**:
  - Score de atividade calculado
  - Comparativo entre departamentos
  - Usuários mais ativos
  - Gráfico radar de comparação

## Arquitetura Técnica

### Modelos de Dados

#### DocumentDownload
```csharp
public class DocumentDownload
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string UserId { get; set; }
    public DateTime DownloadDate { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}
```

#### ViewModels
- `DashboardViewModel`: Agrega todos os dados do dashboard
- `DocumentStatisticsViewModel`: Estatísticas específicas de documentos
- `ReunioesMetricsViewModel`: Métricas de reuniões
- `DepartmentActivityViewModel`: Atividade departamental

### Serviços

#### IAnalyticsService / AnalyticsService
Responsável por:
- Agregação de dados de múltiplas fontes
- Cálculos de métricas e estatísticas
- Geração de relatórios analíticos
- Cache de consultas pesadas

**Métodos Principais**:
```csharp
Task<DashboardViewModel> GetDashboardDataAsync(int? periodoDias = null)
Task<DocumentStatisticsViewModel> GetDocumentStatisticsAsync(int? periodoDias = null)
Task<ReunioesMetricsViewModel> GetMeetingMetricsAsync(int? periodoDias = null)
Task<DepartmentActivityViewModel> GetDepartmentActivityAsync(int? periodoDias = null)
```

### Controladores

#### AnalyticsController
- **Autorização**: `[Authorize(Roles = "Admin,Gestor")]`
- **Endpoints**:
  - `GET /Dashboard`: Dashboard principal
  - `GET /DocumentStatistics`: Estatísticas de documentos
  - `GET /MeetingMetrics`: Métricas de reuniões
  - `GET /DepartmentActivity`: Atividade departamental

### Integração com Rastreamento

#### DocumentsController
Modificado para registrar downloads:
```csharp
[HttpGet]
public async Task<IActionResult> Download(int id)
{
    // ... código existente ...
    
    // Registrar o download para analytics
    await _analyticsService.RegisterDownloadAsync(document.Id, User.Identity.Name, HttpContext);
    
    // ... retornar arquivo ...
}
```

## Visualizações

### Tecnologias Utilizadas
- **Chart.js**: Gráficos interativos
- **Bootstrap 5**: Layout responsivo
- **Bootstrap Icons**: Iconografia
- **Cards e Badges**: Elementos visuais

### Tipos de Gráficos
1. **Barra**: Atividade por departamento
2. **Doughnut**: Reuniões por tipo
3. **Linha**: Histórico temporal
4. **Radar**: Comparação multidimensional

## Segurança e Permissões

### Controle de Acesso
- **Admin**: Acesso total a todas as métricas
- **Gestor**: Acesso a dashboards e relatórios
- **Usuario**: Sem acesso (pode ser expandido futuramente)

### Dados Sensíveis
- IPs são coletados mas não exibidos
- User Agents para análise de dispositivos
- Dados pessoais são anonimizados em relatórios

## Performance

### Otimizações Implementadas
- Queries otimizadas com LINQ
- Uso de índices no banco de dados
- Aggregações em nível de SQL
- Cache em ViewModels para consultas frequentes

### Recomendações
- Implementar cache Redis para dashboards
- Background jobs para relatórios pesados
- Paginação em listas grandes
- Compressão de responses JSON

## Instalação e Configuração

### 1. Migration do Banco de Dados
```bash
dotnet ef database update
```

### 2. Registro de Serviços
```csharp
// Program.cs
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
```

### 3. Navegação
A navegação foi automaticamente adicionada ao layout principal para usuários Admin e Gestor.

## Estrutura de Arquivos

```
Controllers/
  AnalyticsController.cs
Models/
  DocumentDownload.cs
  ViewModels/
    DashboardViewModel.cs
Services/
  IAnalyticsService.cs
  AnalyticsService.cs
Views/
  Analytics/
    Dashboard.cshtml
    DocumentStatistics.cshtml
    MeetingMetrics.cshtml
    DepartmentActivity.cshtml
Migrations/
  20250621000000_AddDocumentDownloadAnalytics.cs
```

## Extensões Futuras

### Funcionalidades Planejadas
1. **Relatórios Exportáveis**: PDF, Excel
2. **Alertas Automáticos**: Notificações por thresholds
3. **Analytics em Tempo Real**: WebSockets/SignalR
4. **Machine Learning**: Predições e recomendações
5. **API REST**: Endpoints para integração externa

### Melhorias de UX
1. **Filtros Avançados**: Data ranges, departamentos específicos
2. **Drill-down**: Detalhamento de métricas
3. **Dashboards Personalizáveis**: Usuários podem configurar
4. **Mobile-first**: Otimização para dispositivos móveis

## Monitoramento e Logs

### Métricas de Sistema
- Tempo de resposta das queries
- Uso de memória durante agregações
- Frequência de acesso aos dashboards

### Logs Importantes
- Falhas em queries de analytics
- Acessos a dashboards por usuário
- Performance de geração de relatórios

## Suporte e Manutenção

### Comandos Úteis
```bash
# Verificar performance das queries
dotnet ef dbcontext optimize

# Backup antes de atualizações
dotnet run backup-database

# Testes de analytics
dotnet test --filter Analytics
```

### Troubleshooting
1. **Dashboard não carrega**: Verificar permissões de usuário
2. **Gráficos não aparecem**: Verificar Chart.js CDN
3. **Dados inconsistentes**: Verificar migrations aplicadas
4. **Performance lenta**: Analisar queries com EF Core logging

---

**Desenvolvido para**: Marcos Contabilidade  
**Versão**: 1.0  
**Data**: Junho 2025  
**Tecnologia**: ASP.NET Core 9.0 + Entity Framework Core + Chart.js
