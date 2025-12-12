using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.Services.Boletos.Rendering;

namespace BoletoNetCore.Server.Services.Boletos;

/// <summary>
/// Orchestrates the boleto generation pipeline.
/// </summary>
public sealed class BoletoGenerator : IBoletoGenerator
{
    private readonly IBancoFactory bancoFactory;
    private readonly IBoletoMapper mapper;
    private readonly IOutputRendererFactory rendererFactory;

    public BoletoGenerator(
        IBancoFactory bancoFactory,
        IBoletoMapper mapper,
        IOutputRendererFactory rendererFactory)
    {
        this.bancoFactory = bancoFactory;
        this.mapper = mapper;
        this.rendererFactory = rendererFactory;
    }

    public GenerationResult Generate(GerarBoletoRequest request)
    {
        // 1. Map conta bancária from request
        var contaBancaria = this.mapper.MapContaBancaria(request);

        // 2. Map beneficiário
        var beneficiario = this.mapper.MapBeneficiario(request.Beneficiario, contaBancaria);

        // 3. Create fresh bank instance (stateless, no concurrency issues)
        var banco = this.bancoFactory.Create(request.BancoCodigo, beneficiario);

        // 4. Create boletos collection
        var boletos = new BoletoNetCore.Boletos { Banco = banco };

        foreach (var input in request.Boletos)
        {
            // Use constructor that ignores carteira from banco.Beneficiario.ContaBancaria
            var boleto = new Boleto(banco, ignorarCarteira: true)
            {
                Carteira = request.Carteira,
                VariacaoCarteira = request.VariacaoCarteira,
                TipoCarteira = this.mapper.MapTipoCarteira(request.TipoCarteira),
            };

            this.mapper.MapBoleto(input, boleto);

            // Library handles: NossoNumeroFormatado, CodigoBarra, LinhaDigitavel
            boleto.ValidarDados();

            boletos.Add(boleto);
        }

        // 5. Render output
        var renderer = this.rendererFactory.GetRenderer(request.OutputFormat);
        var content = renderer.Render(boletos);

        // 6. Build response with metadata
        return new GenerationResult
        {
            Content = content,
            ContentType = renderer.ContentType,
            Boletos = boletos.Select(this.mapper.MapToResponse).ToList(),
        };
    }

}
