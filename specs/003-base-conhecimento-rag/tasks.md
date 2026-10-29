# Tasks: Base de conhecimento da Api.RAG

**Input**: `specs/003-base-conhecimento-rag/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/rag-v1.md

**Tests**: Incluídos. O teste da história fica vermelho antes da implementação correspondente. A suíte não chama OpenAI nem PostgreSQL real.

**Organization**: Tarefas por história. A documentação Swagger segue o mesmo critério da Api.Agente: documento `v1` na versão 1.0.0, descrições objetivas e YAML gerado de `/openapi/v1.json`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode andar em paralelo porque mexe em arquivos diferentes e não depende de tarefa incompleta da mesma fase
- **[Story]**: História da specification (`US1` a `US4`)

## Phase 1: Setup

**Purpose**: Criar a Api.RAG sem regra fiscal e sem número de busca

- [ ] T001 Incluir `src/Api.RAG/Api.RAG.csproj` em `Agente.slnx`, .NET 10, com as pastas `Endpoints`, `Application`, `Domain` e `Infrastructure`, sem referência de projeto à Api.Agente
- [ ] T002 Criar `tests/Api.RAG.Tests/Api.RAG.Tests.csproj` com xUnit e as pastas `unit`, `contract` e `integration`
- [ ] T003 [P] Fixar `Pgvector` 0.3.2, `ModelContextProtocol.AspNetCore` 2.3.0 e `PdfPig` 0.1.16 em `Directory.Packages.props`
- [ ] T004 [P] Criar as opções de ingestão e embedding e preencher `appsettings.json`, `appsettings.Development.json`, `appsettings.Testing.json` e `appsettings.Production.json` com chave vazia, limite de arquivo vazio, parâmetros de busca vazios, `Embedding:Tentativas` 2 e `Embedding:TimeoutSegundos` 60
- [ ] T005 [P] Criar `src/Api.RAG/Properties/launchSettings.json` com os perfis Development em `http://localhost:5120`, Testing em `http://localhost:5121` e Production em `http://localhost:5122`, todos abrindo `swagger`

---

## Phase 2: Fundação

**Purpose**: Persistência, validação e o documento Swagger vazio de regra fiscal

- [ ] T006 Criar documento, chunk e caso confirmado em `src/Api.RAG/Domain`, conforme `data-model.md`, com `DescriptionAttribute` em cada tipo e propriedade públicos
- [ ] T007 Criar a migração idempotente sem coluna `vector`, o repositório PostgreSQL e o repositório em memória usado em Testing e quando `ConnectionStrings:Rag` estiver vazia
- [ ] T008 Implementar o validador que recusa chave ausente, campo obrigatório ausente, formato fora de texto, PDF e XML de notas, arquivo sem limite configurado e conteúdo diferente na mesma versão
- [ ] T009 Publicar o OpenAPI `v1` em 1.0.0, com título, descrição, servidor `/`, descrição de operação e de resposta, descrições de schema a partir de `DescriptionAttribute`, interface em `/swagger` apontando para `/openapi/v1.json` e redirecionamento da raiz para `/swagger` fora do documento

**Checkpoint**: O projeto sobe no Swagger. Nenhuma história grava documento ainda.

---

## Phase 3: User Story 1 - Ingerir um documento versionado (Priority: P1)

**Goal**: Texto com origem, identificação, versão e vigência fica gravado, repetição do mesmo hash não cria chunk e conteúdo diferente na mesma versão é recusado.

**Independent Test**: Ingerir um texto, lê-lo na administração, repetir o mesmo conteúdo e receber `ignorado`.

- [ ] T010 [P] [US1] Escrever o teste que falha em `tests/Api.RAG.Tests/integration/IngestaoTextoTests.cs` para gravação, hash repetido e conteúdo diferente na mesma versão
- [ ] T011 [US1] Implementar `POST /v1/documentos` e `GET /v1/documentos/{documentoId}` com hash SHA-256, fonte `origem/identificacao/versao` e um único chunk quando `Embedding:TamanhoChunk` estiver vazio

---

## Phase 4: User Story 2 - Consultar só a vigência pedida (Priority: P1)

**Goal**: A ferramenta devolve evidência da data pedida, separa versões e falha quando a busca não está configurada.

**Independent Test**: Duas versões com intervalos diferentes. A data de uma não devolve a fonte da outra. Sem parâmetro de busca, a ferramenta volta erro.

- [ ] T012 [P] [US2] Escrever o teste que falha em `tests/Api.RAG.Tests/integration/ConsultaVigenciaTests.cs` para vigência isolada, lista vazia de busca concluída e erro quando modelo, dimensão, topK ou limiar estiverem vazios
- [ ] T013 [US2] Expor `consultar_conhecimento` em `/mcp`, com `texto` e `vigencia`, evidência na data pedida e sem a faixa de caso corporativo

---

## Phase 5: User Story 3 - Guardar o caso confirmado (Priority: P2)

**Goal**: O caso confirmado entra na administração e não sai na ferramenta MCP.

**Independent Test**: Gravar o caso da empresa A, lê-lo no GET e confirmar que a ferramenta não o lista.

- [ ] T014 [P] [US3] Escrever o teste que falha em `tests/Api.RAG.Tests/integration/CasoConfirmadoTests.cs` para vínculo completo, campo ausente e ausência do caso na ferramenta
- [ ] T015 [US3] Exigir decisão, papel, data, empresa e execução de origem na faixa `casoCorporativoConfirmado`, e listar versões em `GET /v1/documentos`

---

## Phase 6: User Story 4 - Recusar o que não tem texto (Priority: P2)

**Goal**: PDF e XML de notas viram texto. Arquivo ilegível, formato estranho ou arquivo sem limite são recusados.

**Independent Test**: Tipo fora da lista recebe recusa. Com limite vazio, PDF não grava chunk.

- [ ] T016 [P] [US4] Escrever o teste que falha em `tests/Api.RAG.Tests/integration/FormatoDocumentoTests.cs` para formato recusado, arquivo sem texto e arquivo sem `Ingestao:TamanhoMaximoBytes`
- [ ] T017 [US4] Extrair texto de PDF com PdfPig e texto de XML de notas com a biblioteca padrão, sem XSD e sem leitura de NF-e completa

---

## Phase 7: Swagger versionado

**Purpose**: A documentação publicada fica completa e não diverge do arquivo do repositório

- [ ] T018 Completar descrição objetiva de cada operação, resposta e propriedade ainda sem texto, incluindo a frase de que a consulta da análise é a ferramenta MCP e não uma rota REST
- [ ] T019 Escrever o teste de contrato que confere versão 1.0.0, caminhos `/v1/documentos` e `/v1/documentos/{documentoId}`, ausência de rota MCP no OpenAPI, `/swagger/index.js` apontando para `/openapi/v1.json` e a raiz redirecionando para `/swagger`
- [ ] T020 Gerar `specs/003-base-conhecimento-rag/contracts/rag-v1.openapi.yaml` a partir de `/openapi/v1.json` e travar o teste para igualdade, sem regravar o arquivo na suíte
- [ ] T021 Atualizar `quickstart.md` com o comando da suíte e o endereço do documento publicado

**Checkpoint**: `dotnet test tests/Api.RAG.Tests/Api.RAG.Tests.csproj` passa. O YAML confere com o Swagger.

---

## Dependencies

- A Phase 2 depende da Phase 1
- US1 depende da Phase 2
- US2 depende de US1, porque a consulta lê documento gravado
- US3 depende de US1 e da exclusão de faixa feita em US2
- US4 depende do POST da US1
- A Phase 7 fecha depois das rotas existirem

## Parallel opportunities

- T003, T004 e T005 podem andar juntas depois de T001
- Os testes T010, T012, T014 e T016 podem ser escritos em paralelo, cada um no próprio arquivo
- T018 pode começar assim que a rota da história existir, e fecha em T020

## Notes

- Não escolher modelo, dimensão, topK, limiar, métrica nem índice vetorial
- Não criar coluna `vector(N)` nesta lista
- Não devolver caso corporativo em `consultar_conhecimento`
- Chave e connection string permanecem vazias nos arquivos versionados
- Dado de teste não vale como legislação
