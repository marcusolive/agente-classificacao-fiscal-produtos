# Roteiro de publicacao - SOFIA (demo/treinamento)

## 1. Preflight

- Identificar `.sln`, projeto host da API (`.csproj`) e seu assembly `.dll`.
- Verificar `TargetFramework=net10.0`, dependencias nativas e build multi-stage Linux.
- Detectar background services, memory-local session, arquivos gravados, stream A2A longo e job de ingestao lento. Vercel Functions podem encerrar execucoes conforme timeout; processar jobs demorados fora da request.
- Incluir `.dockerignore` com `**/bin/`, `**/obj/`, `.git/`, `.env*` (preservar `.env.example` se precisar), `.vs/`, `TestResults/`.
- Nunca expor Swagger em producao sem avaliar autorizacao. Health endpoint deve ser minimo e nao vazar segredos.

## 2. Vercel Hobby + ASP.NET Core por container

- A Vercel suporta containers HTTP via `Dockerfile.vercel` (Container Runtime beta). Nao existe runtime .NET oficial do mesmo tipo que Node.js/Python; **Docker e a rota indicada aqui**.
- Colocar o `Dockerfile.vercel` na raiz do projeto importado. Alterar `ARG PROJECT_PATH` e `ENV APP_DLL` para valores reais.
- O entrypoint do template faz o Kestrel escutar em `0.0.0.0:$PORT`; nao fixar host na URL localhost.
- Com repo no GitHub: importar projeto na Vercel, revisar Root Directory (caminho com Dockerfile e contexto da solucao), definir envs, publicar no Hobby se a finalidade do plano permitir, testar HTTPS. CLI opcional: `npm i -g vercel`, `vercel login`, `vercel` e `vercel --prod`.
- Validar GET `/health` e Agent Card no endereco publicado; validar POST REST e chamada A2A real. Testar rede para Atlas/Neon, cold start, limite de memoria e duracao. Logs e links de implantacao sao evidencias, nao simulacoes.
- A Function nao deve depender de estado em disco ou processos residentes; manter historico no Mongo e dados no Postgres. Reuso de conexoes: singleton MongoClient, NpgsqlDataSource apropriado.
- Confirmar Termos do plano Hobby: uso pessoal/nao comercial. Projeto institucional pode exigir plano pago ou alternativa.

## 3. MongoDB Atlas Free

1. Criar projeto Atlas e cluster **Free** na regiao disponivel proxima da aplicacao.
2. Criar usuario de banco com permissao minima. Nunca usar a conta de usuario web como login de banco.
3. Configurar Network Access: allowlist de egress conhecido. Vercel Hobby pode ter IP dinamico; na ausencia de IP estatico, avaliar o risco da abertura de acesso por IP e autenticar com credenciais fortes/TLS; solicitar aprovacao, ou preferir hospedagem com egress fixo. Evitar afirmar que a operacao esta segura so por usar CORS.
4. Obter SRV URL de conexao e colocar em secreto na Vercel. Escapar caracteres especiais na senha conforme URI.
5. Rodar testes de ping, gravacao e leitura do historico; estabelecer indices `idExecucao`, `usuarioId`, timestamps ou nomes reais do dominio.
6. Plano Free pode limitar storage (documentacao em 2026 indica 0,5 GB), ops/s, transferencia, conexoes e um cluster por projeto. Validar limites no painel. Dados sensiveis da INFIS exigem controles de LGPD e politica institucional; nao inserir documentos reais em ambiente didatico aberto.

## 4. Neon Free + extensao pgvector

1. Criar projeto PostgreSQL no Neon (plano Free). Guardar host, porta, dbname, usuario, senha fora do Git.
2. Abrir SQL Editor e executar:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';
```

3. Verificar tabelas RAG existentes. Uma coluna de embedding deve bater com dimensao do modelo configurado (exemplo: `vector(1536)` para `text-embedding-3-small` com dimensao padrao; verificar se foi configurada outra dimensao).
4. Exemplo SQL **apenas para uma tabela nova**; nao aplicar cegamente em esquema existente:

```sql
CREATE TABLE IF NOT EXISTS demo_conhecimento (
  id BIGSERIAL PRIMARY KEY,
  texto TEXT NOT NULL,
  embedding VECTOR(1536) NOT NULL
);
```

5. Se indice HNSW fizer sentido para volume e testes, criar depois com opclass coerente com consulta:

```sql
CREATE INDEX IF NOT EXISTS ix_demo_conhecimento_embedding
ON demo_conhecimento USING hnsw (embedding vector_cosine_ops);
```

6. No Npgsql configurar SSL `SSL Mode=Require` e tratar certificados conforme orientacao oficial. Aplicar migrations/SQL somente com backup e revisao. Testar ingestao, consulta cosine e topK=3 usado pela SOFIA.
7. Free tem limites de compute, egress e storage; pausas/cold start sao normais. Revalidar quotas em https://neon.com/pricing .

## 5. Checklist de aceite

- [ ] `dotnet test` passou.
- [ ] Build do container passou localmente.
- [ ] HTTP GET `/health` em URL publica (se endpoint existir).
- [ ] Agent Card publico tem URL/transport coerentes.
- [ ] SOFIA envia `idExecucao`, nome do solicitante, pergunta e documentos em texto sem expor segredos.
- [ ] Agente retorna resposta estruturada e SOFIA correlaciona resultados.
- [ ] Atlas armazena memoria/historico e executa leitura de volta.
- [ ] Neon possui extensao vector e recupera documentos por similaridade.
- [ ] Segredos nao aparecem no Git, log ou resposta HTTP.
- [ ] Politica de acesso e autenticacao A2A/REST validadas.
- [ ] Timeout, cold start e limites testados; custos de OpenAI documentados.
- [ ] Backup e rollback preparados.

## Alternativa se Vercel nao for adequada

Render Free suporta .NET via Docker e Web Service HTTP, mas pode suspender apos 15 minutos inativo e tem cotas. Nao usar Render Postgres Free para banco de longa duracao sem considerar prazo de expiracao (docs descrevem 30 dias). Hospedar Postgres/pgvector no Neon e Mongo no Atlas. Nao prometer Render como ambiente de producao confiavel ou tempo de resposta estavel.
