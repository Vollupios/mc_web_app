using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Interfaces.Services;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Refactored
{
    /// <summary>
    /// Serviço de segurança de documentos aplicando SRP
    /// Responsabilidade única: validações de segurança e permissões
    /// </summary>
    public class DocumentSecurityService : IDocumentSecurityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentFolderRepository _folderRepository;

        public DocumentSecurityService(
            UserManager<ApplicationUser> userManager,
            IDocumentRepository documentRepository,
            IDocumentFolderRepository folderRepository)
        {
            _userManager = userManager;
            _documentRepository = documentRepository;
            _folderRepository = folderRepository;
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || user == null)
                return false;

            return await CanUserAccessDocumentAsync(user, document);
        }

        public async Task<bool> CanUserDeleteDocumentAsync(int documentId, ApplicationUser user)
        {
            return await CanUserAccessDocumentAsync(documentId, user);
        }

        public async Task<bool> CanUserEditDocumentAsync(int documentId, ApplicationUser user)
        {
            return await CanUserAccessDocumentAsync(documentId, user);
        }

        public async Task<bool> CanUserDownloadDocumentAsync(int documentId, ApplicationUser user)
        {
            return await CanUserAccessDocumentAsync(documentId, user);
        }

        public async Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user)
        {
            if (user == null)
                return false;

            // Admin pode fazer upload em qualquer departamento
            if (await IsUserAdminAsync(user))
                return true;

            // Gestor pode fazer upload em qualquer departamento
            if (await IsUserInRoleAsync(user, "Gestor"))
                return true;

            // Usuário pode fazer upload apenas no seu departamento
            return departmentId == user.DepartmentId;
        }

        public async Task<bool> CanUserUploadToFolderAsync(int? folderId, ApplicationUser user)
        {
            if (user == null)
                return false;

            // Admin pode fazer upload em qualquer lugar
            if (await IsUserAdminAsync(user))
                return true;

            // Se não há pasta, é upload na raiz - verificar departamento
            if (folderId == null)
                return await CanUserUploadToDepartmentAsync(user.DepartmentId, user);

            var folder = await _folderRepository.GetByIdAsync(folderId.Value);
            if (folder == null)
                return false;

            // Verificar se o usuário pode acessar o departamento da pasta
            return await CanUserAccessDepartmentAsync(user, folder.DepartmentId ?? 1);
        }

        public async Task<bool> CanUserAccessFolderAsync(int? folderId, ApplicationUser user)
        {
            if (user == null)
                return false;

            // Admin pode acessar tudo
            if (await IsUserAdminAsync(user))
                return true;

            // Se não há pasta especificada, verificar acesso ao departamento do usuário
            if (folderId == null)
                return true;

            var folder = await _folderRepository.GetByIdAsync(folderId.Value);
            if (folder == null)
                return false;

            return await CanUserAccessDepartmentAsync(user, folder.DepartmentId ?? 1);
        }

        public async Task<bool> CanUserCreateFolderAsync(int? parentFolderId, int? departmentId, ApplicationUser user)
        {
            if (user == null)
                return false;

            // Admin pode criar pasta em qualquer lugar
            if (await IsUserAdminAsync(user))
                return true;

            // Verificar se pode acessar o departamento
            var targetDepartmentId = departmentId;
            if (!targetDepartmentId.HasValue)
                targetDepartmentId = user.DepartmentId;
            if (!targetDepartmentId.HasValue)
                targetDepartmentId = 1;
            
            return await CanUserUploadToDepartmentAsync(targetDepartmentId, user);
        }

        public async Task<bool> CanUserDeleteFolderAsync(int folderId, ApplicationUser user)
        {
            return await CanUserAccessFolderAsync(folderId, user);
        }

        public async Task<IEnumerable<Department>> GetAccessibleDepartmentsAsync(ApplicationUser user)
        {
            // Implementação simples - pode ser expandida com repository específico
            var allDepartments = new List<Department>();
            
            if (await IsUserAdminAsync(user) || await IsUserInRoleAsync(user, "Gestor"))
            {
                // Admin e Gestor veem todos os departamentos
                // Por enquanto retorna lista vazia - implementar com repository específico
            }
            else
            {
                // Usuário comum vê apenas seu departamento
                // Implementar busca do departamento do usuário
            }

            return allDepartments;
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserAdminAsync(ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(user, "Admin");
        }

        public bool IsFileTypeAllowed(string fileName)
        {
            return IsValidFileType(fileName);
        }

        public bool IsFileSizeAllowed(long fileSize)
        {
            return IsValidFileSize(fileSize);
        }

        public long GetMaxFileSizeForExtension(string extension)
        {
            // Máximo 10MB para todos os tipos
            return 10 * 1024 * 1024;
        }

        // Métodos auxiliares privados
        private async Task<bool> CanUserAccessDocumentAsync(ApplicationUser user, Document document)
        {
            if (user == null || document == null)
                return false;

            // Admin pode acessar tudo
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return true;

            // Gestor pode acessar todos os documentos
            if (await _userManager.IsInRoleAsync(user, "Gestor"))
                return true;

            // Usuário pode acessar documentos do seu departamento ou da área geral
            return user.DepartmentId == document.DepartmentId || 
                   document.Department?.Name == "Geral";
        }

        private async Task<bool> CanUserAccessDepartmentAsync(ApplicationUser user, int departmentId)
        {
            if (user == null)
                return false;

            // Admin pode acessar todos os departamentos
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return true;

            // Gestor pode acessar todos os departamentos
            if (await _userManager.IsInRoleAsync(user, "Gestor"))
                return true;

            // Usuário pode acessar apenas seu departamento
            return user.DepartmentId == departmentId;
        }

        private bool IsValidFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            var allowedExtensions = new[]
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".txt", ".rtf", ".jpg", ".jpeg", ".png", ".gif", ".bmp",
                ".zip", ".rar", ".7z"
            };

            return allowedExtensions.Contains(extension);
        }

        private bool IsValidFileSize(long fileSize)
        {
            // Máximo 10MB
            const long maxSize = 10 * 1024 * 1024;
            return fileSize > 0 && fileSize <= maxSize;
        }
    }
}
