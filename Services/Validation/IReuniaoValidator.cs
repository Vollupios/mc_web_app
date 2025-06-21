using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Interface para validadores de reunião - Strategy Pattern
    /// Implementa Open/Closed Principle (OCP)
    /// </summary>
    public interface IReuniaoValidator
    {
        /// <summary>
        /// Valida a reunião baseado em suas regras específicas
        /// </summary>
        Task<ValidationResult> ValidateAsync(ReuniaoViewModel reuniao, int? reuniaoIdExcluir = null);

        /// <summary>
        /// Indica se este validador pode processar o tipo de reunião
        /// </summary>
        bool CanValidate(TipoReuniao tipoReuniao);
    }
}
