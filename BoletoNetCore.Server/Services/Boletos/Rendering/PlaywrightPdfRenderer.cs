using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Renders boletos to PDF format using Playwright.
/// </summary>
public sealed class PlaywrightPdfRenderer : IBoletoOutputRenderer
{
    private readonly IHtmlRenderer renderer;

    public PlaywrightPdfRenderer(IHtmlRenderer renderer)
    {
        this.renderer = renderer;
    }

    public BoletoOutputFormat Format => BoletoOutputFormat.Pdf;

    public string ContentType => "application/pdf";

    public async Task<byte[]> RenderAsync(BoletoNetCore.Boletos boletos, CancellationToken cancellationToken = default)
    {
        return await boletos.RenderPlaywrightAsync(this.renderer, PdfFormat.Default, cancellationToken: cancellationToken);
    }
}
