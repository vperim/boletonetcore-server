using BoletoNetCore;
using BoletoNetCore.Server.Services.Boletos;
using Google.Protobuf.WellKnownTypes;
using ProtoTypes = BoletoNetCore.Server.Contracts.Generated.Types;
using ProtoV1 = BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Tests.Services.Boletos;

[Trait("Category", "Unit")]
public sealed class BoletoMapperTests
{
    [Fact]
    public void MapBoleto_ValidInput_MapsCoreFields()
    {
        // Arrange
        var protoInput = new ProtoV1.Boleto
        {
            Carteira = "09",
            TipoCarteira = ProtoV1.TipoCarteira.CarteiraCobrancaSimples,
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
        BoletoMapper.MapBoleto(protoInput, boleto);

        // Assert
        Assert.Equal("09", boleto.Carteira);
        Assert.Equal(TipoCarteira.CarteiraCobrancaSimples, boleto.TipoCarteira);
        Assert.Equal(new DateTime(2025, 12, 31), boleto.DataVencimento);
        Assert.Equal(150.50m, boleto.ValorTitulo);
        Assert.Equal("12345678", boleto.NossoNumero);
        Assert.Equal("DOC001", boleto.NumeroDocumento);
        Assert.Equal(TipoEspecieDocumento.DM, boleto.EspecieDocumento);
        Assert.Equal("S", boleto.Aceite);
        Assert.NotNull(boleto.Pagador);
    }

    [Fact]
    public void MapBancoBeneficiario_ValidInput_MapsBeneficiarioAndContaBancaria()
    {
        // Arrange
        var protoBeneficiario = new ProtoV1.Beneficiario
        {
            CpfCnpj = "12345678000199",
            Nome = "Empresa Teste",
            Codigo = "123456",
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
        };
        var target = new Beneficiario();

        // Act
        BoletoMapper.MapBancoBeneficiario(protoBeneficiario, target);

        // Assert - Beneficiario fields
        Assert.Equal("12345678000199", target.CPFCNPJ);
        Assert.Equal("Empresa Teste", target.Nome);
        Assert.Equal("123456", target.Codigo);

        // Assert - ContaBancaria fields
        Assert.NotNull(target.ContaBancaria);
        Assert.Equal("1234", target.ContaBancaria.Agencia);
        Assert.Equal("5", target.ContaBancaria.DigitoAgencia);
        Assert.Equal("12345", target.ContaBancaria.Conta);
        Assert.Equal("6", target.ContaBancaria.DigitoConta);
        Assert.Equal("013", target.ContaBancaria.OperacaoConta);
        Assert.Equal("123456", target.ContaBancaria.CodigoConvenio);
        Assert.Equal(TipoFormaCadastramento.ComRegistro, target.ContaBancaria.TipoFormaCadastramento);
        Assert.Equal(TipoImpressaoBoleto.Empresa, target.ContaBancaria.TipoImpressaoBoleto);
    }

    private static IBanco CreateBancoForTest()
    {
        return new BancoFactory().Create(237); // Bradesco
    }

    private static ProtoV1.Pagador CreateValidPagador()
    {
        return new ProtoV1.Pagador
        {
            CpfCnpj = "98765432100",
            Nome = "Pagador Teste",
            Endereco = new ProtoTypes.Endereco
            {
                LogradouroEndereco = "Rua Teste",
                LogradouroNumero = "123",
                Cidade = "Cidade Teste",
                Uf = "SP",
                Cep = "01234567",
            },
        };
    }
}
