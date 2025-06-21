using IntranetDocumentos.Models;
using IntranetDocumentos.Services.Documents;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Interface principal para serviços de documentos
    /// Agrega todas as funcionalidades segregadas seguindo ISP
    /// </summary>
    public interface IDocumentService : IDocumentReader, IDocumentWriter, IDocumentSecurity, IDocumentDownloader
    {
        // Métodos específicos do serviço principal (não duplicados das interfaces base)
        Task<string> GetDocumentPhysicalPathAsync(Document document);
        Task<List<Department>> GetDepartmentsForUserAsync(ApplicationUser user);
        Task<bool> IsFileTypeAllowed(string fileName);
        Task<bool> IsFileSizeAllowed(long fileSize);
    }
}
