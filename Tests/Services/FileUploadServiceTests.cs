using IntranetDocumentos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace IntranetDocumentos.Tests.Services;

/// <summary>
/// Testes unitários para FileUploadService
/// </summary>
public class FileUploadServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private readonly Mock<ILogger<FileUploadService>> _loggerMock;
    private readonly FileUploadService _fileUploadService;
    private readonly string _tempDirectory;

    public FileUploadServiceTests()
    {
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<FileUploadService>>();
        
        // Criar diretório temporário para testes
        _tempDirectory = Path.Combine(Path.GetTempPath(), "FileUploadTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Configurar mock do ambiente web
        _webHostEnvironmentMock
            .Setup(x => x.WebRootPath)
            .Returns(_tempDirectory);

        _fileUploadService = new FileUploadService(_webHostEnvironmentMock.Object, _loggerMock.Object);
    }    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Just a basic test to ensure the service can be instantiated
        Assert.NotNull(_fileUploadService);
    }

    [Fact]
    public void GenerateUniqueFileName_ShouldGenerateUniqueNames()
    {
        // Arrange
        var originalFileName = "test.jpg";

        // Act
        var result1 = _fileUploadService.GenerateUniqueFileName(originalFileName);
        var result2 = _fileUploadService.GenerateUniqueFileName(originalFileName);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.Contains(".jpg", result1);
        Assert.Contains(".jpg", result2);
    }

    public void Dispose()
    {
        // Limpar diretório temporário
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
