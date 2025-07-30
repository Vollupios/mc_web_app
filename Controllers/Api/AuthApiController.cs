using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Controllers.Api;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Controllers.Api
{
    /// <summary>
    /// DTO para login via API
    /// </summary>
    public class LoginApiDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// DTO para resposta de autenticação
    /// </summary>
    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDTO User { get; set; } = new();
    }

    /// <summary>
    /// DTO para informações do usuário
    /// </summary>
    public class UserInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO para alteração de senha
    /// </summary>
    public class ChangePasswordApiDTO
    {
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter entre 6 e 100 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da nova senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "A confirmação da senha não confere")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// API REST para autenticação e autorização
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthApiController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Realiza login e retorna token JWT
        /// </summary>
        /// <param name="loginDto">Credenciais de login</param>
        /// <returns>Token de acesso e informações do usuário</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> Login([FromBody] LoginApiDTO loginDto)
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

                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null || !user.IsActive)
                {
                    return CreateErrorResponse("Email ou senha inválidos", 401);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return CreateErrorResponse("Conta bloqueada temporariamente devido a muitas tentativas de login incorretas", 401);
                    }

                    return CreateErrorResponse("Email ou senha inválidos", 401);
                }

                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Aqui você poderia salvar o refresh token no banco de dados
                // para implementar renovação de tokens

                var userInfo = new UserInfoDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                };

                var response = new AuthResponseDTO
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
                    User = userInfo
                };

                _logger.LogInformation("Login realizado com sucesso para usuário {Email}", user.Email);

                return CreateSuccessResponse(response, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o login para {Email}", loginDto.Email);
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Realiza logout (invalida o token do lado do cliente)
        /// </summary>
        /// <returns>Confirmação de logout</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();

                // Aqui você poderia invalidar o refresh token no banco de dados
                // para maior segurança

                return CreateSuccessResponse<object>(null, "Logout realizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o logout");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Verifica se o token ainda é válido e retorna informações do usuário
        /// </summary>
        /// <returns>Informações do usuário autenticado</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || !user.IsActive)
                {
                    return CreateErrorResponse("Usuário não encontrado ou inativo", 401);
                }

                var userInfo = new UserInfoDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                };

                return CreateSuccessResponse(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário atual");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Altera a senha do usuário atual
        /// </summary>
        /// <param name="changePasswordDto">Dados para alteração de senha</param>
        /// <returns>Confirmação da alteração</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApiDTO changePasswordDto)
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
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 401);
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return CreateErrorResponse($"Erro ao alterar senha: {string.Join(", ", errors)}");
                }

                _logger.LogInformation("Senha alterada com sucesso para usuário {Email}", user.Email);

                return CreateSuccessResponse<object>(null, "Senha alterada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        /// <summary>
        /// Atualiza o perfil do usuário atual
        /// </summary>
        /// <param name="updateDto">Dados para atualização</param>
        /// <returns>Perfil atualizado</returns>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileApiDTO updateDto)
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
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return CreateErrorResponse("Usuário não encontrado", 401);
                }

                // Verificar se o email já está em uso por outro usuário
                if (user.Email != updateDto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                    if (existingUser != null && existingUser.Id != userId)
                    {
                        return CreateErrorResponse("Email já está em uso");
                    }
                }

                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return CreateErrorResponse($"Erro ao atualizar perfil: {string.Join(", ", errors)}");
                }

                var userInfo = new UserInfoDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = user.Department?.Name ?? "N/A",
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                };

                _logger.LogInformation("Perfil atualizado com sucesso para usuário {Email}", user.Email);

                return CreateSuccessResponse(userInfo, "Perfil atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar perfil");
                return CreateErrorResponse("Erro interno do servidor", 500);
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "your-super-secret-key-that-is-at-least-32-characters-long");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("DepartmentId", user.DepartmentId.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"] ?? "IntranetDocumentos",
                Audience = jwtSettings["Audience"] ?? "IntranetDocumentos"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// DTO para atualização de perfil
    /// </summary>
    public class UpdateProfileApiDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string? LastName { get; set; }
    }
}
