using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace IntranetDocumentos.Models.ValueObjects
{
    /// <summary>
    /// Value Object que representa um checksum SHA256 de documento
    /// </summary>
    public sealed class DocumentChecksum : ValueObject
    {
        private static readonly Regex Sha256Regex = new(
            @"^[A-Fa-f0-9]{64}$",
            RegexOptions.Compiled);

        public string Value { get; }

        public DocumentChecksum(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Checksum não pode ser vazio ou nulo", nameof(value));

            var normalizedValue = value.Trim().ToUpperInvariant();

            if (!IsValidSha256(normalizedValue))
                throw new ArgumentException($"Checksum SHA256 inválido: {value}", nameof(value));

            Value = normalizedValue;
        }

        private static bool IsValidSha256(string checksum)
        {
            return !string.IsNullOrWhiteSpace(checksum) && 
                   checksum.Length == 64 && 
                   Sha256Regex.IsMatch(checksum);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // Conversões implícitas
        public static implicit operator string(DocumentChecksum checksum) => checksum.Value;
        
        public static explicit operator DocumentChecksum(string checksum) => new(checksum);

        // Factory methods
        public static DocumentChecksum Create(string value) => new(value);

        public static bool TryCreate(string value, out DocumentChecksum? checksum)
        {
            checksum = null;
            try
            {
                checksum = new DocumentChecksum(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Calcula o checksum SHA256 de um array de bytes
        /// </summary>
        public static DocumentChecksum FromBytes(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            var hashString = Convert.ToHexString(hashBytes);
            
            return new DocumentChecksum(hashString);
        }

        /// <summary>
        /// Calcula o checksum SHA256 de uma string usando UTF-8
        /// </summary>
        public static DocumentChecksum FromString(string data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var bytes = Encoding.UTF8.GetBytes(data);
            return FromBytes(bytes);
        }

        /// <summary>
        /// Verifica se o checksum corresponde aos dados fornecidos
        /// </summary>
        public bool VerifyData(byte[] data)
        {
            try
            {
                var computedChecksum = FromBytes(data);
                return Equals(computedChecksum);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtém uma versão truncada do checksum para exibição
        /// </summary>
        public string ToShortString(int length = 8)
        {
            if (length <= 0 || length > Value.Length)
                return Value;
                
            return Value.Substring(0, length);
        }
    }
}
