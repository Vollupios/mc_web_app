using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Controllers
{
    [Route("admin/reset")]
    public class AdminResetController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminResetController> _logger;
        private readonly IConfiguration _configuration;

        public AdminResetController(
            UserManager<ApplicationUser> userManager,
            ILogger<AdminResetController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("password")]
        public async Task<IActionResult> ResetAdminPassword()
        {
            try
            {
                var adminEmail = "admin@intranet.com";
                var newPassword = "Admin123!@#X"; // 12 caracteres, atende aos requisitos

                var user = await _userManager.FindByEmailAsync(adminEmail);
                if (user == null)
                {
                    return BadRequest($"Usuário {adminEmail} não encontrado!");
                }

                // Remove the current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    return BadRequest($"Erro ao remover senha atual: {string.Join(", ", removePasswordResult.Errors.Select(e => e.Description))}");
                }

                // Add the new password
                var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (!addPasswordResult.Succeeded)
                {
                    return BadRequest($"Erro ao definir nova senha: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation("Senha do admin resetada com sucesso");

                return Ok(new
                {
                    message = "Senha do admin resetada com sucesso!",
                    email = adminEmail,
                    newPassword = newPassword,
                    userId = user.Id,
                    emailConfirmed = user.EmailConfirmed,
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao resetar senha do admin");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckAdminUser()
        {
            try
            {
                var adminEmail = "admin@intranet.com";
                var testPassword = "Admin123!@#X"; // 12 caracteres

                var user = await _userManager.FindByEmailAsync(adminEmail);
                if (user == null)
                {
                    return BadRequest($"Usuário {adminEmail} não encontrado!");
                }

                var passwordCheck = await _userManager.CheckPasswordAsync(user, testPassword);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    userFound = true,
                    email = user.Email,
                    userName = user.UserName,
                    emailConfirmed = user.EmailConfirmed,
                    isActive = user.IsActive,
                    departmentId = user.DepartmentId,
                    passwordValid = passwordCheck,
                    roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar usuário admin");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
