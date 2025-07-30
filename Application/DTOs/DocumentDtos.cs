using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Application.DTOs
{
    /// <summary>
    /// DTO para criação de documento
    /// </summary>
    public class DocumentCreateDTO : BaseDTO
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
        
        // Propriedades adicionais esperadas pelo código
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public string Version { get; set; } = string.Empty;
        public string? ContentText { get; set; }
        public byte[]? FileData { get; set; }
    }

    /// <summary>
    /// DTO para resposta de documento
    /// </summary>
    public class DocumentResponseDTO : BaseDTO
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileSizeFormatted => FormatBytes(FileSize);
        public string? Description { get; set; }
        public string ContentType { get; set; } = string.Empty;
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
    /// DTO para filtros de busca
    /// </summary>
    public class SearchFilterDTO
    {
        public string? Query { get; set; }
        public string? SearchTerm { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public int? DepartmentId { get; set; }
        public int? FolderId { get; set; }
        public string? FileType { get; set; }
        public DateTime? UploadDateFrom { get; set; }
        public DateTime? UploadDateTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? UploaderName { get; set; }
        public List<string> Tags { get; set; } = new();
        public DocumentStatus? Status { get; set; }
        public bool? IsPublic { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    /// <summary>
    /// DTO para criação de pasta
    /// </summary>
    public class FolderCreateDTO : BaseDTO
    {
        [Required(ErrorMessage = "O nome da pasta é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome da pasta deve ter no máximo 255 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public int? ParentFolderId { get; set; }
        public bool IsPublic { get; set; }
        
        // Propriedades adicionais esperadas pelo código
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystemFolder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}