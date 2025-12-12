using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BoletoNetCore.Server.Versioning;

public class SwaggerDefaultRequestFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath != "api/v1/boletos")
            return;

        if (operation.RequestBody?.Content == null)
            return;

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
            return;

        mediaType.Example = CreateGerarBoletoRequestExample();
    }

    private static OpenApiObject CreateGerarBoletoRequestExample()
    {
        var today = DateTime.Today;
        var vencimento = today.AddDays(30);

        return new OpenApiObject
        {
            ["bancoCodigo"] = new OpenApiInteger(748), // Sicredi
            ["carteira"] = new OpenApiString("1"),
            ["variacaoCarteira"] = new OpenApiString("A"),
            ["tipoCarteira"] = new OpenApiString("TIPO_CARTEIRA_COBRANCA_SIMPLES"),
            ["beneficiario"] = new OpenApiObject
            {
                ["cpfCnpj"] = new OpenApiString("86875666000109"),
                ["nome"] = new OpenApiString("Beneficiario Teste"),
                ["codigo"] = new OpenApiString("85305"),
                ["contaBancaria"] = new OpenApiObject
                {
                    ["agencia"] = new OpenApiString("0156"),
                    ["conta"] = new OpenApiString("85305"),
                    ["digitoConta"] = new OpenApiString("4"),
                    ["operacaoConta"] = new OpenApiString("05"),
                    ["tipoFormaCadastramento"] = new OpenApiString("TIPO_FORMA_CADASTRAMENTO_COM_REGISTRO"),
                    ["tipoImpressaoBoleto"] = new OpenApiString("TIPO_IMPRESSAO_BOLETO_EMPRESA")
                },
                ["endereco"] = new OpenApiObject
                {
                    ["logradouro"] = new OpenApiString("Rua Teste do Beneficiário"),
                    ["numero"] = new OpenApiString("789"),
                    ["bairro"] = new OpenApiString("Bairro"),
                    ["cidade"] = new OpenApiString("Cidade"),
                    ["uf"] = new OpenApiString("SP"),
                    ["cep"] = new OpenApiString("65432987")
                }
            },
            ["boletos"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["pagador"] = new OpenApiObject
                    {
                        ["cpfCnpj"] = new OpenApiString("44331610128"),
                        ["nome"] = new OpenApiString("Pagador Teste PF"),
                        ["endereco"] = new OpenApiObject
                        {
                            ["logradouro"] = new OpenApiString("Rua Testando"),
                            ["numero"] = new OpenApiString("456"),
                            ["bairro"] = new OpenApiString("Bairro PF"),
                            ["cidade"] = new OpenApiString("Cidade PF"),
                            ["uf"] = new OpenApiString("MG"),
                            ["cep"] = new OpenApiString("87654321")
                        }
                    },
                    ["dataVencimento"] = new OpenApiString(FormatTimestamp(vencimento)),
                    ["valorTitulo"] = new OpenApiObject
                    {
                        ["currencyCode"] = new OpenApiString("BRL"),
                        ["units"] = new OpenApiLong(150),
                        ["nanos"] = new OpenApiInteger(0)
                    },
                    ["nossoNumero"] = new OpenApiString("00000001"),
                    ["numeroDocumento"] = new OpenApiString("DOC001"),
                    ["especieDocumento"] = new OpenApiString("TIPO_ESPECIE_DOCUMENTO_DM"),
                    ["aceite"] = new OpenApiString("N"),
                    ["dataEmissao"] = new OpenApiString(FormatTimestamp(today)),
                    ["dataProcessamento"] = new OpenApiString(FormatTimestamp(today))
                }
            },
            ["outputFormat"] = new OpenApiString("OUTPUT_FORMAT_PDF")
        };
    }

    private static string FormatTimestamp(DateTime date) =>
        date.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
}
