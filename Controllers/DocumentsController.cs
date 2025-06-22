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
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<DocumentsController> _logger;

        /// <summary>
        /// Controller para operações de documentos. Garante autenticação e logging.
        /// </summary>
        public DocumentsController(
            IDocumentService documentService,
            UserManager<ApplicationUser> userManager,
            IAnalyticsService analyticsService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _userManager = userManager;
            _analyticsService = analyticsService;
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
        /// Realiza o upload de um documento.
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
                    _logger.LogInformation("Iniciando upload de documento - Arquivo: {FileName}, Tamanho: {FileSize} bytes, Usuário: {UserId}, Departamento: {DepartmentId}", 
                        model.File.FileName, model.File.Length, user.Id, model.DepartmentId);
                    
                    await _documentService.SaveDocumentAsync(model.File, user, model.DepartmentId);
                    
                    _logger.LogInformation("Upload concluído com sucesso - Arquivo: {FileName}", model.File.FileName);
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

            var filePath = await _documentService.GetDocumentPhysicalPathAsync(document);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Arquivo não encontrado no servidor.");
            }

            // Registrar o download para analytics
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _analyticsService.RegisterDocumentDownloadAsync(id, user.Id, userAgent, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar download do documento {DocumentId}", id);
                // Não falha o download por causa do analytics
            }

            // Determinar o Content-Type correto baseado na extensão
            var extension = Path.GetExtension(document.OriginalFileName).ToLowerInvariant();
            var contentType = GetContentType(extension, document.ContentType);
            
            _logger.LogInformation("Download do documento - ID: {DocumentId}, Arquivo: {FileName}, ContentType: {ContentType}", 
                id, document.OriginalFileName, contentType);

            // Para documentos que podem ser visualizados no navegador, usar FileStreamResult para melhor performance
            if (CanBeDisplayedInBrowser(extension))
            {
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = document.OriginalFileName,
                    EnableRangeProcessing = true // Permite streaming parcial
                };
            }

            // Para outros arquivos, continuar com o método atual
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, contentType, document.OriginalFileName);
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

        /// <summary>
        /// Exibe os detalhes de um documento.
        /// </summary>
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

        /// <summary>
        /// Visualiza um documento inline no navegador (quando possível)
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

            // Determinar o Content-Type correto baseado na extensão
            var extension = Path.GetExtension(document.OriginalFileName).ToLowerInvariant();
            var contentType = GetContentType(extension, document.ContentType);
            
            _logger.LogInformation("Visualização do documento - ID: {DocumentId}, Arquivo: {FileName}, ContentType: {ContentType}", 
                id, document.OriginalFileName, contentType);

            // Registrar visualização para analytics (opcional)
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                await _analyticsService.RegisterDocumentDownloadAsync(id, user.Id, userAgent, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar visualização do documento {DocumentId}", id);
            }

            // Usar FileStreamResult para melhor performance e suporte a streaming
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, contentType)
            {
                EnableRangeProcessing = true // Permite streaming parcial para arquivos grandes
            };
        }

        #region Métodos Auxiliares para Download

        /// <summary>
        /// Determina o Content-Type correto baseado na extensão do arquivo
        /// </summary>
        private static string GetContentType(string extension, string originalContentType)
        {
            return extension switch
            {
                // Documentos PDF
                ".pdf" => "application/pdf",
                
                // Documentos Word
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                
                // Planilhas Excel
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                
                // Apresentações PowerPoint
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                
                // Imagens
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".webp" => "image/webp",
                
                // Texto
                ".txt" => "text/plain",
                ".rtf" => "application/rtf",
                ".csv" => "text/csv",
                
                // Arquivos compactados
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                
                // LibreOffice/OpenOffice
                ".odt" => "application/vnd.oasis.opendocument.text",
                ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
                ".odp" => "application/vnd.oasis.opendocument.presentation",
                
                // Padrão: usar o Content-Type original ou application/octet-stream
                _ => !string.IsNullOrEmpty(originalContentType) ? originalContentType : "application/octet-stream"
            };
        }

        /// <summary>
        /// Verifica se o arquivo pode ser exibido diretamente no navegador
        /// </summary>
        private static bool CanBeDisplayedInBrowser(string extension)
        {
            return extension switch
            {
                // PDFs podem ser exibidos diretamente na maioria dos navegadores
                ".pdf" => true,
                
                // Imagens podem ser exibidas diretamente
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => true,
                
                // Texto pode ser exibido diretamente
                ".txt" => true,
                
                // Outros tipos precisam ser baixados
                _ => false
            };
        }

        #endregion
    }
}
