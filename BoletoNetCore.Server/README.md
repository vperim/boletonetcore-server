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
using Grpc.Net.Client;

// Conectar ao servidor gRPC
using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new BoletoV1.BoletoV1Client(channel);

// Dados do beneficiário (empresa emissora do boleto)
var enderecoBeneficiario = Endereco.CriarValido(
    logradouro: "Rua Exemplo",
    numero: "100",
    bairro: "Centro",
    cidade: "São Paulo",
    uf: "SP",
    cep: "01234567");

var contaBancaria = ContaBancaria.CriarValido(
    agencia: "0156",
    conta: "85305",
    tipoFormaCadastramento: TipoFormaCadastramento.ComRegistro,
    tipoImpressaoBoleto: TipoImpressaoBoleto.Empresa,
    digitoConta: "4",
    operacaoConta: "05");

var beneficiario = Beneficiario.CriarValido(
    cpfCnpj: "86875666000109",
    nome: "Empresa Exemplo LTDA",
    contaBancaria: contaBancaria,
    codigo: "85305",
    endereco: enderecoBeneficiario);

// Dados do pagador (cliente que receberá o boleto)
var enderecoPagador = Endereco.CriarValido(
    logradouro: "Av. Brasil",
    numero: "500",
    bairro: "Jardins",
    cidade: "São Paulo",
    uf: "SP",
    cep: "04567890");

var pagador = Pagador.CriarValido(
    cpfCnpj: "44331610128",
    nome: "João da Silva",
    endereco: enderecoPagador);

// Dados do boleto
var boleto = BoletoInput.Builder()
    .ComPagador(pagador)
    .ComVencimento(DateTime.Today.AddDays(30))
    .ComValor(150.00m)
    .ComNossoNumero("00000001")
    .ComDocumento("DOC001", TipoEspecieDocumento.Dm)
    .Build();

// Montar requisição
var request = GerarBoletoRequest.Builder(bancoCodigo: 748) // Sicredi
    .ComCarteira("1", TipoCarteira.CobrancaSimples, variacao: "A")
    .ComBeneficiario(beneficiario)
    .ComBoleto(boleto)
    .Build();

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
- Namespace C#: `BoletoNetCore.Protos.Generated.V{n}`
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
