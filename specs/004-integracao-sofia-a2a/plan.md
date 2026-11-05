# Implementation Plan: Integração SOFIA por A2A

**Branch**: `004-integracao-sofia-a2a` | **Date**: 2026-11-05 | **Spec**: [spec.md](./spec.md)

## Summary

A Api.Agente publica o Agent Card e atende `message/send` em `/a2a`. O contrato de negócio é SOFIA 1.0. O protocolo é A2A JSON-RPC 0.3, implementado no ASP.NET Core já existente, sem pacote A2A de terceiros e sem projeto novo.

## Technical Context

**Language/Version**: .NET 10 com ASP.NET Core

**Primary Dependencies**: nenhuma além das já fixadas. O perfil 0.3 deste repositório segue os exemplos das skills, com JSON-RPC escrito na Api.Agente.

**Storage**: a deduplicação por `idSolicitacao` fica em memória do processo. Não entra no PostgreSQL da execução.

**Testing**: xUnit em `tests/Api.Agente.Tests`. A suíte cobre tarefa concluída, retentativa, documento sem texto, método desconhecido, 401, Agent Card e CORS.

**Constraints**: Não inventar achado fiscal. Não gravar segredo. Não habilitar CORS sem origem configurada. Não substituir `/v1/analises`.

## Constitution Check

- A DP-15 fica restrita ao card e a `message/send`.
- A DP-09 impede achar divergência no texto livre da SOFIA.
- A DP-02 impede persistir nome e documento.
- A DP-16 impede criar o orquestrador como terceiro projeto.

## Fluxo

1. A SOFIA lê o Agent Card e chama a `url`.
2. `/a2a` exige Bearer.
3. O TextPart é validado como SOFIA 1.0.
4. A tarefa concluída devolve a análise fixa de ausência de achado e a lista dos `idDocumento` em `observacoes`.
5. Erro de JSON-RPC volta no campo `error`, com HTTP 200. Falha de autenticação volta 401.

## Configuração

- `Sofia:Chave`: vazia no repositório. O ambiente usa `Sofia__Chave`.
- `Sofia:IdAgente`: `agente-fiscal`.
- `Sofia:Nome`: `Agente Fiscal INFIS`.
- `Sofia:Url`: vazia usa o host da requisição mais `/a2a`.
- `Sofia:TamanhoMaximoBytes`: vazio.
- `Cors:AllowedOrigins`: lista vazia.
