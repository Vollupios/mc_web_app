using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Processador específico para imagens - Strategy Pattern
    /// </summary>
    public class ImageFileProcessor : BaseFileProcessor
    {
        public ImageFileProcessor(ILogger<BaseFileProcessor> logger, IWebHostEnvironment environment) 
            : base(logger, environment) { }

        public override bool CanProcess(string fileExtension)
        {
            return SupportedExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public override IEnumerable<string> SupportedExtensions => 
            new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        public override long MaxFileSize => 5 * 1024 * 1024; // 5MB

        protected override async Task ProcessSpecificAsync(IFormFile file, string destinationPath, ProcessedFileResult result)
        {
            try
            {
                using var image = await Image.LoadAsync(file.OpenReadStream());
                
                // Adicionar metadados da imagem
                result.Metadata["Width"] = image.Width;
                result.Metadata["Height"] = image.Height;
                result.Metadata["Format"] = image.Metadata.DecodedImageFormat?.Name ?? "Unknown";

                // Otimizar imagem se necessário
                if (image.Width > 1920 || image.Height > 1080)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(1920, 1080),
                        Mode = ResizeMode.Max
                    }));
                    
                    result.Metadata["Optimized"] = true;
                    _logger.LogInformation("Imagem redimensionada: {FileName}", file.FileName);
                }

                // Salvar imagem otimizada
                await image.SaveAsync(destinationPath);
                
                result.Success = true;
                _logger.LogInformation("Imagem processada com sucesso: {FileName}", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar imagem: {FileName}", file.FileName);
                result.Errors.Add($"Erro ao processar imagem: {ex.Message}");
                result.Success = false;
            }
        }
    }
}
