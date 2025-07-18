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
}
