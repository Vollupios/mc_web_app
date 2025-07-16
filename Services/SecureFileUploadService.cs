using System.Security.Cryptography;
using System.Text;

namespace IntranetDocumentos.Services
{
    /// <summary>
    /// 🔒 Serviço de upload seguro com validação rigorosa de arquivos
    /// Implementa múltiplas camadas de validação para prevenir uploads maliciosos
    /// </summary>
    public class SecureFileUploadService : ISecureFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SecureFileUploadService> _logger;

        // 🔒 Lista restritiva de extensões permitidas (whitelist)
        private static readonly string[] AllowedExtensions = 
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", 
            ".txt", ".rtf", ".odt", ".ods", ".odp",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
            // ❌ Removidas extensões executáveis: .exe, .bat, .ps1, .js, .vbs, etc.
        };

        // 🔒 Lista restritiva de MIME types permitidos
        private static readonly string[] AllowedMimeTypes = 
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "text/plain",
            "text/rtf",
            "application/vnd.oasis.opendocument.text",
            "application/vnd.oasis.opendocument.spreadsheet",
            "application/vnd.oasis.opendocument.presentation",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        // 🔒 Assinaturas de arquivo conhecidas (magic bytes)
        private static readonly Dictionary<string, string[]> FileSignatures = new()
        {
            { ".pdf", new[] { "25504446" } },                           // %PDF
            { ".docx", new[] { "504B0304", "504B030414" } },           // ZIP (Office Open XML)
            { ".xlsx", new[] { "504B0304", "504B030414" } },           // ZIP (Office Open XML)
            { ".pptx", new[] { "504B0304", "504B030414" } },           // ZIP (Office Open XML)
            { ".doc", new[] { "D0CF11E0A1B11AE1" } },                  // MS Office Legacy
            { ".xls", new[] { "D0CF11E0A1B11AE1" } },                  // MS Office Legacy
            { ".ppt", new[] { "D0CF11E0A1B11AE1" } },                  // MS Office Legacy
            { ".txt", new[] { "EFBBBF", "FFFE", "FEFF" } },            // UTF-8, UTF-16 BOM
            { ".jpg", new[] { "FFD8FFE0", "FFD8FFE1", "FFD8FFDB" } },  // JPEG
            { ".jpeg", new[] { "FFD8FFE0", "FFD8FFE1", "FFD8FFDB" } }, // JPEG
            { ".png", new[] { "89504E47" } },                           // PNG
            { ".gif", new[] { "47494638" } },                           // GIF
            { ".bmp", new[] { "424D" } }                                // BMP
        };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB máximo
        private const int MaxFilenameLength = 255;

        public SecureFileUploadService(IWebHostEnvironment environment, ILogger<SecureFileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// 🔒 Validação completa de arquivo com múltiplas camadas de segurança
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "Nenhum arquivo foi selecionado.");

                // 1. 🔒 Validar tamanho do arquivo
                if (file.Length > MaxFileSize)
                    return (false, $"Arquivo muito grande. Tamanho máximo: {MaxFileSize / 1024 / 1024}MB");

                // 2. 🔒 Validar nome do arquivo
                if (string.IsNullOrWhiteSpace(file.FileName) || file.FileName.Length > MaxFilenameLength)
                    return (false, "Nome do arquivo inválido ou muito longo.");

                // 3. 🔒 Verificar caracteres perigosos no nome
                var dangerousChars = new[] { "..", "\\", "/", ":", "*", "?", "\"", "<", ">", "|", "\0" };
                if (dangerousChars.Any(c => file.FileName.Contains(c)))
                    return (false, "Nome do arquivo contém caracteres não permitidos.");

                // 4. 🔒 Validar extensão (whitelist)
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                    return (false, $"Tipo de arquivo não permitido: {extension}");

                // 5. 🔒 Validar MIME type
                if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                    return (false, $"Tipo MIME não permitido: {file.ContentType}");

                // 6. 🔒 Validar assinatura de arquivo (magic bytes)
                if (!await ValidateFileSignatureAsync(file, extension))
                    return (false, "Arquivo corrompido ou tipo incorreto detectado.");

                // 7. 🔒 Scan básico de conteúdo malicioso
                if (!await ScanForMaliciousContentAsync(file))
                    return (false, "Conteúdo potencialmente malicioso detectado.");

                _logger.LogInformation("Arquivo validado com sucesso: {FileName}, Tamanho: {FileSize} bytes", 
                    file.FileName, file.Length);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na validação do arquivo: {FileName}", file?.FileName);
                return (false, "Erro interno na validação do arquivo.");
            }
        }

        /// <summary>
        /// 🔒 Validação de assinatura de arquivo (magic bytes)
        /// </summary>
        private async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension)
        {
            try
            {
                if (!FileSignatures.ContainsKey(extension))
                    return true; // Se não tem assinatura definida, passa (ex: .txt)

                using var stream = file.OpenReadStream();
                var buffer = new byte[8];
                var bytesRead = await stream.ReadAsync(buffer, 0, 8);
                
                if (bytesRead == 0)
                    return false;

                var signature = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", "");
                var validSignatures = FileSignatures[extension];

                return validSignatures.Any(validSig => signature.StartsWith(validSig, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na validação da assinatura do arquivo");
                return false;
            }
        }

        /// <summary>
        /// 🔒 Scan básico para detectar conteúdo malicioso
        /// </summary>
        private async Task<bool> ScanForMaliciousContentAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

                // Para arquivos de texto, verificar conteúdo malicioso básico
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension == ".txt" || extension == ".rtf")
                {
                    var content = await reader.ReadToEndAsync();
                    
                    // 🔒 Palavras-chave maliciosas básicas
                    var maliciousKeywords = new[] 
                    { 
                        "<script", "javascript:", "vbscript:", "onload=", "onerror=",
                        "eval(", "document.write", "innerHTML", "exec(", "system(",
                        "cmd.exe", "powershell", "bash", "sh -"
                    };

                    foreach (var keyword in maliciousKeywords)
                    {
                        if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Conteúdo malicioso detectado no arquivo: {FileName}, Keyword: {Keyword}", 
                                file.FileName, keyword);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no scan de conteúdo malicioso");
                return false; // Em caso de erro, bloquear por segurança
            }
        }

        /// <summary>
        /// 🔒 Gerar nome seguro para o arquivo
        /// </summary>
        public string GenerateSecureFileName(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var randomId = Guid.NewGuid().ToString("N")[..8];
            
            // Nome seguro: timestamp_randomId.extensao
            return $"{timestamp}_{randomId}{extension}";
        }

        /// <summary>
        /// 🔒 Salvar arquivo de forma segura
        /// </summary>
        public async Task<string> SaveFileSecurelyAsync(IFormFile file, string department)
        {
            try
            {
                // 1. Validar arquivo antes de salvar
                var (isValid, errorMessage) = await ValidateFileAsync(file);
                if (!isValid)
                    throw new SecurityException($"Arquivo rejeitado: {errorMessage}");

                // 2. Criar diretório seguro fora da wwwroot
                var documentsPath = Path.Combine(_environment.ContentRootPath, "DocumentsStorage", department);
                Directory.CreateDirectory(documentsPath);

                // 3. Gerar nome seguro
                var secureFileName = GenerateSecureFileName(file);
                var filePath = Path.Combine(documentsPath, secureFileName);

                // 4. Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 5. Verificar arquivo salvo (paranoia security check)
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length != file.Length)
                {
                    throw new InvalidOperationException("Falha na verificação pós-salvamento do arquivo.");
                }

                _logger.LogInformation("Arquivo salvo com segurança: {OriginalName} -> {SecureName}, Departamento: {Department}", 
                    file.FileName, secureFileName, department);

                return secureFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar arquivo de forma segura: {FileName}", file?.FileName);
                throw;
            }
        }
    }

    /// <summary>
    /// Interface para o serviço de upload seguro
    /// </summary>
    public interface ISecureFileUploadService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file);
        string GenerateSecureFileName(IFormFile file);
        Task<string> SaveFileSecurelyAsync(IFormFile file, string department);
    }

    /// <summary>
    /// Exception personalizada para problemas de segurança em uploads
    /// </summary>
    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
