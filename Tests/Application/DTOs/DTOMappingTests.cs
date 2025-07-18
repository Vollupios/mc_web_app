using Xunit;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Application.DTOs.Documents;
using IntranetDocumentos.Application.DTOs.Users;
using IntranetDocumentos.Application.DTOs.Departments;
using IntranetDocumentos.Application.Mappers;
using IntranetDocumentos.Models;
using IntranetDocumentos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntranetDocumentos.Tests.Application.DTOs
{
    /// <summary>
    /// Testes unitários para os DTOs e seus mapeamentos
    /// </summary>
    public class DTOMappingTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public DTOMappingTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            SeedTestData();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private void SeedTestData()
        {
            // Criar departamento de teste
            var department = new Department
            {
                Id = 1,
                Name = "Departamento de Teste"
            };
            _context.Departments.Add(department);

            // Criar usuário de teste
            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "teste@teste.com",
                Email = "teste@teste.com",
                FirstName = "João",
                LastName = "Silva",
                DepartmentId = 1,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);

            // Criar pasta de teste
            var folder = new DocumentFolder
            {
                Id = 1,
                Name = "Pasta de Teste",
                Description = "Pasta para testes",
                DepartmentId = 1,
                CreatedById = "user123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Color = "#007bff",
                Icon = "bi-folder"
            };
            _context.DocumentFolders.Add(folder);

            // Criar documento de teste
            var document = new Document
            {
                Id = 1,
                OriginalFileName = "documento-teste.pdf",
                StoredFileName = "guid-documento-teste.pdf",
                ContentType = "application/pdf",
                FileSize = 1024000,
                Description = "Documento de teste",
                DepartmentId = 1,
                FolderId = 1,
                UploaderId = "user123",
                Status = Models.DocumentStatus.Published,
                Version = 1,
                UploadDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            _context.Documents.Add(document);

            _context.SaveChanges();
        }

        [Fact]
        public void DocumentToDTO_ShouldMapCorrectly()
        {
            // Arrange
            var document = _context.Documents
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .First();

            // Act
            var dto = document.ToDTO();

            // Assert
            Assert.Equal(document.Id, dto.Id);
            Assert.Equal(document.OriginalFileName, dto.Title);
            Assert.Equal(document.OriginalFileName, dto.OriginalFileName);
            Assert.Equal(document.StoredFileName, dto.StoredFileName);
            Assert.Equal(document.ContentType, dto.ContentType);
            Assert.Equal(document.FileSize, dto.FileSize);
            Assert.Equal(document.Description, dto.Description);
            Assert.Equal(document.DepartmentId, dto.DepartmentId);
            Assert.Equal(document.FolderId, dto.FolderId);
            Assert.Equal(document.UploaderId, dto.UploaderId);
            Assert.Equal(document.Version, dto.Version);
            Assert.Equal(document.UploadDate, dto.UploadDate);
            Assert.Equal(document.LastModified, dto.LastModified);
            Assert.Equal(".pdf", dto.FileExtension);
            Assert.Equal((Application.DTOs.Documents.DocumentStatus)document.Status, dto.Status);
        }

        [Fact]
        public void DocumentCreateDTOToEntity_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new DocumentCreateDTO
            {
                OriginalFileName = "novo-documento.pdf",
                StoredFileName = "guid-novo-documento.pdf",
                ContentType = "application/pdf",
                FileSize = 2048000,
                Description = "Novo documento de teste",
                DepartmentId = 1,
                FolderId = 1,
                UploaderId = "user123",
                Status = Application.DTOs.Documents.DocumentStatus.Draft,
                Version = 1
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            Assert.Equal(dto.OriginalFileName, entity.OriginalFileName);
            Assert.Equal(dto.StoredFileName, entity.StoredFileName);
            Assert.Equal(dto.ContentType, entity.ContentType);
            Assert.Equal(dto.FileSize, entity.FileSize);
            Assert.Equal(dto.Description, entity.Description);
            Assert.Equal(dto.DepartmentId, entity.DepartmentId);
            Assert.Equal(dto.FolderId, entity.FolderId);
            Assert.Equal(dto.UploaderId, entity.UploaderId);
            Assert.Equal(dto.Version, entity.Version);
            Assert.Equal((Models.DocumentStatus)dto.Status, entity.Status);
        }

        [Fact]
        public void UserToDTO_ShouldMapCorrectly()
        {
            // Arrange
            var user = _context.Users
                .Include(u => u.Department)
                .First();

            // Act
            var dto = user.ToDTO();

            // Assert
            Assert.Equal(user.Id, dto.Id);
            Assert.Equal(user.UserName, dto.UserName);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.FirstName, dto.FirstName);
            Assert.Equal(user.LastName, dto.LastName);
            Assert.Equal(user.DepartmentId, dto.DepartmentId);
            Assert.Equal(user.IsActive, dto.IsActive);
            Assert.Equal(user.EmailConfirmed, dto.EmailConfirmed);
            Assert.Equal(user.CreatedAt, dto.CreatedAt);
            Assert.Equal(user.UpdatedAt, dto.UpdatedAt);
        }

        [Fact]
        public void UserCreateDTOToEntity_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new UserCreateDTO
            {
                UserName = "novo.usuario",
                Email = "novo.usuario@teste.com",
                FirstName = "Novo",
                LastName = "Usuário",
                DepartmentId = 1,
                IsActive = true,
                EmailConfirmed = true
            };

            // Act
            var entity = dto.ToEntity();

            // Assert
            Assert.Equal(dto.UserName, entity.UserName);
            Assert.Equal(dto.Email, entity.Email);
            Assert.Equal(dto.FirstName, entity.FirstName);
            Assert.Equal(dto.LastName, entity.LastName);
            Assert.Equal(dto.DepartmentId, entity.DepartmentId);
            Assert.Equal(dto.IsActive, entity.IsActive);
            Assert.Equal(dto.EmailConfirmed, entity.EmailConfirmed);
        }

        [Fact]
        public void DepartmentToDTO_ShouldMapCorrectly()
        {
            // Arrange
            var department = _context.Departments
                .Include(d => d.Users)
                .Include(d => d.Documents)
                .First();

            // Act
            var dto = department.ToDTO();

            // Assert
            Assert.Equal(department.Id, dto.Id);
            Assert.Equal(department.Name, dto.Name);
            Assert.Equal(department.Users.Count, dto.UserCount);
            Assert.Equal(department.Documents.Count, dto.DocumentCount);
        }

        [Fact]
        public void DocumentFolderToDTO_ShouldMapCorrectly()
        {
            // Arrange
            var folder = _context.DocumentFolders
                .Include(f => f.Department)
                .Include(f => f.CreatedBy)
                .Include(f => f.Documents)
                .First();

            // Act
            var dto = folder.ToDTO();

            // Assert
            Assert.Equal(folder.Id, dto.Id);
            Assert.Equal(folder.Name, dto.Name);
            Assert.Equal(folder.Description, dto.Description);
            Assert.Equal(folder.DepartmentId, dto.DepartmentId);
            Assert.Equal(folder.CreatedById, dto.CreatedById);
            Assert.Equal(folder.CreatedAt, dto.CreatedAt);
            Assert.Equal(folder.UpdatedAt, dto.UpdatedAt);
            Assert.Equal(folder.IsActive, dto.IsActive);
            Assert.Equal(folder.Color, dto.Color);
            Assert.Equal(folder.Icon, dto.Icon);
        }

        [Fact]
        public void DocumentSearchCriteria_ShouldFilterCorrectly()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                FileName = "documento",
                ContentType = "application/pdf",
                DepartmentId = 1,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredDocuments);
            Assert.Equal("documento-teste.pdf", filteredDocuments.First().OriginalFileName);
        }

        [Fact]
        public void DocumentSearchCriteria_WithNoMatches_ShouldReturnEmpty()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                FileName = "arquivo-inexistente",
                ContentType = "application/pdf"
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Empty(filteredDocuments);
        }

        [Fact]
        public void DocumentSearchCriteria_WithPartialMatch_ShouldReturnMatches()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                FileName = "documento" // Deve encontrar "documento-teste.pdf"
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredDocuments);
        }

        [Fact]
        public void DocumentSearchCriteria_WithDepartmentFilter_ShouldWork()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                DepartmentId = 1
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredDocuments);
        }

        [Fact]
        public void DocumentSearchCriteria_WithInvalidDepartment_ShouldReturnEmpty()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                DepartmentId = 999 // Departamento inexistente
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Empty(filteredDocuments);
        }

        [Fact]
        public void DocumentSearchCriteria_WithDateRange_ShouldWork()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredDocuments);
        }

        [Fact]
        public void DocumentSearchCriteria_WithFutureDate_ShouldReturnEmpty()
        {
            // Arrange
            var documents = _context.Documents.ToList();
            var searchDto = new DocumentSearchDTO
            {
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            // Act
            var filteredDocuments = documents.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Empty(filteredDocuments);
        }

        [Fact]
        public void FolderSearchCriteria_ShouldFilterCorrectly()
        {
            // Arrange
            var folders = _context.DocumentFolders.ToList();
            var searchDto = new FolderSearchDTO
            {
                Name = "Pasta",
                DepartmentId = 1
            };

            // Act
            var filteredFolders = folders.Where(f => f.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredFolders);
            Assert.Equal("Pasta de Teste", filteredFolders.First().Name);
        }

        [Fact]
        public void DepartmentSearchCriteria_ShouldFilterCorrectly()
        {
            // Arrange
            var departments = _context.Departments.ToList();
            var searchDto = new DepartmentSearchDTO
            {
                Name = "Departamento"
            };

            // Act
            var filteredDepartments = departments.Where(d => d.MatchesSearchCriteria(searchDto)).ToList();

            // Assert
            Assert.Single(filteredDepartments);
            Assert.Equal("Departamento de Teste", filteredDepartments.First().Name);
        }

        [Theory]
        [InlineData("documento-teste.pdf", ".pdf")]
        [InlineData("arquivo.docx", ".docx")]
        [InlineData("imagem.jpg", ".jpg")]
        [InlineData("planilha.xlsx", ".xlsx")]
        [InlineData("arquivo_sem_extensao", "")]
        public void GetFileExtension_ShouldReturnCorrectExtension(string fileName, string expectedExtension)
        {
            // Arrange
            var document = new Document
            {
                OriginalFileName = fileName,
                StoredFileName = "stored_" + fileName,
                ContentType = "application/octet-stream",
                FileSize = 1024,
                UploaderId = "user123",
                UploadDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            // Act
            var dto = document.ToDTO();

            // Assert
            Assert.Equal(expectedExtension, dto.FileExtension);
        }

        [Fact]
        public void MultipleDTOMappings_ShouldWorkCorrectly()
        {
            // Arrange
            var documents = _context.Documents
                .Include(d => d.Department)
                .Include(d => d.Uploader)
                .Include(d => d.Folder)
                .ToList();

            // Act
            var dtos = documents.Select(d => d.ToDTO()).ToList();

            // Assert
            Assert.Single(dtos);
            Assert.All(dtos, dto => 
            {
                Assert.NotNull(dto.OriginalFileName);
                Assert.NotNull(dto.StoredFileName);
                Assert.NotNull(dto.ContentType);
                Assert.True(dto.FileSize > 0);
                Assert.NotNull(dto.UploaderId);
                Assert.True(dto.Version > 0);
            });
        }
    }
}
