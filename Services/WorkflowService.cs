using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using IntranetDocumentos.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IntranetDocumentos.Services
{
    public interface IWorkflowService
    {
        Task<bool> CanUserPerformActionAsync(string userId, int documentId, WorkflowAction action);
        Task<DocumentWorkflowViewModel> GetDocumentWorkflowAsync(int documentId, string userId);
        Task<WorkflowDashboardViewModel> GetWorkflowDashboardAsync(string userId);
        Task<bool> ExecuteWorkflowActionAsync(int documentId, WorkflowAction action, string userId, string? comments = null, string? assignedToUserId = null, DateTime? dueDate = null);
        Task<List<DocumentHistoryItemViewModel>> GetDocumentHistoryAsync(int documentId);
        Task<bool> BulkExecuteWorkflowActionAsync(List<int> documentIds, WorkflowAction action, string userId, string? comments = null);
        Task<WorkflowConfigurationViewModel> GetWorkflowConfigurationAsync();
        Task<bool> UpdateWorkflowConfigurationAsync(WorkflowConfigurationViewModel config);
        Task LogDocumentActionAsync(int documentId, string userId, string action, string? description = null, string? oldValue = null, string? newValue = null);
        Task<(bool Success, int Count, string? ErrorMessage)> ApproveAllPendingDocumentsAsync(string userId);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkflowService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public WorkflowService(ApplicationDbContext context, ILogger<WorkflowService> logger, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // Helper method to transition document status
        private async Task TransitionDocumentStatusAsync(Document document, DocumentStatus newStatus, string userId, string? comment = null)
        {
            var oldStatus = document.Status;
            document.Status = newStatus;
            document.LastModified = DateTime.UtcNow;

            // Add to workflow history
            var workflowAction = newStatus switch
            {
                DocumentStatus.PendingReview => WorkflowAction.Submit,
                DocumentStatus.InReview => WorkflowAction.StartReview,
                DocumentStatus.PendingApproval => WorkflowAction.CompleteReview,
                DocumentStatus.Approved => WorkflowAction.Approve,
                DocumentStatus.Rejected => WorkflowAction.Reject,
                DocumentStatus.ChangesRequested => WorkflowAction.RequestChanges,
                DocumentStatus.Archived => WorkflowAction.Archive,
                DocumentStatus.Published => WorkflowAction.Publish,
                _ => WorkflowAction.Submit
            };

            var history = new DocumentHistory
            {
                DocumentId = document.Id,
                Action = workflowAction.ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Description = comment,
                OldValue = oldStatus.ToString(),
                NewValue = newStatus.ToString()
            };

            _context.DocumentHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        // Method to check if user can perform action on document
        public async Task<bool> CanUserPerformActionAsync(string userId, int documentId, WorkflowAction action)
        {
            var user = await _context.Users.FindAsync(userId);
            var document = await _context.Documents.FindAsync(documentId);
            
            if (user == null || document == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            
            return action switch
            {
                WorkflowAction.Submit => document.UploaderId == userId,
                WorkflowAction.StartReview => userRoles.Contains("Gestor") || userRoles.Contains("Admin"),
                WorkflowAction.CompleteReview => userRoles.Contains("Gestor") || userRoles.Contains("Admin"),
                WorkflowAction.Approve => userRoles.Contains("Admin") || (userRoles.Contains("Gestor") && document.Department?.Name != "TI"),
                WorkflowAction.Reject => userRoles.Contains("Gestor") || userRoles.Contains("Admin"),
                WorkflowAction.RequestChanges => userRoles.Contains("Gestor") || userRoles.Contains("Admin"),
                WorkflowAction.Archive => userRoles.Contains("Admin"),
                WorkflowAction.Publish => userRoles.Contains("Admin"),
                _ => false
            };
        }

        // Method to get available actions for a user on a document
        public async Task<List<WorkflowAction>> GetAvailableActionsAsync(string userId, int documentId)
        {
            var actions = new List<WorkflowAction>();
            var document = await _context.Documents.FindAsync(documentId);
            
            if (document == null) return actions;

            var allActions = Enum.GetValues<WorkflowAction>();
            
            foreach (var action in allActions)
            {
                if (await CanUserPerformActionAsync(userId, documentId, action))
                {
                    // Check if action is valid for current document status
                    var isValid = (document.Status, action) switch
                    {
                        (DocumentStatus.Draft, WorkflowAction.Submit) => true,
                        (DocumentStatus.PendingReview, WorkflowAction.StartReview) => true,
                        (DocumentStatus.PendingReview, WorkflowAction.Reject) => true,
                        (DocumentStatus.InReview, WorkflowAction.CompleteReview) => true,
                        (DocumentStatus.InReview, WorkflowAction.RequestChanges) => true,
                        (DocumentStatus.PendingApproval, WorkflowAction.Approve) => true,
                        (DocumentStatus.PendingApproval, WorkflowAction.Reject) => true,
                        (DocumentStatus.Approved, WorkflowAction.Archive) => true,
                        (DocumentStatus.Approved, WorkflowAction.Publish) => true,
                        _ => false
                    };
                    
                    if (isValid)
                    {
                        actions.Add(action);
                    }
                }
            }

            return actions;
        }

        // Method to send notifications for workflow events
        private async Task SendWorkflowNotificationAsync(Document document, WorkflowAction action, string performedByUserId, string? comment = null)
        {
            try
            {
                var performer = await _userManager.FindByIdAsync(performedByUserId);
                var performerName = performer?.UserName ?? "Usuário desconhecido";

                var actionText = action switch
                {
                    WorkflowAction.Submit => "submetido para revisão",
                    WorkflowAction.StartReview => "iniciado revisão",
                    WorkflowAction.CompleteReview => "revisão concluída",
                    WorkflowAction.Approve => "aprovado",
                    WorkflowAction.Reject => "rejeitado",
                    WorkflowAction.RequestChanges => "solicitado alterações",
                    WorkflowAction.Archive => "arquivado",
                    WorkflowAction.Publish => "publicado",
                    _ => "ação desconhecida"
                };

                var notificationTitle = $"Documento {actionText}";
                var notificationMessage = $"O documento '{document.OriginalFileName}' foi {actionText} por {performerName}.";
                
                if (!string.IsNullOrEmpty(comment))
                {
                    notificationMessage += $"\n\nComentário: {comment}";
                }

                // Log notification for now (can be extended to email later)
                _logger.LogInformation("Workflow Notification: {Title} - {Message}", notificationTitle, notificationMessage);

                // In the future, you can use the notification service to send emails:
                // await _notificationService.SendNotificationAsync(...);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de workflow");
            }
        }

        public async Task<DocumentWorkflowViewModel> GetDocumentWorkflowAsync(int documentId, string userId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Include(d => d.LastModifiedBy)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null) return null;

                var viewModel = new DocumentWorkflowViewModel
                {
                    Id = document.Id,
                    OriginalFileName = document.OriginalFileName,
                    UploaderName = document.Uploader?.UserName ?? "Desconhecido",
                    UploadDate = document.UploadDate,
                    Status = document.Status,
                    DepartmentName = document.Department?.Name ?? "Geral",
                    Version = document.Version,
                    LastModified = document.LastModified,
                    LastModifiedByName = document.LastModifiedBy?.UserName,
                    Description = document.Description,
                    Document = new DocumentDetailViewModel
                    {
                        Id = document.Id,
                        OriginalFileName = document.OriginalFileName,
                        UploaderName = document.Uploader?.UserName ?? "Desconhecido",
                        UploadDate = document.UploadDate,
                        Status = document.Status,
                        DepartmentName = document.Department?.Name ?? "Geral",
                        Version = document.Version,
                        LastModified = document.LastModified,
                        LastModifiedByName = document.LastModifiedBy?.UserName,
                        Description = document.Description
                    }
                };

                // Verificar permissões
                viewModel.CanApprove = await CanUserPerformActionAsync(userId, documentId, WorkflowAction.Approve);
                viewModel.CanReject = await CanUserPerformActionAsync(userId, documentId, WorkflowAction.Reject);

                // Ações disponíveis
                var actions = new List<WorkflowAction>();
                if (await CanUserPerformActionAsync(userId, documentId, WorkflowAction.Submit))
                    actions.Add(WorkflowAction.Submit);
                if (await CanUserPerformActionAsync(userId, documentId, WorkflowAction.Approve))
                    actions.Add(WorkflowAction.Approve);
                if (await CanUserPerformActionAsync(userId, documentId, WorkflowAction.Reject))
                    actions.Add(WorkflowAction.Reject);

                viewModel.AvailableActions = actions;

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter workflow");
                return null;
            }
        }

        public async Task<WorkflowDashboardViewModel> GetWorkflowDashboardAsync(string userId)
        {
            try
            {
                var pendingDocs = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.Status == DocumentStatus.PendingApproval)
                    .Take(10)
                    .ToListAsync();

                var inReviewDocs = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.Status == DocumentStatus.InReview)
                    .Take(10)
                    .ToListAsync();

                var approvedDocs = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.Status == DocumentStatus.Approved)
                    .Take(10)
                    .ToListAsync();

                return new WorkflowDashboardViewModel
                {
                    PendingDocuments = pendingDocs.Select(MapToViewModel).ToList(),
                    InReviewDocuments = inReviewDocs.Select(MapToViewModel).ToList(),
                    ApprovedDocuments = approvedDocs.Select(MapToViewModel).ToList(),
                    ArchivedDocuments = new List<DocumentWorkflowViewModel>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dashboard");
                return new WorkflowDashboardViewModel();
            }
        }

        public async Task<bool> ExecuteWorkflowActionAsync(int documentId, WorkflowAction action, string userId, string? comments = null, string? assignedToUserId = null, DateTime? dueDate = null)
        {
            try
            {
                if (!await CanUserPerformActionAsync(userId, documentId, action))
                    return false;

                var document = await _context.Documents.FindAsync(documentId);
                if (document == null) return false;

                // Transition document status
                var newStatus = action switch
                {
                    WorkflowAction.Submit => DocumentStatus.PendingReview,
                    WorkflowAction.StartReview => DocumentStatus.InReview,
                    WorkflowAction.CompleteReview => DocumentStatus.PendingApproval,
                    WorkflowAction.Approve => DocumentStatus.Approved,
                    WorkflowAction.Reject => DocumentStatus.Rejected,
                    WorkflowAction.RequestChanges => DocumentStatus.ChangesRequested,
                    WorkflowAction.Archive => DocumentStatus.Archived,
                    WorkflowAction.Publish => DocumentStatus.Published,
                    _ => document.Status
                };

                await TransitionDocumentStatusAsync(document, newStatus, userId, comments);

                // Send notification
                await SendWorkflowNotificationAsync(document, action, userId, comments);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar ação");
                return false;
            }
        }

        public async Task<List<DocumentHistoryItemViewModel>> GetDocumentHistoryAsync(int documentId)
        {
            try
            {
                var history = await _context.DocumentHistories
                    .Include(h => h.User)
                    .Where(h => h.DocumentId == documentId)
                    .OrderByDescending(h => h.CreatedAt)
                    .ToListAsync();

                return history.Select(h => new DocumentHistoryItemViewModel
                {
                    UserName = h.User?.UserName ?? "Desconhecido",
                    Action = h.Action,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    IpAddress = h.IpAddress
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter histórico");
                return new List<DocumentHistoryItemViewModel>();
            }
        }

        public async Task<bool> BulkExecuteWorkflowActionAsync(List<int> documentIds, WorkflowAction action, string userId, string? comments = null)
        {
            try
            {
                var success = 0;
                foreach (var id in documentIds)
                {
                    if (await ExecuteWorkflowActionAsync(id, action, userId, comments))
                        success++;
                }
                return success > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em ação em lote");
                return false;
            }
        }

        public async Task<WorkflowConfigurationViewModel> GetWorkflowConfigurationAsync()
        {
            try
            {
                await Task.CompletedTask; // Remove async warning
                return new WorkflowConfigurationViewModel
                {
                    DefaultApprovalTimeout = 7,
                    AutoApprovalEnabled = false,
                    EmailNotificationsEnabled = true,
                    RequireCommentsOnRejection = true,
                    Approvers = new List<DocumentApprover>(),
                    Reviewers = new List<DocumentReviewer>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter configuração");
                return new WorkflowConfigurationViewModel();
            }
        }

        public async Task<bool> UpdateWorkflowConfigurationAsync(WorkflowConfigurationViewModel config)
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar configuração");
                return false;
            }
        }

        public async Task LogDocumentActionAsync(int documentId, string userId, string action, string? description = null, string? oldValue = null, string? newValue = null)
        {
            try
            {
                var history = new DocumentHistory
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = action,
                    Description = description,
                    OldValue = oldValue,
                    NewValue = newValue,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DocumentHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar histórico");
            }
        }

        private DocumentWorkflowViewModel MapToViewModel(Document doc)
        {
            return new DocumentWorkflowViewModel
            {
                Id = doc.Id,
                OriginalFileName = doc.OriginalFileName,
                UploaderName = doc.Uploader?.UserName ?? "Desconhecido",
                UploadDate = doc.UploadDate,
                Status = doc.Status,
                DepartmentName = doc.Department?.Name ?? "Geral",
                Version = doc.Version,
                LastModified = doc.LastModified,
                LastModifiedByName = doc.LastModifiedBy?.UserName
            };
        }

        public async Task<(bool Success, int Count, string? ErrorMessage)> ApproveAllPendingDocumentsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, 0, "Usuário não encontrado.");
                }

                // Buscar todos os documentos pendentes que o usuário pode aprovar
                var pendingDocuments = await _context.Documents
                    .Include(d => d.Department)
                    .Include(d => d.Uploader)
                    .Where(d => d.Status == DocumentStatus.PendingApproval)
                    .ToListAsync();

                if (!pendingDocuments.Any())
                {
                    return (false, 0, "Não há documentos pendentes para aprovação.");
                }

                // Filtrar documentos que o usuário pode realmente aprovar
                var approvableDocuments = new List<Document>();
                foreach (var doc in pendingDocuments)
                {
                    if (await CanUserPerformActionAsync(userId, doc.Id, WorkflowAction.Approve))
                    {
                        approvableDocuments.Add(doc);
                    }
                }

                if (!approvableDocuments.Any())
                {
                    return (false, 0, "Você não tem permissão para aprovar os documentos pendentes.");
                }

                var approvedCount = 0;
                foreach (var document in approvableDocuments)
                {
                    try
                    {
                        await TransitionDocumentStatusAsync(document, DocumentStatus.Approved, userId, "Aprovado em massa");
                        
                        // Log da ação
                        await LogDocumentActionAsync(document.Id, userId, "Aprovação em massa", 
                            "Documento aprovado através da função 'Aprovar Todos'");

                        approvedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao aprovar documento {DocumentId} em aprovação em massa", document.Id);
                        // Continue com os outros documentos
                    }
                }

                await _context.SaveChangesAsync();

                if (approvedCount == 0)
                {
                    return (false, 0, "Não foi possível aprovar nenhum documento.");
                }

                return (true, approvedCount, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar aprovação em massa");
                return (false, 0, "Erro interno do sistema.");
            }
        }
    }
}
