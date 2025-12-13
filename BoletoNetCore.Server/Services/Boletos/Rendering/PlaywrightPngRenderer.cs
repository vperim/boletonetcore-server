using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Renders boletos to PNG format using Playwright.
/// </summary>
public sealed class PlaywrightPngRenderer : IBoletoOutputRenderer
{
    private readonly IHtmlRenderer renderer;

    public PlaywrightPngRenderer(IHtmlRenderer renderer)
    {
        this.renderer = renderer;
    }

    public BoletoOutputFormat Format => BoletoOutputFormat.Png;

    public string ContentType => "image/png";

    public async Task<byte[]> RenderAsync(BoletoNetCore.Boletos boletos, CancellationToken cancellationToken = default)
    {
        return await boletos.RenderPlaywrightAsync(this.renderer, PngFormat.Default, cancellationToken: cancellationToken);
    }
}
