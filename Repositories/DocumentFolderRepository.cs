using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Interfaces.Repositories;

namespace IntranetDocumentos.Repositories
{
    /// <summary>
    /// Implementação do repositório de pastas de documentos
    /// Especializado em operações hierárquicas
    /// </summary>
    public class DocumentFolderRepository : BaseRepository<DocumentFolder, int>, IDocumentFolderRepository
    {
        public DocumentFolderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<DocumentFolder?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<DocumentFolder>> GetByDepartmentAsync(int? departmentId)
        {
            return await _dbSet
                .Where(f => f.DepartmentId == departmentId)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetRootFoldersAsync(int? departmentId)
        {
            return await _dbSet
                .Where(f => f.ParentFolderId == null && f.DepartmentId == departmentId)
                .Include(f => f.Department)
                .Include(f => f.CreatedBy)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetChildrenAsync(int parentId)
        {
            return await _dbSet
                .Where(f => f.ParentFolderId == parentId)
                .Include(f => f.Department)
                .Include(f => f.CreatedBy)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<DocumentFolder?> GetWithChildrenAsync(int id)
        {
            return await _dbSet
                .Include(f => f.ChildFolders)
                    .ThenInclude(c => c.CreatedBy)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<DocumentFolder?> GetWithDocumentsAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Documents)
                    .ThenInclude(d => d.Uploader)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _dbSet.AnyAsync(f => f.ParentFolderId == id);
        }

        public async Task<bool> HasDocumentsAsync(int id)
        {
            return await _context.Documents.AnyAsync(d => d.FolderId == id);
        }

        public async Task<bool> IsDescendantOfAsync(int childId, int ancestorId)
        {
            var child = await GetByIdAsync(childId);
            if (child == null) return false;

            var currentId = child.ParentFolderId;
            while (currentId.HasValue)
            {
                if (currentId.Value == ancestorId)
                    return true;

                var parent = await GetByIdAsync(currentId.Value);
                currentId = parent?.ParentFolderId;
            }

            return false;
        }

        public async Task<int> GetDepthAsync(int id)
        {
            var folder = await GetByIdAsync(id);
            if (folder == null) return -1;

            int depth = 0;
            var currentId = folder.ParentFolderId;
            
            while (currentId.HasValue)
            {
                depth++;
                var parent = await GetByIdAsync(currentId.Value);
                currentId = parent?.ParentFolderId;
            }

            return depth;
        }

        public async Task<IEnumerable<DocumentFolder>> GetBreadcrumbsAsync(int id)
        {
            var breadcrumbs = new List<DocumentFolder>();
            var folder = await GetByIdAsync(id);

            while (folder != null)
            {
                breadcrumbs.Insert(0, folder);
                if (folder.ParentFolderId.HasValue)
                {
                    folder = await GetByIdAsync(folder.ParentFolderId.Value);
                }
                else
                {
                    break;
                }
            }

            return breadcrumbs;
        }

        public async Task<IEnumerable<DocumentFolder>> GetSiblingsAsync(int id)
        {
            var folder = await GetByIdAsync(id);
            if (folder == null) return new List<DocumentFolder>();

            return await _dbSet
                .Where(f => f.ParentFolderId == folder.ParentFolderId && f.Id != id)
                .Include(f => f.Department)
                .Include(f => f.CreatedBy)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> SearchByNameAsync(string name, int? departmentId = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(f => f.Name.Contains(name) || 
                                        (f.Description != null && f.Description.Contains(name)));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(f => f.DepartmentId == departmentId.Value);
            }

            return await query
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetByCreatorAsync(string creatorId)
        {
            return await _dbSet
                .Where(f => f.CreatedById == creatorId)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .Include(f => f.CreatedBy)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
