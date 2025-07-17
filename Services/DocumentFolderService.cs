using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;

namespace IntranetDocumentos.Services
{
    public interface IDocumentFolderService
    {
        Task<DocumentTreeViewModel> GetDocumentTreeAsync(int? folderId, int? departmentId, string? searchTerm, ApplicationUser user);
        Task<FolderNavigationViewModel> GetFolderNavigationAsync(int? folderId, ApplicationUser user);
        Task<DocumentFolder> CreateFolderAsync(FolderFormViewModel model, ApplicationUser user);
        Task<DocumentFolder> UpdateFolderAsync(FolderFormViewModel model, ApplicationUser user);
        Task<bool> DeleteFolderAsync(int folderId, ApplicationUser user);
        Task<bool> MoveFolderAsync(int folderId, int? targetFolderId, ApplicationUser user);
        Task<bool> MoveDocumentToFolderAsync(int documentId, int? targetFolderId, ApplicationUser user);
        Task<List<DocumentFolder>> GetFoldersForUserAsync(ApplicationUser user, int? departmentId = null);
        Task<List<DocumentFolder>> GetBreadcrumbsAsync(int? folderId);
        Task<bool> CanUserAccessFolderAsync(int folderId, ApplicationUser user);
        Task<bool> CanUserCreateFolderAsync(int? parentFolderId, int? departmentId, ApplicationUser user);
        Task<DocumentFolder?> GetFolderByIdAsync(int folderId);
    }

    public class DocumentFolderService : IDocumentFolderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DocumentFolderService> _logger;

        public DocumentFolderService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DocumentFolderService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<DocumentTreeViewModel> GetDocumentTreeAsync(int? folderId, int? departmentId, string? searchTerm, ApplicationUser user)
        {
            try
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");
                var isManager = userRoles.Contains("Gestor");

                // Construir query de pastas baseada nas permissões
                var foldersQuery = _context.DocumentFolders
                    .Include(f => f.Department)
                    .Include(f => f.CreatedBy)
                    .Where(f => f.IsActive);

                if (!isAdmin && !isManager)
                {
                    // Usuário comum só vê pastas do seu departamento ou globais
                    foldersQuery = foldersQuery.Where(f => 
                        f.DepartmentId == null || 
                        f.DepartmentId == user.DepartmentId);
                }

                if (departmentId.HasValue)
                {
                    foldersQuery = foldersQuery.Where(f => f.DepartmentId == departmentId.Value);
                }

                var folders = await foldersQuery
                    .OrderBy(f => f.Level)
                    .ThenBy(f => f.DisplayOrder)
                    .ThenBy(f => f.Name)
                    .ToListAsync();

                _logger.LogInformation("Pastas encontradas: {Count}, FolderId: {FolderId}, DepartmentId: {DepartmentId}, UserRole: {UserRole}", 
                    folders.Count, folderId, departmentId, isAdmin ? "Admin" : isManager ? "Manager" : "User");

                foreach (var folder in folders)
                {
                    _logger.LogInformation("Pasta: {Name} (ID: {Id}, ParentId: {ParentId}, Level: {Level}, DeptId: {DeptId})", 
                        folder.Name, folder.Id, folder.ParentFolderId, folder.Level, folder.DepartmentId);
                }

                // Construir query de documentos
                var documentsQuery = _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Include(d => d.Folder)
                    .AsQueryable();

                if (!isAdmin && !isManager)
                {
                    documentsQuery = documentsQuery.Where(d => 
                        d.DepartmentId == null || 
                        d.DepartmentId == user.DepartmentId);
                }

                if (folderId.HasValue)
                {
                    documentsQuery = documentsQuery.Where(d => d.FolderId == folderId.Value);
                }
                else
                {
                    documentsQuery = documentsQuery.Where(d => d.FolderId == null);
                }

                if (departmentId.HasValue)
                {
                    documentsQuery = documentsQuery.Where(d => d.DepartmentId == departmentId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    documentsQuery = documentsQuery.Where(d => 
                        d.OriginalFileName.Contains(searchTerm) ||
                        (d.Description != null && d.Description.Contains(searchTerm)) ||
                        (d.ContentText != null && d.ContentText.Contains(searchTerm)));
                }

                var documents = await documentsQuery
                    .OrderBy(d => d.OriginalFileName)
                    .ToListAsync();

                var currentFolder = folderId.HasValue 
                    ? await _context.DocumentFolders.FindAsync(folderId.Value)
                    : null;

                var breadcrumbs = await GetBreadcrumbsAsync(folderId);

                // Filtrar pastas para exibir apenas as do nível atual
                var currentLevelFolders = folderId.HasValue
                    ? folders.Where(f => f.ParentFolderId == folderId.Value).ToList()
                    : folders.Where(f => f.ParentFolderId == null).ToList();

                _logger.LogInformation("Pastas do nível atual: {Count}, FolderId: {FolderId}", 
                    currentLevelFolders.Count, folderId);

                foreach (var folder in currentLevelFolders)
                {
                    _logger.LogInformation("Pasta nível atual: {Name} (ID: {Id})", folder.Name, folder.Id);
                }

                var treeNodes = DocumentFolderTreeNode.BuildTree(currentLevelFolders, documents);

                return new DocumentTreeViewModel
                {
                    RootFolders = treeNodes,
                    RootDocuments = folderId.HasValue ? new List<Document>() : documents.Where(d => d.FolderId == null).ToList(),
                    CurrentFolder = currentFolder,
                    Breadcrumbs = breadcrumbs,
                    CurrentDepartmentId = departmentId,
                    SearchTerm = searchTerm,
                    CanCreateFolders = await CanUserCreateFolderAsync(folderId, departmentId, user),
                    CanUpload = true // TODO: Implementar lógica de permissão de upload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter árvore de documentos");
                throw;
            }
        }

        public async Task<FolderNavigationViewModel> GetFolderNavigationAsync(int? folderId, ApplicationUser user)
        {
            try
            {
                var currentFolder = folderId.HasValue 
                    ? await _context.DocumentFolders
                        .Include(f => f.Department)
                        .FirstOrDefaultAsync(f => f.Id == folderId.Value)
                    : null;

                if (folderId.HasValue && currentFolder == null)
                {
                    throw new ArgumentException("Pasta não encontrada");
                }

                if (currentFolder != null && !await CanUserAccessFolderAsync(currentFolder.Id, user))
                {
                    throw new UnauthorizedAccessException("Acesso negado à pasta");
                }

                var subFolders = await _context.DocumentFolders
                    .Include(f => f.Department)
                    .Where(f => f.ParentFolderId == folderId && f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .ThenBy(f => f.Name)
                    .ToListAsync();

                var documents = await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.FolderId == folderId)
                    .OrderBy(d => d.OriginalFileName)
                    .ToListAsync();

                var breadcrumbs = await GetBreadcrumbsAsync(folderId);

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");

                return new FolderNavigationViewModel
                {
                    CurrentFolderId = folderId,
                    CurrentFolder = currentFolder,
                    Breadcrumbs = breadcrumbs,
                    SubFolders = subFolders,
                    Documents = documents,
                    DepartmentId = currentFolder?.DepartmentId,
                    CanCreateFolder = await CanUserCreateFolderAsync(folderId, currentFolder?.DepartmentId, user),
                    CanEditFolder = currentFolder != null && (isAdmin || currentFolder.CreatedById == user.Id),
                    CanDeleteFolder = currentFolder != null && !currentFolder.IsSystemFolder && 
                                    (isAdmin || currentFolder.CreatedById == user.Id),
                    CanUploadDocument = true,
                    TotalFolders = subFolders.Count,
                    TotalDocuments = documents.Count,
                    TotalSize = documents.Sum(d => d.FileSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter navegação de pasta {FolderId}", folderId);
                throw;
            }
        }

        public async Task<DocumentFolder> CreateFolderAsync(FolderFormViewModel model, ApplicationUser user)
        {
            try
            {
                if (!await CanUserCreateFolderAsync(model.ParentFolderId, model.DepartmentId, user))
                {
                    throw new UnauthorizedAccessException("Permissão negada para criar pasta");
                }

                var parentFolder = model.ParentFolderId.HasValue
                    ? await _context.DocumentFolders.FindAsync(model.ParentFolderId.Value)
                    : null;

                var folder = new DocumentFolder
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    ParentFolderId = model.ParentFolderId,
                    DepartmentId = model.DepartmentId,
                    Color = model.Color,
                    Icon = model.Icon,
                    DisplayOrder = model.DisplayOrder,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Gerar nome único se necessário
                if (parentFolder != null)
                {
                    folder.Name = parentFolder.GenerateUniqueChildName(folder.Name);
                }
                else
                {
                    var existingNames = await _context.DocumentFolders
                        .Where(f => f.ParentFolderId == null && f.DepartmentId == model.DepartmentId)
                        .Select(f => f.Name)
                        .ToListAsync();

                    if (existingNames.Contains(folder.Name))
                    {
                        int counter = 1;
                        string uniqueName;
                        do
                        {
                            uniqueName = $"{folder.Name} ({counter})";
                            counter++;
                        }
                        while (existingNames.Contains(uniqueName));
                        folder.Name = uniqueName;
                    }
                }

                _context.DocumentFolders.Add(folder);
                await _context.SaveChangesAsync();

                // Recarregar a pasta com suas relações para BuildPath
                folder = await _context.DocumentFolders
                    .Include(f => f.ParentFolder)
                    .FirstAsync(f => f.Id == folder.Id);

                // Construir o caminho
                folder.BuildPath();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pasta criada: {Name} (ID: {Id}, Level: {Level}) por {UserId}", 
                    folder.Name, folder.Id, folder.Level, user.Id);

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta");
                throw;
            }
        }

        public async Task<DocumentFolder> UpdateFolderAsync(FolderFormViewModel model, ApplicationUser user)
        {
            try
            {
                if (!model.Id.HasValue)
                    throw new ArgumentException("ID da pasta é obrigatório para edição");

                var folder = await _context.DocumentFolders.FindAsync(model.Id.Value);
                if (folder == null)
                    throw new ArgumentException("Pasta não encontrada");

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && folder.CreatedById != user.Id)
                    throw new UnauthorizedAccessException("Permissão negada para editar pasta");

                folder.Name = model.Name.Trim();
                folder.Description = model.Description?.Trim();
                folder.Color = model.Color;
                folder.Icon = model.Icon;
                folder.DisplayOrder = model.DisplayOrder;
                folder.UpdatedById = user.Id;
                folder.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pasta atualizada: {Name} por {UserId}", folder.Name, user.Id);

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pasta {Id}", model.Id);
                throw;
            }
        }

        public async Task<bool> DeleteFolderAsync(int folderId, ApplicationUser user)
        {
            try
            {
                var folder = await _context.DocumentFolders
                    .Include(f => f.ChildFolders)
                    .Include(f => f.Documents)
                    .FirstOrDefaultAsync(f => f.Id == folderId);

                if (folder == null)
                    return false;

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && folder.CreatedById != user.Id)
                    throw new UnauthorizedAccessException("Permissão negada para excluir pasta");

                if (folder.IsSystemFolder)
                    throw new InvalidOperationException("Não é possível excluir pasta do sistema");

                if (folder.ChildFolders.Any() || folder.Documents.Any())
                    throw new InvalidOperationException("Não é possível excluir pasta que contém itens");

                _context.DocumentFolders.Remove(folder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pasta excluída: {Name} por {UserId}", folder.Name, user.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pasta {Id}", folderId);
                throw;
            }
        }

        public async Task<bool> MoveFolderAsync(int folderId, int? targetFolderId, ApplicationUser user)
        {
            try
            {
                var folder = await _context.DocumentFolders.FindAsync(folderId);
                if (folder == null)
                    return false;

                var targetFolder = targetFolderId.HasValue
                    ? await _context.DocumentFolders.FindAsync(targetFolderId.Value)
                    : null;

                if (!folder.CanMoveTo(targetFolder))
                    throw new InvalidOperationException("Movimento inválido: criaria referência circular");

                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && folder.CreatedById != user.Id)
                    throw new UnauthorizedAccessException("Permissão negada para mover pasta");

                folder.ParentFolderId = targetFolderId;
                folder.UpdatedById = user.Id;
                folder.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reconstruir caminhos
                folder.BuildPath();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pasta movida: {Name} para {Target} por {UserId}", 
                    folder.Name, targetFolder?.Name ?? "Raiz", user.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover pasta {Id}", folderId);
                throw;
            }
        }

        public async Task<bool> MoveDocumentToFolderAsync(int documentId, int? targetFolderId, ApplicationUser user)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return false;

                // Verificar permissões
                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = userRoles.Contains("Admin");
                var isManager = userRoles.Contains("Gestor");

                if (!isAdmin && !isManager && document.UploaderId != user.Id)
                    throw new UnauthorizedAccessException("Permissão negada para mover documento");

                if (targetFolderId.HasValue)
                {
                    var targetFolder = await _context.DocumentFolders.FindAsync(targetFolderId.Value);
                    if (targetFolder == null)
                        throw new ArgumentException("Pasta de destino não encontrada");

                    if (!await CanUserAccessFolderAsync(targetFolder.Id, user))
                        throw new UnauthorizedAccessException("Acesso negado à pasta de destino");
                }

                document.FolderId = targetFolderId;
                document.LastModifiedById = user.Id;
                document.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento movido: {Name} para pasta {FolderId} por {UserId}", 
                    document.OriginalFileName, targetFolderId, user.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover documento {Id}", documentId);
                throw;
            }
        }

        public async Task<List<DocumentFolder>> GetFoldersForUserAsync(ApplicationUser user, int? departmentId = null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");
            var isManager = userRoles.Contains("Gestor");

            var query = _context.DocumentFolders
                .Include(f => f.Department)
                .Where(f => f.IsActive);

            if (!isAdmin && !isManager)
            {
                query = query.Where(f => 
                    f.DepartmentId == null || 
                    f.DepartmentId == user.DepartmentId);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(f => f.DepartmentId == departmentId.Value);
            }

            return await query
                .OrderBy(f => f.Level)
                .ThenBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<List<DocumentFolder>> GetBreadcrumbsAsync(int? folderId)
        {
            if (!folderId.HasValue)
                return new List<DocumentFolder>();

            var folder = await _context.DocumentFolders
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync(f => f.Id == folderId.Value);

            if (folder == null)
                return new List<DocumentFolder>();

            var breadcrumbs = new List<DocumentFolder>();
            var current = folder;

            while (current != null)
            {
                breadcrumbs.Insert(0, current);
                current = current.ParentFolder;
            }

            return breadcrumbs;
        }

        public async Task<bool> CanUserAccessFolderAsync(int folderId, ApplicationUser user)
        {
            var folder = await _context.DocumentFolders.FindAsync(folderId);
            if (folder == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");
            var isManager = userRoles.Contains("Gestor");

            if (isAdmin || isManager)
                return true;

            return folder.DepartmentId == null || folder.DepartmentId == user.DepartmentId;
        }

        public async Task<bool> CanUserCreateFolderAsync(int? parentFolderId, int? departmentId, ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");

            if (isAdmin)
                return true;

            // Verificar se tem permissão no departamento
            if (departmentId.HasValue && departmentId.Value != user.DepartmentId)
                return false;

            // Verificar se tem acesso à pasta pai
            if (parentFolderId.HasValue)
                return await CanUserAccessFolderAsync(parentFolderId.Value, user);

            return true;
        }

        public async Task<DocumentFolder?> GetFolderByIdAsync(int folderId)
        {
            return await _context.DocumentFolders
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == folderId);
        }
    }
}
