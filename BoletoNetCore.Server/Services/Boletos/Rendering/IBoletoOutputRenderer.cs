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
    OutputFormat Format { get; }

    /// <summary>
    /// The MIME content type of the rendered output.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Renders the boletos to the output format.
    /// </summary>
    /// <param name="boletos">Collection of boletos to render</param>
    /// <returns>Rendered content as bytes</returns>
    byte[] Render(BoletoNetCore.Boletos boletos);
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
    IBoletoOutputRenderer GetRenderer(OutputFormat format);
}
