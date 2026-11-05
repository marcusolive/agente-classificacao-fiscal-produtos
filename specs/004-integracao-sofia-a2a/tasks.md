# Tasks: Integração SOFIA por A2A

**Input**: `specs/004-integracao-sofia-a2a/`

- [x] T01 Fixar A2A JSON-RPC 0.3 e o contrato SOFIA 1.0 no ADR 0007 e na constituição 1.0.7
- [x] T02 Publicar `/.well-known/agent-card.json` com a skill `analisar_documentos`
- [x] T03 Validar `SolicitacaoAnaliseSofia` e devolver `ResultadoAnaliseSofia` sem achado inventado
- [x] T04 Receber `message/send` em `/a2a` e reutilizar a tarefa da mesma `idSolicitacao`
- [x] T05 Exigir Bearer em `/a2a` e manter o card público
- [x] T06 Aplicar `SofiaBrowser` só quando houver origem configurada
- [x] T07 Cobrir o fluxo na suíte da Api.Agente e publicar as rotas no Swagger v1

Fora desta lista: cliente A2A, registry, despacho para outros agentes e Compose da SOFIA.
