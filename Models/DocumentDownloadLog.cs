using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    public class DocumentDownloadLog
    {
        public int Id { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        [Required]
        public DateTime DownloadDate { get; set; }
        
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
