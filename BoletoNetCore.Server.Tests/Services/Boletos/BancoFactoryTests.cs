using BoletoNetCore.Server.Services.Boletos;

namespace BoletoNetCore.Server.Tests.Services.Boletos;

[Trait("Category", "Unit")]
public sealed class BancoFactoryTests
{
    private readonly BancoFactory sut = new();

    [Fact]
    public void Create_SupportedBank_ReturnsBancoInstance()
    {
        // Arrange - Bradesco bank 237 with valid carteira configuration
        var beneficiario = new Beneficiario
        {
            CPFCNPJ = "12345678000199",
            Nome = "Empresa Teste",
            ContaBancaria = new ContaBancaria
            {
                Agencia = "1234",
                DigitoAgencia = "5",
                Conta = "1234567",
                DigitoConta = "6",
                CarteiraPadrao = "09",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
            },
        };

        // Act
        var result = this.sut.Create(237, beneficiario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(237, result.Codigo);
        Assert.NotNull(result.Beneficiario);
    }

    [Fact]
    public void Create_UnsupportedBank_ThrowsArgumentException()
    {
        // Arrange
        var beneficiario = new Beneficiario
        {
            CPFCNPJ = "12345678000199",
            Nome = "Empresa Teste",
            ContaBancaria = new ContaBancaria
            {
                Agencia = "1234",
                DigitoAgencia = "5",
                Conta = "12345",
                DigitoConta = "6",
            },
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => this.sut.Create(999, beneficiario));
        Assert.Contains("999", ex.Message);
    }
}
