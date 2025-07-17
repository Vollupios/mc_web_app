using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Services.Documents;
using Microsoft.AspNetCore.Identity;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço de documentos refatorado usando Composition Pattern
    /// Implementa princípios SOLID, especialmente SRP e ISP
    /// Usa delegation para responsabilidades segregadas
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentReader _documentReader;
        private readonly IDocumentWriter _documentWriter;
        private readonly IDocumentSecurity _documentSecurity;
        private readonly IDocumentDownloader _documentDownloader;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentReader documentReader,
            IDocumentWriter documentWriter,
            IDocumentSecurity documentSecurity,
            IDocumentDownloader documentDownloader,
            ILogger<DocumentService> logger)
        {
            _documentReader = documentReader;
            _documentWriter = documentWriter;
            _documentSecurity = documentSecurity;
            _documentDownloader = documentDownloader;
            _logger = logger;
        }

        #region IDocumentService Implementation - Delegation Pattern

        public async Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user)
        {
            return await _documentReader.GetDocumentsForUserAsync(user);
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _documentReader.GetDocumentByIdAsync(id);
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId)
        {
            return await _documentWriter.SaveDocumentAsync(file, uploader, departmentId);
        }

        public async Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser)
        {
            return await _documentWriter.DeleteDocumentAsync(id, currentUser);
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
        {
            return await _documentSecurity.CanUserAccessDocumentAsync(documentId, user);
        }        public async Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user)
        {
            return await _documentSecurity.CanUserUploadToDepartmentAsync(departmentId, user);
        }

        public async Task<string> GetDocumentPhysicalPathAsync(Document document)
        {
            if (document?.Id == null)
            {
                _logger.LogWarning("Documento nulo ou sem ID fornecido para obter caminho físico");
                return string.Empty;
            }            var result = await _documentDownloader.GetDocumentPhysicalPathAsync(document.Id);
            return result ?? string.Empty;
        }

        public async Task<List<Department>> GetDepartmentsForUserAsync(ApplicationUser user)
        {
            return await _documentSecurity.GetAccessibleDepartmentsAsync(user);
        }        public async Task<List<Document>> SearchDocumentsAsync(string searchTerm, ApplicationUser user)
        {
            return await _documentReader.SearchDocumentsAsync(searchTerm, user);
        }

        public Task<bool> IsFileTypeAllowed(string fileName)
        {
            // Delegation para validação de tipo de arquivo
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return Task.FromResult(IsFileTypeAllowedInternal(extension));
        }

        public async Task<bool> IsFileSizeAllowed(long fileSize)
        {
            // Validação simples - pode ser estendida para validação por tipo
            const long maxSize = 10 * 1024 * 1024; // 10MB padrão
            return await Task.FromResult(fileSize <= maxSize);
        }

        #endregion

        #region Métodos Privados

        private static bool IsFileTypeAllowedInternal(string extension)
        {
            var allowedExtensions = new[]
            {
                // Documentos
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".txt", ".rtf", ".odt", ".ods", ".odp",
                
                // Imagens
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp",
                
                // Arquivos compactados
                ".zip", ".rar", ".7z",
                
                // Outros
                ".csv"
            };

            return allowedExtensions.Contains(extension);
        }

        #endregion

        #region Novos Métodos com Composition

        /// <summary>
        /// Obtém dados do documento para download
        /// </summary>
        public async Task<(byte[] FileData, string ContentType, string FileName)?> GetDocumentForDownloadAsync(
            int documentId, ApplicationUser user)
        {
            return await _documentDownloader.GetDocumentForDownloadAsync(documentId, user);
        }

        /// <summary>
        /// Obtém stream do documento
        /// </summary>
        public async Task<Stream?> GetDocumentStreamAsync(int documentId, ApplicationUser user)
        {
            return await _documentDownloader.GetDocumentStreamAsync(documentId, user);
        }

        /// <summary>
        /// Verifica se documento existe fisicamente
        /// </summary>
        public async Task<bool> DocumentExistsAsync(int documentId)
        {
            return await _documentDownloader.DocumentExistsAsync(documentId);
        }

        /// <summary>
        /// Verifica se usuário pode deletar documento específico
        /// </summary>
        public async Task<bool> CanUserDeleteDocumentAsync(int documentId, ApplicationUser user)
        {
            return await _documentSecurity.CanUserDeleteDocumentAsync(documentId, user);
        }

        /// <summary>
        /// Verifica se usuário está em uma role específica
        /// </summary>
        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string role)
        {
            return await _documentSecurity.IsUserInRoleAsync(user, role);
        }

        public async Task<List<Document>> GetDocumentsByDepartmentAsync(int departmentId, ApplicationUser user)
        {
            return await _documentReader.GetDocumentsByDepartmentAsync(departmentId, user);
        }

        public async Task<DocumentStatistics> GetDocumentStatisticsAsync(ApplicationUser user)
        {
            return await _documentReader.GetDocumentStatisticsAsync(user);
        }

        public async Task<bool> UpdateDocumentAsync(int id, string fileName, string? description, ApplicationUser currentUser)
        {
            return await _documentWriter.UpdateDocumentAsync(id, fileName, description, currentUser);
        }

        public async Task<bool> MoveDocumentToDepartmentAsync(int documentId, int? newDepartmentId, ApplicationUser currentUser)
        {
            return await _documentWriter.MoveDocumentToDepartmentAsync(documentId, newDepartmentId, currentUser);
        }

        public async Task<bool> ArchiveDocumentAsync(int documentId, bool archive, ApplicationUser currentUser)
        {
            return await _documentWriter.ArchiveDocumentAsync(documentId, archive, currentUser);
        }        public async Task<List<Department>> GetAccessibleDepartmentsAsync(ApplicationUser user)
        {
            return await _documentSecurity.GetAccessibleDepartmentsAsync(user);
        }

        public async Task<string?> GetDocumentPhysicalPathAsync(int documentId)
        {
            return await _documentDownloader.GetDocumentPhysicalPathAsync(documentId);
        }

        public async Task<List<Document>> AdvancedSearchAsync(string? searchTerm, int? departmentId, string? contentType, DateTime? startDate, DateTime? endDate, ApplicationUser user)
        {
            return await _documentReader.AdvancedSearchAsync(searchTerm, departmentId, contentType, startDate, endDate, user);
        }

        public async Task<(bool Success, string? Message)> MoveDocumentAsync(int documentId, int? newFolderId, int? newDepartmentId, string userId)
        {
            return await _documentWriter.MoveDocumentAsync(documentId, newFolderId, newDepartmentId, userId);
        }

        public async Task<bool> CanUserAccessFolderAsync(DocumentFolder folder, ApplicationUser user)
        {
            return await _documentSecurity.CanUserAccessFolderAsync(folder, user);
        }

        #endregion
    }
}
