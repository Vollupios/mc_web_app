using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user);
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId);
        Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser);
        Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user);
        Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user);
        Task<string> GetDocumentPhysicalPathAsync(Document document);
        Task<List<Department>> GetDepartmentsForUserAsync(ApplicationUser user);
    }
}
