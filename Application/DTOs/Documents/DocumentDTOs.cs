using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Documents
{
    /// <summary>
    /// DTO para upload de documento
    /// </summary>
    public class CreateDocumentDTO
    {
        [Required(ErrorMessage = "O arquivo é obrigatório")]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public int? FolderId { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// DTO para atualização de documento
    /// </summary>
    public class UpdateDocumentDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do arquivo é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome do arquivo deve ter no máximo 255 caracteres")]
        public string OriginalFileName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public int? FolderId { get; set; }
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de documento
    /// </summary>
    public class DocumentDTO : BaseDTO
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileSizeFormatted => FormatBytes(FileSize);
        public string? Description { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new();
        
        // Relacionamentos
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? FolderId { get; set; }
        public string? FolderName { get; set; }
        public string UploaderId { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        
        // Metadados
        public DateTime UploadDate { get; set; }
        public DateTime? LastModified { get; set; }
        public int DownloadCount { get; set; }
        public DateTime? LastDownload { get; set; }
        public int Version { get; set; }
        public DocumentStatus Status { get; set; }
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// DTO para download de documento
    /// </summary>
    public class DocumentDownloadDTO
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public long FileSize { get; set; }
    }

    /// <summary>
    /// DTO para mover documento
    /// </summary>
    public class MoveDocumentDTO
    {
        [Required]
        public int DocumentId { get; set; }

        public int? NewFolderId { get; set; }

        [Required]
        public int NewDepartmentId { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para movimentação em lote
    /// </summary>
    public class BulkMoveDocumentDTO
    {
        [Required]
        public List<int> DocumentIds { get; set; } = new();

        public int? NewFolderId { get; set; }

        [Required]
        public int NewDepartmentId { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para filtros de busca
    /// </summary>
    public class DocumentSearchDTO
    {
        public string? Query { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int? DepartmentId { get; set; }
        public int? FolderId { get; set; }
        public string? FileType { get; set; }
        public DateTime? UploadDateFrom { get; set; }
        public DateTime? UploadDateTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UploaderName { get; set; }
        public List<string> Tags { get; set; } = new();
        public DocumentStatus? Status { get; set; }
        public bool? IsPublic { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "UploadDate";
        public string? SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// DTO para estatísticas de documentos
    /// </summary>
    public class DocumentStatisticsDTO
    {
        public int TotalDocuments { get; set; }
        public int DocumentsThisMonth { get; set; }
        public int TotalDownloads { get; set; }
        public int DownloadsThisMonth { get; set; }
        public long TotalStorageUsed { get; set; }
        public string TotalStorageFormatted => FormatBytes(TotalStorageUsed);
        
        public List<DocumentsByDepartmentDTO> DocumentsByDepartment { get; set; } = new();
        public List<DocumentTypeStatDTO> DocumentTypeStats { get; set; } = new();
        public List<TopDownloadedDocumentDTO> TopDownloadedDocuments { get; set; } = new();
        public List<MonthlyDocumentStatDTO> MonthlyStats { get; set; } = new();
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// DTO para documentos por departamento
    /// </summary>
    public class DocumentsByDepartmentDTO
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public int DownloadCount { get; set; }
        public long StorageUsed { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas por tipo de documento
    /// </summary>
    public class DocumentTypeStatDTO
    {
        public string FileType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public long TotalSize { get; set; }
    }

    /// <summary>
    /// DTO para documentos mais baixados
    /// </summary>
    public class TopDownloadedDocumentDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int DownloadCount { get; set; }
        public DateTime LastDownload { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas mensais
    /// </summary>
    public class MonthlyDocumentStatDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
        public long StorageUsed { get; set; }
    }

    /// <summary>
    /// Enum para status do documento
    /// </summary>
    public enum DocumentStatus
    {
        Active = 1,
        Archived = 2,
        Deleted = 3,
        Pending = 4,
        InReview = 5,
        Approved = 6,
        Rejected = 7
    }
}
