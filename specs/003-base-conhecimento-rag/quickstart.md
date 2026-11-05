# Quickstart: Api.RAG

## Pré-requisitos

- .NET SDK 10
- PostgreSQL com permissão para `CREATE EXTENSION vector` somente quando `ConnectionStrings:Rag` estiver preenchida
- Chave de ingestão apenas no ambiente, no nome `Ingestao__Chave`

## Subir o perfil

Na raiz do repositório, depois que o projeto existir:

```powershell
dotnet run --project src/Api.RAG --launch-profile Development
```

Development escuta `http://localhost:5120` e abre `/swagger`. O documento publicado fica em `http://localhost:5120/openapi/v1.json`. A cópia versionada no repositório é `specs/003-base-conhecimento-rag/contracts/rag-v1.openapi.yaml`. Testing escuta `http://localhost:5121`. Production escuta `http://localhost:5122`.

A ferramenta MCP fica em `/mcp`. Na Api.Agente, `Conhecimento:McpUrl` aponta para `http://localhost:5120/mcp` quando a base local for a de Development.

## O que a busca exige

Texto com origem, identificação, versão, faixa e vigência pode ser gravado e lido em `GET /v1/documentos/{documentoId}`. PDF e XML ficam recusados enquanto `Ingestao:TamanhoMaximoBytes` estiver vazio. `consultar_conhecimento` responde erro enquanto modelo, dimensão, topK ou limiar estiverem vazios, ou quando o limiar estiver fora de 0 a 1. Com esses valores no ambiente, a busca é exata por cosseno e o embedding sai da OpenAI. A chave fica em `OpenAI__ApiKey`. Os arquivos versionados continuam sem esses números.

## Teste

```powershell
dotnet test tests/Api.RAG.Tests/Api.RAG.Tests.csproj
```

A suíte não usa chave real, não chama o provedor de embedding e não exige PostgreSQL.
