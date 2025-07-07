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
                using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                } // Garantir que o stream seja fechado antes de qualquer verificação

                // Adicionar metadados
                result.Metadata["Extension"] = extension;
                result.Metadata["OriginalName"] = file.FileName;
                result.Metadata["ProcessedAt"] = DateTime.Now;

                // Arquivo salvo com sucesso - modo super tolerante
                result.Success = true;
                _logger.LogInformation("Documento processado com sucesso: {FileName}", file.FileName);
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
                
                // Não tentar resetar position - pode causar problemas
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao validar arquivo de texto: {FileName}, ignorando validação", file.FileName);
                // Não adicionar erro - apenas ignorar validação
            }
        }

        private async Task<bool> VerifyFileIntegrityAsync(string filePath, string extension)
        {
            try
            {
                _logger.LogInformation("Verificando integridade do arquivo: {FilePath}, Extensão: {Extension}", filePath, extension);
                
                // Verificação básica - arquivo existe e tem conteúdo
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                {
                    _logger.LogWarning("Arquivo não existe ou está vazio: {FilePath}", filePath);
                    return false;
                }

                _logger.LogInformation("Arquivo existe com tamanho: {FileSize} bytes", fileInfo.Length);

                // Verificações específicas por tipo - modo muito tolerante
                switch (extension)
                {
                    case ".pdf":
                        // Para PDF, tentamos verificar mas sempre aceitamos
                        var pdfValid = await VerifyPdfFileAsync(filePath);
                        if (!pdfValid)
                        {
                            _logger.LogInformation("Verificação de PDF falhou, mas aceitando arquivo: {FilePath}", filePath);
                        }
                        return true; // Sempre aceitar PDFs
                    case ".txt":
                        // Para texto, tentamos verificar mas sempre aceitamos
                        var textValid = await VerifyTextFileAsync(filePath);
                        if (!textValid)
                        {
                            _logger.LogInformation("Verificação de texto falhou, mas aceitando arquivo: {FilePath}", filePath);
                        }
                        return true; // Sempre aceitar arquivos de texto
                    default:
                        _logger.LogInformation("Tipo de arquivo aceito sem verificação específica: {Extension}", extension);
                        return true; // Para outros tipos, sempre aceitar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na verificação de integridade: {FilePath}", filePath);
                return true; // Em caso de erro, aceitar o arquivo (mais tolerante)
            }
        }        private async Task<bool> VerifyPdfFileAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Verificando arquivo PDF: {FilePath}", filePath);
                
                // Verificação básica: arquivo PDF deve começar com %PDF
                using var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[5]; // Aumentar para 5 bytes para pegar "%PDF-"
                var bytesRead = await ReadExactAsync(reader, buffer, 0, 5);
                
                _logger.LogInformation("Bytes lidos do PDF: {BytesRead}, Conteúdo: {Content}", 
                    bytesRead, System.Text.Encoding.ASCII.GetString(buffer, 0, Math.Min(bytesRead, 5)));
                
                if (bytesRead < 4) 
                {
                    _logger.LogWarning("PDF tem menos de 4 bytes: {BytesRead}", bytesRead);
                    return false;
                }
                
                var header = System.Text.Encoding.ASCII.GetString(buffer, 0, 4);
                var isValidPdf = header == "%PDF";
                
                _logger.LogInformation("Header do PDF: '{Header}', Válido: {IsValid}", header, isValidPdf);
                
                return isValidPdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar arquivo PDF: {FilePath}", filePath);
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
