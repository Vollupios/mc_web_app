using IntranetDocumentos.Models;
using IntranetDocumentos.Services;
using IntranetDocumentos.Services.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace IntranetDocumentos.Tests.Services;

/// <summary>
/// Testes unitários para DocumentService
/// </summary>
public class DocumentServiceTests : IDisposable
{
    private readonly Mock<IDocumentReader> _documentReaderMock;
    private readonly Mock<IDocumentWriter> _documentWriterMock;
    private readonly Mock<IDocumentSecurity> _documentSecurityMock;
    private readonly Mock<IDocumentDownloader> _documentDownloaderMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _documentReaderMock = new Mock<IDocumentReader>();
        _documentWriterMock = new Mock<IDocumentWriter>();
        _documentSecurityMock = new Mock<IDocumentSecurity>();
        _documentDownloaderMock = new Mock<IDocumentDownloader>();
        _loggerMock = new Mock<ILogger<DocumentService>>();

        _documentService = new DocumentService(
            _documentReaderMock.Object,
            _documentWriterMock.Object,
            _documentSecurityMock.Object,
            _documentDownloaderMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetDocumentsForUserAsync_ShouldCallDocumentReader()
    {
        // Arrange
        var user = new ApplicationUser { Id = "test-user-id", DepartmentId = 1 };
        var expectedDocuments = new List<Document>
        {
            new() { Id = 1, OriginalFileName = "test.pdf", DepartmentId = 1 }
        };

        _documentReaderMock
            .Setup(x => x.GetDocumentsForUserAsync(user))
            .ReturnsAsync(expectedDocuments);

        // Act
        var result = await _documentService.GetDocumentsForUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test.pdf", result.First().OriginalFileName);
        _documentReaderMock.Verify(x => x.GetDocumentsForUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldCallDocumentReader()
    {
        // Arrange
        var documentId = 1;
        var expectedDocument = new Document { Id = documentId, OriginalFileName = "test.pdf" };

        _documentReaderMock
            .Setup(x => x.GetDocumentByIdAsync(documentId))
            .ReturnsAsync(expectedDocument);

        // Act
        var result = await _documentService.GetDocumentByIdAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.Id);
        Assert.Equal("test.pdf", result.OriginalFileName);
        _documentReaderMock.Verify(x => x.GetDocumentByIdAsync(documentId), Times.Once);
    }

    [Fact]
    public async Task SaveDocumentAsync_ShouldCallDocumentWriter()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileContent = "test file content";
        var fileName = "test.pdf";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var user = new ApplicationUser { Id = "test-user-id" };
        var departmentId = 1;
        var expectedDocument = new Document 
        { 
            Id = 1, 
            OriginalFileName = fileName,
            UploaderId = user.Id,
            DepartmentId = departmentId
        };

        _documentWriterMock
            .Setup(x => x.SaveDocumentAsync(mockFile.Object, user, departmentId))
            .ReturnsAsync(expectedDocument);

        // Act
        var result = await _documentService.SaveDocumentAsync(mockFile.Object, user, departmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileName, result.OriginalFileName);
        Assert.Equal(user.Id, result.UploaderId);
        Assert.Equal(departmentId, result.DepartmentId);
        _documentWriterMock.Verify(x => x.SaveDocumentAsync(mockFile.Object, user, departmentId), Times.Once);
    }    [Fact]
    public async Task CanUserAccessDocumentAsync_ShouldCallDocumentSecurity()
    {
        // Arrange
        var user = new ApplicationUser { Id = "test-user-id", DepartmentId = 1 };
        var documentId = 1;

        _documentSecurityMock
            .Setup(x => x.CanUserAccessDocumentAsync(documentId, user))
            .ReturnsAsync(true);

        // Act
        var result = await _documentService.CanUserAccessDocumentAsync(documentId, user);

        // Assert
        Assert.True(result);
        _documentSecurityMock.Verify(x => x.CanUserAccessDocumentAsync(documentId, user), Times.Once);
    }    [Fact]
    public async Task CanUserDeleteDocumentAsync_ShouldCallDocumentSecurity()
    {
        // Arrange
        var user = new ApplicationUser { Id = "test-user-id", DepartmentId = 1 };
        var documentId = 1;

        _documentSecurityMock
            .Setup(x => x.CanUserDeleteDocumentAsync(documentId, user))
            .ReturnsAsync(true);

        // Act
        var result = await _documentService.CanUserDeleteDocumentAsync(documentId, user);

        // Assert
        Assert.True(result);
        _documentSecurityMock.Verify(x => x.CanUserDeleteDocumentAsync(documentId, user), Times.Once);
    }    [Fact]
    public async Task GetDocumentStreamAsync_ShouldCallDocumentDownloader()
    {
        // Arrange
        var documentId = 1;
        var user = new ApplicationUser { Id = "test-user-id", DepartmentId = 1 };
        var expectedStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));

        _documentDownloaderMock
            .Setup(x => x.GetDocumentStreamAsync(documentId, user))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _documentDownloaderMock.Object.GetDocumentStreamAsync(documentId, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStream, result);
        _documentDownloaderMock.Verify(x => x.GetDocumentStreamAsync(documentId, user), Times.Once);
    }    [Fact]
    public async Task DeleteDocumentAsync_ShouldCallDocumentWriter()
    {
        // Arrange
        var documentId = 1;
        var user = new ApplicationUser { Id = "test-user-id", DepartmentId = 1 };

        _documentWriterMock
            .Setup(x => x.DeleteDocumentAsync(documentId, user))
            .ReturnsAsync(true);

        // Act
        var result = await _documentService.DeleteDocumentAsync(documentId, user);

        // Assert
        Assert.True(result);
        _documentWriterMock.Verify(x => x.DeleteDocumentAsync(documentId, user), Times.Once);
    }

    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("document.doc", true)]
    [InlineData("document.docx", true)]
    [InlineData("document.xls", true)]
    [InlineData("document.xlsx", true)]
    [InlineData("document.jpg", true)]
    [InlineData("document.png", true)]
    [InlineData("document.gif", true)]
    [InlineData("document.txt", true)]
    [InlineData("document.zip", true)]
    [InlineData("document.exe", false)]
    [InlineData("document.bat", false)]
    [InlineData("document", false)]
    public async Task IsFileTypeAllowed_ShouldReturnCorrectResult(string fileName, bool expected)
    {
        // Act
        var result = await _documentService.IsFileTypeAllowed(fileName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1024, true)]      // 1KB
    [InlineData(5242880, true)]   // 5MB
    [InlineData(10485760, true)]  // 10MB (limite)
    [InlineData(10485761, false)] // 10MB + 1 byte
    [InlineData(20971520, false)] // 20MB
    public async Task IsFileSizeAllowed_ShouldReturnCorrectResult(long fileSize, bool expected)
    {
        // Act
        var result = await _documentService.IsFileSizeAllowed(fileSize);

        // Assert
        Assert.Equal(expected, result);
    }    [Fact]
    public async Task GetDocumentPhysicalPathAsync_ShouldReturnCorrectPath()
    {
        // Arrange
        var document = new Document 
        { 
            Id = 1, 
            StoredFileName = "stored_file.pdf",
            DepartmentId = 1
        };
        var expectedPath = @"C:\temp\documents\stored_file.pdf";

        _documentDownloaderMock
            .Setup(x => x.GetDocumentPhysicalPathAsync(document.Id))
            .ReturnsAsync(expectedPath);

        // Act
        var result = await _documentService.GetDocumentPhysicalPathAsync(document);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPath, result);
        _documentDownloaderMock.Verify(x => x.GetDocumentPhysicalPathAsync(document.Id), Times.Once);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
