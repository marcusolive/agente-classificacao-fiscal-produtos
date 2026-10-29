# Tasks: Confirmação do caso corporativo

**Input**: `specs/002-confirmacao-caso-corporativo/`

## Phase 1: Contrato

- [x] T001 Publicar `POST /v1/analises/{execucaoId}/confirmacoes` no documento v1, com a confirmação em 1.1.0 e a análise em 1.0.0
- [x] T002 Validar papel obrigatório, execução existente e item pertencente à execução
- [x] T003 Aceitar somente item com NCM sustentada e corrigir somente com valor presente nas evidências
- [x] T004 Devolver a empresa da execução, `gravado` falso e a validação de gravação não realizada, sem alterar a execução
- [x] T005 Cobrir aceite, correção sustentada, código ausente, papel ausente e execução inexistente em `tests/Api.Agente.Tests/integration/ConfirmacaoCasoTests.cs`
- [x] T006 Regenerar `specs/001-execucao-analise-fiscal/contracts/analise-v1.openapi.yaml` a partir de `/openapi/v1.json`
