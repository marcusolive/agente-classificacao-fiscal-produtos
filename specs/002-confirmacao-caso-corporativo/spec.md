# Feature Specification: Confirmação do caso corporativo

**Feature Branch**: `002-confirmacao-caso-corporativo`

**Created**: 2026-10-29

**Status**: Draft

**Input**: Confirmar ou corrigir o resultado de uma execução já analisada, sem gravar o caso como precedente. A gravação fica para a specification da Api.RAG.

## Clarifications

### Session 2026-10-29

- Q: Quem pode confirmar ou corrigir? → A: O papel informado na própria confirmação. Não há catálogo fixo de cargos nesta specification.
- Q: Onde o caso fica gravado? → A: Em lugar nenhum nesta specification. O contrato de confirmação é publicado agora. A gravação espera a Api.RAG.
- Q: O que o revisor pode alterar? → A: Aceitar a sugestão sustentada ou corrigir NCM e tratamento, desde que o código esteja nas evidências daquela execução.
- Q: O caso de uma empresa sustenta outra? → A: Não. O caso fica vinculado à empresa da execução de origem.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Aceitar a sugestão sustentada (Priority: P1)

Um revisor informa o papel dele e aceita a NCM já sustentada na execução. A resposta devolve essa confirmação, a empresa da execução e a informação de que o caso não foi gravado.

**Why this priority**: Sem o aceite explícito, a etapa 25 da constituição não tem o que validar.

**Independent Test**: Confirmar o aceite de um item com NCM sustentada. A resposta traz essa NCM, o papel informado e `gravado` falso. A execução consultada depois permanece a mesma.

**Acceptance Scenarios**:

1. **Given** uma execução com item sugerido e papel informado, **When** o revisor aceita esse item, **Then** a resposta traz a NCM sustentada, a empresa da execução, o papel informado e declara que a gravação na base de conhecimento não foi realizada.
2. **Given** a confirmação foi aceita, **When** a mesma execução é consultada de novo, **Then** o resultado da análise não ganha caso corporativo nem muda a NCM sugerida.
3. **Given** a confirmação não informa papel, **When** ela é enviada, **Then** é recusada antes de qualquer confirmação.

### User Story 2 - Corrigir somente com código já evidenciado (Priority: P1)

O revisor troca a NCM ou o tratamento por outro valor que já aparece nas evidências do item. Valor ausente dessas evidências é recusado.

**Why this priority**: A correção não pode inventar código. A constituição só admite o que a execução já recuperou.

**Independent Test**: Corrigir para uma NCM presente nas evidências e receber essa NCM na confirmação. Corrigir para um código ausente e receber recusa.

**Acceptance Scenarios**:

1. **Given** o item tem duas NCM nas evidências, **When** o revisor corrige para a que não foi a vencedora, **Then** a confirmação devolve essa NCM e não grava o caso.
2. **Given** o código informado não está nas evidências do item, **When** o revisor corrige, **Then** a confirmação é recusada e a execução não muda.
3. **Given** o item não tem NCM sustentada, **When** o revisor aceita esse item, **Then** a confirmação é recusada.

### User Story 3 - Manter o caso na empresa de origem (Priority: P2)

A confirmação não recebe uma empresa diferente. A empresa devolvida é a da execução.

**Why this priority**: A regra de mesma empresa precisa estar no contrato antes de qualquer recuperação futura.

**Independent Test**: Confirmar uma execução da empresa A. A resposta cita a empresa A e não oferece campo para trocá-la.

**Acceptance Scenarios**:

1. **Given** a execução pertence a uma empresa, **When** a confirmação é aceita, **Then** a resposta repete essa empresa.
2. **Given** a confirmação foi produzida, **When** outra análise é executada, **Then** ela não recebe esse caso como evidência.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A confirmação DEVE informar o papel da autoridade. Sem esse papel, a Api.Agente DEVE recusar a confirmação.
- **FR-002**: A confirmação DEVE referir uma execução existente e itens dessa execução. Execução ausente DEVE responder como não encontrada. Item ausente DEVE ser recusado.
- **FR-003**: Aceitar um item DEVE usar a NCM já sustentada nessa execução. Item sem NCM sustentada NÃO DEVE ser aceito.
- **FR-004**: Corrigir NCM ou tratamento DEVE usar somente valor presente nas evidências daquele item. Código ou tratamento ausente DEVE ser recusado.
- **FR-005**: A resposta DEVE vincular a confirmação à empresa da execução de origem. A confirmação NÃO DEVE aceitar outra empresa.
- **FR-006**: Esta specification NÃO DEVE gravar, indexar ou recuperar o caso como precedente. A resposta DEVE declarar que a gravação na base de conhecimento não foi realizada.
- **FR-007**: A confirmação NÃO DEVE alterar a execução já gravada e NÃO DEVE atestar conformidade fiscal.
- **FR-008**: O contrato DEVE permanecer na versão 1 do documento, com a confirmação identificada como 1.1.0. O envelope da análise permanece 1.0.0.

### Key Entities

- **Pedido de confirmação**: Papel da autoridade e a decisão de cada item, aceitar ou corrigir.
- **Item confirmado**: Item da execução, decisão, NCM confirmada e tratamentos sustentados ou corrigidos dentro das evidências.
- **Resultado da confirmação**: Versão 1.1.0, execução, empresa, papel, itens confirmados e a declaração de que nada foi gravado na base.

## Success Criteria *(mandatory)*

- **SC-001**: Em 100% dos aceites com NCM sustentada, a resposta devolve essa NCM, o papel informado e `gravado` falso.
- **SC-002**: Em 100% das correções com código ausente das evidências do item, a confirmação é recusada e a execução permanece igual.
- **SC-003**: Nenhuma confirmação desta specification passa a ser evidência de uma análise seguinte.

## Assumptions

- A lista de cargos e a autenticação de quem informa o papel ficam para a specification de segurança.
- A publicação do caso na base, a recuperação entre análises e o armazenamento vetorial ficam para a specification da Api.RAG.
- Tratamento omitido na correção mantém os tratamentos já sustentados no item.

## Fora de escopo

- Gravar precedente corporativo.
- Criar a Api.RAG ou escrever no banco dela.
- Inventar NCM, alíquota ou tratamento.
- Definir catálogo de papéis ou regra de autenticação.
