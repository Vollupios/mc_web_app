using System;
using System.Collections.Generic;
using System.Globalization;

namespace IntranetDocumentos.Models.ValueObjects
{
    /// <summary>
    /// Value Object que representa um valor monetário em Real brasileiro (BRL)
    /// </summary>
    public sealed class Money : ValueObject, IComparable<Money>
    {
        private static readonly CultureInfo BrazilianCulture = new("pt-BR");
        
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Moeda não pode ser vazia ou nula", nameof(currency));

            if (amount < 0)
                throw new ArgumentException("Valor monetário não pode ser negativo", nameof(amount));

            // Arredondar para 2 casas decimais (centavos)
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            Currency = currency.ToUpperInvariant();
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString()
        {
            return Currency switch
            {
                "BRL" => Amount.ToString("C", BrazilianCulture),
                "USD" => Amount.ToString("C", new CultureInfo("en-US")),
                "EUR" => Amount.ToString("C", new CultureInfo("de-DE")),
                _ => $"{Amount:F2} {Currency}"
            };
        }

        // Conversões implícitas
        public static implicit operator decimal(Money money) => money.Amount;
        
        public static explicit operator Money(decimal amount) => new(amount);

        // Factory methods
        public static Money Create(decimal amount, string currency = "BRL") => new(amount, currency);
        public static Money CreateBRL(decimal amount) => new(amount, "BRL");
        public static Money CreateUSD(decimal amount) => new(amount, "USD");
        public static Money CreateEUR(decimal amount) => new(amount, "EUR");

        public static bool TryCreate(decimal amount, string currency, out Money? money)
        {
            money = null;
            try
            {
                money = new Money(amount, currency);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Operadores aritméticos
        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Não é possível somar moedas diferentes: {left.Currency} e {right.Currency}");

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Não é possível subtrair moedas diferentes: {left.Currency} e {right.Currency}");

            var result = left.Amount - right.Amount;
            if (result < 0)
                throw new InvalidOperationException("Resultado da subtração não pode ser negativo");

            return new Money(result, left.Currency);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            if (multiplier < 0)
                throw new ArgumentException("Multiplicador não pode ser negativo", nameof(multiplier));

            return new Money(money.Amount * multiplier, money.Currency);
        }

        public static Money operator /(Money money, decimal divisor)
        {
            if (divisor <= 0)
                throw new ArgumentException("Divisor deve ser maior que zero", nameof(divisor));

            return new Money(money.Amount / divisor, money.Currency);
        }

        // Operadores de comparação
        public static bool operator >(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Não é possível comparar moedas diferentes: {left.Currency} e {right.Currency}");

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException($"Não é possível comparar moedas diferentes: {left.Currency} e {right.Currency}");

            return left.Amount < right.Amount;
        }

        public static bool operator >=(Money left, Money right) => left == right || left > right;
        public static bool operator <=(Money left, Money right) => left == right || left < right;

        // IComparable implementation
        public int CompareTo(Money? other)
        {
            if (other == null) return 1;
            
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Não é possível comparar moedas diferentes: {Currency} e {other.Currency}");

            return Amount.CompareTo(other.Amount);
        }

        // Métodos de conveniência
        public bool IsZero() => Amount == 0;
        
        public Money GetPercentage(decimal percentage)
        {
            if (percentage < 0)
                throw new ArgumentException("Percentual não pode ser negativo", nameof(percentage));

            return new Money((Amount * percentage) / 100, Currency);
        }

        public Money AddPercentage(decimal percentage)
        {
            return this + GetPercentage(percentage);
        }

        public Money SubtractPercentage(decimal percentage)
        {
            var discountAmount = GetPercentage(percentage);
            return this - discountAmount;
        }

        public string ToExtendedString()
        {
            var wholePart = (long)Amount;
            var decimalPart = (int)((Amount - wholePart) * 100);

            var wholeText = NumberToWords(wholePart);
            var currencyName = Currency switch
            {
                "BRL" => wholePart == 1 ? "real" : "reais",
                "USD" => wholePart == 1 ? "dólar" : "dólares",
                "EUR" => wholePart == 1 ? "euro" : "euros",
                _ => Currency.ToLowerInvariant()
            };

            if (decimalPart == 0)
                return $"{wholeText} {currencyName}";

            var centText = NumberToWords(decimalPart);
            var centName = decimalPart == 1 ? "centavo" : "centavos";

            return $"{wholeText} {currencyName} e {centText} {centName}";
        }

        private static string NumberToWords(long number)
        {
            if (number == 0) return "zero";
            if (number == 1) return "um";
            if (number == 2) return "dois";
            // Implementação simplificada - pode ser expandida
            return number.ToString();
        }
    }
}
