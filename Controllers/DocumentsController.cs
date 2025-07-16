using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services;
using IntranetDocumentos.Services.Security;

namespace IntranetDocumentos.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ISecureFileUploadService _secureFileUploadService;
        private readonly IUserRateLimitingService _userRateLimitingService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAnalyticsService _analyticsService;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ISecureFileUploadService secureFileUploadService,
            IUserRateLimitingService userRateLimitingService,
            UserManager<ApplicationUser> userManager,
            IAnalyticsService analyticsService,
            IWorkflowService workflowService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _secureFileUploadService = secureFileUploadService;
            _userRateLimitingService = userRateLimitingService;
            _userManager = userManager;
            _analyticsService = analyticsService;
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <summary>
        /// Lista documentos disponíveis para o usuário logado.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Usuário não autenticado tentou acessar Index.");
                return Challenge();
            }

            var documents = await _documentService.GetDocumentsForUserAsync(user);
            return View(documents);
        }

        /// <summary>
        /// Exibe o formulário de upload de documentos.
        /// </summary>
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

        /// <summary>
        /// 🔒 Realiza o upload de um documento com rate limiting baseado em usuário específico
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // 🔒 NOVO: Verificar rate limiting por usuário específico ANTES de processar
            if (!await _userRateLimitingService.IsUploadAllowedAsync(user.Id))
            {
                _logger.LogWarning("🔒 UPLOAD BLOQUEADO: Usuário {UserId} ({Email}) excedeu limite de uploads",
                    user.Id, user.Email);
                
                ModelState.AddModelError("", "Você excedeu o limite de uploads permitidos. Tente novamente mais tarde.");
                model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                return View(model);
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
                    _logger.LogInformation("🔒 Iniciando upload de documento - Arquivo: {FileName}, Tamanho: {FileSize} bytes, Usuário: {UserId}, Departamento: {DepartmentId}", 
                        model.File.FileName, model.File.Length, user.Id, model.DepartmentId);
                    
                    await _documentService.SaveDocumentAsync(model.File, user, model.DepartmentId);
                    
                    // 🔒 Registrar upload bem-sucedido para rate limiting
                    await _userRateLimitingService.RecordUploadAttemptAsync(user.Id);
                    
                    _logger.LogInformation("✅ Upload concluído com sucesso - Arquivo: {FileName}, Usuário: {UserId}", 
                        model.File.FileName, user.Id);
                    
                    TempData["Success"] = "Documento enviado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Acesso negado ao fazer upload do documento");
                    ModelState.AddModelError("", "Você não tem permissão para fazer upload neste departamento.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Operação inválida no upload do documento");
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Argumento inválido no upload do documento");
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao salvar documento - Arquivo: {FileName}, Usuário: {UserId}", 
                        model.File?.FileName, user.Id);
                    ModelState.AddModelError("", $"Erro ao salvar o documento: {ex.Message}");
                }
            }

            model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
            return View(model);
        }

        /// <summary>
        /// Realiza o download de um documento.
        /// </summary>
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

            var fileStream = await _documentService.GetDocumentStreamAsync(document.Id, user);
            if (fileStream == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Download de documento - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);

            return File(fileStream, "application/octet-stream", document.OriginalFileName);
        }

        /// <summary>
        /// Exibe a página de busca avançada de documentos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Usuário não autenticado tentou acessar AdvancedSearch.");
                return Challenge();
            }

            ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);
            return View(new List<Document>());
        }

        /// <summary>
        /// Processa a busca avançada de documentos.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvancedSearch(
            string? searchTerm, 
            int? departmentId, 
            string? contentType, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Usuário não autenticado tentou fazer busca avançada.");
                return Challenge();
            }

            try
            {
                _logger.LogInformation("🔍 Busca avançada iniciada - Usuário: {UserId}, Termo: {SearchTerm}, Departamento: {DepartmentId}, Tipo: {ContentType}", 
                    user.Id, searchTerm, departmentId, contentType);

                var documents = await _documentService.AdvancedSearchAsync(
                    searchTerm, 
                    departmentId, 
                    contentType, 
                    startDate, 
                    endDate, 
                    user);

                // Preservar valores do formulário para exibição
                ViewBag.SearchTerm = searchTerm;
                ViewBag.DepartmentId = departmentId;
                ViewBag.ContentType = contentType;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);

                _logger.LogInformation("✅ Busca avançada concluída - Usuário: {UserId}, Resultados: {Count}", 
                    user.Id, documents.Count);

                return View(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na busca avançada - Usuário: {UserId}", user.Id);
                TempData["Error"] = "Erro ao realizar a busca. Tente novamente.";
                
                ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);
                return View(new List<Document>());
            }
        }

        /// <summary>
        /// Exclui um documento.
        /// </summary>
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
                await _documentService.DeleteDocumentAsync(id, user);
                _logger.LogInformation("Documento excluído - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Success"] = "Documento excluído com sucesso!";
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Tentativa não autorizada de exclusão - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Error"] = "Você não tem permissão para excluir este documento.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Error"] = "Erro ao excluir o documento.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Visualiza um documento no navegador (PDFs, imagens, etc).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!await _documentService.CanUserAccessDocumentAsync(id, user))
            {
                _logger.LogWarning("Tentativa não autorizada de visualização - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                return Forbid();
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Documento não encontrado para visualização - ID: {DocumentId}", id);
                return NotFound();
            }

            try
            {
                var fileStream = await _documentService.GetDocumentStreamAsync(document.Id, user);
                if (fileStream == null)
                {
                    _logger.LogWarning("Stream do documento não encontrado - ID: {DocumentId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Visualização de documento - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);

                // Determinar o Content-Type correto baseado na extensão
                var contentType = GetContentTypeFromExtension(document.OriginalFileName);
                
                // Para PDFs e imagens, usar inline para exibir no navegador
                if (IsViewableInBrowser(document.OriginalFileName))
                {
                    Response.Headers["Content-Disposition"] = $"inline; filename=\"{document.OriginalFileName}\"";
                }
                else
                {
                    // Para outros tipos, forçar download
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{document.OriginalFileName}\"";
                }

                return File(fileStream, contentType, document.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar documento - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Error"] = "Erro ao visualizar o documento. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Determina se um arquivo pode ser visualizado diretamente no navegador.
        /// </summary>
        private static bool IsViewableInBrowser(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => true,
                ".jpg" or ".jpeg" => true,
                ".png" => true,
                ".gif" => true,
                ".bmp" => true,
                ".webp" => true,
                ".txt" => true,
                ".svg" => true,
                _ => false
            };
        }

        /// <summary>
        /// Obtém o Content-Type correto baseado na extensão do arquivo.
        /// </summary>
        private static string GetContentTypeFromExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".svg" => "image/svg+xml",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
