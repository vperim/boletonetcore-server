using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Renders boletos to a specific output format.
/// </summary>
public interface IBoletoOutputRenderer
{
    /// <summary>
    /// The output format this renderer handles.
    /// </summary>
    BoletoOutputFormat Format { get; }

    /// <summary>
    /// The MIME content type of the rendered output.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Renders the boletos to the output format asynchronously.
    /// </summary>
    /// <param name="boletos">Collection of boletos to render</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rendered content as bytes</returns>
    Task<byte[]> RenderAsync(BoletoNetCore.Boletos boletos, CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory for selecting the appropriate output renderer.
/// </summary>
public interface IOutputRendererFactory
{
    /// <summary>
    /// Gets the renderer for the specified output format.
    /// </summary>
    /// <param name="format">Desired output format</param>
    /// <returns>Renderer for the format</returns>
    IBoletoOutputRenderer GetRenderer(BoletoOutputFormat format);
}
