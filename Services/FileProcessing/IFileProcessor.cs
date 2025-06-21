namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Interface para processamento de diferentes tipos de arquivo - Strategy Pattern
    /// Implementa Open/Closed Principle (OCP)
    /// </summary>
    public interface IFileProcessor
    {
        /// <summary>
        /// Verifica se o processador pode processar o tipo de arquivo
        /// </summary>
        bool CanProcess(string fileExtension);

        /// <summary>
        /// Processa o arquivo conforme suas regras específicas
        /// </summary>
        Task<ProcessedFileResult> ProcessAsync(IFormFile file, string destinationPath);

        /// <summary>
        /// Obtém as extensões suportadas
        /// </summary>
        IEnumerable<string> SupportedExtensions { get; }

        /// <summary>
        /// Tamanho máximo permitido para este tipo de arquivo
        /// </summary>
        long MaxFileSize { get; }
    }

    /// <summary>
    /// Resultado do processamento de arquivo
    /// </summary>
    public class ProcessedFileResult
    {
        public bool Success { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
