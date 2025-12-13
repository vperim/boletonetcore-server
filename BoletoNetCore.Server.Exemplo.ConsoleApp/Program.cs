using System.Diagnostics;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

Console.WriteLine("=== BoletoNetCore - Exemplo de Cliente gRPC ===");
Console.WriteLine();

// Obter porta do servidor gRPC (HTTP/2)
// Nota: O servidor usa porta 5000 para HTTP/1.1 (Swagger) e 5001 para HTTP/2 (gRPC)
Console.Write("Informe a porta do servidor gRPC (padrão: 5001): ");
var portInput = Console.ReadLine();
var port = string.IsNullOrWhiteSpace(portInput) ? "5001" : portInput.Trim();
var serverAddress = $"http://localhost:{port}";

// Obter diretório de saída
var defaultDir = Path.Combine(AppContext.BaseDirectory, "Output");
Directory.CreateDirectory(defaultDir);
Console.Write($"Informe o diretório para salvar o PDF (padrão: {defaultDir}): ");
var outputDir = Console.ReadLine();
if (string.IsNullOrWhiteSpace(outputDir))
{
    outputDir = defaultDir;
}

try
{
    Console.WriteLine();
    Console.WriteLine($"Conectando ao servidor gRPC em {serverAddress}...");

    using var channel = GrpcChannel.ForAddress(serverAddress);
    var client = new BoletoV1.BoletoV1Client(channel);

    // Montar requisição com banco Sicredi (748) - mesmo do exemplo QuestPDF
    var request = CreateGerarBoletoBradescoRequest();

    Console.WriteLine($"Gerando {request.Boletos.Count} boletos para o banco {request.Banco.Codigo} (Sicredi)...");
    Console.WriteLine();

    // Chamar serviço gRPC com medição de tempo
    var stopwatch = Stopwatch.StartNew();
    var response = await client.GerarBoletoAsync(request);
    stopwatch.Stop();

    // Exibir resultados
    Console.WriteLine($"Tempo de processamento: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine();
    Console.WriteLine("=== Boletos Gerados ===");
    Console.WriteLine();

    foreach (var boleto in response.Boletos)
    {
        Console.WriteLine($"Documento: {boleto.NumeroDocumento}");
        Console.WriteLine($"  Nosso Número: {boleto.NossoNumeroFormatado}");
        Console.WriteLine($"  Código de Barras: {boleto.CodigoBarras}");
        Console.WriteLine($"  Linha Digitável: {boleto.LinhaDigitavel}");
        Console.WriteLine();
    }

    // Salvar PDF com timestamp no nome
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var pdfFileName = $"carne_grpc_{timestamp}.pdf";
    var pdfPath = Path.Combine(outputDir, pdfFileName);
    await File.WriteAllBytesAsync(pdfPath, response.Conteudo.ToByteArray());

    Console.WriteLine($"Content-Type: {response.ContentType}");
    Console.WriteLine($"Tamanho do PDF: {response.Conteudo.Length:N0} bytes");
    Console.WriteLine();
    Console.WriteLine("Arquivo gerado:");
    Console.WriteLine(pdfPath);
}
catch (Grpc.Core.RpcException ex)
{
    Console.WriteLine($"Erro gRPC: {ex.Status.StatusCode} - {ex.Status.Detail}");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("Pressione qualquer tecla para sair...");
Console.ReadKey();
return;

static GerarBoletoRequest CreateGerarBoletoBradescoRequest()
{
    throw new NotImplementedException();
}

static GerarBoletoRequest CreateGerarBoletoSindicredRequest()
{
    // Banco Sicredi (748) - mesmo do exemplo QuestPDF
    var request = new GerarBoletoRequest
    {
        Banco = new Banco
        {
            Codigo = 748,
            Beneficiario = new Beneficiario
            {
                CpfCnpj = "86875666000109",
                Nome = "Beneficiario Teste",
                Codigo = "85305",
                ContaBancaria = new ContaBancaria
                {
                    Agencia = "0156",
                    Conta = "85305",
                    DigitoConta = "4",
                    OperacaoConta = "05",
                    TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa,
                    CarteiraPadrao = "1/A"
                },
                Endereco = new Endereco
                {
                    LogradouroEndereco = "Rua Teste do Beneficiário",
                    LogradouroNumero = "789",
                    LogradouroComplemento = "Cj 333",
                    Bairro = "Bairro",
                    Cidade = "Cidade",
                    Uf = "SP",
                    Cep = "65432987",
                },
            },
        },
        OutputFormat = BoletoOutputFormat.Pdf,
    };

    request.Boletos.Add(CreateBoletoInput(1));
    request.Boletos.Add(CreateBoletoInput(2));
    request.Boletos.Add(CreateBoletoInput(3));
    request.Boletos.Add(CreateBoletoInput(4));

    return request;
}

static Boleto CreateBoletoInput(int index)
{
    var isOdd = index % 2 == 1;
    var baseDate = DateTime.Today;
    var vencimento = baseDate.AddMonths(index);

    var boleto = new Boleto
    {
        // Carteira
        Carteira = "1",
        VariacaoCarteira = "A",
        TipoCarteira = TipoCarteira.CarteiraCobrancaSimples,

        // Pagador
        Pagador = CreatePagador(isOdd),

        // Datas
        DataVencimento = Timestamp.FromDateTime(vencimento.ToUniversalTime()),
        DataEmissao = Timestamp.FromDateTime(baseDate.AddDays(-3).ToUniversalTime()),
        DataProcessamento = Timestamp.FromDateTime(baseDate.ToUniversalTime()),

        // Valores e identificação
        ValorTitulo = 100m * index,
        NossoNumero = $"{index:00000000}",
        NumeroDocumento = $"BB{index:000}{(char)('A' + index - 1)}",
        EspecieDocumento = TipoEspecieDocumento.Dm,
        Aceite = isOdd ? "N" : "A",

        // Instruções
        CodigoInstrucao1 = "11",
        CodigoInstrucao2 = "22",

        // Multa
        DataMulta = Timestamp.FromDateTime(vencimento.AddDays(1).ToUniversalTime()),
        PercentualMulta = "2",
        TipoCodigoMulta = TipoCodigoMulta.Percentual,

        // Juros
        DataJuros = Timestamp.FromDateTime(vencimento.AddDays(1).ToUniversalTime()),
        PercentualJurosDia = "0.2",
        TipoJuros = TipoJuros.Simples,
    };

    // Descontos
    boleto.Descontos.Add(new Desconto
    {
        Data = Timestamp.FromDateTime(vencimento.AddDays(-10).ToUniversalTime()),
        Valor = (100m * index) / 10,
    });
    boleto.Descontos.Add(new Desconto
    {
        Data = Timestamp.FromDateTime(vencimento.AddDays(-5).ToUniversalTime()),
        Valor = (100m * index) * 12 / 100,
    });
    boleto.Descontos.Add(new Desconto
    {
        Data = Timestamp.FromDateTime(vencimento.AddDays(-2).ToUniversalTime()),
        Valor = (100m * index) * 13 / 100,
    });

    return boleto;
}

static Pagador CreatePagador(bool isCompany)
{
    if (isCompany)
    {
        return new Pagador
        {
            CpfCnpj = "71738978000101",
            Nome = "Pagador Teste PJ",
            Endereco = new Endereco
            {
                LogradouroEndereco = "Avenida Testando",
                LogradouroNumero = "123",
                Bairro = "Bairro PJ",
                Cidade = "Cidade PJ",
                Uf = "RJ",
                Cep = "12345678",
            },
        };
    }

    return new Pagador
    {
        CpfCnpj = "44331610128",
        Nome = "Pagador Teste PF",
        Endereco = new Endereco
        {
            LogradouroEndereco = "Rua Testando",
            LogradouroNumero = "456",
            Bairro = "Bairro PF",
            Cidade = "Cidade PF",
            Uf = "MG",
            Cep = "87654321",
        },
    };
}
