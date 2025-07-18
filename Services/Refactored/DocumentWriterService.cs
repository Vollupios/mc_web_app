using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Interfaces.Services;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Refactored
{
    /// <summary>
    /// Serviço de escrita de documentos aplicando SRP
    /// Responsabilidade única: operações de criação/edição/exclusão
    /// </summary>
    public class DocumentWriterService : IDocumentWriterService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentSecurityService _securityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentWriterService(
            IDocumentRepository documentRepository,
            IDocumentSecurityService securityService,
            UserManager<ApplicationUser> userManager)
        {
            _documentRepository = documentRepository;
            _securityService = securityService;
            _userManager = userManager;
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId, int? folderId = null)
        {
            if (file == null || uploader == null)
                throw new ArgumentNullException("File and uploader are required");

            // Validações de segurança
            if (!_securityService.IsFileTypeAllowed(file.FileName))
                throw new InvalidOperationException("Tipo de arquivo não permitido");

            if (!_securityService.IsFileSizeAllowed(file.Length))
                throw new InvalidOperationException("Arquivo muito grande. Máximo 10MB");

            if (!await _securityService.CanUserUploadToFolderAsync(folderId, uploader))
                throw new UnauthorizedAccessException("Usuário não tem permissão para fazer upload nesta pasta");

            // Criar o documento
            int? targetDepartmentId = departmentId;
            if (!targetDepartmentId.HasValue)
                targetDepartmentId = uploader.DepartmentId;
            if (!targetDepartmentId.HasValue)
                targetDepartmentId = 1;
            
            var document = new Document
            {
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.UtcNow,
                UploaderId = uploader.Id,
                DepartmentId = targetDepartmentId,
                FolderId = folderId,
                Status = DocumentStatus.Published
            };

            // Gerar nome único para o arquivo
            var fileExtension = Path.GetExtension(file.FileName);
            document.StoredFileName = $"{Guid.NewGuid()}{fileExtension}";

            // Salvar o arquivo físico
            await SaveFileAsync(file, document, departmentId);

            // Salvar no banco
            return await _documentRepository.AddAsync(document);
        }

        public async Task<bool> UpdateDocumentAsync(int id, string fileName, string? description, ApplicationUser currentUser)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return false;

            // Verificar permissões
            if (!await _securityService.CanUserEditDocumentAsync(id, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este documento");

            // Atualizar campos
            if (!string.IsNullOrEmpty(fileName))
                document.OriginalFileName = fileName;

            if (!string.IsNullOrEmpty(description))
                document.Description = description;

            await _documentRepository.UpdateAsync(document);
            return true;
        }

        public async Task<bool> UpdateDocumentMetadataAsync(int id, string? description, ApplicationUser currentUser)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return false;

            // Verificar permissões
            if (!await _securityService.CanUserEditDocumentAsync(id, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para editar este documento");

            // Atualizar descrição
            if (!string.IsNullOrEmpty(description))
                document.Description = description;

            await _documentRepository.UpdateAsync(document);
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                return false;

            // Verificar permissões
            if (!await _securityService.CanUserDeleteDocumentAsync(id, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para excluir este documento");

            // Marcar como arquivado ao invés de excluir fisicamente
            document.Status = DocumentStatus.Archived;
            await _documentRepository.UpdateAsync(document);

            return true;
        }

        public async Task<bool> ArchiveDocumentAsync(int documentId, bool archive, ApplicationUser currentUser)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return false;

            // Verificar permissões
            if (!await _securityService.CanUserEditDocumentAsync(documentId, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para arquivar este documento");

            // Alterar status
            document.Status = archive ? DocumentStatus.Archived : DocumentStatus.Published;
            await _documentRepository.UpdateAsync(document);

            return true;
        }

        public async Task<bool> MoveDocumentToDepartmentAsync(int documentId, int? newDepartmentId, ApplicationUser currentUser)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return false;

            // Verificar permissões no documento original
            if (!await _securityService.CanUserEditDocumentAsync(documentId, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para mover este documento");

            // Verificar permissões no departamento de destino
            if (newDepartmentId.HasValue && !await _securityService.CanUserUploadToDepartmentAsync(newDepartmentId, currentUser))
                throw new UnauthorizedAccessException("Usuário não tem permissão para mover para este departamento");

            // Atualizar o departamento
            if (newDepartmentId.HasValue)
                document.DepartmentId = newDepartmentId.Value;

            await _documentRepository.UpdateAsync(document);
            return true;
        }

        public async Task<(bool Success, string? Message)> MoveDocumentAsync(int documentId, int? newFolderId, int? newDepartmentId, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return (false, "Usuário não encontrado");

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    return (false, "Documento não encontrado");

                // Verificar permissões
                if (!await _securityService.CanUserEditDocumentAsync(documentId, user))
                    return (false, "Usuário não tem permissão para mover este documento");

                // Atualizar localização
                if (newFolderId.HasValue)
                    document.FolderId = newFolderId;

                if (newDepartmentId.HasValue)
                    document.DepartmentId = newDepartmentId.Value;

                await _documentRepository.UpdateAsync(document);
                return (true, "Documento movido com sucesso");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Message)> BulkMoveDocumentsAsync(IEnumerable<int> documentIds, int? newFolderId, int? newDepartmentId, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return (false, "Usuário não encontrado");

                var successCount = 0;
                var totalCount = documentIds.Count();

                foreach (var documentId in documentIds)
                {
                    var result = await MoveDocumentAsync(documentId, newFolderId, newDepartmentId, userId);
                    if (result.Success)
                        successCount++;
                }

                if (successCount == totalCount)
                    return (true, $"Todos os {totalCount} documentos foram movidos com sucesso");
                else if (successCount > 0)
                    return (true, $"{successCount} de {totalCount} documentos foram movidos");
                else
                    return (false, "Nenhum documento pôde ser movido");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task SaveFileAsync(IFormFile file, Document document, int? departmentId)
        {
            // Determinar o diretório baseado no departamento
            var departmentName = "Geral"; // Padrão
            
            // Aqui seria ideal buscar o nome do departamento pelo ID
            // Por enquanto usando uma lógica simples
            var uploadsPath = Path.Combine("DocumentsStorage", departmentName);
            
            // Criar diretório se não existir
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Caminho completo do arquivo
            var filePath = Path.Combine(uploadsPath, document.StoredFileName);

            // Salvar o arquivo
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Note: Document model doesn't have FilePath property
            // The file path is constructed dynamically when needed
        }
    }
}
