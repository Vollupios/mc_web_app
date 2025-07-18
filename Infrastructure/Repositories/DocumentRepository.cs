using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Data;

namespace IntranetDocumentos.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de documentos - Repository Pattern
    /// </summary>
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Document?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Document>> GetByDepartmentAsync(int? departmentId, string? userId = null)
        {
            var query = _context.Documents.AsQueryable();

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
            return await _context.Documents
                .Where(d => d.FolderId == folderId)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, int? departmentId = null)
        {
            var query = _context.Documents.AsQueryable();

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

        public async Task<Document> AddAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> UpdateAsync(Document document)
        {
            try
            {
                _context.Documents.Update(document);
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
                var document = await GetByIdAsync(id);
                if (document == null) return false;

                _context.Documents.Remove(document);
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
            return await _context.Documents.AnyAsync(d => d.Id == id);
        }

        public async Task<int> CountByDepartmentAsync(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                return await _context.Documents.CountAsync(d => d.DepartmentId == departmentId.Value);
            }
            
            return await _context.Documents.CountAsync();
        }

        // Missing methods from IRepository<Document, int>
        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> FindAsync(System.Linq.Expressions.Expression<Func<Document, bool>> predicate)
        {
            return await _context.Documents
                .Where(predicate)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .ToListAsync();
        }

        public async Task<Document?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Document, bool>> predicate)
        {
            return await _context.Documents
                .Where(predicate)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Document, bool>>? predicate = null)
        {
            if (predicate != null)
            {
                return await _context.Documents.CountAsync(predicate);
            }
            
            return await _context.Documents.CountAsync();
        }

        // Missing methods from IDocumentRepository
        public async Task<IEnumerable<Document>> GetByUploaderAsync(string uploaderId)
        {
            return await _context.Documents
                .Where(d => d.UploaderId == uploaderId)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Documents
                .Where(d => d.UploadDate >= startDate && d.UploadDate <= endDate)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetRecentAsync(int count = 10)
        {
            return await _context.Documents
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetMostDownloadedAsync(int count = 10)
        {
            var mostDownloaded = await _context.DocumentDownloadLogs
                .Where(dl => dl.IsSuccessful)
                .GroupBy(dl => dl.DocumentId)
                .Select(g => new { DocumentId = g.Key, DownloadCount = g.Count() })
                .OrderByDescending(x => x.DownloadCount)
                .Take(count)
                .ToListAsync();

            var documentIds = mostDownloaded.Select(x => x.DocumentId).ToList();
            var documents = await _context.Documents
                .Where(d => documentIds.Contains(d.Id))
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .ToListAsync();

            // Ordenar pelos downloads (mantendo a ordem da consulta anterior)
            return documents.OrderBy(d => documentIds.IndexOf(d.Id));
        }

        public async Task<long> GetTotalFileSizeByDepartmentAsync(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                return await _context.Documents
                    .Where(d => d.DepartmentId == departmentId.Value)
                    .SumAsync(d => d.FileSize);
            }
            
            return await _context.Documents.SumAsync(d => d.FileSize);
        }

        public async Task<Dictionary<string, int>> GetFileTypeStatsAsync()
        {
            return await _context.Documents
                .GroupBy(d => d.ContentType)
                .Select(g => new { ContentType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ContentType, x => x.Count);
        }

        public async Task<IEnumerable<Document>> GetArchivedAsync()
        {
            return await _context.Documents
                .Where(d => d.Status == DocumentStatus.Archived)
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByStatusAsync(string status)
        {
            if (Enum.TryParse<DocumentStatus>(status, true, out var documentStatus))
            {
                return await _context.Documents
                    .Where(d => d.Status == documentStatus)
                    .Include(d => d.Department)
                    .Include(d => d.Uploader)
                    .Include(d => d.Folder)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            
            return new List<Document>();
        }
    }
}
