using IntranetDocumentos.Data;
using IntranetDocumentos.Extensions;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Validador para reuniões internas - Strategy Pattern implementado
    /// </summary>
    public class ReuniaoInternaValidator : IReuniaoValidator
    {
        private readonly ApplicationDbContext _context;

        public ReuniaoInternaValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool CanValidate(TipoReuniao tipoReuniao)
        {
            return tipoReuniao == TipoReuniao.Interno;
        }

        public async Task<ValidationResult> ValidateAsync(ReuniaoViewModel reuniao, int? reuniaoIdExcluir = null)
        {
            var result = new ValidationResult();

            // Validar campos obrigatórios para reuniões internas
            if (!reuniao.Sala.HasValue)
            {
                result.AddError("A sala é obrigatória para reuniões internas.");
                return result;
            }

            // Verificar conflitos de sala
            await ValidarConflitoDeSalaAsync(reuniao, reuniaoIdExcluir, result);

            return result;
        }

        private async Task ValidarConflitoDeSalaAsync(ReuniaoViewModel viewModel, int? reuniaoIdExcluir, ValidationResult result)
        {
            var reunioesConflitantes = await _context.Reunioes
                .Where(r => r.Id != reuniaoIdExcluir &&
                           r.Sala == viewModel.Sala &&
                           r.Data == viewModel.Data &&
                           r.Status == StatusReuniao.Agendada)
                .ToListAsync();

            var temConflito = reunioesConflitantes.Any(r =>
                (r.HoraInicio <= viewModel.HoraInicio && r.HoraFim > viewModel.HoraInicio) ||
                (r.HoraInicio < viewModel.HoraFim && r.HoraFim >= viewModel.HoraFim) ||
                (r.HoraInicio >= viewModel.HoraInicio && r.HoraFim <= viewModel.HoraFim));            if (temConflito)
            {
                result.AddError($"A sala {viewModel.Sala!.Value.GetDisplayName()} já está ocupada neste horário.");
            }
        }
    }
}
