using BoletoNetCore.Server.Services.Boletos;

namespace BoletoNetCore.Server.Tests.Services.Boletos;

[Trait("Category", "Unit")]
public sealed class BancoFactoryTests
{
    private readonly BancoFactory sut = new();

    [Fact]
    public void Create_SupportedBank_ReturnsBancoInstance()
    {
        // Act
        var result = this.sut.Create(237);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(237, result.Codigo);
    }

    [Fact]
    public void Create_UnsupportedBank_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => this.sut.Create(999));
        Assert.Contains("999", ex.Message);
    }
}
