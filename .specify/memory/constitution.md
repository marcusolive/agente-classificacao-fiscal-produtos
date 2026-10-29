# Constituição do Agente de Classificação Fiscal de Produtos

Esta constituição vincula especificações, esclarecimentos, planos técnicos, tarefas, implementações, testes e revisões de pull requests do Agente de Classificação Fiscal de Produtos.

Em conflito com outra prática do repositório, esta constituição prevalece até ser alterada pelo rito da seção 8.

As palavras normativas têm o seguinte sentido:

- **DEVE** indica obrigação.
- **NÃO DEVE** indica proibição.
- **PODE** indica opção permitida, nunca obrigação.

Layouts, regras fiscais, schemas, alíquotas, dispositivos legais e critérios de conformidade **NÃO** são definidos nesta constituição. Esses elementos **DEVEM** ser obtidos de documentos oficiais, ou de decisões corporativas confirmadas, e registrados na especificação correspondente antes da implementação.

**Versão**: 1.0.5 | **Ratificada**: 2026-10-29 | **Última alteração**: 2026-10-29

## 1. Propósito e escopo

O projeto implementa um agente de IA que sugere, identifica e analisa classificações NCM e tratamentos tributários a partir de descrições de produtos, notas fiscais, documentação técnica, casos corporativos e legislação vigente.

O agente **DEVE** ser capaz de:

- sugerir NCM e alternativas sustentadas por evidência;
- auditar NCM e tratamento já declarados;
- sugerir tratamento de IPI, ICMS, CEST, PIS/COFINS e IBS/CBS;
- sugerir XML somente do item ou dos itens analisados;
- recuperar casos corporativos semelhantes e documentação técnica, com fonte e desfecho;
- marcar o item como divergente quando a NCM ou o tratamento declarado não se sustentarem no produto;
- marcar a nota como bloqueada quando ao menos um item estiver divergente.

O agente **NÃO DEVE** atestar conformidade fiscal do documento. O estado "sugestão liberada para uso" significa que a sugestão pode ser consumida por pessoa ou sistema. Esse estado **NÃO** é certificado de conformidade legal.

A solução **DEVE** combinar validação determinística de arquivos e registros, consulta a manuais por RAG, uso controlado de ferramentas externas, análise com LLM, resposta estruturada e rastreável, e exposição para integração futura com outros agentes e orquestradores por A2A.

Fora de escopo desta constituição, e portanto proibido tratar como requisito já aprovado:

- regra fiscal concreta, tabela, alíquota ou critério de enquadramento não registrado em especificação com fonte;
- schema ou layout de documento não obtido de fonte oficial e não registrado em especificação;
- escolha entre MongoDB e Redis;
- tamanho de chunk, topK, limiar de similaridade, modelo de embeddings e dimensão dos vetores;
- versão de SDK, nome de modelo de LLM e parâmetros numéricos de execução.

## 2. Princípios obrigatórios

### Princípio 1 — Fluxo de análise integral

**Regra.** Toda execução de análise **DEVE** percorrer o fluxo obrigatório da seção 3.3 na ordem definida. Uma etapa posterior **NÃO DEVE** compensar a omissão de uma etapa anterior. A etapa 10 **DEVE** encerrar classificação e bloqueio enquanto composição, função ou apresentação forem insuficientes. A etapa 14 **DEVE** ocorrer somente quando a ferramenta for necessária e estiver autorizada. A etapa 25 **NÃO DEVE** ocorrer na execução que produz a sugestão; ela **DEVE** ocorrer somente depois da confirmação de quem tem autoridade para aceitar ou corrigir.

**Justificativa.** Classificação fiscal sem contexto, vigência, evidência e confronto com o declarado não é auditável e não pode bloquear uma nota com segurança.

**Verificação.** O revisor **DEVE** recusar pull request cujo caminho de análise pule etapa, classifique com dado essencial ausente, bloqueie sem o confronto da etapa 19 ou grave caso corporativo antes da confirmação. Teste de fluxo **DEVE** cobrir interrupção por dado ausente, divergência com bloqueio da nota e sugestão sem declaração prévia.

### Princípio 2 — Validação determinística fora do LLM

**Regra.** Regras estruturais e regras de negócio determinísticas **DEVEM** ser implementadas em validadores ou ferramentas verificáveis. O LLM **NÃO DEVE** substituir essas validações. Validação por XSD **DEVE** ser aplicada somente a XML para o qual exista schema correspondente registrado na especificação. Os demais formatos **DEVEM** usar validador compatível com o layout registrado para aquele tipo e versão. O agente **NÃO DEVE** declarar validação concluída, nem liberar sugestão, quando validação obrigatória não tiver terminado ou tiver falhado por dependência indisponível.

**Justificativa.** Schema, integridade e código existente em tabela vigente são verificáveis sem modelo generativo. Delegar essa verificação ao LLM torna o resultado irreproduzível.

**Verificação.** Pull request que aceite resultado de análise só com texto do modelo, sem validador da etapa correspondente, **DEVE** ser recusado. Para cada formato aceito, a especificação e os testes **DEVEM** apontar o validador usado. Teste **DEVE** demonstrar que XML sem schema registrado não é submetido a XSD inexistente.

### Princípio 3 — Classes de achado separadas

**Regra.** Toda resposta de análise **DEVE** distinguir, sem colapsar em um único erro genérico:

- erro estrutural do documento;
- inconsistência de negócio;
- indício identificado pela IA;
- ausência de evidências;
- falha técnica ou dependência indisponível.

Indício identificado pela IA **NÃO DEVE** ser apresentado como inconsistência de negócio determinística. Ausência de evidências **NÃO DEVE** ser apresentada como divergência do item. Falha técnica **NÃO DEVE** ser apresentada como bloqueio fiscal da nota.

**Justificativa.** Bloqueio de nota, pedido de complemento e indisponibilidade de dependência exigem tratamentos diferentes pelo consumidor da API.

**Verificação.** O contrato da especificação **DEVE** conter as cinco classes. Teste de contrato **DEVE** produzir ao menos um exemplo de cada classe. Pull request que mapeie falha de MCP, RAG ou LLM para "item divergente" ou "nota bloqueada" **DEVE** ser recusado.

### Princípio 4 — Evidência vigente e não fabricada

**Regra.** Candidato de NCM, tratamento tributário, alíquota, dispositivo legal e referência **DEVEM** existir nas evidências recuperadas para a vigência fixada na execução. O LLM **NÃO DEVE** criar código, alíquota, dispositivo, evidência, resultado de ferramenta ou referência. Na ausência de evidência suficiente, o agente **DEVE** declarar a limitação e **NÃO DEVE** escolher um candidato. Evidências **DEVEM** ser filtradas pela vigência e ordenadas pela autoridade da fonte. A ordem de autoridade **DEVE** estar definida na especificação antes da implementação da etapa 15.

**Justificativa.** Sugestão fiscal sem fonte recuperável não pode ser revista nem defendida.

**Verificação.** A saída aceita **DEVE** trazer, para cada campo sugerido e para cada bloqueio, fonte, trecho quando disponível, vigência e identificador da execução. Teste **DEVE** reprovar resposta do modelo com código ou referência ausente das evidências recuperadas. Pull request que fixe ordem de autoridade diferente da especificação aprovada **DEVE** ser recusado.

### Princípio 5 — Conteúdo recuperado é dado, não instrução

**Regra.** Arquivo recebido, chunk, manual, caso corporativo, documentação técnica e retorno de ferramenta **DEVEM** ser tratados como dado não confiável. Esse conteúdo **NÃO DEVE** alterar regras do sistema, princípios desta constituição, autorização, contrato de saída ou seleção de ferramentas permitidas. A recuperação **DEVE** respeitar o tipo de documento e a versão do manual aplicável à execução.

**Justificativa.** Documento fiscal e base RAG podem conter texto instruindo o modelo a ignorar validações. Aceitar essa instrução quebra o controle do agente.

**Verificação.** Os prompts e as ferramentas **DEVEM** manter o conteúdo recuperado em delimitadores de dado. Teste **DEVE** usar um documento ou chunk com instrução para ignorar validação ou inventar NCM e comprovar que a execução mantém as regras do sistema. Pull request que concatene conteúdo de arquivo ou chunk às instruções de sistema **DEVE** ser recusado.

### Princípio 6 — Contrato do modelo separado do envelope da API

**Regra.** O JSON produzido pelo LLM e o envelope retornado pela API **DEVEM** ser contratos distintos, explícitos e versionados na especificação. O contrato do LLM **DEVE** ser obtido por mecanismo de saída estruturada compatível com o modelo configurado. A API **DEVE** validar estrutura e regras semânticas antes de aceitar a resposta. Identificador de execução, consumo, custos, tempos e demais metadados operacionais **DEVEM** ser preenchidos pelo sistema. O modelo **NÃO DEVE** inventá-los. O modelo **PODE** apenas ecoar identificador de execução recebido do sistema.

O contrato **DEVE** contemplar, no conjunto formado pela saída do modelo e pelo envelope da API, os itens definidos na especificação para: identificador da execução, status do processamento, resultado da análise, inconsistências e severidades, evidências e referências, limitações e validações não realizadas, e necessidade de revisão humana.

**Justificativa.** Metadado operacional inventado pelo modelo impede auditoria de custo, correlação e confiança no status.

**Verificação.** A especificação **DEVE** mostrar os dois contratos e a versão de cada um. Teste **DEVE** rejeitar JSON fora do contrato e **DEVE** comprovar que tokens, custo e duração permanecem os valores medidos pelo sistema mesmo que o modelo devolva outros. Pull request que publique o JSON bruto do modelo como resposta final da API **DEVE** ser recusado.

### Princípio 7 — Ferramenta escolhida não está autorizada

**Regra.** Cada ferramenta **DEVE** ter nome e responsabilidade únicos, contratos de entrada e saída, validação de argumentos, regras de autenticação e autorização, timeout, tratamento de falha, testes e registro de execução. Quando a operação puder ser repetida, a ferramenta **DEVE** declarar sua política de idempotência. A seleção da ferramenta pelo LLM **NÃO DEVE** conceder autorização de execução. A execução **DEVE** ocorrer somente se a política de autorização da execução permitir aquela ferramenta. Motivo, entrada e saída **DEVEM** entrar na trilha.

**Justificativa.** Function calling indica intenção do modelo. Autorização é decisão do sistema, ligada ao usuário, à organização e à finalidade da execução.

**Verificação.** Teste **DEVE** negar ferramenta selecionada pelo modelo quando a política não autorizar. Pull request de ferramenta sem contrato, timeout, teste ou registro de execução **DEVE** ser recusado. Revisão **DEVE** confirmar que a autorização é avaliada no código da aplicação, não apenas descrita no prompt.

### Princípio 8 — Cada serviço é dono dos seus dados

**Regra.** A comunicação funcional entre Api.Agente e Api.RAG **DEVE** ocorrer por MCP. A Api.Agente **NÃO DEVE** acessar tabelas, vetores ou armazenamento interno da Api.RAG. A Api.RAG **NÃO DEVE** acessar persistência de execução, memória ou consumo da Api.Agente. Cada serviço **DEVE** ser o único escritor dos seus dados.

**Justificativa.** Acesso cruzado ao banco acopla os serviços e impede que a base de conhecimento evolua sem quebrar o agente.

**Verificação.** Pull request da Api.Agente que referencie connection string, schema, tabela ou cliente de banco da Api.RAG **DEVE** ser recusado. A dependência permitida é cliente MCP. Teste de arquitetura ou revisão de referências **DEVE** falhar se essa fronteira for violada.

### Princípio 9 — Memória conversacional não é conhecimento oficial

**Regra.** Histórico conversacional e base normativa **DEVEM** permanecer em armazenamentos e usos separados. Memória **DEVE** ser isolada por usuário e por conversa, e por organização quando a especificação determinar que o cenário é multiorganização. A execução **DEVE** limitar o histórico enviado ao modelo. A memória **NÃO DEVE** ser fonte suficiente para candidato de NCM, tratamento tributário ou bloqueio. Caso corporativo só **DEVE** sustentar análise depois de recuperado da base de conhecimento, com fonte e desfecho.

**Justificativa.** Conversa anterior pode estar desatualizada ou pertencer a outro contexto fiscal. Precedente interno nasce da confirmação registrada, não do histórico do chat.

**Verificação.** Teste **DEVE** impedir que texto presente só na memória conversacional apareça como evidência normativa. Pull request que grave manual, legislação ou caso confirmado apenas no armazenamento de chat **DEVE** ser recusado. A política de retenção, expiração e exclusão **DEVE** estar aprovada no plano técnico antes da implementação da memória.

### Princípio 10 — Consumo desconhecido não é zero

**Regra.** O controle de consumo **DEVE** ser persistido em PostgreSQL. Cada chamada ao provedor, inclusive chamada adicional e retentativa, **DEVE** gerar registro quando a chamada tiver sido tentada. O registro **DEVE** conter execução e correlação, provedor e modelo, data, duração e status. Tokens de entrada, tokens de saída, consumo de embeddings e custo **DEVEM** ser gravados quando o provedor os informar. Valor estimado **DEVE** ser distinguido de valor informado pelo provedor. Dado de consumo desconhecido **NÃO DEVE** ser gravado como zero. A estimativa **DEVE** citar a referência da tabela de preços usada.

**Justificativa.** Zero falso subestima custo e esconde chamada cujo provedor não devolveu uso.

**Verificação.** Teste **DEVE** cobrir chamada com uso informado, chamada com uso ausente e retentativa. No caso ausente, o campo **DEVE** permanecer desconhecido, não zero. Pull request que some retentativa sem persistir a chamada, ou que grave custo sem indicar se é estimado ou informado, **DEVE** ser recusado.

### Princípio 11 — Parâmetro ajustável fica fora da constituição

**Regra.** Modelo de LLM, parâmetros de execução, tamanho de chunk, critério de segmentação, deduplicação, reprocessamento, atualização, dimensão de vetor, estratégia de busca, topK e limiar de similaridade **NÃO DEVEM** ser fixados nesta constituição. Cada um **DEVE** ser configurado fora do código de domínio ou especificado no plano técnico. Parâmetros de recuperação e de embeddings **DEVEM** ser compatíveis entre si, documentados e avaliados com exemplos representativos antes de serem aceitos como padrão da implementação.

**Justificativa.** Esses valores mudam com medição. Congelá-los na constituição obriga alteração constitucional para ajuste operacional.

**Verificação.** Pull request que introduza esses números como constante de domínio, sem configuração externa e sem referência ao plano ou à avaliação, **DEVE** ser recusado. O plano **DEVE** apontar os exemplos usados na avaliação.

### Princípio 12 — Rastreabilidade de requisito a teste

**Regra.** Cada funcionalidade **DEVE** ter critérios de aceite verificáveis e ligação explícita entre requisito, tarefa e teste. Decisão arquitetural relevante **DEVE** ser registrada em ADR antes da implementação que depende dela. Desvio desta constituição **NÃO DEVE** entrar no código sem o registro exigido na seção 8.

**Justificativa.** Especificação que não chega ao teste não controla o comportamento fiscal do agente.

**Verificação.** Pull request de funcionalidade sem critério de aceite, sem teste associado ou com desvio não registrado **DEVE** ser recusado. ADR ausente para decisão listada como pendente na seção 9 bloqueia a implementação daquela decisão.

## 3. Restrições arquiteturais

### 3.1 Forma da solução

A solução **DEVE** ser denominada Agente de Classificação Fiscal de Produtos.

A implementação **DEVE** usar .NET 10 com ASP.NET Core.

Os projetos principais **DEVEM** ser Api.Agente e Api.RAG. Projeto ou biblioteca adicional **PODE** existir somente com justificativa de responsabilidade no plano técnico, demonstrando por que a responsabilidade não cabe a um dos dois projetos. Complexidade sem responsabilidade distinta **NÃO DEVE** ser introduzida.

Cada serviço **DEVE** separar endpoints, aplicação, domínio e infraestrutura. Endpoint **NÃO DEVE** conter regra de classificação, acesso a vetor ou chamada direta ao provedor de modelo.

### 3.2 Api.Agente

A Api.Agente **DEVE**:

- disponibilizar API REST para execução do agente;
- receber arquivos e solicitações de análise;
- orquestrar o fluxo obrigatório da seção 3.3;
- integrar-se à OpenAI com modelo e parâmetros de execução configurados fora do código;
- consultar a Api.RAG como cliente MCP;
- usar Semantic Kernel para integração e execução de ferramentas por function calling;
- gerenciar memória conversacional no armazenamento decidido na seção 9;
- registrar consumo de tokens e custos conforme o Princípio 10;
- validar a resposta estruturada do LLM antes de aceitá-la;
- expor o agente por servidor A2A.

A Api.Agente **NÃO DEVE** gerar embedding da base de conhecimento nem escrever vetor da Api.RAG.

A sugestão de XML **DEVE** restringir-se ao item ou aos itens analisados. A Api.Agente **NÃO DEVE** montar NF-e completa, salvo se uma alteração futura desta constituição incluir esse escopo.

### 3.3 Fluxo obrigatório de análise

A Api.Agente **DEVE** executar a análise nestas etapas:

1. Abrir a execução com usuário, empresa, papel, finalidade, UF, regime tributário, tipo de operação e data de vigência.
2. Registrar a trilha de auditoria com o identificador da execução, a versão do agente e a versão de cada base consultada.
3. Receber a entrada e classificar a intenção: sugerir NCM, auditar o declarado, sugerir tratamento tributário, sugerir o XML do item ou consultar caso corporativo e documentação técnica.
4. Validar tamanho, formato, integridade e condições de processamento, rejeitando arquivo ilegível ou fora do contrato.
5. Identificar o tipo de documento e a versão do layout, separando fato do produto, declaração fiscal e anexo de apoio.
6. Validar estrutura, registros e o schema aplicável de cada item.
7. Extrair de cada item a descrição, a unidade, a quantidade, a NCM, o CEST, o CFOP, o CST e os tributos de IPI, ICMS, PIS/COFINS e IBS/CBS.
8. Normalizar o produto de cada item em função, material, composição, uso, apresentação e termos técnicos, preservando o texto original.
9. Detectar lacunas, ambiguidades e conflitos entre descrição, XML do item e anexos.
10. Interromper e pedir o dado essencial ausente, sem classificar e sem bloquear enquanto composição, função ou apresentação forem insuficientes.
11. Fixar, para cada item, a vigência, a UF, o regime, a origem, o destino e o tipo de operação.
12. Consultar por MCP os chunks do manual e das bases de NCM, notas, Regras Gerais de Interpretação, IPI, ICMS, CEST, PIS/COFINS e IBS/CBS na vigência definida.
13. Recuperar casos corporativos semelhantes e a documentação técnica associada, com fonte e desfecho.
14. Executar ferramenta externa somente quando for necessária e estiver autorizada, registrando motivo, entrada e saída.
15. Filtrar as evidências pela vigência e ordená-las pela autoridade da fonte.
16. Gerar candidatos de NCM existentes na tabela vigente, com a regra, a nota e o trecho que sustentam cada um.
17. Derivar, para cada item, o tratamento de IPI, ICMS, CEST, PIS/COFINS e IBS/CBS a partir da NCM candidata e do contexto da operação.
18. Produzir a análise com o LLM apenas sobre as evidências recuperadas, sem criar código, alíquota ou dispositivo legal.
19. Comparar o declarado com a conclusão e, se a NCM ou o tratamento não se sustentarem no produto, marcar o item como divergente e indicar o que está incorreto.
20. Validar a saída estruturada: schema, existência dos códigos na vigência e coerência entre NCM, CEST, tributos e XML do item.
21. Calcular a confiança e encaminhar para revisão humana confiança baixa, conflito de fontes ou impacto relevante; divergência sustentada já bloqueia o item.
22. Marcar a nota como bloqueada quando ao menos um item estiver divergente; caso contrário, liberar a sugestão para uso.
23. Montar, por item, a NCM sugerida, as alternativas, o tratamento tributário completo, o XML somente daquele item, os casos corporativos e as referências técnicas.
24. Retornar o resultado com fonte, trecho, vigência, item, estado da nota e identificador da execução.
25. Persistir o caso na base corporativa somente após confirmação de quem tem autoridade para aceitar ou corrigir a sugestão e o bloqueio.

Limites de tamanho, formatos aceitos, condições de processamento, limiar de confiança e definição de impacto relevante **DEVEM** ser resolvidos na especificação ou no plano técnico, conforme a seção 9. Até lá, a implementação **NÃO DEVE** inventar esses critérios no código.

### 3.4 Api.RAG

A Api.RAG **DEVE**:

- disponibilizar API REST para administração da base de conhecimento;
- disponibilizar endpoint POST de ingestão;
- processar documentos, gerar chunks e embeddings;
- armazenar e consultar vetores em PostgreSQL com PgVector;
- expor ferramentas de recuperação de conhecimento por servidor MCP.

A ingestão **DEVE** definir, na especificação da Api.RAG, critérios de segmentação, deduplicação, reprocessamento e atualização antes da implementação correspondente.

Manual e documento ingerido **DEVEM** possuir origem, identificação e versão. Cada chunk **DEVE** permitir recuperar sua fonte e, quando a fonte tiver localização, essa localização. Modelo de embeddings, dimensão dos vetores e estratégia de busca **DEVEM** ser compatíveis e documentados no plano técnico.

### 3.5 Integração e retentativa

A política de retentativa de chamada a LLM, MCP e ferramenta externa **DEVE** ser limitada e definida no plano técnico antes da implementação. Retentativa **NÃO DEVE** ser ilimitada. Resposta inválida do LLM **DEVE** seguir essa política e, esgotada a política, **DEVE** resultar em falha técnica, não em resposta parcialmente aceita.

Operação externa **DEVE** ter timeout. Timeout e falha **DEVEM** ser registrados na trilha da execução.

## 4. Contratos e governança dos dados

### 4.1 Entrada

A especificação do contrato de entrada **DEVE** definir tipos de entrada aceitos, tamanho, formato, integridade e condições de processamento. A implementação **NÃO DEVE** aceitar tipo não especificado.

A entrada **DEVE** permitir expressar ao menos estas intenções: sugerir NCM, auditar o declarado, sugerir tratamento tributário, sugerir o XML do item, consultar caso corporativo e documentação técnica.

Contexto obrigatório da execução: usuário, empresa, papel, finalidade, UF, regime tributário, tipo de operação e data de vigência. Item com contexto próprio **DEVE** fixar vigência, UF, regime, origem, destino e tipo de operação daquele item.

### 4.2 Saída

A especificação **DEVE** versionar:

- o contrato JSON da saída estruturada do LLM;
- o envelope da API.

O envelope **DEVE** carregar o estado da nota, o resultado por item e a classe de cada achado, conforme o Princípio 3. Por item, quando a análise tiver sido concluída, o envelope **DEVE** poder representar NCM sugerida, alternativas, tratamento de IPI, ICMS, CEST, PIS/COFINS e IBS/CBS, XML somente daquele item, casos corporativos relacionados e referências técnicas.

Campo sugerido e bloqueio **DEVEM** apontar evidência recuperável. Validação não realizada **DEVE** aparecer de forma explícita no resultado.

### 4.3 Conhecimento

Resposta fundamentada em RAG **DEVE** apresentar referências recuperáveis até a fonte versionada e, quando existir, até a localização do chunk.

A Api.RAG **DEVE** recusar ingestão sem origem, identificação e versão do documento. Atualização de documento **DEVE** produzir nova versão consultável ou substituir a versão anterior pelo critério de atualização já especificado. A implementação **NÃO DEVE** misturar chunks de versões diferentes como se fossem a mesma vigência.

Caso corporativo confirmado **DEVE** guardar a decisão, a autoridade que confirmou, a data e o vínculo com a execução de origem. Sugestão não confirmada **NÃO DEVE** ser recuperada como precedente.

### 4.4 Memória, consumo e trilha

Memória conversacional **DEVE** usar MongoDB ou Redis. A escolha **DEVE** ser registrada em ADR, na etapa de plano técnico, antes de qualquer implementação de memória.

Consumo e custos **DEVEM** usar PostgreSQL próprio da Api.Agente, distinto do PostgreSQL de vetores da Api.RAG.

A trilha de auditoria **DEVE** permitir reconstruir, pelo identificador da execução: versões das bases, evidências usadas, ferramentas executadas, validações realizadas, validações não realizadas e resultado entregue.

Dado sensível **DEVE** ter proteção definida no plano técnico antes da persistência de memória, trilha ou consumo. A constituição não define o mecanismo criptográfico.

### 4.5 Propriedade

Api.Agente é proprietária de execução, trilha, memória conversacional e consumo.

Api.RAG é proprietária de documentos, chunks, embeddings e índices vetoriais.

Nenhuma das duas **DEVE** ler ou escrever o armazenamento interno da outra fora do contrato MCP ou da API REST de administração explicitamente especificada para a Api.RAG.

## 5. Segurança e qualidade

Autenticação e autorização **DEVEM** ser aplicadas aos endpoints das duas APIs e à execução de ferramentas. A especificação de segurança **DEVE** definir papéis antes da implementação dos endpoints protegidos.

Segredo, connection string e chave de provedor **NÃO DEVEM** ser gravados no repositório. **DEVEM** ser fornecidos por configuração do ambiente.

Conteúdo de documento fiscal usado em log **DEVE** ser limitado ao necessário para correlação. Log **NÃO DEVE** ser substituto da trilha de auditoria.

Teste automatizado **DEVE** acompanhar validador determinístico, contrato de saída, autorização de ferramenta, fronteira MCP e registro de consumo. Funcionalidade sem teste para seus critérios de aceite **NÃO DEVE** ser considerada pronta.

A revisão de pull request **DEVE** verificar os princípios da seção 2 atingidos pela mudança. Violação de **DEVE** ou **NÃO DEVE** **DEVE** impedir o merge até correção ou até registro de desvio aprovado na seção 8.

Ambientes de desenvolvimento, teste e produção **DEVEM** poder usar configuração distinta de provedor, modelo, bases e persistência. Código de domínio **NÃO DEVE** assumir um único ambiente.

Dado simulado **PODE** existir apenas em teste. Desenvolvimento e produção **NÃO DEVEM** usar base fiscal simulada como se fosse legislação vigente.

## 6. Fluxo de desenvolvimento SDD

Toda funcionalidade **DEVE** seguir esta ordem:

1. Constituição vigente.
2. Especificação, com critérios de aceite verificáveis.
3. Esclarecimentos das decisões que a especificação encontrar em aberto.
4. Plano técnico, incluindo ADR das decisões arquiteturais da funcionalidade.
5. Tarefas rastreáveis até os requisitos.
6. Implementação limitada às tarefas e à especificação aprovadas.
7. Validação contra os critérios de aceite e contra esta constituição.

Implementação **NÃO DEVE** começar enquanto a especificação da funcionalidade depender de decisão pendente da seção 9 que a afete. Esclarecimento **DEVE** atualizar a especificação; **NÃO DEVE** permanecer apenas na conversa.

A especificação **NÃO DEVE** inventar layout, regra fiscal, schema ou critério de conformidade. Quando a fonte oficial ainda não tiver sido incorporada, o item **DEVE** permanecer em aberto.

O plano técnico **DEVE** registrar versões de SDK, parâmetros ajustáveis, topologia de persistência e política de retentativa. Esses detalhes **NÃO DEVEM** ser promovidos a princípio constitucional só por constarem do plano.

Cada tarefa **DEVE** indicar o requisito que cumpre e o teste que demonstra o cumprimento.

## 7. Definition of Done

Uma funcionalidade só está pronta quando todos os itens abaixo forem verdadeiros:

1. A especificação aprovada contém critérios de aceite verificáveis e a rastreabilidade até tarefas e testes.
2. Decisões pendentes que afetam a funcionalidade foram resolvidas na etapa indicada na seção 9, ou o escopo foi reduzido para não depender delas.
3. O fluxo da seção 3.3 aplicável à funcionalidade é cumprido, incluindo interrupção por dado essencial e bloqueio de nota por item divergente.
4. Validadores determinísticos cobrem as regras estruturais e de negócio implementadas, com teste automatizado.
5. A saída do LLM é validada contra o contrato versionado antes de chegar ao consumidor.
6. Evidência sugerida ou usada em bloqueio é recuperável até a fonte e a vigência.
7. Consumo da funcionalidade segue o Princípio 10, inclusive retentativas.
8. Ferramentas novas seguem o Princípio 7.
9. A fronteira de dados do Princípio 8 permanece intacta.
10. A revisão de pull request confirma ausência de violação desta constituição, ou existe desvio aprovado e vigente.
11. Nenhum layout, regra fiscal ou schema foi introduzido sem fonte e sem registro na especificação.

## 8. Governança e alterações

Esta constituição é o artefato normativo duradouro do projeto. Detalhe de implementação, versão de SDK e parâmetro ajustável pertencem ao plano técnico.

Alteração desta constituição **DEVE** conter:

- nova versão semântica;
- data da alteração;
- descrição do que mudou;
- avaliação de impacto nas especificações existentes;
- responsável pela alteração.

A versão **DEVE** evoluir assim:

- **MAJOR**: remoção ou redefinição incompatível de princípio, fluxo obrigatório ou fronteira entre Api.Agente e Api.RAG.
- **MINOR**: inclusão de princípio ou ampliação material de obrigação.
- **PATCH**: esclarecimento, correção de redação ou refinamento que não muda a obrigação.

Desvio temporário ou excepcional **DEVE** registrar justificativa, impacto, responsável, aprovação e prazo de revisão. Desvio vencido **NÃO DEVE** ser usado como precedente. Desvio **NÃO DEVE** autorizar fabricar evidência, acessar dados internos do outro serviço ou declarar conformidade fiscal.

A revisão de conformidade com esta constituição **DEVE** ocorrer em todo pull request que altere comportamento do agente, contrato, persistência, ferramenta, prompt ou ingestão de conhecimento.

### Registro de alterações

#### 1.0.1 — 2026-10-29

- A DP-05 foi resolvida pela specification `specs/001-execucao-analise-fiscal/spec.md`.
- A DP-07 foi resolvida apenas quanto aos tipos de entrada. Tamanho máximo e limites numéricos continuam em aberto.
- Impacto: essa specification passa a ser a fonte da ordem de autoridade e da lista de entradas. Não havia outra specification.
- Responsável: mantenedor do produto, na sessão de especificação.

#### 1.0.2 — 2026-10-29

- A DP-06 foi resolvida apenas quanto ao limiar de confiança: menor que 70 em escala de 0 a 100, na specification `specs/001-execucao-analise-fiscal/spec.md`.
- A definição de impacto relevante continua em aberto.
- Impacto: o plano técnico deve calcular a confiança nessa escala. Não havia outra specification além da `001`.
- Responsável: mantenedor do produto, na sessão de especificação.

#### 1.0.3 — 2026-10-29

- A DP-10 foi resolvida pelo contrato `specs/001-execucao-analise-fiscal/contracts/analise-v1.openapi.yaml`.
- A DP-13 foi resolvida quanto a tempo limite e retentativa no plano da specification 001. O identificador do modelo continua em configuração, sem nome fixo.
- Impacto: o plano e o contrato da execução da análise passam a ser a fonte desses itens. As demais specifications ainda não existem.
- Responsável: mantenedor do produto, na sessão de especificação.

#### 1.0.4 — 2026-10-29

- A DP-04 foi resolvida apenas quanto à autoridade e ao limite da correção, na specification `specs/002-confirmacao-caso-corporativo/spec.md`. A autoridade é o papel informado na confirmação, sem catálogo fixo de cargos. A correção só pode usar código presente nas evidências da execução. O caso vale somente para a empresa dessa execução.
- A gravação do precedente continua proibida até a specification da Api.RAG.
- Impacto: a specification 001 não passa a gravar caso. A specification 002 publica o contrato de confirmação sem persistir precedente.
- Responsável: mantenedor do produto, na sessão de especificação.

#### 1.0.5 — 2026-10-29

- A DP-11 foi resolvida quanto ao comportamento da ingestão, na specification `specs/003-base-conhecimento-rag/spec.md`. Versões convivem e a consulta por data não as mistura. O mesmo hash é ignorado. Conteúdo diferente na mesma versão é recusado.
- A DP-04 passa a permitir gravar o caso confirmado na Api.RAG. A ferramenta MCP atual não devolve esse caso, porque não recebe a empresa.
- Os números de chunk, embedding e busca continuam na DP-12, para o plano técnico.
- Impacto: a specification 003 é a fonte desses critérios. A specification 002 não passa a gravar o caso. A Api.Agente não acessa o banco da Api.RAG.
- Responsável: mantenedor do produto, na sessão de especificação.

## 9. Decisões pendentes

Enquanto uma decisão abaixo não for resolvida na etapa indicada, a implementação **NÃO DEVE** transformá-la em comportamento fixo.

| ID | Decisão em aberto | Etapa de resolução | Restrição até a resolução |
| --- | --- | --- | --- |
| DP-01 | Armazenamento de memória conversacional: MongoDB ou Redis. | Plano técnico, por ADR, antes da implementação de memória. | Não implementar memória. |
| DP-02 | Política de retenção, expiração, exclusão e proteção de dados sensíveis da memória, da trilha e do consumo. | Plano técnico, por ADR, antes da persistência correspondente. | Não persistir conteúdo sensível com política implícita. |
| DP-03 | Aplicação do isolamento por organização e papéis de acesso. | Esclarecimentos e especificação de segurança. | Não assumir cenário multiorganização nem papel não especificado. |
| DP-04 | Autoridade e correção resolvidas na specification `002`. A gravação do caso confirmado está na specification `003`. A consulta MCP atual não devolve esse caso, porque não recebe a empresa. | Contrato MCP com empresa, antes de o agente recuperar o caso | Não devolver caso corporativo em `consultar_conhecimento` enquanto a chamada não informar a empresa. |
| DP-05 | Resolvida em `specs/001-execucao-analise-fiscal/spec.md`. Ordem: legislação vigente, notas e Regras Gerais de Interpretação, caso corporativo confirmado, texto do fornecedor. | Especificação da análise | Seguir essa ordem. Empate na mesma faixa pede revisão humana. |
| DP-06 | Limiar resolvido na specification `001`: confiança baixa é menor que 70 em escala de 0 a 100. Impacto relevante segue em aberto. | Esclarecimento da análise, com exemplos reais de impacto | Não inventar o que é impacto relevante. Calcular confiança na escala decidida. |
| DP-07 | Tipos resolvidos na specification `001`: descrição textual, XML de NF-e dos itens, planilha, PDF, laudo e ficha técnica. Tamanho máximo e limites numéricos seguem em aberto. | Esclarecimento do contrato de entrada | Não aceitar tipo fora dessa lista e não inventar limite numérico. |
| DP-08 | Layouts, versões, schemas XSD e validadores por tipo de documento, obtidos de fonte oficial. | Especificação documental, antes do validador correspondente. | Não inventar schema nem aplicar XSD sem schema registrado. |
| DP-09 | Regras fiscais, tabelas vigentes, alíquotas e critérios de enquadramento de NCM, IPI, ICMS, CEST, PIS/COFINS e IBS/CBS. | Especificação de conhecimento e de tratamento, com fonte e vigência. | Não codificar regra fiscal sem essa especificação. |
| DP-10 | Resolvida no contrato `analise-v1.openapi.yaml` da specification 001. | Especificação da execução do agente | Publicar a resposta no contrato versão 1.0.0. |
| DP-11 | Comportamento resolvido na specification `003`: versões convivem, o mesmo hash é ignorado e conteúdo diferente na mesma versão é recusado. O tamanho numérico do chunk permanece na DP-12. | Plano técnico da Api.RAG | Não inventar tamanho de chunk nem misturar versões na mesma vigência. |
| DP-12 | Modelo de embeddings, dimensão, estratégia de busca, topK e limiar, avaliados com exemplos representativos. | Plano técnico da Api.RAG. | Não fixar esses parâmetros como padrão de produção. |
| DP-13 | Tempos e retentativa resolvidos no plano da specification 001. O nome do modelo permanece configuração obrigatória. | Plano técnico da Api.Agente | Não gravar nome de modelo no código de domínio. |
| DP-14 | Tabela de preços, moeda e regra de estimativa de custo. | Plano técnico de consumo. | Não gravar custo estimado sem referência da tabela. |
| DP-15 | Operações expostas pelo servidor A2A e seu contrato. | Especificação de integração, antes da exposição A2A. | Não expor operação A2A não especificada. |
| DP-16 | Projetos ou bibliotecas além de Api.Agente e Api.RAG. | Plano técnico, com justificativa de responsabilidade. | Não criar projeto adicional sem essa justificativa. |
| DP-17 | Versões dos SDKs de ASP.NET Core, Semantic Kernel, cliente MCP, servidor A2A e PgVector. | Plano técnico. | Não tratar versão de SDK como princípio constitucional. |
