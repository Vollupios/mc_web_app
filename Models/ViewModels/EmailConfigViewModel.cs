using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel para configuração de email SMTP
    /// </summary>
    public class EmailConfigViewModel
    {
        [Required(ErrorMessage = "O servidor SMTP é obrigatório")]
        [Display(Name = "Servidor SMTP")]
        public string SmtpHost { get; set; } = string.Empty;

        [Required(ErrorMessage = "A porta SMTP é obrigatória")]
        [Range(1, 65535, ErrorMessage = "A porta deve estar entre 1 e 65535")]
        [Display(Name = "Porta SMTP")]
        public int SmtpPort { get; set; } = 587;

        [Required(ErrorMessage = "O usuário SMTP é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Usuário SMTP")]
        public string SmtpUser { get; set; } = string.Empty;

        [Display(Name = "Senha SMTP")]
        [DataType(DataType.Password)]
        public string? SmtpPassword { get; set; }

        [Required(ErrorMessage = "O email de origem é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email de Origem")]
        public string FromEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome de origem é obrigatório")]
        [Display(Name = "Nome de Origem")]
        public string FromName { get; set; } = "Sistema Intranet";

        [Display(Name = "Usar SSL")]
        public bool EnableSsl { get; set; } = true;

        [Display(Name = "Habilitar Notificações")]
        public bool NotificationsEnabled { get; set; } = true;

        [Range(1, 1440, ErrorMessage = "O intervalo deve estar entre 1 e 1440 minutos")]
        [Display(Name = "Intervalo de Lembretes (minutos)")]
        public int ReminderIntervalMinutes { get; set; } = 15;

        [Range(1, 1440, ErrorMessage = "O tempo deve estar entre 1 e 1440 minutos")]
        [Display(Name = "Antecedência dos Lembretes (minutos)")]
        public int ReminderLeadTimeMinutes { get; set; } = 120;

        // Propriedades de notificação específicas
        [Display(Name = "Enviar notificações de documentos")]
        public bool SendDocumentNotifications { get; set; } = true;

        [Display(Name = "Enviar notificações de reuniões")]
        public bool SendMeetingNotifications { get; set; } = true;

        [Display(Name = "Enviar lembretes de reunião")]
        public bool SendMeetingReminders { get; set; } = true;

        [Range(1, 48, ErrorMessage = "O intervalo deve estar entre 1 e 48 horas")]
        [Display(Name = "Intervalo de lembretes (horas)")]
        public int ReminderIntervalHours { get; set; } = 6;

        // Propriedades auxiliares
        public bool ConfigurationExists { get; set; }
        public bool TestEmailSent { get; set; }
        public string? TestResult { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
