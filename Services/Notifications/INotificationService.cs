using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Notifications
{
    /// <summary>
    /// Interface para serviços de notificação
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Notifica sobre novo documento
        /// </summary>
        Task NotifyNewDocumentAsync(Document document, ApplicationUser uploader);

        /// <summary>
        /// Notifica sobre nova reunião
        /// </summary>
        Task NotifyNewMeetingAsync(Reuniao reuniao, ApplicationUser organizer);

        /// <summary>
        /// Notifica sobre cancelamento de reunião
        /// </summary>
        Task NotifyMeetingCancellationAsync(Reuniao reuniao, ApplicationUser organizer);

        /// <summary>
        /// Notifica sobre atualização de reunião
        /// </summary>
        Task NotifyMeetingUpdateAsync(Reuniao reuniao, ApplicationUser organizer);

        /// <summary>
        /// Envia lembretes de reuniões próximas
        /// </summary>
        Task SendMeetingRemindersAsync();

        /// <summary>
        /// Obtém usuários que devem receber notificações de documentos
        /// </summary>
        Task<List<ApplicationUser>> GetDocumentNotificationRecipientsAsync(Document document);

        /// <summary>
        /// Obtém usuários que devem receber notificações de reuniões
        /// </summary>
        Task<List<ApplicationUser>> GetMeetingNotificationRecipientsAsync(Reuniao reuniao);
    }
}
