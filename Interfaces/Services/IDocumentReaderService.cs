using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Interfaces.Services
{
    /// <summary>
    /// Interface para operações de leitura de documentos
    /// Aplicando ISP - Interface Segregation Principle
    /// </summary>
    public interface IDocumentReaderService
    {
        // Operações básicas de leitura
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<IEnumerable<Document>> GetDocumentsForUserAsync(ApplicationUser user);
        Task<IEnumerable<Document>> GetDocumentsByDepartmentAsync(int? departmentId, ApplicationUser user);
        Task<IEnumerable<Document>> GetDocumentsByFolderAsync(int? folderId, ApplicationUser user);
        
        // Operações de busca
        Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm, ApplicationUser user, int? departmentId = null);
        Task<IEnumerable<Document>> AdvancedSearchAsync(string searchTerm, DateTime? startDate, DateTime? endDate, string? fileType, ApplicationUser user);
        
        // Operações de estatísticas
        Task<DocumentStatisticsViewModel> GetDocumentStatisticsAsync(ApplicationUser user);
        Task<IEnumerable<Document>> GetRecentDocumentsAsync(ApplicationUser user, int count = 10);
        Task<IEnumerable<Document>> GetMostDownloadedAsync(ApplicationUser user, int count = 10);
    }
}
