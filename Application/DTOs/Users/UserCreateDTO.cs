using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Users
{
    /// <summary>
    /// DTO para criar usuário
    /// </summary>
    public class UserCreateDTO : BaseDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O sobrenome é obrigatório")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Selecione uma função")]
        public string Role { get; set; } = string.Empty;

        public string? UserName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
