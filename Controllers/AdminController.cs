using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Data;
using IntranetDocumentos.Services.Notifications;

namespace IntranetDocumentos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IEmailService _emailService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        /// <summary>
        /// Lista todos os usuários e seus papéis.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Acessando lista de usuários.");
            var users = await _userManager.Users
                .Include(u => u.Department)
                .ToListAsync();

            var usersWithRoles = new List<(ApplicationUser user, IList<string> roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add((user, roles));
            }

            return View(usersWithRoles);
        }        /// <summary>
        /// Exibe formulário para criação de usuário.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new CreateUserViewModel
            {
                AvailableDepartments = departments,
                AvailableRoles = roles
            };

            return View(viewModel);
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DepartmentId = model.DepartmentId
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Adicionar role ao usuário
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    _logger.LogInformation("Usuário {Email} criado com sucesso.", model.Email);
                    TempData["Success"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Erro ao criar usuário: {Error}", error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Recarregar listas em caso de erro
            model.AvailableDepartments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new CreateUserViewModel
            {
                Email = user.Email ?? string.Empty,
                DepartmentId = user.DepartmentId,
                Role = userRoles.FirstOrDefault() ?? string.Empty,
                AvailableDepartments = departments,
                AvailableRoles = allRoles
            };

            ViewBag.UserId = id;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, CreateUserViewModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Id de usuário nulo ou vazio ao editar usuário.");
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado ao editar: {Id}", id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Atualizar dados do usuário
                user.Email = model.Email;
                user.UserName = model.Email;
                user.DepartmentId = model.DepartmentId;

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    _logger.LogInformation("Usuário {Email} atualizado com sucesso.", model.Email);
                    // Atualizar roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Atualizar senha se fornecida
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    TempData["Success"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in updateResult.Errors)
                {
                    _logger.LogWarning("Erro ao atualizar usuário: {Error}", error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Recarregar listas em caso de erro
            model.AvailableDepartments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            ViewBag.UserId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Id de usuário nulo ou vazio ao excluir usuário.");
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado ao excluir: {Id}", id);
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário {Email} excluído com sucesso.", user.Email);
                TempData["Success"] = "Usuário excluído com sucesso!";
            }
            else
            {
                _logger.LogError("Erro ao excluir usuário {Email}", user.Email);
                TempData["Error"] = "Erro ao excluir usuário.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Departments
                .Include(d => d.Users)
                .Include(d => d.Documents)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(departments);
        }

        /// <summary>
        /// Exibe a página de envio de emails para administradores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SendEmail()
        {
            var model = new SendEmailViewModel
            {
                Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync()
            };

            return View(model);
        }

        /// <summary>
        /// Processa o envio de emails
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(SendEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                return View(model);
            }

            try
            {
                var recipients = await GetEmailRecipientsAsync(model);
                
                if (!recipients.Any())
                {
                    model.ErrorMessage = "Nenhum destinatário encontrado com os critérios selecionados.";
                    model.Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                    return View(model);
                }

                var emailAddresses = recipients.Where(e => !string.IsNullOrEmpty(e)).ToList();
                model.TotalRecipients = emailAddresses.Count;

                if (!emailAddresses.Any())
                {
                    model.ErrorMessage = "Nenhum email válido encontrado para os destinatários selecionados.";
                    model.Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                    return View(model);
                }

                // Enviar email
                var success = await _emailService.SendEmailToMultipleAsync(
                    emailAddresses,
                    model.Subject,
                    model.Message,
                    model.IsHtml
                );

                if (success)
                {
                    model.EmailSent = true;
                    _logger.LogInformation("Email enviado pelo admin para {Count} destinatários. Assunto: {Subject}", 
                        emailAddresses.Count, model.Subject);
                    TempData["Success"] = $"Email enviado com sucesso para {emailAddresses.Count} destinatários!";
                }
                else
                {
                    model.ErrorMessage = "Erro ao enviar o email. Verifique as configurações SMTP.";
                    _logger.LogWarning("Falha ao enviar email pelo admin. Assunto: {Subject}", model.Subject);
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Erro interno ao enviar email. Tente novamente.";
                _logger.LogError(ex, "Erro ao enviar email pelo admin. Assunto: {Subject}", model.Subject);
            }

            model.Departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// API para obter contagem de destinatários em tempo real
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetRecipientsCount([FromBody] SendEmailViewModel model)
        {
            try
            {
                var recipients = await GetEmailRecipientsAsync(model);
                var validEmails = recipients.Where(e => !string.IsNullOrEmpty(e)).Count();
                
                return Json(new { count = validEmails, success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar destinatários");
                return Json(new { count = 0, success = false, error = "Erro ao contar destinatários" });
            }
        }

        /// <summary>
        /// Obtém lista de emails baseada nos critérios selecionados
        /// </summary>
        private async Task<List<string>> GetEmailRecipientsAsync(SendEmailViewModel model)
        {
            var emails = new List<string>();

            switch (model.RecipientType)
            {
                case EmailRecipientType.AllUsers:
                    var allUsers = await _userManager.Users
                        .Where(u => !string.IsNullOrEmpty(u.Email))
                        .Select(u => u.Email!)
                        .ToListAsync();
                    emails.AddRange(allUsers);
                    break;

                case EmailRecipientType.Department:
                    if (model.DepartmentId.HasValue)
                    {
                        var departmentUsers = await _userManager.Users
                            .Where(u => u.DepartmentId == model.DepartmentId.Value && !string.IsNullOrEmpty(u.Email))
                            .Select(u => u.Email!)
                            .ToListAsync();
                        emails.AddRange(departmentUsers);
                    }
                    break;

                case EmailRecipientType.Specific:
                    if (!string.IsNullOrEmpty(model.SpecificEmails))
                    {
                        var specificEmails = model.SpecificEmails
                            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .ToList();
                        emails.AddRange(specificEmails);
                    }
                    break;

                case EmailRecipientType.AdminOnly:
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                    var adminEmails = adminUsers
                        .Where(u => !string.IsNullOrEmpty(u.Email))
                        .Select(u => u.Email!)
                        .ToList();
                    emails.AddRange(adminEmails);
                    break;

                case EmailRecipientType.ManagersOnly:
                    var managerUsers = await _userManager.GetUsersInRoleAsync("Gestor");
                    var managerEmails = managerUsers
                        .Where(u => !string.IsNullOrEmpty(u.Email))
                        .Select(u => u.Email!)
                        .ToList();
                    emails.AddRange(managerEmails);
                    break;
            }

            return emails.Distinct().ToList();
        }
    }
}
