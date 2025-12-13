using System;
namespace BoletoNetCore.Server.Contracts.Generated.Types;

public partial class Money
{
    public static implicit operator decimal(Money value) => value.Units + value.Nanos / 1_000_000_000m;

    public static implicit operator Money(decimal value)
    {
        var units = (long)Math.Truncate(value);
        var nanos = (int)((value - units) * 1_000_000_000m);

        return new Money
        {
            Units = units,
            Nanos = nanos
        };
    }
}
