using BoletoNetCore.Server.Services.Boletos;
using Google.Protobuf.WellKnownTypes;
using ProtoTypes = BoletoNetCore.Server.Contracts.Generated.Types;
using ProtoV1 = BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Tests.Services.Boletos;

[Trait("Category", "Unit")]
public sealed class BoletoMapperTests
{
    private readonly BoletoMapper sut = new();

    [Fact]
    public void MapContaBancaria_ValidRequest_MapsAllFields()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = this.sut.MapContaBancaria(request);

        // Assert
        Assert.Equal("1234", result.Agencia);
        Assert.Equal("5", result.DigitoAgencia);
        Assert.Equal("12345", result.Conta);
        Assert.Equal("6", result.DigitoConta);
        Assert.Equal("013", result.OperacaoConta);
        Assert.Equal("123456", result.CodigoConvenio);
        Assert.Equal("109", result.CarteiraPadrao);
        Assert.Equal("19", result.VariacaoCarteiraPadrao);
        Assert.Equal(TipoCarteira.CarteiraCobrancaSimples, result.TipoCarteiraPadrao);
        Assert.Equal(TipoFormaCadastramento.ComRegistro, result.TipoFormaCadastramento);
        Assert.Equal(TipoImpressaoBoleto.Empresa, result.TipoImpressaoBoleto);
    }

    [Fact]
    public void MapBoleto_ValidInput_MapsCoreFields()
    {
        // Arrange
        var protoInput = new ProtoV1.BoletoInput
        {
            Pagador = CreateValidPagador(),
            DataVencimento = Timestamp.FromDateTime(DateTime.SpecifyKind(new DateTime(2025, 12, 31), DateTimeKind.Utc)),
            ValorTitulo = new ProtoTypes.Money { Units = 150, Nanos = 500_000_000 },
            NossoNumero = "12345678",
            NumeroDocumento = "DOC001",
            EspecieDocumento = ProtoV1.TipoEspecieDocumento.Dm,
            Aceite = "S",
        };
        var banco = CreateBancoForTest();
        var boleto = new BoletoNetCore.Boleto(banco, ignorarCarteira: true);

        // Act
        this.sut.MapBoleto(protoInput, boleto);

        // Assert
        Assert.Equal(new DateTime(2025, 12, 31), boleto.DataVencimento);
        Assert.Equal(150.50m, boleto.ValorTitulo);
        Assert.Equal("12345678", boleto.NossoNumero);
        Assert.Equal("DOC001", boleto.NumeroDocumento);
        Assert.Equal(TipoEspecieDocumento.DM, boleto.EspecieDocumento);
        Assert.Equal("S", boleto.Aceite);
        Assert.NotNull(boleto.Pagador);
    }

    private static IBanco CreateBancoForTest()
    {
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
        return new BancoFactory().Create(237, beneficiario); // Bradesco
    }

    private static ProtoV1.GerarBoletoRequest CreateValidRequest()
    {
        return new ProtoV1.GerarBoletoRequest
        {
            Beneficiario = new ProtoV1.Beneficiario
            {
                CpfCnpj = "12345678000199",
                Nome = "Empresa Teste",
                ContaBancaria = new ProtoV1.ContaBancaria
                {
                    Agencia = "1234",
                    DigitoAgencia = "5",
                    Conta = "12345",
                    DigitoConta = "6",
                    OperacaoConta = "013",
                    CodigoConvenio = "123456",
                    TipoFormaCadastramento = ProtoV1.TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = ProtoV1.TipoImpressaoBoleto.Empresa,
                },
            },
            Carteira = "109",
            VariacaoCarteira = "19",
            TipoCarteira = ProtoV1.TipoCarteira.CobrancaSimples,
        };
    }

    private static ProtoV1.Pagador CreateValidPagador()
    {
        return new ProtoV1.Pagador
        {
            CpfCnpj = "98765432100",
            Nome = "Pagador Teste",
            Endereco = new ProtoTypes.Endereco
            {
                Logradouro = "Rua Teste",
                Numero = "123",
                Cidade = "Cidade Teste",
                Uf = "SP",
                Cep = "01234567",
            },
        };
    }
}
