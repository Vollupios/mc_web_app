using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Documents
{
    /// <summary>
    /// DTO para criar pasta
    /// </summary>
    public class CreateFolderDTO
    {
        [Required(ErrorMessage = "O nome da pasta é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome da pasta deve ter no máximo 255 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Selecione um departamento")]
        public int DepartmentId { get; set; }

        public int? ParentFolderId { get; set; }

        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// DTO para atualização de pasta
    /// </summary>
    public class UpdateFolderDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da pasta é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome da pasta deve ter no máximo 255 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }

        public int? ParentFolderId { get; set; }
        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// DTO para resposta de pasta
    /// </summary>
    public class FolderDTO : BaseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        
        // Relacionamentos
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public string? ParentFolderName { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        
        // Conteúdo da pasta
        public List<FolderDTO> SubFolders { get; set; } = new();
        public List<DocumentDTO> Documents { get; set; } = new();
        
        // Estatísticas
        public int TotalDocuments { get; set; }
        public int TotalSubFolders { get; set; }
        public long TotalSize { get; set; }
        public string TotalSizeFormatted => FormatBytes(TotalSize);
        
        // Hierarquia
        public string Path { get; set; } = string.Empty;
        public int Level { get; set; }
        
        private static string FormatBytes(long bytes)
        {
            const int unit = 1024;
            if (bytes < unit) return $"{bytes} B";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }

    /// <summary>
    /// DTO para mover pasta
    /// </summary>
    public class MoveFolderDTO
    {
        [Required]
        public int FolderId { get; set; }

        public int? NewParentFolderId { get; set; }

        [Required]
        public int NewDepartmentId { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para árvore de pastas
    /// </summary>
    public class FolderTreeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? ParentFolderId { get; set; }
        public string Path { get; set; } = string.Empty;
        public int Level { get; set; }
        public int DocumentCount { get; set; }
        public int SubFolderCount { get; set; }
        public long TotalSize { get; set; }
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
        public List<FolderTreeDTO> Children { get; set; } = new();
    }

    /// <summary>
    /// DTO para breadcrumb de navegação
    /// </summary>
    public class BreadcrumbDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public bool IsRoot { get; set; }
    }

    /// <summary>
    /// DTO para navegação de pastas
    /// </summary>
    public class FolderNavigationDTO
    {
        public FolderDTO? CurrentFolder { get; set; }
        public List<BreadcrumbDTO> Breadcrumbs { get; set; } = new();
        public List<FolderDTO> SubFolders { get; set; } = new();
        public List<DocumentDTO> Documents { get; set; } = new();
        public FolderPermissionsDTO Permissions { get; set; } = new();
    }

    /// <summary>
    /// DTO para permissões de pasta
    /// </summary>
    public class FolderPermissionsDTO
    {
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreateSubfolder { get; set; }
        public bool CanUploadDocument { get; set; }
        public bool CanMoveFolder { get; set; }
        public bool CanShareFolder { get; set; }
    }
}
