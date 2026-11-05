# ADR 0006 — Busca exata por cosseno

- Status: aceita
- Data: 2026-11-05
- Specification: `specs/003-base-conhecimento-rag/spec.md`

## Decisão

Quando modelo, dimensão, topK e limiar estiverem configurados, a Api.RAG gera embeddings na OpenAI e ordena os chunks pela similaridade de cosseno. O limiar é a similaridade mínima, no intervalo de 0 a 1. A busca é exata: não há índice HNSW nem IVFFlat. A coluna `vector(N)` é criada na subida somente se `Embedding:Dimensao` estiver preenchida e a coluna ainda não existir. Se a coluna existir com outra dimensão, a subida falha. Nome do modelo, dimensão, topK, limiar, tamanho de chunk e sobreposição não entram nos arquivos versionados.

## Contexto

A DP-12 exigia avaliação antes de um padrão de produção. O produto escolheu o provedor, a métrica e a busca exata, e manteve os números fora do repositório até haver exemplos representativos. A primeira migração continua sem coluna de embedding.

## Consequências

Sem qualquer um desses quatro parâmetros, ou com limiar fora de 0 a 1, `consultar_conhecimento` responde erro. Com eles preenchidos e a chamada de embedding concluída, a ferramenta devolve só chunks da vigência pedida, acima do limiar, até o topK, e continua sem caso corporativo. A chave da OpenAI fica no ambiente, em `OpenAI__ApiKey`.
