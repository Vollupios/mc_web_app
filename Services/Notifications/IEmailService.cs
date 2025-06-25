using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Notifications
{
    /// <summary>
    /// Interface para serviços de email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia email para um único destinatário
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envia email para múltiplos destinatários
        /// </summary>
        Task<bool> SendEmailToMultipleAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envia email usando template
        /// </summary>
        Task<bool> SendTemplateEmailAsync(string to, string templateName, object model);

        /// <summary>
        /// Testa o envio de email com configurações personalizadas
        /// </summary>
        Task<bool> TestEmailWithConfigAsync(string smtpHost, int smtpPort, string username, string password, 
            bool enableSsl, string fromEmail, string fromName, string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Verifica se o serviço de email está configurado
        /// </summary>
        bool IsConfigured { get; }
    }
}
