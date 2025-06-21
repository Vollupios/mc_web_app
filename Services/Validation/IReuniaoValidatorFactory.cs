using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Interface para factory de validadores de reunião
    /// Implementa Dependency Inversion Principle (DIP)
    /// </summary>
    public interface IReuniaoValidatorFactory
    {
        /// <summary>
        /// Obtém o validador apropriado para o tipo de reunião
        /// </summary>
        /// <param name="tipoReuniao">Tipo da reunião</param>
        /// <returns>Validador específico para o tipo</returns>
        IReuniaoValidator GetValidator(TipoReuniao tipoReuniao);
    }
}
