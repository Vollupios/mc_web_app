using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Modelo para rastrear downloads de documentos para analytics
    /// </summary>
    public class DocumentDownload
    {
        public int Id { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public DateTime DownloadDate { get; set; }
        
        [StringLength(100)]
        public string UserAgent { get; set; } = string.Empty;
        
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Document Document { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
