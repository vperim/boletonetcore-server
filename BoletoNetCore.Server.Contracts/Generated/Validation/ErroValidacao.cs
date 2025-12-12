#nullable enable

namespace BoletoNetCore.Server.Contracts.Generated.Validation;

/// <summary>
/// Códigos de erro padrão para validações.
/// </summary>
public enum CodigoErroValidacao
{
    /// <summary>Campo obrigatório não informado.</summary>
    CampoObrigatorio,

    /// <summary>Formato do valor inválido.</summary>
    FormatoInvalido,

    /// <summary>Valor fora do intervalo permitido.</summary>
    ValorInvalido,

    /// <summary>Tamanho do valor inválido.</summary>
    TamanhoInvalido,

    /// <summary>Limite máximo excedido.</summary>
    LimiteExcedido
}

/// <summary>
/// Representa um erro de validação com informações estruturadas.
/// </summary>
/// <param name="Campo">Nome do campo que falhou na validação.</param>
/// <param name="Mensagem">Mensagem descritiva do erro.</param>
/// <param name="Codigo">Código do erro para tratamento programático.</param>
public sealed record ErroValidacao(string Campo, string Mensagem, CodigoErroValidacao Codigo)
{
    /// <summary>
    /// Cria um erro de campo obrigatório não informado.
    /// </summary>
    public static ErroValidacao CampoObrigatorio(string campo, string? mensagem = null)
        => new(campo, mensagem ?? $"{campo} não informado.", CodigoErroValidacao.CampoObrigatorio);

    /// <summary>
    /// Cria um erro de formato inválido.
    /// </summary>
    public static ErroValidacao FormatoInvalido(string campo, string mensagem)
        => new(campo, mensagem, CodigoErroValidacao.FormatoInvalido);

    /// <summary>
    /// Cria um erro de valor inválido.
    /// </summary>
    public static ErroValidacao ValorInvalido(string campo, string mensagem)
        => new(campo, mensagem, CodigoErroValidacao.ValorInvalido);

    /// <summary>
    /// Cria um erro de limite excedido.
    /// </summary>
    public static ErroValidacao LimiteExcedido(string campo, string mensagem)
        => new(campo, mensagem, CodigoErroValidacao.LimiteExcedido);
}
