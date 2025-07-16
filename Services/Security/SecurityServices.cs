using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Security
{
    /// <summary>
    /// 🔒 Validador de senha customizado com regras rigorosas de segurança
    /// </summary>
    public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        private readonly ILogger<CustomPasswordValidator> _logger;

        public CustomPasswordValidator(ILogger<CustomPasswordValidator> logger)
        {
            _logger = logger;
        }

        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            var errors = new List<IdentityError>();

            try
            {
                // 1. 🔒 Verificar se senha não contém partes do email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailParts = user.Email.Split('@')[0].Split('.');
                    foreach (var part in emailParts)
                    {
                        if (part.Length >= 3 && password.Contains(part, StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add(new IdentityError
                            {
                                Code = "PasswordContainsEmail",
                                Description = "A senha não pode conter partes do seu email."
                            });
                            break;
                        }
                    }
                }

                // 2. 🔒 Verificar senhas comuns conhecidas
                var commonPasswords = new[]
                {
                    "123456", "password", "123456789", "12345678", "12345", "qwerty",
                    "abc123", "password123", "admin", "letmein", "welcome", "monkey",
                    "senha123", "123abc", "admin123", "root", "toor", "pass"
                };

                if (commonPasswords.Any(common => password.Equals(common, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "CommonPassword",
                        Description = "Esta senha é muito comum e não pode ser utilizada."
                    });
                }

                // 3. 🔒 Verificar padrões sequenciais
                if (HasSequentialChars(password))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "SequentialChars",
                        Description = "A senha não pode conter sequências de caracteres (ex: 123, abc, qwe)."
                    });
                }

                // 4. 🔒 Verificar caracteres repetidos
                if (HasRepeatedChars(password))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "RepeatedChars",
                        Description = "A senha não pode ter mais de 2 caracteres iguais consecutivos."
                    });
                }

                // 5. 🔒 Verificar complexidade mínima
                if (!HasMinimumComplexity(password))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "InsufficientComplexity",
                        Description = "A senha deve ter pelo menos 3 dos 4 tipos: maiúscula, minúscula, número, símbolo."
                    });
                }

                // 6. 🔒 Verificar se não é palavra de dicionário
                if (IsDictionaryWord(password))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DictionaryWord",
                        Description = "A senha não pode ser uma palavra comum do dicionário."
                    });
                }

                // Log tentativas de senhas fracas
                if (errors.Any())
                {
                    _logger.LogWarning("Tentativa de senha fraca rejeitada para usuário {Email}: {Errors}", 
                        user.Email, string.Join(", ", errors.Select(e => e.Code)));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na validação de senha para usuário {Email}", user.Email);
                errors.Add(new IdentityError
                {
                    Code = "ValidationError",
                    Description = "Erro interno na validação da senha."
                });
            }

            return Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
        }

        /// <summary>
        /// Verifica se há caracteres sequenciais na senha
        /// </summary>
        private static bool HasSequentialChars(string password)
        {
            const string sequences = "0123456789abcdefghijklmnopqrstuvwxyzqwertyuiopasdfghjklzxcvbnm";
            
            for (int i = 0; i < password.Length - 2; i++)
            {
                var substr = password.Substring(i, 3).ToLowerInvariant();
                if (sequences.Contains(substr))
                    return true;
                
                // Verificar sequência reversa
                var reversed = new string(substr.Reverse().ToArray());
                if (sequences.Contains(reversed))
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Verifica se há caracteres repetidos consecutivos
        /// </summary>
        private static bool HasRepeatedChars(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Verifica complexidade mínima da senha
        /// </summary>
        private static bool HasMinimumComplexity(string password)
        {
            int complexityScore = 0;
            
            if (Regex.IsMatch(password, @"[a-z]")) complexityScore++;      // Minúscula
            if (Regex.IsMatch(password, @"[A-Z]")) complexityScore++;      // Maiúscula
            if (Regex.IsMatch(password, @"[0-9]")) complexityScore++;      // Número
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) complexityScore++; // Símbolo
            
            return complexityScore >= 3;
        }

        /// <summary>
        /// Verifica se é uma palavra comum de dicionário
        /// </summary>
        private static bool IsDictionaryWord(string password)
        {
            // Lista básica de palavras comuns em português
            var commonWords = new[]
            {
                "senha", "password", "brasil", "empresa", "sistema", "usuario", 
                "admin", "root", "teste", "exemplo", "documento", "arquivo",
                "pessoa", "trabalho", "escritorio", "computador", "internet"
            };
            
            return commonWords.Any(word => password.Equals(word, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// 🔒 Serviço para detectar e alertar sobre tentativas de acesso suspeitas
    /// </summary>
    public class SecurityAlertService : ISecurityAlertService
    {
        private readonly ILogger<SecurityAlertService> _logger;
        private static readonly Dictionary<string, List<DateTime>> LoginAttempts = new();
        private static readonly object LockObject = new();

        public SecurityAlertService(ILogger<SecurityAlertService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra tentativa de login e detecta atividade suspeita
        /// </summary>
        public bool CheckSuspiciousActivity(string ipAddress, string email)
        {
            lock (LockObject)
            {
                var key = $"{ipAddress}:{email}";
                var now = DateTime.UtcNow;
                
                if (!LoginAttempts.ContainsKey(key))
                    LoginAttempts[key] = new List<DateTime>();

                // Limpar tentativas antigas (mais de 1 hora)
                LoginAttempts[key].RemoveAll(attempt => now - attempt > TimeSpan.FromHours(1));
                
                // Adicionar tentativa atual
                LoginAttempts[key].Add(now);
                
                // Verificar se há muitas tentativas recentes (mais de 5 em 1 hora)
                if (LoginAttempts[key].Count > 5)
                {
                    _logger.LogWarning("🚨 ATIVIDADE SUSPEITA DETECTADA - IP: {IP}, Email: {Email}, Tentativas: {Count}", 
                        ipAddress, email, LoginAttempts[key].Count);
                    return true;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Limpa dados antigos de tentativas de login (manutenção)
        /// </summary>
        public void CleanupOldAttempts()
        {
            lock (LockObject)
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                var keysToRemove = new List<string>();
                
                foreach (var kvp in LoginAttempts)
                {
                    kvp.Value.RemoveAll(attempt => attempt < cutoff);
                    if (!kvp.Value.Any())
                        keysToRemove.Add(kvp.Key);
                }
                
                foreach (var key in keysToRemove)
                    LoginAttempts.Remove(key);
            }
        }
    }

    /// <summary>
    /// Interface para o serviço de alertas de segurança
    /// </summary>
    public interface ISecurityAlertService
    {
        bool CheckSuspiciousActivity(string ipAddress, string email);
        void CleanupOldAttempts();
    }
}
