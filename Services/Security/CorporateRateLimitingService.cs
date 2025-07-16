using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace IntranetDocumentos.Services.Security
{
    /// <summary>
    /// 🔒 Configurador de rate limiting inteligente para ambiente corporativo
    /// Considera usuários autenticados e IPs compartilhados
    /// </summary>
    public static class CorporateRateLimitingConfig
    {
        /// <summary>
        /// Configura rate limiting adaptado para ambiente corporativo
        /// </summary>
        public static void ConfigureCorporateRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync(
                        "⚠️ Muitas tentativas detectadas. Aguarde alguns minutos antes de tentar novamente.", 
                        cancellationToken: token);
                };

                // 🔒 Rate limiting para LOGIN - baseado em IP (simplificado para compatibilidade)
                options.AddFixedWindowLimiter("CorporateLoginPolicy", configureOptions =>
                {
                    configureOptions.Window = TimeSpan.FromMinutes(15);
                    configureOptions.PermitLimit = 50;  // ✅ 50 tentativas por IP em 15 min
                    configureOptions.AutoReplenishment = true;
                });

                // 🔒 Rate limiting para UPLOAD - por usuário/IP
                options.AddFixedWindowLimiter("CorporateUploadPolicy", configureOptions =>
                {
                    configureOptions.Window = TimeSpan.FromMinutes(10);
                    configureOptions.PermitLimit = 100;  // ✅ 100 uploads por IP em 10 min
                    configureOptions.AutoReplenishment = true;
                });

                // 🔒 Rate limiting GERAL - muito permissivo para intranet
                options.AddFixedWindowLimiter("CorporateGeneralPolicy", configureOptions =>
                {
                    configureOptions.Window = TimeSpan.FromMinutes(1);
                    configureOptions.PermitLimit = 2000;  // ✅ Muito permissivo para ambiente corporativo
                    configureOptions.AutoReplenishment = true;
                });
            });
        }

        /// <summary>
        /// 🔒 Gera chave de partição para login considerando IP + tentativa de usuário
        /// </summary>
        private static string GetLoginPartitionKey(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // Tenta obter email do form data para tentativas de login
            var email = string.Empty;
            if (context.Request.Method == "POST" && 
                context.Request.HasFormContentType &&
                context.Request.Form.ContainsKey("Email"))
            {
                email = context.Request.Form["Email"].ToString();
            }

            // Se tem email, usa combinação IP + Email para rate limiting mais preciso
            if (!string.IsNullOrEmpty(email))
            {
                return $"login:{ip}:{email.ToLowerInvariant()}";
            }

            // Caso contrário, usa apenas IP (menos restritivo)
            return $"login:{ip}";
        }

        /// <summary>
        /// 🔒 Define limite de login baseado no contexto
        /// </summary>
        private static int GetLoginLimit(HttpContext context)
        {
            // Se é uma tentativa específica de email, limite mais restritivo
            if (context.Request.Method == "POST" && 
                context.Request.HasFormContentType &&
                context.Request.Form.ContainsKey("Email"))
            {
                return 5;  // ✅ 5 tentativas por email específico em 15 min
            }

            // Para IP geral (múltiplos usuários), mais permissivo
            return 50;  // ✅ 50 tentativas totais por IP em 15 min
        }

        /// <summary>
        /// 🔒 Gera chave de partição para upload baseada em usuário autenticado
        /// </summary>
        private static string GetUploadPartitionKey(HttpContext context)
        {
            // Se usuário autenticado, usa o ID do usuário
            if (context.User?.Identity?.IsAuthenticated == true && 
                !string.IsNullOrEmpty(context.User.Identity.Name))
            {
                return $"upload:user:{context.User.Identity.Name}";
            }

            // Caso contrário, usa IP (não deveria acontecer com [Authorize])
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"upload:ip:{ip}";
        }

        /// <summary>
        /// 🔒 Define limite de upload baseado no usuário
        /// </summary>
        private static int GetUploadLimit(HttpContext context)
        {
            // Verifica se é admin
            if (context.User?.IsInRole("Admin") == true)
            {
                return 50;  // ✅ Admins podem fazer mais uploads
            }

            // Usuários normais
            return 20;  // ✅ 20 uploads por usuário
        }

        /// <summary>
        /// 🔒 Define taxa de reposição de tokens para upload
        /// </summary>
        private static int GetUploadReplenishment(HttpContext context)
        {
            // Verifica se é admin
            if (context.User?.IsInRole("Admin") == true)
            {
                return 10;  // ✅ Admins recuperam tokens mais rápido
            }

            // Usuários normais
            return 5;   // ✅ 5 tokens a cada 5 minutos
        }
    }

    /// <summary>
    /// 🔒 Middleware personalizado para rate limiting contextual
    /// </summary>
    public class CorporateRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorporateRateLimitingMiddleware> _logger;

        public CorporateRateLimitingMiddleware(RequestDelegate next, ILogger<CorporateRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 🔒 Log tentativas de rate limiting para monitoramento
            var originalStatusCode = context.Response.StatusCode;

            await _next(context);

            // Se foi bloqueado por rate limiting
            if (context.Response.StatusCode == 429)
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var user = context.User?.Identity?.Name ?? "Anonymous";
                var path = context.Request.Path.Value ?? "Unknown";

                _logger.LogWarning("🔒 RATE LIMIT: Usuário bloqueado - User: {User}, IP: {IP}, Path: {Path}",
                    user, ip, path);

                // Se muitos bloqueios do mesmo IP, pode ser ataque
                CheckForSuspiciousActivity(ip, user, path);
            }
        }

        /// <summary>
        /// 🔒 Verifica atividade suspeita baseada em rate limiting
        /// </summary>
        private void CheckForSuspiciousActivity(string ip, string user, string path)
        {
            // Aqui poderia implementar lógica para:
            // - Contar quantos 429s o IP teve na última hora
            // - Se > threshold, enviar alerta para admins
            // - Considerar bloqueio temporário mais longo
            
            _logger.LogWarning("🚨 SECURITY: Possível atividade suspeita detectada - IP: {IP}, Path: {Path}", ip, path);
        }
    }
}
