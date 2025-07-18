using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services;

namespace IntranetDocumentos.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento dos ramais telefônicos
    /// Refatorado para usar FileUploadService (SRP)
    /// </summary>
    [Authorize]
    public class RamaisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RamaisController> _logger;
        private readonly IFileUploadService _fileUploadService;

        public RamaisController(
            ApplicationDbContext context, 
            ILogger<RamaisController> logger,
            IFileUploadService fileUploadService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Exibe a lista de ramais
        /// </summary>
        /// <returns>View com lista de ramais</returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Acessando lista de ramais");

                var ramais = await _context.Ramais
                    .Include(r => r.Department)
                    .Where(r => r.Ativo)
                    .OrderBy(r => r.Numero)
                    .ToListAsync();

                return View(ramais);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar lista de ramais");
                TempData["Error"] = "Erro ao carregar a lista de ramais";
                return View(new List<Ramal>());
            }
        }

        /// <summary>
        /// Exibe formulário para criar novo ramal
        /// </summary>
        /// <returns>View de criação</returns>
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            try
            {
                var viewModel = new RamalViewModel
                {
                    AvailableDepartments = await _context.Departments
                        .OrderBy(d => d.Name)
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de criação de ramal");
                TempData["Error"] = "Erro ao carregar o formulário";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processa a criação de um novo ramal
        /// </summary>
        /// <param name="viewModel">Dados do ramal</param>
        /// <returns>Redirecionamento ou view com erros</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(RamalViewModel viewModel)
        {            try
            {
                if (ModelState.IsValid)
                {                    // Removido: validação de número único - permitir múltiplas pessoas com mesmo ramal
                    
                    var ramal = new Ramal
                    {
                        Numero = viewModel.Numero,
                        Nome = viewModel.Nome,
                        TipoFuncionario = viewModel.TipoFuncionario,
                        DepartmentId = viewModel.DepartmentId,
                        Observacoes = viewModel.Observacoes,
                        Ativo = viewModel.Ativo,
                        DataCriacao = DateTime.Now
                    };

                    // Processar upload da foto
                    if (viewModel.FotoFile != null && viewModel.FotoFile.Length > 0)
                    {
                        ramal.FotoPath = await SalvarFoto(viewModel.FotoFile);
                    }

                    _context.Ramais.Add(ramal);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Ramal {Numero} criado com sucesso para {Nome}", 
                        ramal.Numero, ramal.Nome);

                    TempData["Success"] = "Ramal criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.AvailableDepartments = await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ramal");
                TempData["Error"] = "Erro ao criar o ramal";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Exibe formulário para editar ramal
        /// </summary>
        /// <param name="id">ID do ramal</param>
        /// <returns>View de edição</returns>
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var ramal = await _context.Ramais
                    .Include(r => r.Department)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (ramal == null)
                {
                    TempData["Error"] = "Ramal não encontrado";
                    return RedirectToAction(nameof(Index));
                }                var viewModel = new RamalViewModel
                {
                    Id = ramal.Id,
                    Numero = ramal.Numero,
                    Nome = ramal.Nome,
                    TipoFuncionario = ramal.TipoFuncionario,
                    DepartmentId = ramal.DepartmentId,
                    Observacoes = ramal.Observacoes,
                    Ativo = ramal.Ativo,
                    FotoPath = ramal.FotoPath,
                    AvailableDepartments = await _context.Departments
                        .OrderBy(d => d.Name)
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar ramal para edição: {Id}", id);
                TempData["Error"] = "Erro ao carregar o ramal";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processa a edição de um ramal
        /// </summary>
        /// <param name="id">ID do ramal</param>
        /// <param name="viewModel">Dados atualizados</param>
        /// <returns>Redirecionamento ou view com erros</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int id, RamalViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Id)
                {
                    TempData["Error"] = "Dados inválidos";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    var ramal = await _context.Ramais.FindAsync(id);
                    if (ramal == null)
                    {
                        TempData["Error"] = "Ramal não encontrado";
                        return RedirectToAction(nameof(Index));
                    }                    // Removido: validação de número único - permitir múltiplas pessoas com mesmo ramalramal.Numero = viewModel.Numero;
                    ramal.Nome = viewModel.Nome;
                    ramal.TipoFuncionario = viewModel.TipoFuncionario;
                    ramal.DepartmentId = viewModel.DepartmentId;
                    ramal.Observacoes = viewModel.Observacoes;
                    ramal.Ativo = viewModel.Ativo;                    // Processar nova foto se fornecida
                    if (viewModel.FotoFile != null && viewModel.FotoFile.Length > 0)
                    {
                        // Remover foto antiga se existir
                        if (!string.IsNullOrEmpty(ramal.FotoPath))
                        {
                            await RemoverFoto(ramal.FotoPath);
                        }

                        ramal.FotoPath = await SalvarFoto(viewModel.FotoFile);
                    }

                    _context.Update(ramal);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Ramal {Numero} atualizado com sucesso", ramal.Numero);

                    TempData["Success"] = "Ramal atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.AvailableDepartments = await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar ramal: {Id}", id);
                TempData["Error"] = "Erro ao atualizar o ramal";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Remove um ramal (desativa)
        /// </summary>
        /// <param name="id">ID do ramal</param>
        /// <returns>Redirecionamento</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var ramal = await _context.Ramais.FindAsync(id);
                if (ramal == null)
                {
                    TempData["Warning"] = "Ramal não encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Desativar em vez de excluir
                ramal.Ativo = false;
                _context.Update(ramal);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ramal {Numero} desativado", ramal.Numero);

                TempData["Success"] = "Ramal removido com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover ramal: {Id}", id);
                TempData["Error"] = "Erro ao remover o ramal";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Salva a foto do ramal no servidor        /// <summary>
        /// Salva uma foto usando o FileUploadService
        /// </summary>
        /// <param name="foto">Arquivo da foto</param>
        /// <returns>Caminho da foto salva</returns>
        private async Task<string> SalvarFoto(IFormFile foto)
        {
            return await _fileUploadService.SaveImageAsync(foto, "ramais");
        }

        /// <summary>
        /// Remove uma foto usando o FileUploadService
        /// </summary>
        /// <param name="fotoPath">Caminho da foto</param>
        private async Task RemoverFoto(string fotoPath)
        {
            if (!string.IsNullOrEmpty(fotoPath))
            {                await _fileUploadService.DeleteImageAsync(fotoPath);
            }
        }
    }
}
