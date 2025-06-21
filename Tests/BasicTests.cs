using Xunit;

namespace IntranetDocumentos.Tests;

/// <summary>
/// Testes básicos para verificar a configuração
/// </summary>
public class BasicTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Teste simples para verificar se o ambiente de teste está funcionando
        Assert.True(true);
    }

    [Fact]
    public void StringTest_ShouldWork()
    {
        // Teste simples de string
        var testString = "Hello World";
        Assert.NotNull(testString);
        Assert.Equal("Hello World", testString);
    }
}
