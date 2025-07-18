using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Repositories
{
    /// <summary>
    /// Interface para repositório de pastas de documentos
    /// Segregação específica para operações de hierarquia
    /// </summary>
    public interface IDocumentFolderRepository : IRepository<DocumentFolder, int>
    {
        // Operações hierárquicas
        Task<IEnumerable<DocumentFolder>> GetByDepartmentAsync(int? departmentId);
        Task<IEnumerable<DocumentFolder>> GetRootFoldersAsync(int? departmentId);
        Task<IEnumerable<DocumentFolder>> GetChildrenAsync(int parentId);
        Task<DocumentFolder?> GetWithChildrenAsync(int id);
        Task<DocumentFolder?> GetWithDocumentsAsync(int id);
        
        // Operações de validação hierárquica
        Task<bool> HasChildrenAsync(int id);
        Task<bool> HasDocumentsAsync(int id);
        Task<bool> IsDescendantOfAsync(int childId, int ancestorId);
        Task<int> GetDepthAsync(int id);
        
        // Operações de navegação
        Task<IEnumerable<DocumentFolder>> GetBreadcrumbsAsync(int id);
        Task<IEnumerable<DocumentFolder>> GetSiblingsAsync(int id);
        
        // Operações de busca
        Task<IEnumerable<DocumentFolder>> SearchByNameAsync(string name, int? departmentId = null);
        Task<IEnumerable<DocumentFolder>> GetByCreatorAsync(string creatorId);
    }
}
