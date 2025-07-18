using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Models.ValueObjects;
using IntranetDocumentos.Services;
using IntranetDocumentos.Services.Security;
using IntranetDocumentos.Data;

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
        private readonly IDocumentFolderService _documentFolderService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ISecureFileUploadService secureFileUploadService,
            IUserRateLimitingService userRateLimitingService,
            UserManager<ApplicationUser> userManager,
            IAnalyticsService analyticsService,
            IWorkflowService workflowService,
            IDocumentFolderService documentFolderService,
            ApplicationDbContext context,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _secureFileUploadService = secureFileUploadService;
            _userRateLimitingService = userRateLimitingService;
            _userManager = userManager;
            _analyticsService = analyticsService;
            _workflowService = workflowService;
            _documentFolderService = documentFolderService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista documentos disponíveis para o usuário logado com navegação hierárquica.
        /// </summary>
        public async Task<ActionResult> Index(int? folderId, int? departmentId, string? searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Usuário não autenticado tentou acessar Index.");
                return Challenge();
            }

            try
            {
                // Temporariamente usar a versão debug para resolver o problema
                return await DebugIndex();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar documentos para usuário {UserId}", user.Id);
                TempData["Error"] = "Erro ao carregar documentos. Tente novamente.";
                
                // Fallback para uma view vazia
                var emptyViewModel = new DocumentTreeViewModel();
                ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);
                return View(emptyViewModel);
            }
        }

        /// <summary>
        /// Exibe o formulário de upload de documentos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Upload()
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
        public async Task<ActionResult> Upload(UploadViewModel model)
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

                // Validar tamanho do arquivo usando Value Object (10MB max)
                var fileSize = FileSize.FromBytes(model.File.Length);
                var maxSize = FileSize.FromMegabytes(10);
                
                if (fileSize.Bytes > maxSize.Bytes)
                {
                    ModelState.AddModelError("File", $"O arquivo deve ter no máximo {maxSize.ToHumanReadableString()}. Tamanho atual: {fileSize.ToHumanReadableString()}");
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View(model);
                }

                try
                {
                    _logger.LogInformation("🔒 Iniciando upload de documento - Arquivo: {FileName}, Tamanho: {FileSize}, Usuário: {UserId}, Departamento: {DepartmentId}", 
                        model.File.FileName, fileSize.ToHumanReadableString(), user.Id, model.DepartmentId);
                    
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
        public async Task<ActionResult> Download(int id)
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
        public async Task<ActionResult> AdvancedSearch()
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
        public async Task<ActionResult> AdvancedSearch(
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
        public async Task<ActionResult> Delete(int id)
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
        public async Task<ActionResult> View(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Usuário não autenticado tentando visualizar documento {DocumentId}", id);
                return Challenge();
            }

            _logger.LogInformation("Tentativa de visualização - DocumentId: {DocumentId}, UserId: {UserId}", id, user.Id);

            if (!await _documentService.CanUserAccessDocumentAsync(id, user))
            {
                _logger.LogWarning("Tentativa não autorizada de visualização - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Error"] = "Você não tem permissão para visualizar este documento.";
                return RedirectToAction(nameof(Index));
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Documento não encontrado para visualização - ID: {DocumentId}", id);
                TempData["Error"] = "Documento não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Documento encontrado - ID: {DocumentId}, Nome: {FileName}", id, document.OriginalFileName);

            try
            {
                var fileStream = await _documentService.GetDocumentStreamAsync(document.Id, user);
                if (fileStream == null)
                {
                    _logger.LogWarning("Stream do documento não encontrado - ID: {DocumentId}", id);
                    TempData["Error"] = "Arquivo não encontrado no servidor.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Stream obtido com sucesso - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);

                // Determinar o Content-Type correto baseado na extensão
                var contentType = GetContentTypeFromExtension(document.OriginalFileName);
                
                // Codificar o nome do arquivo para evitar problemas com caracteres especiais
                var encodedFileName = Uri.EscapeDataString(document.OriginalFileName);
                
                // Para PDFs e imagens, usar inline para exibir no navegador
                if (IsViewableInBrowser(document.OriginalFileName))
                {
                    Response.Headers["Content-Disposition"] = $"inline; filename*=UTF-8''{encodedFileName}";
                    return File(fileStream, contentType);
                }
                else
                {
                    // Para outros tipos, forçar download
                    Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{encodedFileName}";
                    return File(fileStream, contentType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar documento - ID: {DocumentId}, Usuário: {UserId}", id, user.Id);
                TempData["Error"] = $"Erro ao visualizar o documento: {ex.Message}";
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

        /// <summary>
        /// Exibe a visualização hierárquica de documentos com pastas e subpastas
        /// </summary>
        public async Task<ActionResult> Tree(int? folderId, int? departmentId, string? search)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var viewModel = await _documentFolderService.GetDocumentTreeAsync(folderId, departmentId, search, user);
                
                ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);
                
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar visualização em árvore");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Navegação dentro de uma pasta específica
        /// </summary>
        public async Task<ActionResult> Browse(int? folderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var viewModel = await _documentFolderService.GetFolderNavigationAsync(folderId, user);
                
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Você não tem permissão para acessar esta pasta.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar na pasta {FolderId}", folderId);
                TempData["ErrorMessage"] = "Erro ao acessar a pasta.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Formulário para criar nova pasta
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> CreateFolder(int? parentFolderId, int? departmentId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                if (!await _documentFolderService.CanUserCreateFolderAsync(parentFolderId, departmentId, user))
                    return Forbid();

                var availableFolders = await _documentFolderService.GetFoldersForUserAsync(user, departmentId);
                var departments = await _documentService.GetDepartmentsForUserAsync(user);

                var model = new FolderFormViewModel
                {
                    ParentFolderId = parentFolderId,
                    DepartmentId = departmentId ?? user.DepartmentId,
                    AvailableParentFolders = availableFolders,
                    AvailableDepartments = departments
                };

                return View("FolderForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de criação de pasta");
                TempData["ErrorMessage"] = "Erro ao carregar formulário.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Criar nova pasta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateFolder(FolderFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableParentFolders = await _documentFolderService.GetFoldersForUserAsync(user, model.DepartmentId);
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View("FolderForm", model);
                }

                var folder = await _documentFolderService.CreateFolderAsync(model, user);
                
                TempData["SuccessMessage"] = $"Pasta '{folder.Name}' criada com sucesso!";
                
                return RedirectToAction(nameof(Browse), new { folderId = model.ParentFolderId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Você não tem permissão para criar pastas.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta");
                TempData["ErrorMessage"] = "Erro ao criar pasta.";
                
                model.AvailableParentFolders = await _documentFolderService.GetFoldersForUserAsync(user, model.DepartmentId);
                model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                return View("FolderForm", model);
            }
        }

        /// <summary>
        /// Formulário para editar pasta
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> EditFolder(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var folder = await _context.DocumentFolders.FindAsync(id);
                if (folder == null)
                    return NotFound();

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && folder.CreatedById != user.Id)
                    return Forbid();

                var availableFolders = await _documentFolderService.GetFoldersForUserAsync(user, folder.DepartmentId);
                var departments = await _documentService.GetDepartmentsForUserAsync(user);

                var model = new FolderFormViewModel
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    Description = folder.Description,
                    ParentFolderId = folder.ParentFolderId,
                    DepartmentId = folder.DepartmentId,
                    Color = folder.Color,
                    Icon = folder.Icon,
                    DisplayOrder = folder.DisplayOrder,
                    AvailableParentFolders = availableFolders.Where(f => f.Id != id).ToList(),
                    AvailableDepartments = departments
                };

                return View("FolderForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de edição de pasta");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Atualizar pasta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditFolder(FolderFormViewModel model)
        {
            try
            {
                _logger.LogInformation("Iniciando edição de pasta. ID: {FolderId}, Nome: {Name}", model.Id, model.Name);
                
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não autenticado tentando editar pasta");
                    return Challenge();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido para edição de pasta. Erros: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    model.AvailableParentFolders = await _documentFolderService.GetFoldersForUserAsync(user, model.DepartmentId);
                    model.AvailableDepartments = await _documentService.GetDepartmentsForUserAsync(user);
                    return View("FolderForm", model);
                }

                var folder = await _documentFolderService.UpdateFolderAsync(model, user);
                
                _logger.LogInformation("Pasta atualizada com sucesso. ID: {FolderId}, Nome: {Name}", folder.Id, folder.Name);
                TempData["SuccessMessage"] = $"Pasta '{folder.Name}' atualizada com sucesso!";
                
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acesso negado ao editar pasta {FolderId}", model.Id);
                TempData["ErrorMessage"] = "Você não tem permissão para editar esta pasta.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pasta {FolderId}", model.Id);
                TempData["ErrorMessage"] = "Erro ao atualizar pasta.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Excluir pasta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteFolder(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var folder = await _context.DocumentFolders.FindAsync(id);
                if (folder == null)
                    return NotFound();

                var success = await _documentFolderService.DeleteFolderAsync(id, user);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Pasta excluída com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao excluir pasta.";
                }
                
                return RedirectToAction(nameof(Browse), new { folderId = folder.ParentFolderId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Você não tem permissão para excluir esta pasta.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pasta");
                TempData["ErrorMessage"] = "Erro ao excluir pasta.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Mover documento para pasta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveDocumentAjax(int documentId, int? targetFolderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Challenge();

                var success = await _documentFolderService.MoveDocumentToFolderAsync(documentId, targetFolderId, user);
                
                if (success)
                {
                    return Json(new { success = true, message = "Documento movido com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao mover documento." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Permissão negada." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover documento");
                return Json(new { success = false, message = "Erro ao mover documento." });
            }
        }

        /// <summary>
        /// Obter pastas para dropdown (AJAX)
        /// </summary>
        public async Task<ActionResult> GetFoldersJson(int? departmentId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false });

                var folders = await _documentFolderService.GetFoldersForUserAsync(user, departmentId);
                
                var result = folders.Select(f => new
                {
                    id = f.Id,
                    name = f.Name,
                    path = f.Path,
                    level = f.Level,
                    parentId = f.ParentFolderId,
                    icon = f.Icon,
                    color = f.Color
                }).ToList();

                return Json(new { success = true, folders = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pastas");
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Exibir modal para mover documento
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> MoveDocumentModal(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "Usuário não encontrado" });

                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                    return Json(new { success = false, message = "Documento não encontrado" });

                // Verificar permissões
                if (!User.IsInRole("Admin") && !User.IsInRole("Gestor") && document.UploaderId != user.Id)
                    return Json(new { success = false, message = "Sem permissão para mover este documento" });

                // Obter departamentos disponíveis
                var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                
                // Obter pastas do departamento atual
                var currentFolders = await _documentFolderService.GetFoldersForUserAsync(user, document.DepartmentId);
                
                var viewModel = new MoveDocumentViewModel
                {
                    DocumentId = document.Id,
                    DocumentName = document.OriginalFileName,
                    CurrentDepartmentId = document.DepartmentId,
                    CurrentFolderId = document.FolderId,
                    Departments = departments,
                    AvailableFolders = currentFolders
                };

                return PartialView("_MoveDocumentModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar modal de mover documento {DocumentId}", id);
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Executar movimento do documento
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveDocument(MoveDocumentViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Usuário não encontrado";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Dados inválidos";
                    return RedirectToAction("Index");
                }

                var document = await _documentService.GetDocumentByIdAsync(model.DocumentId);
                if (document == null)
                {
                    TempData["Error"] = "Documento não encontrado";
                    return RedirectToAction("Index");
                }

                // Verificar permissões
                if (!User.IsInRole("Admin") && !User.IsInRole("Gestor") && document.UploaderId != user.Id)
                {
                    TempData["Error"] = "Sem permissão para mover este documento";
                    return RedirectToAction("Index");
                }

                // Verificar se houve mudança
                if (document.DepartmentId == model.NewDepartmentId && document.FolderId == model.NewFolderId)
                {
                    TempData["Warning"] = "Nenhuma alteração foi feita";
                    return RedirectToAction("Index");
                }

                // Executar movimento
                var result = await _documentService.MoveDocumentAsync(
                    model.DocumentId, 
                    model.NewFolderId, 
                    model.NewDepartmentId, 
                    user.Id
                );

                if (result.Success)
                {
                    var departmentName = model.NewDepartmentId.HasValue 
                        ? (await _context.Departments.FindAsync(model.NewDepartmentId.Value))?.Name ?? "Geral"
                        : "Geral";
                        
                    var folderName = model.NewFolderId.HasValue 
                        ? (await _documentFolderService.GetFolderByIdAsync(model.NewFolderId.Value))?.Name ?? "Raiz"
                        : "Raiz";

                    TempData["Success"] = $"Documento '{document.OriginalFileName}' movido para '{departmentName}' > '{folderName}' com sucesso!";
                }
                else
                {
                    TempData["Error"] = result.Message ?? "Erro ao mover documento";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover documento {DocumentId}", model.DocumentId);
                TempData["Error"] = "Erro interno do servidor";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Mover múltiplos documentos (seleção em lote)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveBulkDocuments(string documentIds, int? newFolderId, int? newDepartmentId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "Usuário não encontrado" });

                if (string.IsNullOrWhiteSpace(documentIds))
                    return Json(new { success = false, message = "Nenhum documento selecionado" });

                // Converter string de IDs para array de inteiros
                int[] documentIdArray;
                try
                {
                    documentIdArray = documentIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
                }
                catch
                {
                    return Json(new { success = false, message = "IDs de documentos inválidos" });
                }

                if (documentIdArray.Length == 0)
                    return Json(new { success = false, message = "Nenhum documento selecionado" });

                var successCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                foreach (var documentId in documentIdArray)
                {
                    var document = await _documentService.GetDocumentByIdAsync(documentId);
                    if (document == null)
                    {
                        errorCount++;
                        errors.Add($"Documento ID {documentId} não encontrado");
                        continue;
                    }

                    // Verificar permissões
                    if (!User.IsInRole("Admin") && !User.IsInRole("Gestor") && document.UploaderId != user.Id)
                    {
                        errorCount++;
                        errors.Add($"Sem permissão para mover '{document.OriginalFileName}'");
                        continue;
                    }

                    var result = await _documentService.MoveDocumentAsync(documentId, newFolderId, newDepartmentId, user.Id);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                        errors.Add($"Erro ao mover '{document.OriginalFileName}': {result.Message}");
                    }
                }

                var message = $"{successCount} documento(s) movido(s) com sucesso";
                if (errorCount > 0)
                {
                    message += $", {errorCount} erro(s)";
                }

                return Json(new { 
                    success = successCount > 0, 
                    message = message,
                    errors = errors,
                    successCount = successCount,
                    errorCount = errorCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover documentos em lote");
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Carregar modal de movimentação em lote
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetBulkMoveModal()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "Usuário não encontrado" });

                // Obter departamentos disponíveis
                var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
                ViewBag.Departments = departments;

                var viewModel = new BulkMoveDocumentViewModel
                {
                    Departments = departments
                };

                return PartialView("_BulkMoveModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar modal de movimentação em lote");
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Criar nova pasta via AJAX (para modal)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateFolderAjax(FolderFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Usuário não autenticado" });

            _logger.LogInformation("CreateFolderAjax: Name={Name}, Description={Description}, DepartmentId={DepartmentId}, ParentFolderId={ParentFolderId}", 
                model.Name, model.Description, model.DepartmentId, model.ParentFolderId);

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("CreateFolderAjax validation errors: {Errors}", string.Join(", ", errors));
                    
                    return Json(new { success = false, message = "Dados inválidos", errors = errors });
                }

                var folder = await _documentFolderService.CreateFolderAsync(model, user);
                
                _logger.LogInformation("CreateFolderAjax success: Folder {Name} created with ID {Id}", folder.Name, folder.Id);
                
                return Json(new { 
                    success = true, 
                    message = $"Pasta '{folder.Name}' criada com sucesso!",
                    folderId = folder.Id,
                    folderName = folder.Name
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Você não tem permissão para criar pastas." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta via AJAX");
                return Json(new { success = false, message = "Erro interno do servidor ao criar pasta." });
            }
        }

        /// <summary>
        /// Debug: Listar todas as pastas do usuário atual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DebugFolders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");
            var isManager = userRoles.Contains("Gestor");

            var folders = await _context.DocumentFolders
                .Include(f => f.Department)
                .Where(f => f.IsActive)
                .ToListAsync();

            var result = new
            {
                User = new { user.Id, user.UserName, user.DepartmentId },
                Roles = userRoles,
                IsAdmin = isAdmin,
                IsManager = isManager,
                TotalFolders = folders.Count,
                Folders = folders.Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.ParentFolderId,
                    f.DepartmentId,
                    f.Level,
                    f.IsActive,
                    f.CreatedAt,
                    Department = f.Department?.Name
                }).ToList()
            };

            return Json(result);
        }

        /// <summary>
        /// Versão simplificada do Index para debug
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DebugIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Buscar pastas diretamente sem filtros complexos
            var folders = await _context.DocumentFolders
                .Include(f => f.Department)
                .Where(f => f.IsActive && f.ParentFolderId == null)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var documents = await _context.Documents
                .Include(d => d.Uploader)
                .Include(d => d.Department)
                .Where(d => d.FolderId == null)
                .OrderBy(d => d.OriginalFileName)
                .ToListAsync();

            var viewModel = new DocumentTreeViewModel
            {
                RootFolders = folders.Select(f => new DocumentFolderTreeNode 
                { 
                    Folder = f,
                    Documents = new List<Document>()
                }).ToList(),
                RootDocuments = documents,
                CanCreateFolders = true,
                CanUpload = true
            };

            ViewBag.Departments = await _documentService.GetDepartmentsForUserAsync(user);
            
            return View("Index", viewModel);
        }
    }
}
