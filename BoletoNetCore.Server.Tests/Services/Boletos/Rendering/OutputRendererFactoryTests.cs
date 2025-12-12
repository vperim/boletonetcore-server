using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.Services.Boletos.Rendering;

namespace BoletoNetCore.Server.Tests.Services.Boletos.Rendering;

[Trait("Category", "Unit")]
public sealed class OutputRendererFactoryTests
{
    [Theory]
    [InlineData(OutputFormat.Pdf)]
    [InlineData(OutputFormat.Unspecified)]  // Defaults to PDF
    public void GetRenderer_SupportedFormat_ReturnsMatchingRenderer(OutputFormat format)
    {
        // Arrange
        var pdfRenderer = new FakeRenderer(OutputFormat.Pdf);
        var sut = new OutputRendererFactory([pdfRenderer]);

        // Act
        var result = sut.GetRenderer(format);

        // Assert
        Assert.Equal(OutputFormat.Pdf, result.Format);
    }

    [Fact]
    public void GetRenderer_UnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        var pdfRenderer = new FakeRenderer(OutputFormat.Pdf);
        var sut = new OutputRendererFactory([pdfRenderer]);
        var unsupportedFormat = (OutputFormat)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.GetRenderer(unsupportedFormat));
    }

    private sealed class FakeRenderer : IBoletoOutputRenderer
    {
        public OutputFormat Format { get; }
        public string ContentType => "application/pdf";

        public FakeRenderer(OutputFormat format)
        {
            Format = format;
        }

        public byte[] Render(BoletoNetCore.Boletos boletos) => [];
    }
}
