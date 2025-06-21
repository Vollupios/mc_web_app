using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using System.Globalization;
using IntranetDocumentos.Extensions;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço para coletar e processar dados de analytics
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o dashboard completo de analytics
        /// </summary>
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            try
            {
                _logger.LogInformation("Coletando dados para dashboard de analytics");

                var documentStats = await GetDocumentStatisticsAsync();
                var reunioesMetrics = await GetReunioesMetricsAsync();
                var departmentActivity = await GetDepartmentActivityAsync();

                return new DashboardViewModel
                {
                    DocumentStatistics = documentStats,
                    ReunioesMetrics = reunioesMetrics,
                    DepartmentActivity = departmentActivity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao coletar dados do dashboard");
                throw;
            }
        }

        /// <summary>
        /// Registra um download de documento
        /// </summary>
        public async Task RegisterDocumentDownloadAsync(int documentId, string userId, string userAgent, string ipAddress)
        {
            try
            {
                var download = new DocumentDownload
                {
                    DocumentId = documentId,
                    UserId = userId,
                    DownloadDate = DateTime.UtcNow,
                    UserAgent = userAgent,
                    IpAddress = ipAddress
                };

                _context.DocumentDownloads.Add(download);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Download registrado para documento {DocumentId} por usuário {UserId}", documentId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar download do documento {DocumentId}", documentId);
            }
        }

        /// <summary>
        /// Obtém estatísticas de documentos
        /// </summary>
        public async Task<DocumentStatisticsViewModel> GetDocumentStatisticsAsync()
        {
            try
            {
                var totalDocuments = await _context.Documents.CountAsync();
                var documentsThisMonth = await _context.Documents
                    .Where(d => d.UploadDate.Month == DateTime.Now.Month && d.UploadDate.Year == DateTime.Now.Year)
                    .CountAsync();

                var totalDownloads = await _context.DocumentDownloads.CountAsync();
                var downloadsThisMonth = await _context.DocumentDownloads
                    .Where(d => d.DownloadDate.Month == DateTime.Now.Month && d.DownloadDate.Year == DateTime.Now.Year)
                    .CountAsync();

                var totalStorageUsed = await _context.Documents.SumAsync(d => d.FileSize);

                var documentsByDepartment = await GetDocumentsByDepartmentAsync();
                var documentTypeStats = await GetDocumentTypeStatsAsync();
                var topDownloadedDocuments = await GetTopDownloadedDocumentsAsync();
                var monthlyStats = await GetMonthlyDocumentStatsAsync();

                return new DocumentStatisticsViewModel
                {
                    TotalDocuments = totalDocuments,
                    DocumentsThisMonth = documentsThisMonth,
                    TotalDownloads = totalDownloads,
                    DownloadsThisMonth = downloadsThisMonth,
                    TotalStorageUsed = totalStorageUsed,
                    DocumentsByDepartment = documentsByDepartment,
                    DocumentTypeStats = documentTypeStats,
                    TopDownloadedDocuments = topDownloadedDocuments,
                    MonthlyStats = monthlyStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtém métricas de reuniões
        /// </summary>
        public async Task<ReunioesMetricsViewModel> GetReunioesMetricsAsync()
        {
            try
            {
                var totalReunioes = await _context.Reunioes.CountAsync();
                var reunioesMesAtual = await _context.Reunioes
                    .Where(r => r.Data.Month == DateTime.Now.Month && r.Data.Year == DateTime.Now.Year)
                    .CountAsync();

                var reunioesPendentes = await _context.Reunioes
                    .Where(r => r.Data.Date >= DateTime.Now.Date)
                    .CountAsync();

                var reunioesPassadas = await _context.Reunioes
                    .Where(r => r.Data.Date < DateTime.Now.Date)
                    .CountAsync();

                var tempoMedioReunioes = await _context.Reunioes
                    .Select(r => (r.HoraFim - r.HoraInicio).TotalMinutes)
                    .DefaultIfEmpty(0)
                    .AverageAsync();

                var reuniaoPorTipo = await GetReuniaoPorTipoAsync();
                var reuniaoPorStatus = await GetReuniaoPorStatusAsync();
                var reuniaoPorDepartamento = await GetReuniaoPorDepartamentoAsync();
                var reuniaoPorMes = await GetReuniaoPorMesAsync();

                return new ReunioesMetricsViewModel
                {
                    TotalReunioes = totalReunioes,
                    ReunioesMesAtual = reunioesMesAtual,
                    ReunioesPendentes = reunioesPendentes,
                    ReunioesPassadas = reunioesPassadas,
                    TempoMedioReunioes = tempoMedioReunioes,
                    ReuniaoPorTipo = reuniaoPorTipo,
                    ReuniaoPorStatus = reuniaoPorStatus,
                    ReuniaoPorDepartamento = reuniaoPorDepartamento,
                    ReuniaoPorMes = reuniaoPorMes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas de reuniões");
                throw;
            }
        }

        /// <summary>
        /// Obtém atividade por departamento
        /// </summary>
        public async Task<DepartmentActivityViewModel> GetDepartmentActivityAsync()
        {
            try
            {
                var departmentStats = await GetDepartmentStatsAsync();
                var topActiveUsers = await GetTopActiveUsersAsync();

                return new DepartmentActivityViewModel
                {
                    DepartmentStats = departmentStats,
                    TopActiveUsers = topActiveUsers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter atividade por departamento");
                throw;
            }
        }

        // Métodos auxiliares privados

        private async Task<List<DocumentsByDepartmentViewModel>> GetDocumentsByDepartmentAsync()
        {
            var query = await _context.Documents
                .Include(d => d.Department)
                .GroupBy(d => d.Department != null ? d.Department.Name : "Geral")
                .Select(g => new DocumentsByDepartmentViewModel
                {
                    DepartmentName = g.Key,
                    DocumentCount = g.Count(),
                    StorageUsed = g.Sum(d => d.FileSize)
                })
                .ToListAsync();

            // Obter contagem de downloads por departamento
            foreach (var dept in query)
            {
                var downloads = await _context.DocumentDownloads
                    .Include(dd => dd.Document)
                    .ThenInclude(d => d.Department)
                    .Where(dd => dd.Document.Department != null ? dd.Document.Department.Name == dept.DepartmentName : dept.DepartmentName == "Geral")
                    .CountAsync();
                dept.DownloadCount = downloads;
            }

            return query;
        }

        private async Task<List<DocumentTypeStatViewModel>> GetDocumentTypeStatsAsync()
        {
            var totalDocs = await _context.Documents.CountAsync();
            if (totalDocs == 0) return new List<DocumentTypeStatViewModel>();

            return await _context.Documents
                .GroupBy(d => d.ContentType)
                .Select(g => new DocumentTypeStatViewModel
                {
                    FileType = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / totalDocs * 100
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        private async Task<List<TopDownloadedDocumentViewModel>> GetTopDownloadedDocumentsAsync()
        {
            return await _context.DocumentDownloads
                .Include(dd => dd.Document)
                .ThenInclude(d => d.Department)
                .GroupBy(dd => dd.Document)
                .Select(g => new TopDownloadedDocumentViewModel
                {
                    FileName = g.Key.OriginalFileName,
                    DepartmentName = g.Key.Department != null ? g.Key.Department.Name : "Geral",
                    DownloadCount = g.Count(),
                    LastDownload = g.Max(dd => dd.DownloadDate)
                })
                .OrderByDescending(x => x.DownloadCount)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<MonthlyDocumentStatsViewModel>> GetMonthlyDocumentStatsAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var cultureInfo = new CultureInfo("pt-BR");

            var monthlyUploads = await _context.Documents
                .Where(d => d.UploadDate >= sixMonthsAgo)
                .GroupBy(d => new { d.UploadDate.Year, d.UploadDate.Month })
                .Select(g => new MonthlyDocumentStatsViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = cultureInfo.DateTimeFormat.GetMonthName(g.Key.Month),
                    DocumentsUploaded = g.Count(),
                    TotalDownloads = 0 // Será preenchido abaixo
                })
                .ToListAsync();

            // Preencher downloads mensais
            foreach (var month in monthlyUploads)
            {
                month.TotalDownloads = await _context.DocumentDownloads
                    .Where(dd => dd.DownloadDate.Year == month.Year && dd.DownloadDate.Month == month.Month)
                    .CountAsync();
            }

            return monthlyUploads.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
        }

        private async Task<List<ReuniaoPorTipoViewModel>> GetReuniaoPorTipoAsync()
        {
            var totalReunioes = await _context.Reunioes.CountAsync();
            if (totalReunioes == 0) return new List<ReuniaoPorTipoViewModel>();

            return await _context.Reunioes
                .GroupBy(r => r.TipoReuniao)
                .Select(g => new ReuniaoPorTipoViewModel
                {
                    TipoReuniao = g.Key.GetDisplayName(),
                    Count = g.Count(),
                    Percentage = (double)g.Count() / totalReunioes * 100
                })
                .ToListAsync();
        }

        private async Task<List<ReuniaoPorStatusViewModel>> GetReuniaoPorStatusAsync()
        {
            var totalReunioes = await _context.Reunioes.CountAsync();
            if (totalReunioes == 0) return new List<ReuniaoPorStatusViewModel>();

            var now = DateTime.Now;
            var reunioesPendentes = await _context.Reunioes.Where(r => r.Data.Date >= now.Date).CountAsync();
            var reunioesPassadas = await _context.Reunioes.Where(r => r.Data.Date < now.Date).CountAsync();

            return new List<ReuniaoPorStatusViewModel>
            {
                new ReuniaoPorStatusViewModel
                {
                    Status = "Pendentes",
                    Count = reunioesPendentes,
                    Percentage = (double)reunioesPendentes / totalReunioes * 100
                },
                new ReuniaoPorStatusViewModel
                {
                    Status = "Passadas",
                    Count = reunioesPassadas,
                    Percentage = (double)reunioesPassadas / totalReunioes * 100
                }
            };
        }        private async Task<List<ReuniaoPorDepartamentoViewModel>> GetReuniaoPorDepartamentoAsync()
        {
            var totalReunioes = await _context.Reunioes.CountAsync();
            if (totalReunioes == 0) return new List<ReuniaoPorDepartamentoViewModel>();            return await _context.Reunioes
                .Include(r => r.ResponsavelUser)
                .ThenInclude(u => u!.Department)
                .GroupBy(r => r.ResponsavelUser != null && r.ResponsavelUser.Department != null ? r.ResponsavelUser.Department.Name : "Sem Departamento")
                .Select(g => new ReuniaoPorDepartamentoViewModel
                {
                    DepartmentName = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / totalReunioes * 100
                })
                .ToListAsync();
        }

        private async Task<List<ReuniaoPorMesViewModel>> GetReuniaoPorMesAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var cultureInfo = new CultureInfo("pt-BR");

            return await _context.Reunioes
                .Where(r => r.Data >= sixMonthsAgo)
                .GroupBy(r => new { r.Data.Year, r.Data.Month })
                .Select(g => new ReuniaoPorMesViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = cultureInfo.DateTimeFormat.GetMonthName(g.Key.Month),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        private async Task<List<DepartmentStatsViewModel>> GetDepartmentStatsAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            var departmentStats = new List<DepartmentStatsViewModel>();

            foreach (var dept in departments)
            {
                var userCount = await _context.Users.Where(u => u.DepartmentId == dept.Id).CountAsync();
                var documentCount = await _context.Documents.Where(d => d.DepartmentId == dept.Id).CountAsync();
                var downloadCount = await _context.DocumentDownloads
                    .Include(dd => dd.Document)
                    .Where(dd => dd.Document.DepartmentId == dept.Id)
                    .CountAsync();                var reunioesCount = await _context.Reunioes
                    .Include(r => r.ResponsavelUser)
                    .Where(r => r.ResponsavelUser != null && r.ResponsavelUser.DepartmentId == dept.Id)
                    .CountAsync();

                // Cálculo de pontuação de atividade (peso: documentos=2, downloads=1, reuniões=3)
                var activityScore = (documentCount * 2) + downloadCount + (reunioesCount * 3);

                departmentStats.Add(new DepartmentStatsViewModel
                {
                    DepartmentName = dept.Name,
                    UserCount = userCount,
                    DocumentCount = documentCount,
                    DownloadCount = downloadCount,
                    ReunioesCount = reunioesCount,
                    ActivityScore = activityScore
                });
            }

            // Adicionar estatísticas para documentos "Geral" (sem departamento)
            var geralDocuments = await _context.Documents.Where(d => d.DepartmentId == null).CountAsync();
            var geralDownloads = await _context.DocumentDownloads
                .Include(dd => dd.Document)
                .Where(dd => dd.Document.DepartmentId == null)
                .CountAsync();

            if (geralDocuments > 0 || geralDownloads > 0)
            {
                departmentStats.Add(new DepartmentStatsViewModel
                {
                    DepartmentName = "Geral",
                    UserCount = 0,
                    DocumentCount = geralDocuments,
                    DownloadCount = geralDownloads,
                    ReunioesCount = 0,
                    ActivityScore = (geralDocuments * 2) + geralDownloads
                });
            }

            return departmentStats.OrderByDescending(x => x.ActivityScore).ToList();
        }

        private async Task<List<UserActivityViewModel>> GetTopActiveUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .ToListAsync();

            var userActivities = new List<UserActivityViewModel>();

            foreach (var user in users)
            {                var documentsUploaded = await _context.Documents.Where(d => d.UploaderId == user.Id).CountAsync();
                var documentsDownloaded = await _context.DocumentDownloads.Where(dd => dd.UserId == user.Id).CountAsync();
                var reunioesCreated = await _context.Reunioes.Where(r => r.ResponsavelUserId == user.Id).CountAsync();

                // Cálculo de pontuação de atividade
                var activityScore = (documentsUploaded * 3) + documentsDownloaded + (reunioesCreated * 5);

                if (activityScore > 0)
                {
                    userActivities.Add(new UserActivityViewModel
                    {
                        UserName = user.UserName ?? "Usuário",
                        DepartmentName = user.Department?.Name ?? "Sem Departamento",
                        DocumentsUploaded = documentsUploaded,
                        DocumentsDownloaded = documentsDownloaded,
                        ReunioesCreated = reunioesCreated,
                        ActivityScore = activityScore
                    });
                }
            }

            return userActivities.OrderByDescending(x => x.ActivityScore).Take(10).ToList();
        }
    }
}
