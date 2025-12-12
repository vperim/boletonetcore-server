using BoletoNetCore.Server.Contracts.Generated.V1;
using ProtoTypes = BoletoNetCore.Server.Contracts.Generated.Types;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Maps between proto contracts and BoletoNetCore domain objects.
/// </summary>
public sealed class BoletoMapper : IBoletoMapper
{
    public ContaBancaria MapContaBancaria(GerarBoletoRequest request)
    {
        var proto = request.Beneficiario.ContaBancaria;
        return new ContaBancaria
        {
            Agencia = proto.Agencia,
            DigitoAgencia = proto.DigitoAgencia,
            Conta = proto.Conta,
            DigitoConta = proto.DigitoConta,
            OperacaoConta = proto.OperacaoConta,
            CodigoConvenio = proto.CodigoConvenio,
            CarteiraPadrao = request.Carteira,
            VariacaoCarteiraPadrao = request.VariacaoCarteira,
            TipoCarteiraPadrao = MapTipoCarteira(request.TipoCarteira),
            TipoFormaCadastramento = MapTipoFormaCadastramento(proto.TipoFormaCadastramento),
            TipoImpressaoBoleto = MapTipoImpressaoBoleto(proto.TipoImpressaoBoleto),
            ChavePix = proto.ChavePix,
            TipoChavePix = MapTipoChavePix(proto.TipoChavePix),
        };
    }

    public Beneficiario MapBeneficiario(Contracts.Generated.V1.Beneficiario proto, ContaBancaria conta)
    {
        return new Beneficiario
        {
            CPFCNPJ = proto.CpfCnpj,
            Nome = proto.Nome,
            Codigo = proto.Codigo,
            CodigoDV = proto.CodigoDv,
            CodigoTransmissao = proto.CodigoTransmissao,
            Endereco = MapEndereco(proto.Endereco),
            ContaBancaria = conta,
        };
    }

    public Pagador MapPagador(Contracts.Generated.V1.Pagador proto)
    {
        return new Pagador
        {
            CPFCNPJ = proto.CpfCnpj,
            Nome = proto.Nome,
            Endereco = MapEndereco(proto.Endereco),
            Telefone = proto.Telefone,
            Observacoes = proto.Observacoes,
        };
    }

    public void MapBoleto(BoletoInput proto, Boleto boleto)
    {
        boleto.Pagador = MapPagador(proto.Pagador);
        boleto.DataVencimento = proto.DataVencimento.ToDateTime();
        boleto.ValorTitulo = MapMoney(proto.ValorTitulo);
        boleto.NossoNumero = proto.NossoNumero;
        boleto.NumeroDocumento = proto.NumeroDocumento;
        boleto.EspecieDocumento = MapTipoEspecieDocumento(proto.EspecieDocumento);
        boleto.Aceite = string.IsNullOrEmpty(proto.Aceite) ? "N" : proto.Aceite;

        if (proto.DataEmissao != null)
            boleto.DataEmissao = proto.DataEmissao.ToDateTime();

        if (proto.DataProcessamento != null)
            boleto.DataProcessamento = proto.DataProcessamento.ToDateTime();

        if (!string.IsNullOrEmpty(proto.NumeroControleParticipante))
            boleto.NumeroControleParticipante = proto.NumeroControleParticipante;

        // Interest
        if (proto.DataJuros != null)
            boleto.DataJuros = proto.DataJuros.ToDateTime();

        if (proto.ValorJurosDia != null)
            boleto.ValorJurosDia = MapMoney(proto.ValorJurosDia);

        if (!string.IsNullOrEmpty(proto.PercentualJurosDia) &&
            decimal.TryParse(proto.PercentualJurosDia, out var percJuros))
            boleto.PercentualJurosDia = percJuros;

        boleto.TipoJuros = MapTipoJuros(proto.TipoJuros);

        // Penalty
        if (proto.DataMulta != null)
            boleto.DataMulta = proto.DataMulta.ToDateTime();

        if (proto.ValorMulta != null)
            boleto.ValorMulta = MapMoney(proto.ValorMulta);

        if (!string.IsNullOrEmpty(proto.PercentualMulta) &&
            decimal.TryParse(proto.PercentualMulta, out var percMulta))
            boleto.PercentualMulta = percMulta;

        boleto.TipoCodigoMulta = MapTipoCodigoMulta(proto.TipoCodigoMulta);

        // Discounts
        if (proto.Descontos.Count > 0)
        {
            boleto.DataDesconto = proto.Descontos[0].Data.ToDateTime();
            boleto.ValorDesconto = MapMoney(proto.Descontos[0].Valor);
        }
        if (proto.Descontos.Count > 1)
        {
            boleto.DataDesconto2 = proto.Descontos[1].Data.ToDateTime();
            boleto.ValorDesconto2 = MapMoney(proto.Descontos[1].Valor);
        }
        if (proto.Descontos.Count > 2)
        {
            boleto.DataDesconto3 = proto.Descontos[2].Data.ToDateTime();
            boleto.ValorDesconto3 = MapMoney(proto.Descontos[2].Valor);
        }

        // Instructions
        if (!string.IsNullOrEmpty(proto.CodigoInstrucao1))
            boleto.CodigoInstrucao1 = proto.CodigoInstrucao1;

        if (!string.IsNullOrEmpty(proto.CodigoInstrucao2))
            boleto.CodigoInstrucao2 = proto.CodigoInstrucao2;

        if (!string.IsNullOrEmpty(proto.MensagemInstrucoesCaixa))
            boleto.MensagemInstrucoesCaixa = proto.MensagemInstrucoesCaixa;

        boleto.ImprimirValoresAuxiliares = proto.ImprimirValoresAuxiliares;
    }

    public BoletoGerado MapToResponse(Boleto boleto)
    {
        return new BoletoGerado
        {
            NossoNumero = boleto.NossoNumero,
            NossoNumeroDv = boleto.NossoNumeroDV,
            NossoNumeroFormatado = boleto.NossoNumeroFormatado,
            CodigoBarras = boleto.CodigoBarra.CodigoDeBarras,
            LinhaDigitavel = boleto.CodigoBarra.LinhaDigitavel,
            NumeroDocumento = boleto.NumeroDocumento,
        };
    }

    private static Endereco MapEndereco(ProtoTypes.Endereco? proto)
    {
        if (proto == null)
            return new Endereco();

        return new Endereco
        {
            LogradouroEndereco = proto.Logradouro,
            LogradouroNumero = proto.Numero,
            LogradouroComplemento = proto.Complemento,
            Bairro = proto.Bairro,
            Cidade = proto.Cidade,
            UF = proto.Uf,
            CEP = proto.Cep,
        };
    }

    private static decimal MapMoney(ProtoTypes.Money? money)
    {
        if (money == null)
            return 0m;

        return money.Units + (money.Nanos / 1_000_000_000m);
    }

    public TipoCarteira MapTipoCarteira(Contracts.Generated.V1.TipoCarteira proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoCarteira.CobrancaSimples => TipoCarteira.CarteiraCobrancaSimples,
            Contracts.Generated.V1.TipoCarteira.CobrancaVinculada => TipoCarteira.CarteiraCobrancaVinculada,
            Contracts.Generated.V1.TipoCarteira.CobrancaCaucionada => TipoCarteira.CarteiraCobrancaCaucionada,
            Contracts.Generated.V1.TipoCarteira.CobrancaDescontada => TipoCarteira.CarteiraCobrancaDescontada,
            Contracts.Generated.V1.TipoCarteira.CobrancaVendor => TipoCarteira.CarteiraCobrancaVendor,
            Contracts.Generated.V1.TipoCarteira.CobrancaDebito => TipoCarteira.CarteiraCobrancaDebito,
            _ => TipoCarteira.CarteiraCobrancaSimples,
        };
    }

    private static TipoEspecieDocumento MapTipoEspecieDocumento(Contracts.Generated.V1.TipoEspecieDocumento proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoEspecieDocumento.Ch => TipoEspecieDocumento.CH,
            Contracts.Generated.V1.TipoEspecieDocumento.Dm => TipoEspecieDocumento.DM,
            Contracts.Generated.V1.TipoEspecieDocumento.Dmi => TipoEspecieDocumento.DMI,
            Contracts.Generated.V1.TipoEspecieDocumento.Ds => TipoEspecieDocumento.DS,
            Contracts.Generated.V1.TipoEspecieDocumento.Dsi => TipoEspecieDocumento.DSI,
            Contracts.Generated.V1.TipoEspecieDocumento.Dr => TipoEspecieDocumento.DR,
            Contracts.Generated.V1.TipoEspecieDocumento.Lc => TipoEspecieDocumento.LC,
            Contracts.Generated.V1.TipoEspecieDocumento.Ncc => TipoEspecieDocumento.NCC,
            Contracts.Generated.V1.TipoEspecieDocumento.Nce => TipoEspecieDocumento.NCE,
            Contracts.Generated.V1.TipoEspecieDocumento.Nci => TipoEspecieDocumento.NCI,
            Contracts.Generated.V1.TipoEspecieDocumento.Ncr => TipoEspecieDocumento.NCR,
            Contracts.Generated.V1.TipoEspecieDocumento.Np => TipoEspecieDocumento.NP,
            Contracts.Generated.V1.TipoEspecieDocumento.Npr => TipoEspecieDocumento.NPR,
            Contracts.Generated.V1.TipoEspecieDocumento.Tm => TipoEspecieDocumento.TM,
            Contracts.Generated.V1.TipoEspecieDocumento.Ts => TipoEspecieDocumento.TS,
            Contracts.Generated.V1.TipoEspecieDocumento.Ns => TipoEspecieDocumento.NS,
            Contracts.Generated.V1.TipoEspecieDocumento.Rc => TipoEspecieDocumento.RC,
            Contracts.Generated.V1.TipoEspecieDocumento.Fat => TipoEspecieDocumento.FAT,
            Contracts.Generated.V1.TipoEspecieDocumento.Nd => TipoEspecieDocumento.ND,
            Contracts.Generated.V1.TipoEspecieDocumento.Ap => TipoEspecieDocumento.AP,
            Contracts.Generated.V1.TipoEspecieDocumento.Me => TipoEspecieDocumento.ME,
            Contracts.Generated.V1.TipoEspecieDocumento.Pc => TipoEspecieDocumento.PC,
            Contracts.Generated.V1.TipoEspecieDocumento.Nf => TipoEspecieDocumento.NF,
            Contracts.Generated.V1.TipoEspecieDocumento.Dd => TipoEspecieDocumento.DD,
            Contracts.Generated.V1.TipoEspecieDocumento.Cc => TipoEspecieDocumento.CC,
            Contracts.Generated.V1.TipoEspecieDocumento.Bdp => TipoEspecieDocumento.BP,
            Contracts.Generated.V1.TipoEspecieDocumento.Ou => TipoEspecieDocumento.OU,
            _ => TipoEspecieDocumento.NaoDefinido,
        };
    }

    private static TipoJuros MapTipoJuros(Contracts.Generated.V1.TipoJuros proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoJuros.Isento => TipoJuros.Isento,
            Contracts.Generated.V1.TipoJuros.Simples => TipoJuros.Simples,
            Contracts.Generated.V1.TipoJuros.Ida => TipoJuros.IDA,
            _ => TipoJuros.Isento,
        };
    }

    private static BoletoNetCore.Enums.TipoCodigoMulta MapTipoCodigoMulta(Contracts.Generated.V1.TipoCodigoMulta proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoCodigoMulta.Valor => BoletoNetCore.Enums.TipoCodigoMulta.Valor,
            Contracts.Generated.V1.TipoCodigoMulta.Percentual => BoletoNetCore.Enums.TipoCodigoMulta.Percentual,
            Contracts.Generated.V1.TipoCodigoMulta.Dispensar => BoletoNetCore.Enums.TipoCodigoMulta.DispensarCobrancaMulta,
            _ => BoletoNetCore.Enums.TipoCodigoMulta.Valor,
        };
    }

    private static TipoFormaCadastramento MapTipoFormaCadastramento(Contracts.Generated.V1.TipoFormaCadastramento proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoFormaCadastramento.ComRegistro => TipoFormaCadastramento.ComRegistro,
            Contracts.Generated.V1.TipoFormaCadastramento.SemRegistro => TipoFormaCadastramento.SemRegistro,
            Contracts.Generated.V1.TipoFormaCadastramento.DebitoAutomatico => TipoFormaCadastramento.DebitoAutomatico,
            _ => TipoFormaCadastramento.ComRegistro,
        };
    }

    private static TipoImpressaoBoleto MapTipoImpressaoBoleto(Contracts.Generated.V1.TipoImpressaoBoleto proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoImpressaoBoleto.Banco => TipoImpressaoBoleto.Banco,
            Contracts.Generated.V1.TipoImpressaoBoleto.Empresa => TipoImpressaoBoleto.Empresa,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoPreEmite => TipoImpressaoBoleto.BancoPreEmiteClienteComplementa,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoReemite => TipoImpressaoBoleto.BancoReemite,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoNaoReemite => TipoImpressaoBoleto.BancoNaoReemite,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoEmitenteAberta => TipoImpressaoBoleto.BancoEmitenteAberta,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoAutoEnvelopavel => TipoImpressaoBoleto.BancoEmitenteAutoEnvolopavel,
            _ => TipoImpressaoBoleto.Empresa,
        };
    }

    private static TipoChavePix MapTipoChavePix(Contracts.Generated.V1.TipoChavePix proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoChavePix.Cpf => TipoChavePix.CPF,
            Contracts.Generated.V1.TipoChavePix.Cnpj => TipoChavePix.CNPJ,
            Contracts.Generated.V1.TipoChavePix.Celular => TipoChavePix.Celular,
            Contracts.Generated.V1.TipoChavePix.Email => TipoChavePix.Email,
            Contracts.Generated.V1.TipoChavePix.Aleatoria => TipoChavePix.Aleatoria,
            _ => default,
        };
    }
}
