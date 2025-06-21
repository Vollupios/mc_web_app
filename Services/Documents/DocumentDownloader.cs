using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Implementação para download de documentos
    /// Responsabilidade única: gerenciar downloads e acesso aos arquivos
    /// </summary>
    public class DocumentDownloader : IDocumentDownloader
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentDownloader> _logger;
        private readonly IDocumentSecurity _documentSecurity;

        public DocumentDownloader(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<DocumentDownloader> logger,
            IDocumentSecurity documentSecurity)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _documentSecurity = documentSecurity;
        }

        public async Task<(byte[] FileData, string ContentType, string FileName)?> GetDocumentForDownloadAsync(
            int documentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo tentando fazer download do documento {Id}", documentId);
                return null;
            }

            try
            {
                // Verificar permissões
                if (!await _documentSecurity.CanUserAccessDocumentAsync(documentId, user))
                {
                    _logger.LogWarning("Usuário {UserId} tentou download sem permissão do documento {DocId}", 
                        user.Id, documentId);
                    return null;
                }

                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para download: {Id}", documentId);
                    return null;
                }

                var physicalPath = GetDocumentPhysicalPath(document);
                
                if (!File.Exists(physicalPath))
                {
                    _logger.LogError("Arquivo físico não encontrado: {Path} para documento {Id}", 
                        physicalPath, documentId);
                    return null;
                }

                var fileData = await File.ReadAllBytesAsync(physicalPath);
                
                _logger.LogInformation("Download realizado: documento {DocId} por usuário {UserId}", 
                    documentId, user.Id);

                return (fileData, document.ContentType, document.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer download do documento {Id}", documentId);
                return null;
            }
        }

        public async Task<Stream?> GetDocumentStreamAsync(int documentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo tentando obter stream do documento {Id}", documentId);
                return null;
            }

            try
            {
                // Verificar permissões
                if (!await _documentSecurity.CanUserAccessDocumentAsync(documentId, user))
                {
                    _logger.LogWarning("Usuário {UserId} tentou stream sem permissão do documento {DocId}", 
                        user.Id, documentId);
                    return null;
                }

                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para stream: {Id}", documentId);
                    return null;
                }

                var physicalPath = GetDocumentPhysicalPath(document);
                
                if (!File.Exists(physicalPath))
                {
                    _logger.LogError("Arquivo físico não encontrado: {Path} para documento {Id}", 
                        physicalPath, documentId);
                    return null;
                }

                _logger.LogInformation("Stream fornecido: documento {DocId} por usuário {UserId}", 
                    documentId, user.Id);

                return new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter stream do documento {Id}", documentId);
                return null;
            }
        }

        public async Task<string?> GetDocumentPhysicalPathAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para obter caminho físico: {Id}", documentId);
                    return null;
                }

                return GetDocumentPhysicalPath(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter caminho físico do documento {Id}", documentId);
                return null;
            }
        }

        public async Task<bool> DocumentExistsAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                    return false;

                var physicalPath = GetDocumentPhysicalPath(document);
                return File.Exists(physicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do documento {Id}", documentId);
                return false;
            }
        }

        #region Métodos Privados

        private string GetDocumentPhysicalPath(Document document)
        {
            var folderName = document.Department?.Name ?? "Geral";
            return Path.Combine(_environment.ContentRootPath, "DocumentsStorage", folderName, document.StoredFileName);
        }

        #endregion
    }
}
