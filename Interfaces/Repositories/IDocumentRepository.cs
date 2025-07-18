using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Repositories
{
    /// <summary>
    /// Interface específica para repositório de documentos
    /// Aplicando Interface Segregation Principle (ISP)
    /// </summary>
    public interface IDocumentRepository : IRepository<Document, int>
    {
        // Operações específicas de documento
        Task<IEnumerable<Document>> GetByDepartmentAsync(int? departmentId, string? userId = null);
        Task<IEnumerable<Document>> GetByFolderAsync(int? folderId);
        Task<IEnumerable<Document>> SearchAsync(string searchTerm, int? departmentId = null);
        Task<IEnumerable<Document>> GetByUploaderAsync(string uploaderId);
        Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Document>> GetRecentAsync(int count = 10);
        Task<IEnumerable<Document>> GetMostDownloadedAsync(int count = 10);
        
        // Estatísticas específicas
        Task<int> CountByDepartmentAsync(int? departmentId);
        Task<long> GetTotalFileSizeByDepartmentAsync(int? departmentId);
        Task<Dictionary<string, int>> GetFileTypeStatsAsync();
        
        // Operações de auditoria
        Task<IEnumerable<Document>> GetArchivedAsync();
        Task<IEnumerable<Document>> GetByStatusAsync(string status);
    }
}
