using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Interfaces.Services;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Refactored
{
    /// <summary>
    /// Serviço de download de documentos aplicando SRP
    /// Responsabilidade única: operações de download e streaming
    /// </summary>
    public class DocumentDownloadService : IDocumentDownloadService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentSecurityService _securityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentDownloadService(
            IDocumentRepository documentRepository,
            IDocumentSecurityService securityService,
            UserManager<ApplicationUser> userManager)
        {
            _documentRepository = documentRepository;
            _securityService = securityService;
            _userManager = userManager;
        }

        public async Task<(Document? Document, Stream? Stream, string? ErrorMessage)> GetDocumentForDownloadAsync(int documentId, ApplicationUser user)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    return (null, null, "Documento não encontrado");

                // Verificar permissões
                if (!await _securityService.CanUserDownloadDocumentAsync(documentId, user))
                    return (null, null, "Usuário não tem permissão para acessar este documento");

                // Verificar se o arquivo existe fisicamente
                var filePath = GetDocumentPhysicalPath(document);
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return (null, null, "Arquivo físico não encontrado");

                // Registrar o download
                await RegisterDownloadAsync(documentId, user);

                // Criar stream do arquivo
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                
                return (document, stream, null);
            }
            catch (Exception ex)
            {
                return (null, null, ex.Message);
            }
        }

        public async Task<Stream?> GetDocumentStreamAsync(int documentId, ApplicationUser user)
        {
            var result = await GetDocumentForDownloadAsync(documentId, user);
            return result.Stream;
        }

        public async Task<string?> GetDocumentPhysicalPathAsync(int documentId, ApplicationUser user)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return null;

            // Verificar permissões
            if (!await _securityService.CanUserDownloadDocumentAsync(documentId, user))
                return null;

            // Construir o caminho físico do arquivo
            var filePath = GetDocumentPhysicalPath(document);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            return filePath;
        }

        public async Task<bool> DocumentExistsAsync(int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return false;

            var filePath = GetDocumentPhysicalPath(document);
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }

        public async Task<bool> CanDownloadAsync(int documentId, ApplicationUser user)
        {
            return await _securityService.CanUserDownloadDocumentAsync(documentId, user);
        }

        public async Task RegisterDownloadAsync(int documentId, ApplicationUser user, string? ipAddress = null)
        {
            try
            {
                // Implementação opcional de log de downloads
                // Por enquanto apenas incrementar um contador se existir
                // Em uma implementação completa, salvaria em uma tabela de logs
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null)
                {
                    // Aqui seria criado um registro na tabela DocumentDownloadLog
                    // Por enquanto apenas um placeholder
                    await Task.CompletedTask;
                }
            }
            catch
            {
                // Log de download é opcional, não deve falhar o download
            }
        }

        public async Task<IEnumerable<DocumentDownloadLog>> GetDownloadHistoryAsync(int documentId)
        {
            // Implementação futura com repository específico para logs
            // Por enquanto retorna lista vazia
            await Task.CompletedTask;
            return new List<DocumentDownloadLog>();
        }

        public async Task<IEnumerable<DocumentDownloadLog>> GetUserDownloadHistoryAsync(string userId, int take = 50)
        {
            // Implementação futura com repository específico para logs
            // Por enquanto retorna lista vazia
            await Task.CompletedTask;
            return new List<DocumentDownloadLog>();
        }

        public string GetContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "application/octet-stream";

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }

        public string GetSafeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "documento";

            // Remover caracteres não seguros
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Limitar tamanho
            if (safeName.Length > 100)
                safeName = safeName.Substring(0, 100);

            return string.IsNullOrEmpty(safeName) ? "documento" : safeName;
        }

        public async Task<long> GetFileSizeAsync(int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                return 0;

            // Tentar obter o tamanho do banco primeiro
            if (document.FileSize > 0)
                return document.FileSize;

            // Se não tem no banco, tentar obter do arquivo físico
            var filePath = GetDocumentPhysicalPath(document);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }

            return 0;
        }

        /// <summary>
        /// Constrói o caminho físico do arquivo baseado no departamento e nome armazenado
        /// </summary>
        private string GetDocumentPhysicalPath(Document document)
        {
            // Determinar o diretório baseado no departamento
            var departmentName = "Geral"; // Padrão
            
            // Aqui seria ideal buscar o nome do departamento pelo ID
            // Por enquanto usando uma lógica simples baseada no DepartmentId
            if (document.DepartmentId.HasValue)
            {
                departmentName = document.DepartmentId.Value switch
                {
                    1 => "Pessoal",
                    2 => "Fiscal", 
                    3 => "Contábil",
                    4 => "Cadastro",
                    5 => "Apoio",
                    6 => "TI",
                    _ => "Geral"
                };
            }

            var uploadsPath = Path.Combine("DocumentsStorage", departmentName);
            return Path.Combine(uploadsPath, document.StoredFileName);
        }
    }
}
