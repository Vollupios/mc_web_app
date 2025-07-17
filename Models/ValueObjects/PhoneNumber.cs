using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IntranetDocumentos.Models.ValueObjects
{
    /// <summary>
    /// Value Object que representa um número de telefone brasileiro
    /// </summary>
    public sealed class PhoneNumber : ValueObject
    {
        private static readonly Regex BrazilianPhoneRegex = new(
            @"^(?:\+55\s?)?(?:\(?(?:0?11|0?[12-9]\d)\)?\s?)?(?:9\s?)?[2-9]\d{3}[\s\-]?\d{4}$",
            RegexOptions.Compiled);

        private static readonly Regex DigitsOnlyRegex = new(
            @"\D",
            RegexOptions.Compiled);

        public string Value { get; }
        public string DigitsOnly { get; }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Número de telefone não pode ser vazio ou nulo", nameof(value));

            var normalizedValue = value.Trim();
            var digitsOnly = DigitsOnlyRegex.Replace(normalizedValue, "");

            if (!IsValidBrazilianPhone(normalizedValue) && !IsValidBrazilianPhone(digitsOnly))
                throw new ArgumentException($"Número de telefone inválido: {value}", nameof(value));

            Value = normalizedValue;
            DigitsOnly = digitsOnly;
        }

        private static bool IsValidBrazilianPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove todos os não-dígitos para validação básica
            var digitsOnly = Regex.Replace(phone, @"\D", "");

            // Validações básicas de tamanho
            if (digitsOnly.Length < 10 || digitsOnly.Length > 13)
                return false;

            // Se começar com 55 (código do Brasil), deve ter 13 dígitos
            if (digitsOnly.StartsWith("55") && digitsOnly.Length == 13)
                return true;

            // Telefone com DDD (10 ou 11 dígitos)
            if (digitsOnly.Length >= 10 && digitsOnly.Length <= 11)
                return true;

            // Validação com regex para formato brasileiro
            return BrazilianPhoneRegex.IsMatch(phone);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return DigitsOnly; // Usa apenas os dígitos para igualdade
        }

        public override string ToString() => Value;

        // Conversões implícitas
        public static implicit operator string(PhoneNumber phone) => phone.Value;
        
        public static explicit operator PhoneNumber(string phone) => new(phone);

        // Factory methods
        public static PhoneNumber Create(string value) => new(value);

        public static bool TryCreate(string value, out PhoneNumber? phone)
        {
            phone = null;
            try
            {
                phone = new PhoneNumber(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Métodos de conveniência
        public string GetAreaCode()
        {
            if (DigitsOnly.Length >= 10)
            {
                // Se começar com 55 (código do país), pular os primeiros 2 dígitos
                var startIndex = DigitsOnly.StartsWith("55") ? 2 : 0;
                
                if (DigitsOnly.Length - startIndex >= 10)
                    return DigitsOnly.Substring(startIndex, 2);
            }
            
            return string.Empty;
        }

        public string GetNumber()
        {
            if (DigitsOnly.Length >= 10)
            {
                var startIndex = DigitsOnly.StartsWith("55") ? 4 : 2; // Pular código país + DDD
                return DigitsOnly.Substring(startIndex);
            }
            
            return DigitsOnly;
        }

        public bool IsMobile()
        {
            var number = GetNumber();
            return number.Length == 9 && number.StartsWith("9");
        }

        public bool IsLandline()
        {
            var number = GetNumber();
            return number.Length == 8 && !number.StartsWith("9");
        }

        public string ToFormattedString()
        {
            if (DigitsOnly.Length == 11) // Celular com DDD
            {
                return $"({DigitsOnly.Substring(0, 2)}) {DigitsOnly.Substring(2, 5)}-{DigitsOnly.Substring(7, 4)}";
            }
            else if (DigitsOnly.Length == 10) // Fixo com DDD
            {
                return $"({DigitsOnly.Substring(0, 2)}) {DigitsOnly.Substring(2, 4)}-{DigitsOnly.Substring(6, 4)}";
            }
            else if (DigitsOnly.Length == 13 && DigitsOnly.StartsWith("55")) // Com código do país
            {
                var areaCode = DigitsOnly.Substring(2, 2);
                var number = DigitsOnly.Substring(4);
                
                if (number.Length == 9) // Celular
                    return $"+55 ({areaCode}) {number.Substring(0, 5)}-{number.Substring(5, 4)}";
                else if (number.Length == 8) // Fixo
                    return $"+55 ({areaCode}) {number.Substring(0, 4)}-{number.Substring(4, 4)}";
            }
            
            return Value; // Retorna o valor original se não conseguir formatar
        }
    }
}
