#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoletoNetCore.Server.Contracts.Generated.Validation;

/// <summary>
/// Exceção lançada quando uma ou mais validações falham durante a criação de objetos do domínio de boletos.
/// Contém uma lista de todos os erros de validação encontrados.
/// </summary>
public sealed class BoletoValidationException : Exception
{
    /// <summary>
    /// Lista de erros de validação encontrados.
    /// </summary>
    public IReadOnlyList<ErroValidacao> Erros { get; }

    /// <summary>
    /// Indica se há múltiplos erros de validação.
    /// </summary>
    public bool MultiplosErros => Erros.Count > 1;

    /// <summary>
    /// Cria uma exceção com múltiplos erros de validação.
    /// </summary>
    /// <param name="erros">Lista de erros de validação.</param>
    public BoletoValidationException(IList<ErroValidacao> erros)
        : base(FormatarMensagem(erros))
    {
        Erros = erros.ToList();
    }

    /// <summary>
    /// Cria uma exceção a partir de um único ErroValidacao.
    /// </summary>
    /// <param name="erro">Erro de validação.</param>
    public BoletoValidationException(ErroValidacao erro)
        : this([erro])
    {
    }

    private static string FormatarMensagem(IList<ErroValidacao> erros)
    {
        var listaErros = erros.ToList();

        return listaErros.Count switch
        {
            0 => "Erro de validação.",
            1 => $"{listaErros[0].Campo}: {listaErros[0].Mensagem}",
            _ => $"Erros de validação: {string.Join("; ", listaErros.Select(e => $"{e.Campo}: {e.Mensagem}"))}"
        };
    }

    /// <summary>
    /// Retorna os erros formatados como lista com marcadores.
    /// </summary>
    public string FormatarComoLista()
        => string.Join(Environment.NewLine, Erros.Select(e => $"• {e.Campo}: {e.Mensagem}"));

    /// <summary>
    /// Retorna os erros agrupados por código.
    /// </summary>
    public ILookup<CodigoErroValidacao, ErroValidacao> AgruparPorCodigo()
        => Erros.ToLookup(e => e.Codigo);

    /// <summary>
    /// Verifica se há erro para um campo específico.
    /// </summary>
    /// <param name="campo">Nome do campo.</param>
    public bool TemErroPara(string campo)
        => Erros.Any(e => e.Campo.Equals(campo, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Obtém os erros de um campo específico.
    /// </summary>
    /// <param name="campo">Nome do campo.</param>
    public IEnumerable<ErroValidacao> ObterErrosPara(string campo)
        => Erros.Where(e => e.Campo.Equals(campo, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Verifica se há erro com um código específico.
    /// </summary>
    /// <param name="codigo">Código do erro.</param>
    public bool TemErroComCodigo(CodigoErroValidacao codigo)
        => Erros.Any(e => e.Codigo == codigo);

    /// <summary>
    /// Obtém os erros com um código específico.
    /// </summary>
    /// <param name="codigo">Código do erro.</param>
    public IEnumerable<ErroValidacao> ObterErrosComCodigo(CodigoErroValidacao codigo)
        => Erros.Where(e => e.Codigo == codigo);
}
