using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Processador base com funcionalidades comuns
    /// Template Method Pattern
    /// </summary>
    public abstract class BaseFileProcessor : IFileProcessor
    {
        protected readonly ILogger<BaseFileProcessor> _logger;
        protected readonly IWebHostEnvironment _environment;

        protected BaseFileProcessor(ILogger<BaseFileProcessor> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public abstract bool CanProcess(string fileExtension);
        public abstract IEnumerable<string> SupportedExtensions { get; }
        public abstract long MaxFileSize { get; }

        public virtual async Task<ProcessedFileResult> ProcessAsync(IFormFile file, string destinationPath)
        {
            var result = new ProcessedFileResult();

            try
            {
                // Validações básicas
                if (!ValidateFile(file, result))
                    return result;

                // Criar diretório se não existir
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Processamento específico do tipo
                await ProcessSpecificAsync(file, destinationPath, result);

                if (result.Success)
                {
                    result.FilePath = destinationPath;
                    result.FileName = Path.GetFileName(destinationPath);
                    result.FileSize = file.Length;
                    result.ContentType = file.ContentType;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar arquivo: {FileName}", file.FileName);
                result.Success = false;
                result.Errors.Add($"Erro interno ao processar arquivo: {ex.Message}");
                return result;
            }
        }

        protected virtual bool ValidateFile(IFormFile file, ProcessedFileResult result)
        {
            if (file == null || file.Length == 0)
            {
                result.Errors.Add("Arquivo não fornecido ou vazio");
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!CanProcess(extension))
            {
                result.Errors.Add($"Tipo de arquivo não suportado: {extension}");
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                result.Errors.Add($"Arquivo muito grande. Tamanho máximo: {MaxFileSize / (1024 * 1024)}MB");
                return false;
            }

            return true;
        }

        protected abstract Task ProcessSpecificAsync(IFormFile file, string destinationPath, ProcessedFileResult result);
    }
}
