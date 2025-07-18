using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Reunioes
{
    /// <summary>
    /// DTO para criar reunião
    /// </summary>
    public class CreateReuniaoDTO
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O horário de início é obrigatório")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "O horário de fim é obrigatório")]
        public TimeSpan HoraFim { get; set; }

        [StringLength(200, ErrorMessage = "O local deve ter no máximo 200 caracteres")]
        public string? Local { get; set; }

        public TipoReuniao TipoReuniao { get; set; } = TipoReuniao.Interna;

        public int? DepartmentId { get; set; }

        public string? ResponsavelUserId { get; set; }

        public List<ParticipanteDTO> Participantes { get; set; } = new();

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        public bool NotificarParticipantes { get; set; } = true;
    }

    /// <summary>
    /// DTO para atualização de reunião
    /// </summary>
    public class UpdateReuniaoDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "O horário de início é obrigatório")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "O horário de fim é obrigatório")]
        public TimeSpan HoraFim { get; set; }

        [StringLength(200, ErrorMessage = "O local deve ter no máximo 200 caracteres")]
        public string? Local { get; set; }

        public TipoReuniao TipoReuniao { get; set; } = TipoReuniao.Interna;

        public int? DepartmentId { get; set; }

        public string? ResponsavelUserId { get; set; }

        public List<ParticipanteDTO> Participantes { get; set; } = new();

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        public bool NotificarParticipantes { get; set; } = true;
    }

    /// <summary>
    /// DTO para resposta de reunião
    /// </summary>
    public class ReuniaoDTO : BaseDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public string? Local { get; set; }
        public TipoReuniao TipoReuniao { get; set; }
        public string TipoReuniaoDescricao => TipoReuniao.GetDescription();
        public StatusReuniao Status { get; set; }
        public string StatusDescricao => Status.GetDescription();
        public string? Observacoes { get; set; }
        
        // Relacionamentos
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ResponsavelUserId { get; set; }
        public string? ResponsavelName { get; set; }
        public string CriadoPorUserId { get; set; } = string.Empty;
        public string CriadoPorName { get; set; } = string.Empty;
        
        // Participantes
        public List<ParticipanteDTO> Participantes { get; set; } = new();
        public int TotalParticipantes => Participantes.Count;
        public int ParticipantesConfirmados => Participantes.Count(p => p.Confirmado);
        public int ParticipantesPendentes => Participantes.Count(p => !p.Confirmado);
        
        // Informações calculadas
        public TimeSpan Duracao => HoraFim - HoraInicio;
        public string DuracaoFormatada => $"{Duracao.Hours:D2}:{Duracao.Minutes:D2}";
        public bool JaOcorreu => Data.Date < DateTime.Now.Date || (Data.Date == DateTime.Now.Date && HoraFim < DateTime.Now.TimeOfDay);
        public bool EstaOcorrendo => Data.Date == DateTime.Now.Date && HoraInicio <= DateTime.Now.TimeOfDay && HoraFim >= DateTime.Now.TimeOfDay;
        public bool VaiOcorrer => Data.Date > DateTime.Now.Date || (Data.Date == DateTime.Now.Date && HoraInicio > DateTime.Now.TimeOfDay);
        
        // Metadados
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public string? UltimaAtualizacaoPor { get; set; }
    }

    /// <summary>
    /// DTO para participante da reunião
    /// </summary>
    public class ParticipanteDTO
    {
        public int Id { get; set; }
        public int ReuniaoId { get; set; }
        public string? UserId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool Confirmado { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public bool Presente { get; set; }
        public string? Observacoes { get; set; }
        public TipoParticipacao TipoParticipacao { get; set; } = TipoParticipacao.Participante;
        public string TipoParticipacaoDescricao => TipoParticipacao.GetDescription();
    }

    /// <summary>
    /// DTO para filtros de busca de reuniões
    /// </summary>
    public class ReuniaoSearchDTO
    {
        public string? Query { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public TipoReuniao? TipoReuniao { get; set; }
        public StatusReuniao? Status { get; set; }
        public int? DepartmentId { get; set; }
        public string? ResponsavelUserId { get; set; }
        public string? ParticipanteUserId { get; set; }
        public string? Local { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "Data";
        public string? SortOrder { get; set; } = "asc";
    }

    /// <summary>
    /// DTO para calendário de reuniões
    /// </summary>
    public class CalendarioReuniaoDTO
    {
        public int Ano { get; set; } = DateTime.Now.Year;
        public int Mes { get; set; } = DateTime.Now.Month;
        public string MesNome => new DateTime(Ano, Mes, 1).ToString("MMMM");
        public List<ReuniaoDTO> Reunioes { get; set; } = new();
        public List<DateTime> DiasComReuniao { get; set; } = new();
        public CalendarioFiltrosDTO Filtros { get; set; } = new();
    }

    /// <summary>
    /// DTO para filtros do calendário
    /// </summary>
    public class CalendarioFiltrosDTO
    {
        public int? DepartmentId { get; set; }
        public TipoReuniao? TipoReuniao { get; set; }
        public StatusReuniao? Status { get; set; }
        public string? ResponsavelUserId { get; set; }
        public bool ApenasMinhasReunioes { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas de reuniões
    /// </summary>
    public class ReuniaoStatisticsDTO
    {
        public int TotalReunioes { get; set; }
        public int ReunioesMesAtual { get; set; }
        public int ReunioesPendentes { get; set; }
        public int ReunioesRealizadas { get; set; }
        public int ReunioesCanceladas { get; set; }
        public double TempoMedioReunioes { get; set; } // em minutos
        public double TaxaComparecimento { get; set; }
        
        public List<ReunioesByTipoDTO> ReunioesByTipo { get; set; } = new();
        public List<ReunioesByStatusDTO> ReunioesByStatus { get; set; } = new();
        public List<ReunioesByDepartmentDTO> ReunioesByDepartment { get; set; } = new();
        public List<ReunioesByMesDTO> ReunioesByMes { get; set; } = new();
    }

    /// <summary>
    /// DTO para reuniões por tipo
    /// </summary>
    public class ReunioesByTipoDTO
    {
        public TipoReuniao TipoReuniao { get; set; }
        public string TipoReuniaoDescricao => TipoReuniao.GetDescription();
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para reuniões por status
    /// </summary>
    public class ReunioesByStatusDTO
    {
        public StatusReuniao Status { get; set; }
        public string StatusDescricao => Status.GetDescription();
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para reuniões por departamento
    /// </summary>
    public class ReunioesByDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para reuniões por mês
    /// </summary>
    public class ReunioesByMesDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double MediaDuracao { get; set; }
        public int TotalParticipantes { get; set; }
    }

    /// <summary>
    /// DTO para confirmação de participação
    /// </summary>
    public class ConfirmarParticipacaoDTO
    {
        [Required]
        public int ReuniaoId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public bool Confirmado { get; set; }
        
        public string? Observacoes { get; set; }
    }

    /// <summary>
    /// DTO para marcar presença
    /// </summary>
    public class MarcarPresencaDTO
    {
        [Required]
        public int ReuniaoId { get; set; }
        
        [Required]
        public List<int> ParticipanteIds { get; set; } = new();
    }

    /// <summary>
    /// Enum para tipo de reunião
    /// </summary>
    public enum TipoReuniao
    {
        Interna = 1,
        Externa = 2,
        Diretoria = 3,
        Departamental = 4,
        Treinamento = 5,
        Apresentacao = 6,
        Workshop = 7,
        Videoconferencia = 8
    }

    /// <summary>
    /// Enum para status da reunião
    /// </summary>
    public enum StatusReuniao
    {
        Agendada = 1,
        Confirmada = 2,
        EmAndamento = 3,
        Realizada = 4,
        Cancelada = 5,
        Reagendada = 6
    }

    /// <summary>
    /// Enum para tipo de participação
    /// </summary>
    public enum TipoParticipacao
    {
        Participante = 1,
        Organizador = 2,
        Palestrante = 3,
        Convidado = 4,
        Opcional = 5
    }

    /// <summary>
    /// Extensões para os enums
    /// </summary>
    public static class ReuniaoEnumExtensions
    {
        public static string GetDescription(this TipoReuniao tipo)
        {
            return tipo switch
            {
                TipoReuniao.Interna => "Interna",
                TipoReuniao.Externa => "Externa",
                TipoReuniao.Diretoria => "Diretoria",
                TipoReuniao.Departamental => "Departamental",
                TipoReuniao.Treinamento => "Treinamento",
                TipoReuniao.Apresentacao => "Apresentação",
                TipoReuniao.Workshop => "Workshop",
                TipoReuniao.Videoconferencia => "Videoconferência",
                _ => "Desconhecido"
            };
        }

        public static string GetDescription(this StatusReuniao status)
        {
            return status switch
            {
                StatusReuniao.Agendada => "Agendada",
                StatusReuniao.Confirmada => "Confirmada",
                StatusReuniao.EmAndamento => "Em Andamento",
                StatusReuniao.Realizada => "Realizada",
                StatusReuniao.Cancelada => "Cancelada",
                StatusReuniao.Reagendada => "Reagendada",
                _ => "Desconhecido"
            };
        }

        public static string GetDescription(this TipoParticipacao tipo)
        {
            return tipo switch
            {
                TipoParticipacao.Participante => "Participante",
                TipoParticipacao.Organizador => "Organizador",
                TipoParticipacao.Palestrante => "Palestrante",
                TipoParticipacao.Convidado => "Convidado",
                TipoParticipacao.Opcional => "Opcional",
                _ => "Desconhecido"
            };
        }
    }
}
