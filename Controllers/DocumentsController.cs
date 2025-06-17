using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services;

namespace IntranetDocumentos.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            UserManager<ApplicationUser> userManager,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var documents = await _documentService.GetDocumentsForUserAsync(user);
            return View(documents);
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var departments = await _documentService.GetDepartmentsForUserAsync(user);
            var viewModel = new UploadViewModel
            {
                AvailableDepartments = departments
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                // Verificar se o usuário pode fazer upload para o departamento selecionado
                if (!await _documentService.CanUserUploadToDepartmentAsync(model.DepartmentId, user))
                {
                    ModelState.AddModelError("", "Você não tem permissão para fazer upload neste departamento.");
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View(model);
                }

                // Validar arquivo
                if (model.File == null || model.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Por favor, selecione um arquivo.");
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View(model);
                }

                // Validar tamanho do arquivo (10MB max)
                if (model.File.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("File", "O arquivo deve ter no máximo 10MB.");
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View(model);
                }

                try
                {
                    await _documentService.SaveDocumentAsync(model.File, user, model.DepartmentId);
                    TempData["Success"] = "Documento enviado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao salvar documento");
                    ModelState.AddModelError("", "Erro ao salvar o documento. Tente novamente.");
                }
            }

            model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!await _documentService.CanUserAccessDocumentAsync(id, user))
            {
                return Forbid();
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = await _documentService.GetDocumentPhysicalPathAsync(document);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Arquivo não encontrado no servidor.");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, document.ContentType, document.OriginalFileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            try
            {
                var result = await _documentService.DeleteDocumentAsync(id, user);
                if (result)
                {
                    TempData["Success"] = "Documento excluído com sucesso!";
                }
                else
                {
                    TempData["Error"] = "Erro ao excluir documento ou você não tem permissão para esta ação.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento {DocumentId}", id);
                TempData["Error"] = "Erro ao excluir documento.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!await _documentService.CanUserAccessDocumentAsync(id, user))
            {
                return Forbid();
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }
    }
}
