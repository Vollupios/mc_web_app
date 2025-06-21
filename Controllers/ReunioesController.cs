using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Controllers
{
    /// <summary>
    /// Controller refatorado para usar ReuniaoService
    /// Implementa Princípio da Responsabilidade Única (SRP)
    /// </summary>
    [Authorize]
    public class ReunioesController : Controller
    {
        private readonly IReuniaoService _reuniaoService;
        private readonly ILogger<ReunioesController> _logger;

        public ReunioesController(IReuniaoService reuniaoService, ILogger<ReunioesController> logger)
        {
            _reuniaoService = reuniaoService;
            _logger = logger;
        }        // GET: Reunioes
        public async Task<IActionResult> Index(int ano = 0, int mes = 0)
        {
            try
            {
                if (ano == 0) ano = DateTime.Now.Year;
                if (mes == 0) mes = DateTime.Now.Month;

                var primeiroDiaMes = new DateTime(ano, mes, 1);
                var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

                var reunioes = await _reuniaoService.GetReunioesPorPeriodoAsync(primeiroDiaMes, ultimoDiaMes);

                var viewModel = new CalendarioViewModel
                {
                    Ano = ano,
                    Mes = mes,
                    Reunioes = reunioes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar calendário de reuniões");
                TempData["ErrorMessage"] = "Erro ao carregar as reuniões. Tente novamente.";
                return View(new CalendarioViewModel { Ano = DateTime.Now.Year, Mes = DateTime.Now.Month, Reunioes = new List<Reuniao>() });
            }
        }        // GET: Reunioes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var reuniao = await _reuniaoService.GetReuniaoDetalhadaAsync(id.Value);
                if (reuniao == null) return NotFound();

                return View(reuniao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da reunião: {Id}", id);
                TempData["ErrorMessage"] = "Erro ao carregar os detalhes da reunião.";
                return RedirectToAction(nameof(Index));
            }
        }        // GET: Reunioes/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new ReuniaoViewModel
                {
                    Data = DateTime.Today,
                    HoraInicio = TimeSpan.FromHours(9), // 09:00
                    HoraFim = TimeSpan.FromHours(10)    // 10:00
                };

                await _reuniaoService.PopularDadosViewModelAsync(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de reunião");
                TempData["ErrorMessage"] = "Erro ao carregar o formulário.";
                return RedirectToAction(nameof(Index));
            }
        }// POST: Reunioes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReuniaoViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return await ReturnCreateViewWithData(viewModel);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                    throw new InvalidOperationException("Usuário não encontrado");

                var reuniao = await _reuniaoService.CriarReuniaoAsync(viewModel, userId);
                
                TempData["Success"] = "Reunião agendada com sucesso!";
                return RedirectToAction(nameof(Details), new { id = reuniao.Id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return await ReturnCreateViewWithData(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reunião");
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
                return await ReturnCreateViewWithData(viewModel);
            }
        }

        private async Task<IActionResult> ReturnCreateViewWithData(ReuniaoViewModel viewModel)
        {
            await _reuniaoService.PopularDadosViewModelAsync(viewModel);
            return View(viewModel);
        }        // GET: Reunioes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var reuniao = await _reuniaoService.GetReuniaoDetalhadaAsync(id.Value);
                if (reuniao == null) return NotFound();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var isAdmin = User.IsInRole("Admin");

                // Verificar permissões
                if (!await _reuniaoService.PodeEditarReuniaoAsync(id.Value, userId, isAdmin))
                {
                    return Forbid();
                }

                var viewModel = await _reuniaoService.MapearParaViewModelAsync(reuniao);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar reunião para edição: {Id}", id);
                TempData["ErrorMessage"] = "Erro ao carregar a reunião.";
                return RedirectToAction(nameof(Index));
            }
        }        // POST: Reunioes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReuniaoViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (!ModelState.IsValid)
                return await ReturnEditViewWithData(viewModel);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var isAdmin = User.IsInRole("Admin");

                var sucesso = await _reuniaoService.AtualizarReuniaoAsync(id, viewModel, userId);
                
                if (!sucesso)
                    return NotFound();

                TempData["Success"] = "Reunião atualizada com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return await ReturnEditViewWithData(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar reunião: {Id}", id);
                if (ex is DbUpdateConcurrencyException)
                {
                    if (!await _reuniaoService.ReuniaoExisteAsync(id))
                        return NotFound();
                }
                
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
                return await ReturnEditViewWithData(viewModel);
            }
        }

        private async Task<IActionResult> ReturnEditViewWithData(ReuniaoViewModel viewModel)
        {
            await _reuniaoService.PopularDadosViewModelAsync(viewModel);
            return View(viewModel);
        }        // POST: Reunioes/MarcarRealizada/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarRealizada(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var isAdmin = User.IsInRole("Admin");

                var sucesso = await _reuniaoService.MarcarReuniaoRealizadaAsync(id, userId, isAdmin);

                if (!sucesso)
                    return NotFound();

                TempData["Success"] = "Reunião marcada como realizada!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar reunião como realizada: {Id}", id);
                TempData["ErrorMessage"] = "Erro ao marcar reunião como realizada.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Reunioes/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var isAdmin = User.IsInRole("Admin");

                var sucesso = await _reuniaoService.CancelarReuniaoAsync(id, userId, isAdmin);

                if (!sucesso)
                    return NotFound();

                TempData["Success"] = "Reunião cancelada!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reunião: {Id}", id);
                TempData["ErrorMessage"] = "Erro ao cancelar reunião.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Reunioes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var isAdmin = User.IsInRole("Admin");

                var sucesso = await _reuniaoService.RemoverReuniaoAsync(id, userId);

                if (!sucesso)
                    return NotFound();

                TempData["Success"] = "Reunião excluída com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir reunião: {Id}", id);
                TempData["ErrorMessage"] = "Erro ao excluir reunião.";
                return RedirectToAction(nameof(Index));
            }
        }        // GET: Reunioes/CreateSimple - Teste simples
        [AllowAnonymous]
        public IActionResult CreateSimple()
        {
            var viewModel = new ReuniaoViewModel
            {
                Data = DateTime.Today,
                HoraInicio = TimeSpan.FromHours(9),
                HoraFim = TimeSpan.FromHours(10)
            };
            return View(viewModel);
        }
    }
}
