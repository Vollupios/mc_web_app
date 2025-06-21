using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Interface para operações de escrita de documentos - ISP aplicado
    /// </summary>
    public interface IDocumentWriter
    {
        /// <summary>
        /// Salva um novo documento
        /// </summary>
        Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId);

        /// <summary>
        /// Atualiza metadados de um documento
        /// </summary>
        Task<bool> UpdateDocumentAsync(int id, string fileName, string? description, ApplicationUser currentUser);

        /// <summary>
        /// Remove um documento
        /// </summary>
        Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser);

        /// <summary>
        /// Move documento para outro departamento
        /// </summary>
        Task<bool> MoveDocumentToDepartmentAsync(int documentId, int? newDepartmentId, ApplicationUser currentUser);

        /// <summary>
        /// Arquiva/desarquiva documento
        /// </summary>
        Task<bool> ArchiveDocumentAsync(int documentId, bool archive, ApplicationUser currentUser);
    }
}
