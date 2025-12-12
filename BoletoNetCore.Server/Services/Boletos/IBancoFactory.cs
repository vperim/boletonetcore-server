using BoletoNetCore;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Factory for creating fresh bank instances per request.
/// Avoids singleton concurrency issues with mutable Beneficiario state.
/// </summary>
public interface IBancoFactory
{
    /// <summary>
    /// Creates a fresh bank instance with the specified beneficiary.
    /// </summary>
    /// <param name="codigoBanco">Bank code (001, 033, 237, 341, etc.)</param>
    /// <param name="beneficiario">Beneficiary to assign to the bank instance</param>
    /// <returns>Configured bank instance</returns>
    IBanco Create(int codigoBanco, Beneficiario beneficiario);
}
