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

        public DocumentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
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
            return await _context.Documents
                .Include(d => d.Uploader)
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Document> SaveDocumentAsync(IFormFile file, ApplicationUser uploader, int? departmentId)
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

            return document;
        }

        public async Task<bool> DeleteDocumentAsync(int id, ApplicationUser currentUser)
        {
            var document = await GetDocumentByIdAsync(id);
            if (document == null) return false;

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            
            // Verificar permissões
            if (!userRoles.Contains("Admin") && document.UploaderId != currentUser.Id)
            {
                return false;
            }

            try
            {
                // Remover arquivo físico
                var physicalPath = await GetDocumentPhysicalPathAsync(document);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                // Remover registro do banco
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanUserAccessDocumentAsync(int documentId, ApplicationUser user)
        {
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
