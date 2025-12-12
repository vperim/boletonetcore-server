using BoletoNetCore.Server.Contracts.Generated.V1;

namespace BoletoNetCore.Server.Services.Boletos.Rendering;

/// <summary>
/// Renders boletos to PDF format using QuestPDF.
/// </summary>
public sealed class PdfBoletoRenderer : IBoletoOutputRenderer
{
    public OutputFormat Format => OutputFormat.Pdf;

    public string ContentType => "application/pdf";

    public byte[] Render(BoletoNetCore.Boletos boletos)
    {
        // Uses BoletoNetCore.QuestPDF extension method
        return boletos.ImprimirCarnePdf();
    }
}
