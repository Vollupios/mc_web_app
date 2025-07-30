using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Controllers.Api;
using IntranetDocumentos.Services;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using System.ComponentModel.DataAnnotations;
using ModelDocumentStatus = IntranetDocumentos.Models.DocumentStatus;

namespace IntranetDocumentos.Controllers
{
    /// <summary>
    /// DTO para criação de documento via API
    /// </summary>
    public class CreateDocumentApiDTO
    {
        [Required(ErrorMessage = "O arquivo é obrigatório")]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public int? FolderId { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO para atualização de documento via API
    /// </summary>
    public class UpdateDocumentApiDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do arquivo é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome do arquivo deve ter no máximo 255 caracteres")]
        public string OriginalFileName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public int? FolderId { get; set; }
    }

    /// <summary>
    /// DTO para resposta de documento via API
    /// </summary>
    public class DocumentApiDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileSizeFormatted => FormatBytes(FileSize);
        public string? Description { get; set; }
        public string ContentType { get; set; } = string.Empty;
        
        // Relacionamentos
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? FolderId { get; set; }
        public string? FolderName { get; set; }
        public string UploaderId { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        
        // Metadados
        public DateTime UploadDate { get; set; }
        public DateTime? LastModified { get; set; }
        public int Version { get; set; }
        public string Status { get; set; } = string.Empty;
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// API REST para gerenciamento de documentos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsApiController : BaseApiController
    {
        private readonly IDocumentService _documentService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentsApiController> _logger;

        public DocumentsApiController(
            IDocumentService documentService,
            ApplicationDbContext context,
            ILogger<DocumentsApiController> logger)
        {
            _documentService = documentService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os documentos com paginação e filtros
        /// </summary>
        /// <param name="page">Página atual (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 10, máximo: 100)</param>
        /// <param name="departmentId">Filtrar por departamento</param>
        /// <param name="search">Buscar por nome ou descrição</param>
        /// <param name="status">Filtrar por status</param>
        /// <returns>Lista paginada de documentos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? search = null,
            [FromQuery] ModelDocumentStatus? status = null)
        {
            try
            {
                // Validação de parâmetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var query = _context.Documents
                    .Include(d => d.Department)
                    .Include(d => d.Uploader)
                    .Include(d => d.Folder)
                    .AsQueryable();

                // Aplicar filtros de permissão
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    var userDepartmentId = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.DepartmentId)
                        .FirstOrDefaultAsync();

                    query = query.Where(d => d.DepartmentId == userDepartmentId || 
                                           d.Department!.Name == "Geral");
                }

                // Aplicar filtros
                if (departmentId.HasValue)
                {
                    query = query.Where(d => d.DepartmentId == departmentId.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d => d.OriginalFileName.Contains(search) ||
                                           (d.Description != null && d.Description.Contains(search)));
                }

                if (status.HasValue)
                {
                    query = query.Where(d => d.Status == status.Value);
                }

                // Ordenar por data de upload (mais recente primeiro)
                query = query.OrderByDescending(d => d.UploadDate);

                var totalCount = await query.CountAsync();
                var documents = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new DocumentApiDTO
                    {
                        Id = d.Id,
                        CreatedAt = d.UploadDate,
                        UpdatedAt = d.LastModified,
                        OriginalFileName = d.OriginalFileName,
                        Title = d.OriginalFileName,
                        FileSize = d.FileSize,
                        Description = d.Description,
                        ContentType = d.ContentType,
                        DepartmentId = d.DepartmentId ?? 0,
                        DepartmentName = d.Department != null ? d.Department.Name : "N/A",
                        FolderId = d.FolderId,
                        FolderName = d.Folder != null ? d.Folder.Name : null,
                        UploaderId = d.UploaderId,
                        UploaderName = d.Uploader != null ? d.Uploader.UserName ?? "N/A" : "N/A",
                        UploadDate = d.UploadDate,
                        LastModified = d.LastModified,
                        Version = d.Version,
                        Status = d.Status.ToString()
                    })
                    .ToListAsync();

                return CreatePaginatedResponse(documents, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar documentos");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém um documento específico por ID
        /// </summary>
        /// <param name="id">ID do documento</param>
        /// <returns>Dados do documento</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> GetDocument(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var document = await _context.Documents
                    .Include(d => d.Department)
                    .Include(d => d.Uploader)
                    .Include(d => d.Folder)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    return CreateErrorResponse("Documento não encontrado", 404);
                }

                // Verificar permissões
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    var userDepartmentId = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.DepartmentId)
                        .FirstOrDefaultAsync();

                    if (document.DepartmentId != userDepartmentId && 
                        document.Department?.Name != "Geral")
                    {
                        return CreateErrorResponse("Acesso negado", 403);
                    }
                }

                var documentDto = new DocumentApiDTO
                {
                    Id = document.Id,
                    CreatedAt = document.UploadDate,
                    UpdatedAt = document.LastModified,
                    OriginalFileName = document.OriginalFileName,
                    Title = document.OriginalFileName,
                    FileSize = document.FileSize,
                    Description = document.Description,
                    ContentType = document.ContentType,
                    DepartmentId = document.DepartmentId ?? 0,
                    DepartmentName = document.Department?.Name ?? "N/A",
                    FolderId = document.FolderId,
                    FolderName = document.Folder?.Name,
                    UploaderId = document.UploaderId,
                    UploaderName = document.Uploader?.UserName ?? "N/A",
                    UploadDate = document.UploadDate,
                    LastModified = document.LastModified,
                    Version = document.Version,
                    Status = document.Status.ToString()
                };

                return CreateSuccessResponse(documentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter documento {DocumentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Faz upload de um novo documento
        /// </summary>
        /// <param name="createDto">Dados do documento</param>
        /// <returns>Documento criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentApiDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return CreateErrorResponse($"Dados inválidos: {string.Join(", ", errors)}");
                }

                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                // Obter usuário completo
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                // Verificar permissões de departamento
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    var targetDepartment = await _context.Departments
                        .FirstOrDefaultAsync(d => d.Id == createDto.DepartmentId);

                    if (targetDepartment?.Name != "Geral" && createDto.DepartmentId != user.DepartmentId)
                    {
                        return CreateErrorResponse("Você não tem permissão para enviar documentos para este departamento", 403);
                    }
                }

                var document = await _documentService.SaveDocumentAsync(
                    createDto.File,
                    user,
                    createDto.DepartmentId);

                if (document != null)
                {
                    // Atualizar descrição se fornecida
                    if (!string.IsNullOrEmpty(createDto.Description))
                    {
                        document.Description = createDto.Description;
                        _context.Documents.Update(document);
                        await _context.SaveChangesAsync();
                    }

                    var documentDto = new DocumentApiDTO
                    {
                        Id = document.Id,
                        CreatedAt = document.UploadDate,
                        UpdatedAt = document.LastModified,
                        OriginalFileName = document.OriginalFileName,
                        Title = document.OriginalFileName,
                        FileSize = document.FileSize,
                        Description = document.Description,
                        ContentType = document.ContentType,
                        DepartmentId = document.DepartmentId ?? 0,
                        UploaderId = document.UploaderId,
                        UploadDate = document.UploadDate,
                        Version = document.Version,
                        Status = document.Status.ToString()
                    };

                    return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, new
                    {
                        success = true,
                        message = "Documento enviado com sucesso",
                        data = documentDto,
                        timestamp = DateTime.UtcNow
                    }) as IActionResult;
                }

                return CreateErrorResponse("Erro ao salvar documento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar documento");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Atualiza os metadados de um documento
        /// </summary>
        /// <param name="id">ID do documento</param>
        /// <param name="updateDto">Dados para atualização</param>
        /// <returns>Documento atualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentApiDTO updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return CreateErrorResponse("ID do documento não confere");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return CreateErrorResponse($"Dados inválidos: {string.Join(", ", errors)}");
                }

                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    return CreateErrorResponse("Documento não encontrado", 404);
                }

                // Verificar permissões
                if (!userRoles.Contains("Admin"))
                {
                    if (document.UploaderId != userId && !userRoles.Contains("Gestor"))
                    {
                        return CreateErrorResponse("Você não tem permissão para editar este documento", 403);
                    }
                }

                var success = await _documentService.UpdateDocumentAsync(
                    id, updateDto.OriginalFileName, updateDto.Description, user);

                if (success)
                {
                    // Recarregar documento atualizado
                    document = await _context.Documents
                        .Include(d => d.Department)
                        .Include(d => d.Uploader)
                        .Include(d => d.Folder)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    var documentDto = new DocumentApiDTO
                    {
                        Id = document!.Id,
                        CreatedAt = document.UploadDate,
                        UpdatedAt = document.LastModified,
                        OriginalFileName = document.OriginalFileName,
                        Title = document.OriginalFileName,
                        FileSize = document.FileSize,
                        Description = document.Description,
                        ContentType = document.ContentType,
                        DepartmentId = document.DepartmentId ?? 0,
                        DepartmentName = document.Department?.Name ?? "N/A",
                        FolderId = document.FolderId,
                        FolderName = document.Folder?.Name,
                        UploaderId = document.UploaderId,
                        UploaderName = document.Uploader?.UserName ?? "N/A",
                        UploadDate = document.UploadDate,
                        LastModified = document.LastModified,
                        Version = document.Version,
                        Status = document.Status.ToString()
                    };

                    return CreateSuccessResponse(documentDto, "Documento atualizado com sucesso");
                }

                return CreateErrorResponse("Erro ao atualizar documento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar documento {DocumentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Remove um documento
        /// </summary>
        /// <param name="id">ID do documento</param>
        /// <returns>Confirmação da remoção</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                var document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    return CreateErrorResponse("Documento não encontrado", 404);
                }

                // Verificar permissões
                if (!userRoles.Contains("Admin"))
                {
                    if (document.UploaderId != userId && !userRoles.Contains("Gestor"))
                    {
                        return CreateErrorResponse("Você não tem permissão para excluir este documento", 403);
                    }
                }

                var success = await _documentService.DeleteDocumentAsync(id, user);

                if (success)
                {
                    return CreateSuccessResponse(new { documentId = id }, "Documento excluído com sucesso");
                }

                return CreateErrorResponse("Erro ao excluir documento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento {DocumentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Faz download de um documento
        /// </summary>
        /// <param name="id">ID do documento</param>
        /// <returns>Arquivo do documento</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                var document = await _context.Documents
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    return CreateErrorResponse("Documento não encontrado", 404);
                }

                // Verificar permissões
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    if (document.DepartmentId != user.DepartmentId && 
                        document.Department?.Name != "Geral")
                    {
                        return CreateErrorResponse("Acesso negado", 403);
                    }
                }

                var downloadData = await _documentService.GetDocumentForDownloadAsync(id, user);

                if (downloadData.HasValue)
                {
                    return File(downloadData.Value.FileData, downloadData.Value.ContentType, downloadData.Value.FileName) as IActionResult;
                }

                return CreateErrorResponse("Erro ao fazer download do documento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer download do documento {DocumentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém estatísticas de documentos
        /// </summary>
        /// <returns>Estatísticas gerais</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                var query = _context.Documents.AsQueryable();

                // Aplicar filtros de permissão
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    var userDepartmentId = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.DepartmentId)
                        .FirstOrDefaultAsync();

                    query = query.Where(d => d.DepartmentId == userDepartmentId || 
                                           d.Department!.Name == "Geral");
                }

                var stats = new
                {
                    totalDocuments = await query.CountAsync(),
                    documentsThisMonth = await query.Where(d => d.UploadDate.Month == DateTime.Now.Month && 
                                                              d.UploadDate.Year == DateTime.Now.Year).CountAsync(),
                    documentsByStatus = await query.GroupBy(d => d.Status)
                                                  .Select(g => new { status = g.Key.ToString(), count = g.Count() })
                                                  .ToListAsync(),
                    documentsByDepartment = await query.Include(d => d.Department)
                                                      .GroupBy(d => d.Department!.Name)
                                                      .Select(g => new { department = g.Key, count = g.Count() })
                                                      .ToListAsync(),
                    totalFileSize = await query.SumAsync(d => d.FileSize),
                    averageFileSize = await query.AverageAsync(d => (double)d.FileSize)
                };

                return CreateSuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de documentos");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }
    }
}