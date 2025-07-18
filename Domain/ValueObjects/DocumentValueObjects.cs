using IntranetDocumentos.Domain.ValueObjects;
using System.IO;
using System.Linq;

namespace IntranetDocumentos.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para nomes de arquivo
    /// </summary>
    public class FileName : ValidatableValueObject, IConvertible<string>
    {
        private static readonly string[] AllowedExtensions = {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".rtf", ".odt", ".ods", ".odp", ".zip", ".rar",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff"
        };

        private static readonly int MaxLength = 255;
        private static readonly int MinLength = 1;

        public string Value { get; }
        public string Extension { get; }
        public string NameWithoutExtension { get; }

        public FileName(string fileName)
        {
            Value = fileName?.Trim() ?? string.Empty;
            Extension = Path.GetExtension(Value).ToLowerInvariant();
            NameWithoutExtension = Path.GetFileNameWithoutExtension(Value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Nome do arquivo é obrigatório");

            if (Value.Length < MinLength)
                errors.Add($"Nome do arquivo deve ter pelo menos {MinLength} caractere");

            if (Value.Length > MaxLength)
                errors.Add($"Nome do arquivo não pode exceder {MaxLength} caracteres");

            if (string.IsNullOrEmpty(Extension))
                errors.Add("Arquivo deve ter uma extensão");

            if (!AllowedExtensions.Contains(Extension))
                errors.Add($"Extensão {Extension} não é permitida");

            var invalidChars = Path.GetInvalidFileNameChars();
            if (Value.Any(c => invalidChars.Contains(c)))
                errors.Add("Nome do arquivo contém caracteres inválidos");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(FileName fileName) => fileName.Value;
        public static explicit operator FileName(string fileName) => new FileName(fileName);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para tamanho de arquivo
    /// </summary>
    public class FileSize : ValidatableValueObject, IConvertible<long>
    {
        private static readonly long MaxSizeInBytes = 10 * 1024 * 1024; // 10MB
        private static readonly long MinSizeInBytes = 1;

        public long Value { get; }
        public double SizeInKB => Value / 1024.0;
        public double SizeInMB => Value / (1024.0 * 1024.0);
        public string FormattedSize => FormatSize(Value);

        public FileSize(long sizeInBytes)
        {
            Value = sizeInBytes;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (Value < MinSizeInBytes)
                errors.Add("Tamanho do arquivo deve ser maior que zero");

            if (Value > MaxSizeInBytes)
                errors.Add($"Arquivo excede o tamanho máximo de {FormatSize(MaxSizeInBytes)}");

            return errors;
        }

        public long ToValue() => Value;

        public static implicit operator long(FileSize fileSize) => fileSize.Value;
        public static explicit operator FileSize(long size) => new FileSize(size);

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} bytes";
        }

        public override string ToString() => FormattedSize;
    }

    /// <summary>
    /// Value Object para descrição de documento
    /// </summary>
    public class DocumentDescription : ValidatableValueObject, IConvertible<string>
    {
        private static readonly int MaxLength = 1000;
        private static readonly int MinLength = 0;

        public string Value { get; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
        public int Length => Value?.Length ?? 0;

        public DocumentDescription(string? description)
        {
            Value = description?.Trim() ?? string.Empty;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (Length > MaxLength)
                errors.Add($"Descrição não pode exceder {MaxLength} caracteres");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(DocumentDescription description) => description.Value;
        public static explicit operator DocumentDescription(string description) => new DocumentDescription(description);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para versão de documento
    /// </summary>
    public class DocumentVersion : ValidatableValueObject, IConvertible<string>
    {
        private static readonly int MaxLength = 20;
        private static readonly string DefaultVersion = "1.0";

        public string Value { get; }
        public bool IsDefault => Value == DefaultVersion;

        public DocumentVersion(string? version)
        {
            Value = string.IsNullOrWhiteSpace(version) ? DefaultVersion : version.Trim();
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Versão é obrigatória");

            if (Value.Length > MaxLength)
                errors.Add($"Versão não pode exceder {MaxLength} caracteres");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(DocumentVersion version) => version.Value;
        public static explicit operator DocumentVersion(string version) => new DocumentVersion(version);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para tipo de conteúdo (MIME type)
    /// </summary>
    public class ContentType : ValidatableValueObject, IConvertible<string>
    {
        private static readonly Dictionary<string, string> AllowedMimeTypes = new()
        {
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".txt", "text/plain" },
            { ".rtf", "application/rtf" },
            { ".zip", "application/zip" },
            { ".rar", "application/x-rar-compressed" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".tiff", "image/tiff" }
        };

        public string Value { get; }
        public string MainType => Value.Split('/')[0];
        public string SubType => Value.Split('/').Length > 1 ? Value.Split('/')[1] : string.Empty;
        public bool IsImage => MainType.Equals("image", StringComparison.OrdinalIgnoreCase);
        public bool IsDocument => MainType.Equals("application", StringComparison.OrdinalIgnoreCase);
        public bool IsText => MainType.Equals("text", StringComparison.OrdinalIgnoreCase);

        public ContentType(string contentType)
        {
            Value = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Tipo de conteúdo é obrigatório");

            if (!Value.Contains('/'))
                errors.Add("Tipo de conteúdo deve estar no formato tipo/subtipo");

            if (!AllowedMimeTypes.Values.Contains(Value))
                errors.Add($"Tipo de conteúdo {Value} não é permitido");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(ContentType contentType) => contentType.Value;
        public static explicit operator ContentType(string contentType) => new ContentType(contentType);

        /// <summary>
        /// Obtém o tipo de conteúdo baseado na extensão do arquivo
        /// </summary>
        /// <param name="extension">Extensão do arquivo</param>
        /// <returns>Tipo de conteúdo correspondente</returns>
        public static ContentType FromExtension(string extension)
        {
            var ext = extension.ToLowerInvariant();
            var mimeType = AllowedMimeTypes.ContainsKey(ext) ? AllowedMimeTypes[ext] : "application/octet-stream";
            return new ContentType(mimeType);
        }

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para caminho de arquivo armazenado
    /// </summary>
    public class StoredFilePath : ValidatableValueObject, IConvertible<string>
    {
        public string Value { get; }
        public string Directory => Path.GetDirectoryName(Value) ?? string.Empty;
        public string FileName => Path.GetFileName(Value);
        public string Extension => Path.GetExtension(Value);
        public bool Exists => File.Exists(Value);

        public StoredFilePath(string filePath)
        {
            Value = filePath?.Trim() ?? string.Empty;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Caminho do arquivo é obrigatório");

            if (!Path.IsPathFullyQualified(Value))
                errors.Add("Caminho deve ser absoluto");

            var invalidChars = Path.GetInvalidPathChars();
            if (Value.Any(c => invalidChars.Contains(c)))
                errors.Add("Caminho contém caracteres inválidos");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(StoredFilePath filePath) => filePath.Value;
        public static explicit operator StoredFilePath(string filePath) => new StoredFilePath(filePath);

        public override string ToString() => Value;
    }
}
