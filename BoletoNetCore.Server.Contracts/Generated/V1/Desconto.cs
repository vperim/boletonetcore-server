#nullable enable
using System;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.Validation;
using Google.Protobuf.WellKnownTypes;

namespace BoletoNetCore.Server.Contracts.Generated.V1;

public partial class Desconto
{
    /// <summary>
    /// Cria uma instância de Desconto validada.
    /// </summary>
    /// <param name="data">Data limite para o desconto.</param>
    /// <param name="valor">Valor do desconto (deve ser positivo).</param>
    /// <returns>Uma instância de Desconto validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando o valor é inválido.</exception>
    public static Desconto Criar(DateTime data, decimal valor)
    {
        Validator.Create()
            .Value(valor > 0, nameof(valor), "Valor do desconto deve ser maior que zero.")
            .ThrowIfInvalid();

        return new Desconto
        {
            Data = Timestamp.FromDateTime(DateTime.SpecifyKind(data.Date, DateTimeKind.Utc)),
            Valor = Money.BrlPositivo(valor)
        };
    }

    /// <summary>
    /// Cria uma instância de Desconto validada com um valor Money existente.
    /// </summary>
    /// <param name="data">Data limite para o desconto.</param>
    /// <param name="valor">Valor do desconto (use Money.BrlPositivo()).</param>
    /// <returns>Uma instância de Desconto validada.</returns>
    /// <exception cref="BoletoValidationException">Lançada quando o valor é inválido.</exception>
    public static Desconto Criar(DateTime data, Money valor)
    {
        Validator.Create()
            .Required(valor, nameof(valor), "Valor do desconto não informado.")
            .Value(valor?.ToDecimal() > 0, nameof(valor), "Valor do desconto deve ser maior que zero.")
            .ThrowIfInvalid();

        return new Desconto
        {
            Data = Timestamp.FromDateTime(DateTime.SpecifyKind(data.Date, DateTimeKind.Utc)),
            Valor = valor!
        };
    }
}
