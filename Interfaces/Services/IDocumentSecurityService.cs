using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Services
{
    /// <summary>
    /// Interface para validações de segurança de documentos
    /// Segregada para responsabilidade única (SRP + ISP)
    /// </summary>
    public interface IDocumentSecurityService
    {
        // Validações de acesso
        Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user);
        Task<bool> CanUserDeleteDocumentAsync(int documentId, ApplicationUser user);
        Task<bool> CanUserEditDocumentAsync(int documentId, ApplicationUser user);
        Task<bool> CanUserDownloadDocumentAsync(int documentId, ApplicationUser user);
        
        // Validações de upload
        Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user);
        Task<bool> CanUserUploadToFolderAsync(int? folderId, ApplicationUser user);
        
        // Validações de pastas
        Task<bool> CanUserAccessFolderAsync(int? folderId, ApplicationUser user);
        Task<bool> CanUserCreateFolderAsync(int? parentFolderId, int? departmentId, ApplicationUser user);
        Task<bool> CanUserDeleteFolderAsync(int folderId, ApplicationUser user);
        
        // Operações de autorização
        Task<IEnumerable<Department>> GetAccessibleDepartmentsAsync(ApplicationUser user);
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName);
        Task<bool> IsUserAdminAsync(ApplicationUser user);
        
        // Validações de arquivo
        bool IsFileTypeAllowed(string fileName);
        bool IsFileSizeAllowed(long fileSize);
        long GetMaxFileSizeForExtension(string extension);
    }
}
