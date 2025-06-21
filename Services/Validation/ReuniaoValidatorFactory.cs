using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Validation
{
    /// <summary>
    /// Factory para criação de validadores - Factory Pattern
    /// Implementa Single Responsibility Principle (SRP)
    /// </summary>
    public class ReuniaoValidatorFactory : IReuniaoValidatorFactory
    {
        private readonly IEnumerable<IReuniaoValidator> _validators;

        public ReuniaoValidatorFactory(IEnumerable<IReuniaoValidator> validators)
        {
            _validators = validators;
        }

        /// <summary>
        /// Obtém o validador apropriado para o tipo de reunião
        /// </summary>
        public IReuniaoValidator GetValidator(TipoReuniao tipoReuniao)
        {
            var validator = _validators.FirstOrDefault(v => v.CanValidate(tipoReuniao));
            
            if (validator == null)
                throw new NotSupportedException($"Tipo de reunião {tipoReuniao} não suportado");

            return validator;
        }

        /// <summary>
        /// Obtém todos os validadores aplicáveis para o tipo de reunião
        /// </summary>
        public IEnumerable<IReuniaoValidator> GetValidators(TipoReuniao tipoReuniao)
        {
            return _validators.Where(v => v.CanValidate(tipoReuniao));
        }
    }
}
