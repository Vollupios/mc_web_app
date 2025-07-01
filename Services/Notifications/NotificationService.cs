using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace IntranetDocumentos.Services.Notifications
{
    /// <summary>
    /// Implementação do serviço de notificações
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task NotifyNewDocumentAsync(Document document, ApplicationUser uploader)
        {
            try
            {
                if (!_emailService.IsConfigured)
                {
                    _logger.LogInformation("Email não configurado. Notificação de documento não enviada.");
                    return;
                }

                var recipients = await GetDocumentNotificationRecipientsAsync(document);
                
                if (!recipients.Any())
                {
                    _logger.LogInformation("Nenhum destinatário encontrado para notificação de documento {DocumentId}", document.Id);
                    return;
                }                var emailAddresses = recipients.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).Cast<string>().ToList();
                
                if (!emailAddresses.Any())
                {
                    _logger.LogWarning("Nenhum email válido encontrado para notificação de documento {DocumentId}", document.Id);
                    return;
                }

                var departmentName = document.Department?.Name ?? "Geral";
                var body = GenerateNewDocumentEmailBody(document.OriginalFileName, departmentName, uploader.Email ?? "", document.UploadDate);

                var success = await _emailService.SendEmailToMultipleAsync(
                    emailAddresses,
                    "📄 Novo Documento Disponível - Sistema Intranet",
                    body,
                    true);

                if (success)
                {
                    _logger.LogInformation("Notificação de novo documento enviada para {Count} usuários", emailAddresses.Count);
                }
                else
                {
                    _logger.LogWarning("Falha ao enviar notificação de novo documento para alguns destinatários");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de novo documento {DocumentId}", document.Id);
            }
        }

        public async Task NotifyNewMeetingAsync(Reuniao reuniao, ApplicationUser organizer)
        {
            try
            {
                if (!_emailService.IsConfigured)
                {
                    _logger.LogInformation("Email não configurado. Notificação de reunião não enviada.");
                    return;
                }

                var recipients = await GetMeetingNotificationRecipientsAsync(reuniao);
                
                if (!recipients.Any())
                {
                    _logger.LogInformation("Nenhum destinatário encontrado para notificação de reunião {MeetingId}", reuniao.Id);
                    return;
                }                var emailAddresses = recipients.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).Cast<string>().ToList();
                
                if (!emailAddresses.Any())
                {
                    _logger.LogWarning("Nenhum email válido encontrado para notificação de reunião {MeetingId}", reuniao.Id);
                    return;
                }

                var body = GenerateNewMeetingEmailBody(reuniao, organizer.Email ?? "");

                var success = await _emailService.SendEmailToMultipleAsync(
                    emailAddresses,
                    "📅 Nova Reunião Agendada - Sistema Intranet",
                    body,
                    true);

                if (success)
                {
                    _logger.LogInformation("Notificação de nova reunião enviada para {Count} usuários", emailAddresses.Count);
                }
                else
                {
                    _logger.LogWarning("Falha ao enviar notificação de nova reunião para alguns destinatários");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de nova reunião {MeetingId}", reuniao.Id);
            }
        }

        public async Task NotifyMeetingCancellationAsync(Reuniao reuniao, ApplicationUser organizer)
        {
            try
            {
                if (!_emailService.IsConfigured)
                {
                    _logger.LogInformation("Email não configurado. Notificação de cancelamento não enviada.");
                    return;
                }

                var recipients = await GetMeetingNotificationRecipientsAsync(reuniao);
                
                if (!recipients.Any())
                {
                    _logger.LogInformation("Nenhum destinatário encontrado para notificação de cancelamento {MeetingId}", reuniao.Id);
                    return;
                }                var emailAddresses = recipients.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).Cast<string>().ToList();
                
                if (!emailAddresses.Any())
                {
                    _logger.LogWarning("Nenhum email válido encontrado para notificação de cancelamento {MeetingId}", reuniao.Id);
                    return;
                }

                var body = GenerateMeetingCancellationEmailBody(reuniao, organizer.Email ?? "");

                var success = await _emailService.SendEmailToMultipleAsync(
                    emailAddresses,
                    "❌ Reunião Cancelada - Sistema Intranet",
                    body,
                    true);

                if (success)
                {
                    _logger.LogInformation("Notificação de cancelamento de reunião enviada para {Count} usuários", emailAddresses.Count);
                }
                else
                {
                    _logger.LogWarning("Falha ao enviar notificação de cancelamento para alguns destinatários");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de cancelamento de reunião {MeetingId}", reuniao.Id);
            }
        }

        public async Task NotifyMeetingUpdateAsync(Reuniao reuniao, ApplicationUser organizer)
        {
            try
            {
                if (!_emailService.IsConfigured)
                {
                    _logger.LogInformation("Email não configurado. Notificação de atualização não enviada.");
                    return;
                }

                var recipients = await GetMeetingNotificationRecipientsAsync(reuniao);
                
                if (!recipients.Any())
                {
                    _logger.LogInformation("Nenhum destinatário encontrado para notificação de atualização {MeetingId}", reuniao.Id);
                    return;
                }                var emailAddresses = recipients.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).Cast<string>().ToList();
                
                if (!emailAddresses.Any())
                {
                    _logger.LogWarning("Nenhum email válido encontrado para notificação de atualização {MeetingId}", reuniao.Id);
                    return;
                }

                var body = GenerateMeetingUpdateEmailBody(reuniao, organizer.Email ?? "");

                var success = await _emailService.SendEmailToMultipleAsync(
                    emailAddresses,
                    "🔄 Reunião Atualizada - Sistema Intranet",
                    body,
                    true);

                if (success)
                {
                    _logger.LogInformation("Notificação de atualização de reunião enviada para {Count} usuários", emailAddresses.Count);
                }
                else
                {
                    _logger.LogWarning("Falha ao enviar notificação de atualização para alguns destinatários");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação de atualização de reunião {MeetingId}", reuniao.Id);
            }
        }

        public async Task SendMeetingRemindersAsync()
        {
            try
            {
                if (!_emailService.IsConfigured)
                {
                    _logger.LogInformation("Email não configurado. Lembretes de reunião não enviados.");
                    return;
                }

                // Buscar reuniões que acontecem nas próximas 24 horas
                var tomorrow = DateTime.Today.AddDays(1);
                var dayAfterTomorrow = DateTime.Today.AddDays(2);

                var upcomingMeetings = await _context.Reunioes
                    .Include(r => r.ResponsavelUser)
                    .Include(r => r.Participantes)
                    .ThenInclude(p => p.Departamento)
                    .Where(r => r.Status == StatusReuniao.Agendada && 
                               r.Data >= tomorrow && 
                               r.Data < dayAfterTomorrow)
                    .ToListAsync();

                if (!upcomingMeetings.Any())
                {
                    _logger.LogInformation("Nenhuma reunião encontrada para envio de lembretes");
                    return;
                }

                var remindersSent = 0;

                foreach (var meeting in upcomingMeetings)
                {
                    var recipients = await GetMeetingNotificationRecipientsAsync(meeting);                    var emailAddresses = recipients.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).Cast<string>().ToList();

                    if (emailAddresses.Any())
                    {
                        var timeUntil = GetTimeUntilMeeting(meeting.Data, meeting.HoraInicio);
                        var body = GenerateMeetingReminderEmailBody(meeting, timeUntil);

                        var success = await _emailService.SendEmailToMultipleAsync(
                            emailAddresses,
                            "⏰ Lembrete: Reunião Próxima - Sistema Intranet",
                            body,
                            true);

                        if (success)
                        {
                            remindersSent++;
                        }
                    }
                }

                _logger.LogInformation("Enviados {Count} lembretes de reunião", remindersSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar lembretes de reunião");
            }
        }

        public async Task<List<ApplicationUser>> GetDocumentNotificationRecipientsAsync(Document document)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Se o documento é de um departamento específico, notificar usuários desse departamento + admins + gestores
                if (document.DepartmentId.HasValue)
                {
                    // Buscar usuários do departamento específico
                    var departmentUsers = await query
                        .Where(u => u.DepartmentId == document.DepartmentId)
                        .ToListAsync();

                    // Buscar admins e gestores
                    var adminAndManagerUsers = new List<ApplicationUser>();
                    foreach (var user in await query.ToListAsync())
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin") || roles.Contains("Gestor"))
                        {
                            adminAndManagerUsers.Add(user);
                        }
                    }

                    // Combinar e remover duplicatas
                    return departmentUsers.Union(adminAndManagerUsers).ToList();
                }
                else
                {
                    // Documento geral - notificar todos os usuários
                    return await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter destinatários para notificação de documento {DocumentId}", document.Id);
                return new List<ApplicationUser>();
            }
        }

        public async Task<List<ApplicationUser>> GetMeetingNotificationRecipientsAsync(Reuniao reuniao)
        {
            try
            {
                var recipients = new List<ApplicationUser>();

                // Adicionar organizador
                if (reuniao.ResponsavelUser != null)
                {
                    recipients.Add(reuniao.ResponsavelUser);
                }

                // Adicionar participantes que têm departamento
                if (reuniao.Participantes?.Any() == true)
                {
                    var departmentIds = reuniao.Participantes
                        .Where(p => p.DepartamentoId.HasValue)
                        .Select(p => p.DepartamentoId!.Value)
                        .Distinct()
                        .ToList();

                    if (departmentIds.Any())
                    {
                        var departmentUsers = await _context.Users
                            .Where(u => departmentIds.Contains(u.DepartmentId))
                            .ToListAsync();
                        
                        recipients.AddRange(departmentUsers);
                    }
                }

                // Adicionar admins e gestores
                var allUsers = await _context.Users.ToListAsync();
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin") || roles.Contains("Gestor"))
                    {
                        recipients.Add(user);
                    }
                }

                // Remover duplicatas
                return recipients.DistinctBy(u => u.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter destinatários para notificação de reunião {MeetingId}", reuniao.Id);
                return new List<ApplicationUser>();
            }
        }

        #region Métodos privados para geração de emails

        private string GenerateNewDocumentEmailBody(string fileName, string department, string uploader, DateTime uploadDate)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5aa0; border-bottom: 2px solid #2c5aa0; padding-bottom: 10px;'>
                            📄 Novo Documento Disponível
                        </h2>
                        <p>Um novo documento foi adicionado ao sistema:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Documento:</strong> {fileName}</p>
                            <p><strong>Departamento:</strong> {department}</p>
                            <p><strong>Enviado por:</strong> {uploader}</p>
                            <p><strong>Data:</strong> {uploadDate:dd/MM/yyyy HH:mm}</p>
                        </div>
                        <p>Acesse o sistema para visualizar o documento.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Documentos - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateNewMeetingEmailBody(Reuniao reuniao, string organizer)
        {
            var location = GetMeetingLocation(reuniao);
            
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #28a745; border-bottom: 2px solid #28a745; padding-bottom: 10px;'>
                            📅 Nova Reunião Agendada
                        </h2>
                        <p>Uma nova reunião foi agendada:</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Título:</strong> {reuniao.Titulo}</p>
                            <p><strong>Data:</strong> {reuniao.Data:dd/MM/yyyy}</p>
                            <p><strong>Horário:</strong> {reuniao.HoraInicio:hh\\:mm} às {reuniao.HoraFim:hh\\:mm}</p>
                            <p><strong>Tipo:</strong> {reuniao.TipoReuniao}</p>
                            <p><strong>Local:</strong> {location}</p>
                            <p><strong>Organizador:</strong> {organizer}</p>
                            {(string.IsNullOrEmpty(reuniao.Observacoes) ? "" : $"<p><strong>Observações:</strong> {reuniao.Observacoes}</p>")}
                        </div>
                        <p>Acesse o sistema para mais detalhes da reunião.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMeetingCancellationEmailBody(Reuniao reuniao, string cancelledBy)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #dc3545; border-bottom: 2px solid #dc3545; padding-bottom: 10px;'>
                            ❌ Reunião Cancelada
                        </h2>
                        <p>A seguinte reunião foi <strong>cancelada</strong>:</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p><strong>Título:</strong> {reuniao.Titulo}</p>
                            <p><strong>Data:</strong> {reuniao.Data:dd/MM/yyyy}</p>
                            <p><strong>Horário:</strong> {reuniao.HoraInicio:hh\\:mm} às {reuniao.HoraFim:hh\\:mm}</p>
                            <p><strong>Cancelada por:</strong> {cancelledBy}</p>
                        </div>
                        <p>Por favor, atualize sua agenda removendo este compromisso.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMeetingUpdateEmailBody(Reuniao reuniao, string updatedBy)
        {
            var location = GetMeetingLocation(reuniao);
            
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #fd7e14; border-bottom: 2px solid #fd7e14; padding-bottom: 10px;'>
                            🔄 Reunião Atualizada
                        </h2>
                        <p>Uma reunião foi atualizada:</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p><strong>Título:</strong> {reuniao.Titulo}</p>
                            <p><strong>Data:</strong> {reuniao.Data:dd/MM/yyyy}</p>
                            <p><strong>Horário:</strong> {reuniao.HoraInicio:hh\\:mm} às {reuniao.HoraFim:hh\\:mm}</p>
                            <p><strong>Tipo:</strong> {reuniao.TipoReuniao}</p>
                            <p><strong>Local:</strong> {location}</p>
                            <p><strong>Atualizada por:</strong> {updatedBy}</p>
                        </div>
                        <p>Acesse o sistema para verificar os detalhes atualizados da reunião.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMeetingReminderEmailBody(Reuniao reuniao, string timeUntil)
        {
            var location = GetMeetingLocation(reuniao);
            
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #17a2b8; border-bottom: 2px solid #17a2b8; padding-bottom: 10px;'>
                            ⏰ Lembrete de Reunião
                        </h2>
                        <p>Você tem uma reunião agendada em breve:</p>
                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8;'>
                            <p><strong>Título:</strong> {reuniao.Titulo}</p>
                            <p><strong>Data:</strong> {reuniao.Data:dd/MM/yyyy}</p>
                            <p><strong>Horário:</strong> {reuniao.HoraInicio:hh\\:mm} às {reuniao.HoraFim:hh\\:mm}</p>
                            <p><strong>Tipo:</strong> {reuniao.TipoReuniao}</p>
                            <p><strong>Local:</strong> {location}</p>
                        </div>
                        <p><strong>⚠️ Não se esqueça!</strong> A reunião acontecerá {timeUntil}.</p>
                        <hr style='margin: 20px 0; border: none; height: 1px; background-color: #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            Sistema de Gestão de Reuniões - Intranet Corporativa
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GetMeetingLocation(Reuniao reuniao)
        {
            return reuniao.TipoReuniao switch
            {
                TipoReuniao.Online => reuniao.LinkOnline ?? "Link não informado",
                TipoReuniao.Interno or TipoReuniao.Externo => reuniao.Sala?.ToString() ?? "Sala não informada",
                _ => "Local não especificado"
            };
        }

        private string GetTimeUntilMeeting(DateTime meetingDate, TimeSpan meetingTime)
        {
            var meetingDateTime = meetingDate.Date.Add(meetingTime);
            var now = DateTime.Now;
            var timeSpan = meetingDateTime - now;

            if (timeSpan.TotalHours < 1)
            {
                return $"em {timeSpan.Minutes} minutos";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"em {(int)timeSpan.TotalHours} horas e {timeSpan.Minutes} minutos";
            }
            else
            {
                return $"em {(int)timeSpan.TotalDays} dias";
            }
        }

        #endregion
    }
}
