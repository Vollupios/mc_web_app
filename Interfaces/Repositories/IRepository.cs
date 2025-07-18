using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Repositories
{
    /// <summary>
    /// Interface base para repositórios - Repository Pattern
    /// Aplicando Generic Repository para reutilização
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <typeparam name="TKey">Tipo da chave primária</typeparam>
    public interface IRepository<T, TKey> where T : class
    {
        // CRUD Operations - Single Responsibility
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(TKey id);
        Task<bool> ExistsAsync(TKey id);
        
        // Query Operations
        Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null);
    }
}
