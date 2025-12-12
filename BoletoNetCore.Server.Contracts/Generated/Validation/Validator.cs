#nullable enable
using System.Collections.Generic;

namespace BoletoNetCore.Server.Contracts.Generated.Validation;

/// <summary>
/// Builder fluente para coleta e validação de erros.
/// Uso interno da biblioteca.
/// </summary>
internal sealed class Validator
{
    private readonly List<ErroValidacao> _errors = [];

    private Validator() { }

    /// <summary>
    /// Cria uma nova instância do validador.
    /// </summary>
    public static Validator Create() => new();

    /// <summary>
    /// Valida se uma string não é nula ou vazia.
    /// </summary>
    public Validator Required(string? value, string field, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            _errors.Add(ErroValidacao.CampoObrigatorio(field, message));
        return this;
    }

    /// <summary>
    /// Valida se um objeto não é nulo.
    /// </summary>
    public Validator Required<T>(T? value, string field, string? message = null) where T : class
    {
        if (value is null)
            _errors.Add(ErroValidacao.CampoObrigatorio(field, message));
        return this;
    }

    /// <summary>
    /// Valida formato quando a condição é falsa.
    /// </summary>
    /// <param name="isValid">Condição que deve ser verdadeira para passar a validação.</param>
    public Validator Format(bool isValid, string field, string message)
    {
        if (!isValid)
            _errors.Add(ErroValidacao.FormatoInvalido(field, message));
        return this;
    }

    /// <summary>
    /// Valida valor quando a condição é falsa.
    /// </summary>
    /// <param name="isValid">Condição que deve ser verdadeira para passar a validação.</param>
    public Validator Value(bool isValid, string field, string message)
    {
        if (!isValid)
            _errors.Add(ErroValidacao.ValorInvalido(field, message));
        return this;
    }

    /// <summary>
    /// Valida limite quando a condição é falsa.
    /// </summary>
    /// <param name="isValid">Condição que deve ser verdadeira para passar a validação.</param>
    public Validator Limit(bool isValid, string field, string message)
    {
        if (!isValid)
            _errors.Add(ErroValidacao.LimiteExcedido(field, message));
        return this;
    }

    /// <summary>
    /// Adiciona um erro customizado.
    /// </summary>
    public Validator Error(ErroValidacao error)
    {
        _errors.Add(error);
        return this;
    }

    /// <summary>
    /// Adiciona um erro quando a condição é verdadeira.
    /// </summary>
    /// <param name="hasError">Condição que indica erro.</param>
    public Validator When(bool hasError, ErroValidacao error)
    {
        if (hasError)
            _errors.Add(error);
        return this;
    }

    /// <summary>
    /// Indica se a validação passou (sem erros).
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Lista de erros coletados.
    /// </summary>
    public IReadOnlyList<ErroValidacao> Errors => _errors;

    /// <summary>
    /// Lança BoletoValidationException se houver erros.
    /// </summary>
    /// <exception cref="BoletoValidationException">Lançada quando há erros de validação.</exception>
    public void ThrowIfInvalid()
    {
        if (_errors.Count > 0)
            throw new BoletoValidationException(_errors);
    }
}
