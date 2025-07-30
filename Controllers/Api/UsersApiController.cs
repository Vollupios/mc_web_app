using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Controllers.Api;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Controllers.Api
{
    /// <summary>
    /// DTO para criação de usuário via API
    /// </summary>
    public class CreateUserApiDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO para atualização de usuário via API
    /// </summary>
    public class UpdateUserApiDTO
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de usuário via API
    /// </summary>
    public class UserApiDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int DocumentCount { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// API REST para gerenciamento de usuários
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersApiController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersApiController> _logger;

        public UsersApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os usuários com paginação e filtros
        /// </summary>
        /// <param name="page">Página atual (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 10, máximo: 100)</param>
        /// <param name="departmentId">Filtrar por departamento</param>
        /// <param name="search">Buscar por nome ou email</param>
        /// <param name="isActive">Filtrar por status ativo</param>
        /// <returns>Lista paginada de usuários</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Gestor")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                // Validação de parâmetros
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.Users
                    .Include(u => u.Department)
                    .AsQueryable();

                // Aplicar filtros
                if (departmentId.HasValue)
                {
                    query = query.Where(u => u.DepartmentId == departmentId.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Email!.Contains(search) ||
                                           (u.FirstName != null && u.FirstName.Contains(search)) ||
                                           (u.LastName != null && u.LastName.Contains(search)));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                // Ordenar por data de criação (mais recente primeiro)
                query = query.OrderByDescending(u => u.CreatedAt);

                var totalCount = await query.CountAsync();
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserApiDTO>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var documentCount = await _context.Documents.CountAsync(d => d.UploaderId == user.Id);

                    userDtos.Add(new UserApiDTO
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName,
                        DepartmentId = user.DepartmentId,
                        DepartmentName = user.Department?.Name ?? "N/A",
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        Roles = roles.ToList(),
                        DocumentCount = documentCount
                    });
                }

                return CreatePaginatedResponse(userDtos, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar usuários");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém um usuário específico por ID
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Dados do usuário</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Gestor")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var documentCount = await _context.Documents.CountAsync(d => d.UploaderId == user.Id);

                var userDto = new UserApiDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles.ToList(),
                    DocumentCount = documentCount
                };

                return CreateSuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário {UserId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        /// <param name="createDto">Dados do usuário</param>
        /// <returns>Usuário criado</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserApiDTO createDto)
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

                // Verificar se o departamento existe
                var department = await _context.Departments.FindAsync(createDto.DepartmentId);
                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado");
                }

                // Verificar se o email já existe
                var existingUser = await _userManager.FindByEmailAsync(createDto.Email);
                if (existingUser != null)
                {
                    return CreateErrorResponse("Email já está em uso");
                }

                var user = new ApplicationUser
                {
                    Email = createDto.Email,
                    UserName = createDto.Email,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    DepartmentId = createDto.DepartmentId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, createDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return CreateErrorResponse($"Erro ao criar usuário: {string.Join(", ", errors)}");
                }

                // Adicionar roles
                if (createDto.Roles.Any())
                {
                    foreach (var role in createDto.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                else
                {
                    // Role padrão
                    await _userManager.AddToRoleAsync(user, "Usuario");
                }

                // Recarregar usuário com departamento
                user = await _context.Users
                    .Include(u => u.Department)
                    .FirstAsync(u => u.Id == user.Id);

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserApiDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles.ToList(),
                    DocumentCount = 0
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    success = true,
                    message = "Usuário criado com sucesso",
                    data = userDto,
                    timestamp = DateTime.UtcNow
                }) as IActionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <param name="updateDto">Dados para atualização</param>
        /// <returns>Usuário atualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserApiDTO updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return CreateErrorResponse("ID do usuário não confere");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return CreateErrorResponse($"Dados inválidos: {string.Join(", ", errors)}");
                }

                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                // Verificar se o departamento existe
                var department = await _context.Departments.FindAsync(updateDto.DepartmentId);
                if (department == null)
                {
                    return CreateErrorResponse("Departamento não encontrado");
                }

                // Verificar se o email já existe (exceto para o usuário atual)
                var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return CreateErrorResponse("Email já está em uso");
                }

                // Atualizar dados do usuário
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.DepartmentId = updateDto.DepartmentId;
                user.IsActive = updateDto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return CreateErrorResponse($"Erro ao atualizar usuário: {string.Join(", ", errors)}");
                }

                // Atualizar roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (updateDto.Roles.Any())
                {
                    foreach (var role in updateDto.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
                else
                {
                    // Role padrão
                    await _userManager.AddToRoleAsync(user, "Usuario");
                }

                // Recarregar usuário
                user = await _context.Users
                    .Include(u => u.Department)
                    .FirstAsync(u => u.Id == id);

                var roles = await _userManager.GetRolesAsync(user);
                var documentCount = await _context.Documents.CountAsync(d => d.UploaderId == user.Id);

                var userDto = new UserApiDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles.ToList(),
                    DocumentCount = documentCount
                };

                return CreateSuccessResponse(userDto, "Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Remove um usuário
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Confirmação da remoção</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Não permitir que o usuário delete a si mesmo
                if (id == currentUserId)
                {
                    return CreateErrorResponse("Você não pode excluir sua própria conta");
                }

                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                // Verificar se o usuário tem documentos
                var hasDocuments = await _context.Documents.AnyAsync(d => d.UploaderId == id);
                if (hasDocuments)
                {
                    return CreateErrorResponse("Não é possível excluir usuário que possui documentos associados");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return CreateErrorResponse($"Erro ao excluir usuário: {string.Join(", ", errors)}");
                }

                return CreateSuccessResponse(new { userId = id }, "Usuário excluído com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {UserId}", id);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém o perfil do usuário atual
        /// </summary>
        /// <returns>Dados do usuário atual</returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();

                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 404);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var documentCount = await _context.Documents.CountAsync(d => d.UploaderId == user.Id);

                var userDto = new UserApiDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles.ToList(),
                    DocumentCount = documentCount
                };

                return CreateSuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter perfil do usuário");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Obtém estatísticas de usuários
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
                    totalUsers = await _context.Users.CountAsync(),
                    activeUsers = await _context.Users.CountAsync(u => u.IsActive),
                    inactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
                    usersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt.Month == DateTime.Now.Month && 
                                                                        u.CreatedAt.Year == DateTime.Now.Year),
                    usersByDepartment = await _context.Users
                                                     .Include(u => u.Department)
                                                     .GroupBy(u => u.Department!.Name)
                                                     .Select(g => new { department = g.Key, count = g.Count() })
                                                     .ToListAsync(),
                    usersByRole = await (from ur in _context.UserRoles
                                        join r in _context.Roles on ur.RoleId equals r.Id
                                        group ur by r.Name into g
                                        select new { role = g.Key, count = g.Count() })
                                        .ToListAsync()
                };

                return CreateSuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de usuários");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }
    }
}
