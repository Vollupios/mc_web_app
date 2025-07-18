using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Departments
{
    /// <summary>
    /// DTO para busca de departamentos
    /// </summary>
    public class DepartmentSearchDTO : BaseDTO
    {
        public string? Query { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "asc";
    }
}
