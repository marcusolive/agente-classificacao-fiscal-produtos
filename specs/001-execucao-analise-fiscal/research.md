# Pesquisa: Execução da análise fiscal de itens

## Decisão 1 — Resposta na mesma solicitação

**Decisão**: `POST /analises` devolve o resultado completo. `GET /analises/{execucaoId}` devolve a execução persistida.

**Racional**: Os critérios de aceite observam o resultado da análise. Os dois acessos cumprem a specification sem uma fila.

**Alternativas consideradas**: Processamento apenas assíncrono. Foi adiado porque o tamanho máximo do arquivo ainda não existe e a fila não ajuda os cenários de texto.

## Decisão 2 — PostgreSQL exclusivo da Api.Agente

**Decisão**: Execução, itens, achados e evidências citadas ficam no PostgreSQL da Api.Agente.

**Racional**: O resultado precisa sobreviver ao processo e continuar propriedade da Api.Agente.

**Alternativas consideradas**: Memória do processo, MongoDB e Redis. Memória perde o identificador no reinício. MongoDB e Redis permanecem em aberto só para memória conversacional, que está fora desta fatia.

## Decisão 3 — Confiança determinística

**Decisão**: Usar a tabela da ADR 0002.

**Racional**: Uma nota pedida ao modelo não se repete e não se explica em revisão.

**Alternativas consideradas**: Confiança informada pelo modelo. Rejeitada para esta fatia.

## Decisão 4 — Porta de conhecimento

**Decisão**: Interface de aplicação com adaptador MCP e dublê de teste. Api.RAG não é criada agora.

**Racional**: A fronteira da constituição é respeitada sem implementar ingestão nesta fatia.

**Alternativas consideradas**: Projeto Api.RAG vazio. Rejeitado porque não teria responsabilidade nesta specification.

## Decisão 5 — Arquivo sem limite configurado

**Decisão**: Descrição textual segue sem arquivo. Entrada com arquivo é recusada, sem bloqueio fiscal, enquanto `Analise:TamanhoMaximoBytes` não estiver configurado. Com o valor configurado, arquivo maior é recusado do mesmo modo.

**Racional**: A specification proíbe inventar o limite. Recusar o arquivo é mais seguro do que aceitar qualquer tamanho.

**Alternativas consideradas**: Adotar 10 MB ou 20 MB. O produto pediu para deixar o número em aberto.

## Decisão 6 — Impacto relevante

**Decisão**: Nenhum item é marcado com impacto relevante nesta fatia. O campo existe no contrato e permanece falso.

**Racional**: Não há exemplos reais. Marcar impacto por troca de NCM ou de tributo contradiz o esclarecimento.

**Alternativas consideradas**: Tratar toda troca de NCM como impacto. Foi oferecida e não foi escolhida.

## Decisão 7 — Saída do modelo e retentativa

**Decisão**: O modelo recebe apenas evidências e o contrato reduzido do item. No máximo duas novas tentativas depois de uma saída inválida. Esgotadas, o item termina em falha técnica. Tempo limite inicial: 60 segundos para o modelo e 15 segundos para o MCP. Os três valores vêm de configuração.

**Racional**: A constituição exige política limitada e proíbe número ilimitado no domínio.

**Alternativas consideradas**: Aceitar JSON parcial do modelo. Rejeitada pelo FR-024.

## Decisão 8 — Modelo e pacotes

**Decisão**: O identificador do modelo é configuração obrigatória, sem valor fiscal embutido no código. Os pacotes previstos são `Microsoft.SemanticKernel`, `Microsoft.SemanticKernel.Connectors.OpenAI`, `ModelContextProtocol` e `Npgsql`. A versão exata entra no primeiro commit de código, no gerenciador central de pacotes.

**Racional**: A versão de SDK não é princípio constitucional, mas precisa ficar registrada antes da implementação. Fixar um número desatualizado aqui seria uma suposição.

**Alternativas consideradas**: Escolher um nome de modelo no repositório. Rejeitado porque o ambiente define o modelo disponível.
