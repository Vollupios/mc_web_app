using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Documents
{
    /// <summary>
    /// DTO para busca de pastas
    /// </summary>
    public class FolderSearchDTO : BaseDTO
    {
        public string? Query { get; set; }
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? ParentFolderId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPublic { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc";
    }
}
