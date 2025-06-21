using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Validador base com validações comuns a todos os tipos de reunião
    /// Template Method Pattern
    /// </summary>
    public class BaseReuniaoValidator : IReuniaoValidator
    {
        public virtual bool CanValidate(TipoReuniao tipoReuniao)
        {
            return true; // Validações básicas aplicam-se a todos os tipos
        }

        public virtual Task<ValidationResult> ValidateAsync(ReuniaoViewModel reuniao, int? reuniaoIdExcluir = null)
        {
            var result = new ValidationResult();

            ValidarCamposObrigatorios(reuniao, result);
            ValidarHorarios(reuniao, result);
            ValidarData(reuniao, result);
            ValidarParticipantes(reuniao, result);

            return Task.FromResult(result);
        }

        protected virtual void ValidarCamposObrigatorios(ReuniaoViewModel viewModel, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Titulo))
                result.AddError("O título da reunião é obrigatório.");
        }

        protected virtual void ValidarHorarios(ReuniaoViewModel viewModel, ValidationResult result)
        {
            // Validar horário de início e fim
            if (viewModel.HoraFim <= viewModel.HoraInicio)
                result.AddError("O horário de fim deve ser maior que o horário de início.");

            // Validar horário comercial (7h às 18h)
            if (viewModel.HoraInicio < new TimeSpan(7, 0, 0) || viewModel.HoraFim > new TimeSpan(18, 0, 0))
                result.AddError("As reuniões devem ser agendadas entre 07:00 e 18:00.");

            // Validar duração mínima (15 minutos)
            if (viewModel.HoraFim - viewModel.HoraInicio < TimeSpan.FromMinutes(15))
                result.AddError("A reunião deve ter duração mínima de 15 minutos.");

            // Validar duração máxima (8 horas)
            if (viewModel.HoraFim - viewModel.HoraInicio > TimeSpan.FromHours(8))
                result.AddError("A reunião não pode durar mais de 8 horas.");
        }

        protected virtual void ValidarData(ReuniaoViewModel viewModel, ValidationResult result)
        {
            if (viewModel.Data < DateTime.Today)
                result.AddError("A data da reunião deve ser hoje ou uma data futura.");

            // Não permitir reuniões aos sábados e domingos
            if (viewModel.Data.DayOfWeek == DayOfWeek.Saturday || viewModel.Data.DayOfWeek == DayOfWeek.Sunday)
                result.AddError("Reuniões não podem ser agendadas aos finais de semana.");
        }

        protected virtual void ValidarParticipantes(ReuniaoViewModel viewModel, ValidationResult result)
        {
            if (viewModel.Participantes?.Any() == true)
            {
                var participantesValidos = viewModel.Participantes.Where(p => !string.IsNullOrEmpty(p.Nome)).ToList();

                if (participantesValidos.Count > 50)
                    result.AddError("O número máximo de participantes é 50.");

                // Verificar nomes duplicados
                var nomesDuplicados = participantesValidos
                    .GroupBy(p => p.Nome!.Trim().ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (nomesDuplicados.Any())
                    result.AddError("Existem participantes com nomes duplicados.");
            }
            else
            {
                result.AddError("Pelo menos um participante deve ser informado.");
            }
        }
    }
}
