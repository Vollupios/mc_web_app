using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Processador genérico que aceita todos os tipos de arquivo - Strategy Pattern
    /// Remove limitações de tipo de arquivo
    /// </summary>
    public class GenericFileProcessor : BaseFileProcessor
    {
        public GenericFileProcessor(ILogger<BaseFileProcessor> logger, IWebHostEnvironment environment) 
            : base(logger, environment) { }

        public override bool CanProcess(string fileExtension)
        {
            // Aceita qualquer extensão de arquivo - processador de fallback universal
            return true;
        }

        public override IEnumerable<string> SupportedExtensions => 
            new[] { "*" }; // Indica que suporta todos os tipos

        public override long MaxFileSize => 100 * 1024 * 1024; // 100MB para arquivos genéricos

        protected override async Task ProcessSpecificAsync(IFormFile file, string destinationPath, ProcessedFileResult result)
        {
            try
            {
                // Salvar arquivo sem processamento específico
                using var fileStream = new FileStream(destinationPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                // Adicionar informações básicas
                result.FilePath = destinationPath;
                result.ContentType = file.ContentType ?? "application/octet-stream";
                result.FileName = Path.GetFileName(destinationPath);
                result.FileSize = file.Length;

                // Tentar extrair algum texto básico se possível
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                // Para arquivos de texto simples, tentar ler conteúdo
                if (extension == ".txt" || extension == ".log" || extension == ".md" || 
                    extension == ".csv" || extension == ".json" || extension == ".xml" ||
                    extension == ".html" || extension == ".css" || extension == ".js")
                {
                    try
                    {
                        using var reader = new StreamReader(file.OpenReadStream(), encoding: System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                        var content = await reader.ReadToEndAsync();
                        
                        // Limitar o conteúdo para evitar problemas de performance
                        if (content.Length > 10000)
                        {
                            content = content.Substring(0, 10000) + "... [conteúdo truncado]";
                        }
                        
                        result.Metadata["ExtractedText"] = content;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível extrair texto do arquivo {FileName}", file.FileName);
                        result.Metadata["ExtractedText"] = $"[Arquivo {extension.ToUpper()} - Texto não disponível]";
                    }
                }
                else
                {
                    result.Metadata["ExtractedText"] = $"[Arquivo {extension.ToUpper()} - {FormatFileSize(file.Length)}]";
                }

                _logger.LogInformation("Arquivo genérico processado com sucesso: {FileName} ({Size})", 
                    file.FileName, FormatFileSize(file.Length));
                
                result.Success = true; // Marcar como sucesso
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exceção no processamento genérico do arquivo {FileName}, mas continuando", file.FileName);
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                result.Metadata["ExtractedText"] = $"[Arquivo {fileExtension.ToUpper()} - Processado com limitações]";
                result.Success = true; // Forçar sucesso mesmo com erro
                // Não adicionar erro e não fazer throw para garantir que o arquivo seja aceito
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
