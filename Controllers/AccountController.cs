using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services.Security;

namespace IntranetDocumentos.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISecurityAlertService _securityAlertService;
        private readonly IUserRateLimitingService _userRateLimitingService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ISecurityAlertService securityAlertService,
            IUserRateLimitingService userRateLimitingService,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _securityAlertService = securityAlertService;
            _userRateLimitingService = userRateLimitingService;
            _logger = logger;
        }

        /// <summary>
        /// Exibe tela de login.
        /// </summary>
        [HttpGet]
        public ActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            _logger.LogInformation("Acessando tela de login.");
            return View();
        }

        /// <summary>
        /// 🔒 Realiza login do usuário com rate limiting baseado em email/usuário específico
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // 🔒 NOVO: Verificar rate limiting por usuário específico
                if (!await _userRateLimitingService.IsLoginAllowedAsync(model.Email, clientIp))
                {
                    var status = await _userRateLimitingService.GetUserStatusAsync(model.Email);
                    
                    _logger.LogWarning("🔒 LOGIN BLOQUEADO: Usuário {Email} excedeu limite de tentativas - Tempo restante: {RemainingTime} (IP: {IP})", 
                        model.Email, status.RemainingLockoutTime, clientIp);
                    
                    var lockoutMessage = status.RemainingLockoutTime.HasValue 
                        ? $"Muitas tentativas de login. Conta bloqueada por mais {Math.Ceiling(status.RemainingLockoutTime.Value.TotalMinutes)} minutos."
                        : $"Muitas tentativas de login. Você pode tentar novamente em alguns minutos.";
                    
                    ModelState.AddModelError(string.Empty, lockoutMessage);
                    return View(model);
                }

                _logger.LogInformation("🔒 Tentativa de login para: {Email} do IP: {IP}", model.Email, clientIp);
                
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false); // Rate limiting próprio, não usar o do Identity

                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Usuário {Email} logado com sucesso.", model.Email);
                    
                    // 🔒 Registrar login bem-sucedido (limpa histórico de falhas)
                    await _userRateLimitingService.RecordLoginAttemptAsync(model.Email, clientIp, true);
                    
                    return LocalRedirect(returnUrl ?? "/Documents");
                }

                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("Login requer autenticação de dois fatores para {Email}.", model.Email);
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta bloqueada para {Email}.", model.Email);
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    _logger.LogWarning("❌ Tentativa de login inválida para {Email}.", model.Email);
                    
                    // 🔒 Registrar tentativa falhada
                    await _userRateLimitingService.RecordLoginAttemptAsync(model.Email, clientIp, false);
                    
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuário fez logout.");
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            // Implementar autenticação de dois fatores se necessário
            return View();
        }
    }
}
