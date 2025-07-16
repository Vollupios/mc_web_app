using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Security
{
    /// <summary>
    /// 🔴 Serviço de rate limiting baseado em usuário/email com Redis
    /// Suporte a cache distribuído para ambientes em cluster
    /// </summary>
    public interface IUserRateLimitingService
    {
        Task<bool> IsLoginAllowedAsync(string email, string ipAddress);
        Task<bool> IsUploadAllowedAsync(string userId);
        Task RecordLoginAttemptAsync(string email, string ipAddress, bool success);
        Task RecordUploadAttemptAsync(string userId);
        Task<RateLimitStatus> GetUserStatusAsync(string email);
        Task ClearUserLimitAsync(string email); // Para admins
    }

    /// <summary>
    /// 🔴 Implementação Redis-powered do rate limiting por usuário
    /// </summary>
    public class UserRateLimitingService : IUserRateLimitingService
    {
        private readonly ILogger<UserRateLimitingService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _distributedCache;
        
        // Configurações de limite
        private const int MAX_LOGIN_ATTEMPTS = 5;           // 5 tentativas por usuário
        private const int LOGIN_WINDOW_MINUTES = 15;       // Em 15 minutos
        private const int LOGIN_LOCKOUT_MINUTES = 30;      // Bloqueio de 30 minutos
        
        private const int MAX_UPLOADS_PER_USER = 50;       // 50 uploads por usuário
        private const int UPLOAD_WINDOW_MINUTES = 60;      // Em 60 minutos

        public UserRateLimitingService(
            ILogger<UserRateLimitingService> logger, 
            UserManager<ApplicationUser> userManager,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            _userManager = userManager;
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// 🔴 Verifica se usuário pode tentar login (Redis-powered)
        /// </summary>
        public async Task<bool> IsLoginAllowedAsync(string email, string ipAddress)
        {
            try
            {
                var key = $"login_attempts:{email.ToLowerInvariant()}";
                var historyJson = await _distributedCache.GetStringAsync(key);
                
                UserAttemptHistory history;
                if (historyJson != null)
                {
                    history = JsonSerializer.Deserialize<UserAttemptHistory>(historyJson)!;
                }
                else
                {
                    history = new UserAttemptHistory();
                }

                var now = DateTime.UtcNow;
                
                // Limpar tentativas antigas
                history.LoginAttempts.RemoveAll(a => a.Timestamp < now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                
                // Verificar se está em período de lockout
                if (history.IsLockedUntil.HasValue && history.IsLockedUntil > now)
                {
                    var remainingTime = history.IsLockedUntil.Value.Subtract(now);
                    _logger.LogWarning("🔒 Login bloqueado para usuário {Email} por mais {Minutes} minutos. IP: {IP}", 
                        email, Math.Ceiling(remainingTime.TotalMinutes), ipAddress);
                    return false;
                }
                
                // Verificar número de tentativas na janela de tempo
                var recentAttempts = history.LoginAttempts.Count(a => a.Timestamp >= now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                var isAllowed = recentAttempts < MAX_LOGIN_ATTEMPTS;
                
                if (!isAllowed)
                {
                    _logger.LogWarning("🔒 Rate limit atingido para login - Email: {Email}, Tentativas: {Count}/{Max}, IP: {IP}", 
                        email, recentAttempts, MAX_LOGIN_ATTEMPTS, ipAddress);
                }
                else
                {
                    _logger.LogInformation("🔒 Login permitido - Email: {Email}, Tentativas: {Count}/{Max}, IP: {IP}", 
                        email, recentAttempts, MAX_LOGIN_ATTEMPTS, ipAddress);
                }
                
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao verificar rate limit de login para {Email}", email);
                return true; // Em caso de erro, permitir (fail-open)
            }
        }

        /// <summary>
        /// 🔴 Registra tentativa de login (Redis-powered)
        /// </summary>
        public async Task RecordLoginAttemptAsync(string email, string ipAddress, bool success)
        {
            try
            {
                var key = $"login_attempts:{email.ToLowerInvariant()}";
                var historyJson = await _distributedCache.GetStringAsync(key);
                
                UserAttemptHistory history;
                if (historyJson != null)
                {
                    history = JsonSerializer.Deserialize<UserAttemptHistory>(historyJson)!;
                }
                else
                {
                    history = new UserAttemptHistory();
                }

                var now = DateTime.UtcNow;
                
                // Adicionar nova tentativa
                history.LoginAttempts.Add(new LoginAttempt
                {
                    Timestamp = now,
                    IpAddress = ipAddress,
                    Success = success
                });
                
                // Limpar tentativas antigas
                history.LoginAttempts.RemoveAll(a => a.Timestamp < now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                
                // Se login foi bem-sucedido, resetar lockout
                if (success)
                {
                    history.IsLockedUntil = null;
                    _logger.LogInformation("🔒 Login bem-sucedido para {Email} - Rate limit resetado. IP: {IP}", email, ipAddress);
                }
                else
                {
                    // Contar falhas recentes
                    var recentFailures = history.LoginAttempts.Count(a => 
                        !a.Success && a.Timestamp >= now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                    
                    // Se atingiu o limite, aplicar lockout
                    if (recentFailures >= MAX_LOGIN_ATTEMPTS)
                    {
                        history.IsLockedUntil = now.AddMinutes(LOGIN_LOCKOUT_MINUTES);
                        _logger.LogWarning("🔒 USUÁRIO BLOQUEADO - Email: {Email}, Falhas: {Count}, Lockout até: {Until}, IP: {IP}", 
                            email, recentFailures, history.IsLockedUntil, ipAddress);
                    }
                    else
                    {
                        _logger.LogWarning("🔒 Login falhado para {Email} - Tentativas restantes: {Remaining}, IP: {IP}", 
                            email, MAX_LOGIN_ATTEMPTS - recentFailures, ipAddress);
                    }
                }
                
                // Salvar no cache distribuído (Redis) com expiração
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2) // Cache por 2 horas
                };
                
                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(history), options);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao registrar tentativa de login para {Email}", email);
            }
        }

        /// <summary>
        /// 🔴 Verifica se usuário pode fazer upload (Redis-powered)
        /// </summary>
        public async Task<bool> IsUploadAllowedAsync(string userId)
        {
            try
            {
                var key = $"upload_attempts:{userId}";
                var historyJson = await _distributedCache.GetStringAsync(key);
                
                UserUploadHistory history;
                if (historyJson != null)
                {
                    history = JsonSerializer.Deserialize<UserUploadHistory>(historyJson)!;
                }
                else
                {
                    history = new UserUploadHistory();
                }

                var now = DateTime.UtcNow;
                
                // Limpar uploads antigos
                history.UploadAttempts.RemoveAll(u => u.Timestamp < now.AddMinutes(-UPLOAD_WINDOW_MINUTES));
                
                var recentUploads = history.UploadAttempts.Count;
                var isAllowed = recentUploads < MAX_UPLOADS_PER_USER;
                
                if (!isAllowed)
                {
                    _logger.LogWarning("🔒 Rate limit de upload atingido - Usuário: {UserId}, Uploads: {Count}/{Max}", 
                        userId, recentUploads, MAX_UPLOADS_PER_USER);
                }
                
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao verificar rate limit de upload para usuário {UserId}", userId);
                return true; // Em caso de erro, permitir
            }
        }

        /// <summary>
        /// 🔴 Registra tentativa de upload (Redis-powered)
        /// </summary>
        public async Task RecordUploadAttemptAsync(string userId)
        {
            try
            {
                var key = $"upload_attempts:{userId}";
                var historyJson = await _distributedCache.GetStringAsync(key);
                
                UserUploadHistory history;
                if (historyJson != null)
                {
                    history = JsonSerializer.Deserialize<UserUploadHistory>(historyJson)!;
                }
                else
                {
                    history = new UserUploadHistory();
                }

                var now = DateTime.UtcNow;
                
                // Adicionar novo upload
                history.UploadAttempts.Add(new UploadAttempt
                {
                    Timestamp = now
                });
                
                // Limpar uploads antigos
                history.UploadAttempts.RemoveAll(u => u.Timestamp < now.AddMinutes(-UPLOAD_WINDOW_MINUTES));
                
                _logger.LogInformation("🔒 Upload registrado - Usuário: {UserId}, Total na janela: {Count}/{Max}", 
                    userId, history.UploadAttempts.Count, MAX_UPLOADS_PER_USER);
                
                // Salvar no cache distribuído
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };
                
                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(history), options);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao registrar upload para usuário {UserId}", userId);
            }
        }

        /// <summary>
        /// 🔴 Obtém status atual do rate limiting para um usuário (Redis-powered)
        /// </summary>
        public async Task<RateLimitStatus> GetUserStatusAsync(string email)
        {
            try
            {
                var key = $"login_attempts:{email.ToLowerInvariant()}";
                var historyJson = await _distributedCache.GetStringAsync(key);
                
                if (historyJson == null)
                {
                    return new RateLimitStatus
                    {
                        Email = email,
                        IsBlocked = false,
                        AttemptsCount = 0,
                        MaxAttempts = MAX_LOGIN_ATTEMPTS,
                        WindowMinutes = LOGIN_WINDOW_MINUTES,
                        RemainingLockoutTime = null
                    };
                }

                var history = JsonSerializer.Deserialize<UserAttemptHistory>(historyJson)!;
                var now = DateTime.UtcNow;
                
                // Limpar tentativas antigas
                history.LoginAttempts.RemoveAll(a => a.Timestamp < now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                
                var recentAttempts = history.LoginAttempts.Count(a => a.Timestamp >= now.AddMinutes(-LOGIN_WINDOW_MINUTES));
                var isLocked = history.IsLockedUntil.HasValue && history.IsLockedUntil > now;
                
                return new RateLimitStatus
                {
                    Email = email,
                    IsBlocked = isLocked,
                    AttemptsCount = recentAttempts,
                    MaxAttempts = MAX_LOGIN_ATTEMPTS,
                    WindowMinutes = LOGIN_WINDOW_MINUTES,
                    RemainingLockoutTime = isLocked ? history.IsLockedUntil!.Value.Subtract(now) : null,
                    LastAttemptTime = history.LoginAttempts.LastOrDefault()?.Timestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao obter status do usuário {Email}", email);
                return new RateLimitStatus { Email = email, IsBlocked = false };
            }
        }

        /// <summary>
        /// 🔴 Limpa rate limit para um usuário (Admin function)
        /// </summary>
        public async Task ClearUserLimitAsync(string email)
        {
            try
            {
                var key = $"login_attempts:{email.ToLowerInvariant()}";
                await _distributedCache.RemoveAsync(key);
                
                _logger.LogInformation("🔒 Rate limit limpo para usuário {Email} por administrador", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 Erro ao limpar rate limit para usuário {Email}", email);
            }
        }
    }

    /// <summary>
    /// Histórico de tentativas de login de um usuário
    /// </summary>
    public class UserAttemptHistory
    {
        public List<LoginAttempt> LoginAttempts { get; set; } = new();
        public DateTime? IsLockedUntil { get; set; }
    }

    /// <summary>
    /// Histórico de uploads de um usuário
    /// </summary>
    public class UserUploadHistory
    {
        public List<UploadAttempt> UploadAttempts { get; set; } = new();
    }

    /// <summary>
    /// Tentativa individual de login
    /// </summary>
    public class LoginAttempt
    {
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// Tentativa individual de upload
    /// </summary>
    public class UploadAttempt
    {
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Status atual do rate limiting para um usuário
    /// </summary>
    public class RateLimitStatus
    {
        public string Email { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public int AttemptsCount { get; set; }
        public int MaxAttempts { get; set; }
        public int WindowMinutes { get; set; }
        public TimeSpan? RemainingLockoutTime { get; set; }
        public DateTime? LastAttemptTime { get; set; }
    }
}
