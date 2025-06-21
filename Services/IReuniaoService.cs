using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Interface para serviços de reunião
    /// Implementa separação de responsabilidades conforme SRP
    /// </summary>
    public interface IReuniaoService
    {
        /// <summary>
        /// Obtém reuniões por período com filtros
        /// </summary>
        Task<List<Reuniao>> GetReunioesPorPeriodoAsync(DateTime inicio, DateTime fim, CalendarioFiltros? filtros = null);

        /// <summary>
        /// Obtém reunião por ID com detalhes completos
        /// </summary>
        Task<Reuniao?> GetReuniaoDetalhadaAsync(int id);

        /// <summary>
        /// Cria uma nova reunião
        /// </summary>
        Task<Reuniao> CriarReuniaoAsync(ReuniaoViewModel viewModel, string userId);

        /// <summary>
        /// Atualiza uma reunião existente
        /// </summary>
        Task<bool> AtualizarReuniaoAsync(int id, ReuniaoViewModel viewModel, string userId);

        /// <summary>
        /// Remove uma reunião
        /// </summary>
        Task<bool> RemoverReuniaoAsync(int id, string userId);

        /// <summary>
        /// Valida regras de negócio para reunião
        /// </summary>
        Task<ValidationResult> ValidarReuniaoAsync(ReuniaoViewModel viewModel, int? reuniaoIdExcluir = null);

        /// <summary>
        /// Verifica se usuário pode editar a reunião
        /// </summary>
        Task<bool> PodeEditarReuniaoAsync(int reuniaoId, string userId, bool isAdmin);        /// <summary>
        /// Popula dados para ViewModels (Ramais, Departamentos)
        /// </summary>
        Task PopularDadosViewModelAsync(ReuniaoViewModel viewModel);

        /// <summary>
        /// Marca reunião como realizada
        /// </summary>
        Task<bool> MarcarReuniaoRealizadaAsync(int reuniaoId, string userId, bool isAdmin);

        /// <summary>
        /// Cancela uma reunião
        /// </summary>
        Task<bool> CancelarReuniaoAsync(int reuniaoId, string userId, bool isAdmin);

        /// <summary>
        /// Verifica se reunião existe
        /// </summary>
        Task<bool> ReuniaoExisteAsync(int reuniaoId);

        /// <summary>
        /// Obtém reunião simples por ID (sem includes)
        /// </summary>
        Task<Reuniao?> GetReuniaoAsync(int id);

        /// <summary>
        /// Mapeia reunião para ViewModel para edição
        /// </summary>
        Task<ReuniaoViewModel> MapearParaViewModelAsync(Reuniao reuniao);
    }

    /// <summary>
    /// Resultado de validação para reuniões
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void Merge(ValidationResult other)
        {
            Errors.AddRange(other.Errors);
        }
    }
}
