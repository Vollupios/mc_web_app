using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Data;

namespace IntranetDocumentos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
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
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var viewModel = new CreateUserViewModel
            {
                AvailableDepartments = departments,
                AvailableRoles = roles
            };

            return View(viewModel);
        }

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
                    ModelState.AddModelError(string.Empty, error.Description);
                }            }

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

            var userRoles = await _userManager.GetRolesAsync(user);            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
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
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
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
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }            // Recarregar listas em caso de erro
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
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Usuário excluído com sucesso!";
            }
            else
            {
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
    }
}
