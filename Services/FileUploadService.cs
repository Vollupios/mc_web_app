using IntranetDocumentos.Services;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// Serviço responsável pelo upload e gerenciamento de arquivos
    /// Implementa o Princípio da Responsabilidade Única (SRP)
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (!IsValidImage(file))
                    throw new ArgumentException("Arquivo de imagem inválido");

                // Criar diretório se não existir
                var uploadPath = Path.Combine(_environment.WebRootPath, "images", folder);
                Directory.CreateDirectory(uploadPath);

                // Gerar nome único
                var uniqueFileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Imagem salva com sucesso: {FileName}", uniqueFileName);

                // Retornar caminho relativo
                return Path.Combine("images", folder, uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar imagem: {FileName}", file?.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var physicalPath = GetPhysicalPath(filePath);

                if (File.Exists(physicalPath))
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    _logger.LogInformation("Imagem removida: {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover imagem: {FilePath}", filePath);
                return false;
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return false;

            // Verificar content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8]; // 8 primeiros caracteres

            return $"{fileName}_{timestamp}_{guid}{extension}";
        }

        public string GetPhysicalPath(string relativePath)
        {
            return Path.Combine(_environment.WebRootPath, relativePath.Replace("/", "\\"));
        }
    }
}
