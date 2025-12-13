using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Factory for selecting output renderers based on format.
/// </summary>
public sealed class OutputRendererFactory : IOutputRendererFactory
{
    private readonly IEnumerable<IBoletoOutputRenderer> renderers;

    public OutputRendererFactory(IEnumerable<IBoletoOutputRenderer> renderers)
    {
        this.renderers = renderers;
    }

    public IBoletoOutputRenderer GetRenderer(BoletoOutputFormat format)
    {
        // Default to PDF if unspecified
        var targetFormat = format == BoletoOutputFormat.Unspecified ? BoletoOutputFormat.Pdf : format;

        var renderer = this.renderers.FirstOrDefault(r => r.Format == targetFormat);

        if (renderer == null)
        {
            throw new ArgumentException($"Formato de saída {format} não suportado. Formatos suportados: PDF, PNG, JPEG");
        }

        return renderer;
    }
}
