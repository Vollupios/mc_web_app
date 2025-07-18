using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Application.DTOs.Users
{
    /// <summary>
    /// DTO para criar usuário
    /// </summary>
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Selecione uma função")]
        public string Role { get; set; } = string.Empty;

        public string? UserName { get; set; }
    }

    /// <summary>
    /// DTO para atualizar usuário
    /// </summary>
    public class UpdateUserDTO
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Selecione uma função")]
        public string Role { get; set; } = string.Empty;

        public string? Password { get; set; }
        public string? UserName { get; set; }
    }

    /// <summary>
    /// DTO para resposta de usuário
    /// </summary>
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO para login
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// DTO para resposta de login
    /// </summary>
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDTO? User { get; set; }
        public string? Token { get; set; }
    }

    /// <summary>
    /// DTO para alteração de senha
    /// </summary>
    public class ChangePasswordDTO
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
