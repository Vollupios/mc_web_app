using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    public class UploadViewModel
    {
        [Required(ErrorMessage = "Selecione um arquivo")]
        [Display(Name = "Arquivo")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Descrição")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        // Lista de departamentos para dropdown
        public List<Department> AvailableDepartments { get; set; } = new List<Department>();
    }
}
