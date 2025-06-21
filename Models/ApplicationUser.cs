using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public int DepartmentId { get; set; }
        
        // Navigation properties
        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
    }
}
