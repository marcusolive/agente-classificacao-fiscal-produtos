# Plano: publicação experimental

A solução é `Agente.slnx`. Os hosts são `src/Api.Agente/Api.Agente.csproj` (`Api.Agente.dll`) e `src/Api.RAG/Api.RAG.csproj` (`Api.RAG.dll`), os dois em `net10.0`. Não há `vercel.json`. O Compose continua só com os Postgres locais.

1. A imagem da Vercel copia o repositório filtrado pelo `.dockerignore`, publica um csproj e sobe com `ASPNETCORE_URLS=http://0.0.0.0:$PORT`.
2. A Api.Agente lê `ConnectionStrings:Agente`, `Conhecimento:McpUrl`, `OpenAI:ApiKey`, `OpenAI:Model` e `Sofia:*`.
3. A Api.RAG lê `ConnectionStrings:Rag`, `Ingestao:Chave`, `Embedding:*` e `OpenAI:ApiKey`.
4. Sem a connection string, a API usa repositório em memória. Isso serve ao teste local da imagem. Na nuvem, a string do Neon entra só no painel.
5. A base `rag` recebe `CREATE EXTENSION IF NOT EXISTS vector` na subida. A coluna `vector(N)` só nasce quando `Embedding:Dimensao` estiver definida. Não há índice ANN.
6. Atlas fica de fora até a DP-01. Não há variável `MongoDb` para preencher.

## Roteiro manual

1. Criar um projeto Neon Free e duas bases, `agente` e `rag`. Copiar as connection strings com `SSL Mode=Require`.
2. Na base `rag`, a primeira subida da API cria a extensão. Conferir com `SELECT extname FROM pg_extension WHERE extname = 'vector';`.
3. Importar o repositório duas vezes na Vercel Hobby. O projeto da Api.Agente usa `Dockerfile.vercel`. O da Api.RAG só usa `Dockerfile.rag.vercel` se o painel permitir escolher o arquivo. Sem esse campo, a RAG não sobe pela detecção automática.
4. Colar as variáveis do `.env.example` no painel. `Conhecimento__McpUrl` é a URL HTTPS da RAG terminada em `/mcp`.
5. Abrir `/.well-known/agent-card.json` na URL pública. `POST /a2a` sem `Sofia__Chave` responde 401.
6. Ingerir um documento de teste na RAG e repetir a consulta. Sem `Embedding:*` e sem chave OpenAI, o documento fica `consultavel: false`.

## Rollback

Apagar o deployment na Vercel não apaga o Neon. Para voltar o código, reverter o commit desta feature. As bases locais do Compose não entram nesse rollback.

## Limites

Hobby é pessoal e não comercial. A Function desliga sem tráfego e cada pedido tem teto de duração. Neon Free suspende o compute parado e limita disco e transferência. OpenAI cobra token à parte. Preço e cota mudam. Conferir no dia do deploy: documentação de Hobby, Container Runtime, planos do Neon e da extensão `vector`.
