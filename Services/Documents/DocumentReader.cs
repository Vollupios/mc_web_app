using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Services.Documents;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Implementação das operações de leitura de documentos - ISP aplicado
    /// </summary>
    public class DocumentReader : IDocumentReader
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentReader> _logger;

        public DocumentReader(ApplicationDbContext context, ILogger<DocumentReader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .AsQueryable();

                if (!await IsUserAdminAsync(user))
                {
                    // Usuários normais só veem documentos do seu departamento ou gerais
                    query = query.Where(d => d.DepartmentId == user.DepartmentId || d.DepartmentId == null);
                }

                return await query
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar documentos para usuário {UserId}", user.Id);
                throw;
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            try
            {
                return await _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar documento {DocumentId}", id);
                throw;
            }
        }

        public async Task<List<Document>> SearchDocumentsAsync(string searchTerm, ApplicationUser user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetDocumentsForUserAsync(user);                var query = _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.OriginalFileName.Contains(searchTerm) || 
                               d.StoredFileName.Contains(searchTerm));

                if (!await IsUserAdminAsync(user))
                {
                    query = query.Where(d => d.DepartmentId == user.DepartmentId || d.DepartmentId == null);
                }

                return await query
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar documentos com termo {SearchTerm} para usuário {UserId}", 
                    searchTerm, user.Id);
                throw;
            }
        }

        public async Task<List<Document>> GetDocumentsByDepartmentAsync(int departmentId, ApplicationUser user)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .Where(d => d.DepartmentId == departmentId);

                // Verificar se usuário pode acessar este departamento
                if (!await IsUserAdminAsync(user) && user.DepartmentId != departmentId)
                {
                    _logger.LogWarning("Usuário {UserId} tentou acessar documentos do departamento {DepartmentId} sem permissão", 
                        user.Id, departmentId);
                    return new List<Document>();
                }

                return await query
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar documentos do departamento {DepartmentId} para usuário {UserId}", 
                    departmentId, user.Id);
                throw;
            }
        }

        public async Task<DocumentStatistics> GetDocumentStatisticsAsync(ApplicationUser user)
        {
            try
            {
                var query = _context.Documents.AsQueryable();

                if (!await IsUserAdminAsync(user))
                {
                    query = query.Where(d => d.DepartmentId == user.DepartmentId || d.DepartmentId == null);
                }

                var documents = await query
                    .Include(d => d.Department)
                    .ToListAsync();

                var stats = new DocumentStatistics
                {
                    TotalDocuments = documents.Count,
                    DocumentsThisMonth = documents.Count(d => d.UploadDate.Month == DateTime.Now.Month && 
                                                             d.UploadDate.Year == DateTime.Now.Year),
                    TotalSizeBytes = documents.Sum(d => d.FileSize)
                };                // Documentos por tipo
                stats.DocumentsByType = documents
                    .GroupBy(d => Path.GetExtension(d.OriginalFileName).ToLowerInvariant())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Documentos por departamento
                stats.DocumentsByDepartment = documents
                    .GroupBy(d => d.Department?.Name ?? "Geral")
                    .ToDictionary(g => g.Key, g => g.Count());

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar estatísticas para usuário {UserId}", user.Id);
                throw;
            }
        }

        public async Task<List<Document>> AdvancedSearchAsync(
            string? searchTerm,
            int? departmentId,
            string? contentType,
            DateTime? startDate,
            DateTime? endDate,
            ApplicationUser user)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Uploader)
                    .Include(d => d.Department)
                    .AsQueryable();

                // Permissões
                if (!await IsUserAdminAsync(user))
                {
                    query = query.Where(d => d.DepartmentId == user.DepartmentId || d.DepartmentId == null);
                }

                // Filtros
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(d =>
                        d.OriginalFileName.Contains(searchTerm) ||
                        d.StoredFileName.Contains(searchTerm) ||
                        (d.ContentText != null && d.ContentText.Contains(searchTerm))
                    );
                }
                if (departmentId.HasValue)
                    query = query.Where(d => d.DepartmentId == departmentId);
                if (!string.IsNullOrWhiteSpace(contentType))
                    query = query.Where(d => d.ContentType.Contains(contentType));
                if (startDate.HasValue)
                    query = query.Where(d => d.UploadDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(d => d.UploadDate <= endDate.Value);

                return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na busca avançada de documentos para usuário {UserId}", user.Id);
                throw;
            }
        }

        private async Task<bool> IsUserAdminAsync(ApplicationUser user)
        {
            try
            {
                var userWithRoles = await _context.Users
                    .Where(u => u.Id == user.Id)
                    .FirstOrDefaultAsync();
                
                if (userWithRoles == null) return false;

                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                return userRoles.Contains("Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se usuário {UserId} é admin", user.Id);
                return false;
            }
        }
    }
}
