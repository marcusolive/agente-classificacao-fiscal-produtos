# ADR 0003 — Conhecimento por porta MCP

- Status: aceita
- Data: 2026-10-29
- Specification: `specs/001-execucao-analise-fiscal/spec.md`

## Decisão

A Api.Agente consulta conhecimento somente por uma porta de aplicação implementada com cliente MCP. Os testes desta fatia usam um dublê em memória. O projeto Api.RAG e a ingestão ficam para a specification da base.

## Contexto

A constituição exige MCP e proíbe acesso ao banco vetorial. A specification 001 não administra a base.

## Consequências

A análise é testável sem PostgreSQL vetorial. Indisponibilidade da porta é falha técnica do item, não bloqueio fiscal. Conteúdo recuperado entra como dado, nunca como instrução.
