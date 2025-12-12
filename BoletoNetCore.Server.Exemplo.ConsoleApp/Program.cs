using System.Diagnostics;
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.V1;
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
    var request = CreateGerarBoletoRequest();

    Console.WriteLine($"Gerando {request.Boletos.Count} boletos para o banco {request.BancoCodigo} (Sicredi)...");
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

static GerarBoletoRequest CreateGerarBoletoRequest()
{
    // Banco Sicredi (748) - mesmo do exemplo QuestPDF
    return GerarBoletoRequest.Builder(bancoCodigo: 748)
        .ComCarteira("1", TipoCarteira.CobrancaSimples, variacao: "A")
        .ComBeneficiario(Beneficiario.CriarValido(
            cpfCnpj: "86875666000109",
            nome: "Beneficiario Teste",
            contaBancaria: ContaBancaria.CriarValido(
                agencia: "0156",
                conta: "85305",
                tipoFormaCadastramento: TipoFormaCadastramento.ComRegistro,
                tipoImpressaoBoleto: TipoImpressaoBoleto.Empresa,
                digitoConta: "4",
                operacaoConta: "05"),
            codigo: "85305",
            endereco: Endereco.CriarValido(
                logradouro: "Rua Teste do Beneficiário",
                numero: "789",
                bairro: "Bairro",
                cidade: "Cidade",
                uf: "SP",
                cep: "65432987",
                complemento: "Cj 333")))
        .ComBoleto(
            CreateBoletoInput(1),
            CreateBoletoInput(2),
            CreateBoletoInput(3),
            CreateBoletoInput(4))
        .Build();
}

static BoletoInput CreateBoletoInput(int index)
{
    var isOdd = index % 2 == 1;
    var baseDate = DateTime.Today;
    var vencimento = baseDate.AddMonths(index);

    var pagador = CreatePagador(isOdd);

    return BoletoInput.Builder()
        .ComPagador(pagador)
        .ComVencimento(vencimento)
        .ComValor(100m * index)
        .ComNossoNumero($"{index:00000000}")
        .ComDocumento($"BB{index:000}{(char)('A' + index - 1)}", TipoEspecieDocumento.Dm)
        .ComAceite(isOdd ? "N" : "A")
        .ComDataEmissao(baseDate.AddDays(-3))
        .ComDataProcessamento(baseDate)
        .ComInstrucoes("11", "22")
        .ComMultaPercentual(vencimento.AddDays(1), 2m)
        .ComJurosPercentual(vencimento.AddDays(1), 0.2m)
        .ComDesconto(vencimento.AddDays(-10), (100m * index) / 10)
        .ComDesconto(vencimento.AddDays(-5), (100m * index) * 12 / 100)
        .ComDesconto(vencimento.AddDays(-2), (100m * index) * 13 / 100)
        .Build();
}

static Pagador CreatePagador(bool isCompany)
{
    if (isCompany)
    {
        return Pagador.CriarValido(
            cpfCnpj: "71738978000101",
            nome: "Pagador Teste PJ",
            endereco: Endereco.CriarValido(
                logradouro: "Avenida Testando",
                numero: "123",
                bairro: "Bairro PJ",
                cidade: "Cidade PJ",
                uf: "RJ",
                cep: "12345678"));
    }

    return Pagador.CriarValido(
        cpfCnpj: "44331610128",
        nome: "Pagador Teste PF",
        endereco: Endereco.CriarValido(
            logradouro: "Rua Testando",
            numero: "456",
            bairro: "Bairro PF",
            cidade: "Cidade PF",
            uf: "MG",
            cep: "87654321"));
}
