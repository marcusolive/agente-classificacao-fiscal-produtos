# Feature Specification: Base de conhecimento da Api.RAG

**Feature Branch**: `003-base-conhecimento-rag`

**Created**: 2026-10-29

**Status**: Draft

**Input**: A Api.RAG ingere documentos da base, guarda chunks e embeddings no próprio PostgreSQL e devolve evidência à Api.Agente só pela ferramenta MCP já chamada hoje. O caso corporativo confirmado entra como documento. O caso não confirmado não vira precedente.

## Clarifications

### Session 2026-10-29

- Q: O que entra nesta specification? → A: Ingestão, administração REST, consulta MCP e o caso corporativo confirmado como documento recuperável na administração.
- Q: Uma versão nova apaga a anterior? → A: Não. As versões convivem. A consulta por uma data não mistura chunks de versões diferentes como se fossem a mesma vigência.
- Q: O mesmo documento pode ser enviado de novo? → A: Conteúdo igual, na mesma origem, identificação e versão, é ignorado. Conteúdo diferente na mesma versão é recusado e exige versão nova.
- Q: A ferramenta MCP muda? → A: Não. Continua `consultar_conhecimento`, com texto e vigência. A evidência traz fonte, faixa, trecho, localização, vigência e, quando tiverem sido informados na ingestão, NCM e tributos.
- Q: Quem ingere? → A: Uma chave obrigatória em configuração. Chave vazia recusa a ingestão. Não há catálogo de papéis.
- Q: Quais formatos? → A: Texto puro, PDF e XML de notas. Nenhum schema oficial é definido aqui.
- Q: A consulta atual pode devolver caso de outra empresa? → A: Não. A ferramenta não recebe empresa. O caso confirmado é gravado e lido na administração. A ferramenta MCP não devolve caso corporativo.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ingerir um documento versionado (Priority: P1)

Quem administra a base envia um documento com origem, identificação, versão, faixa de autoridade e intervalo de vigência. A Api.RAG guarda esse documento e os chunks dessa versão. O conteúdo do arquivo permanece dado, não instrução.

**Why this priority**: Sem documento versionado, a consulta não tem fonte recuperável.

**Independent Test**: Ingerir um texto com origem, identificação, versão e vigência. Consultar a administração e encontrar essa versão, a fonte e o trecho. Enviar de novo o mesmo conteúdo e receber indicação de que nada novo foi gravado.

**Acceptance Scenarios**:

1. **Given** origem, identificação, versão, faixa e vigência inicial, **When** o documento de texto é ingerido com a chave configurada, **Then** a versão fica gravada, cada chunk aponta para essa fonte e a versão pode ser lida na administração.
2. **Given** a mesma origem, identificação, versão e o mesmo conteúdo, **When** a ingestão é repetida, **Then** nenhum chunk novo é criado.
3. **Given** a mesma origem, identificação e versão com conteúdo diferente, **When** a ingestão é enviada, **Then** ela é recusada e a versão já gravada permanece.
4. **Given** a chave de ingestão não está configurada ou não confere, **When** a ingestão é enviada, **Then** ela é recusada antes de gravar.

### User Story 2 - Consultar só a vigência pedida (Priority: P1)

A Api.Agente chama `consultar_conhecimento` com o texto do item e a data de vigência. A Api.RAG devolve evidências cujo intervalo contém essa data. Cada evidência usa a data pedida e identifica a própria versão. Versões diferentes não são fundidas.

**Why this priority**: A análise já filtra a evidência pela data recebida. Uma evidência de outra data seria descartada pelo agente.

**Independent Test**: Duas versões do mesmo documento, com intervalos diferentes. Consultar uma data coberta só pela primeira versão devolve apenas a fonte dessa versão. Consultar uma data fora dos dois intervalos devolve lista vazia.

**Acceptance Scenarios**:

1. **Given** uma versão vigente na data pedida, **When** a ferramenta é chamada com texto e essa data, **Then** a evidência traz fonte, faixa, trecho, localização quando existir, a data pedida e a versão da fonte.
2. **Given** duas versões cobrem a mesma data, **When** a consulta ocorre, **Then** as duas voltam separadas, cada uma com a própria fonte, sem serem tratadas como uma só vigência.
3. **Given** nenhuma versão cobre a data, **When** a consulta ocorre, **Then** a resposta é uma lista vazia, sem evidência inventada.
4. **Given** o provedor de embeddings ou o banco da Api.RAG falha na consulta, **When** a ferramenta é chamada, **Then** a falha é técnica e nenhuma evidência parcial é devolvida como se a busca tivesse ocorrido.

### User Story 3 - Guardar o caso confirmado sem devolvê-lo na ferramenta atual (Priority: P2)

A administração grava um caso já confirmado, com decisão, autoridade, data, empresa e execução de origem. Sugestão não confirmada não entra. A ferramenta MCP atual não devolve esse caso.

**Why this priority**: A specification 002 já fixou que o caso vale só para a empresa da execução. A ferramenta atual não recebe empresa.

**Independent Test**: Gravar um caso confirmado da empresa A. A administração devolve esse caso. Uma chamada de `consultar_conhecimento` não devolve esse caso.

**Acceptance Scenarios**:

1. **Given** empresa, execução, papel, data e decisão, **When** o caso confirmado é ingerido, **Then** a administração o recupera com esses vínculos e a faixa de caso corporativo confirmado.
2. **Given** falta empresa, execução, papel, data ou decisão, **When** a ingestão do caso é enviada, **Then** ela é recusada.
3. **Given** o caso foi gravado, **When** a ferramenta MCP é chamada só com texto e vigência, **Then** esse caso não aparece na lista.

### User Story 4 - Recusar o que esta specification não sabe ler (Priority: P2)

PDF e XML de notas são aceitos como extração de texto. Arquivo sem texto identificável, formato fora da lista ou arquivo sem limite configurado é recusado, sem virar evidência.

**Why this priority**: A constituição proíbe schema inventado e limite numérico inventado.

**Independent Test**: Enviar um tipo que não seja texto, PDF ou XML de notas e receber recusa. Com o limite de arquivo vazio, receber recusa de arquivo sem gravação.

**Acceptance Scenarios**:

1. **Given** um PDF ou XML de notas com texto extraível, origem, identificação e versão, **When** a ingestão ocorre, **Then** o texto extraído fica vinculado à fonte e nenhum layout oficial é assumido.
2. **Given** o arquivo não produz texto identificável, **When** a ingestão ocorre, **Then** ela é recusada e nenhum chunk é consultável.
3. **Given** o limite de arquivo não está configurado, **When** um arquivo é enviado, **Then** a ingestão é recusada, sem bloqueio fiscal e sem número inventado.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A Api.RAG DEVE oferecer administração REST da base, POST de ingestão e a ferramenta MCP `consultar_conhecimento`.
- **FR-002**: A ingestão DEVE exigir origem, identificação, versão, faixa de autoridade e vigência inicial. Sem qualquer um desses dados, a Api.RAG DEVE recusar o documento.
- **FR-003**: A faixa DEVE ser uma das quatro já decididas: legislação vigente, notas e Regras Gerais de Interpretação, caso corporativo confirmado ou texto do fornecedor. A Api.RAG NÃO DEVE criar outra faixa.
- **FR-004**: Os formatos aceitos DEVEM ser texto puro, PDF e XML de notas. Outro formato DEVE ser recusado. Esta specification NÃO DEVE definir XSD, layout oficial nem leitura de NF-e completa.
- **FR-005**: Arquivo vazio, ilegível ou sem texto identificável DEVE ser recusado. Enquanto o limite de arquivo não estiver configurado, arquivo DEVE ser recusado. O número desse limite NÃO DEVE ser inventado aqui.
- **FR-006**: A ingestão DEVE exigir chave configurada. Chave ausente ou diferente DEVE recusar a operação antes de gravar. Esta specification NÃO DEVE definir catálogo de papéis.
- **FR-007**: Conteúdo idêntico, medido por hash, na mesma origem, identificação e versão, NÃO DEVE gerar chunks novos. Conteúdo diferente na mesma versão DEVE ser recusado.
- **FR-008**: Versão nova do mesmo documento DEVE conviver com a anterior. Cada chunk DEVE pertencer a uma única versão e DEVE guardar documento, hash, fonte, versão, vigência e localização quando a fonte tiver localização.
- **FR-009**: A consulta por uma data DEVE devolver só chunks cujo intervalo contém essa data. A evidência DEVE usar essa data no campo de vigência e DEVE identificar a versão na fonte. Chunks de versões diferentes NÃO DEVEM ser apresentados como uma única vigência.
- **FR-010**: A ferramenta DEVE receber texto e vigência. A evidência DEVE poder trazer fonte, faixa, trecho, localização, vigência, NCM e tributos. NCM e tributos SÓ DEVEM voltar quando tiverem sido informados na ingestão. A Api.RAG NÃO DEVE extrair código, alíquota ou dispositivo por regra própria.
- **FR-011**: Texto, chunk e retorno MCP DEVEM ser dados. Esse conteúdo NÃO DEVE alterar chave, faixa, vigência, contrato ou a decisão de devolver um caso.
- **FR-012**: Falha do provedor de embeddings ou do banco DEVE impedir que a versão fique consultável. A consulta NÃO DEVE devolver resultado parcial como se a busca tivesse sido concluída.
- **FR-013**: Caso corporativo confirmado DEVE guardar decisão, papel da autoridade, data, empresa e execução de origem. Sem esses vínculos, a ingestão do caso DEVE ser recusada. Sugestão não confirmada NÃO DEVE ser gravada como precedente.
- **FR-014**: A ferramenta `consultar_conhecimento` NÃO DEVE devolver caso corporativo. A administração DEVE permitir ler o caso gravado. A Api.Agente NÃO DEVE acessar o PostgreSQL da Api.RAG.
- **FR-015**: Tamanho de chunk, sobreposição, modelo de embeddings, dimensão, topK e limiar NÃO DEVEM ser fixados nesta specification. Eles ficam no plano técnico, avaliados com exemplos, antes de virarem padrão. Até lá, a implementação NÃO DEVE inventar esses números.

### Key Entities

- **Documento**: Origem, identificação, versão, faixa, intervalo de vigência, formato e hash do conteúdo.
- **Chunk**: Trecho de uma única versão, com fonte, localização quando existir e vínculo ao documento.
- **Caso confirmado**: Documento da faixa de caso corporativo confirmado, com decisão, autoridade, data, empresa e execução de origem.
- **Evidência consultada**: Fonte versionada, faixa, trecho, localização, data pedida e NCM ou tributos só quando informados na ingestão.

## Success Criteria *(mandatory)*

- **SC-001**: Em 100% das ingestões repetidas com o mesmo hash, origem, identificação e versão, a base não ganha chunk novo.
- **SC-002**: Em 100% das consultas de aceite, toda evidência devolvida pertence a uma versão cujo intervalo contém a data pedida e traz a fonte dessa versão.
- **SC-003**: Em 100% das consultas MCP de aceite, nenhum caso corporativo é devolvido.
- **SC-004**: Em 100% das falhas do provedor de embeddings, a versão em processamento não fica consultável.
- **SC-005**: Nenhuma evidência de aceite traz NCM ou tributo que não tenha sido informado na ingestão.

## Assumptions

- XML de notas é um arquivo de texto da base, não uma NF-e montada por esta specification. O schema oficial continua em aberto.
- A chave de ingestão vem da configuração do ambiente. Ela não é gravada no repositório.
- A administração REST lê documento e caso por identificação. Ela não substitui a ferramenta MCP da análise.
- A recuperação do caso pela Api.Agente espera um contrato MCP que informe a empresa. Isso não está nesta specification.
- Parâmetros numéricos de chunk, embedding e busca serão decididos no plano, com exemplos representativos.

## Fora de escopo

- Inventar tamanho de chunk, topK, limiar, modelo ou dimensão de embedding.
- Inventar schema, XSD, alíquota ou regra de enquadramento.
- Devolver caso corporativo na ferramenta atual.
- Gravar sugestão não confirmada.
- A Api.Agente escrever no banco da Api.RAG.
- Memória conversacional, custo, papéis de acesso e A2A.
