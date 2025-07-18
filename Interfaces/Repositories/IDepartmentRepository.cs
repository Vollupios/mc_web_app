using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Repositories
{
    /// <summary>
    /// Interface para repositório de departamentos
    /// Operações específicas de organização empresarial
    /// </summary>
    public interface IDepartmentRepository : IRepository<Department, int>
    {
        // Operações específicas de departamento
        Task<Department?> GetByNameAsync(string name);
        Task<IEnumerable<Department>> GetActiveAsync();
        Task<IEnumerable<Department>> GetWithUsersAsync();
        Task<IEnumerable<Department>> GetWithDocumentsAsync();
        
        // Estatísticas de departamento
        Task<int> GetUserCountAsync(int departmentId);
        Task<int> GetDocumentCountAsync(int departmentId);
        Task<long> GetTotalFileSizeAsync(int departmentId);
        
        // Operações de validação
        Task<bool> HasUsersAsync(int departmentId);
        Task<bool> HasDocumentsAsync(int departmentId);
        Task<bool> CanDeleteAsync(int departmentId);
        
        // Operações de configuração
        Task<Dictionary<string, object>> GetSettingsAsync(int departmentId);
        Task<bool> UpdateSettingsAsync(int departmentId, Dictionary<string, object> settings);
    }
}
