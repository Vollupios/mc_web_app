using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Models.ValueObjects;

namespace IntranetDocumentos.Models
{
    public enum DocumentStatus
    {
        Draft = 0,                 // Rascunho
        PendingReview = 1,         // Aguardando Revisão
        InReview = 2,              // Em Revisão
        PendingApproval = 3,       // Aguardando Aprovação
        Approved = 4,              // Aprovado
        Rejected = 5,              // Rejeitado
        ChangesRequested = 6,      // Alterações Solicitadas
        Published = 7,             // Publicado
        Archived = 8               // Arquivado
    }

    public enum WorkflowAction
    {
        Submit = 0,          // Enviar para revisão
        Approve = 1,         // Aprovar
        Reject = 2,          // Rejeitar
        RequestChanges = 3,  // Solicitar alterações
        Publish = 4,         // Publicar
        Archive = 5,         // Arquivar
        Reactivate = 6,      // Reativar
        AssignReviewer = 7,  // Atribuir Revisor
        StartReview = 8,     // Iniciar Revisão
        CompleteReview = 9   // Concluir Revisão
    }

    public class Document
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        [Required]
        public string UploaderId { get; set; } = string.Empty;
        
        // Se DepartmentId for nulo, o documento é do setor "Geral"
        public int? DepartmentId { get; set; }
        
        // Texto extraído via OCR para indexação e busca
        public string? ContentText { get; set; }
        
        // Workflow Status
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        
        // Descrição/Comentários do documento
        [StringLength(1000)]
        public string? Description { get; set; }
        
        // Versão do documento
        public int Version { get; set; } = 1;
        
        // Data da última modificação
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        // ID do usuário que fez a última modificação
        public string? LastModifiedById { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser Uploader { get; set; } = null!;
        public virtual Department? Department { get; set; }
        public virtual ApplicationUser? LastModifiedBy { get; set; }
        
        // Workflow relationships
        public virtual ICollection<DocumentWorkflow> Workflows { get; set; } = new List<DocumentWorkflow>();
        public virtual ICollection<DocumentHistory> History { get; set; } = new List<DocumentHistory>();
        
        // Métodos helper para trabalhar com FileSize Value Object
        public ValueObjects.FileSize GetFileSizeValueObject()
        {
            return ValueObjects.FileSize.FromBytes(FileSize);
        }
        
        public string GetFormattedFileSize(int decimalPlaces = 2)
        {
            return GetFileSizeValueObject().ToHumanReadableString(decimalPlaces);
        }
        
        public void SetFileSize(ValueObjects.FileSize fileSize)
        {
            FileSize = fileSize.Bytes;
        }
    }

    public class DocumentWorkflow
    {
        public int Id { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        public Document Document { get; set; }
        
        [Required]
        public DocumentStatus CurrentStatus { get; set; }
        
        public DocumentStatus? PreviousStatus { get; set; }
        
        [Required]
        public WorkflowAction Action { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? DueDate { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        
        public DateTime? CompletedAt { get; set; }
        
        // Metadados para auditoria
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Propriedades derivadas para facilitar o uso nas views
        public string UserName => User?.UserName ?? "Usuário não encontrado";
        public string? AssignedToUserName => AssignedToUser?.UserName;
    }

    public class DocumentHistory
    {
        public int Id { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        public Document Document { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class DocumentApprover
    {
        public int Id { get; set; }
        
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int Order { get; set; } = 1; // Ordem de aprovação
        
        public bool IsRequired { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DocumentReviewer
    {
        public int Id { get; set; }
        
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
