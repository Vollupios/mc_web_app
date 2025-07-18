using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Departments
{
    /// <summary>
    /// DTO para criar departamento
    /// </summary>
    public class CreateDepartmentDTO
    {
        [Required(ErrorMessage = "O nome do departamento é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome do departamento deve ter no máximo 255 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        [StringLength(10, ErrorMessage = "O código deve ter no máximo 10 caracteres")]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para atualização de departamento
    /// </summary>
    public class UpdateDepartmentDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do departamento é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome do departamento deve ter no máximo 255 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        [StringLength(10, ErrorMessage = "O código deve ter no máximo 10 caracteres")]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para resposta de departamento
    /// </summary>
    public class DepartmentDTO : BaseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        
        // Estatísticas
        public int UserCount { get; set; }
        public int DocumentCount { get; set; }
        public int FolderCount { get; set; }
        public long TotalStorageUsed { get; set; }
        public string TotalStorageFormatted => FormatBytes(TotalStorageUsed);
        public int TotalDownloads { get; set; }
        public int ReuniaoCount { get; set; }
        
        // Atividade
        public DateTime? LastActivity { get; set; }
        public double ActivityScore { get; set; }
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// DTO para estatísticas de departamento
    /// </summary>
    public class DepartmentStatisticsDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int DocumentCount { get; set; }
        public int DownloadCount { get; set; }
        public int ReuniaoCount { get; set; }
        public long StorageUsed { get; set; }
        public double ActivityScore { get; set; }
        public DateTime? LastActivity { get; set; }
        
        // Comparação com outros departamentos
        public double DocumentCountPercentage { get; set; }
        public double StorageUsedPercentage { get; set; }
        public double DownloadCountPercentage { get; set; }
        public double UserCountPercentage { get; set; }
        
        // Métricas mensais
        public List<MonthlyDepartmentStatDTO> MonthlyStats { get; set; } = new();
    }

    /// <summary>
    /// DTO para estatísticas mensais do departamento
    /// </summary>
    public class MonthlyDepartmentStatDTO
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int TotalDownloads { get; set; }
        public int ReunioesCriadas { get; set; }
        public int NewUsers { get; set; }
        public long StorageUsed { get; set; }
    }

    /// <summary>
    /// DTO para atividade do departamento
    /// </summary>
    public class DepartmentActivityDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<DepartmentUserActivityDTO> TopActiveUsers { get; set; } = new();
        public List<DepartmentDocumentActivityDTO> RecentDocuments { get; set; } = new();
        public List<DepartmentReunionActivityDTO> RecentReunions { get; set; } = new();
        public DepartmentStatisticsDTO Statistics { get; set; } = new();
    }

    /// <summary>
    /// DTO para atividade de usuário no departamento
    /// </summary>
    public class DepartmentUserActivityDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DocumentsUploaded { get; set; }
        public int DocumentsDownloaded { get; set; }
        public int ReunioesCriadas { get; set; }
        public double ActivityScore { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// DTO para atividade de documentos no departamento
    /// </summary>
    public class DepartmentDocumentActivityDTO
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UploaderName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public int DownloadCount { get; set; }
        public DateTime? LastDownload { get; set; }
        public long FileSize { get; set; }
    }

    /// <summary>
    /// DTO para atividade de reuniões no departamento
    /// </summary>
    public class DepartmentReunionActivityDTO
    {
        public int ReuniaoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string ResponsavelName { get; set; } = string.Empty;
        public DateTime DataReuniao { get; set; }
        public int ParticipantesCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para permissões de departamento
    /// </summary>
    public class DepartmentPermissionsDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreateFolder { get; set; }
        public bool CanUploadDocument { get; set; }
        public bool CanDownloadDocument { get; set; }
        public bool CanMoveItems { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanViewStatistics { get; set; }
    }
}
