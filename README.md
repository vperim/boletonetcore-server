# BoletoNetCore Server

Fork do [BoletoNetCore](https://github.com/BoletoNet/BoletoNetCore) que adiciona um servidor gRPC/REST para geração de boletos bancários brasileiros.

> **Preview**: Este projeto está em desenvolvimento ativo. APIs e contratos podem sofrer alterações até a versão estável.

> **Nota**: Este projeto não mantém a biblioteca BoletoNetCore. A biblioteca é sincronizada do upstream conforme necessário. Para documentação da biblioteca, consulte o [repositório original](https://github.com/BoletoNet/BoletoNetCore).

## Quick Start

```bash
# Executar via Docker
docker run -d \
  -p 5000:8080 \
  -p 5001:8081 \
  ghcr.io/vperim/boletonetcore-server:latest

# Instalar pacote de contratos
dotnet add package BoletoNetCore.Server.Contracts
```

**Portas:**

| Ambiente | HTTP/REST | gRPC |
|----------|-----------|------|
| Production (padrão) | 8080 | 8081 |
| Development | 5000 | 5001 |

**Endpoints:**
- Health check: `http://localhost:5000/health`
- Swagger: `http://localhost:5000/swagger` (apenas Development)
- gRPC: `http://localhost:5001`

## Documentação

- [Guia de Consumo e Desenvolvimento](BoletoNetCore.Server/README.md) - Setup, exemplos de código, convenções do projeto

## Estrutura do Projeto

| Projeto | Descrição |
|---------|-----------|
| `BoletoNetCore.Server` | Servidor gRPC com transcoding JSON |
| `BoletoNetCore.Server.Contracts` | Contratos Protobuf para clientes |
| `BoletoNetCore` | Biblioteca original (upstream) |

## Branches

| Branch | Propósito |
|--------|-----------|
| `server/main` | Branch principal do fork |
| `master` | Sincronização com upstream |

## Contribuindo

**Escopo de manutenção:**
- Issues e PRs relacionados ao **servidor gRPC/REST** são bem-vindos
- Issues relacionados à **biblioteca BoletoNetCore** (geração de boletos, CNAB, bancos) devem ser reportados no [repositório original](https://github.com/BoletoNet/BoletoNetCore/issues)

Este fork sincroniza periodicamente com o upstream, mas não mantém a biblioteca core.

## Licença

MIT License - Veja [LICENSE](LICENSE) para detalhes.

Este projeto é derivado do [BoletoNetCore](https://github.com/BoletoNet/BoletoNetCore) © 2019 BoletoNet.
