#nullable enable
using System.Text.RegularExpressions;
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class ContaBancaria
{
    private static readonly Regex AgenciaRegex = new(@"^\d{1,5}$", RegexOptions.Compiled);
    private static readonly Regex ContaRegex = new(@"^\d{1,12}$", RegexOptions.Compiled);

    /// <summary>
    /// Cria uma instância de ContaBancaria validada.
    /// </summary>
    /// <param name="agencia">Número da agência (1-5 dígitos).</param>
    /// <param name="conta">Número da conta (1-12 dígitos).</param>
    /// <param name="tipoFormaCadastramento">Tipo de forma de cadastramento.</param>
    /// <param name="tipoImpressaoBoleto">Tipo de impressão do boleto.</param>
    /// <param name="digitoAgencia">Dígito verificador da agência (opcional).</param>
    /// <param name="digitoConta">Dígito verificador da conta (opcional).</param>
    /// <param name="operacaoConta">Código da operação (específico do banco, opcional).</param>
    /// <param name="codigoConvenio">Código do convênio (opcional).</param>
    /// <returns>Uma instância de ContaBancaria validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando uma ou mais validações falham.</exception>
    public static ContaBancaria CriarValido(
        string agencia,
        string conta,
        TipoFormaCadastramento tipoFormaCadastramento,
        TipoImpressaoBoleto tipoImpressaoBoleto,
        string? digitoAgencia = null,
        string? digitoConta = null,
        string? operacaoConta = null,
        string? codigoConvenio = null)
    {
        Validator.Create()
            .Required(agencia, nameof(agencia), "Agência não informada.")
            .Format(AgenciaRegex.IsMatch(agencia), nameof(agencia), $"Agência inválida: {agencia}. Use 1-5 dígitos.")
            .Required(conta, nameof(conta), "Conta não informada.")
            .Format(ContaRegex.IsMatch(conta), nameof(conta), $"Conta inválida: {conta}. Use 1-12 dígitos.")
            .Value(tipoFormaCadastramento != TipoFormaCadastramento.Unspecified, nameof(tipoFormaCadastramento), "Tipo de forma de cadastramento não informado.")
            .Value(tipoImpressaoBoleto != TipoImpressaoBoleto.Unspecified, nameof(tipoImpressaoBoleto), "Tipo de impressão do boleto não informado.")
            .ThrowIfInvalid();

        var contaBancaria = new ContaBancaria
        {
            Agencia = agencia!.Trim(),
            Conta = conta!.Trim(),
            TipoFormaCadastramento = tipoFormaCadastramento,
            TipoImpressaoBoleto = tipoImpressaoBoleto
        };

        if (!string.IsNullOrWhiteSpace(digitoAgencia))
            contaBancaria.DigitoAgencia = digitoAgencia!.Trim();

        if (!string.IsNullOrWhiteSpace(digitoConta))
            contaBancaria.DigitoConta = digitoConta!.Trim();

        if (!string.IsNullOrWhiteSpace(operacaoConta))
            contaBancaria.OperacaoConta = operacaoConta!.Trim();

        if (!string.IsNullOrWhiteSpace(codigoConvenio))
            contaBancaria.CodigoConvenio = codigoConvenio!.Trim();

        return contaBancaria;
    }

    /// <summary>
    /// Configura as informações de PIX para esta conta bancária.
    /// </summary>
    /// <param name="chavePix">Valor da chave PIX.</param>
    /// <param name="tipoChavePix">Tipo da chave PIX.</param>
    /// <returns>Esta instância para encadeamento de métodos.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando uma ou mais validações falham.</exception>
    public ContaBancaria ComPix(string chavePix, TipoChavePix tipoChavePix)
    {
        Validator.Create()
            .Required(chavePix, nameof(chavePix), "Chave PIX não informada.")
            .Value(tipoChavePix != TipoChavePix.Unspecified, nameof(tipoChavePix), "Tipo da chave PIX não informado.")
            .ThrowIfInvalid();

        ChavePix = chavePix.Trim();
        TipoChavePix = tipoChavePix;
        return this;
    }
}
