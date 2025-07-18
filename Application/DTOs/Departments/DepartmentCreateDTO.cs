using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Departments
{
    /// <summary>
    /// DTO para criar departamento
    /// </summary>
    public class DepartmentCreateDTO : BaseDTO
    {
        [Required(ErrorMessage = "O nome do departamento é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
    }
}
