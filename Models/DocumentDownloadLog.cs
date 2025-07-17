using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Modelo para rastrear downloads de documentos para analytics e auditoria
    /// </summary>
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
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(50)]
        public string? SessionId { get; set; }
        
        public bool IsSuccessful { get; set; } = true;
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        public long? FileSizeAtDownload { get; set; }
        
        // Métodos de domínio para criar logs
        public static DocumentDownloadLog CreateSuccessful(int documentId, string userId, string? userAgent, string? ipAddress, long fileSize, string? sessionId = null)
        {
            return new DocumentDownloadLog
            {
                DocumentId = documentId,
                UserId = userId,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                SessionId = sessionId,
                FileSizeAtDownload = fileSize,
                IsSuccessful = true,
                DownloadedAt = DateTime.UtcNow
            };
        }
        
        public static DocumentDownloadLog CreateFailed(int documentId, string userId, string errorMessage, string? userAgent, string? ipAddress, string? sessionId = null)
        {
            return new DocumentDownloadLog
            {
                DocumentId = documentId,
                UserId = userId,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                SessionId = sessionId,
                IsSuccessful = false,
                ErrorMessage = errorMessage,
                DownloadedAt = DateTime.UtcNow
            };
        }
    }
}
