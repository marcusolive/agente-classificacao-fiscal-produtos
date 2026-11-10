# Tasks: Execução da análise fiscal de itens

**Input**: Design documents from `/specs/001-execucao-analise-fiscal/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/analise-v1.openapi.yaml

**Tests**: Incluídos. O plano pede xUnit, teste de contrato e teste de integração com dublê. Escrever o teste da história e vê-lo falhar antes da implementação correspondente.

**Organization**: Tarefas agrupadas por história para permitir implementação e verificação independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode andar em paralelo porque mexe em arquivos diferentes e não depende de tarefa incompleta da mesma fase
- **[Story]**: História da specification (`US1` a `US5`)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Criar a solução da Api.Agente sem regra fiscal

- [x] T001 Criar `Agente.sln`, o projeto `src/Api.Agente/Api.Agente.csproj` em .NET 10 e as pastas `Endpoints`, `Application`, `Domain` e `Infrastructure`
- [x] T002 Criar `tests/Api.Agente.Tests/Api.Agente.Tests.csproj` com xUnit e as pastas `unit`, `contract` e `integration`
- [x] T003 [P] Fixar as versões de `Microsoft.SemanticKernel`, `Microsoft.SemanticKernel.Connectors.OpenAI`, `ModelContextProtocol` e `Npgsql` em `Directory.Packages.props`, registrando os números escolhidos nesse arquivo
- [x] T004 [P] Criar `src/Api.Agente/Infrastructure/OpcoesAnalise.cs` com `OpenAI:Model`, `Analise:TamanhoMaximoBytes`, `Analise:TentativasSaidaInvalida`, `Analise:TimeoutModeloSegundos` e `Analise:TimeoutConhecimentoSegundos`, sem segredo no repositório

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Base usada por todas as histórias

**⚠️ CRITICAL**: Nenhuma história começa antes desta fase

- [x] T005 Criar as entidades de `specs/001-execucao-analise-fiscal/data-model.md` em `src/Api.Agente/Domain/Execucao.cs`, `ItemAnalise.cs`, `Evidencia.cs`, `TratamentoSugerido.cs` e `Achado.cs`
- [x] T006 [P] Escrever o teste que falha em `tests/Api.Agente.Tests/unit/ConfiancaPorFaixaTests.cs` para as notas 90, 80, 65 e 40 e para confiança desconhecida sem gravar zero
- [x] T007 Implementar `src/Api.Agente/Domain/ConfiancaPorFaixa.cs` até T006 passar, conforme `docs/adr/0002-confianca-por-faixa-de-autoridade.md`
- [x] T008 [P] Escrever o teste que falha em `tests/Api.Agente.Tests/unit/OrdenacaoEvidenciasTests.cs` para a ordem legislação, notas e regras, caso confirmado e texto do fornecedor, incluindo empate
- [x] T009 Implementar `src/Api.Agente/Domain/OrdenacaoDeEvidencias.cs` até T008 passar
- [x] T010 [P] Escrever o teste que falha em `tests/Api.Agente.Tests/unit/EstadoNotaTests.cs` para a precedência bloqueada, pendente de dado, falha técnica e sugestão liberada
- [x] T011 Implementar `src/Api.Agente/Domain/CalculadoraEstadoNota.cs` até T010 passar
- [x] T012 Criar a porta `src/Api.Agente/Application/IConsultaConhecimento.cs` e o dublê `tests/Api.Agente.Tests/integration/DubleConsultaConhecimento.cs`, sem referência ao banco da Api.RAG
- [x] T013 Criar `src/Api.Agente/Infrastructure/PostgresRepositorioExecucao.cs` e a migração das tabelas `execucao`, `item_analise`, `evidencia_citada` e `achado` no PostgreSQL da Api.Agente
- [x] T014 Implementar `src/Api.Agente/Application/ValidadorEntrada.cs` para rejeitar contexto incompleto, tipo fora da lista e arquivo quando `Analise:TamanhoMaximoBytes` estiver vazio ou for excedido
- [x] T015 Implementar `src/Api.Agente/Application/ValidadorSaidaModelo.cs` para recusar código, alíquota, dispositivo ou referência ausente das evidências da vigência
- [x] T016 Expor `POST /v1/analises` e `GET /v1/analises/{execucaoId}` em `src/Api.Agente/Endpoints/AnalisesEndpoints.cs`, devolvendo o contrato `specs/001-execucao-analise-fiscal/contracts/analise-v1.openapi.yaml`

**Checkpoint**: Fundação pronta. As histórias podem seguir.

---

## Phase 3: User Story 1 - Sugerir classificação e tratamento do item (Priority: P1) 🎯 MVP

**Goal**: Descrição com dado essencial devolve NCM sustentada, tratamento com evidência, confiança 90 quando a fonte vencedora é legislação e XML vazio enquanto não houver layout registrado.

**Independent Test**: Enviar texto com composição, função e apresentação e uma legislação no dublê. O resultado traz a NCM dessa legislação, a fonte e o trecho. Tributo sem evidência volta como limitação.

### Tests for User Story 1

- [x] T017 [P] [US1] Escrever o teste de contrato que falha em `tests/Api.Agente.Tests/contract/AnaliseContratoTests.cs` para o envelope versão 1.0.0
- [x] T018 [P] [US1] Escrever o teste de integração que falha em `tests/Api.Agente.Tests/integration/SugestaoTextoTests.cs` cobrindo sugestão por legislação, conflito com texto de fornecedor, ausência de evidência e tributo sem evidência

### Implementation for User Story 1

- [x] T019 [US1] Implementar `src/Api.Agente/Application/AvaliadorDadosEssenciais.cs` para composição, função e apresentação
- [x] T020 [US1] Implementar `src/Api.Agente/Application/NormalizadorProduto.cs` preservando o texto original
- [x] T021 [US1] Implementar `src/Api.Agente/Application/AnalisarExecucao.cs` para a intenção `sugerirNcm`, usando somente candidatos recuperados
- [x] T022 [US1] Implementar `src/Api.Agente/Infrastructure/OpenAiAnalisador.cs` com saída estruturada, no máximo duas novas tentativas e tempo limite configurado
- [x] T023 [US1] Implementar `src/Api.Agente/Application/MontadorEnvelope.cs` preenchendo identificador, versões e estado pelo sistema, com `xmlItem` vazio e validação não realizada quando não houver layout de item
- [x] T024 [US1] Ligar o adaptador MCP em `src/Api.Agente/Infrastructure/McpConsultaConhecimento.cs` à porta da T012, tratando indisponibilidade como falha técnica

**Checkpoint**: A história 1 funciona sozinha com o dublê de conhecimento.

---

## Phase 4: User Story 2 - Auditar o declarado e bloquear a nota (Priority: P1)

**Goal**: Item cuja NCM ou tratamento declarado não se sustenta na evidência vencedora fica divergente e a nota fica bloqueada, sem declaração de conformidade.

**Independent Test**: Dois itens na mesma execução: um contradiz a legislação e o outro coincide. Só o primeiro fica divergente e a nota fica bloqueada.

### Tests for User Story 2

- [x] T025 [US2] Escrever o teste de integração que falha em `tests/Api.Agente.Tests/integration/BloqueioNotaTests.cs` para divergência, declaração compatível e mistura de item consistente com item divergente

### Implementation for User Story 2

- [x] T026 [US2] Implementar `src/Api.Agente/Application/ComparadorDeclaracao.cs` marcando `divergente`, o que está incorreto e a classificação sustentada
- [x] T027 [US2] Usar o comparador em `src/Api.Agente/Application/AnalisarExecucao.cs` para a intenção `auditarDeclarado`, sem atestar conformidade fiscal

**Checkpoint**: As histórias 1 e 2 passam nos próprios testes.

---

## Phase 5: User Story 3 - Continuar os itens que já podem ser analisados (Priority: P2)

**Goal**: Item sem dado essencial pede o dado, não recebe NCM e não bloqueia a nota por si só. Os outros itens seguem.

**Independent Test**: Uma execução com um item completo e um incompleto. O incompleto fica pendente e o completo recebe resultado. A nota só bloqueia se existir outro item divergente.

### Tests for User Story 3

- [x] T028 [US3] Escrever o teste de integração que falha em `tests/Api.Agente.Tests/integration/ItemIncompletoTests.cs` para item isolado incompleto e para incompleto junto de divergente

### Implementation for User Story 3

- [x] T029 [US3] Encerrar só o item incompleto em `src/Api.Agente/Application/AnalisarExecucao.cs`, com situação `dadoAusente` e estado `pendenteDeDado` quando não houver divergência

**Checkpoint**: As histórias 1, 2 e 3 passam sem uma impedir a outra.

---

## Phase 6: User Story 4 - Aceitar planilha e documento técnico (Priority: P2)

**Goal**: Texto continua válido. Arquivo ilegível, vazio ou de layout não registrado produz erro estrutural e não bloqueia a nota. Nenhum parser fiscal é inventado.

**Independent Test**: Enviar PDF, planilha, laudo ou ficha sem layout registrado e receber erro estrutural. Enviar texto e continuar recebendo a sugestão da história 1.

### Tests for User Story 4

- [x] T030 [P] [US4] Escrever o teste de integração que falha em `tests/Api.Agente.Tests/integration/DocumentoSemLayoutTests.cs` para arquivo sem layout, arquivo vazio e tipo de entrada desconhecido

### Implementation for User Story 4

- [x] T031 [US4] Criar `src/Api.Agente/Application/RegistroDeLayouts.cs` vazio de layouts oficiais e usá-lo em `src/Api.Agente/Application/ValidadorEntrada.cs` para devolver `erroEstrutural` sem bloqueio fiscal
- [x] T032 [US4] Manter `xmlItem` restrito ao item em `src/Api.Agente/Application/MontadorEnvelope.cs` quando a intenção for `sugerirXmlItem`, sem montar NF-e completa

**Checkpoint**: Documento sem fonte de layout não entra na classificação.

---

## Phase 7: User Story 5 - Pedir revisão humana sem bloquear por incerteza (Priority: P3)

**Goal**: Confiança menor que 70, empate ou confiança desconhecida pedem revisão e devolvem a sugestão quando ela existir. Impacto relevante permanece falso. Esses motivos, sozinhos, não bloqueiam a nota.

**Independent Test**: Caso corporativo como melhor fonte devolve sugestão, confiança 65 e revisão. Duas legislações empatadas não escolhem vencedor. Confiança 90 não pede revisão por nota baixa.

### Tests for User Story 5

- [x] T033 [US5] Escrever o teste de integração que falha em `tests/Api.Agente.Tests/integration/RevisaoHumanaTests.cs` para nota 65, empate, nota 90 e impacto relevante falso

### Implementation for User Story 5

- [x] T034 [US5] Aplicar o marcador `revisaoHumana` em `src/Api.Agente/Application/AnalisarExecucao.cs` sem alterar o estado da nota quando não houver divergência
- [x] T035 [US5] Fixar `impactoRelevante` como falso em `src/Api.Agente/Application/MontadorEnvelope.cs` até existir definição com exemplos reais

**Checkpoint**: Todas as histórias ficam verificáveis pelo quickstart.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Fechar a fatia sem ampliar o escopo

- [x] T036 Executar os cenários de `specs/001-execucao-analise-fiscal/quickstart.md` e registrar o resultado no teste de integração já criado
- [x] T037 Revisar `src/Api.Agente` para garantir que não existe gravação de caso corporativo, acesso a vetor da Api.RAG, memória conversacional, consumo ou servidor A2A
- [x] T038 [P] Atualizar `specs/001-execucao-analise-fiscal/plan.md` com as versões exatas gravadas em `Directory.Packages.props`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: começa imediatamente
- **Foundational (Phase 2)**: depende do Setup e bloqueia todas as histórias
- **User Stories (Phase 3+)**: dependem da Phase 2
- **Polish (Phase 8)**: depende das histórias desejadas

### User Story Dependencies

- **User Story 1 (P1)**: começa após a Phase 2 e não depende de outra história
- **User Story 2 (P1)**: começa após a Phase 2 e usa o mesmo fluxo da história 1, com teste próprio
- **User Story 3 (P2)**: começa após a Phase 2 e cobre o item que a história 1 não classifica
- **User Story 4 (P2)**: começa após a Phase 2 e não implementa layout oficial
- **User Story 5 (P3)**: começa após a Phase 2 e usa a confiança da Phase 2

### Within Each User Story

- O teste da história fica vermelho antes da implementação
- Domínio e validadores vêm antes do endpoint
- A história termina quando o teste independente passa

### Parallel Opportunities

- T003 e T004 podem andar juntas depois de T001
- T006, T008 e T010 podem ser escritas em paralelo
- T017 e T018 podem ser escritas em paralelo
- Histórias diferentes só andam em paralelo depois da Phase 2 e evitando editar ao mesmo tempo `AnalisarExecucao.cs`

---

## Parallel Example: User Story 1

```text
T017 Contract test em tests/Api.Agente.Tests/contract/AnaliseContratoTests.cs
T018 Integration test em tests/Api.Agente.Tests/integration/SugestaoTextoTests.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Phase 1
2. Concluir Phase 2
3. Concluir Phase 3
4. Parar e validar a sugestão por texto com o dublê
5. Só então seguir para bloqueio, item incompleto, documento e revisão

### Incremental Delivery

Cada história acrescenta um teste independente sem apagar o comportamento da anterior. Layout oficial, ingestão, memória, consumo, A2A e caso corporativo continuam fora desta lista.

---

## Notes

- Confiança confirmada: legislação 90, notas e regras 80, caso corporativo confirmado 65, texto do fornecedor 40
- Abaixo de 70 pede revisão e mantém a sugestão sustentada
- Arquivo sem `Analise:TamanhoMaximoBytes` é recusado, sem bloqueio fiscal
- `impactoRelevante` permanece falso
- Dado de teste não vale como legislação de produção
