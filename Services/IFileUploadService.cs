using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Interface para serviços de upload e gerenciamento de arquivos
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Salva uma imagem no servidor
        /// </summary>
        /// <param name="file">Arquivo de imagem</param>
        /// <param name="folder">Pasta de destino</param>
        /// <returns>Caminho relativo do arquivo salvo</returns>
        Task<string> SaveImageAsync(IFormFile file, string folder);

        /// <summary>
        /// Remove uma imagem do servidor
        /// </summary>
        /// <param name="filePath">Caminho do arquivo a ser removido</param>
        /// <returns>True se removido com sucesso</returns>
        Task<bool> DeleteImageAsync(string filePath);

        /// <summary>
        /// Valida se o arquivo é uma imagem válida
        /// </summary>
        /// <param name="file">Arquivo a ser validado</param>
        /// <returns>True se válido</returns>
        bool IsValidImage(IFormFile file);

        /// <summary>
        /// Gera um nome único para o arquivo
        /// </summary>
        /// <param name="originalFileName">Nome original do arquivo</param>
        /// <returns>Nome único gerado</returns>
        string GenerateUniqueFileName(string originalFileName);

        /// <summary>
        /// Obtém o caminho físico completo do arquivo
        /// </summary>
        /// <param name="relativePath">Caminho relativo</param>
        /// <returns>Caminho físico completo</returns>
        string GetPhysicalPath(string relativePath);
    }
}
