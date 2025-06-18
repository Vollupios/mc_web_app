using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel para criação e edição de ramais
    /// </summary>
    public class RamalViewModel
    {
        public int Id { get; set; }        [Display(Name = "Número do Ramal")]
        [StringLength(10, ErrorMessage = "O número do ramal deve ter no máximo 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de funcionário é obrigatório")]
        [Display(Name = "Tipo de Funcionário")]
        public TipoFuncionario TipoFuncionario { get; set; } = TipoFuncionario.Normal;

        [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Foto")]
        public IFormFile? FotoFile { get; set; }

        public string? FotoPath { get; set; }

        public List<Department> AvailableDepartments { get; set; } = new();
    }
}
