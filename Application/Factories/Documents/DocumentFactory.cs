using IntranetDocumentos.Application.DTOs;
using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.Factories;
using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ValueObjects;
using System.IO;

namespace IntranetDocumentos.Application.Factories.Documents
{
    /// <summary>
    /// Factory para criação de documentos
    /// Implementa regras de negócio específicas para documentos
    /// </summary>
    public class DocumentFactory : UserContextFactory<Document, DocumentCreateDTO>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _storageBasePath;

        /// <summary>
        /// Construtor da factory de documentos
        /// </summary>
        /// <param name="currentUserId">ID do usuário atual</param>
        /// <param name="serviceProvider">Provedor de serviços</param>
        /// <param name="storageBasePath">Caminho base para armazenamento</param>
        public DocumentFactory(string currentUserId, IServiceProvider serviceProvider, string storageBasePath = "DocumentsStorage")
            : base(currentUserId)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _storageBasePath = storageBasePath ?? throw new ArgumentNullException(nameof(storageBasePath));
        }

        /// <summary>
        /// Cria a entidade Document a partir do DTO
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância do Document</returns>
        protected override Document CreateEntity(DocumentCreateDTO createDto)
        {
            var document = new Document
            {
                OriginalFileName = createDto.OriginalFileName,
                StoredFileName = GenerateStoredFileName(createDto.OriginalFileName),
                ContentType = createDto.ContentType,
                FileSize = createDto.FileSize,
                Description = createDto.Description,
                DepartmentId = createDto.DepartmentId,
                FolderId = createDto.FolderId,
                Status = createDto.Status,
                Version = int.TryParse(createDto.Version, out int version) ? version : 1,
                UploadDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                ContentText = createDto.ContentText
            };

            // Aplicar configurações padrão
            SetDefaults(document, createDto);
            
            // Aplicar transformações
            ApplyTransformations(document, createDto);
            
            // Configurações finais
            FinalizeEntity(document, createDto);

            return document;
        }

        /// <summary>
        /// Validações customizadas para documentos
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>Lista de erros customizados</returns>
        protected override List<string> ValidateCustom(DocumentCreateDTO createDto)
        {
            var errors = new List<string>();

            // Validar nome do arquivo
            if (string.IsNullOrWhiteSpace(createDto.OriginalFileName))
            {
                errors.Add("Nome do arquivo é obrigatório");
            }
            else
            {
                var fileName = createDto.OriginalFileName.Trim();
                if (fileName.Length > 255)
                {
                    errors.Add("Nome do arquivo deve ter no máximo 255 caracteres");
                }

                // Validar extensão
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!IsValidExtension(extension))
                {
                    errors.Add($"Extensão {extension} não é permitida");
                }
            }

            // Validar tamanho do arquivo
            if (createDto.FileSize <= 0)
            {
                errors.Add("Tamanho do arquivo deve ser maior que zero");
            }
            else if (createDto.FileSize > GetMaxFileSize())
            {
                errors.Add($"Arquivo muito grande. Máximo permitido: {GetMaxFileSize() / (1024 * 1024)}MB");
            }

            // Validar tipo de conteúdo
            if (string.IsNullOrWhiteSpace(createDto.ContentType))
            {
                errors.Add("Tipo de conteúdo é obrigatório");
            }
            else if (!IsValidContentType(createDto.ContentType))
            {
                errors.Add($"Tipo de conteúdo {createDto.ContentType} não é permitido");
            }

            // Validar versão
            if (string.IsNullOrEmpty(createDto.Version))
            {
                errors.Add("Versão não pode ser vazia");
            }

            return errors;
        }

        /// <summary>
        /// Validações assíncronas para documentos
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>Task de validação</returns>
        protected override async Task ValidateAsync(DocumentCreateDTO createDto)
        {
            // Validações assíncronas podem ser implementadas aqui
            // Por exemplo, verificar se o departamento existe
            await Task.CompletedTask;
        }

        /// <summary>
        /// Define valores padrão para o documento
        /// </summary>
        /// <param name="entity">Documento a ser configurado</param>
        /// <param name="createDto">DTO com dados</param>
        protected override void SetDefaults(Document entity, DocumentCreateDTO createDto)
        {
            // Definir status padrão se não especificado
            if (entity.Status == 0)
            {
                entity.Status = Models.DocumentStatus.Draft;
            }

            // Definir versão padrão se não especificada
            if (entity.Version <= 0)
            {
                entity.Version = 1;
            }

            // Definir departamento padrão (Geral) se não especificado
            if (!entity.DepartmentId.HasValue)
            {
                entity.DepartmentId = null; // Geral
            }
        }

        /// <summary>
        /// Aplica transformações ao documento
        /// </summary>
        /// <param name="entity">Documento a ser transformado</param>
        /// <param name="createDto">DTO com dados</param>
        protected override void ApplyTransformations(Document entity, DocumentCreateDTO createDto)
        {
            // Normalizar nome do arquivo
            entity.OriginalFileName = NormalizeFileName(entity.OriginalFileName);

            // Gerar checksum se necessário
            if (createDto.FileData != null)
            {
                entity.ContentText = ExtractTextContent(createDto.FileData, entity.ContentType);
            }
        }

        /// <summary>
        /// Configurações finais do documento
        /// </summary>
        /// <param name="entity">Documento a ser configurado</param>
        /// <param name="createDto">DTO com dados</param>
        protected override void FinalizeEntity(Document entity, DocumentCreateDTO createDto)
        {
            // Criar Value Objects se necessário
            if (entity.FileSize > 0)
            {
                entity.SetFileSize(FileSize.FromBytes(entity.FileSize));
            }
        }

        /// <summary>
        /// Gera nome único para armazenamento do arquivo
        /// </summary>
        /// <param name="originalFileName">Nome original do arquivo</param>
        /// <returns>Nome único para armazenamento</returns>
        private string GenerateStoredFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var guid = Guid.NewGuid().ToString("N");
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            
            return $"{guid}_{timestamp}{extension}";
        }

        /// <summary>
        /// Verifica se a extensão do arquivo é válida
        /// </summary>
        /// <param name="extension">Extensão a ser verificada</param>
        /// <returns>True se válida, False caso contrário</returns>
        private bool IsValidExtension(string extension)
        {
            var allowedExtensions = new[]
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".txt", ".rtf", ".csv", ".jpg", ".jpeg", ".png", ".gif",
                ".bmp", ".tiff", ".zip", ".rar", ".7z", ".xml", ".json"
            };

            return allowedExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Verifica se o tipo de conteúdo é válido
        /// </summary>
        /// <param name="contentType">Tipo de conteúdo a ser verificado</param>
        /// <returns>True se válido, False caso contrário</returns>
        private bool IsValidContentType(string contentType)
        {
            var allowedContentTypes = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "text/plain",
                "text/csv",
                "application/rtf",
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/tiff",
                "application/zip",
                "application/x-rar-compressed",
                "application/x-7z-compressed",
                "application/xml",
                "application/json"
            };

            return allowedContentTypes.Contains(contentType.ToLowerInvariant());
        }

        /// <summary>
        /// Obtém o tamanho máximo permitido para arquivos
        /// </summary>
        /// <returns>Tamanho máximo em bytes</returns>
        private long GetMaxFileSize()
        {
            // 10MB por padrão
            return 10 * 1024 * 1024;
        }

        /// <summary>
        /// Normaliza o nome do arquivo
        /// </summary>
        /// <param name="fileName">Nome do arquivo a ser normalizado</param>
        /// <returns>Nome normalizado</returns>
        private string NormalizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return fileName;

            return fileName.Trim();
        }

        /// <summary>
        /// Extrai conteúdo de texto do arquivo para indexação
        /// </summary>
        /// <param name="fileData">Dados do arquivo</param>
        /// <param name="contentType">Tipo de conteúdo</param>
        /// <returns>Texto extraído</returns>
        private string? ExtractTextContent(byte[] fileData, string contentType)
        {
            // Implementação básica - pode ser expandida com OCR, etc.
            if (contentType.StartsWith("text/"))
            {
                try
                {
                    return System.Text.Encoding.UTF8.GetString(fileData);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Factory para criação de pastas de documentos
    /// </summary>
    public class DocumentFolderFactory : UserContextFactory<DocumentFolder, FolderCreateDTO>
    {
        /// <summary>
        /// Construtor da factory de pastas
        /// </summary>
        /// <param name="currentUserId">ID do usuário atual</param>
        public DocumentFolderFactory(string currentUserId) : base(currentUserId)
        {
        }

        /// <summary>
        /// Cria a entidade DocumentFolder a partir do DTO
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância do DocumentFolder</returns>
        protected override DocumentFolder CreateEntity(FolderCreateDTO createDto)
        {
            var folder = new DocumentFolder
            {
                Name = createDto.Name,
                Description = createDto.Description,
                ParentFolderId = createDto.ParentFolderId,
                DepartmentId = createDto.DepartmentId,
                Color = createDto.Color,
                Icon = createDto.Icon,
                DisplayOrder = createDto.DisplayOrder,
                IsSystemFolder = createDto.IsSystemFolder,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Aplicar configurações padrão
            SetDefaults(folder, createDto);
            
            // Aplicar transformações
            ApplyTransformations(folder, createDto);
            
            // Configurações finais
            FinalizeEntity(folder, createDto);

            return folder;
        }

        /// <summary>
        /// Validações customizadas para pastas
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>Lista de erros customizados</returns>
        protected override List<string> ValidateCustom(FolderCreateDTO createDto)
        {
            var errors = new List<string>();

            // Validar nome da pasta
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                errors.Add("Nome da pasta é obrigatório");
            }
            else
            {
                var name = createDto.Name.Trim();
                if (name.Length > 255)
                {
                    errors.Add("Nome da pasta deve ter no máximo 255 caracteres");
                }

                // Validar caracteres especiais
                var invalidChars = Path.GetInvalidFileNameChars();
                if (name.Any(c => invalidChars.Contains(c)))
                {
                    errors.Add("Nome da pasta contém caracteres inválidos");
                }
            }

            // Validar cor
            if (!string.IsNullOrWhiteSpace(createDto.Color))
            {
                if (!IsValidHexColor(createDto.Color))
                {
                    errors.Add("Cor deve estar no formato hexadecimal válido (#RRGGBB)");
                }
            }

            // Validar ícone
            if (!string.IsNullOrWhiteSpace(createDto.Icon))
            {
                if (!IsValidBootstrapIcon(createDto.Icon))
                {
                    errors.Add("Ícone deve ser um ícone Bootstrap válido");
                }
            }

            return errors;
        }

        /// <summary>
        /// Define valores padrão para a pasta
        /// </summary>
        /// <param name="entity">Pasta a ser configurada</param>
        /// <param name="createDto">DTO com dados</param>
        protected override void SetDefaults(DocumentFolder entity, FolderCreateDTO createDto)
        {
            // Definir cor padrão se não especificada
            if (string.IsNullOrWhiteSpace(entity.Color))
            {
                entity.Color = "#007bff"; // Azul padrão
            }

            // Definir ícone padrão se não especificado
            if (string.IsNullOrWhiteSpace(entity.Icon))
            {
                entity.Icon = "bi-folder";
            }

            // Definir ordem de exibição padrão
            if (entity.DisplayOrder <= 0)
            {
                entity.DisplayOrder = 1;
            }

            // Definir como ativa por padrão
            if (!entity.IsActive)
            {
                entity.IsActive = true;
            }
        }

        /// <summary>
        /// Aplica transformações à pasta
        /// </summary>
        /// <param name="entity">Pasta a ser transformada</param>
        /// <param name="createDto">DTO com dados</param>
        protected override void ApplyTransformations(DocumentFolder entity, FolderCreateDTO createDto)
        {
            // Normalizar nome da pasta
            entity.Name = entity.Name.Trim();

            // Construir caminho da pasta
            entity.Path = BuildFolderPath(entity);
            
            // Definir nível hierárquico
            entity.Level = CalculateFolderLevel(entity.ParentFolderId);
        }

        /// <summary>
        /// Verifica se a cor está no formato hexadecimal válido
        /// </summary>
        /// <param name="color">Cor a ser verificada</param>
        /// <returns>True se válida, False caso contrário</returns>
        private bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (!color.StartsWith("#") || color.Length != 7)
                return false;

            return color.Skip(1).All(c => 
                (c >= '0' && c <= '9') || 
                (c >= 'A' && c <= 'F') || 
                (c >= 'a' && c <= 'f'));
        }

        /// <summary>
        /// Verifica se o ícone é um ícone Bootstrap válido
        /// </summary>
        /// <param name="icon">Ícone a ser verificado</param>
        /// <returns>True se válido, False caso contrário</returns>
        private bool IsValidBootstrapIcon(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
                return false;

            // Lista básica de ícones Bootstrap válidos para pastas
            var validIcons = new[]
            {
                "bi-folder", "bi-folder-fill", "bi-folder-plus", "bi-folder-minus",
                "bi-folder-check", "bi-folder-x", "bi-folder-symlink",
                "bi-archive", "bi-archive-fill", "bi-box", "bi-box-seam",
                "bi-collection", "bi-collection-fill", "bi-inbox", "bi-inbox-fill"
            };

            return validIcons.Contains(icon.ToLowerInvariant());
        }

        /// <summary>
        /// Constrói o caminho da pasta
        /// </summary>
        /// <param name="folder">Pasta</param>
        /// <returns>Caminho da pasta</returns>
        private string BuildFolderPath(DocumentFolder folder)
        {
            // Implementação básica - pode ser expandida com consulta ao banco
            return folder.ParentFolderId.HasValue ? $"Parent/{folder.Name}" : folder.Name;
        }

        /// <summary>
        /// Calcula o nível hierárquico da pasta
        /// </summary>
        /// <param name="parentFolderId">ID da pasta pai</param>
        /// <returns>Nível hierárquico</returns>
        private int CalculateFolderLevel(int? parentFolderId)
        {
            // Implementação básica - pode ser expandida com consulta ao banco
            return parentFolderId.HasValue ? 1 : 0;
        }
    }
}
