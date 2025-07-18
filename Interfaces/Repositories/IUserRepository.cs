using IntranetDocumentos.Models;

namespace IntranetDocumentos.Interfaces.Repositories
{
    /// <summary>
    /// Interface para repositório de usuários
    /// Aplicando ISP para operações específicas de usuário
    /// </summary>
    public interface IUserRepository : IRepository<ApplicationUser, string>
    {
        // Operações específicas de usuário
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByUserNameAsync(string userName);
        Task<IEnumerable<ApplicationUser>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName);
        
        // Operações de autenticação e autorização
        Task<bool> IsInRoleAsync(string userId, string roleName);
        Task<IEnumerable<string>> GetRolesAsync(string userId);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        
        // Estatísticas de usuário
        Task<IEnumerable<ApplicationUser>> GetMostActiveUsersAsync(int count = 10);
        Task<int> CountByDepartmentAsync(int departmentId);
        Task<DateTime?> GetLastLoginAsync(string userId);
        
        // Operações de perfil
        Task<bool> UpdateProfileAsync(ApplicationUser user);
        Task<bool> UpdatePasswordAsync(string userId, string newPasswordHash);
    }
}
