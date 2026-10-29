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

Development escuta `http://localhost:5120` e abre `/swagger`. O documento versionado fica em `/openapi/v1.json` e o arquivo `contracts/rag-v1.openapi.yaml` é a cópia gerada dele. Testing escuta `http://localhost:5121`. Production escuta `http://localhost:5122`.

A ferramenta MCP fica em `/mcp`. Na Api.Agente, `Conhecimento:McpUrl` aponta para `http://localhost:5120/mcp` quando a base local for a de Development.

## O que responde antes da DP-12

Texto com origem, identificação, versão, faixa e vigência pode ser gravado e lido em `GET /v1/documentos/{documentoId}`. PDF e XML ficam recusados enquanto `Ingestao:TamanhoMaximoBytes` estiver vazio. `consultar_conhecimento` responde erro enquanto modelo, dimensão, topK ou limiar estiverem vazios.

## Teste

```powershell
dotnet test tests/Api.RAG.Tests/Api.RAG.Tests.csproj
```

A suíte não usa chave real, não chama o provedor de embedding e não exige PostgreSQL.
