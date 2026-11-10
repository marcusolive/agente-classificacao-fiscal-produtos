# Checklist de publicação (Vercel Hobby + Neon Free)

Passos para publicar a **Api.Agente** e a **Api.RAG** e alimentar o RAG.
Baseado na skill `sofia-devops-free-cloud` e na spec `005-publicacao-cloud`.

> **A skill não faz deploy.** Ela orienta e gera arquivos; conta, deploy e
> credenciais são manuais. Nada aqui declara deploy concluído sem URL pública,
> log e teste do operador (FR-006).
>
> **Fora de escopo agora:** MongoDB Atlas. A memória conversacional depende da
> DP-01 (não decidida) e o app não lê Mongo. Só precisamos de **Neon + Vercel**.

---

## 0. Segurança (bloqueia tudo)

- [ ] **Rotacionar** a chave da OpenAI que esteve em texto puro nos `appsettings` — considere-a comprometida.
- [ ] **Remover** a chave dos 8 `appsettings*.json`; ela passa a vir só do ambiente (`OpenAI__ApiKey`).
- [ ] Confirmar que nenhum segredo está em Git, Dockerfile, `appsettings` versionado ou log (FR-004).

## 1. Definições (padrões de mercado)

Valores não sensíveis já estão em [`.env.example`](../.env.example). Os segredos ficam em branco e vão no painel.

| Variável | Valor | Nota |
| --- | --- | --- |
| `Embedding__Model` | `text-embedding-3-small` | embedding mais barato da OpenAI |
| `Embedding__Dimensao` | `1536` | dimensão nativa → `vector(1536)` no Neon |
| `Embedding__TopK` | `5` | evidências por consulta (DP-12) |
| `Embedding__Limiar` | `0.5` | similaridade mínima de cosseno (DP-12) |
| `Embedding__TamanhoChunk` | `2000` | caracteres (~500 tokens) (DP-12) |
| `Embedding__Sobreposicao` | `200` | ~10% de overlap (DP-12) |
| `Ingestao__TamanhoMaximoBytes` | `10485760` | 10 MB de upload |
| `Ingestao__Chave` | gerar 32+ bytes | segredo; `X-Chave-Ingestao` |
| `OpenAI__Model` | sua escolha (ex.: `gpt-4o-mini`) | LLM de análise (DP-13) |

> **DP-12 / DP-13:** `Dimensao`, `TopK`, `Limiar`, `TamanhoChunk` e o modelo de LLM são
> ponto de partida de mercado, **não** padrão de produção. A constituição pede avaliá-los
> com exemplos reais de classificação antes de fixar.

## 2. Neon Free (PostgreSQL + pgvector)

- [ ] Criar projeto Neon (Free), região próxima.
- [ ] Criar duas bases: `agente` e `rag`.
- [ ] Na base `rag`, no SQL Editor:
  ```sql
  CREATE EXTENSION IF NOT EXISTS vector;
  SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';
  ```
- [ ] Copiar as duas connection strings com `SSL Mode=Require` → `ConnectionStrings__Agente` e `ConnectionStrings__Rag` (fora do Git).
- [ ] A coluna `vector(1536)` nasce na 1ª subida da Api.RAG com `Embedding__Dimensao` preenchida. Confirmar que a dimensão bate com o modelo.

## 3. Build local das imagens (antes da nuvem)

```powershell
docker build -f Dockerfile.vercel -t agente-vercel:local .
docker run --rm -e PORT=8080 -p 8080:8080 agente-vercel:local
# Agent Card: http://localhost:8080/.well-known/agent-card.json

docker build -f Dockerfile.rag.vercel -t rag-vercel:local .
docker run --rm -e PORT=8080 -p 8081:8080 rag-vercel:local
# Swagger: http://localhost:8081/swagger
```

- [ ] `dotnet test` verde nas duas APIs.
- [ ] As duas imagens sobem lendo `PORT` e respondem (Agent Card / Swagger).

## 4. Vercel Hobby

- [ ] ⚠️ **Confirmar os termos:** Hobby é pessoal/não-comercial. Uso institucional da INFIS pode exigir plano pago.
- [ ] Importar **dois** projetos do repositório: um com `Dockerfile.vercel` (Agente), outro com `Dockerfile.rag.vercel` (RAG).
- [ ] Colar as variáveis de ambiente de cada projeto (do `.env.example` + segredos).
- [ ] Confirmar que cada serviço escuta `PORT` e obter as URLs públicas.
- [ ] Apontar `Conhecimento__McpUrl` do Agente para `https://<rag>.vercel.app/mcp` e `Sofia__Url` para a URL do Agente.

## 5. Aceite (com o operador)

- [ ] `GET https://<agente>.vercel.app/.well-known/agent-card.json` responde.
- [ ] `POST /a2a` autenticado com `Sofia__Chave` retorna resposta estruturada.
- [ ] Extensão `vector` ativa na base `rag` (passo 2).
- [ ] **Alimentar o RAG:** rodar o carregador da NCM contra a URL pública:
  ```powershell
  $env:INGESTAO_CHAVE = '<chave-de-ingestao>'
  ./tools/carga-ncm/Invoke-CargaNcm.ps1 -BaseUrl https://<rag>.vercel.app -SomenteOitoDigitos -Limite 10
  ```
  Validar 10 códigos; depois rodar a carga cheia (10.516 → 10.516 embeddings, **custo OpenAI**).
- [ ] Consulta MCP por uma vigência coberta devolve evidência; vigência fora do intervalo devolve lista vazia.
- [ ] Segredos não aparecem em Git, log nem resposta HTTP.

## 6. Riscos conhecidos (decidir antes de uso real)

- **Swagger aberto sem auth em produção** — avaliar proteger ou desligar (fora do escopo de `005`).
- **Sem `/health`** — a Vercel sobe sem, mas o aceite da skill pede um endpoint mínimo.
- **Estado de `idSolicitacao` em memória** some no cold start da Function — idempotência de repetição não sobrevive; a execução gravada no Postgres permanece.
- **Autorização REST/A2A (DP-03)** — hoje só há a chave Sofia (A2A) e a chave de ingestão (RAG); a análise REST não tem papel/auth.
- **Chunk só resolvido para textos curtos** — a NCM vai bem com 1 chunk; leis/NESH longos precisam da avaliação de chunk (DP-12).

## 7. Rollback

- Reverter é despublicar/trocar a imagem na Vercel; os dados ficam no Neon.
- Reingestão é idempotente por hash: repetir o carregador não duplica chunk.
