# Quickstart: publicação experimental

Na raiz do repositório, sem segredo:

```powershell
docker build -f Dockerfile.vercel -t agente-vercel:local .
docker run --rm -e PORT=8080 -p 8080:8080 agente-vercel:local
```

O Agent Card local fica em `http://localhost:8080/.well-known/agent-card.json`.

A segunda imagem:

```powershell
docker build -f Dockerfile.rag.vercel -t rag-vercel:local .
docker run --rm -e PORT=8080 -p 8081:8080 rag-vercel:local
```

O Swagger local fica em `http://localhost:8081/swagger`.

O Compose local não muda: `docker compose up -d` continua subindo só os Postgres da máquina.

Conta, painel e deploy ficam manuais. As chaves vazias estão em `.env.example`.
