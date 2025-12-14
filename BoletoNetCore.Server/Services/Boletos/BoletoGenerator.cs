using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.Services.Boletos.Rendering;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Orchestrates the boleto generation pipeline.
/// </summary>
public sealed class BoletoGenerator : IBoletoGenerator
{
    private readonly IBancoFactory bancoFactory;
    private readonly IOutputRendererFactory rendererFactory;

    public BoletoGenerator(
        IBancoFactory bancoFactory,
        IOutputRendererFactory rendererFactory)
    {
        this.bancoFactory = bancoFactory;
        this.rendererFactory = rendererFactory;
    }

    public async Task<GenerationResult> GenerateAsync(GerarBoletoRequest request, CancellationToken cancellationToken = default)
    {
        var banco = this.bancoFactory.Create(request.Banco.Codigo);
        banco.Beneficiario ??= new Beneficiario();
        BoletoMapper.MapBancoBeneficiario(request.Banco.Beneficiario, banco.Beneficiario);
        banco.FormataBeneficiario();
        
        var boletos = new BoletoNetCore.Boletos { Banco = banco };

        foreach (var input in request.Boletos)
        {
            var boleto = new Boleto(banco);

            BoletoMapper.MapBoleto(input, boleto);

            // Skip validation when CodigoBarra is provided directly (reconstruction scenario)
            // Library handles: NossoNumeroFormatado, CodigoBarra, LinhaDigitavel
            if (input.CodigoBarra == null)
                boleto.ValidarDados();

            // Apply post-validation overrides (fields that would be overwritten by ValidarDados)
            BoletoMapper.ApplyPostValidationOverrides(input, boleto);

            boletos.Add(boleto);
        }

        // 5. Render output
        var renderer = this.rendererFactory.GetRenderer(request.OutputFormat);
        var content = await renderer.RenderAsync(boletos, cancellationToken);

        // 6. Build response with metadata
        return new GenerationResult
        {
            Content = content,
            ContentType = renderer.ContentType,
            Boletos = boletos.Select(BoletoMapper.MapToResponse).ToList(),
        };
    }

}
