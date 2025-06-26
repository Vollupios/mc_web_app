using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services;

namespace IntranetDocumentos.Controllers
{
    [Authorize]
    public class WorkflowController : Controller
    {
        private readonly IWorkflowService _workflowService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IWorkflowService workflowService,
            UserManager<ApplicationUser> userManager,
            ILogger<WorkflowController> logger)
        {
            _workflowService = workflowService;
            _userManager = userManager;
            _logger = logger;
        }

        // Dashboard principal do workflow
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var dashboard = await _workflowService.GetWorkflowDashboardAsync(userId);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard do workflow");
                TempData["ErrorMessage"] = "Erro ao carregar o dashboard. Tente novamente.";
                return RedirectToAction("Index", "Documents");
            }
        }

        // Visualizar workflow de um documento específico
        public async Task<IActionResult> Document(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var workflow = await _workflowService.GetDocumentWorkflowAsync(id, userId);
                
                if (workflow == null)
                {
                    TempData["ErrorMessage"] = "Documento não encontrado ou você não tem permissão para visualizá-lo.";
                    return RedirectToAction("Index");
                }

                return View(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar workflow do documento {DocumentId}", id);
                TempData["ErrorMessage"] = "Erro ao carregar o workflow do documento.";
                return RedirectToAction("Index");
            }
        }

        // Executar ação de workflow
        [HttpPost]
        public async Task<IActionResult> ExecuteAction(int documentId, WorkflowAction action, string? comments = null, string? assignedToUserId = null, DateTime? dueDate = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Verificar permissões
                if (!await _workflowService.CanUserPerformActionAsync(userId, documentId, action))
                {
                    TempData["ErrorMessage"] = "Você não tem permissão para executar esta ação.";
                    return RedirectToAction("Document", new { id = documentId });
                }

                // Executar ação
                var success = await _workflowService.ExecuteWorkflowActionAsync(documentId, action, userId, comments, assignedToUserId, dueDate);
                
                if (success)
                {
                    TempData["SuccessMessage"] = GetActionSuccessMessage(action);
                }
                else
                {
                    TempData["ErrorMessage"] = "Não foi possível executar a ação. Tente novamente.";
                }

                return RedirectToAction("Document", new { id = documentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar ação {Action} no documento {DocumentId}", action, documentId);
                TempData["ErrorMessage"] = "Erro ao executar a ação. Tente novamente.";
                return RedirectToAction("Document", new { id = documentId });
            }
        }

        // Ações em lote
        [HttpPost]
        public async Task<IActionResult> BulkAction(List<int> documentIds, WorkflowAction action, string? comments = null)
        {
            try
            {
                if (documentIds?.Any() != true)
                {
                    TempData["ErrorMessage"] = "Selecione pelo menos um documento.";
                    return RedirectToAction("Index");
                }

                var userId = _userManager.GetUserId(User);
                var success = await _workflowService.BulkExecuteWorkflowActionAsync(documentIds, action, userId, comments);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Ação '{GetActionDisplayName(action)}' executada em {documentIds.Count} documento(s).";
                }
                else
                {
                    TempData["ErrorMessage"] = "Não foi possível executar a ação em todos os documentos selecionados.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar ação em lote {Action}", action);
                TempData["ErrorMessage"] = "Erro ao executar ação em lote. Tente novamente.";
                return RedirectToAction("Index");
            }
        }

        // Histórico de um documento
        public async Task<IActionResult> History(int id)
        {
            try
            {
                var history = await _workflowService.GetDocumentHistoryAsync(id);
                ViewBag.DocumentId = id;
                return View(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar histórico do documento {DocumentId}", id);
                TempData["ErrorMessage"] = "Erro ao carregar o histórico do documento.";
                return RedirectToAction("Index");
            }
        }

        // Configuração do workflow (apenas Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Configuration()
        {
            try
            {
                var config = await _workflowService.GetWorkflowConfigurationAsync();
                return View(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar configuração do workflow");
                TempData["ErrorMessage"] = "Erro ao carregar a configuração do workflow.";
                return RedirectToAction("Index");
            }
        }

        // Salvar configuração do workflow (apenas Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Configuration(WorkflowConfigurationViewModel config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(config);
                }

                var success = await _workflowService.UpdateWorkflowConfigurationAsync(config);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Configuração do workflow atualizada com sucesso.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao atualizar a configuração do workflow.";
                }

                return RedirectToAction("Configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar configuração do workflow");
                TempData["ErrorMessage"] = "Erro ao salvar a configuração. Tente novamente.";
                return View(config);
            }
        }

        // Métodos auxiliares
        private static string GetActionSuccessMessage(WorkflowAction action)
        {
            return action switch
            {
                WorkflowAction.Submit => "Documento enviado para revisão com sucesso.",
                WorkflowAction.Approve => "Documento aprovado com sucesso.",
                WorkflowAction.Reject => "Documento rejeitado com sucesso.",
                WorkflowAction.RequestChanges => "Solicitação de alterações enviada com sucesso.",
                WorkflowAction.Publish => "Documento publicado com sucesso.",
                WorkflowAction.Archive => "Documento arquivado com sucesso.",
                WorkflowAction.Reactivate => "Documento reativado com sucesso.",
                WorkflowAction.AssignReviewer => "Revisor atribuído com sucesso.",
                WorkflowAction.StartReview => "Revisão iniciada com sucesso.",
                WorkflowAction.CompleteReview => "Revisão concluída com sucesso.",
                _ => "Ação executada com sucesso."
            };
        }

        private static string GetActionDisplayName(WorkflowAction action)
        {
            return action switch
            {
                WorkflowAction.Submit => "Enviar para Revisão",
                WorkflowAction.Approve => "Aprovar",
                WorkflowAction.Reject => "Rejeitar",
                WorkflowAction.RequestChanges => "Solicitar Alterações",
                WorkflowAction.Publish => "Publicar",
                WorkflowAction.Archive => "Arquivar",
                WorkflowAction.Reactivate => "Reativar",
                WorkflowAction.AssignReviewer => "Atribuir Revisor",
                WorkflowAction.StartReview => "Iniciar Revisão",
                WorkflowAction.CompleteReview => "Concluir Revisão",
                _ => action.ToString()
            };
        }

        // Método para administradores visualizarem todos os workflows
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> AllDocuments()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Para admin/gestor, mostrar todos os documentos do sistema
                var dashboard = await _workflowService.GetWorkflowDashboardAsync(userId);
                
                ViewData["Title"] = "Todos os Workflows";
                ViewData["ShowAllDocuments"] = true;
                
                return View("Index", dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar todos os workflows");
                TempData["ErrorMessage"] = "Erro ao carregar os workflows. Tente novamente.";
                return RedirectToAction("Index");
            }
        }
    }
}
