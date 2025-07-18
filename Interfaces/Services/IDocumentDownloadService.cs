using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Services
{
    /// <summary>
    /// Interface para operações de download de documentos
    /// Segregada para operações específicas de download (ISP)
    /// </summary>
    public interface IDocumentDownloadService
    {
        // Operações de download
        Task<(Document? Document, Stream? Stream, string? ErrorMessage)> GetDocumentForDownloadAsync(int documentId, ApplicationUser user);
        Task<Stream?> GetDocumentStreamAsync(int documentId, ApplicationUser user);
        Task<string?> GetDocumentPhysicalPathAsync(int documentId, ApplicationUser user);
        
        // Validações de download
        Task<bool> DocumentExistsAsync(int documentId);
        Task<bool> CanDownloadAsync(int documentId, ApplicationUser user);
        
        // Operações de auditoria
        Task RegisterDownloadAsync(int documentId, ApplicationUser user, string? ipAddress = null);
        Task<IEnumerable<DocumentDownloadLog>> GetDownloadHistoryAsync(int documentId);
        Task<IEnumerable<DocumentDownloadLog>> GetUserDownloadHistoryAsync(string userId, int take = 50);
        
        // Utilitários
        string GetContentType(string fileName);
        string GetSafeFileName(string fileName);
        Task<long> GetFileSizeAsync(int documentId);
    }
}
