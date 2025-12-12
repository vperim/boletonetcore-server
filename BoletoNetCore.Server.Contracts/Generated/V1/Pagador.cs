#nullable enable
using System.Text.RegularExpressions;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class Pagador
{
    private static readonly Regex CpfRegex = new(@"^\d{11}$", RegexOptions.Compiled);
    private static readonly Regex CnpjRegex = new(@"^\d{14}$", RegexOptions.Compiled);

    /// <summary>
    /// Cria uma instância de Pagador validada.
    /// </summary>
    /// <param name="cpfCnpj">CPF (11 dígitos) ou CNPJ (14 dígitos) sem formatação.</param>
    /// <param name="nome">Nome do pagador.</param>
    /// <param name="endereco">Endereço opcional (use Endereco.CriarValido()).</param>
    /// <param name="telefone">Telefone opcional.</param>
    /// <param name="observacoes">Observações opcionais.</param>
    /// <returns>Uma instância de Pagador validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando uma ou mais validações falham.</exception>
    public static Pagador CriarValido(
        string cpfCnpj,
        string nome,
        Endereco? endereco = null,
        string? telefone = null,
        string? observacoes = null)
    {
        var cpfCnpjLimpo = cpfCnpj?.Replace(".", "").Replace("-", "").Replace("/", "");

        Validator.Create()
            .Required(cpfCnpj, nameof(cpfCnpj), "CPF/CNPJ não informado.")
            .Format(cpfCnpjLimpo != null && (CpfRegex.IsMatch(cpfCnpjLimpo) || CnpjRegex.IsMatch(cpfCnpjLimpo)),
                    nameof(cpfCnpj), $"CPF/CNPJ inválido: {cpfCnpj}. Use 11 dígitos (CPF) ou 14 dígitos (CNPJ) sem formatação.")
            .Required(nome, nameof(nome), "Nome não informado.")
            .ThrowIfInvalid();

        var pagador = new Pagador
        {
            CpfCnpj = cpfCnpjLimpo!,
            Nome = nome.Trim()
        };

        if (endereco != null)
            pagador.Endereco = endereco;

        if (!string.IsNullOrWhiteSpace(telefone))
            pagador.Telefone = telefone!.Trim();

        if (!string.IsNullOrWhiteSpace(observacoes))
            pagador.Observacoes = observacoes!.Trim();

        return pagador;
    }
}
