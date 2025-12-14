using BoletoNetCore.Server.Contracts.Generated.V1;
using ProtoTypes = BoletoNetCore.Server.Contracts.Generated.Types;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Maps between proto contracts and BoletoNetCore domain objects.
/// </summary>
public static class BoletoMapper
{
    public static Pagador MapPagador(Contracts.Generated.V1.Pagador proto)
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

    /// <summary>
    /// Applies post-validation overrides to boleto.
    /// These fields must be applied AFTER ValidarDados() to prevent being overwritten by library formatters.
    /// </summary>
    public static void ApplyPostValidationOverrides(Contracts.Generated.V1.Boleto proto, Boleto boleto)
    {
        // MensagemInstrucoesCaixaFormatado - user can provide pre-formatted message to override library auto-generation
        if (!string.IsNullOrEmpty(proto.MensagemInstrucoesCaixaFormatado))
            boleto.MensagemInstrucoesCaixaFormatado = proto.MensagemInstrucoesCaixaFormatado;
    }

    public static void MapBoleto(Contracts.Generated.V1.Boleto proto, Boleto boleto)
    {
        // Carteira
        if (!string.IsNullOrEmpty(proto.Carteira))
            boleto.Carteira = proto.Carteira;

        if (!string.IsNullOrEmpty(proto.VariacaoCarteira))
            boleto.VariacaoCarteira = proto.VariacaoCarteira;

        if (proto.TipoCarteira != Contracts.Generated.V1.TipoCarteira.Unspecified)
            boleto.TipoCarteira = MapTipoCarteira(proto.TipoCarteira);

        // Core
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

        // Currency
        if (proto.CodigoMoeda != 0)
            boleto.CodigoMoeda = proto.CodigoMoeda;

        if (!string.IsNullOrEmpty(proto.EspecieMoeda))
            boleto.EspecieMoeda = proto.EspecieMoeda;

        if (proto.QuantidadeMoeda != 0)
            boleto.QuantidadeMoeda = proto.QuantidadeMoeda;

        if (!string.IsNullOrEmpty(proto.ValorMoeda))
            boleto.ValorMoeda = proto.ValorMoeda;

        boleto.TipoMoeda = MapTipoMoeda(proto.TipoMoeda);

        // Document
        if (!string.IsNullOrEmpty(proto.CarteiraImpressaoBoleto))
            boleto.CarteiraImpressaoBoleto = proto.CarteiraImpressaoBoleto;

        if (!string.IsNullOrEmpty(proto.NumeroParcela))
            boleto.NumeroParcela = proto.NumeroParcela;

        if (!string.IsNullOrEmpty(proto.UsoBanco))
            boleto.UsoBanco = proto.UsoBanco;

        // Messages
        boleto.ImprimirMensagemInstrucao = proto.ImprimirMensagemInstrucao;

        if (!string.IsNullOrEmpty(proto.MensagemArquivoRemessa))
            boleto.MensagemArquivoRemessa = proto.MensagemArquivoRemessa;

        if (!string.IsNullOrEmpty(proto.MensagemProtesto))
            boleto.MensagemProtesto = proto.MensagemProtesto;

        if (!string.IsNullOrEmpty(proto.MensagemLivre))
            boleto.MensagemLivre = proto.MensagemLivre;

        // Values
        if (proto.ValorOutrasDespesas != null)
            boleto.ValorOutrasDespesas = MapMoney(proto.ValorOutrasDespesas);

        if (proto.ValorOutrosCreditos != null)
            boleto.ValorOutrosCreditos = MapMoney(proto.ValorOutrosCreditos);

        if (proto.ValorIof != null)
            boleto.ValorIOF = MapMoney(proto.ValorIof);

        if (proto.ValorAbatimento != null)
            boleto.ValorAbatimento = MapMoney(proto.ValorAbatimento);

        // Automatic Debit
        if (!string.IsNullOrEmpty(proto.EmiteBoletoDebitoAutomatico))
            boleto.EmiteBoletoDebitoAutomatico = proto.EmiteBoletoDebitoAutomatico;

        if (!string.IsNullOrEmpty(proto.RateioCredito))
            boleto.RateioCredito = proto.RateioCredito;

        if (!string.IsNullOrEmpty(proto.AvisoDebitoAutomaticoContaCorrente))
            boleto.AvisoDebitoAutomaticoContaCorrente = proto.AvisoDebitoAutomaticoContaCorrente;

        if (!string.IsNullOrEmpty(proto.QuantidadePagamentos))
            boleto.QuantidadePagamentos = proto.QuantidadePagamentos;

        if (!string.IsNullOrEmpty(proto.AgenciaDebitada))
            boleto.AgenciaDebitada = proto.AgenciaDebitada;

        if (!string.IsNullOrEmpty(proto.ContaDebitada))
            boleto.ContaDebitada = proto.ContaDebitada;

        if (!string.IsNullOrEmpty(proto.DigitoVerificadorAgenciaDebitada))
            boleto.DigitoVerificadorAgenciaDebitada = proto.DigitoVerificadorAgenciaDebitada;

        if (!string.IsNullOrEmpty(proto.DigitoVerificadorAgenciaContaDebitada))
            boleto.DigitoVerificadorAgenciaContaDebitada = proto.DigitoVerificadorAgenciaContaDebitada;

        // Protest/Return
        boleto.CodigoProtesto = MapTipoCodigoProtesto(proto.CodigoProtesto);

        if (proto.DiasProtesto != 0)
            boleto.DiasProtesto = proto.DiasProtesto;

        boleto.CodigoBaixaDevolucao = MapTipoCodigoBaixaDevolucao(proto.CodigoBaixaDevolucao);

        if (proto.DiasBaixaDevolucao != 0)
            boleto.DiasBaixaDevolucao = proto.DiasBaixaDevolucao;

        // Instructions (complements and instruction 3)
        if (!string.IsNullOrEmpty(proto.ComplementoInstrucao1))
            boleto.ComplementoInstrucao1 = proto.ComplementoInstrucao1;

        if (!string.IsNullOrEmpty(proto.ComplementoInstrucao2))
            boleto.ComplementoInstrucao2 = proto.ComplementoInstrucao2;

        if (!string.IsNullOrEmpty(proto.CodigoInstrucao3))
            boleto.CodigoInstrucao3 = proto.CodigoInstrucao3;

        if (!string.IsNullOrEmpty(proto.ComplementoInstrucao3))
            boleto.ComplementoInstrucao3 = proto.ComplementoInstrucao3;

        // Other
        if (proto.DiasLimiteRecebimento != 0)
            boleto.DiasLimiteRecebimento = proto.DiasLimiteRecebimento;

        if (proto.Distribuicao != 0)
            boleto.Distribuicao = proto.Distribuicao;

        if (proto.Avalista != null)
            boleto.Avalista = MapPagador(proto.Avalista);

        if (!string.IsNullOrEmpty(proto.ParcelaInformativo))
            boleto.ParcelaInformativo = proto.ParcelaInformativo;

        if (!string.IsNullOrEmpty(proto.ByteNossoNumero))
            boleto.ByteNossoNumero = proto.ByteNossoNumero;

        if (!string.IsNullOrEmpty(proto.QrCode))
            boleto.QRCode = proto.QrCode;

        if (!string.IsNullOrEmpty(proto.TxId))
            boleto.TxId = proto.TxId;

        // Direct assignment fields (reconstruction scenario)
        if (!string.IsNullOrEmpty(proto.NossoNumeroDv))
            boleto.NossoNumeroDV = proto.NossoNumeroDv;

        if (!string.IsNullOrEmpty(proto.NossoNumeroFormatado))
            boleto.NossoNumeroFormatado = proto.NossoNumeroFormatado;

        if (proto.CodigoBarra != null)
            MapCodigoBarra(proto.CodigoBarra, boleto.CodigoBarra);
    }

    private static void MapCodigoBarra(Contracts.Generated.V1.CodigoBarra proto, CodigoBarra target)
    {
        if (!string.IsNullOrEmpty(proto.CodigoBanco))
            target.CodigoBanco = proto.CodigoBanco;

        if (proto.Moeda != 0)
            target.Moeda = proto.Moeda;

        if (!string.IsNullOrEmpty(proto.CampoLivre))
            target.CampoLivre = proto.CampoLivre;

        if (proto.FatorVencimento != 0)
            target.FatorVencimento = proto.FatorVencimento;

        if (!string.IsNullOrEmpty(proto.ValorDocumento))
            target.ValorDocumento = proto.ValorDocumento;

        if (!string.IsNullOrEmpty(proto.LinhaDigitavel))
            target.LinhaDigitavel = proto.LinhaDigitavel;
    }

    public static BoletoGerado MapToResponse(Boleto boleto)
    {
        return new BoletoGerado
        {
            NossoNumero = boleto.NossoNumero,
            NossoNumeroDv = boleto.NossoNumeroDV,
            NossoNumeroFormatado = boleto.NossoNumeroFormatado,
            CodigoBarras = boleto.CodigoBarra.CodigoDeBarras,
            LinhaDigitavel = boleto.CodigoBarra.LinhaDigitavel,
            NumeroDocumento = boleto.NumeroDocumento,
            DigitoVerificador = boleto.CodigoBarra.DigitoVerificador,
        };
    }

    public static void MapBancoBeneficiario(Contracts.Generated.V1.Beneficiario proto, Beneficiario target)
    {
        if (!string.IsNullOrEmpty(proto.CpfCnpj))
            target.CPFCNPJ = proto.CpfCnpj;

        if (!string.IsNullOrEmpty(proto.Nome))
            target.Nome = proto.Nome;

        if (!string.IsNullOrEmpty(proto.Codigo))
            target.Codigo = proto.Codigo;

        if (!string.IsNullOrEmpty(proto.CodigoDv))
            target.CodigoDV = proto.CodigoDv;

        if (!string.IsNullOrEmpty(proto.CodigoTransmissao))
            target.CodigoTransmissao = proto.CodigoTransmissao;

        if (!string.IsNullOrEmpty(proto.CodigoFormatado))
            target.CodigoFormatado = proto.CodigoFormatado;

        if (!string.IsNullOrEmpty(proto.Observacoes))
            target.Observacoes = proto.Observacoes;

        if (proto.Endereco != null)
            target.Endereco = MapEndereco(proto.Endereco);

        target.MostrarCNPJnoBoleto = proto.MostrarCnpjNoBoleto;

        if (proto.ContaBancaria != null)
            target.ContaBancaria = MapContaBancaria(proto.ContaBancaria);
    }

    private static ContaBancaria MapContaBancaria(Contracts.Generated.V1.ContaBancaria proto)
    {
        return new ContaBancaria
        {
            Agencia = proto.Agencia,
            DigitoAgencia = proto.DigitoAgencia,
            Conta = proto.Conta,
            DigitoConta = proto.DigitoConta,
            OperacaoConta = proto.OperacaoConta,
            CodigoConvenio = proto.CodigoConvenio,
            TipoFormaCadastramento = MapTipoFormaCadastramento(proto.TipoFormaCadastramento),
            TipoImpressaoBoleto = MapTipoImpressaoBoleto(proto.TipoImpressaoBoleto),
            ChavePix = proto.ChavePix,
            TipoChavePix = MapTipoChavePix(proto.TipoChavePix),
            TipoDocumento = MapTipoDocumento(proto.TipoDocumento),
            LocalPagamento = proto.LocalPagamento,
            MensagemFixaTopoBoleto = proto.MensagemFixaTopoBoleto,
            MensagemFixaPagador = proto.MensagemFixaPagador,
            CodigoBancoCorrespondente = proto.CodigoBancoCorrespondente,
            NossoNumeroBancoCorrespondente = proto.NossoNumeroBancoCorrespondente,
            TipoDistribuicao = MapTipoDistribuicaoBoleto(proto.TipoDistribuicao),
            TipoCarteiraPadrao = MapTipoCarteira(proto.TipoCarteiraPadrao),
            CarteiraPadrao = proto.CarteiraPadrao,
            VariacaoCarteiraPadrao = proto.VariacaoCarteiraPadrao,
        };
    }

    private static Endereco MapEndereco(ProtoTypes.Endereco? proto)
    {
        if (proto == null)
            return new Endereco();

        return new Endereco
        {
            LogradouroEndereco = proto.LogradouroEndereco,
            LogradouroNumero = proto.LogradouroNumero,
            LogradouroComplemento = proto.LogradouroComplemento,
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

    public static TipoCarteira MapTipoCarteira(Contracts.Generated.V1.TipoCarteira proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaSimples => TipoCarteira.CarteiraCobrancaSimples,
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaVinculada => TipoCarteira.CarteiraCobrancaVinculada,
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaCaucionada => TipoCarteira.CarteiraCobrancaCaucionada,
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaDescontada => TipoCarteira.CarteiraCobrancaDescontada,
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaVendor => TipoCarteira.CarteiraCobrancaVendor,
            Contracts.Generated.V1.TipoCarteira.CarteiraCobrancaDebito => TipoCarteira.CarteiraCobrancaDebito,
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
            Contracts.Generated.V1.TipoEspecieDocumento.Cpr => TipoEspecieDocumento.CPR,
            Contracts.Generated.V1.TipoEspecieDocumento.War => TipoEspecieDocumento.WAR,
            Contracts.Generated.V1.TipoEspecieDocumento.Dae => TipoEspecieDocumento.DAE,
            Contracts.Generated.V1.TipoEspecieDocumento.Dam => TipoEspecieDocumento.DAM,
            Contracts.Generated.V1.TipoEspecieDocumento.Dau => TipoEspecieDocumento.DAU,
            Contracts.Generated.V1.TipoEspecieDocumento.Ec => TipoEspecieDocumento.EC,
            Contracts.Generated.V1.TipoEspecieDocumento.Cc => TipoEspecieDocumento.CC,
            Contracts.Generated.V1.TipoEspecieDocumento.Bp => TipoEspecieDocumento.BP,
            Contracts.Generated.V1.TipoEspecieDocumento.Dv => TipoEspecieDocumento.DV,
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

    private static BoletoNetCore.Enums.TipoCodigoMulta MapTipoCodigoMulta(TipoCodigoMulta proto)
    {
        return proto switch
        {
            TipoCodigoMulta.Valor => Enums.TipoCodigoMulta.Valor,
            TipoCodigoMulta.Percentual => Enums.TipoCodigoMulta.Percentual,
            TipoCodigoMulta.DispensarCobrancaMulta => Enums.TipoCodigoMulta.DispensarCobrancaMulta,
            _ => Enums.TipoCodigoMulta.Valor,
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
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoPreEmiteClienteComplementa => TipoImpressaoBoleto.BancoPreEmiteClienteComplementa,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoReemite => TipoImpressaoBoleto.BancoReemite,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoNaoReemite => TipoImpressaoBoleto.BancoNaoReemite,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoEmitenteAberta => TipoImpressaoBoleto.BancoEmitenteAberta,
            Contracts.Generated.V1.TipoImpressaoBoleto.BancoEmitenteAutoEnvolopavel => TipoImpressaoBoleto.BancoEmitenteAutoEnvolopavel,
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

    private static TipoDocumento MapTipoDocumento(Contracts.Generated.V1.TipoDocumento proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoDocumento.Tradicional => TipoDocumento.Tradicional,
            Contracts.Generated.V1.TipoDocumento.Escritural => TipoDocumento.Escritural,
            _ => TipoDocumento.Tradicional,
        };
    }

    private static TipoDistribuicaoBoleto MapTipoDistribuicaoBoleto(Contracts.Generated.V1.TipoDistribuicaoBoleto proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoDistribuicaoBoleto.BancoDistribui => TipoDistribuicaoBoleto.BancoDistribui,
            Contracts.Generated.V1.TipoDistribuicaoBoleto.ClienteDistribui => TipoDistribuicaoBoleto.ClienteDistribui,
            Contracts.Generated.V1.TipoDistribuicaoBoleto.BancoEnviaEmail => TipoDistribuicaoBoleto.BancoEnviaEmail,
            Contracts.Generated.V1.TipoDistribuicaoBoleto.BancoEnviaSms => TipoDistribuicaoBoleto.BancoEnviaSMS,
            _ => TipoDistribuicaoBoleto.ClienteDistribui,
        };
    }

    private static TipoMoeda MapTipoMoeda(Contracts.Generated.V1.TipoMoeda proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoMoeda.Fag => TipoMoeda.FAG,
            Contracts.Generated.V1.TipoMoeda.Idt => TipoMoeda.IDT,
            Contracts.Generated.V1.TipoMoeda.Rea => TipoMoeda.REA,
            Contracts.Generated.V1.TipoMoeda.Usd => TipoMoeda.USD,
            Contracts.Generated.V1.TipoMoeda.Brl => TipoMoeda.BRL,
            _ => TipoMoeda.REA,
        };
    }

    private static TipoCodigoProtesto MapTipoCodigoProtesto(Contracts.Generated.V1.TipoCodigoProtesto proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoCodigoProtesto.NaoProtestar => TipoCodigoProtesto.NaoProtestar,
            Contracts.Generated.V1.TipoCodigoProtesto.ProtestarDiasCorridos => TipoCodigoProtesto.ProtestarDiasCorridos,
            Contracts.Generated.V1.TipoCodigoProtesto.ProtestarDiasUteis => TipoCodigoProtesto.ProtestarDiasUteis,
            Contracts.Generated.V1.TipoCodigoProtesto.UtilizarPerfilBeneficiario => TipoCodigoProtesto.UtilizarPerfilBeneficiario,
            Contracts.Generated.V1.TipoCodigoProtesto.CancelamentoProtestoAutomatico => TipoCodigoProtesto.CancelamentoProtestoAutomatico,
            Contracts.Generated.V1.TipoCodigoProtesto.NegativacaoSemProtesto => TipoCodigoProtesto.NegativacaoSemProtesto,
            _ => TipoCodigoProtesto.NaoProtestar,
        };
    }

    private static TipoCodigoBaixaDevolucao MapTipoCodigoBaixaDevolucao(Contracts.Generated.V1.TipoCodigoBaixaDevolucao proto)
    {
        return proto switch
        {
            Contracts.Generated.V1.TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver => TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver,
            Contracts.Generated.V1.TipoCodigoBaixaDevolucao.BaixarDevolver => TipoCodigoBaixaDevolucao.BaixarDevolver,
            _ => TipoCodigoBaixaDevolucao.BaixarDevolver,
        };
    }
}
