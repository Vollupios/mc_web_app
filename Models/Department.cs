using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    public class Department
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
