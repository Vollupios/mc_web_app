using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel para envio de emails pelos administradores
    /// </summary>
    public class SendEmailViewModel
    {
        [Required(ErrorMessage = "O assunto é obrigatório")]
        [StringLength(200, ErrorMessage = "O assunto deve ter no máximo 200 caracteres")]
        [Display(Name = "Assunto")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(5000, ErrorMessage = "A mensagem deve ter no máximo 5000 caracteres")]
        [Display(Name = "Mensagem")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Usar formatação HTML")]
        public bool IsHtml { get; set; } = true;

        [Display(Name = "Tipo de Destinatários")]
        public EmailRecipientType RecipientType { get; set; } = EmailRecipientType.AllUsers;

        [Display(Name = "Departamento Específico")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Emails Específicos")]
        [EmailAddresses(ErrorMessage = "Um ou mais endereços de email são inválidos")]
        public string? SpecificEmails { get; set; }

        // Propriedades auxiliares para a view
        public List<Department>? Departments { get; set; }
        public int TotalRecipients { get; set; }
        public bool EmailSent { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Tipos de destinatários para envio de email
    /// </summary>
    public enum EmailRecipientType
    {
        [Display(Name = "Todos os usuários")]
        AllUsers = 1,
        
        [Display(Name = "Usuários de um departamento")]
        Department = 2,
        
        [Display(Name = "Emails específicos")]
        Specific = 3,
        
        [Display(Name = "Apenas administradores")]
        AdminOnly = 4,
        
        [Display(Name = "Apenas gestores")]
        ManagersOnly = 5
    }

    /// <summary>
    /// Atributo personalizado para validação de múltiplos emails
    /// </summary>
    public class EmailAddressesAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true;

            var emails = value.ToString()?.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e));

            if (emails == null || !emails.Any())
                return true;

            return emails.All(email => IsValidEmail(email));
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
