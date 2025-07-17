using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Models.ValueObjects;

namespace IntranetDocumentos.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public int DepartmentId { get; set; }
        
        // Navigation properties
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
        
        // Métodos helper para trabalhar com Email Value Object
        public void SetEmail(string emailAddress)
        {
            try
            {
                var emailVO = ValueObjects.Email.Create(emailAddress);
                Email = emailVO.Value;
                NormalizedEmail = emailVO.Value.ToUpperInvariant();
            }
            catch (ArgumentException)
            {
                // Se o email for inválido, mantém o valor original ou usa um padrão
                Email = emailAddress;
                NormalizedEmail = emailAddress?.ToUpperInvariant();
            }
        }
        
        public ValueObjects.Email? GetEmailValueObject()
        {
            if (ValueObjects.Email.TryCreate(Email ?? string.Empty, out var emailVO))
                return emailVO;
            return null;
        }
    }
}
