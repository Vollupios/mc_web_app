using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    public class Document
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        [Required]
        public string UploaderId { get; set; } = string.Empty;
        
        // Se DepartmentId for nulo, o documento é do setor "Geral"
        public int? DepartmentId { get; set; }
        
        // Texto extraído via OCR para indexação e busca
        public string? ContentText { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser Uploader { get; set; } = null!;
        public virtual Department? Department { get; set; }
    }
}
