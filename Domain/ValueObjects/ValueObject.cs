using System;
using System.Collections.Generic;
using System.Linq;

namespace IntranetDocumentos.Domain.ValueObjects
{
    /// <summary>
    /// Classe base abstrata para implementação de Value Objects
    /// seguindo os princípios do Domain-Driven Design (DDD)
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// Obtém os componentes de igualdade do Value Object.
        /// Implementações devem retornar todos os valores que definem a igualdade.
        /// </summary>
        /// <returns>Uma coleção de objetos que definem a igualdade</returns>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            return Equals(other);
        }

        public bool Equals(ValueObject? other)
        {
            if (other == null)
                return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Where(x => x != null)
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + obj.GetHashCode();
                    }
                });
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Interface para Value Objects que podem ser validados
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Valida o Value Object
        /// </summary>
        /// <returns>True se válido</returns>
        bool IsValid();

        /// <summary>
        /// Obtém os erros de validação
        /// </summary>
        /// <returns>Lista de erros</returns>
        IEnumerable<string> GetValidationErrors();
    }

    /// <summary>
    /// Classe base para Value Objects com validação
    /// </summary>
    public abstract class ValidatableValueObject : ValueObject, IValidatable
    {
        /// <summary>
        /// Construtor que força validação
        /// </summary>
        protected ValidatableValueObject()
        {
            var errors = GetValidationErrors().ToList();
            if (errors.Any())
            {
                throw new ArgumentException($"Value Object inválido: {string.Join(", ", errors)}");
            }
        }

        /// <summary>
        /// Valida o Value Object
        /// </summary>
        /// <returns>True se válido</returns>
        public bool IsValid()
        {
            return !GetValidationErrors().Any();
        }

        /// <summary>
        /// Obtém os erros de validação
        /// </summary>
        /// <returns>Lista de erros</returns>
        public abstract IEnumerable<string> GetValidationErrors();
    }

    /// <summary>
    /// Resultado de validação
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public IEnumerable<string> Errors { get; }

        public ValidationResult(bool isValid, IEnumerable<string> errors)
        {
            IsValid = isValid;
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, Enumerable.Empty<string>());
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult(false, errors);
        }

        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            return new ValidationResult(false, errors);
        }
    }

    /// <summary>
    /// Interface para conversão de Value Objects
    /// </summary>
    /// <typeparam name="T">Tipo de destino</typeparam>
    public interface IConvertible<out T>
    {
        /// <summary>
        /// Converte para o tipo especificado
        /// </summary>
        /// <returns>Valor convertido</returns>
        T ToValue();
    }

    /// <summary>
    /// Interface para criação de Value Objects
    /// </summary>
    /// <typeparam name="TValueObject">Tipo do Value Object</typeparam>
    /// <typeparam name="TValue">Tipo do valor</typeparam>
    public interface IValueObjectFactory<TValueObject, in TValue>
        where TValueObject : ValueObject
    {
        /// <summary>
        /// Cria um Value Object a partir de um valor
        /// </summary>
        /// <param name="value">Valor de entrada</param>
        /// <returns>Value Object criado</returns>
        TValueObject Create(TValue value);

        /// <summary>
        /// Tenta criar um Value Object a partir de um valor
        /// </summary>
        /// <param name="value">Valor de entrada</param>
        /// <param name="valueObject">Value Object criado</param>
        /// <returns>True se criado com sucesso</returns>
        bool TryCreate(TValue value, out TValueObject? valueObject);
    }
}
