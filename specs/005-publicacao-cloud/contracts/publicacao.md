# Contrato operacional da publicação

Isto não muda o OpenAPI. As rotas atuais permanecem.

## Variáveis

A Api.Agente lê `ConnectionStrings:Agente`, `Conhecimento:McpUrl`, `OpenAI:ApiKey`, `OpenAI:Model`, `Sofia:Chave` e `Sofia:Url`.

A Api.RAG lê `ConnectionStrings:Rag`, `Ingestao:Chave`, `OpenAI:ApiKey`, `OpenAI:Endpoint`, `Embedding:Model`, `Embedding:Dimensao`, `Embedding:TopK` e `Embedding:Limiar`.

Não existe leitura de MongoDB.

## Aceite local desta feature

- A imagem da Api.Agente, com `PORT=8080` e sem connection string, responde o Agent Card.
- A imagem da Api.RAG, com `PORT=8080` e sem connection string, responde o Swagger.
- O repositório não contém senha, chave ou connection string de nuvem.

## Aceite que fica com o operador

URL pública, Neon com `vector`, ingestão consultável e chamada A2A autenticada. Sem esses testes, o deploy não está concluído.
