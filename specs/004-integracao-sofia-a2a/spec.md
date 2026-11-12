# Specification: Integração SOFIA por A2A

**Feature Branch**: `004-integracao-sofia-a2a`  
**Created**: 2026-11-05  
**Status**: Especificada  
**Input**: Skills `sofia-a2a-contrato`, `sofia-a2a-implementacao` e `sofia-cors-rest-a2a`.

## Escopo

A Api.Agente é o agente especializado. A SOFIA é o orquestrador e não mora neste repositório. A análise REST das specifications 001 e 002 permanece. Esta specification acrescenta o perfil A2A JSON-RPC 0.3 e o contrato de negócio SOFIA 1.0.

## Requisitos

- **FR-001**: O Agent Card fica em `/.well-known/agent-card.json`, com `protocolVersion` 0.3.0, `preferredTransport` JSONRPC, esquema `bearer` e a skill `analisar_documentos`. A `url` aponta para `/a2a`.
- **FR-002**: `POST /a2a` aceita JSON-RPC 2.0 `message/send`, `tasks/get` e `tasks/cancel`; outro método responde erro `-32601`. `message/send` exige `role=user`, `messageId` e um TextPart, com o texto `SolicitacaoAnaliseSofia`. `tasks/get` e `tasks/cancel` recebem `params.id` da tarefa: `tasks/get` devolve a tarefa concluída (`-32001` se não existir) e `tasks/cancel` responde `-32002` para tarefa concluída (`-32001` se não existir), pois o processamento é síncrono.
- **FR-003**: A tarefa tem `kind` `task` e estado `completed`. O artefato `analise-sofia-v1` devolve `ResultadoAnaliseSofia`, com os mesmos `idExecucao` e `idSolicitacao`. `contextId` repete o `idExecucao`.
- **FR-004**: `versaoContrato` 1.0, UUID, `usuario.nome`, `pergunta` e documentos com `idDocumento`, `nome` e `texto` são obrigatórios. `tipo` e `contexto` são opcionais.
- **FR-005**: O texto do documento e `contexto.instrucoes` são dado. Não produzem achado nem alteram a análise.
- **FR-010**: No contrato real da SOFIA o `contexto` traz apenas `idioma` e `instrucoes`. Os campos de negócio `empresa`, `uf`, `regimeTributario`, `dataVigencia`, `papel`, `finalidade` e `tipoOperacao` são OPCIONAIS: quando a SOFIA os informa, prevalecem; quando não (o caso usual), o agente usa defaults e classifica mesmo assim, com a vigência igual à data corrente. A intenção executada é somente sugerir NCM. O agente NÃO DEVE recusar a solicitação por falta desses campos.
- **FR-011**: O `texto` de cada documento é o texto extraído que a SOFIA envia, classificado diretamente, sem exigir o formato de atributos (função, composição, apresentação). NÃO é um JSON de `/v1/analises`. Quando o texto é uma **NF-e** (XML, reconhecido por marcadores como `infNFe`/`det`/`prod`), o agente lê os itens e **audita a NCM declarada** de cada um (`auditarDeclarado`): declaração que não se sustenta na evidência vira achado e bloqueia a nota. Caso contrário, trata o texto como descrição de produto e **sugere** a NCM. As salvaguardas continuam valendo: ordem de autoridade, empate resolvido só entre os candidatos com revisão humana e ausência de evidência declarada como limitação. Falha técnica deixa a tarefa com status erro.
- **FR-006**: A mesma `idSolicitacao` reutiliza a tarefa já respondida. A memória não grava o nome do usuário nem o texto do documento.
- **FR-007**: O Agent Card e `/a2a` exigem `Authorization: Bearer` igual ao segredo configurado; sem ele respondem 401 com `WWW-Authenticate: Bearer`. O segredo vem de `SOFIA_A2A_TOKEN` (prevalece sobre `Sofia:Chave`), comparado em tempo constante e sem normalizar caixa. Segredo vazio recusa (fail-closed). O segredo não entra em código nem em arquivo versionado.
- **FR-008**: `Cors:AllowedOrigins` vazio não habilita CORS. Com origens, a política `SofiaBrowser` vale para o card e para `/a2a`, nos métodos GET, POST e OPTIONS e nos cabeçalhos `Content-Type` e `Authorization`.
- **FR-009**: Quando `Sofia:TamanhoMaximoBytes` está preenchido, texto maior é recusado sem truncar. Sem o valor, a specification não inventa limite.

## Fora deste escopo

Cliente A2A, registro de agentes, despacho para outros agentes e síntese da SOFIA. Eles pertencem ao orquestrador. Docker Compose novo, JWT e catálogo de papéis continuam fora.
