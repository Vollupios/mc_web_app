using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Models.ViewModels
{
    public class WorkflowActionViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public DocumentStatus CurrentStatus { get; set; }
        public WorkflowAction Action { get; set; }
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public string? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class DocumentWorkflowViewModel
    {
        // Dados do documento
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedByName { get; set; }
        public string? Description { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime? DueDate { get; set; }
        
        // Informações do documento
        public DocumentDetailViewModel Document { get; set; } = new();
        
        // Ações disponíveis para o usuário atual
        public List<WorkflowAction> AvailableActions { get; set; } = new();
        
        // Permissões do usuário atual
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanRequestChanges { get; set; }
        public bool CanAssignReviewer { get; set; }
        
        // Histórico do workflow
        public List<DocumentWorkflow> WorkflowHistory { get; set; } = new();
    }

    public class DocumentDetailViewModel
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public DocumentStatus Status { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedByName { get; set; }
        public string? Description { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class WorkflowDashboardViewModel
    {
        // Documentos por status
        public List<DocumentWorkflowViewModel> PendingDocuments { get; set; } = new();
        public List<DocumentWorkflowViewModel> InReviewDocuments { get; set; } = new();
        public List<DocumentWorkflowViewModel> ApprovedDocuments { get; set; } = new();
        public List<DocumentWorkflowViewModel> ArchivedDocuments { get; set; } = new();
        
        // Estatísticas
        public WorkflowStatistics Statistics { get; set; } = new();
    }

    public class WorkflowStatistics
    {
        public int TotalPending { get; set; }
        public int TotalInReview { get; set; }
        public int TotalPendingApproval { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int MyPendingDocuments { get; set; }
        public int OverdueDocuments { get; set; }
        
        // Tempo médio de aprovação (em dias)
        public double AverageApprovalTime { get; set; }
    }

    public class DocumentHistoryViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public List<DocumentHistoryItemViewModel> HistoryItems { get; set; } = new();
    }

    public class DocumentHistoryItemViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Comments { get; set; }
        public string ActionIcon { get; set; } = string.Empty;
        public string ActionColor { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
    }

    public class WorkflowConfigurationViewModel
    {
        // Configurações gerais
        public bool AutoApprovalEnabled { get; set; }
        public int DefaultApprovalTimeout { get; set; } = 7;
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool RequireCommentsOnRejection { get; set; } = true;
        public bool RequireSequentialApproval { get; set; }
        
        // Aprovadores e revisores
        public List<DocumentApprover> Approvers { get; set; } = new();
        public List<DocumentReviewer> Reviewers { get; set; } = new();
    }

    public class DepartmentWorkflowConfig
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<UserRoleConfig> Reviewers { get; set; } = new();
        public List<UserRoleConfig> Approvers { get; set; } = new();
    }

    public class UserRoleConfig
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }

    public class BulkWorkflowActionViewModel
    {
        public List<int> DocumentIds { get; set; } = new();
        public WorkflowAction Action { get; set; }
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public string? AssignedToUserId { get; set; }
    }
}
