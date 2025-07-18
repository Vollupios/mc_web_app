using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Workflow
{
    /// <summary>
    /// DTO para ação de workflow
    /// </summary>
    public class WorkflowActionDTO
    {
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public WorkflowActionType Action { get; set; }
        
        [StringLength(1000, ErrorMessage = "Os comentários devem ter no máximo 1000 caracteres")]
        public string? Comments { get; set; }
        
        public string? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 1;
    }

    /// <summary>
    /// DTO para resposta de workflow do documento
    /// </summary>
    public class DocumentWorkflowDTO : BaseDTO
    {
        // Dados do documento
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public DocumentWorkflowStatus Status { get; set; }
        public string StatusDescription => Status.GetDescription();
        public string DepartmentName { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedByName { get; set; }
        public string? Description { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        
        // Ações disponíveis para o usuário atual
        public List<WorkflowActionType> AvailableActions { get; set; } = new();
        
        // Permissões do usuário atual
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanRequestChanges { get; set; }
        public bool CanAssignReviewer { get; set; }
        public bool CanArchive { get; set; }
        public bool CanDelete { get; set; }
        
        // Histórico do workflow
        public List<WorkflowHistoryDTO> WorkflowHistory { get; set; } = new();
        
        // Informações calculadas
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != DocumentWorkflowStatus.Approved && Status != DocumentWorkflowStatus.Archived;
        public TimeSpan? TimeToDeadline => DueDate.HasValue ? DueDate.Value - DateTime.Now : null;
        public int DaysInCurrentStatus => (DateTime.Now - LastModified).Days;
    }

    /// <summary>
    /// DTO para histórico de workflow
    /// </summary>
    public class WorkflowHistoryDTO : BaseDTO
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public WorkflowActionType Action { get; set; }
        public string ActionDescription => Action.GetDescription();
        public DocumentWorkflowStatus FromStatus { get; set; }
        public DocumentWorkflowStatus ToStatus { get; set; }
        public string? Comments { get; set; }
        public DateTime ActionDate { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Informações calculadas
        public string ActionIcon => GetActionIcon();
        public string ActionColor => GetActionColor();
        public bool IsSystemAction => string.IsNullOrEmpty(UserId);
        
        private string GetActionIcon()
        {
            return Action switch
            {
                WorkflowActionType.Submit => "bi-upload",
                WorkflowActionType.Approve => "bi-check-circle",
                WorkflowActionType.Reject => "bi-x-circle",
                WorkflowActionType.RequestChanges => "bi-arrow-clockwise",
                WorkflowActionType.AssignReviewer => "bi-person-plus",
                WorkflowActionType.Archive => "bi-archive",
                WorkflowActionType.Delete => "bi-trash",
                WorkflowActionType.Restore => "bi-arrow-counterclockwise",
                _ => "bi-info-circle"
            };
        }
        
        private string GetActionColor()
        {
            return Action switch
            {
                WorkflowActionType.Submit => "primary",
                WorkflowActionType.Approve => "success",
                WorkflowActionType.Reject => "danger",
                WorkflowActionType.RequestChanges => "warning",
                WorkflowActionType.AssignReviewer => "info",
                WorkflowActionType.Archive => "secondary",
                WorkflowActionType.Delete => "danger",
                WorkflowActionType.Restore => "success",
                _ => "secondary"
            };
        }
    }

    /// <summary>
    /// DTO para dashboard de workflow
    /// </summary>
    public class WorkflowDashboardDTO
    {
        // Documentos por status
        public List<DocumentWorkflowDTO> PendingDocuments { get; set; } = new();
        public List<DocumentWorkflowDTO> InReviewDocuments { get; set; } = new();
        public List<DocumentWorkflowDTO> ApprovedDocuments { get; set; } = new();
        public List<DocumentWorkflowDTO> ArchivedDocuments { get; set; } = new();
        public List<DocumentWorkflowDTO> OverdueDocuments { get; set; } = new();
        public List<DocumentWorkflowDTO> MyAssignedDocuments { get; set; } = new();
        
        // Estatísticas
        public WorkflowStatisticsDTO Statistics { get; set; } = new();
        
        // Filtros aplicados
        public WorkflowFilterDTO Filters { get; set; } = new();
    }

    /// <summary>
    /// DTO para estatísticas de workflow
    /// </summary>
    public class WorkflowStatisticsDTO
    {
        public int TotalDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int InReviewDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
        public int ArchivedDocuments { get; set; }
        public int OverdueDocuments { get; set; }
        public int MyAssignedDocuments { get; set; }
        
        public double AverageProcessingTime { get; set; } // em horas
        public double AverageApprovalTime { get; set; } // em horas
        public double ApprovalRate { get; set; } // percentual
        public double RejectionRate { get; set; } // percentual
        
        public List<WorkflowByStatusDTO> DocumentsByStatus { get; set; } = new();
        public List<WorkflowByDepartmentDTO> DocumentsByDepartment { get; set; } = new();
        public List<WorkflowByUserDTO> DocumentsByUser { get; set; } = new();
        public List<WorkflowByMonthDTO> DocumentsByMonth { get; set; } = new();
    }

    /// <summary>
    /// DTO para filtros de workflow
    /// </summary>
    public class WorkflowFilterDTO
    {
        public DocumentWorkflowStatus? Status { get; set; }
        public int? DepartmentId { get; set; }
        public string? AssignedToUserId { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int? Priority { get; set; }
        public bool? IsOverdue { get; set; }
        public string? Query { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "LastModified";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO para documentos por status
    /// </summary>
    public class WorkflowByStatusDTO
    {
        public DocumentWorkflowStatus Status { get; set; }
        public string StatusDescription => Status.GetDescription();
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para documentos por departamento
    /// </summary>
    public class WorkflowByDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO para documentos por usuário
    /// </summary>
    public class WorkflowByUserDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DocumentsCreated { get; set; }
        public int DocumentsApproved { get; set; }
        public int DocumentsRejected { get; set; }
        public int DocumentsAssigned { get; set; }
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO para documentos por mês
    /// </summary>
    public class WorkflowByMonthDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int DocumentsCreated { get; set; }
        public int DocumentsApproved { get; set; }
        public int DocumentsRejected { get; set; }
        public double AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO para ação em lote
    /// </summary>
    public class BulkWorkflowActionDTO
    {
        [Required]
        public List<int> DocumentIds { get; set; } = new();
        
        [Required]
        public WorkflowActionType Action { get; set; }
        
        [StringLength(1000, ErrorMessage = "Os comentários devem ter no máximo 1000 caracteres")]
        public string? Comments { get; set; }
        
        public string? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 1;
    }

    /// <summary>
    /// DTO para configuração de workflow
    /// </summary>
    public class WorkflowConfigurationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<WorkflowStepDTO> Steps { get; set; } = new();
        public List<WorkflowRuleDTO> Rules { get; set; } = new();
        public List<int> DepartmentIds { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
    }

    /// <summary>
    /// DTO para etapa de workflow
    /// </summary>
    public class WorkflowStepDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public List<string> RequiredRoles { get; set; } = new();
        public List<string> RequiredUsers { get; set; } = new();
        public int? MaxDurationHours { get; set; }
        public bool AllowSkip { get; set; }
        public bool AllowDelegate { get; set; }
    }

    /// <summary>
    /// DTO para regra de workflow
    /// </summary>
    public class WorkflowRuleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Enum para tipos de ação de workflow
    /// </summary>
    public enum WorkflowActionType
    {
        Submit = 1,
        Approve = 2,
        Reject = 3,
        RequestChanges = 4,
        AssignReviewer = 5,
        Archive = 6,
        Delete = 7,
        Restore = 8,
        Delegate = 9,
        Escalate = 10
    }

    /// <summary>
    /// Enum para status de workflow do documento
    /// </summary>
    public enum DocumentWorkflowStatus
    {
        Draft = 1,
        Submitted = 2,
        InReview = 3,
        Approved = 4,
        Rejected = 5,
        ChangesRequested = 6,
        Archived = 7,
        Deleted = 8
    }

    /// <summary>
    /// Extensões para os enums de workflow
    /// </summary>
    public static class WorkflowEnumExtensions
    {
        public static string GetDescription(this WorkflowActionType action)
        {
            return action switch
            {
                WorkflowActionType.Submit => "Submeter",
                WorkflowActionType.Approve => "Aprovar",
                WorkflowActionType.Reject => "Rejeitar",
                WorkflowActionType.RequestChanges => "Solicitar Alterações",
                WorkflowActionType.AssignReviewer => "Atribuir Revisor",
                WorkflowActionType.Archive => "Arquivar",
                WorkflowActionType.Delete => "Excluir",
                WorkflowActionType.Restore => "Restaurar",
                WorkflowActionType.Delegate => "Delegar",
                WorkflowActionType.Escalate => "Escalar",
                _ => "Desconhecido"
            };
        }

        public static string GetDescription(this DocumentWorkflowStatus status)
        {
            return status switch
            {
                DocumentWorkflowStatus.Draft => "Rascunho",
                DocumentWorkflowStatus.Submitted => "Submetido",
                DocumentWorkflowStatus.InReview => "Em Revisão",
                DocumentWorkflowStatus.Approved => "Aprovado",
                DocumentWorkflowStatus.Rejected => "Rejeitado",
                DocumentWorkflowStatus.ChangesRequested => "Alterações Solicitadas",
                DocumentWorkflowStatus.Archived => "Arquivado",
                DocumentWorkflowStatus.Deleted => "Excluído",
                _ => "Desconhecido"
            };
        }
    }
}
