# Implementation Plan: Base de conhecimento da Api.RAG

**Branch**: `003-base-conhecimento-rag` | **Date**: 2026-10-29 | **Spec**: [spec.md](./spec.md)

**Note**: Este plano cobre a Api.RAG. Não cria tarefas e não implementa código.

## Summary

A Api.RAG grava documento, versão e chunk no PostgreSQL próprio, expõe a administração em REST e responde `consultar_conhecimento` por MCP. A ferramenta recebe texto e vigência e devolve evidência no formato que a Api.Agente já desserializa. Caso corporativo confirmado é gravado e lido na administração. A ferramenta não o devolve. Modelo, dimensão, topK, limiar e tamanho de chunk ficam vazios até avaliação. Sem esses valores, a consulta é falha técnica, não lista vazia.

## Technical Context

**Language/Version**: .NET 10 com ASP.NET Core

**Primary Dependencies**: Npgsql 10.0.3, já fixado no repositório; Pgvector 0.3.2; ModelContextProtocol.AspNetCore 2.3.0, na mesma linha do cliente já usado; PdfPig 0.1.16 para texto de PDF. XML de notas usa a biblioteca padrão do .NET. Sem Entity Framework.

**Storage**: PostgreSQL exclusivo da Api.RAG, connection string `Rag`. A primeira migração cria a extensão `vector`, documento e chunk, sem coluna de embedding. A coluna `vector(N)` e o índice entram numa migração posterior, só quando a dimensão e a métrica tiverem sido avaliadas e registradas.

**Testing**: xUnit em `tests/Api.RAG.Tests`. Ambiente Testing usa repositório em memória e não chama OpenAI nem PostgreSQL. A suíte cobre reingestão, versão isolada, lista vazia depois de busca concluída, fonte rastreável, caso ausente na ferramenta e falha quando a busca não está configurada. O arquivo `contracts/rag-v1.openapi.yaml` será a serialização de `/openapi/v1.json`. O teste de contrato recusa divergência e não regrava o arquivo.

**Target Platform**: API HTTP com perfis Development, Testing e Production, cada um na própria porta.

**Project Type**: Segundo projeto web da solução. Pastas de endpoint, aplicação, domínio e infraestrutura. A Api.Agente não referencia este projeto.

**Constraints**: Não inventar schema fiscal, alíquota, XSD, modelo de embedding, dimensão, topK, limiar nem tamanho de chunk. Não devolver caso corporativo na ferramenta atual. Não ler o banco da Api.Agente.

## Constitution Check

- A Api.RAG é dona de documento, chunk e vetor. A Api.Agente continua só com o cliente MCP.
- Conteúdo ingerido e chunk recuperado são dados. Não alteram chave, faixa nem contrato.
- A retentativa da chamada de embedding é limitada: 2 tentativas adicionais e 60 segundos, o mesmo teto já fixado para chamada externa de modelo. Os valores ficam em configuração.
- DP-12 permanece aberta. Este plano não escolhe modelo, dimensão, topK, limiar, métrica nem índice vetorial.
- DP-16 não cria biblioteca extra. O projeto Api.RAG já é um dos dois projetos previstos.
- O limite numérico de arquivo continua vazio. Arquivo sem esse valor é recusado.

Gate de desenho: aprovado para gravar, versionar e administrar. A busca vetorial permanece bloqueada até a avaliação da DP-12.

## Project Structure

### Documentation (this feature)

```text
specs/003-base-conhecimento-rag/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── rag-v1.md
└── tasks.md

docs/adr/
└── 0005-rag-em-postgres-proprio.md
```

`tasks.md` fica para a etapa seguinte. O contrato OpenAPI publicado será gerado de `/openapi/v1.json` na implementação, no mesmo critério da Api.Agente.

### Source Code (repository root)

```text
src/Api.RAG/
├── Endpoints/
├── Application/
├── Domain/
└── Infrastructure/

tests/Api.RAG.Tests/
├── unit/
├── contract/
└── integration/
```

**Structure Decision**: Um projeto web. A solução passa a ter Api.Agente e Api.RAG. Nenhum dos dois referencia o projeto do outro.

## Fluxo técnico

1. O POST exige a chave configurada, origem, identificação, versão, faixa e vigência inicial.
2. Texto puro segue o corpo. PDF e XML de notas só seguem quando `Ingestao:TamanhoMaximoBytes` está configurado e o arquivo cabe nele.
3. O hash SHA-256 do conteúdo decide: igual ao já gravado ignora; diferente na mesma versão recusa; outra versão convive.
4. Sem `Embedding:TamanhoChunk`, o texto extraído inteiro vira um único chunk. Com o valor configurado, a segmentação usa esse tamanho e a sobreposição configurada.
5. A versão fica legível na administração. Ela só fica consultável pela ferramenta quando todos os chunks tiverem embedding e os parâmetros de busca estiverem configurados.
6. `consultar_conhecimento` filtra o intervalo de vigência, remove a faixa de caso corporativo e devolve a data pedida em cada evidência. A fonte é `origem/identificacao/versao`.
7. Parâmetro de busca vazio ou falha do provedor devolve erro da ferramenta. A Api.Agente já trata esse erro como falha técnica. Lista vazia só sai de uma busca concluída sem resultado.

## Configuração

- `Ingestao:Chave`: vazia recusa o POST. Não entra no repositório.
- `Ingestao:TamanhoMaximoBytes`: vazio recusa PDF e XML.
- `Embedding:Model`, `Embedding:Dimensao`, `Embedding:TopK`, `Embedding:Limiar`, `Embedding:TamanhoChunk`, `Embedding:Sobreposicao`: vazios. Nenhum recebe número neste plano.
- `Embedding:Tentativas`: 2.
- `Embedding:TimeoutSegundos`: 60.
- `ConnectionStrings:Rag`: vazia fora de produção configurada. Em Testing o repositório é memória.

Segredos ficam no ambiente.

## Perfis locais

- Development: `http://localhost:5120`
- Testing: `http://localhost:5121`
- Production: `http://localhost:5122`

A raiz redireciona para `/swagger` e esse redirecionamento fica fora do documento. O MCP fica em `/mcp`. A Api.Agente aponta `Conhecimento:McpUrl` para essa URL absoluta quando os dois processos estiverem no ar.

## Documentação Swagger

O documento OpenAPI se chama `v1` e a versão publicada é 1.0.0. Título, descrição da API, cada operação, cada resposta e cada propriedade de schema têm texto objetivo. A descrição de propriedade sai de `DescriptionAttribute`, no mesmo mecanismo da Api.Agente. O servidor do documento é `/`. A interface fica em `/swagger` e aponta para `/openapi/v1.json`. A ferramenta MCP não vira rota REST: a descrição do documento informa que a consulta da análise é `consultar_conhecimento`. Uma mudança incompatível será um documento v2.

## Fora deste plano

Escolher modelo e dimensão de embedding, métrica, índice, topK e limiar. Recuperar caso corporativo na ferramenta. Memória, custo, papéis e A2A. Schema oficial de XML de notas.

## Complexity Tracking

Nenhuma violação da constituição precisa de exceção. A coluna vetorial adiada é a restrição da DP-12, não um desvio.
