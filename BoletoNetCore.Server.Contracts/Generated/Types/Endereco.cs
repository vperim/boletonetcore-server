#nullable enable
using System;
using System.Text.RegularExpressions;
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.Types;

public partial class Endereco
{
    private static readonly Regex CepRegex = new(@"^\d{8}$", RegexOptions.Compiled);
    private static readonly string[] UfsValidas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
        "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
        "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    /// <summary>
    /// Cria uma instância de Endereco validada.
    /// </summary>
    /// <param name="logradouro">Nome da rua/avenida.</param>
    /// <param name="numero">Número do endereço.</param>
    /// <param name="bairro">Nome do bairro.</param>
    /// <param name="cidade">Nome da cidade.</param>
    /// <param name="uf">Sigla do estado (2 letras).</param>
    /// <param name="cep">CEP (8 dígitos, com ou sem formatação).</param>
    /// <param name="complemento">Complemento opcional.</param>
    /// <returns>Uma instância de Endereco validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando uma ou mais validações falham.</exception>
    public static Endereco CriarValido(
        string logradouro,
        string numero,
        string bairro,
        string cidade,
        string uf,
        string cep,
        string? complemento = null)
    {
        var ufUpper = uf?.ToUpperInvariant();
        var cepLimpo = cep?.Replace("-", "").Replace(".", "");

        Validator.Create()
            .Required(logradouro, nameof(logradouro), "Logradouro não informado.")
            .Required(numero, nameof(numero), "Número não informado.")
            .Required(bairro, nameof(bairro), "Bairro não informado.")
            .Required(cidade, nameof(cidade), "Cidade não informada.")
            .Required(uf, nameof(uf), "UF não informada.")
            .Format(ufUpper != null && Array.IndexOf(UfsValidas, ufUpper) >= 0, nameof(uf), $"UF inválida: {uf}. Use sigla de 2 letras (ex: SP, RJ).")
            .Required(cep, nameof(cep), "CEP não informado.")
            .Format(cepLimpo != null && CepRegex.IsMatch(cepLimpo), nameof(cep), $"CEP inválido: {cep}. Use 8 dígitos sem formatação.")
            .ThrowIfInvalid();

        return new Endereco
        {
            Logradouro = logradouro.Trim(),
            Numero = numero.Trim(),
            Bairro = bairro.Trim(),
            Cidade = cidade.Trim(),
            Uf = ufUpper!,
            Cep = cepLimpo!,
            Complemento = complemento?.Trim() ?? string.Empty
        };
    }
}
