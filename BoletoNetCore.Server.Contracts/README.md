# BoletoNetCore.Server.Contracts

Contratos gRPC (definições Protobuf) para comunicação com o [BoletoNetCore.Server](https://github.com/vperim/boletonetcore-server).

## Instalação

```bash
dotnet add package BoletoNetCore.Server.Contracts
```

## Uso

```csharp
using BoletoNetCore.Server.Contracts.Generated.V1;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5001");
var client = new BoletoV1.BoletoV1Client(channel);

var response = await client.GerarBoletoAsync(request);
```

## Documentação

Consulte o [repositório principal](https://github.com/vperim/boletonetcore-server) para documentação completa e exemplos.

## Licença

MIT - Baseado em [BoletoNetCore](https://github.com/BoletoNet/BoletoNetCore) © 2019 BoletoNet.
