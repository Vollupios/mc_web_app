using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Validador para reuniões online - Strategy Pattern implementado
    /// </summary>
    public class ReuniaoOnlineValidator : IReuniaoValidator
    {
        public bool CanValidate(TipoReuniao tipoReuniao)
        {
            return tipoReuniao == TipoReuniao.Online;
        }

        public Task<ValidationResult> ValidateAsync(ReuniaoViewModel reuniao, int? reuniaoIdExcluir = null)
        {
            var result = new ValidationResult();

            // Validar campos obrigatórios para reuniões online
            if (string.IsNullOrWhiteSpace(reuniao.LinkOnline))
            {
                result.AddError("O link é obrigatório para reuniões online.");
            }
            else
            {
                // Validar formato do URL
                if (!Uri.TryCreate(reuniao.LinkOnline, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    result.AddError("O link deve ser uma URL válida (http ou https).");
                }
            }

            // Reuniões online não podem ter sala definida
            if (reuniao.Sala.HasValue)
            {
                result.AddError("Reuniões online não devem ter sala definida.");
            }

            // Reuniões online não podem ter veículo definido
            if (reuniao.Veiculo.HasValue)
            {
                result.AddError("Reuniões online não devem ter veículo definido.");
            }

            return Task.FromResult(result);
        }
    }
}
