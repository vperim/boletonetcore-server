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

    public override Task<GerarBoletoResponse> GerarBoleto(GerarBoletoRequest request, ServerCallContext context)
    {
        ValidateRequest(request);

        var result = this.generator.Generate(request);

        var response = new GerarBoletoResponse
        {
            Conteudo = ByteString.CopyFrom(result.Content),
            ContentType = result.ContentType,
        };

        response.Boletos.AddRange(result.Boletos);

        this.logger.LogInformation(
            "Generated {Count} boleto(s) for bank {BankCode}, carteira {Carteira}",
            result.Boletos.Count,
            request.BancoCodigo,
            request.Carteira);

        return Task.FromResult(response);
    }

    private static void ValidateRequest(GerarBoletoRequest request)
    {
        var violations = new List<(string Field, string Description)>();

        if (request.BancoCodigo <= 0)
            violations.Add(("banco_codigo", "Código do banco é obrigatório"));

        if (string.IsNullOrWhiteSpace(request.Carteira))
            violations.Add(("carteira", "Carteira é obrigatória"));

        if (request.Beneficiario == null)
            violations.Add(("beneficiario", "Beneficiário é obrigatório"));
        else
        {
            if (string.IsNullOrWhiteSpace(request.Beneficiario.CpfCnpj))
                violations.Add(("beneficiario.cpf_cnpj", "CPF/CNPJ do beneficiário é obrigatório"));

            if (string.IsNullOrWhiteSpace(request.Beneficiario.Nome))
                violations.Add(("beneficiario.nome", "Nome do beneficiário é obrigatório"));

            if (request.Beneficiario.ContaBancaria == null)
                violations.Add(("beneficiario.conta_bancaria", "Conta bancária é obrigatória"));
            else
            {
                if (string.IsNullOrWhiteSpace(request.Beneficiario.ContaBancaria.Agencia))
                    violations.Add(("beneficiario.conta_bancaria.agencia", "Agência é obrigatória"));

                if (string.IsNullOrWhiteSpace(request.Beneficiario.ContaBancaria.Conta))
                    violations.Add(("beneficiario.conta_bancaria.conta", "Conta é obrigatória"));
            }
        }

        if (request.Boletos.Count == 0)
            violations.Add(("boletos", "Pelo menos um boleto é obrigatório"));

        for (var i = 0; i < request.Boletos.Count; i++)
        {
            var boleto = request.Boletos[i];

            if (boleto.Pagador == null)
                violations.Add(($"boletos[{i}].pagador", "Pagador é obrigatório"));
            else
            {
                if (string.IsNullOrWhiteSpace(boleto.Pagador.CpfCnpj))
                    violations.Add(($"boletos[{i}].pagador.cpf_cnpj", "CPF/CNPJ do pagador é obrigatório"));

                if (string.IsNullOrWhiteSpace(boleto.Pagador.Nome))
                    violations.Add(($"boletos[{i}].pagador.nome", "Nome do pagador é obrigatório"));
            }

            if (boleto.DataVencimento == null)
                violations.Add(($"boletos[{i}].data_vencimento", "Data de vencimento é obrigatória"));

            if (boleto.ValorTitulo == null || (boleto.ValorTitulo.Units <= 0 && boleto.ValorTitulo.Nanos <= 0))
                violations.Add(($"boletos[{i}].valor_titulo", "Valor do título deve ser maior que zero"));

            if (string.IsNullOrWhiteSpace(boleto.NumeroDocumento))
                violations.Add(($"boletos[{i}].numero_documento", "Número do documento é obrigatório"));
        }

        if (violations.Count > 0)
            throw RpcErrors.InvalidArgument(violations.ToArray());
    }
}
