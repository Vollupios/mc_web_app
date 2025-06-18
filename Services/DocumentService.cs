using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IntranetDocumentos.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentService> _logger;

        /// <summary>
        /// Serviço de documentos, responsável por regras de negócio e persistência.
        /// </summary>
        public DocumentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        public async Task<List<Document>> GetDocumentsForUserAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            IQueryable<Document> query = _context.Documents
                .Include(d => d.Uploader)
                .Include(d => d.Department);

            if (userRoles.Contains("Admin"))
            {
                // Admin (TI) tem acesso a todos os documentos
                return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            }
            else if (userRoles.Contains("Gestor"))
            {
                // Gestor tem acesso a todos os documentos
                return await query.OrderByDescending(d => d.UploadDate).ToListAsync();
            }
            else
            {
                // Usuário comum vê apenas documentos do seu departamento e do setor Geral
                return await query.Where(d => d.DepartmentId == user.DepartmentId || d.DepartmentId == null)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Id inválido para busca de documento: {Id}", id);
                return null;
            }

            return await _context.Documents
                .Include(d => d.Uploader)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId)
        {
            try
            {
                // Gerar nome único para o arquivo
                var fileExtension = Path.GetExtension(file.FileName);
                var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                // Determinar pasta de destino
                var folderName = departmentId.HasValue 
                    ? (await _context.Departments.FindAsync(departmentId.Value))?.Name ?? "Geral"
                    : "Geral";
                
                var documentsPath = Path.Combine(_environment.ContentRootPath, "DocumentsStorage", folderName);
                Directory.CreateDirectory(documentsPath);
                
                var filePath = Path.Combine(documentsPath, storedFileName);

                // Salvar arquivo no disco
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Criar registro no banco
                var document = new Document
                {
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.Now,
                    UploaderId = uploader.Id,
                    DepartmentId = departmentId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento salvo: {FileName} por {User}", file.FileName, uploader.Email);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar documento {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser)
        {
            try
            {
                var document = await GetDocumentByIdAsync(id);
                if (document == null) return false;

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Verificar permissões
                if (!userRoles.Contains("Admin") && document.UploaderId != currentUser.Id)
                {
                    return false;
                }

                // Remover arquivo físico
                var physicalPath = await GetDocumentPhysicalPathAsync(document);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                // Remover registro do banco
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Documento excluído: {Id} por {User}", id, currentUser.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento {Id}", id);
                return false;
            }
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao verificar acesso a documento {Id}", documentId);
                return false;
            }

            var document = await GetDocumentByIdAsync(documentId);
            if (document == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin e Gestor têm acesso a todos os documentos
            if (userRoles.Contains("Admin") || userRoles.Contains("Gestor"))
            {
                return true;
            }

            // Usuário comum só acessa documentos do seu departamento ou do setor Geral
            return document.DepartmentId == user.DepartmentId || document.DepartmentId == null;
        }

        public async Task<bool> CanUserUploadToDepartmentAsync(int? departmentId, ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            // Admin pode fazer upload para qualquer departamento
            if (userRoles.Contains("Admin"))
            {
                return true;
            }

            // Gestor pode fazer upload para qualquer departamento
            if (userRoles.Contains("Gestor"))
            {
                return true;
            }

            // Usuário comum só pode fazer upload para seu departamento ou setor Geral
            return departmentId == user.DepartmentId || departmentId == null;
        }

        public async Task<string> GetDocumentPhysicalPathAsync(Document document)
        {
            var folderName = document.DepartmentId.HasValue
                ? (await _context.Departments.FindAsync(document.DepartmentId.Value))?.Name ?? "Geral"
                : "Geral";

            return Path.Combine(_environment.ContentRootPath, "DocumentsStorage", folderName, document.StoredFileName);
        }

        public async Task<List<Department>> GetDepartmentsForUserAsync(ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("Usuário nulo ao buscar departamentos disponíveis.");
                return new List<Department>();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains("Admin") || userRoles.Contains("Gestor"))
            {
                // Admin e Gestor podem ver todos os departamentos
                return await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            }
            else
            {
                // Usuário comum só vê seu próprio departamento
                return await _context.Departments
                    .Where(d => d.Id == user.DepartmentId)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
        }
    }
}
