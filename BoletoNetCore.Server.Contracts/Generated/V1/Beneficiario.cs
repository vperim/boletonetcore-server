#nullable enable
using System.Text.RegularExpressions;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class Beneficiario
{
    private static readonly Regex CpfRegex = new(@"^\d{11}$", RegexOptions.Compiled);
    private static readonly Regex CnpjRegex = new(@"^\d{14}$", RegexOptions.Compiled);

    /// <summary>
    /// Cria uma instância de Beneficiario validada.
    /// </summary>
    /// <param name="cpfCnpj">CPF (11 dígitos) ou CNPJ (14 dígitos) sem formatação.</param>
    /// <param name="nome">Nome do beneficiário.</param>
    /// <param name="contaBancaria">Conta bancária (use ContaBancaria.CriarValido()).</param>
    /// <param name="codigo">Código do beneficiário (específico do banco, opcional).</param>
    /// <param name="codigoDv">Dígito verificador do código do beneficiário (opcional).</param>
    /// <param name="codigoTransmissao">Código de transmissão (específico do banco, opcional).</param>
    /// <param name="endereco">Endereço opcional (use Endereco.CriarValido()).</param>
    /// <returns>Uma instância de Beneficiario validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando uma ou mais validações falham.</exception>
    public static Beneficiario CriarValido(
        string cpfCnpj,
        string nome,
        ContaBancaria contaBancaria,
        string? codigo = null,
        string? codigoDv = null,
        string? codigoTransmissao = null,
        Endereco? endereco = null)
    {
        var cpfCnpjLimpo = cpfCnpj?.Replace(".", "").Replace("-", "").Replace("/", "");

        Validator.Create()
            .Required(cpfCnpj, nameof(cpfCnpj), "CPF/CNPJ não informado.")
            .Format(cpfCnpjLimpo != null && (CpfRegex.IsMatch(cpfCnpjLimpo) || CnpjRegex.IsMatch(cpfCnpjLimpo)),
                    nameof(cpfCnpj), $"CPF/CNPJ inválido: {cpfCnpj}. Use 11 dígitos (CPF) ou 14 dígitos (CNPJ) sem formatação.")
            .Required(nome, nameof(nome), "Nome não informado.")
            .Required(contaBancaria, nameof(contaBancaria), "Conta bancária não informada.")
            .ThrowIfInvalid();

        var beneficiario = new Beneficiario
        {
            CpfCnpj = cpfCnpjLimpo!,
            Nome = nome.Trim(),
            ContaBancaria = contaBancaria
        };

        if (!string.IsNullOrWhiteSpace(codigo))
            beneficiario.Codigo = codigo!.Trim();

        if (!string.IsNullOrWhiteSpace(codigoDv))
            beneficiario.CodigoDv = codigoDv!.Trim();

        if (!string.IsNullOrWhiteSpace(codigoTransmissao))
            beneficiario.CodigoTransmissao = codigoTransmissao!.Trim();

        if (endereco != null)
            beneficiario.Endereco = endereco;

        return beneficiario;
    }
}
