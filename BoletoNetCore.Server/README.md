# BoletoNetCore.Server

API gRPC em ASP.NET Core com transcoding JSON para suporte dual gRPC/REST.

| Projeto                          | Target            | Propósito                          |
| -------------------------------- | ----------------- | ---------------------------------- |
| `BoletoNetCore.Server`           | .NET 9.0          | Servidor gRPC com transcoding JSON |
| `BoletoNetCore.Server.Contracts` | .NET Standard 2.0 | Contratos Protobuf                 |

---

# Consumindo o Serviço

## Executando com Docker

```bash
# Build da imagem
docker build -t boletonetcore-server -f BoletoNetCore.Server/Dockerfile .

# Executar o container
docker run -d \
  --name boletonetcore \
  -p 5000:8080 \
  -p 5001:8081 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  boletonetcore-server
```

**Portas:**

| Ambiente | HTTP/REST | gRPC |
|----------|-----------|------|
| Production (padrão) | 8080 | 8081 |
| Development | 5000 | 5001 |

O container usa Production por padrão. Para Development:
```bash
docker run -d \
  -p 5000:5000 \
  -p 5001:5001 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  boletonetcore-server
```

## Gerando Boletos via gRPC

### C# (.NET)

```bash
dotnet add package BoletoNetCore.Server.Contracts
```

```csharp
using BoletoNetCore.Server.Contracts.Generated.Types;
using BoletoNetCore.Server.Contracts.Generated.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

// Conectar ao servidor gRPC
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new BoletoV1.BoletoV1Client(channel);

// Montar requisição
var request = new GerarBoletoRequest
{
    Banco = new Banco
    {
        Codigo = 748, // Sicredi
        Beneficiario = new Beneficiario
        {
            CpfCnpj = "86875666000109",
            Nome = "Empresa Exemplo LTDA",
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
                LogradouroEndereco = "Rua Exemplo",
                LogradouroNumero = "100",
                Bairro = "Centro",
                Cidade = "São Paulo",
                Uf = "SP",
                Cep = "01234567"
            }
        }
    },
    OutputFormat = BoletoOutputFormat.Pdf
};

// Adicionar boletos à requisição
request.Boletos.Add(new Boleto
{
    Carteira = "1",
    VariacaoCarteira = "A",
    TipoCarteira = TipoCarteira.CarteiraCobrancaSimples,
    Pagador = new Pagador
    {
        CpfCnpj = "44331610128",
        Nome = "João da Silva",
        Endereco = new Endereco
        {
            LogradouroEndereco = "Av. Brasil",
            LogradouroNumero = "500",
            Bairro = "Jardins",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "04567890"
        }
    },
    DataVencimento = Timestamp.FromDateTime(DateTime.Today.AddDays(30).ToUniversalTime()),
    ValorTitulo = 150.00m,
    NossoNumero = "00000001",
    NumeroDocumento = "DOC001",
    EspecieDocumento = TipoEspecieDocumento.Dm
});

// Gerar boleto
var response = await client.GerarBoletoAsync(request);

// Processar resposta
foreach (var resultado in response.Boletos)
{
    Console.WriteLine($"Nosso Número: {resultado.NossoNumeroFormatado}");
    Console.WriteLine($"Código de Barras: {resultado.CodigoBarras}");
    Console.WriteLine($"Linha Digitável: {resultado.LinhaDigitavel}");
}

// Salvar PDF
await File.WriteAllBytesAsync("boleto.pdf", response.Conteudo.ToByteArray());
```

> **Nota**: Os contratos gRPC espelham diretamente as classes do BoletoNetCore, permitindo migração simplificada de código que já utiliza a biblioteca.

### Outras Linguagens

Gere clientes a partir dos arquivos `.proto` disponíveis em [`BoletoNetCore.Server.Contracts/boletonetcore/`](../BoletoNetCore.Server.Contracts/boletonetcore/):

Consulte a [documentação oficial do gRPC](https://grpc.io/docs/languages/) para guias específicos por linguagem.

## Gerando Boletos via REST

A API REST não requer instalação de pacotes - utilize qualquer cliente HTTP com JSON.

**Swagger UI**: Acesse `http://localhost:5000/swagger` (Development) para documentação interativa com exemplos prontos para execução.

**OpenAPI Spec**: Disponível em `http://localhost:5000/swagger/v1/swagger.json` para geração de clientes.

### Gerando Clientes Tipados

Para clientes com tipos gerados automaticamente, utilize ferramentas de geração OpenAPI:

| Linguagem | Ferramenta |
|-----------|------------|
| C# | [NSwag](https://github.com/RicoSuter/NSwag), [Kiota](https://github.com/microsoft/kiota) |
| TypeScript | [openapi-typescript](https://github.com/drwpow/openapi-typescript) |
| Outras | [OpenAPI Generator](https://openapi-generator.tech/) |

## Endpoints Disponíveis

- **Health check**: `http://localhost:5000/health`
- **Swagger UI**: `http://localhost:5000/swagger` (apenas em Development)
- **gRPC**: `http://localhost:5001` (use grpcurl ou cliente gRPC)
- **REST**: `http://localhost:5000/api/v1/boletos` (POST)

---

# Desenvolvimento

## Estrutura do Projeto

```
BoletoNetCore.Server/
├── Errors/                 # Helpers de erro gRPC (RpcErrors)
├── Extensions/             # Extensões de IServiceCollection
├── Interceptors/           # Interceptors gRPC (logging, tratamento de erros)
├── Services/V{n}/          # Implementações de serviço por versão
├── Versioning/             # Gerenciamento de versão da API
└── Program.cs

BoletoNetCore.Server.Contracts/
└── boletonetcore/
    ├── {domínio}/v{n}/     # Serviços de domínio por versão
    └── types/              # Tipos proto compartilhados
```

## Versionamento

- String de versão centralizada em `ApiVersions.Versions`
- Arquivos proto: `boletonetcore/{domínio}/v{n}/{serviço}.proto`
- Namespace C#: `BoletoNetCore.Server.Contracts.Generated.V{n}`
- Rotas HTTP: `/api/v{n}/{recurso}`
- Classes de serviço: `{Domínio}V{n}Service`

### Adicionando uma Nova Versão

1. Adicione a versão ao array `ApiVersions.Versions`
2. Crie o arquivo proto em `boletonetcore/{domínio}/v{n}/`
3. Crie a classe de serviço em `Services/V{n}/`
4. Registre o serviço em `Program.cs` via `MapGrpcService<>()`

## Convenções Proto

### Estrutura de Arquivo

```protobuf
syntax = "proto3";

option csharp_namespace = "BoletoNetCore.Server.Contracts.Generated.V1";
import "google/api/annotations.proto";

package boletonetcore.{domínio}.v1;

message {Entidade}Request { }
message {Entidade}Response { }

service {Domínio}V1 {
    rpc {Ação}({Entidade}Request) returns ({Entidade}Response) {
        option (google.api.http) = {
            post: "/api/v1/{recurso}"
            body: "*"
        };
    }
}
```

### Regras de Transcoding HTTP

| Método HTTP | Body         | Caso de Uso                            |
| ----------- | ------------ | -------------------------------------- |
| `GET`       | Nenhum       | Operações de leitura (use query params)|
| `POST`      | `body: "*"`  | Operações de criação                   |
| `PUT`       | `body: "*"`  | Atualizações completas                 |
| `PATCH`     | `body: "*"`  | Atualizações parciais                  |
| `DELETE`    | Nenhum       | Operações de exclusão                  |

### Tipos Compartilhados

Localizados em `boletonetcore/types/`. Importe conforme necessário.

**Money** (`types/money.proto`): Valor monetário em reais com precisão nano.
```protobuf
import "boletonetcore/types/money.proto";

message Exemplo {
    boletonetcore.types.Money valor = 1;
}
```

O tipo `Money` possui conversões implícitas para `decimal` em C#:
```csharp
// Atribuir decimal diretamente
boleto.ValorTitulo = 150.00m;

// Converter para decimal
decimal valor = response.Boletos[0].ValorTitulo;
```

## Implementação de Serviço

```csharp
public class {Domínio}V{n}Service : {Domínio}V{n}.{Domínio}V{n}Base
{
    public override Task<Response> Method(Request request, ServerCallContext context)
    {
        // Implementação
    }
}
```

- Lance exceções .NET padrão; `LoggerInterceptor` mapeia para códigos de status gRPC
- Lance `RpcException` diretamente para controle explícito de status gRPC

### Mapeamento de Exceção para Código de Status (LoggerInterceptor)

| Exceção                        | Status gRPC        |
| ------------------------------ | ------------------ |
| `ArgumentException`            | `InvalidArgument`  |
| `InvalidOperationException`    | `FailedPrecondition` |
| `NotImplementedException`      | `Unimplemented`    |
| `UnauthorizedAccessException`  | `PermissionDenied` |
| `TimeoutException`             | `DeadlineExceeded` |
| `OperationCanceledException`   | `Cancelled`        |
| Outras                         | `Internal`         |

### Erros de Validação (RpcErrors)

Use `RpcErrors` para erros de validação estruturados com detalhes por campo:

```csharp
throw RpcErrors.InvalidArgument(
    ("boleto.nosso_numero", "NossoNumero é obrigatório"),
    ("boleto.valor", "Valor deve ser maior que zero"));
```

Utiliza o [modelo de erro enriquecido do Google](https://cloud.google.com/apis/design/errors#error_details) com `BadRequest.FieldViolation`.

## Extensões de Configuração

Adicione novas configurações via métodos de extensão em `Extensions/ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection Configure{Funcionalidade}(this IServiceCollection services)
{
    // Configuração
    return services;
}
```

Encadeie em `Program.cs`:
```csharp
builder.Services
    .ConfigureCoreServices()
    .ConfigureLogging(builder.Configuration)
    .ConfigureGrpc()
    .ConfigureSwagger()
    .Configure{Funcionalidade}();
```
