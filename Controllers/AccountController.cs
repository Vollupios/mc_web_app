using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Exibe tela de login.
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            _logger.LogInformation("Acessando tela de login.");
            return View();
        }

        /// <summary>
        /// Realiza login do usuário.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário {Email} logado com sucesso.", model.Email);
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
                    _logger.LogWarning("Tentativa de login inválida para {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            // Implementar autenticação de dois fatores se necessário
            return View();
        }

        /// <summary>
        /// Realiza logout do usuário.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuário deslogado.");
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Exibe tela de acesso negado.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Acesso negado a recurso protegido.");
            return View();
        }
    }
}
