using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Result of boleto generation containing output content and metadata.
/// </summary>
public sealed class GenerationResult
{
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
    public required IReadOnlyList<BoletoGerado> Boletos { get; init; }
}

/// <summary>
/// Orchestrates the boleto generation pipeline.
/// </summary>
public interface IBoletoGenerator
{
    /// <summary>
    /// Generates boletos from the request and renders them to the specified format asynchronously.
    /// </summary>
    /// <param name="request">The generation request containing all boleto data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generation result with content and metadata</returns>
    Task<GenerationResult> GenerateAsync(GerarBoletoRequest request, CancellationToken cancellationToken = default);
}
