using BoletoNetCore.Server.Contracts.Generated.V1;
using BoletoNetCore.Server.Errors;
using BoletoNetCore.Server.Services.Boletos;
using Google.Protobuf;
using Grpc.Core;

namespace BoletoNetCore.Server.Services.V1;

public sealed class BoletoV1Service : BoletoV1.BoletoV1Base
{
    private readonly IBoletoGenerator generator;
    private readonly ILogger<BoletoV1Service> logger;

    public BoletoV1Service(IBoletoGenerator generator, ILogger<BoletoV1Service> logger)
    {
        this.generator = generator;
        this.logger = logger;
    }

    public override async Task<GerarBoletoResponse> GerarBoleto(GerarBoletoRequest request, ServerCallContext context)
    {
        ValidateRequest(request);

        var result = await this.generator.GenerateAsync(request, context.CancellationToken);

        var response = new GerarBoletoResponse
        {
            Conteudo = ByteString.CopyFrom(result.Content),
            ContentType = result.ContentType,
        };

        response.Boletos.AddRange(result.Boletos);

        this.logger.LogInformation(
            "Generated {Count} boleto(s) for bank {BankCode}",
            result.Boletos.Count,
            request.Banco.Codigo);

        return response;
    }

    private static void ValidateRequest(GerarBoletoRequest request)
    {
        var violations = new List<(string Field, string Description)>();

        if (request.Banco == null)
            violations.Add(("banco", "Banco é obrigatório"));
        else
        {
            if (request.Banco.Beneficiario == null)
                violations.Add(("banco.beneficiario", "Beneficiário é obrigatório"));
            else if (request.Banco.Beneficiario.ContaBancaria == null)
                violations.Add(("banco.beneficiario.conta_bancaria", "Conta bancária é obrigatória"));
        }

        if (request.Boletos.Count == 0)
            violations.Add(("boletos", "Pelo menos um boleto é obrigatório"));

        for (var i = 0; i < request.Boletos.Count; i++)
        {
            if (request.Boletos[i].Pagador == null)
                violations.Add(($"boletos[{i}].pagador", "Pagador é obrigatório"));
        }

        if (violations.Count > 0)
            throw RpcErrors.InvalidArgument(violations.ToArray());
    }
}
