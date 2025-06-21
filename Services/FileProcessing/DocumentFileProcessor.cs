using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Processador para documentos PDF e Office - Strategy Pattern
    /// </summary>
    public class DocumentFileProcessor : BaseFileProcessor
    {
        public DocumentFileProcessor(ILogger<BaseFileProcessor> logger, IWebHostEnvironment environment) 
            : base(logger, environment) { }

        public override bool CanProcess(string fileExtension)
        {
            return SupportedExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public override IEnumerable<string> SupportedExtensions => 
            new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf" };

        public override long MaxFileSize => 10 * 1024 * 1024; // 10MB

        protected override async Task ProcessSpecificAsync(IFormFile file, string destinationPath, ProcessedFileResult result)
        {
            try
            {
                // Validações específicas para documentos
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                // Verificar se é um arquivo de texto válido
                if (extension == ".txt")
                {
                    await ValidateTextFileAsync(file, result);
                }

                // Salvar arquivo
                using var fileStream = new FileStream(destinationPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                // Adicionar metadados
                result.Metadata["Extension"] = extension;
                result.Metadata["OriginalName"] = file.FileName;
                result.Metadata["ProcessedAt"] = DateTime.Now;

                // Verificação adicional de integridade
                if (await VerifyFileIntegrityAsync(destinationPath, extension))
                {
                    result.Success = true;
                    _logger.LogInformation("Documento processado com sucesso: {FileName}", file.FileName);
                }
                else
                {
                    result.Errors.Add("Arquivo pode estar corrompido");
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar documento: {FileName}", file.FileName);
                result.Errors.Add($"Erro ao processar documento: {ex.Message}");
                result.Success = false;
            }
        }

        private async Task ValidateTextFileAsync(IFormFile file, ProcessedFileResult result)
        {
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                
                result.Metadata["LineCount"] = content.Split('\n').Length;
                result.Metadata["CharacterCount"] = content.Length;
                
                // Reset stream position
                file.OpenReadStream().Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao validar arquivo de texto: {FileName}", file.FileName);
            }
        }

        private async Task<bool> VerifyFileIntegrityAsync(string filePath, string extension)
        {
            try
            {
                // Verificação básica - arquivo existe e tem conteúdo
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    return false;

                // Verificações específicas por tipo
                switch (extension)
                {
                    case ".pdf":
                        return await VerifyPdfFileAsync(filePath);
                    case ".txt":
                        return await VerifyTextFileAsync(filePath);
                    default:
                        return true; // Para outros tipos, assumir válido se existe
                }
            }
            catch
            {
                return false;
            }
        }        private async Task<bool> VerifyPdfFileAsync(string filePath)
        {
            try
            {
                // Verificação básica: arquivo PDF deve começar com %PDF
                using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[4];
                var bytesRead = await ReadExactAsync(reader, buffer, 0, 4);
                if (bytesRead < 4) return false;
                
                var header = System.Text.Encoding.ASCII.GetString(buffer);
                return header == "%PDF";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lê exatamente o número especificado de bytes do stream
        /// </summary>
        private static async Task<int> ReadExactAsync(Stream stream, byte[] buffer, int offset, int count)
        {
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

        private async Task<bool> VerifyTextFileAsync(string filePath)
        {
            try
            {
                // Verificar se é texto válido (UTF-8)
                var content = await File.ReadAllTextAsync(filePath);
                return !string.IsNullOrEmpty(content);
            }
            catch
            {
                return false;
            }
        }
    }
}
