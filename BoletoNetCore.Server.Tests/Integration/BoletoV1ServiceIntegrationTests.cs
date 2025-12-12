using BoletoNetCore.Server.Tests.Fixtures;
using ProtoV1 = BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class BoletoV1ServiceIntegrationTests : IClassFixture<BoletoGrpcTestFixture>
{
    private readonly BoletoGrpcTestFixture fixture;

    public BoletoV1ServiceIntegrationTests(BoletoGrpcTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task GerarBoleto_ValidBradescoRequest_ReturnsCompleteResponse()
    {
        // Arrange
        var request = CreateValidBradescoRequest();

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Boletos);
        Assert.NotEmpty(response.Conteudo);
        Assert.Equal("application/pdf", response.ContentType);

        var boleto = response.Boletos[0];
        Assert.Equal(44, boleto.CodigoBarras.Length);
        Assert.Equal(47, GetDigitsOnly(boleto.LinhaDigitavel).Length);
        Assert.False(string.IsNullOrWhiteSpace(boleto.NossoNumeroFormatado));
        Assert.Equal("DOC001", boleto.NumeroDocumento);
    }

    [Fact]
    public async Task GerarBoleto_MultipleBoletos_ReturnsAllGenerated()
    {
        // Arrange
        var request = ProtoV1.GerarBoletoRequest.Builder(bancoCodigo: 237)
            .ComCarteira("09", ProtoV1.TipoCarteira.CobrancaSimples)
            .ComBeneficiario(ProtoV1.Beneficiario.CriarValido(
                cpfCnpj: "12345678000195",
                nome: "Empresa Teste LTDA",
                contaBancaria: ProtoV1.ContaBancaria.CriarValido(
                    agencia: "1234",
                    conta: "0012345",
                    tipoFormaCadastramento: ProtoV1.TipoFormaCadastramento.ComRegistro,
                    tipoImpressaoBoleto: ProtoV1.TipoImpressaoBoleto.Empresa,
                    digitoAgencia: "5",
                    digitoConta: "6"),
                codigo: "123456"))
            .ComBoleto(
                CreateBoletoInput("00000000001", "DOC001"),
                CreateBoletoInput("00000000002", "DOC002"))
            .Build();

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.Equal(2, response.Boletos.Count);
        Assert.All(response.Boletos, boleto =>
        {
            Assert.Equal(44, boleto.CodigoBarras.Length);
            Assert.Equal(47, GetDigitsOnly(boleto.LinhaDigitavel).Length);
        });
    }

    [Fact]
    public async Task GerarBoleto_UsingSteppedBuilder_ReturnsValidResponse()
    {
        // Arrange - demonstrates the stepped builder pattern with local dates (Brazil)
        var vencimento = DateTime.Today.AddDays(30);

        var boletoInput = ProtoV1.BoletoInput.Builder()
            .ComPagador("98765432100", "Cliente Teste Builder")
            .ComVencimento(vencimento)
            .ComValor(250.75m)
            .ComNossoNumero("00000000003")
            .ComDocumento("DOC-BUILDER-001", ProtoV1.TipoEspecieDocumento.Dm)
            .ComMulta(vencimento.AddDays(1), 5.00m)
            .ComDesconto(DateTime.Today.AddDays(10), 10.00m)
            .ComMensagemCaixa("Boleto gerado via stepped builder")
            .Build();

        var request = ProtoV1.GerarBoletoRequest.Builder(bancoCodigo: 237)
            .ComCarteira("09", ProtoV1.TipoCarteira.CobrancaSimples)
            .ComBeneficiario(ProtoV1.Beneficiario.CriarValido(
                cpfCnpj: "12345678000195",
                nome: "Empresa Teste LTDA",
                contaBancaria: ProtoV1.ContaBancaria.CriarValido(
                    agencia: "1234",
                    conta: "0012345",
                    tipoFormaCadastramento: ProtoV1.TipoFormaCadastramento.ComRegistro,
                    tipoImpressaoBoleto: ProtoV1.TipoImpressaoBoleto.Empresa,
                    digitoAgencia: "5",
                    digitoConta: "6"),
                codigo: "123456"))
            .ComBoleto(boletoInput)
            .Build();

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Boletos);
        Assert.NotEmpty(response.Conteudo);
        Assert.Equal("application/pdf", response.ContentType);

        var boleto = response.Boletos[0];
        Assert.Equal(44, boleto.CodigoBarras.Length);
        Assert.Equal(47, GetDigitsOnly(boleto.LinhaDigitavel).Length);
        Assert.Equal("DOC-BUILDER-001", boleto.NumeroDocumento);
    }

    private static ProtoV1.GerarBoletoRequest CreateValidBradescoRequest()
    {
        return ProtoV1.GerarBoletoRequest.Builder(bancoCodigo: 237)
            .ComCarteira("09", ProtoV1.TipoCarteira.CobrancaSimples)
            .ComBeneficiario(ProtoV1.Beneficiario.CriarValido(
                cpfCnpj: "12345678000195",
                nome: "Empresa Teste LTDA",
                contaBancaria: ProtoV1.ContaBancaria.CriarValido(
                    agencia: "1234",
                    conta: "0012345",
                    tipoFormaCadastramento: ProtoV1.TipoFormaCadastramento.ComRegistro,
                    tipoImpressaoBoleto: ProtoV1.TipoImpressaoBoleto.Empresa,
                    digitoAgencia: "5",
                    digitoConta: "6"),
                codigo: "123456"))
            .ComBoleto(CreateBoletoInput("00000000001", "DOC001"))
            .Build();
    }

    private static ProtoV1.BoletoInput CreateBoletoInput(string nossoNumero, string numeroDocumento)
    {
        return ProtoV1.BoletoInput.Builder()
            .ComPagador("98765432100", "Cliente Teste")
            .ComVencimento(DateTime.Today.AddDays(30))
            .ComValor(150m)
            .ComNossoNumero(nossoNumero)
            .ComDocumento(numeroDocumento, ProtoV1.TipoEspecieDocumento.Dm)
            .Build();
    }

    private static string GetDigitsOnly(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
