using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.Services.Boletos.Rendering;

namespace BoletoNetCore.Server.Tests.Services.Boletos.Rendering;

[Trait("Category", "Unit")]
public sealed class OutputRendererFactoryTests
{
    [Theory]
    [InlineData(BoletoOutputFormat.Pdf)]
    [InlineData(BoletoOutputFormat.Unspecified)]  // Defaults to PDF
    public void GetRenderer_SupportedFormat_ReturnsMatchingRenderer(BoletoOutputFormat format)
    {
        // Arrange
        var pdfRenderer = new FakeRenderer(BoletoOutputFormat.Pdf);
        var sut = new OutputRendererFactory([pdfRenderer]);

        // Act
        var result = sut.GetRenderer(format);

        // Assert
        Assert.Equal(BoletoOutputFormat.Pdf, result.Format);
    }

    [Fact]
    public void GetRenderer_UnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        var pdfRenderer = new FakeRenderer(BoletoOutputFormat.Pdf);
        var sut = new OutputRendererFactory([pdfRenderer]);
        var unsupportedFormat = (BoletoOutputFormat)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sut.GetRenderer(unsupportedFormat));
    }

    private sealed class FakeRenderer : IBoletoOutputRenderer
    {
        public BoletoOutputFormat Format { get; }
        public string ContentType => "application/pdf";

        public FakeRenderer(BoletoOutputFormat format)
        {
            Format = format;
        }

        public Task<byte[]> RenderAsync(BoletoNetCore.Boletos boletos, CancellationToken cancellationToken = default)
            => Task.FromResult(Array.Empty<byte>());
    }
}
