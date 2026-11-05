# Specification: Integração SOFIA por A2A

**Feature Branch**: `004-integracao-sofia-a2a`  
**Created**: 2026-11-05  
**Status**: Especificada  
**Input**: Skills `sofia-a2a-contrato`, `sofia-a2a-implementacao` e `sofia-cors-rest-a2a`.

## Escopo

A Api.Agente é o agente especializado. A SOFIA é o orquestrador e não mora neste repositório. A análise REST das specifications 001 e 002 permanece. Esta specification acrescenta o perfil A2A JSON-RPC 0.3 e o contrato de negócio SOFIA 1.0.

## Requisitos

- **FR-001**: O Agent Card fica em `/.well-known/agent-card.json`, com a skill `analisar_documentos` e `url` apontando para `/a2a`.
- **FR-002**: `POST /a2a` aceita somente JSON-RPC 2.0 `message/send`, com `role=user`, `messageId` e um TextPart. O texto é `SolicitacaoAnaliseSofia`.
- **FR-003**: A tarefa `completed` devolve `ResultadoAnaliseSofia` no artefato `analise-sofia-v1`, com os mesmos `idExecucao` e `idSolicitacao`.
- **FR-004**: `versaoContrato` 1.0, UUID, `usuario.nome`, `pergunta` e documentos com `idDocumento`, `nome` e `texto` são obrigatórios. `tipo` e `contexto` são opcionais.
- **FR-005**: O texto do documento e `contexto.instrucoes` são dado. Não produzem achado nem alteram a análise.
- **FR-006**: A mesma `idSolicitacao` reutiliza a tarefa já respondida. A memória não grava o nome do usuário nem o texto do documento.
- **FR-007**: Sem `Authorization: Bearer` igual a `Sofia:Chave`, `/a2a` responde 401. Chave vazia recusa. O Agent Card permanece público.
- **FR-008**: `Cors:AllowedOrigins` vazio não habilita CORS. Com origens, a política `SofiaBrowser` vale para o card e para `/a2a`, nos métodos GET, POST e OPTIONS e nos cabeçalhos `Content-Type` e `Authorization`.
- **FR-009**: Quando `Sofia:TamanhoMaximoBytes` está preenchido, texto maior é recusado sem truncar. Sem o valor, a specification não inventa limite.

## Fora deste escopo

Cliente A2A, registro de agentes, despacho para outros agentes e síntese da SOFIA. Eles pertencem ao orquestrador. Docker Compose novo, JWT e catálogo de papéis continuam fora.
