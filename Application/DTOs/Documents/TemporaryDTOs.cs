using IntranetDocumentos.Application.DTOs.Common;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Application.DTOs.Documents
{
    /// <summary>
    /// DTO temporário para integração com Strategies e Factories
    /// </summary>
    public class DocumentCreateDTO : BaseDTO
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public int? FolderId { get; set; }
        public string? UploaderId { get; set; }
        public Models.DocumentStatus Status { get; set; }
        public string Version { get; set; } = "1.0";
        public string? ContentText { get; set; }
        public byte[]? FileData { get; set; }
    }

    /// <summary>
    /// DTO temporário para respostas de documentos
    /// </summary>
    public class DocumentResponseDTO : BaseDTO
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? FolderId { get; set; }
        public string? FolderName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO temporário para criação de pastas
    /// </summary>
    public class FolderCreateDTO : BaseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public int? ParentFolderId { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSystemFolder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO temporário para filtros de busca
    /// </summary>
    public class SearchFilterDTO : BaseDTO
    {
        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public int? FolderId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ContentType { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
