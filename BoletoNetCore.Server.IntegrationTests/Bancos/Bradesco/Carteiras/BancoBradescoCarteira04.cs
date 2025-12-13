using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.IntegrationTests.Fixtures;
using Google.Protobuf.WellKnownTypes;

namespace BoletoNetCore.Server.IntegrationTests.Bancos.Bradesco.Carteiras;

[Trait("Category", "Integration")]
[Trait("Banco", "Bradesco")]
[Trait("Carteira", "04")]
public sealed class BancoBradescoCarteira04 : IClassFixture<BoletoGrpcTestFixture>
{
    private readonly BoletoGrpcTestFixture fixture;

    public BancoBradescoCarteira04(BoletoGrpcTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(141.50, "453", "BB943A", "23791.23405 40000.000048 53012.345608 2 69040000014150", 2016, 9, 1)]
    [InlineData(2717.16, "456", "BB874A", "23791.23405 40000.000048 56012.345601 5 69340000271716", 2016, 10, 1)]
    [InlineData(297.21, "444", "BB834A", "23791.23405 40000.000048 44012.345607 6 69050000029721", 2016, 9, 2)]
    [InlineData(297.21, "468", "BB856A", "23791.23405 40000.000048 68012.345606 7 69350000029721", 2016, 10, 2)]
    [InlineData(297.21, "443", "BB833A", "23791.23405 40000.000048 43012.345609 8 69050000029721", 2016, 9, 2)]
    [InlineData(649.39, "414", "BB815A", "23791.23405 40000.000048 14012.345600 9 68730000064939", 2016, 8, 1)]
    [InlineData(270, "561", "BB932A", "23791.23405 40000.000055 61012.345601 1 70260000027000", 2017, 1, 1)]
    [InlineData(2924.11, "445", "BB874A", "23791.23405 40000.000048 45012.345604 1 69050000292411", 2016, 9, 2)]
    [InlineData(830, "562", "BB933A", "23791.23405 40000.000055 62012.345609 1 70260000083000", 2017, 1, 1)]
    public async Task Bradesco_04_LinhaDigitavel(
        decimal valorTitulo,
        string nossoNumero,
        string numeroDocumento,
        string expectedLinhaDigitavel,
        int ano, int mes, int dia)
    {
        // Arrange
        var request = CreateRequest(valorTitulo, nossoNumero, numeroDocumento, ano, mes, dia);

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.Single(response.Boletos);
        Assert.Equal(expectedLinhaDigitavel, response.Boletos[0].LinhaDigitavel);
    }

    [Theory]
    [InlineData(141.50, "453", "BB943A", "2", 2016, 9, 1)]
    [InlineData(2717.16, "456", "BB874A", "5", 2016, 10, 1)]
    [InlineData(297.21, "444", "BB834A", "6", 2016, 9, 2)]
    [InlineData(297.21, "468", "BB856A", "7", 2016, 10, 2)]
    [InlineData(297.21, "443", "BB833A", "8", 2016, 9, 2)]
    [InlineData(649.39, "414", "BB815A", "9", 2016, 8, 1)]
    [InlineData(270, "561", "BB932A", "1", 2017, 1, 1)]
    [InlineData(2924.11, "445", "BB874A", "1", 2016, 9, 2)]
    [InlineData(830, "562", "BB933A", "1", 2017, 1, 1)]
    public async Task Bradesco_04_DigitoVerificador(
        decimal valorTitulo,
        string nossoNumero,
        string numeroDocumento,
        string expectedDigitoVerificador,
        int ano, int mes, int dia)
    {
        // Arrange
        var request = CreateRequest(valorTitulo, nossoNumero, numeroDocumento, ano, mes, dia);

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.Single(response.Boletos);
        Assert.Equal(expectedDigitoVerificador, response.Boletos[0].DigitoVerificador);
    }

    [Theory]
    [InlineData(141.50, "453", "BB943A", "004/00000000453-1", 2016, 9, 1)]
    [InlineData(2717.16, "456", "BB874A", "004/00000000456-6", 2016, 10, 1)]
    [InlineData(297.21, "444", "BB834A", "004/00000000444-2", 2016, 9, 2)]
    [InlineData(297.21, "468", "BB856A", "004/00000000468-P", 2016, 10, 2)]
    [InlineData(297.21, "443", "BB833A", "004/00000000443-4", 2016, 9, 2)]
    [InlineData(649.39, "414", "BB815A", "004/00000000414-0", 2016, 8, 1)]
    [InlineData(270, "561", "BB932A", "004/00000000561-9", 2017, 1, 1)]
    [InlineData(2924.11, "445", "BB874A", "004/00000000445-0", 2016, 9, 2)]
    [InlineData(830, "562", "BB933A", "004/00000000562-7", 2017, 1, 1)]
    public async Task Bradesco_04_NossoNumeroFormatado(
        decimal valorTitulo,
        string nossoNumero,
        string numeroDocumento,
        string expectedNossoNumeroFormatado,
        int ano, int mes, int dia)
    {
        // Arrange
        var request = CreateRequest(valorTitulo, nossoNumero, numeroDocumento, ano, mes, dia);

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.Single(response.Boletos);
        Assert.Equal(expectedNossoNumeroFormatado, response.Boletos[0].NossoNumeroFormatado);
    }

    [Theory]
    [InlineData(141.50, "453", "BB943A", "23792690400000141501234040000000045301234560", 2016, 9, 1)]
    [InlineData(2717.16, "456", "BB874A", "23795693400002717161234040000000045601234560", 2016, 10, 1)]
    [InlineData(297.21, "444", "BB834A", "23796690500000297211234040000000044401234560", 2016, 9, 2)]
    [InlineData(297.21, "468", "BB856A", "23797693500000297211234040000000046801234560", 2016, 10, 2)]
    [InlineData(297.21, "443", "BB833A", "23798690500000297211234040000000044301234560", 2016, 9, 2)]
    [InlineData(649.39, "414", "BB815A", "23799687300000649391234040000000041401234560", 2016, 8, 1)]
    [InlineData(270, "561", "BB932A", "23791702600000270001234040000000056101234560", 2017, 1, 1)]
    [InlineData(2924.11, "445", "BB874A", "23791690500002924111234040000000044501234560", 2016, 9, 2)]
    [InlineData(830, "562", "BB933A", "23791702600000830001234040000000056201234560", 2017, 1, 1)]
    public async Task Bradesco_04_CodigoDeBarras(
        decimal valorTitulo,
        string nossoNumero,
        string numeroDocumento,
        string expectedCodigoDeBarras,
        int ano, int mes, int dia)
    {
        // Arrange
        var request = CreateRequest(valorTitulo, nossoNumero, numeroDocumento, ano, mes, dia);

        // Act
        var response = await this.fixture.Client.GerarBoletoAsync(request);

        // Assert
        Assert.Single(response.Boletos);
        Assert.Equal(expectedCodigoDeBarras, response.Boletos[0].CodigoBarras);
    }

    private static GerarBoletoRequest CreateRequest(
        decimal valorTitulo,
        string nossoNumero,
        string numeroDocumento,
        int ano, int mes, int dia)
    {
        var dataVencimento = new DateTime(ano, mes, dia, 0, 0, 0, DateTimeKind.Utc);

        var request = new GerarBoletoRequest
        {
            Banco = new Banco
            {
                Codigo = 237, // Bradesco
                Beneficiario = new Beneficiario
                {
                    CpfCnpj = "86875666000109",
                    Nome = "Beneficiario Teste",
                    Codigo = "1213141",
                    ContaBancaria = new ContaBancaria
                    {
                        Agencia = "1234",
                        DigitoAgencia = "X",
                        Conta = "123456",
                        DigitoConta = "X",
                        CarteiraPadrao = "04",
                        TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                        TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                        TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    },
                    Endereco = new Endereco
                    {
                        LogradouroEndereco = "Rua Teste do Beneficiário",
                        LogradouroNumero = "789",
                        LogradouroComplemento = "Cj 333",
                        Bairro = "Bairro",
                        Cidade = "Cidade",
                        Uf = "SP",
                        Cep = "65432987",
                    },
                },
            },
            OutputFormat = OutputFormat.Pdf,
        };

        request.Boletos.Add(new Boleto
        {
            Carteira = "04",
            TipoCarteira = TipoCarteira.CarteiraCobrancaSimples,
            DataVencimento = Timestamp.FromDateTime(dataVencimento),
            ValorTitulo = valorTitulo,
            NossoNumero = nossoNumero,
            NumeroDocumento = numeroDocumento,
            EspecieDocumento = TipoEspecieDocumento.Dm,
            Pagador = new Pagador
            {
                CpfCnpj = "71738978000101",
                Nome = "Pagador Teste PJ",
                Observacoes = "Matricula 123/4",
                Endereco = new Endereco
                {
                    LogradouroEndereco = "Avenida Testando",
                    LogradouroNumero = "123",
                    Bairro = "Bairro",
                    Cidade = "Cidade",
                    Uf = "SP",
                    Cep = "01013020",
                },
            },
        });

        return request;
    }
}
