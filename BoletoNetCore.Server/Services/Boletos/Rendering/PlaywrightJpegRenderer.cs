using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Renders boletos to JPEG format using Playwright.
/// </summary>
public sealed class PlaywrightJpegRenderer : IBoletoOutputRenderer
{
    private readonly IHtmlRenderer renderer;

    public PlaywrightJpegRenderer(IHtmlRenderer renderer)
    {
        this.renderer = renderer;
    }

    public BoletoOutputFormat Format => BoletoOutputFormat.Jpeg;

    public string ContentType => "image/jpeg";

    public async Task<byte[]> RenderAsync(BoletoNetCore.Boletos boletos, CancellationToken cancellationToken = default)
    {
        return await boletos.RenderPlaywrightAsync(this.renderer, new JpegFormat(Quality: 90), cancellationToken: cancellationToken);
    }
}
