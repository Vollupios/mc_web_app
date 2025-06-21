using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel para criação e edição de reuniões
    /// </summary>
    public class ReuniaoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [Display(Name = "Título")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data da reunião é obrigatória")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O horário de início é obrigatório")]
        [Display(Name = "Hora Início")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "O horário de fim é obrigatório")]
        [Display(Name = "Hora Fim")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFim { get; set; }

        [Required(ErrorMessage = "O tipo de reunião é obrigatório")]
        [Display(Name = "Tipo de Reunião")]
        public TipoReuniao TipoReuniao { get; set; }

        [Display(Name = "Sala de Reunião")]
        public SalaReuniao? Sala { get; set; }

        [Display(Name = "Veículo")]
        public VeiculoReuniao? Veiculo { get; set; }

        [Display(Name = "Link da Reunião")]
        [StringLength(500, ErrorMessage = "O link deve ter no máximo 500 caracteres")]
        [Url(ErrorMessage = "Por favor, insira um link válido")]
        public string? LinkOnline { get; set; }

        [Display(Name = "Empresa/Cliente")]
        [StringLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres")]
        public string? Empresa { get; set; }

        [Display(Name = "Status")]
        public StatusReuniao Status { get; set; } = StatusReuniao.Agendada;

        [Display(Name = "Observações")]
        [StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres")]
        public string? Observacoes { get; set; }

        [Display(Name = "Data/Hora da Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Responsável")]
        public string? ResponsavelUserId { get; set; }        [Display(Name = "Participantes")]
        public List<ParticipanteViewModel> Participantes { get; set; } = new List<ParticipanteViewModel>();

        /// <summary>
        /// Lista de ramais disponíveis para seleção
        /// </summary>
        public List<Ramal> RamaisDisponiveis { get; set; } = new List<Ramal>();

        /// <summary>
        /// Lista de departamentos disponíveis para seleção
        /// </summary>
        public List<Department> DepartamentosDisponiveis { get; set; } = new List<Department>();
    }

    /// <summary>
    /// ViewModel para visualização de calendário
    /// </summary>
    public class CalendarioViewModel
    {
        /// <summary>
        /// Ano atual sendo visualizado
        /// </summary>
        public int Ano { get; set; } = DateTime.Now.Year;

        /// <summary>
        /// Mês atual sendo visualizado
        /// </summary>
        public int Mes { get; set; } = DateTime.Now.Month;

        /// <summary>
        /// Lista de reuniões do mês
        /// </summary>
        public List<Reuniao> Reunioes { get; set; } = new List<Reuniao>();

        /// <summary>
        /// Filtros aplicados
        /// </summary>
        public CalendarioFiltros Filtros { get; set; } = new CalendarioFiltros();
    }

    /// <summary>
    /// Filtros para o calendário
    /// </summary>
    public class CalendarioFiltros
    {
        [Display(Name = "Tipo de Reunião")]
        public TipoReuniao? TipoReuniao { get; set; }

        [Display(Name = "Status")]
        public StatusReuniao? Status { get; set; }

        [Display(Name = "Sala")]
        public SalaReuniao? Sala { get; set; }

        [Display(Name = "Veículo")]
        public VeiculoReuniao? Veiculo { get; set; }

        [Display(Name = "Empresa")]
        public string? Empresa { get; set; }
    }

    /// <summary>
    /// ViewModel para participantes da reunião
    /// </summary>
    public class ParticipanteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do participante é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Ramal")]
        public int? RamalId { get; set; }

        [Display(Name = "Departamento")]
        public int? DepartamentoId { get; set; }
    }
}
