using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Interfaces.Repositories;

namespace IntranetDocumentos.Repositories
{
    /// <summary>
    /// Implementação concreta do repositório de documentos
    /// Aplicando Repository Pattern e Single Responsibility Principle
    /// </summary>
    public class DocumentRepository : BaseRepository<Document, int>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Document?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Document>> GetByDepartmentAsync(int? departmentId, string? userId = null)
        {
            var query = _dbSet.AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(d => d.UploaderId == userId);
            }

            return await query
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByFolderAsync(int? folderId)
        {
            return await _dbSet
                .Where(d => d.FolderId == folderId)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, int? departmentId = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => 
                    d.OriginalFileName.Contains(searchTerm) || 
                    (d.Description != null && d.Description.Contains(searchTerm)));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            return await query
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByUploaderAsync(string uploaderId)
        {
            return await _dbSet
                .Where(d => d.UploaderId == uploaderId)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(d => d.UploadDate >= startDate && d.UploadDate <= endDate)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetRecentAsync(int count = 10)
        {
            return await _dbSet
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetMostDownloadedAsync(int count = 10)
        {
            return await _dbSet
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate) // Mudando para UploadDate já que DownloadCount não existe
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> CountByDepartmentAsync(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                return await _dbSet.CountAsync(d => d.DepartmentId == departmentId.Value);
            }
            
            return await _dbSet.CountAsync();
        }

        public async Task<long> GetTotalFileSizeByDepartmentAsync(int? departmentId)
        {
            var query = _dbSet.AsQueryable();
            
            if (departmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            return await query.SumAsync(d => d.FileSize);
        }

        public async Task<Dictionary<string, int>> GetFileTypeStatsAsync()
        {
            return await _dbSet
                .GroupBy(d => d.ContentType ?? "unknown")
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        public async Task<IEnumerable<Document>> GetArchivedAsync()
        {
            return await _dbSet
                .Where(d => d.Status == DocumentStatus.Archived)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByStatusAsync(string status)
        {
            // Usando enum DocumentStatus
            if (Enum.TryParse<DocumentStatus>(status, true, out var documentStatus))
            {
                return await _dbSet
                    .Where(d => d.Status == documentStatus)
                    .Include(d => d.Department)
                    .Include(d => d.Uploader)
                    .Include(d => d.Folder)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            
            return await _dbSet
                .Where(d => d.Status != DocumentStatus.Archived) // Documentos ativos
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }
    }
}
