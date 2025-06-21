using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Modelo para representar as reuniões agendadas
    /// </summary>
    public class Reuniao
    {
        public int Id { get; set; }

        /// <summary>
        /// Título da reunião
        /// </summary>
        [Required(ErrorMessage = "O título da reunião é obrigatório")]
        [Display(Name = "Título")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Data da reunião
        /// </summary>
        [Required(ErrorMessage = "A data da reunião é obrigatória")]
        [Display(Name = "Data")]
        public DateTime Data { get; set; }

        /// <summary>
        /// Horário de início da reunião
        /// </summary>
        [Required(ErrorMessage = "O horário de início é obrigatório")]
        [Display(Name = "Hora Início")]
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Horário de fim da reunião
        /// </summary>
        [Required(ErrorMessage = "O horário de fim é obrigatório")]
        [Display(Name = "Hora Fim")]
        public TimeSpan HoraFim { get; set; }

        /// <summary>
        /// Tipo de reunião (Interno, Externo, Online)
        /// </summary>
        [Required(ErrorMessage = "O tipo de reunião é obrigatório")]
        [Display(Name = "Tipo de Reunião")]
        public TipoReuniao TipoReuniao { get; set; }

        /// <summary>
        /// Sala da reunião (para reuniões internas e externas)
        /// </summary>
        [Display(Name = "Sala de Reunião")]
        public SalaReuniao? Sala { get; set; }

        /// <summary>
        /// Veículo utilizado (para reuniões externas)
        /// </summary>
        [Display(Name = "Veículo")]
        public VeiculoReuniao? Veiculo { get; set; }

        /// <summary>
        /// Link da reunião online
        /// </summary>
        [Display(Name = "Link da Reunião")]
        [StringLength(500, ErrorMessage = "O link deve ter no máximo 500 caracteres")]
        public string? LinkOnline { get; set; }

        /// <summary>
        /// Nome da empresa/cliente
        /// </summary>
        [Display(Name = "Empresa/Cliente")]
        [StringLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres")]
        public string? Empresa { get; set; }

        /// <summary>
        /// Status da reunião
        /// </summary>
        [Display(Name = "Status")]
        public StatusReuniao Status { get; set; } = StatusReuniao.Agendada;

        /// <summary>
        /// Observações adicionais sobre a reunião
        /// </summary>
        [Display(Name = "Observações")]
        [StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres")]
        public string? Observacoes { get; set; }

        /// <summary>
        /// Data e hora em que a reunião foi cadastrada
        /// </summary>
        [Display(Name = "Cadastrado em")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        /// <summary>
        /// ID do usuário responsável pelo cadastro
        /// </summary>
        [Required]
        [Display(Name = "Responsável pelo Cadastro")]
        public string ResponsavelUserId { get; set; } = string.Empty;

        /// <summary>
        /// Navegação para o usuário responsável
        /// </summary>
        [ForeignKey("ResponsavelUserId")]
        public virtual ApplicationUser? ResponsavelUser { get; set; }

        /// <summary>
        /// Lista de participantes da reunião
        /// </summary>
        public virtual ICollection<ReuniaoParticipante> Participantes { get; set; } = new List<ReuniaoParticipante>();
    }

    /// <summary>
    /// Modelo para representar os participantes de uma reunião
    /// </summary>
    public class ReuniaoParticipante
    {
        public int Id { get; set; }

        /// <summary>
        /// ID da reunião
        /// </summary>
        public int ReuniaoId { get; set; }

        /// <summary>
        /// Navegação para a reunião
        /// </summary>
        [ForeignKey("ReuniaoId")]
        public virtual Reuniao? Reuniao { get; set; }

        /// <summary>
        /// Nome do participante
        /// </summary>
        [Required(ErrorMessage = "O nome do participante é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// ID do ramal do participante
        /// </summary>
        [Display(Name = "Ramal")]
        public int? RamalId { get; set; }

        /// <summary>
        /// Navegação para o ramal
        /// </summary>
        [ForeignKey("RamalId")]
        public virtual Ramal? Ramal { get; set; }

        /// <summary>
        /// ID do departamento do participante
        /// </summary>
        [Display(Name = "Departamento")]
        public int? DepartamentoId { get; set; }

        /// <summary>
        /// Navegação para o departamento
        /// </summary>
        [ForeignKey("DepartamentoId")]
        public virtual Department? Departamento { get; set; }
    }

    /// <summary>
    /// Enum para tipos de reunião
    /// </summary>
    public enum TipoReuniao
    {
        [Display(Name = "Interno")]
        Interno = 0,
        
        [Display(Name = "Externo")]
        Externo = 1,
        
        [Display(Name = "Online")]
        Online = 2
    }    /// <summary>
    /// Enum para salas de reunião
    /// </summary>
    public enum SalaReuniao
    {
        [Display(Name = "Sala de Reunião 1")]
        Reuniao1 = 0,
        
        [Display(Name = "Sala de Reunião 2")]
        Reuniao2 = 1,
        
        [Display(Name = "Sala da Diretoria")]
        Diretoria = 2,
        
        [Display(Name = "Auditório")]
        Auditorio = 3
    }

    /// <summary>
    /// Enum para veículos
    /// </summary>
    public enum VeiculoReuniao
    {
        [Display(Name = "Carro 1")]
        Carro1 = 0,
        
        [Display(Name = "Carro 2")]
        Carro2 = 1,
        
        [Display(Name = "Van")]
        Van = 2
    }

    /// <summary>
    /// Enum para status da reunião
    /// </summary>
    public enum StatusReuniao
    {
        [Display(Name = "Agendada")]
        Agendada = 0,
        
        [Display(Name = "Realizada")]
        Realizada = 1,
        
        [Display(Name = "Cancelada")]
        Cancelada = 2
    }
}
