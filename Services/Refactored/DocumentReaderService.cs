using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Interfaces.Services;
using IntranetDocumentos.Interfaces.Repositories;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Services.Refactored
{
    /// <summary>
    /// Serviço de leitura de documentos aplicando SRP
    /// Responsabilidade única: operações de consulta/leitura
    /// </summary>
    public class DocumentReaderService : IDocumentReaderService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentSecurityService _securityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentReaderService(
            IDocumentRepository documentRepository,
            IDocumentSecurityService securityService,
            UserManager<ApplicationUser> userManager)
        {
            _documentRepository = documentRepository;
            _securityService = securityService;
            _userManager = userManager;
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _documentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Document>> GetDocumentsForUserAsync(ApplicationUser user)
        {
            var accessibleDepartments = await _securityService.GetAccessibleDepartmentsAsync(user);
            var documents = new List<Document>();

            foreach (var department in accessibleDepartments)
            {
                var deptDocuments = await _documentRepository.GetByDepartmentAsync(department.Id, user.Id);
                documents.AddRange(deptDocuments);
            }

            return documents.OrderByDescending(d => d.UploadDate);
        }

        public async Task<IEnumerable<Document>> GetDocumentsByDepartmentAsync(int? departmentId, ApplicationUser user)
        {
            // Verificar se o usuário tem acesso ao departamento
            if (departmentId.HasValue)
            {
                var accessibleDepartments = await _securityService.GetAccessibleDepartmentsAsync(user);
                if (!accessibleDepartments.Any(d => d.Id == departmentId.Value))
                {
                    return new List<Document>();
                }
            }

            return await _documentRepository.GetByDepartmentAsync(departmentId, user.Id);
        }

        public async Task<IEnumerable<Document>> GetDocumentsByFolderAsync(int? folderId, ApplicationUser user)
        {
            // Verificar se o usuário tem acesso à pasta
            if (folderId.HasValue && !await _securityService.CanUserAccessFolderAsync(folderId, user))
            {
                return new List<Document>();
            }

            return await _documentRepository.GetByFolderAsync(folderId);
        }

        public async Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm, ApplicationUser user, int? departmentId = null)
        {
            // Verificar acesso ao departamento se especificado
            if (departmentId.HasValue)
            {
                var accessibleDepartments = await _securityService.GetAccessibleDepartmentsAsync(user);
                if (!accessibleDepartments.Any(d => d.Id == departmentId.Value))
                {
                    return new List<Document>();
                }
            }

            var results = await _documentRepository.SearchAsync(searchTerm, departmentId);
            
            // Filtrar apenas documentos que o usuário pode acessar
            var accessibleDocuments = new List<Document>();
            foreach (var doc in results)
            {
                if (await _securityService.CanUserAccessDocumentAsync(doc.Id, user))
                {
                    accessibleDocuments.Add(doc);
                }
            }

            return accessibleDocuments;
        }

        public async Task<IEnumerable<Document>> AdvancedSearchAsync(string searchTerm, DateTime? startDate, DateTime? endDate, string? fileType, ApplicationUser user)
        {
            var baseResults = await SearchDocumentsAsync(searchTerm, user);
            var filteredResults = baseResults.AsEnumerable();

            if (startDate.HasValue)
            {
                filteredResults = filteredResults.Where(d => d.UploadDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredResults = filteredResults.Where(d => d.UploadDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(fileType))
            {
                filteredResults = filteredResults.Where(d => 
                    d.ContentType?.Contains(fileType, StringComparison.OrdinalIgnoreCase) == true ||
                    d.OriginalFileName.EndsWith(fileType, StringComparison.OrdinalIgnoreCase));
            }

            return filteredResults.OrderByDescending(d => d.UploadDate);
        }

        public async Task<DocumentStatisticsViewModel> GetDocumentStatisticsAsync(ApplicationUser user)
        {
            var accessibleDepartments = await _securityService.GetAccessibleDepartmentsAsync(user);
            var isAdmin = await _securityService.IsUserAdminAsync(user);

            var stats = new DocumentStatisticsViewModel();

            if (isAdmin)
            {
                // Admin vê estatísticas globais
                stats.TotalDocuments = await _documentRepository.CountAsync();
                var allDocuments = await _documentRepository.GetAllAsync();
                stats.TotalStorageUsed = allDocuments.Sum(d => d.FileSize);
            }
            else
            {
                // Usuário comum vê apenas suas estatísticas
                var userDocuments = await GetDocumentsForUserAsync(user);
                stats.TotalDocuments = userDocuments.Count();
                stats.TotalStorageUsed = userDocuments.Sum(d => d.FileSize);
            }

            var fileTypeStats = await _documentRepository.GetFileTypeStatsAsync();
            // Assumindo que DocumentStatisticsViewModel tem uma propriedade para tipos de arquivo
            
            return stats;
        }

        public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(ApplicationUser user, int count = 10)
        {
            var userDocuments = await GetDocumentsForUserAsync(user);
            return userDocuments.Take(count);
        }

        public async Task<IEnumerable<Document>> GetMostDownloadedAsync(ApplicationUser user, int count = 10)
        {
            var userDocuments = await GetDocumentsForUserAsync(user);
            return userDocuments
                .OrderByDescending(d => d.UploadDate) // Usando UploadDate já que DownloadCount não existe
                .Take(count);
        }
    }
}
