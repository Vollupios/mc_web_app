using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntranetDocumentos.Services.Notifications
{
    /// <summary>
    /// Implementação do serviço de email usando SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient? _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpClient = ConfigureSmtpClient();
        }

        public bool IsConfigured => _smtpClient != null;

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Serviço de email não está configurado. Email não foi enviado para {To}", to);
                return false;
            }

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@empresa.com", 
                                              _configuration["Email:FromName"] ?? "Sistema Intranet");
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await _smtpClient!.SendMailAsync(message);
                _logger.LogInformation("Email enviado com sucesso para {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email para {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailToMultipleAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = true)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Serviço de email não está configurado. Emails não foram enviados");
                return false;
            }

            var results = new List<bool>();
            
            foreach (var recipient in recipients)
            {
                var result = await SendEmailAsync(recipient, subject, body, isHtml);
                results.Add(result);
            }

            return results.All(r => r);
        }

        public async Task<bool> SendTemplateEmailAsync(string to, string templateName, object model)
        {
            var template = GetEmailTemplate(templateName, model);
            return await SendEmailAsync(to, template.Subject, template.Body, true);
        }

        /// <summary>
        /// Testa o envio de email com configurações personalizadas
        /// </summary>
        public async Task<bool> TestEmailWithConfigAsync(string smtpHost, int smtpPort, string username, string password, 
            bool enableSsl, string fromEmail, string fromName, string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = enableSsl;

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await client.SendMailAsync(message);
                _logger.LogInformation("Email de teste enviado com sucesso para {To} usando configurações personalizadas", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de teste com configurações personalizadas para {To}", to);
                return false;
            }
        }

        private SmtpClient? ConfigureSmtpClient()
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPortStr = _configuration["Email:SmtpPort"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];
                var enableSslStr = _configuration["Email:EnableSsl"];

                if (string.IsNullOrEmpty(smtpHost) || 
                    string.IsNullOrEmpty(smtpPortStr) || 
                    string.IsNullOrEmpty(username) || 
                    string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Configurações de email incompletas. Serviço de email desabilitado.");
                    return null;
                }

                if (!int.TryParse(smtpPortStr, out var smtpPort))
                {
                    _logger.LogWarning("Porta SMTP inválida. Usando porta padrão 587.");
                    smtpPort = 587;
                }

                if (!bool.TryParse(enableSslStr, out var enableSsl))
                {
                    enableSsl = true; // Padrão para segurança
                }

                var client = new SmtpClient(smtpHost, smtpPort);
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = enableSsl;

                _logger.LogInformation("Serviço de email configurado com sucesso: {Host}:{Port}", smtpHost, smtpPort);
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar cliente SMTP");
                return null;
            }
        }

        private (string Subject, string Body) GetEmailTemplate(string templateName, object model)
        {
            return templateName.ToLower() switch
            {
                "new_document" => GetNewDocumentTemplate(model),
                "new_meeting" => GetNewMeetingTemplate(model),
                "meeting_cancellation" => GetMeetingCancellationTemplate(model),
                "meeting_update" => GetMeetingUpdateTemplate(model),
                "meeting_reminder" => GetMeetingReminderTemplate(model),
                _ => ("Notificação", "Nova notificação do sistema.")
            };
        }

        private (string Subject, string Body) GetNewDocumentTemplate(object model)
        {
            // Aqui você pode usar um template engine mais sofisticado se necessário
            var subject = "📄 Novo Documento Disponível - Sistema Intranet";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5aa0; border-bottom: 2px solid #2c5aa0; padding-bottom: 10px;'>
                            📄 Novo Documento Disponível
                        </h2>
                        <p>Um novo documento foi adicionado ao sistema:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Documento:</strong> {{FileName}}</p>
                            <p><strong>Departamento:</strong> {{Department}}</p>
                            <p><strong>Enviado por:</strong> {{Uploader}}</p>
                            <p><strong>Data:</strong> {{UploadDate}}</p>
                        </div>
                        <p>Acesse o sistema para visualizar o documento.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Documentos - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }

        private (string Subject, string Body) GetNewMeetingTemplate(object model)
        {
            var subject = "📅 Nova Reunião Agendada - Sistema Intranet";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #28a745; border-bottom: 2px solid #28a745; padding-bottom: 10px;'>
                            📅 Nova Reunião Agendada
                        </h2>
                        <p>Uma nova reunião foi agendada:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Título:</strong> {{Title}}</p>
                            <p><strong>Data:</strong> {{Date}}</p>
                            <p><strong>Horário:</strong> {{StartTime}} às {{EndTime}}</p>
                            <p><strong>Tipo:</strong> {{Type}}</p>
                            <p><strong>Local:</strong> {{Location}}</p>
                            <p><strong>Organizador:</strong> {{Organizer}}</p>
                        </div>
                        <p>Acesse o sistema para mais detalhes da reunião.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }

        private (string Subject, string Body) GetMeetingCancellationTemplate(object model)
        {
            var subject = "❌ Reunião Cancelada - Sistema Intranet";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #dc3545; border-bottom: 2px solid #dc3545; padding-bottom: 10px;'>
                            ❌ Reunião Cancelada
                        </h2>
                        <p>A seguinte reunião foi <strong>cancelada</strong>:</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p><strong>Título:</strong> {{Title}}</p>
                            <p><strong>Data:</strong> {{Date}}</p>
                            <p><strong>Horário:</strong> {{StartTime}} às {{EndTime}}</p>
                            <p><strong>Cancelada por:</strong> {{CancelledBy}}</p>
                        </div>
                        <p>Por favor, atualize sua agenda removendo este compromisso.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }

        private (string Subject, string Body) GetMeetingUpdateTemplate(object model)
        {
            var subject = "🔄 Reunião Atualizada - Sistema Intranet";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #fd7e14; border-bottom: 2px solid #fd7e14; padding-bottom: 10px;'>
                            🔄 Reunião Atualizada
                        </h2>
                        <p>Uma reunião foi atualizada:</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p><strong>Título:</strong> {{Title}}</p>
                            <p><strong>Data:</strong> {{Date}}</p>
                            <p><strong>Horário:</strong> {{StartTime}} às {{EndTime}}</p>
                            <p><strong>Tipo:</strong> {{Type}}</p>
                            <p><strong>Local:</strong> {{Location}}</p>
                            <p><strong>Atualizada por:</strong> {{UpdatedBy}}</p>
                        </div>
                        <p>Acesse o sistema para verificar os detalhes atualizados da reunião.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }

        private (string Subject, string Body) GetMeetingReminderTemplate(object model)
        {
            var subject = "⏰ Lembrete: Reunião Próxima - Sistema Intranet";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #17a2b8; border-bottom: 2px solid #17a2b8; padding-bottom: 10px;'>
                            ⏰ Lembrete de Reunião
                        </h2>
                        <p>Você tem uma reunião agendada em breve:</p>
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8;'>
                            <p><strong>Título:</strong> {{Title}}</p>
                            <p><strong>Data:</strong> {{Date}}</p>
                            <p><strong>Horário:</strong> {{StartTime}} às {{EndTime}}</p>
                            <p><strong>Tipo:</strong> {{Type}}</p>
                            <p><strong>Local:</strong> {{Location}}</p>
                        </div>
                        <p><strong>⚠️ Não se esqueça!</strong> A reunião acontecerá em {{TimeUntil}}.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
