namespace IntranetDocumentos.Middleware
{
    /// <summary>
    /// 🔒 Middleware para adicionar headers de segurança automáticos
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 🔒 Headers de segurança básicos
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // 🔒 Content Security Policy básico
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                     "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                     "img-src 'self' data: https:; " +
                     "font-src 'self' https://cdn.jsdelivr.net; " +
                     "connect-src 'self'; " +
                     "frame-ancestors 'none'; " +
                     "base-uri 'self';";
            context.Response.Headers.Append("Content-Security-Policy", csp);
            
            // 🔒 HTTPS Strict Transport Security (apenas em produção)
            if (!context.Request.IsHttps && context.RequestServices.GetService<IWebHostEnvironment>()?.IsProduction() == true)
            {
                context.Response.Headers.Append("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }
            
            // 🔒 Permissions Policy
            context.Response.Headers.Append("Permissions-Policy", 
                "camera=(), microphone=(), geolocation=(), payment=()");
            
            // 🔒 Remover headers que expõem informações do servidor
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            context.Response.Headers.Remove("X-AspNetMvc-Version");

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method para registrar o middleware de headers de segurança
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
