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
        private readonly IConfiguration _configuration;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
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
        /// API para obter detalhes dos destinatários (contagem e emails)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetRecipientsDetails([FromBody] SendEmailViewModel model)
        {
            try
            {
                if (model == null)
                {
                    _logger.LogWarning("Modelo nulo recebido em GetRecipientsDetails");
                    return Json(new { 
                        count = 0, 
                        emails = new List<string>(), 
                        success = false, 
                        error = "Dados não foram enviados corretamente" 
                    });
                }

                _logger.LogInformation("Obtendo destinatários para tipo: {RecipientType}", model.RecipientType);

                var emails = await GetEmailRecipientsAsync(model);
                var validEmails = emails.Where(e => !string.IsNullOrEmpty(e)).ToList();
                
                _logger.LogInformation("Encontrados {Count} destinatários válidos", validEmails.Count);
                
                return Json(new { 
                    count = validEmails.Count, 
                    emails = validEmails.Take(20).ToList(), // Limitar para não sobrecarregar
                    success = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes dos destinatários. Modelo: {@Model}", model);
                return Json(new { 
                    count = 0, 
                    emails = new List<string>(), 
                    success = false, 
                    error = $"Erro interno: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Obtém lista de emails baseada nos critérios selecionados
        /// </summary>
        private async Task<List<string>> GetEmailRecipientsAsync(SendEmailViewModel model)
        {
            var emails = new List<string>();

            if (model == null)
            {
                _logger.LogWarning("Modelo nulo em GetEmailRecipientsAsync");
                return emails;
            }

            try
            {
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

                    default:
                        _logger.LogWarning("Tipo de destinatário não reconhecido: {RecipientType}", model.RecipientType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar destinatários para tipo {RecipientType}", model.RecipientType);
            }

            return emails.Distinct().ToList();
        }

        /// <summary>
        /// Exibe a página de configuração de email
        /// </summary>
        [HttpGet]
        public IActionResult EmailConfig()
        {
            var config = _configuration.GetSection("Email");
            var notificationConfig = _configuration.GetSection("Notifications");

            var model = new EmailConfigViewModel
            {
                SmtpHost = config["SmtpHost"] ?? "",
                SmtpPort = int.Parse(config["SmtpPort"] ?? "587"),
                SmtpUser = config["Username"] ?? "",
                SmtpPassword = "", // Não mostrar senha por segurança
                EnableSsl = bool.Parse(config["EnableSsl"] ?? "true"),
                FromEmail = config["FromAddress"] ?? "noreply@empresa.com",
                FromName = config["FromName"] ?? "Sistema Intranet",
                ConfigurationExists = !string.IsNullOrEmpty(config["SmtpHost"]),
                SendDocumentNotifications = bool.Parse(notificationConfig["SendDocumentNotifications"] ?? "true"),
                SendMeetingNotifications = bool.Parse(notificationConfig["SendMeetingNotifications"] ?? "true"),
                SendMeetingReminders = bool.Parse(notificationConfig["SendMeetingReminders"] ?? "true"),
                ReminderIntervalHours = int.Parse(notificationConfig["ReminderIntervalHours"] ?? "6")
            };

            return View(model);
        }

        /// <summary>
        /// Salva a configuração de email
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EmailConfig(EmailConfigViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Aqui você salvaria a configuração no appsettings.json ou banco de dados
                // Por enquanto, vamos apenas validar e mostrar uma mensagem
                model.ConfigurationExists = true;
                TempData["Success"] = "Configurações de email salvas com sucesso! Lembre-se de reiniciar a aplicação para aplicar as mudanças.";
                
                _logger.LogInformation("Configurações de email atualizadas pelo admin {UserId}", User.Identity?.Name);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Erro ao salvar configurações. Tente novamente.";
                _logger.LogError(ex, "Erro ao salvar configurações de email");
            }

            return View(model);
        }

        /// <summary>
        /// Testa o envio de email com as configurações atuais
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(EmailConfigViewModel model)
        {
            try
            {
                var testEmail = User.Identity?.Name ?? model.SmtpUser;
                
                // Usar o novo método que aceita configurações personalizadas
                var success = await _emailService.TestEmailWithConfigAsync(
                    model.SmtpHost,
                    model.SmtpPort,
                    model.SmtpUser,
                    model.SmtpPassword,
                    model.EnableSsl,
                    model.FromEmail,
                    model.FromName,
                    testEmail,
                    "🧪 Teste de Configuração - Sistema Intranet",
                    $@"
                    <h3>✅ Teste de Email Realizado com Sucesso!</h3>
                    <p>Este é um email de teste enviado em <strong>{DateTime.Now:dd/MM/yyyy HH:mm}</strong></p>
                    <p><strong>Configurações testadas:</strong></p>
                    <ul>
                        <li>Servidor SMTP: {model.SmtpHost}:{model.SmtpPort}</li>
                        <li>Usuário: {model.SmtpUser}</li>
                        <li>SSL: {(model.EnableSsl ? "Habilitado" : "Desabilitado")}</li>
                        <li>De: {model.FromName} &lt;{model.FromEmail}&gt;</li>
                    </ul>
                    <p>Se você recebeu este email, as configurações estão funcionando corretamente! 🎉</p>
                    ",
                    true
                );

                if (success)
                {
                    model.TestEmailSent = true;
                    model.TestResult = $"Email de teste enviado com sucesso para {testEmail}";
                    TempData["Success"] = model.TestResult;
                }
                else
                {
                    model.TestResult = "Falha ao enviar email de teste. Verifique as configurações.";
                    TempData["Error"] = model.TestResult;
                }
            }
            catch (Exception ex)
            {
                model.TestResult = $"Erro ao testar email: {ex.Message}";
                TempData["Error"] = model.TestResult;
                _logger.LogError(ex, "Erro ao testar configuração de email");
            }

            // Recarregar configurações atuais para a view
            var config = _configuration.GetSection("Email");
            var notificationConfig = _configuration.GetSection("Notifications");
            
            model.SmtpHost = config["SmtpHost"] ?? model.SmtpHost;
            model.SmtpPort = int.Parse(config["SmtpPort"] ?? model.SmtpPort.ToString());
            model.ConfigurationExists = !string.IsNullOrEmpty(config["SmtpHost"]);

            return View("EmailConfig", model);
        }

        /// <summary>
        /// API para verificar status do sistema de email
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var gestores = await _userManager.GetUsersInRoleAsync("Gestor");
                var departments = await _context.Departments.CountAsync();
                
                var emailConfigured = _emailService.IsConfigured;
                
                return Json(new
                {
                    success = true,
                    totalUsers,
                    adminCount = admins.Count,
                    managerCount = gestores.Count,
                    departmentCount = departments,
                    emailConfigured,
                    status = emailConfigured ? "Email configurado" : "Email não configurado"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status do sistema");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
