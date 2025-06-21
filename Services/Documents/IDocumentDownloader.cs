using IntranetDocumentos.Models;

namespace IntranetDocumentos.Services.Documents
{
    /// <summary>
    /// Interface para operações de download de documentos - ISP aplicado
    /// </summary>
    public interface IDocumentDownloader
    {
        /// <summary>
        /// Obtém dados do documento para download
        /// </summary>
        Task<(byte[] FileData, string ContentType, string FileName)?> GetDocumentForDownloadAsync(
            int documentId, ApplicationUser user);

        /// <summary>
        /// Obtém stream do documento para visualização/download
        /// </summary>
        Task<Stream?> GetDocumentStreamAsync(int documentId, ApplicationUser user);

        /// <summary>
        /// Obtém caminho físico do documento
        /// </summary>
        Task<string?> GetDocumentPhysicalPathAsync(int documentId);

        /// <summary>
        /// Verifica se documento existe fisicamente
        /// </summary>
        Task<bool> DocumentExistsAsync(int documentId);    }
}
