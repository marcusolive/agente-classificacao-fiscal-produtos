# ADR 0001 — Execução síncrona e PostgreSQL da Api.Agente

- Status: aceita
- Data: 2026-10-29
- Specification: `specs/001-execucao-analise-fiscal/spec.md`

## Decisão

A análise responde na mesma solicitação e o resultado fica recuperável pelo identificador da execução. A Api.Agente persiste execução, itens e evidências citadas no próprio PostgreSQL.

## Contexto

A specification exige o resultado completo por identificador e deixa o canal para o plano técnico. Memória conversacional, consumo e vetores da base de conhecimento estão fora desta fatia.

## Consequências

A Api.Agente não acessa o banco da Api.RAG. Reiniciar o processo não apaga o resultado. Consulta posterior usa o mesmo contrato da resposta inicial.
