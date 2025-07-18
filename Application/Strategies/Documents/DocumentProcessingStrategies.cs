using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.Strategies;
using IntranetDocumentos.Models;
using System.IO;

namespace IntranetDocumentos.Application.Strategies.Documents
{
    /// <summary>
    /// Contexto para processamento de documentos
    /// </summary>
    public class DocumentProcessingContext
    {
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int UserDepartmentId { get; set; }
        public string StorageBasePath { get; set; } = string.Empty;
        public int MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public bool RequireVirusCheck { get; set; } = false;
    }

    /// <summary>
    /// Resultado do processamento de documentos
    /// </summary>
    public class DocumentProcessingResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public Document? ProcessedDocument { get; set; }
        public string? StoredFilePath { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Estratégia para upload de documentos
    /// </summary>
    public class DocumentUploadStrategy : IContextStrategy<DocumentCreateDTO, DocumentProcessingResult, DocumentProcessingContext>
    {
        /// <summary>
        /// Executa o upload do documento
        /// </summary>
        /// <param name="input">DTO com dados do documento</param>
        /// <param name="context">Contexto do processamento</param>
        /// <returns>Resultado do processamento</returns>
        public DocumentProcessingResult Execute(DocumentCreateDTO input, DocumentProcessingContext context)
        {
            var result = new DocumentProcessingResult();

            try
            {
                // Validar entrada
                if (!ValidateInput(input, context, out var validationError))
                {
                    result.ErrorMessage = validationError;
                    return result;
                }

                // Gerar nome único para o arquivo
                var storedFileName = GenerateUniqueFileName(input.OriginalFileName);
                var departmentPath = GetDepartmentPath(input.DepartmentId, context);
                var fullPath = Path.Combine(context.StorageBasePath, departmentPath, storedFileName);

                // Criar diretório se não existir
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                // Copiar arquivo (simulação - em implementação real seria feito pelo controller)
                result.StoredFilePath = fullPath;

                // Criar documento
                var document = new Document
                {
                    OriginalFileName = input.OriginalFileName,
                    StoredFileName = storedFileName,
                    ContentType = input.ContentType,
                    FileSize = input.FileSize,
                    Description = input.Description,
                    DepartmentId = input.DepartmentId,
                    FolderId = input.FolderId,
                    Status = Models.DocumentStatus.Published,
                    Version = 1,
                    UploadDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    UploaderId = context.UserId
                };

                result.ProcessedDocument = document;
                result.IsSuccess = true;

                // Adicionar metadata
                result.Metadata["ProcessedAt"] = DateTime.UtcNow;
                result.Metadata["ProcessedBy"] = context.UserId;
                result.Metadata["FileExtension"] = Path.GetExtension(input.OriginalFileName);
                result.Metadata["DepartmentPath"] = departmentPath;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Erro no processamento: {ex.Message}";
            }

            return result;
        }

        private bool ValidateInput(DocumentCreateDTO input, DocumentProcessingContext context, out string? error)
        {
            error = null;

            if (string.IsNullOrEmpty(input.OriginalFileName))
            {
                error = "Nome do arquivo é obrigatório";
                return false;
            }

            if (input.FileSize > context.MaxFileSizeInBytes)
            {
                error = $"Arquivo excede o tamanho máximo de {context.MaxFileSizeInBytes / 1024 / 1024}MB";
                return false;
            }

            var extension = Path.GetExtension(input.OriginalFileName).ToLowerInvariant();
            if (context.AllowedExtensions.Length > 0 && !context.AllowedExtensions.Contains(extension))
            {
                error = $"Extensão {extension} não permitida";
                return false;
            }

            return true;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var guid = Guid.NewGuid().ToString("N");
            return $"{guid}{extension}";
        }

        private string GetDepartmentPath(int departmentId, DocumentProcessingContext context)
        {
            // Em implementação real, buscar nome do departamento no banco
            return $"Department_{departmentId}";
        }
    }

    /// <summary>
    /// Estratégia para download de documentos
    /// </summary>
    public class DocumentDownloadStrategy : IContextStrategy<int, DocumentProcessingResult, DocumentProcessingContext>
    {
        /// <summary>
        /// Executa o download do documento
        /// </summary>
        /// <param name="documentId">ID do documento</param>
        /// <param name="context">Contexto do processamento</param>
        /// <returns>Resultado do processamento</returns>
        public DocumentProcessingResult Execute(int documentId, DocumentProcessingContext context)
        {
            var result = new DocumentProcessingResult();

            try
            {
                // Em implementação real, buscar documento no banco
                // Validar permissões do usuário
                // Verificar se arquivo existe fisicamente
                // Registrar log de download

                result.IsSuccess = true;
                result.Metadata["DownloadedAt"] = DateTime.UtcNow;
                result.Metadata["DownloadedBy"] = context.UserId;
                result.Metadata["DocumentId"] = documentId;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Erro no download: {ex.Message}";
            }

            return result;
        }
    }

    /// <summary>
    /// Estratégia para validação de documentos
    /// </summary>
    public class DocumentValidationStrategy : IValidatedStrategy<DocumentCreateDTO, DocumentProcessingResult>
    {
        private readonly DocumentProcessingContext _context;

        public DocumentValidationStrategy(DocumentProcessingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Executa a validação do documento
        /// </summary>
        /// <param name="input">DTO do documento</param>
        /// <returns>Resultado da validação</returns>
        public DocumentProcessingResult Execute(DocumentCreateDTO input)
        {
            var result = new DocumentProcessingResult();

            if (!IsValid(input))
            {
                result.ErrorMessage = string.Join("; ", GetValidationErrors(input));
                return result;
            }

            result.IsSuccess = true;
            result.Metadata["ValidationPassed"] = true;
            result.Metadata["ValidatedAt"] = DateTime.UtcNow;

            return result;
        }

        /// <summary>
        /// Valida a entrada
        /// </summary>
        /// <param name="input">DTO do documento</param>
        /// <returns>True se válido</returns>
        public bool IsValid(DocumentCreateDTO input)
        {
            return !GetValidationErrors(input).Any();
        }

        /// <summary>
        /// Obtém erros de validação
        /// </summary>
        /// <param name="input">DTO do documento</param>
        /// <returns>Lista de erros</returns>
        public IEnumerable<string> GetValidationErrors(DocumentCreateDTO input)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(input.OriginalFileName))
                errors.Add("Nome do arquivo é obrigatório");

            if (input.FileSize <= 0)
                errors.Add("Tamanho do arquivo inválido");

            if (input.FileSize > _context.MaxFileSizeInBytes)
                errors.Add($"Arquivo excede o tamanho máximo de {_context.MaxFileSizeInBytes / 1024 / 1024}MB");

            if (string.IsNullOrEmpty(input.ContentType))
                errors.Add("Tipo de conteúdo é obrigatório");

            if (input.DepartmentId <= 0)
                errors.Add("Departamento é obrigatório");

            var extension = Path.GetExtension(input.OriginalFileName)?.ToLowerInvariant();
            if (_context.AllowedExtensions.Length > 0 && !_context.AllowedExtensions.Contains(extension))
                errors.Add($"Extensão {extension} não permitida");

            return errors;
        }
    }

    /// <summary>
    /// Estratégia em pipeline para processamento completo de documentos
    /// </summary>
    public class DocumentPipelineStrategy : IPipelineStrategy<DocumentCreateDTO, DocumentProcessingResult, DocumentProcessingResult>
    {
        private readonly DocumentValidationStrategy _validationStrategy;
        private readonly DocumentUploadStrategy _uploadStrategy;
        private readonly DocumentProcessingContext _context;

        public DocumentPipelineStrategy(DocumentProcessingContext context)
        {
            _context = context;
            _validationStrategy = new DocumentValidationStrategy(context);
            _uploadStrategy = new DocumentUploadStrategy();
        }

        /// <summary>
        /// Primeira etapa: validação
        /// </summary>
        /// <param name="input">DTO do documento</param>
        /// <returns>Resultado da validação</returns>
        public DocumentProcessingResult ProcessFirst(DocumentCreateDTO input)
        {
            return _validationStrategy.Execute(input);
        }

        /// <summary>
        /// Etapa final: upload
        /// </summary>
        /// <param name="intermediate">Resultado da validação</param>
        /// <returns>Resultado final</returns>
        public DocumentProcessingResult ProcessFinal(DocumentProcessingResult intermediate)
        {
            if (!intermediate.IsSuccess)
                return intermediate;

            // Em implementação real, o DTO seria passado novamente
            return intermediate;
        }

        /// <summary>
        /// Executa todo o pipeline
        /// </summary>
        /// <param name="input">DTO do documento</param>
        /// <returns>Resultado final</returns>
        public DocumentProcessingResult Execute(DocumentCreateDTO input)
        {
            var validationResult = ProcessFirst(input);
            if (!validationResult.IsSuccess)
                return validationResult;

            var uploadResult = _uploadStrategy.Execute(input, _context);
            return ProcessFinal(uploadResult);
        }
    }
}
