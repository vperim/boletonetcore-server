#nullable enable
using System;
using BoletoNetCore.Server.Contracts.Generated.Validation;

namespace BoletoNetCore.Server.Contracts.Generated.Types;

public partial class Money
{
    /// <summary>
    /// Cria uma instância de Money a partir de um valor decimal.
    /// </summary>
    /// <param name="value">O valor monetário (pode ser negativo para estornos/ajustes).</param>
    /// <param name="currencyCode">Código da moeda ISO 4217 (padrão: BRL).</param>
    /// <returns>Uma nova instância de Money.</returns>
    public static Money FromDecimal(decimal value, string currencyCode = "BRL")
    {
        Validator.Create()
            .Required(currencyCode, nameof(currencyCode), "Código da moeda não informado.")
            .Format(currencyCode?.Length == 3, nameof(currencyCode), "Código da moeda deve ter 3 caracteres (ISO 4217).")
            .ThrowIfInvalid();

        var units = (long)Math.Truncate(value);
        var nanos = (int)((value - units) * 1_000_000_000m);

        return new Money
        {
            CurrencyCode = currencyCode!.ToUpperInvariant(),
            Units = units,
            Nanos = nanos
        };
    }

    /// <summary>
    /// Cria uma instância de Money em Reais (BRL).
    /// </summary>
    /// <param name="value">O valor monetário em BRL.</param>
    /// <returns>Uma nova instância de Money com CurrencyCode = "BRL".</returns>
    public static Money Brl(decimal value) => FromDecimal(value, "BRL");

    /// <summary>
    /// Cria uma instância de Money em Reais (BRL) que deve ser positiva.
    /// Use para valores como ValorTitulo, ValorMulta, ValorDesconto.
    /// </summary>
    /// <param name="value">O valor monetário em BRL (deve ser > 0).</param>
    /// <returns>Uma nova instância de Money com CurrencyCode = "BRL".</returns>
    public static Money BrlPositivo(decimal value)
    {
        Validator.Create()
            .Value(value > 0, nameof(value), "Valor deve ser maior que zero.")
            .ThrowIfInvalid();

        return Brl(value);
    }

    /// <summary>
    /// Converte esta instância de Money de volta para um valor decimal.
    /// </summary>
    /// <returns>A representação decimal do valor monetário.</returns>
    public decimal ToDecimal() => Units + (Nanos / 1_000_000_000m);
}
