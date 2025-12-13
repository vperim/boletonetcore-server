using BoletoNetCore;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Factory for creating fresh bank instances per request.
/// Avoids singleton concurrency issues with mutable Beneficiario state.
/// </summary>
public interface IBancoFactory
{
    /// <summary>
    /// Creates a fresh bank instance without beneficiary.
    /// Beneficiary should be assigned separately via SetBeneficiario.
    /// </summary>
    /// <param name="codigoBanco">Bank code (001, 033, 237, 341, etc.)</param>
    /// <returns>Bank instance without beneficiary configured</returns>
    IBanco Create(int codigoBanco);
}
