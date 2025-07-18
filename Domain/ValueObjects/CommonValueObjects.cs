using IntranetDocumentos.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace IntranetDocumentos.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para nome de usuário
    /// </summary>
    public class UserName : ValidatableValueObject, IConvertible<string>
    {
        private static readonly int MaxLength = 100;
        private static readonly int MinLength = 2;
        private static readonly Regex NameRegex = new(@"^[a-zA-ZÀ-ÿ\s]+$", RegexOptions.Compiled);

        public string Value { get; }
        public string FirstName => Value.Split(' ')[0];
        public string LastName => Value.Split(' ').Length > 1 ? Value.Split(' ').Last() : string.Empty;
        public string Initials => string.Join("", Value.Split(' ').Select(n => n.FirstOrDefault()).Where(c => c != default));

        public UserName(string name)
        {
            Value = name?.Trim() ?? string.Empty;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Nome é obrigatório");

            if (Value.Length < MinLength)
                errors.Add($"Nome deve ter pelo menos {MinLength} caracteres");

            if (Value.Length > MaxLength)
                errors.Add($"Nome não pode exceder {MaxLength} caracteres");

            if (!NameRegex.IsMatch(Value))
                errors.Add("Nome deve conter apenas letras e espaços");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(UserName userName) => userName.Value;
        public static explicit operator UserName(string name) => new UserName(name);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para telefone/ramal
    /// </summary>
    public class PhoneNumber : ValidatableValueObject, IConvertible<string>
    {
        private static readonly Regex PhoneRegex = new(@"^[\d\s\(\)\-\+]+$", RegexOptions.Compiled);
        private static readonly int MaxLength = 20;
        private static readonly int MinLength = 4;

        public string Value { get; }
        public string CleanValue { get; }
        public bool IsRamal => CleanValue.Length <= 4;
        public bool IsCellPhone => CleanValue.Length == 11;
        public bool IsFixedPhone => CleanValue.Length == 10;

        public PhoneNumber(string phone)
        {
            Value = phone?.Trim() ?? string.Empty;
            CleanValue = Regex.Replace(Value, @"[^\d]", "");
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CleanValue;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Telefone é obrigatório");

            if (Value.Length < MinLength)
                errors.Add($"Telefone deve ter pelo menos {MinLength} caracteres");

            if (Value.Length > MaxLength)
                errors.Add($"Telefone não pode exceder {MaxLength} caracteres");

            if (!PhoneRegex.IsMatch(Value))
                errors.Add("Telefone contém caracteres inválidos");

            if (CleanValue.Length < 4)
                errors.Add("Telefone deve ter pelo menos 4 dígitos");

            return errors;
        }

        public string ToValue() => Value;

        /// <summary>
        /// Formata o telefone de acordo com o padrão brasileiro
        /// </summary>
        /// <returns>Telefone formatado</returns>
        public string ToFormattedString()
        {
            if (IsRamal)
                return CleanValue;

            if (IsCellPhone)
                return $"({CleanValue.Substring(0, 2)}) {CleanValue.Substring(2, 5)}-{CleanValue.Substring(7, 4)}";

            if (IsFixedPhone)
                return $"({CleanValue.Substring(0, 2)}) {CleanValue.Substring(2, 4)}-{CleanValue.Substring(6, 4)}";

            return Value;
        }

        public static implicit operator string(PhoneNumber phone) => phone.Value;
        public static explicit operator PhoneNumber(string phone) => new PhoneNumber(phone);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para CPF
    /// </summary>
    public class Cpf : ValidatableValueObject, IConvertible<string>
    {
        private static readonly Regex CpfRegex = new(@"^[\d\.\-]+$", RegexOptions.Compiled);

        public string Value { get; }
        public string CleanValue { get; }

        public Cpf(string cpf)
        {
            Value = cpf?.Trim() ?? string.Empty;
            CleanValue = Regex.Replace(Value, @"[^\d]", "");
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CleanValue;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("CPF é obrigatório");

            if (!CpfRegex.IsMatch(Value))
                errors.Add("CPF contém caracteres inválidos");

            if (CleanValue.Length != 11)
                errors.Add("CPF deve ter 11 dígitos");

            if (!IsValidCpf(CleanValue))
                errors.Add("CPF inválido");

            return errors;
        }

        public string ToValue() => Value;

        /// <summary>
        /// Formata o CPF
        /// </summary>
        /// <returns>CPF formatado</returns>
        public string ToFormattedString()
        {
            if (CleanValue.Length == 11)
                return $"{CleanValue.Substring(0, 3)}.{CleanValue.Substring(3, 3)}.{CleanValue.Substring(6, 3)}-{CleanValue.Substring(9, 2)}";

            return Value;
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            var sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);

            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (int.Parse(cpf[9].ToString()) != digit1)
                return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);

            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return int.Parse(cpf[10].ToString()) == digit2;
        }

        public static implicit operator string(Cpf cpf) => cpf.Value;
        public static explicit operator Cpf(string cpf) => new Cpf(cpf);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para nome de departamento
    /// </summary>
    public class DepartmentName : ValidatableValueObject, IConvertible<string>
    {
        private static readonly int MaxLength = 100;
        private static readonly int MinLength = 2;
        private static readonly string[] ValidDepartments = {
            "Pessoal", "Fiscal", "Contábil", "Cadastro", "Apoio", "TI", "Geral"
        };

        public string Value { get; }
        public bool IsGeneral => Value.Equals("Geral", StringComparison.OrdinalIgnoreCase);
        public new bool IsValid => ValidDepartments.Contains(Value, StringComparer.OrdinalIgnoreCase);

        public DepartmentName(string name)
        {
            Value = name?.Trim() ?? string.Empty;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("Nome do departamento é obrigatório");

            if (Value.Length < MinLength)
                errors.Add($"Nome do departamento deve ter pelo menos {MinLength} caracteres");

            if (Value.Length > MaxLength)
                errors.Add($"Nome do departamento não pode exceder {MaxLength} caracteres");

            if (!ValidDepartments.Contains(Value, StringComparer.OrdinalIgnoreCase))
                errors.Add($"Departamento '{Value}' não é válido. Departamentos válidos: {string.Join(", ", ValidDepartments)}");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(DepartmentName departmentName) => departmentName.Value;
        public static explicit operator DepartmentName(string name) => new DepartmentName(name);

        public override string ToString() => Value;
    }

    /// <summary>
    /// Value Object para data/hora
    /// </summary>
    public class BusinessDateTime : ValidatableValueObject, IConvertible<DateTime>
    {
        private static readonly DateTime MinDate = new DateTime(2000, 1, 1);
        private static readonly DateTime MaxDate = new DateTime(2100, 12, 31);

        public DateTime Value { get; }
        public bool IsPast => Value < DateTime.Now;
        public bool IsFuture => Value > DateTime.Now;
        public bool IsToday => Value.Date == DateTime.Today;
        public bool IsBusinessHours => Value.Hour >= 8 && Value.Hour <= 18;
        public bool IsWeekend => Value.DayOfWeek == DayOfWeek.Saturday || Value.DayOfWeek == DayOfWeek.Sunday;

        public BusinessDateTime(DateTime dateTime)
        {
            Value = dateTime;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (Value < MinDate)
                errors.Add($"Data deve ser posterior a {MinDate:dd/MM/yyyy}");

            if (Value > MaxDate)
                errors.Add($"Data deve ser anterior a {MaxDate:dd/MM/yyyy}");

            return errors;
        }

        public DateTime ToValue() => Value;

        /// <summary>
        /// Formata a data para o padrão brasileiro
        /// </summary>
        /// <returns>Data formatada</returns>
        public string ToFormattedString()
        {
            return Value.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Formata apenas a data
        /// </summary>
        /// <returns>Data formatada</returns>
        public string ToDateString()
        {
            return Value.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Formata apenas a hora
        /// </summary>
        /// <returns>Hora formatada</returns>
        public string ToTimeString()
        {
            return Value.ToString("HH:mm");
        }

        public static implicit operator DateTime(BusinessDateTime businessDateTime) => businessDateTime.Value;
        public static explicit operator BusinessDateTime(DateTime dateTime) => new BusinessDateTime(dateTime);

        public override string ToString() => ToFormattedString();
    }

    /// <summary>
    /// Value Object para URL
    /// </summary>
    public class Url : ValidatableValueObject, IConvertible<string>
    {
        private static readonly int MaxLength = 2048;

        public string Value { get; }
        public Uri Uri { get; }
        public string Domain => Uri.Host;
        public string Scheme => Uri.Scheme;
        public bool IsHttps => Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        public bool IsHttp => Scheme.Equals("http", StringComparison.OrdinalIgnoreCase);

        public Url(string url)
        {
            Value = url?.Trim() ?? string.Empty;
            Uri = Uri.TryCreate(Value, UriKind.Absolute, out var uri) ? uri : new Uri("about:blank");
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Value))
                errors.Add("URL é obrigatória");

            if (Value.Length > MaxLength)
                errors.Add($"URL não pode exceder {MaxLength} caracteres");

            if (!Uri.TryCreate(Value, UriKind.Absolute, out var uri))
                errors.Add("URL inválida");

            if (uri != null && !uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && 
                !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                errors.Add("URL deve usar protocolo HTTP ou HTTPS");

            return errors;
        }

        public string ToValue() => Value;

        public static implicit operator string(Url url) => url.Value;
        public static explicit operator Url(string url) => new Url(url);

        public override string ToString() => Value;
    }
}
