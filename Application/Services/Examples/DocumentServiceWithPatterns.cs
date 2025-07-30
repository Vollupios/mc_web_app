using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.DTOs.Common;
using IntranetDocumentos.Application.Strategies;
using IntranetDocumentos.Application.Strategies.Documents;
using IntranetDocumentos.Application.Strategies.Search;
using IntranetDocumentos.Application.Factories.Documents;
using IntranetDocumentos.Domain.ValueObjects;
using IntranetDocumentos.Models;
using System.IO;

namespace IntranetDocumentos.Application.Services.Examples
{
    /// <summary>
    /// Exemplo de integração dos padrões Factory, Strategy e Value Objects
    /// </summary>
    public class DocumentServiceWithPatterns
    {
        private readonly DocumentFactory _documentFactory;
        private readonly DocumentPipelineStrategy _pipelineStrategy;
        private readonly AdvancedSearchStrategy _searchStrategy;
        private readonly DocumentProcessingContext _context;

        public DocumentServiceWithPatterns(
            DocumentFactory documentFactory,
            string currentUserId,
            string userRole,
            int userDepartmentId)
        {
            _documentFactory = documentFactory;
            _pipelineStrategy = new DocumentPipelineStrategy(CreateContext(currentUserId, userRole));
            _searchStrategy = new AdvancedSearchStrategy();
            _context = CreateContext(currentUserId, userRole);
        }

        /// <summary>
        /// Exemplo de upload usando Factory Pattern e Strategy Pattern
        /// </summary>
        public Document? UploadDocumentAsync(CreateDocumentDTO createDto)
        {
            try
            {
                // Usar Value Objects para validação
                var fileName = new FileName(createDto.File.FileName);
                var fileSize = new FileSize(createDto.File.Length);
                var contentType = ContentType.FromExtension(Path.GetExtension(createDto.File.FileName));
                var description = new DocumentDescription(createDto.Description);

                // Verificar se todos os Value Objects são válidos
                var valueObjects = new IValidatable[] { fileName, fileSize, contentType, description };
                var validationErrors = valueObjects
                    .Where(vo => !vo.IsValid())
                    .SelectMany(vo => vo.GetValidationErrors())
                    .ToList();

                if (validationErrors.Any())
                {
                    throw new ArgumentException($"Dados inválidos: {string.Join(", ", validationErrors)}");
                }

                // Simular conversão para DTO usado pela Strategy
                var strategyDto = new DocumentCreateDTO
                {
                    OriginalFileName = createDto.File.FileName,
                    FileSize = createDto.File.Length,
                    ContentType = createDto.File.ContentType,
                    Description = createDto.Description,
                    DepartmentId = createDto.DepartmentId,
                    FolderId = createDto.FolderId,
                    Status = Models.DocumentStatus.Published,
                    Version = "1.0"
                };

                // Usar Strategy Pattern para processamento
                var result = _pipelineStrategy.Execute(strategyDto);
                
                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.ErrorMessage);
                }

                // Usar Factory Pattern para criação
                var document = _documentFactory.Create(strategyDto);
                
                // Aplicar Value Objects na entidade
                ApplyValueObjectsToDocument(document, fileName, fileSize, contentType, description);

                return document;
            }
            catch (Exception ex)
            {
                // Log do erro
                throw new ApplicationException($"Erro no upload: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exemplo de busca usando Strategy Pattern
        /// </summary>
        public SearchResult<DocumentResponseDTO> SearchDocumentsAsync(SearchFilterDTO filter)
        {
            try
            {
                var searchContext = new SearchContext
                {
                    UserId = _context.UserId,
                    UserRole = _context.UserRole,
                    UserDepartmentId = _context.UserDepartmentId,
                    PageSize = filter.PageSize,
                    MaxResults = 1000
                };

                return _searchStrategy.Execute(filter, searchContext);
            }
            catch (Exception ex)
            {
                // Log do erro
                return new SearchResult<DocumentResponseDTO>
                {
                    Results = Enumerable.Empty<DocumentResponseDTO>(),
                    TotalCount = 0,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Metadata = new Dictionary<string, object> { ["Error"] = ex.Message }
                };
            }
        }

        /// <summary>
        /// Exemplo de validação usando Value Objects
        /// </summary>
        public ValidationResult ValidateDocument(DocumentCreateDTO createDto)
        {
            var errors = new List<string>();

            try
            {
                // Validar nome do arquivo
                var fileName = new FileName(createDto.OriginalFileName);
                if (!fileName.IsValid())
                    errors.AddRange(fileName.GetValidationErrors());

                // Validar tamanho do arquivo
                var fileSize = new FileSize(createDto.FileSize);
                if (!fileSize.IsValid())
                    errors.AddRange(fileSize.GetValidationErrors());

                // Validar tipo de conteúdo
                var contentType = ContentType.FromExtension(Path.GetExtension(createDto.OriginalFileName));
                if (!contentType.IsValid())
                    errors.AddRange(contentType.GetValidationErrors());

                // Validar descrição
                var description = new DocumentDescription(createDto.Description);
                if (!description.IsValid())
                    errors.AddRange(description.GetValidationErrors());

                // Validar versão
                var version = new DocumentVersion(createDto.Version);
                if (!version.IsValid())
                    errors.AddRange(version.GetValidationErrors());

                return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Erro na validação: {ex.Message}");
            }
        }

        /// <summary>
        /// Exemplo de processamento com múltiplas strategies
        /// </summary>
        public ProcessingResult ProcessDocumentAsync(DocumentCreateDTO createDto)
        {
            var processingResult = new ProcessingResult();

            try
            {
                // Etapa 1: Validação usando Value Objects
                var validationResult = ValidateDocument(createDto);
                if (!validationResult.IsValid)
                {
                    processingResult.AddErrors(validationResult.Errors);
                    return processingResult;
                }

                // Etapa 2: Processamento usando Strategy
                var uploadStrategy = new DocumentUploadStrategy();
                var uploadResult = uploadStrategy.Execute(createDto, _context);
                
                if (!uploadResult.IsSuccess)
                {
                    processingResult.AddError(uploadResult.ErrorMessage);
                    return processingResult;
                }

                // Etapa 3: Criação usando Factory
                var document = _documentFactory.Create(createDto);
                
                // Etapa 4: Aplicar Value Objects
                ApplyValueObjectsToDocument(document, 
                    new FileName(createDto.OriginalFileName),
                    new FileSize(createDto.FileSize),
                    ContentType.FromExtension(Path.GetExtension(createDto.OriginalFileName)),
                    new DocumentDescription(createDto.Description));

                processingResult.Document = document;
                processingResult.IsSuccess = true;
                processingResult.Metadata = uploadResult.Metadata;

                return processingResult;
            }
            catch (Exception ex)
            {
                processingResult.AddError($"Erro no processamento: {ex.Message}");
                return processingResult;
            }
        }

        /// <summary>
        /// Exemplo de uso de diferentes strategies baseadas em contexto
        /// </summary>
        public DocumentUploadStrategy GetUploadStrategy(string userRole)
        {
            // Retornar sempre a mesma strategy, mas com contexto diferente
            return new DocumentUploadStrategy();
        }

        /// <summary>
        /// Exemplo de uso de Strategy Pattern para diferentes tipos de busca
        /// </summary>
        public Task<SearchResult<DocumentResponseDTO>> SearchByTypeAsync(string searchType, object searchCriteria)
        {
            var result = searchType.ToLower() switch
            {
                "simple" => new SimpleTextSearchStrategy().Execute((string)searchCriteria, 
                    new SearchContext { UserId = _context.UserId, UserRole = _context.UserRole }),
                
                "advanced" => new AdvancedSearchStrategy().Execute((SearchFilterDTO)searchCriteria, 
                    new SearchContext { UserId = _context.UserId, UserRole = _context.UserRole }),
                
                "cached" => new CachedSearchStrategy(TimeSpan.FromMinutes(5)).Execute((SearchFilterDTO)searchCriteria, 
                    new SearchContext { UserId = _context.UserId, UserRole = _context.UserRole }),
                
                _ => throw new ArgumentException($"Tipo de busca '{searchType}' não suportado")
            };
            
            return Task.FromResult(result);
        }

        private DocumentProcessingContext CreateContext(string userId, string userRole)
        {
            return new DocumentProcessingContext
            {
                UserId = userId,
                UserRole = userRole,
                StorageBasePath = "DocumentsStorage",
                MaxFileSizeInBytes = 10 * 1024 * 1024, // 10MB
                AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".png" },
                RequireVirusCheck = false
            };
        }

        private void ApplyValueObjectsToDocument(Document document, FileName fileName, FileSize fileSize, 
            ContentType contentType, DocumentDescription description)
        {
            document.OriginalFileName = fileName.Value;
            document.ContentType = contentType.Value;
            document.FileSize = fileSize.Value;
            document.Description = description.Value;
        }
    }

    /// <summary>
    /// Resultado do processamento
    /// </summary>
    public class ProcessingResult
    {
        public bool IsSuccess { get; set; }
        public Document? Document { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();

        public void AddError(string? error)
        {
            if (!string.IsNullOrEmpty(error))
                Errors.Add(error);
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }
    }

    /// <summary>
    /// Exemplo de uso dos padrões no controller
    /// </summary>
    public class DocumentControllerExample
    {
        private readonly DocumentServiceWithPatterns _documentService;

        public DocumentControllerExample(DocumentServiceWithPatterns documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Exemplo de action que usa todos os padrões
        /// </summary>
        public ProcessingResult UploadDocument(DocumentCreateDTO createDto)
        {
            try
            {
                // Usar o serviço que integra todos os padrões
                var result = _documentService.ProcessDocumentAsync(createDto);

                return result;
            }
            catch (Exception ex)
            {
                var errorResult = new ProcessingResult();
                errorResult.AddError($"Erro interno do servidor: {ex.Message}");
                return errorResult;
            }
        }
    }
}
