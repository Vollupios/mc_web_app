using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Implementação para segurança e controle de acesso a documentos
    /// Responsabilidade única: validar permissões de acesso
    /// </summary>
    public class DocumentSecurity : IDocumentSecurity
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DocumentSecurity> _logger;

        public DocumentSecurity(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DocumentSecurity> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao verificar acesso a documento {Id}", documentId);
                return false;
            }

            var document = await _context.Documents
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                _logger.LogWarning("Documento não encontrado: {Id}", documentId);
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin e Gestor têm acesso a todos os documentos
            if (userRoles.Contains("Admin") || userRoles.Contains("Gestor"))
            {
                return true;
            }

            // Usuário comum só acessa documentos do seu departamento ou do setor Geral
            var hasAccess = document.DepartmentId == user.DepartmentId || document.DepartmentId == null;
            
            if (!hasAccess)
            {
                _logger.LogWarning("Usuário {UserId} tentou acessar documento {DocId} sem permissão", 
                    user.Id, documentId);
            }

            return hasAccess;
        }

        public async Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao verificar permissão de upload");
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin pode fazer upload para qualquer departamento
            if (userRoles.Contains("Admin"))
            {
                return true;
            }

            // Gestor pode fazer upload para qualquer departamento
            if (userRoles.Contains("Gestor"))
            {
                return true;
            }

            // Usuário comum só pode fazer upload para seu departamento ou setor Geral
            var canUpload = departmentId == user.DepartmentId || departmentId == null;
            
            if (!canUpload)
            {
                _logger.LogWarning("Usuário {UserId} tentou fazer upload para departamento {DeptId} sem permissão", 
                    user.Id, departmentId);
            }

            return canUpload;
        }

        public async Task<bool> CanUserDeleteDocumentAsync(int documentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao verificar permissão de exclusão");
                return false;
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                _logger.LogWarning("Documento não encontrado para verificação de exclusão: {Id}", documentId);
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin pode excluir qualquer documento
            if (userRoles.Contains("Admin"))
            {
                return true;
            }

            // Usuário só pode excluir documentos que ele mesmo fez upload
            var canDelete = document.UploaderId == user.Id;
            
            if (!canDelete)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir documento {DocId} de outro usuário", 
                    user.Id, documentId);
            }

            return canDelete;
        }

        public async Task<List<Department>> GetAccessibleDepartmentsAsync(ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao buscar departamentos acessíveis");
                return new List<Department>();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains("Admin") || userRoles.Contains("Gestor"))
            {
                // Admin e Gestor podem ver todos os departamentos
                return await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            else
            {
                // Usuário comum só vê seu próprio departamento
                return await _context.Departments
                    .Where(d => d.Id == user.DepartmentId)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
        }

        public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string role)
        {
            if (user == null || string.IsNullOrEmpty(role))
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            return userRoles.Contains(role);
        }

        public async Task<bool> CanUserAccessFolderAsync(DocumentFolder folder, ApplicationUser user)
        {
            if (user == null || folder == null)
            {
                _logger.LogWarning("Usuário ou pasta nulos ao verificar acesso");
                return false;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin pode acessar qualquer pasta
            if (userRoles.Contains("Admin"))
            {
                return true;
            }

            // Gestor pode acessar qualquer pasta
            if (userRoles.Contains("Gestor"))
            {
                return true;
            }

            // Se a pasta não tem departamento específico (é geral), todos podem acessar
            if (!folder.DepartmentId.HasValue)
            {
                return true;
            }

            // Usuário comum só pode acessar pastas do seu departamento
            var hasAccess = folder.DepartmentId == user.DepartmentId;
            
            if (!hasAccess)
            {
                _logger.LogWarning("Usuário {UserId} tentou acessar pasta {FolderId} do departamento {DeptId} sem permissão", 
                    user.Id, folder.Id, folder.DepartmentId);
            }

            return hasAccess;
        }
    }
}
