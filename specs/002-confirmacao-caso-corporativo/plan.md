# Implementation Plan: Confirmação do caso corporativo

**Branch**: `002-confirmacao-caso-corporativo` | **Date**: 2026-10-29 | **Spec**: [spec.md](./spec.md)

## Summary

A Api.Agente recebe a confirmação de uma execução já gravada, valida o papel informado e a decisão de cada item, e devolve o contrato 1.1.0. Aceite usa a NCM sustentada. Correção só usa valor presente nas evidências do item. Nada é gravado como caso corporativo.

## Technical Context

**Language/Version**: .NET 10 com ASP.NET Core

**Primary Dependencies**: As mesmas da specification 001. Nenhum pacote novo.

**Storage**: Nenhuma tabela nova. A execução já gravada é apenas lida.

**Testing**: xUnit, no mesmo projeto da Api.Agente, com o dublê de conhecimento já usado na análise.

**Constraints**: Não gravar precedente. Não acessar a Api.RAG. Não inventar código ausente das evidências. Não criar catálogo de papéis.

## Constitution Check

- A etapa 25 não grava caso. A resposta explicita a validação não realizada.
- A fronteira com a Api.RAG permanece: não há escrita em vetor nem em banco de outro serviço.
- O envelope da análise continua 1.0.0. A confirmação entra no mesmo documento v1 como 1.1.0.

## Design

`POST /v1/analises/{execucaoId}/confirmacoes` lê a execução, valida o pedido e devolve o resultado. Não há repositório de caso. A empresa da resposta é a da execução. O documento OpenAPI publicado é regenerado a partir de `/openapi/v1.json`.
