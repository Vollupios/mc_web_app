using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Controllers.Api;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Controllers.Api
{
    /// <summary>
    /// DTO para criação de departamento via API
    /// </summary>
    public class CreateDepartmentApiDTO
    {
        [Required(ErrorMessage = "O nome do departamento é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualização de departamento via API
    /// </summary>
    public class UpdateDepartmentApiDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do departamento é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de departamento via API
    /// </summary>
    public class DepartmentApiDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int DocumentCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// API REST para gerenciamento de departamentos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsApiController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentsApiController> _logger;

        public DepartmentsApiController(
            ApplicationDbContext context,
            ILogger<DepartmentsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os departamentos
        /// </summary>
        /// <param name="includeStats">Incluir estatísticas de usuários e documentos</param>
        /// <returns>Lista de departamentos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetDepartments([FromQuery] bool includeStats = false)
        {
            try
            {
                var query = _context.Departments.AsQueryable();

                var departments = await query.OrderBy(d => d.Name).ToListAsync();

                var departmentDtos = new List<DepartmentApiDTO>();

                foreach (var department in departments)
                {
                    var dto = new DepartmentApiDTO
                    {
                        Id = department.Id,
                        Name = department.Name
                    };

                    if (includeStats)
                    {
                        dto.UserCount = await _context.Users.CountAsync(u => u.DepartmentId == department.Id);
                        dto.DocumentCount = await _context.Documents.CountAsync(d => d.DepartmentId == department.Id);
                    }

                    departmentDtos.Add(dto);
                }

                return CreateSuccessResponse(departmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar departamentos");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém um departamento específico por ID
        /// </summary>
        /// <param name="id">ID do departamento</param>
        /// <returns>Dados do departamento</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado", 404);
                }

                var userCount = await _context.Users.CountAsync(u => u.DepartmentId == id);
                var documentCount = await _context.Documents.CountAsync(d => d.DepartmentId == id);

                var departmentDto = new DepartmentApiDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    UserCount = userCount,
                    DocumentCount = documentCount
                };

                return CreateSuccessResponse(departmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter departamento {DepartmentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Cria um novo departamento
        /// </summary>
        /// <param name="createDto">Dados do departamento</param>
        /// <returns>Departamento criado</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentApiDTO createDto)
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

                // Verificar se já existe um departamento com o mesmo nome
                var existingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == createDto.Name.ToLower());

                if (existingDepartment != null)
                {
                    return CreateErrorResponse("Já existe um departamento com este nome");
                }

                var department = new Department
                {
                    Name = createDto.Name
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentApiDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    UserCount = 0,
                    DocumentCount = 0
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new
                {
                    success = true,
                    message = "Departamento criado com sucesso",
                    data = departmentDto,
                    timestamp = DateTime.UtcNow
                }) as IActionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar departamento");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Atualiza um departamento existente
        /// </summary>
        /// <param name="id">ID do departamento</param>
        /// <param name="updateDto">Dados para atualização</param>
        /// <returns>Departamento atualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentApiDTO updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return CreateErrorResponse("ID do departamento não confere");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return CreateErrorResponse($"Dados inválidos: {string.Join(", ", errors)}");
                }

                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado", 404);
                }

                // Verificar se já existe outro departamento com o mesmo nome
                var existingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == updateDto.Name.ToLower() && d.Id != id);

                if (existingDepartment != null)
                {
                    return CreateErrorResponse("Já existe um departamento com este nome");
                }

                department.Name = updateDto.Name;

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                var userCount = await _context.Users.CountAsync(u => u.DepartmentId == id);
                var documentCount = await _context.Documents.CountAsync(d => d.DepartmentId == id);

                var departmentDto = new DepartmentApiDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    UserCount = userCount,
                    DocumentCount = documentCount
                };

                return CreateSuccessResponse(departmentDto, "Departamento atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar departamento {DepartmentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Remove um departamento
        /// </summary>
        /// <param name="id">ID do departamento</param>
        /// <returns>Confirmação da remoção</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado", 404);
                }

                // Verificar se há usuários associados
                var hasUsers = await _context.Users.AnyAsync(u => u.DepartmentId == id);
                if (hasUsers)
                {
                    return CreateErrorResponse("Não é possível excluir departamento que possui usuários associados");
                }

                // Verificar se há documentos associados
                var hasDocuments = await _context.Documents.AnyAsync(d => d.DepartmentId == id);
                if (hasDocuments)
                {
                    return CreateErrorResponse("Não é possível excluir departamento que possui documentos associados");
                }

                // Verificar se é um departamento padrão
                if (department.Name.ToLower() == "geral" || department.Name.ToLower() == "ti")
                {
                    return CreateErrorResponse("Não é possível excluir departamentos padrão do sistema");
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return CreateSuccessResponse(new { departmentId = id }, "Departamento excluído com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir departamento {DepartmentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém usuários de um departamento
        /// </summary>
        /// <param name="id">ID do departamento</param>
        /// <param name="page">Página atual</param>
        /// <param name="pageSize">Itens por página</param>
        /// <returns>Lista paginada de usuários</returns>
        [HttpGet("{id}/users")]
        [Authorize(Roles = "Admin,Gestor")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetDepartmentUsers(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado", 404);
                }

                // Validação de parâmetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.Users
                    .Where(u => u.DepartmentId == id)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName);

                var totalCount = await query.CountAsync();
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        FullName = (u.FirstName + " " + u.LastName).Trim(),
                        u.IsActive,
                        u.CreatedAt,
                        u.UpdatedAt
                    })
                    .ToListAsync();

                return CreatePaginatedResponse(users, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuários do departamento {DepartmentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém documentos de um departamento
        /// </summary>
        /// <param name="id">ID do departamento</param>
        /// <param name="page">Página atual</param>
        /// <param name="pageSize">Itens por página</param>
        /// <returns>Lista paginada de documentos</returns>
        [HttpGet("{id}/documents")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetDepartmentDocuments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado", 404);
                }

                var userId = GetCurrentUserId();
                var userRoles = GetCurrentUserRoles().ToList();

                // Verificar permissões
                if (!userRoles.Contains("Admin") && !userRoles.Contains("Gestor"))
                {
                    var userDepartmentId = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.DepartmentId)
                        .FirstOrDefaultAsync();

                    if (id != userDepartmentId && department.Name != "Geral")
                    {
                        return CreateErrorResponse("Acesso negado", 403);
                    }
                }

                // Validação de parâmetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.Documents
                    .Include(d => d.Uploader)
                    .Where(d => d.DepartmentId == id)
                    .OrderByDescending(d => d.UploadDate);

                var totalCount = await query.CountAsync();
                var documents = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new
                    {
                        d.Id,
                        d.OriginalFileName,
                        d.FileSize,
                        FileSizeFormatted = d.FileSize < 1024 ? $"{d.FileSize} B" :
                                          d.FileSize < 1048576 ? $"{d.FileSize / 1024:F2} KB" :
                                          d.FileSize < 1073741824 ? $"{d.FileSize / 1048576:F2} MB" :
                                          $"{d.FileSize / 1073741824:F2} GB",
                        d.ContentType,
                        d.Description,
                        d.UploadDate,
                        UploaderName = d.Uploader != null ? d.Uploader.FirstName + " " + d.Uploader.LastName : "N/A",
                        Status = d.Status.ToString()
                    })
                    .ToListAsync();

                return CreatePaginatedResponse(documents, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter documentos do departamento {DepartmentId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém estatísticas de departamentos
        /// </summary>
        /// <returns>Estatísticas gerais</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Gestor")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = new
                {
                    totalDepartments = await _context.Departments.CountAsync(),
                    departmentsSummary = await _context.Departments
                        .Select(d => new
                        {
                            d.Id,
                            d.Name,
                            userCount = _context.Users.Count(u => u.DepartmentId == d.Id),
                            documentCount = _context.Documents.Count(doc => doc.DepartmentId == d.Id),
                            activeUserCount = _context.Users.Count(u => u.DepartmentId == d.Id && u.IsActive)
                        })
                        .OrderBy(d => d.Name)
                        .ToListAsync()
                };

                return CreateSuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de departamentos");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }
    }
}
