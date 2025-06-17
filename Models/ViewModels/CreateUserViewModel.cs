using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Selecione uma função")]
        [Display(Name = "Função")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Nome de usuário")]
        public string? UserName { get; set; }

        // Lista de departamentos e roles para dropdowns
        public List<Department> AvailableDepartments { get; set; } = new List<Department>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
