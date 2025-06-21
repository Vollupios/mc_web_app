using System;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Exceção lançada quando há erros de validação de negócio
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException() { }
        
        public ValidationException(string message) : base(message) { }
        
        public ValidationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
