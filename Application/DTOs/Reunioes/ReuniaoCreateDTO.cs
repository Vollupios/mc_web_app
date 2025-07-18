using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Reunioes
{
    /// <summary>
    /// DTO para criar reunião
    /// </summary>
    public class ReuniaoCreateDTO : BaseDTO
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "O local é obrigatório")]
        [StringLength(100, ErrorMessage = "O local deve ter no máximo 100 caracteres")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public List<string> Participants { get; set; } = new();
    }
}
