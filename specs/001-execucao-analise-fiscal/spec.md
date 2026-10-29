# Feature Specification: Execução da análise fiscal de itens

**Feature Branch**: `001-execucao-analise-fiscal`

**Created**: 2026-10-29

**Status**: Draft

**Input**: User description: "Agente que sugere NCM e tratamento tributário, devolve o XML do item e pode bloquear a nota quando a classificação declarada não se sustenta no produto. A primeira specification cobre somente a execução dessa análise."

## Clarifications

### Session 2026-10-29

- Q: Qual precedência vale quando os itens terminam de formas diferentes? → A: Bloqueada, depois pendente de dado, depois falha técnica, depois sugestão liberada para uso.
- Q: Quando há sugestão sustentada e o item pede revisão humana, o que é devolvido? → A: A sugestão e o marcador de revisão humana.
- Q: O que é confiança baixa? → A: Valor menor que 70 em uma escala de 0 a 100. O valor 70 não é confiança baixa.
- Q: O que é impacto relevante? → A: Permanece em aberto até haver exemplos reais.
- Q: Qual é o tamanho máximo do arquivo? → A: Permanece em aberto.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sugerir classificação e tratamento do item (Priority: P1)

Um analista, ou um sistema autorizado em nome dele, envia uma descrição de produto com contexto fiscal e dados essenciais suficientes. O agente devolve a NCM sugerida, as alternativas sustentadas, o tratamento tributário encontrado nas evidências e o XML somente daquele item.

**Why this priority**: Sem a sugestão rastreável por item, auditoria, bloqueio e XML não têm o que devolver.

**Independent Test**: Enviar uma descrição com função, material, composição, uso e apresentação, mais o contexto da execução, e verificar que o resultado traz candidato existente na tabela vigente, tratamento apenas quando houver evidência, XML só do item e a fonte de cada campo.

**Acceptance Scenarios**:

1. **Given** uma descrição com composição, função e apresentação suficientes, contexto completo e legislação vigente que sustenta uma NCM, **When** a análise é solicitada com a intenção de sugerir NCM e tratamento, **Then** o item recebe a NCM sugerida, as alternativas também sustentadas, o tratamento de IPI, ICMS, CEST, PIS/COFINS e IBS/CBS coberto por evidência, o XML somente desse item e a referência recuperável de cada campo.
2. **Given** a legislação vigente indica uma NCM e o texto do fornecedor indica outra, **When** a análise é concluída, **Then** a sugestão segue a legislação vigente e o texto do fornecedor permanece citado como evidência de faixa inferior.
3. **Given** não há evidência suficiente para escolher uma NCM na vigência do item, **When** a análise é concluída, **Then** o resultado declara a limitação, não escolhe candidato e não marca o item como divergente.
4. **Given** um tributo do item não possui evidência na vigência, **When** a análise sugere a NCM, **Then** esse tributo volta como limitação, sem alíquota, código ou dispositivo criado pela análise.

---

### User Story 2 - Auditar o declarado e bloquear a nota (Priority: P1)

O analista envia itens que já trazem NCM ou tratamento declarado. Quando a evidência de maior autoridade não sustenta o que foi declarado, o item volta divergente e a nota volta bloqueada, junto com a classificação que as evidências sustentam.

**Why this priority**: O bloqueio da nota é o resultado que impede o uso de uma classificação incorreta. Ele depende da mesma análise da história 1 e precisa ser aceito junto com ela.

**Independent Test**: Montar uma nota com um item cuja NCM declarada contradiz a legislação vigente aplicável e outro item cuja declaração coincide com essa evidência. O primeiro fica divergente, o segundo não, e a nota fica bloqueada.

**Acceptance Scenarios**:

1. **Given** um item com NCM ou tratamento declarado incompatível com a evidência de maior autoridade, **When** a auditoria termina, **Then** o item fica divergente, o resultado indica o que está incorreto e qual classificação as evidências sustentam, e a nota fica bloqueada.
2. **Given** todos os itens da nota têm declaração compatível com a evidência vencedora e nenhum outro impedimento, **When** a auditoria termina, **Then** nenhum item fica divergente e a nota fica com sugestão liberada para uso.
3. **Given** a nota tem itens consistentes e um item divergente, **When** a auditoria termina, **Then** a nota fica bloqueada e os itens consistentes continuam com o próprio resultado.
4. **Given** a análise conclui sugestão ou bloqueio, **When** o resultado é lido, **Then** ele não contém declaração de conformidade fiscal do documento.

---

### User Story 3 - Continuar os itens que já podem ser analisados (Priority: P2)

Em uma nota com vários itens, um item sem composição, função ou apresentação suficientes não impede a análise dos demais. Esse item pede o dado que falta e não é classificado nem usado para bloquear a nota.

**Why this priority**: O lote real mistura item completo e item incompleto. Parar a nota inteira atrasaria itens que já podem ser sugeridos ou auditados.

**Independent Test**: Enviar dois itens no mesmo contexto, um com os atributos essenciais e outro sem composição, função e apresentação. Somente o incompleto pede dado; o completo recebe resultado; a nota não fica bloqueada por causa do incompleto.

**Acceptance Scenarios**:

1. **Given** uma nota com um item completo e um item sem dado essencial, **When** a análise é executada, **Then** o item incompleto pede o dado ausente, não recebe NCM e não bloqueia a nota, e o item completo recebe o resultado da análise.
2. **Given** a mesma nota também possui um item divergente, **When** a análise termina, **Then** a nota fica bloqueada pelo item divergente e o item incompleto continua sem classificação.
3. **Given** não existe item divergente e existe item aguardando dado essencial, **When** a análise termina, **Then** a nota fica pendente de dado.

---

### User Story 4 - Aceitar planilha e documento técnico (Priority: P2)

O analista envia, além de texto e XML de NF-e, uma planilha de itens, um PDF, um laudo ou uma ficha técnica. O que puder ser lido vira item de análise e segue as mesmas regras. Arquivo ilegível ou layout não registrado volta como erro estrutural.

**Why this priority**: Essas entradas fazem parte da primeira versão, mas a sugestão e o bloqueio já podem ser aceitos com texto e XML. O layout concreto de cada formato depende de fonte registrada, que esta specification não inventa.

**Independent Test**: Enviar um arquivo ilegível e um arquivo cujo texto permita identificar um item com dados essenciais. O ilegível produz erro estrutural sem bloqueio fiscal. O legível produz resultado por item, da mesma natureza da descrição textual.

**Acceptance Scenarios**:

1. **Given** uma planilha, um PDF, um laudo ou uma ficha técnica da qual se extraem itens com dado essencial, **When** a análise é solicitada, **Then** cada item extraído segue a sugestão, a auditoria, o pedido de dado ou o bloqueio já definidos nas histórias anteriores.
2. **Given** um arquivo ilegível, vazio ou de layout não registrado para o tipo e a versão do documento, **When** a análise é solicitada, **Then** o resultado é erro estrutural do documento e a nota não é bloqueada por divergência fiscal.
3. **Given** um XML de NF-e com layout registrado, **When** a análise trata os itens, **Then** fato do produto, declaração fiscal e anexo de apoio permanecem distinguíveis no resultado, e o XML devolvido não é uma NF-e completa.

---

### User Story 5 - Pedir revisão humana sem bloquear por incerteza (Priority: P3)

Quando a confiança é menor que 70, duas fontes da mesma faixa de autoridade permanecem empatadas, ou o item se enquadra em impacto relevante, o resultado pede revisão humana. Esse pedido, sozinho, não bloqueia a nota. A divergência já sustentada continua bloqueando.

**Why this priority**: O bloqueio por evidência suficiente já entrega valor na história 2. A revisão humana evita que incerteza seja tratada como erro fiscal. O corte de confiança está definido; a definição de impacto relevante ainda depende de exemplos reais.

**Independent Test**: Produzir um item com duas evidências da mesma faixa, sem fonte de faixa superior, e outro item com divergência sustentada por legislação. O primeiro pede revisão e não bloqueia por si só. O segundo bloqueia a nota.

**Acceptance Scenarios**:

1. **Given** duas evidências da mesma faixa de autoridade apontam NCM diferentes e não há fonte de faixa superior, **When** a análise termina, **Then** o item pede revisão humana, não escolhe um vencedor inventado e, isoladamente, não bloqueia a nota.
2. **Given** a análise possui candidato sustentado com confiança menor que 70, ou com impacto relevante, **When** a análise termina, **Then** a sugestão é devolvida junto com o pedido de revisão humana e a nota só fica bloqueada se também houver divergência sustentada.
3. **Given** o candidato sustentado tem confiança 70 ou maior e não há empate, divergência nem impacto relevante, **When** a análise termina, **Then** a sugestão é devolvida sem pedido de revisão por confiança baixa.
4. **Given** um item divergente e um item apenas encaminhado à revisão, **When** a análise termina, **Then** a nota fica bloqueada pelo item divergente e o outro item permanece marcado para revisão.

---

### Edge Cases

- Contexto obrigatório ausente: a execução é rejeitada antes de classificar, auditar ou bloquear.
- Arquivo acima do limite de processamento, quando esse limite estiver registrado: a execução é rejeitada como condição de processamento, sem bloqueio fiscal.
- Confiança 70 ou maior: não dispara revisão por confiança baixa. Confiança menor que 70 dispara revisão e mantém a sugestão sustentada.
- Confiança que não pôde ser calculada: permanece desconhecida, sem ser gravada como zero, e o item pede revisão humana.
- XML, planilha ou documento sem nenhum item identificável: erro estrutural, sem bloqueio fiscal.
- Item com declaração igual à evidência vencedora: não é divergente.
- Legislação vigente contra caso corporativo confirmado: prevalece a legislação; o caso permanece citado.
- Caso corporativo confirmado contra texto de fornecedor, sem legislação nem nota aplicável: prevalece o caso confirmado.
- Base de conhecimento indisponível: falha técnica, sem bloqueio e sem classificação daquele item.
- Conteúdo do arquivo ou da evidência pedindo para ignorar regra ou inventar código: o conteúdo permanece dado e as regras desta specification continuam valendo.
- Modelo ou etapa posterior devolve código, alíquota, dispositivo ou referência que não está nas evidências da vigência: a saída não é aceita e o item termina em falha técnica, não em sugestão.
- Todos os itens falham por dependência indisponível: a nota fica em falha técnica.
- Item divergente junto com item em falha técnica: a nota fica bloqueada e a falha técnica continua identificada no item afetado.
- Vigência, UF, regime, origem, destino ou tipo de operação informados no item: esses valores prevalecem sobre os da execução para aquele item.
- Intenção de sugerir XML: o artefato continua restrito ao item analisado.
- Não existe caso corporativo semelhante: a análise segue com essa evidência ausente e não grava caso novo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A execução DEVE informar usuário, empresa, papel, finalidade, UF, regime tributário, tipo de operação e data de vigência. Sem qualquer um desses dados, o agente DEVE rejeitar a execução antes da análise.
- **FR-002**: O agente DEVE aceitar como entrada descrição textual, XML de NF-e, planilha de itens, PDF, laudo e ficha técnica. Tipo fora dessa lista DEVE ser recusado.
- **FR-003**: O agente DEVE rejeitar arquivo ilegível, vazio, acima do limite ou fora das condições de processamento, sem classificá-lo e sem bloqueá-lo. [NEEDS CLARIFICATION: tamanho máximo e demais limites numéricos de processamento não foram informados]
- **FR-004**: O agente DEVE identificar tipo de documento e versão de layout quando o arquivo tiver layout. Versão sem layout registrado DEVE produzir erro estrutural. O layout concreto, o schema e o validador de cada formato NÃO são definidos nesta specification e DEVEM ser incorporados de fonte oficial antes da implementação do validador correspondente.
- **FR-005**: Para cada item, o agente DEVE extrair, quando presentes, descrição, unidade, quantidade, NCM, CEST, CFOP, CST e tributos declarados de IPI, ICMS, PIS/COFINS e IBS/CBS, preservando o texto original.
- **FR-006**: O agente DEVE normalizar cada item em função, material, composição, uso, apresentação e termos técnicos, sem substituir o texto original.
- **FR-007**: Item sem composição, função ou apresentação suficientes DEVE receber pedido objetivo do dado ausente. Esse item NÃO DEVE ser classificado, NÃO DEVE ser marcado divergente e NÃO DEVE, sozinho, bloquear a nota. Os demais itens da mesma execução DEVEM ser analisados.
- **FR-008**: Cada item DEVE ficar vinculado a vigência, UF, regime, origem, destino e tipo de operação. Valor informado no item DEVE prevalecer sobre o valor da execução.
- **FR-009**: A análise DEVE usar evidências da vigência do item para NCM, notas, Regras Gerais de Interpretação, IPI, ICMS, CEST, PIS/COFINS, IBS/CBS, casos corporativos confirmados e documentação técnica. Indisponibilidade dessa base DEVE ser falha técnica do item afetado.
- **FR-010**: As evidências DEVEM ser ordenadas pela autoridade da fonte, nesta ordem: legislação vigente na data do item; notas e Regras Gerais de Interpretação; caso corporativo confirmado; texto do fornecedor. Fonte de faixa superior DEVE prevalecer. Empate dentro da mesma faixa DEVE pedir revisão humana e NÃO DEVE ser resolvido por preferência inventada.
- **FR-011**: Todo candidato de NCM DEVE existir na tabela vigente usada como evidência e DEVE indicar a regra, a nota e o trecho que o sustentam. A análise NÃO DEVE criar código, alíquota, dispositivo legal, evidência ou referência.
- **FR-012**: O tratamento de cada item DEVE contemplar IPI, ICMS, CEST, PIS/COFINS e IBS/CBS. Valor sem evidência na vigência DEVE ser devolvido como limitação daquele tributo.
- **FR-013**: A sugestão de XML DEVE conter somente o item ou os itens analisados e DEVE seguir o layout de item registrado. A análise NÃO DEVE montar uma NF-e completa.
- **FR-014**: Quando a NCM ou o tratamento declarado não se sustentar na evidência vencedora, o item DEVE ficar divergente, indicar o que está incorreto e apresentar a classificação sustentada. Se ao menos um item estiver divergente, a nota DEVE ficar bloqueada.
- **FR-015**: Declaração compatível com a evidência vencedora NÃO DEVE tornar o item divergente.
- **FR-016**: Ausência de evidência suficiente DEVE ser declarada como limitação. Ela NÃO DEVE escolher candidato, NÃO DEVE aparecer como divergência e NÃO DEVE bloquear a nota.
- **FR-017**: Cada item com sugestão ou divergência DEVE receber uma confiança de 0 a 100. Confiança menor que 70, confiança desconhecida, empate de fontes da mesma faixa ou impacto relevante DEVEM marcar o item para revisão humana. O valor 70 não é confiança baixa. Confiança desconhecida NÃO DEVE ser gravada como zero. Isoladamente, esses motivos NÃO DEVEM bloquear a nota. Havendo candidato sustentado, a sugestão DEVE ser devolvida junto com o pedido de revisão. A forma de calcular a confiança fica no plano técnico e DEVE usar essa escala. [NEEDS CLARIFICATION: definição de impacto relevante não foi informada]
- **FR-018**: O estado da nota DEVE seguir esta precedência: bloqueada, quando existir item divergente; pendente de dado, quando não houver divergente e existir item aguardando dado essencial; falha técnica, quando não houver divergente nem dado pendente e a análise de algum item tiver falhado por dependência ou saída inválida; sugestão liberada para uso, quando todos os itens tiverem sido analisados sem divergência.
- **FR-019**: Cada achado DEVE pertencer a uma única classe: erro estrutural do documento, inconsistência de negócio, indício identificado pela IA, ausência de evidências ou falha técnica ou dependência indisponível. Indício da IA NÃO DEVE ser apresentado como inconsistência determinística. Falha técnica NÃO DEVE ser apresentada como bloqueio fiscal.
- **FR-020**: O resultado DEVE trazer identificador da execução, versão do agente, versão de cada base consultada, estado da nota, resultado por item, fonte, trecho quando existir, vigência, limitações, validações não realizadas e necessidade de revisão humana. Consumo, custo e duração, quando existirem, DEVEM ser produzidos pelo sistema que mede a execução, não pela análise generativa.
- **FR-021**: O agente NÃO DEVE atestar conformidade fiscal. Sugestão liberada para uso DEVE significar apenas que o consumidor pode usar a sugestão daquela execução.
- **FR-022**: Arquivo, evidência, caso e documentação técnica DEVEM ser tratados como dados. Esse conteúdo NÃO DEVE alterar contexto obrigatório, ordem de autoridade, autorização ou regras de bloqueio.
- **FR-023**: Esta specification NÃO DEVE gravar, confirmar ou corrigir caso corporativo. Caso já confirmado na base PODE ser recuperado como evidência, com fonte e desfecho.
- **FR-024**: A saída só DEVE ser aceita quando a estrutura e as regras semânticas forem válidas, incluindo a existência dos códigos na vigência e a coerência entre NCM, CEST, tributos e XML do item. Saída inválida, esgotadas as tentativas previstas no plano técnico, DEVE terminar em falha técnica.
- **FR-025**: As intenções aceitas nesta specification DEVEM ser sugerir NCM, auditar o declarado, sugerir tratamento tributário e sugerir o XML do item. A recuperação de caso confirmado e de documentação técnica DEVE ocorrer como parte dessas intenções, não como rotina de cadastro.

### Key Entities

- **Execução**: Pedido de análise identificado, com usuário, empresa, papel, finalidade, contexto fiscal, intenção, versão do agente, versões das bases e estado da nota.
- **Item de análise**: Produto dentro da execução, com texto original, atributos normalizados, declaração fiscal eventualmente presente, contexto fiscal efetivo e resultado próprio.
- **Evidência**: Trecho recuperável de legislação, nota, Regra Geral de Interpretação, caso corporativo confirmado ou documentação técnica, com fonte, localização quando existir, vigência e faixa de autoridade.
- **Resultado do item**: Situação do item, NCM sugerida e alternativas quando existirem, tratamento sustentado, limitações por tributo, XML do item quando a intenção o incluir, pedido de dado ausente, pedido de revisão humana e achados classificados.
- **Estado da nota**: Bloqueada, pendente de dado, falha técnica ou sugestão liberada para uso, calculado pela precedência do FR-018.
- **Achado**: Ocorrência de uma das cinco classes do FR-019, ligada à execução ou a um item, com severidade definida no contrato versionado da saída.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos cenários de aceite com divergência sustentada pela evidência de maior autoridade, a nota retorna bloqueada e o item divergente mostra o que está incorreto, a classificação sustentada e a fonte.
- **SC-002**: Em 100% dos cenários de aceite com item sem composição, função ou apresentação suficientes, esse item não recebe NCM e não bloqueia a nota, e cada item completo da mesma execução recebe resultado próprio.
- **SC-003**: Em 100% dos cenários de aceite em que legislação vigente contradiz caso corporativo confirmado ou texto de fornecedor, a classificação sugerida ou o bloqueio seguem a legislação vigente.
- **SC-004**: Em 100% dos resultados aceitos, todo código NCM sugerido ou usado para bloquear existe nas evidências da vigência do item, e o XML devolvido não é uma NF-e completa.
- **SC-005**: Em 100% dos cenários de aceite das cinco classes de achado, um revisor identifica a classe no resultado sem convertê-la em bloqueio fiscal, exceto na inconsistência de negócio que caracteriza item divergente.
- **SC-006**: Nenhum cenário de aceite desta specification grava caso corporativo novo.
- **SC-007**: Em 100% dos cenários de aceite com candidato sustentado e confiança menor que 70, o resultado devolve a sugestão e o pedido de revisão humana, e a nota só fica bloqueada se também houver item divergente.

## Assumptions

- O consumidor é um analista fiscal ou um sistema que atua com o contexto obrigatório da execução. O meio de autenticação fica para a especificação de segurança.
- O resultado completo de uma execução pode ser obtido pelo seu identificador. A escolha entre resposta na mesma solicitação ou consulta posterior fica no plano técnico.
- Pedido de revisão humana devolve o marcador no resultado. A fila, o responsável e a tela de revisão não fazem parte desta specification.
- Impacto relevante pede revisão e, sozinho, não bloqueia. A definição do que é relevante permanece em aberto no FR-017.
- Layout, schema, XSD e nomes de campos fiscais de XML, planilha, PDF, laudo e ficha serão registrados a partir de fonte oficial. Esta specification exige o comportamento e recusa layout desconhecido.
- Regra fiscal concreta, tabela, alíquota e critério de enquadramento vêm das evidências da vigência, não do texto desta specification.
- Ingestão e administração da base de conhecimento, memória conversacional, registro de consumo, exposição para outros agentes e gravação de caso corporativo ficam para specifications próprias.
- A ordem de autoridade do FR-010 e o corte de confiança do FR-017 estão decididos. O limite numérico de arquivo e a definição de impacto relevante continuam pendentes.

## Fora de escopo

- Gravar, confirmar ou corrigir caso corporativo.
- Administrar ou ingerir a base de conhecimento.
- Expor o agente para outros agentes ou orquestradores.
- Montar NF-e completa.
- Atestar conformidade fiscal.
- Definir retenção de memória, custo, modelo de linguagem ou parâmetro de busca.
- Inventar layout, schema, alíquota ou dispositivo legal.
