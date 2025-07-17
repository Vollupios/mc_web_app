using System;
using System.Collections.Generic;

namespace IntranetDocumentos.Models.ValueObjects
{
    /// <summary>
    /// Value Object que representa o tamanho de um arquivo
    /// </summary>
    public sealed class FileSize : ValueObject, IComparable<FileSize>
    {
        public long Bytes { get; }

        public FileSize(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentException("Tamanho do arquivo não pode ser negativo", nameof(bytes));

            Bytes = bytes;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Bytes;
        }

        public override string ToString() => ToHumanReadableString();

        // Conversões implícitas
        public static implicit operator long(FileSize fileSize) => fileSize.Bytes;
        
        public static explicit operator FileSize(long bytes) => new(bytes);

        // Factory methods
        public static FileSize FromBytes(long bytes) => new(bytes);
        public static FileSize FromKilobytes(double kilobytes) => new((long)(kilobytes * 1024));
        public static FileSize FromMegabytes(double megabytes) => new((long)(megabytes * 1024 * 1024));
        public static FileSize FromGigabytes(double gigabytes) => new((long)(gigabytes * 1024 * 1024 * 1024));

        public static bool TryCreate(long bytes, out FileSize? fileSize)
        {
            fileSize = null;
            try
            {
                fileSize = new FileSize(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Propriedades calculadas
        public double Kilobytes => Bytes / 1024.0;
        public double Megabytes => Bytes / (1024.0 * 1024.0);
        public double Gigabytes => Bytes / (1024.0 * 1024.0 * 1024.0);
        public double Terabytes => Bytes / (1024.0 * 1024.0 * 1024.0 * 1024.0);

        // Operadores aritméticos
        public static FileSize operator +(FileSize left, FileSize right)
        {
            return new FileSize(left.Bytes + right.Bytes);
        }

        public static FileSize operator -(FileSize left, FileSize right)
        {
            var result = left.Bytes - right.Bytes;
            if (result < 0)
                throw new InvalidOperationException("Resultado da subtração não pode ser negativo");

            return new FileSize(result);
        }

        public static FileSize operator *(FileSize fileSize, double multiplier)
        {
            if (multiplier < 0)
                throw new ArgumentException("Multiplicador não pode ser negativo", nameof(multiplier));

            return new FileSize((long)(fileSize.Bytes * multiplier));
        }

        public static FileSize operator /(FileSize fileSize, double divisor)
        {
            if (divisor <= 0)
                throw new ArgumentException("Divisor deve ser maior que zero", nameof(divisor));

            return new FileSize((long)(fileSize.Bytes / divisor));
        }

        // Operadores de comparação
        public static bool operator >(FileSize left, FileSize right) => left.Bytes > right.Bytes;
        public static bool operator <(FileSize left, FileSize right) => left.Bytes < right.Bytes;
        public static bool operator >=(FileSize left, FileSize right) => left.Bytes >= right.Bytes;
        public static bool operator <=(FileSize left, FileSize right) => left.Bytes <= right.Bytes;

        // IComparable implementation
        public int CompareTo(FileSize? other)
        {
            if (other == null) return 1;
            return Bytes.CompareTo(other.Bytes);
        }

        // Métodos de formatação
        public string ToHumanReadableString(int decimalPlaces = 1)
        {
            if (Bytes == 0) return "0 B";

            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
            var size = (double)Bytes;
            var unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size.ToString($"F{decimalPlaces}")} {units[unitIndex]}";
        }

        public string ToBytesString() => $"{Bytes:N0} bytes";

        public string ToDetailedString()
        {
            if (Bytes < 1024) return ToBytesString();

            var humanReadable = ToHumanReadableString();
            return $"{humanReadable} ({ToBytesString()})";
        }

        // Métodos de conveniência
        public bool IsZero() => Bytes == 0;
        public bool IsEmpty() => IsZero();

        public bool IsLargerThan(FileSize other) => this > other;
        public bool IsSmallerThan(FileSize other) => this < other;

        public FileSize GetPercentage(double percentage)
        {
            if (percentage < 0)
                throw new ArgumentException("Percentual não pode ser negativo", nameof(percentage));

            return new FileSize((long)(Bytes * percentage / 100));
        }

        // Constantes úteis
        public static readonly FileSize Zero = new(0);
        public static readonly FileSize OneKilobyte = new(1024);
        public static readonly FileSize OneMegabyte = new(1024 * 1024);
        public static readonly FileSize OneGigabyte = new(1024 * 1024 * 1024);

        // Validações úteis para upload de arquivos
        public bool IsWithinLimit(FileSize maxSize) => this <= maxSize;
        
        public bool ExceedsLimit(FileSize maxSize) => this > maxSize;

        public static FileSize MaxUploadSize => FromMegabytes(10); // 10MB padrão

        public bool IsValidForUpload() => IsWithinLimit(MaxUploadSize) && !IsZero();
    }
}
