using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Services
{
    /// <summary>
    /// Interface para operações de escrita/modificação de documentos
    /// Segregada da interface de leitura (ISP)
    /// </summary>
    public interface IDocumentWriterService
    {
        // Operações de criação
        Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId, int? folderId = null);
        
        // Operações de atualização
        Task<bool> UpdateDocumentAsync(int id, string fileName, string? description, ApplicationUser currentUser);
        Task<bool> UpdateDocumentMetadataAsync(int id, string? description, ApplicationUser currentUser);
        
        // Operações de remoção
        Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser);
        Task<bool> ArchiveDocumentAsync(int documentId, bool archive, ApplicationUser currentUser);
        
        // Operações de movimentação
        Task<bool> MoveDocumentToDepartmentAsync(int documentId, int? newDepartmentId, ApplicationUser currentUser);
        Task<(bool Success, string? Message)> MoveDocumentAsync(int documentId, int? newFolderId, int? newDepartmentId, string userId);
        Task<(bool Success, string? Message)> BulkMoveDocumentsAsync(IEnumerable<int> documentIds, int? newFolderId, int? newDepartmentId, string userId);
    }
}
