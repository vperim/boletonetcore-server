using BoletoNetCore.Server.IntegrationTests.Fixtures;
using Google.Protobuf.WellKnownTypes;
using ProtoV1 = BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.IntegrationTests;

[Trait("Category", "Endpoints")]
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
        var request = new ProtoV1.GerarBoletoRequest
        {
            Banco = new ProtoV1.Banco
            {
                Codigo = 237,
                Beneficiario = new ProtoV1.Beneficiario
                {
                    CpfCnpj = "12345678000195",
                    Nome = "Empresa Teste LTDA",
                    Codigo = "123456",
                    ContaBancaria = new ProtoV1.ContaBancaria
                    {
                        Agencia = "1234",
                        DigitoAgencia = "5",
                        Conta = "0012345",
                        DigitoConta = "6",
                        CarteiraPadrao = "09",
                        TipoFormaCadastramento = ProtoV1.TipoFormaCadastramento.ComRegistro,
                        TipoImpressaoBoleto = ProtoV1.TipoImpressaoBoleto.Empresa,
                    },
                },
            },
            OutputFormat = ProtoV1.BoletoOutputFormat.Pdf,
        };
        request.Boletos.Add(CreateBoletoInput("00000000001", "DOC001"));
        request.Boletos.Add(CreateBoletoInput("00000000002", "DOC002"));

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

    private static ProtoV1.GerarBoletoRequest CreateValidBradescoRequest()
    {
        var request = new ProtoV1.GerarBoletoRequest
        {
            Banco = new ProtoV1.Banco
            {
                Codigo = 237,
                Beneficiario = new ProtoV1.Beneficiario
                {
                    CpfCnpj = "12345678000195",
                    Nome = "Empresa Teste LTDA",
                    Codigo = "123456",
                    ContaBancaria = new ProtoV1.ContaBancaria
                    {
                        Agencia = "1234",
                        DigitoAgencia = "5",
                        Conta = "0012345",
                        DigitoConta = "6",
                        CarteiraPadrao = "09",
                        TipoFormaCadastramento = ProtoV1.TipoFormaCadastramento.ComRegistro,
                        TipoImpressaoBoleto = ProtoV1.TipoImpressaoBoleto.Empresa,
                    },
                },
            },
            OutputFormat = ProtoV1.BoletoOutputFormat.Pdf,
        };
        request.Boletos.Add(CreateBoletoInput("00000000001", "DOC001"));
        return request;
    }

    private static ProtoV1.Boleto CreateBoletoInput(string nossoNumero, string numeroDocumento)
    {
        return new ProtoV1.Boleto
        {
            Carteira = "09",
            TipoCarteira = ProtoV1.TipoCarteira.CarteiraCobrancaSimples,
            Pagador = new ProtoV1.Pagador
            {
                CpfCnpj = "98765432100",
                Nome = "Cliente Teste",
            },
            DataVencimento = Timestamp.FromDateTime(DateTime.Today.AddDays(30).ToUniversalTime()),
            ValorTitulo = 150m,
            NossoNumero = nossoNumero,
            NumeroDocumento = numeroDocumento,
            EspecieDocumento = ProtoV1.TipoEspecieDocumento.Dm,
        };
    }

    private static string GetDigitsOnly(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
