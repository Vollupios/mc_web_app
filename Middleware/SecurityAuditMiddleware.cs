using System.Security.Claims;
using System.Text.Json;

namespace IntranetDocumentos.Middleware
{
    /// <summary>
    /// 🔒 Middleware para auditoria de segurança - registra ações sensíveis
    /// </summary>
    public class SecurityAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityAuditMiddleware> _logger;

        // Ações que devem ser auditadas
        private static readonly HashSet<string> AuditableActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Account/Login",
            "/Account/Logout", 
            "/Documents/Upload",
            "/Documents/Download",
            "/Documents/Delete",
            "/Admin/CreateUser",
            "/Admin/DeleteUser",
            "/Admin/ChangeUserRole"
        };

        public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestPath = context.Request.Path.Value ?? "";
            var shouldAudit = ShouldAuditRequest(context, requestPath);

            // Log de entrada para ações sensíveis
            if (shouldAudit)
            {
                await LogSecurityEvent(context, "REQUEST_START", requestPath, null);
            }

            try
            {
                await _next(context);

                // Log de sucesso para ações sensíveis
                if (shouldAudit)
                {
                    var duration = DateTime.UtcNow - startTime;
                    await LogSecurityEvent(context, "REQUEST_SUCCESS", requestPath, new
                    {
                        StatusCode = context.Response.StatusCode,
                        Duration = duration.TotalMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                // Log de erro para ações sensíveis
                if (shouldAudit)
                {
                    await LogSecurityEvent(context, "REQUEST_ERROR", requestPath, new
                    {
                        ErrorMessage = ex.Message,
                        ErrorType = ex.GetType().Name
                    });
                }
                throw;
            }
        }

        private static bool ShouldAuditRequest(HttpContext context, string requestPath)
        {
            // Auditar se é uma ação sensível ou se usuário está autenticado
            return AuditableActions.Any(action => requestPath.StartsWith(action, StringComparison.OrdinalIgnoreCase)) ||
                   context.User?.Identity?.IsAuthenticated == true;
        }

        private async Task LogSecurityEvent(HttpContext context, string eventType, string action, object? details)
        {
            await Task.CompletedTask; // Fix CS1998 warning
            
            try
            {
                var auditInfo = new
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = eventType,
                    Action = action,
                    UserId = GetUserId(context),
                    UserEmail = GetUserEmail(context),
                    IpAddress = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Method = context.Request.Method,
                    Details = details
                };

                _logger.LogInformation("🔒 SECURITY_AUDIT: {AuditData}", JsonSerializer.Serialize(auditInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar evento de auditoria");
            }
        }

        private static string GetUserId(HttpContext context)
        {
            return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        }

        private static string GetUserEmail(HttpContext context)
        {
            return context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Verificar headers de proxy primeiro
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    /// <summary>
    /// Extension method para registrar o middleware
    /// </summary>
    public static class SecurityAuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityAudit(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityAuditMiddleware>();
        }
    }
}
