# Implementation Plan: Execução da análise fiscal de itens

**Branch**: `001-execucao-analise-fiscal` | **Date**: 2026-10-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-execucao-analise-fiscal/spec.md`

**Note**: Este plano cobre a execução da análise. Não cria tarefas e não implementa código.

## Summary

A Api.Agente recebe o contexto e os itens, valida o que é determinístico, consulta evidências por uma porta MCP e devolve sugestão, divergência ou bloqueio no contrato versão 1. A confiança segue a faixa da evidência vencedora. Arquivo sem limite configurado é recusado. Impacto relevante não é inferido.

## Technical Context

**Language/Version**: .NET 10 com ASP.NET Core

**Primary Dependencies**: Semantic Kernel 1.81.0, conector OpenAI 1.81.0, ModelContextProtocol 2.3.0, Npgsql 10.0.3, Microsoft.AspNetCore.OpenApi 10.0.14, Microsoft.OpenApi 2.12.2 e Swashbuckle.AspNetCore.SwaggerUI 10.3.1, fixados em `Directory.Packages.props`.

**Storage**: PostgreSQL da Api.Agente para execução e resultado. Sem vetor e sem memória conversacional nesta fatia.

**Testing**: xUnit. Testes de unidade do fluxo, teste de contrato do OpenAPI e teste de integração com dublê de conhecimento. O arquivo `contracts/analise-v1.openapi.yaml` é a serialização do documento publicado em `/openapi/v1.json`.

**Target Platform**: API HTTP executada em desenvolvimento, teste e produção com configuração própria.

**Project Type**: Serviço HTTP único, com pastas de endpoint, aplicação, domínio e infraestrutura.

**Performance Goals**: Sem meta numérica de vazão. O aceite é o comportamento da specification.

**Constraints**: Não inventar layout, alíquota, schema fiscal nem dispositivo legal. Não acessar o banco da Api.RAG. Não gravar caso corporativo. Confiança menor que 70 pede revisão. Impacto relevante e tamanho máximo continuam em aberto.

**Scale/Scope**: Uma execução com um ou mais itens. Quatro intenções: sugerir NCM, auditar o declarado, sugerir tratamento e sugerir XML do item.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- O fluxo obrigatório permanece na aplicação, com validadores determinísticos antes do modelo.
- As cinco classes de achado existem no contrato.
- A saída do modelo e o envelope da API são artefatos diferentes. O contrato versionado está em `contracts/analise-v1.openapi.yaml`.
- A comunicação com conhecimento passa por porta MCP. Não há acesso a tabela vetorial.
- Ferramenta selecionada pelo modelo não autoriza execução. Nesta fatia a única ferramenta externa prevista é a consulta de conhecimento, autorizada pela aplicação.
- Caso corporativo não é persistido.
- Memória, consumo, ingestão e A2A ficam fora e não ganham implementação implícita.
- Nenhum projeto extra foi criado. Api.RAG espera a specification da base.
- Limite de arquivo e impacto relevante não foram transformados em número ou regra fiscal.

Gate de desenho: aprovado. As pendências continuam explícitas e não viram comportamento silencioso.

## Project Structure

### Documentation (this feature)

```text
specs/001-execucao-analise-fiscal/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── analise-v1.openapi.yaml
└── tasks.md

docs/adr/
├── 0001-execucao-sincrona-e-postgresql.md
├── 0002-confianca-por-faixa-de-autoridade.md
└── 0003-conhecimento-por-porta-mcp.md
```

### Source Code (repository root)

```text
src/Api.Agente/
├── Endpoints/
├── Application/
├── Domain/
└── Infrastructure/

tests/Api.Agente.Tests/
├── unit/
├── contract/
└── integration/
```

**Structure Decision**: Um projeto web com quatro pastas. Bibliotecas separadas só entram se uma responsabilidade deixar de caber nesse projeto. Api.RAG não é criada nesta fatia.

## Fluxo técnico

1. O endpoint valida o contrato de entrada e rejeita contexto incompleto, tipo desconhecido e arquivo sem limite configurado.
2. A aplicação abre a execução, grava a trilha e separa os itens.
3. Texto segue para normalização. Arquivo com layout registrado segue para o validador daquele layout. Layout ausente produz erro estrutural.
4. Item sem dado essencial encerra só aquele item.
5. A porta de conhecimento devolve evidências da vigência. A aplicação ordena pela autoridade e calcula a confiança.
6. O modelo explica e escolhe apenas entre candidatos já recuperados. A aplicação recusa código ou referência fora das evidências.
7. A aplicação compara o declarado, marca divergência, calcula o estado da nota e monta o envelope.
8. O resultado é persistido e devolvido.

## Configuração

- `OpenAI:Model`: obrigatório, sem valor padrão no código.
- `Analise:TamanhoMaximoBytes`: vazio até o esclarecimento. Vazio recusa arquivo.
- `Analise:TentativasSaidaInvalida`: 2.
- `Analise:TimeoutModeloSegundos`: 60.
- `Analise:TimeoutConhecimentoSegundos`: 15.

Segredos ficam no ambiente, nunca no repositório.

## Fora deste plano

Ingestão da base, memória conversacional, registro de consumo, exposição A2A, gravação de caso corporativo, autenticação corporativa e layouts oficiais.

## Complexity Tracking

Nenhuma violação da constituição precisa de exceção.
