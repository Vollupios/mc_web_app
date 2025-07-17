using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Interface para operações de segurança de documentos - ISP aplicado
    /// </summary>
    public interface IDocumentSecurity
    {
        /// <summary>
        /// Verifica se usuário pode acessar documento
        /// </summary>
        Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user);

        /// <summary>
        /// Verifica se usuário pode fazer upload para departamento
        /// </summary>
        Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user);        /// <summary>
        /// Verifica se usuário pode deletar documento
        /// </summary>
        Task<bool> CanUserDeleteDocumentAsync(int documentId, ApplicationUser user);

        /// <summary>
        /// Obtém departamentos que o usuário pode acessar
        /// </summary>
        Task<List<Department>> GetAccessibleDepartmentsAsync(ApplicationUser user);

        /// <summary>
        /// Verifica se usuário está em uma role específica
        /// </summary>
        Task<bool> IsUserInRoleAsync(ApplicationUser user, string role);

        /// <summary>
        /// Verifica se usuário pode acessar uma pasta específica
        /// </summary>
        Task<bool> CanUserAccessFolderAsync(DocumentFolder folder, ApplicationUser user);
    }
}
