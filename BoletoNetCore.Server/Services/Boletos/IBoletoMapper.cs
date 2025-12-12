using BoletoNetCore;
using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Handles conversion between proto contracts and BoletoNetCore domain objects.
/// </summary>
public interface IBoletoMapper
{
    /// <summary>
    /// Maps proto ContaBancaria to domain ContaBancaria.
    /// </summary>
    ContaBancaria MapContaBancaria(GerarBoletoRequest request);

    /// <summary>
    /// Maps proto Beneficiario to domain Beneficiario.
    /// </summary>
    Beneficiario MapBeneficiario(Contracts.Generated.V1.Beneficiario proto, ContaBancaria conta);

    /// <summary>
    /// Maps proto Pagador to domain Pagador.
    /// </summary>
    Pagador MapPagador(Contracts.Generated.V1.Pagador proto);

    /// <summary>
    /// Maps proto BoletoInput to existing domain Boleto.
    /// </summary>
    void MapBoleto(BoletoInput proto, Boleto boleto);

    /// <summary>
    /// Maps domain Boleto to proto BoletoGerado response.
    /// </summary>
    BoletoGerado MapToResponse(Boleto boleto);

    /// <summary>
    /// Maps proto TipoCarteira to domain TipoCarteira.
    /// </summary>
    TipoCarteira MapTipoCarteira(Contracts.Generated.V1.TipoCarteira proto);
}
