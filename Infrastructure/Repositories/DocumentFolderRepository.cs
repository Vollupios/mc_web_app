using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Data;

namespace IntranetDocumentos.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de pastas de documentos
    /// </summary>
    public class DocumentFolderRepository : IDocumentFolderRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentFolderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentFolder?> GetByIdAsync(int id)
        {
            return await _context.DocumentFolders
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<DocumentFolder?> GetByIdWithChildrenAsync(int id)
        {
            return await _context.DocumentFolders
                .Include(f => f.ChildFolders)
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<DocumentFolder>> GetByDepartmentAsync(int? departmentId)
        {
            return await _context.DocumentFolders
                .Where(f => f.DepartmentId == departmentId)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetRootFoldersAsync(int? departmentId)
        {
            return await _context.DocumentFolders
                .Where(f => f.ParentFolderId == null && f.DepartmentId == departmentId)
                .Include(f => f.Department)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetChildrenAsync(int parentId)
        {
            return await _context.DocumentFolders
                .Where(f => f.ParentFolderId == parentId)
                .Include(f => f.Department)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<DocumentFolder> AddAsync(DocumentFolder folder)
        {
            _context.DocumentFolders.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task<bool> UpdateAsync(DocumentFolder folder)
        {
            try
            {
                _context.DocumentFolders.Update(folder);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var folder = await GetByIdAsync(id);
                if (folder == null) return false;

                _context.DocumentFolders.Remove(folder);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DocumentFolders.AnyAsync(f => f.Id == id);
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _context.DocumentFolders.AnyAsync(f => f.ParentFolderId == id);
        }

        public async Task<bool> HasDocumentsAsync(int id)
        {
            return await _context.Documents.AnyAsync(d => d.FolderId == id);
        }

        // Missing methods from IRepository<DocumentFolder, int>
        public async Task<IEnumerable<DocumentFolder>> GetAllAsync()
        {
            return await _context.DocumentFolders
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> FindAsync(System.Linq.Expressions.Expression<Func<DocumentFolder, bool>> predicate)
        {
            return await _context.DocumentFolders
                .Where(predicate)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .ToListAsync();
        }

        public async Task<DocumentFolder?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<DocumentFolder, bool>> predicate)
        {
            return await _context.DocumentFolders
                .Where(predicate)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<DocumentFolder, bool>>? predicate = null)
        {
            if (predicate != null)
            {
                return await _context.DocumentFolders.CountAsync(predicate);
            }
            
            return await _context.DocumentFolders.CountAsync();
        }

        // Missing methods from IDocumentFolderRepository
        public async Task<DocumentFolder?> GetWithChildrenAsync(int id)
        {
            return await _context.DocumentFolders
                .Include(f => f.ChildFolders)
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<DocumentFolder?> GetWithDocumentsAsync(int id)
        {
            return await _context.DocumentFolders
                .Include(f => f.Documents)
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> IsDescendantOfAsync(int childId, int ancestorId)
        {
            var child = await _context.DocumentFolders
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync(f => f.Id == childId);
            
            if (child == null) return false;
            
            var current = child;
            while (current?.ParentFolderId != null)
            {
                if (current.ParentFolderId == ancestorId)
                    return true;
                
                current = await _context.DocumentFolders
                    .Include(f => f.ParentFolder)
                    .FirstOrDefaultAsync(f => f.Id == current.ParentFolderId);
            }
            
            return false;
        }

        public async Task<int> GetDepthAsync(int id)
        {
            var folder = await _context.DocumentFolders
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (folder == null) return -1;
            
            int depth = 0;
            var current = folder;
            while (current?.ParentFolderId != null)
            {
                depth++;
                current = await _context.DocumentFolders
                    .Include(f => f.ParentFolder)
                    .FirstOrDefaultAsync(f => f.Id == current.ParentFolderId);
            }
            
            return depth;
        }

        public async Task<IEnumerable<DocumentFolder>> GetBreadcrumbsAsync(int id)
        {
            var breadcrumbs = new List<DocumentFolder>();
            var current = await _context.DocumentFolders
                .Include(f => f.ParentFolder)
                .Include(f => f.Department)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            while (current != null)
            {
                breadcrumbs.Insert(0, current);
                if (current.ParentFolderId != null)
                {
                    current = await _context.DocumentFolders
                        .Include(f => f.ParentFolder)
                        .Include(f => f.Department)
                        .FirstOrDefaultAsync(f => f.Id == current.ParentFolderId);
                }
                else
                {
                    current = null;
                }
            }
            
            return breadcrumbs;
        }

        public async Task<IEnumerable<DocumentFolder>> GetSiblingsAsync(int id)
        {
            var folder = await _context.DocumentFolders
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (folder == null) return new List<DocumentFolder>();
            
            return await _context.DocumentFolders
                .Where(f => f.ParentFolderId == folder.ParentFolderId && f.Id != id)
                .Include(f => f.Department)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> SearchByNameAsync(string name, int? departmentId = null)
        {
            var query = _context.DocumentFolders.AsQueryable();
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(f => f.Name.Contains(name));
            }
            
            if (departmentId.HasValue)
            {
                query = query.Where(f => f.DepartmentId == departmentId);
            }
            
            return await query
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentFolder>> GetByCreatorAsync(string creatorId)
        {
            return await _context.DocumentFolders
                .Where(f => f.CreatedById == creatorId)
                .Include(f => f.Department)
                .Include(f => f.ParentFolder)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
    }
}
