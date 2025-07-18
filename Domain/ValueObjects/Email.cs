using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IntranetDocumentos.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um endereço de email válido
    /// </summary>
    public sealed class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email não pode ser vazio ou nulo", nameof(value));

            var normalizedValue = value.Trim().ToLowerInvariant();

            if (!IsValidEmail(normalizedValue))
                throw new ArgumentException($"Email inválido: {value}", nameof(value));

            Value = normalizedValue;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Validação básica com regex
            if (!EmailRegex.IsMatch(email))
                return false;

            // Validação adicional com EmailAddressAttribute do .NET
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // Conversões implícitas para facilitar o uso
        public static implicit operator string(Email email) => email.Value;
        
        public static explicit operator Email(string email) => new(email);

        // Métodos de conveniência
        public string GetDomain()
        {
            var atIndex = Value.IndexOf('@');
            return atIndex >= 0 ? Value.Substring(atIndex + 1) : string.Empty;
        }

        public string GetLocalPart()
        {
            var atIndex = Value.IndexOf('@');
            return atIndex >= 0 ? Value.Substring(0, atIndex) : Value;
        }

        public bool IsFromDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            return GetDomain().Equals(domain.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Factory methods
        public static Email Create(string value) => new(value);

        public static bool TryCreate(string value, out Email? email)
        {
            email = null;
            try
            {
                email = new Email(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
