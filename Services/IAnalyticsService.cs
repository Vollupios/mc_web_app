using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Interface para o serviço de analytics
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Obtém o dashboard completo de analytics
        /// </summary>
        Task<DashboardViewModel> GetDashboardDataAsync();
        
        /// <summary>
        /// Registra um download de documento
        /// </summary>
        Task RegisterDocumentDownloadAsync(int documentId, string userId, string userAgent, string ipAddress);
        
        /// <summary>
        /// Obtém estatísticas de documentos
        /// </summary>
        Task<DocumentStatisticsViewModel> GetDocumentStatisticsAsync();
        
        /// <summary>
        /// Obtém métricas de reuniões
        /// </summary>
        Task<ReunioesMetricsViewModel> GetReunioesMetricsAsync();
        
        /// <summary>
        /// Obtém atividade por departamento
        /// </summary>
        Task<DepartmentActivityViewModel> GetDepartmentActivityAsync();
    }
}
