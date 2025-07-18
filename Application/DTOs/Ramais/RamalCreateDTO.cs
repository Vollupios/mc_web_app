using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Ramais
{
    /// <summary>
    /// DTO para criar ramal
    /// </summary>
    public class RamalCreateDTO : BaseDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número do ramal é obrigatório")]
        [StringLength(20, ErrorMessage = "O ramal deve ter no máximo 20 caracteres")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        [StringLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres")]
        public string? Position { get; set; }

        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
