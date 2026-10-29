# ADR 0005 — Api.RAG no PostgreSQL próprio

- Status: aceita
- Data: 2026-10-29
- Specification: `specs/003-base-conhecimento-rag/spec.md`

## Decisão

A base de conhecimento mora no projeto Api.RAG e no PostgreSQL identificado por `ConnectionStrings:Rag`. A Api.Agente só chega nela pela ferramenta `consultar_conhecimento`, em `/mcp`. A primeira migração não cria a coluna de embedding. Essa coluna espera a dimensão avaliada na DP-12.

## Contexto

A specification 003 exige ingestão, administração e a mesma ferramenta que o agente já chama. A DP-12 proíbe fixar modelo, dimensão, topK e limiar sem exemplos. Uma lista vazia nessa ferramenta seria lida pelo agente como ausência de evidência.

## Consequências

Documento e caso confirmado podem ser gravados e lidos na administração sem vetor. A consulta MCP, sem os parâmetros de busca, falha como ferramenta indisponível. Caso corporativo não sai nessa ferramenta. Nenhum dos dois projetos referencia o outro.
