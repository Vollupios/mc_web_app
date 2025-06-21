using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Interface para operações de leitura de documentos - ISP aplicado
    /// </summary>
    public interface IDocumentReader
    {
        /// <summary>
        /// Obtém documentos que o usuário pode acessar
        /// </summary>
        Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user);

        /// <summary>
        /// Obtém documento por ID
        /// </summary>
        Task<Document?> GetDocumentByIdAsync(int id);

        /// <summary>
        /// Busca documentos por termo
        /// </summary>
        Task<List<Document>> SearchDocumentsAsync(string searchTerm, ApplicationUser user);

        /// <summary>
        /// Obtém documentos por departamento
        /// </summary>
        Task<List<Document>> GetDocumentsByDepartmentAsync(int departmentId, ApplicationUser user);

        /// <summary>
        /// Obtém estatísticas de documentos
        /// </summary>
        Task<DocumentStatistics> GetDocumentStatisticsAsync(ApplicationUser user);
    }

    /// <summary>
    /// Estatísticas de documentos
    /// </summary>
    public class DocumentStatistics
    {
        public int TotalDocuments { get; set; }
        public int DocumentsThisMonth { get; set; }
        public long TotalSizeBytes { get; set; }
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<string, int> DocumentsByDepartment { get; set; } = new();
    }
}
