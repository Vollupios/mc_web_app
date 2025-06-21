using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Processador para arquivos compactados - Strategy Pattern
    /// </summary>
    public class ArchiveFileProcessor : BaseFileProcessor
    {
        public ArchiveFileProcessor(ILogger<BaseFileProcessor> logger, IWebHostEnvironment environment) 
            : base(logger, environment) { }

        public override bool CanProcess(string fileExtension)
        {
            return SupportedExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public override IEnumerable<string> SupportedExtensions => 
            new[] { ".zip", ".rar", ".7z", ".tar", ".gz" };

        public override long MaxFileSize => 50 * 1024 * 1024; // 50MB

        protected override async Task ProcessSpecificAsync(IFormFile file, string destinationPath, ProcessedFileResult result)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Salvar arquivo
                using var fileStream = new FileStream(destinationPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                // Adicionar metadados específicos para arquivos
                result.Metadata["Extension"] = extension;
                result.Metadata["OriginalName"] = file.FileName;
                result.Metadata["IsArchive"] = true;
                result.Metadata["ProcessedAt"] = DateTime.Now;

                // Verificação básica de integridade para ZIP
                if (extension == ".zip")
                {
                    await VerifyZipFileAsync(destinationPath, result);
                }

                result.Success = true;
                _logger.LogInformation("Arquivo compactado processado com sucesso: {FileName}", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar arquivo compactado: {FileName}", file.FileName);
                result.Errors.Add($"Erro ao processar arquivo compactado: {ex.Message}");
                result.Success = false;
            }
        }        private async Task VerifyZipFileAsync(string filePath, ProcessedFileResult result)
        {
            try
            {
                // Verificação básica: arquivo ZIP deve ter header válido
                using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[4];
                var bytesRead = await ReadExactAsync(reader, buffer, 0, 4);
                
                // ZIP header: PK (0x504B)
                if (bytesRead >= 2 && buffer[0] == 0x50 && buffer[1] == 0x4B)
                {
                    result.Metadata["ZipHeaderValid"] = true;
                }
                else
                {
                    result.Metadata["ZipHeaderValid"] = false;
                    _logger.LogWarning("Arquivo ZIP pode estar corrompido: header inválido");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao verificar arquivo ZIP: {FilePath}", filePath);
                result.Metadata["ZipHeaderValid"] = false;
            }
        }

        /// <summary>
        /// Lê exatamente o número especificado de bytes do stream
        /// </summary>
        private static async Task<int> ReadExactAsync(Stream stream, byte[] buffer, int offset, int count)        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset + totalBytesRead, count - totalBytesRead));
                if (bytesRead == 0)
                    break;
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
