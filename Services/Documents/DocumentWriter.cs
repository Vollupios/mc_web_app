using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Services.FileProcessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Implementação para gravação de documentos
    /// Responsabilidade única: criar e salvar documentos
    /// </summary>
    public class DocumentWriter : IDocumentWriter
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentWriter> _logger;
        private readonly FileProcessorFactory _fileProcessorFactory;
        private readonly IDocumentSecurity _documentSecurity;

        public DocumentWriter(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<DocumentWriter> logger,
            FileProcessorFactory fileProcessorFactory,
            IDocumentSecurity documentSecurity)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
            _fileProcessorFactory = fileProcessorFactory;
            _documentSecurity = documentSecurity;
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Arquivo não fornecido ou vazio", nameof(file));

            if (uploader == null)
                throw new ArgumentNullException(nameof(uploader));

            try
            {
                // Validar permissões
                if (!await _documentSecurity.CanUserUploadToDepartmentAsync(departmentId, uploader))
                {
                    throw new UnauthorizedAccessException("Usuário não tem permissão para fazer upload neste departamento");
                }

                // Validar arquivo usando Factory Pattern
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!IsFileTypeAllowed(fileExtension))
                {
                    throw new InvalidOperationException($"Tipo de arquivo não permitido: {fileExtension}");
                }

                if (!IsFileSizeAllowed(file.Length, fileExtension))
                {
                    throw new InvalidOperationException($"Arquivo muito grande. Tamanho máximo permitido: {GetMaxFileSizeForExtension(fileExtension) / (1024 * 1024)}MB");
                }

                // Obter processador adequado
                var processor = _fileProcessorFactory.GetProcessor(fileExtension);
                
                // Gerar nome único para o arquivo
                var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                // Determinar pasta de destino
                var folderName = departmentId.HasValue 
                    ? (await _context.Departments.FindAsync(departmentId.Value))?.Name ?? "Geral"
                    : "Geral";
                
                var documentsPath = Path.Combine(_environment.ContentRootPath, "DocumentsStorage", folderName);
                Directory.CreateDirectory(documentsPath);
                
                var filePath = Path.Combine(documentsPath, storedFileName);

                // Processar arquivo usando Strategy Pattern
                var processingResult = await processor.ProcessAsync(file, filePath);
                
                if (!processingResult.Success)
                {
                    var errors = string.Join("; ", processingResult.Errors);
                    throw new InvalidOperationException($"Erro ao processar arquivo: {errors}");
                }                // Criar registro no banco
                var document = new Document
                {
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.Now,
                    UploaderId = uploader.Id,
                    DepartmentId = departmentId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento salvo com sucesso: {FileName} por {UserId} usando {ProcessorType}", 
                    file.FileName, uploader.Id, processor.GetType().Name);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar documento: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException(nameof(currentUser));

            try
            {
                var document = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para exclusão: {Id}", id);
                    return false;
                }

                // Verificar permissões usando IDocumentSecurity
                if (!await _documentSecurity.CanUserAccessDocumentAsync(id, currentUser))
                {
                    _logger.LogWarning("Usuário {UserId} tentou excluir documento {DocId} sem permissão", 
                        currentUser.Id, id);
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Verificar se pode excluir (Admin ou próprio uploader)
                if (!userRoles.Contains("Admin") && document.UploaderId != currentUser.Id)
                {
                    _logger.LogWarning("Usuário {UserId} tentou excluir documento {DocId} de outro usuário", 
                        currentUser.Id, id);
                    return false;
                }

                // Remover arquivo físico
                var physicalPath = GetDocumentPhysicalPath(document);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                // Remover registro do banco
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento excluído: {Id} por {User}", id, currentUser.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento {Id}", id);
                return false;
            }
        }

        public async Task<bool> UpdateDocumentAsync(int id, string fileName, string? description, ApplicationUser currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException(nameof(currentUser));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Nome do arquivo não pode ser vazio", nameof(fileName));

            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para atualização: {Id}", id);
                    return false;
                }

                // Verificar permissões
                if (!await _documentSecurity.CanUserAccessDocumentAsync(id, currentUser))
                {
                    _logger.LogWarning("Usuário {UserId} tentou atualizar documento {DocId} sem permissão", 
                        currentUser.Id, id);
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Verificar se pode editar (Admin ou próprio uploader)
                if (!userRoles.Contains("Admin") && document.UploaderId != currentUser.Id)
                {
                    _logger.LogWarning("Usuário {UserId} tentou editar documento {DocId} de outro usuário", 
                        currentUser.Id, id);
                    return false;
                }

                // Atualizar metadados
                document.OriginalFileName = fileName;
                // Note: description não está no modelo atual, poderia ser adicionado futuramente

                _context.Documents.Update(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento atualizado: {Id} por {User}", id, currentUser.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar documento {Id}", id);
                return false;
            }
        }

        public async Task<bool> MoveDocumentToDepartmentAsync(int documentId, int? newDepartmentId, ApplicationUser currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException(nameof(currentUser));

            try
            {
                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para mover: {Id}", documentId);
                    return false;
                }

                // Verificar permissões para o documento atual
                if (!await _documentSecurity.CanUserAccessDocumentAsync(documentId, currentUser))
                {
                    _logger.LogWarning("Usuário {UserId} tentou mover documento {DocId} sem permissão", 
                        currentUser.Id, documentId);
                    return false;
                }

                // Verificar permissões para o novo departamento
                if (!await _documentSecurity.CanUserUploadToDepartmentAsync(newDepartmentId, currentUser))
                {
                    _logger.LogWarning("Usuário {UserId} tentou mover documento para departamento {DeptId} sem permissão", 
                        currentUser.Id, newDepartmentId);
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Somente Admin pode mover documentos
                if (!userRoles.Contains("Admin"))
                {
                    _logger.LogWarning("Usuário {UserId} tentou mover documento sem permissão de Admin", 
                        currentUser.Id);
                    return false;
                }

                // Determinar pastas origem e destino
                var oldFolderName = document.Department?.Name ?? "Geral";
                var newFolderName = newDepartmentId.HasValue 
                    ? (await _context.Departments.FindAsync(newDepartmentId.Value))?.Name ?? "Geral"
                    : "Geral";

                // Se o departamento for o mesmo, não precisa mover o arquivo
                if (document.DepartmentId != newDepartmentId)
                {
                    var oldPath = Path.Combine(_environment.ContentRootPath, "DocumentsStorage", oldFolderName, document.StoredFileName);
                    var newPath = Path.Combine(_environment.ContentRootPath, "DocumentsStorage", newFolderName, document.StoredFileName);

                    // Criar pasta de destino se não existir
                    var newDir = Path.GetDirectoryName(newPath);
                    if (!string.IsNullOrEmpty(newDir))
                    {
                        Directory.CreateDirectory(newDir);
                    }

                    // Mover arquivo físico
                    if (File.Exists(oldPath))
                    {
                        File.Move(oldPath, newPath);
                    }

                    // Atualizar registro no banco
                    document.DepartmentId = newDepartmentId;

                    _context.Documents.Update(document);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Documento movido: {Id} de {OldDept} para {NewDept} por {User}", 
                        documentId, oldFolderName, newFolderName, currentUser.Email);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover documento {Id}", documentId);
                return false;
            }
        }

        public async Task<bool> ArchiveDocumentAsync(int documentId, bool archive, ApplicationUser currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException(nameof(currentUser));

            try
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Documento não encontrado para arquivar: {Id}", documentId);
                    return false;
                }

                // Verificar permissões
                if (!await _documentSecurity.CanUserAccessDocumentAsync(documentId, currentUser))
                {
                    _logger.LogWarning("Usuário {UserId} tentou arquivar documento {DocId} sem permissão", 
                        currentUser.Id, documentId);
                    return false;
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Verificar se pode arquivar (Admin ou próprio uploader)
                if (!userRoles.Contains("Admin") && document.UploaderId != currentUser.Id)
                {
                    _logger.LogWarning("Usuário {UserId} tentou arquivar documento {DocId} de outro usuário", 
                        currentUser.Id, documentId);
                    return false;
                }

                // Note: O modelo Document atual não tem campo IsArchived
                // Isso seria uma futura extensão do modelo
                // Por enquanto, só log da operação
                _logger.LogInformation("Documento {Action}: {Id} por {User}", 
                    archive ? "arquivado" : "desarquivado", documentId, currentUser.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao arquivar documento {Id}", documentId);
                return false;
            }
        }

        #region Métodos Privados de Validação

        private static bool IsFileTypeAllowed(string extension)
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

        private static bool IsFileSizeAllowed(long fileSize, string extension)
        {
            var maxSize = GetMaxFileSizeForExtension(extension);
            return fileSize <= maxSize;
        }

        private static long GetMaxFileSizeForExtension(string extension)
        {
            return extension switch
            {
                // Imagens: 5MB
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".webp" => 5 * 1024 * 1024,
                
                // Arquivos compactados: 20MB
                ".zip" or ".rar" or ".7z" => 20 * 1024 * 1024,
                
                // Documentos e outros: 10MB
                _ => 10 * 1024 * 1024
            };
        }

        private string GetDocumentPhysicalPath(Document document)
        {
            var folderName = document.Department?.Name ?? "Geral";
            return Path.Combine(_environment.ContentRootPath, "DocumentsStorage", folderName, document.StoredFileName);
        }

        #endregion
    }
}
