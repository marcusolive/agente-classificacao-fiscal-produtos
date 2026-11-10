---
name: sofia-devops-free-cloud
description: Orientar DevOps e publicar APIs ASP.NET Core .NET 10 da SOFIA/INFIS e dos agentes especializados em Vercel Hobby via container Docker serverless, MongoDB Atlas Free e PostgreSQL com pgvector no Neon Free. Usar quando preparar deployment, migrar bancos Docker locais para cloud, configurar secrets, GitHub CI/CD, validar A2A/REST e health checks, ou planejar publicacao gratuita com Spec Kit em projeto .NET existente.
---

# SOFIA / INFIS - DevOps gratuito, .NET 10, Atlas e pgvector

## Regras obrigatorias

1. Inspecionar a solucao existente (.sln, .csproj, Dockerfile, Program.cs, appsettings, EF/Dapper, MongoDB, modelos de embedding, contratos A2A), e **nunca recriar a API**. Identificar o csproj e DLL reais antes de adaptar os templates em `assets/`.
2. Tratar **Vercel Container Runtime (beta)** como Function serverless, e nao VM/servico sempre ativo. Desde 2026, Dockerfile.vercel permite containers HTTP. Conferir se a conta Hobby e a regiao comportam os limites atuais antes de publicar. Nao prometer que toda aplicacao ASP.NET Core/A2A funcionara sem adaptacao. Sem processos persistentes, workers residentes ou estado local duravel. Transacoes e streams longos podem atingir timeouts; medir.
3. **Atlas Free** hospeda MongoDB (historico/memoria). **Neon Free** hospeda Postgres com extensao `vector` (RAG/embeddings). O `pgvector` e uma extensao SQL e nao um servidor a publicar separadamente.
4. Jamais inserir credenciais em Git, SKILL.md, appsettings versionados, Dockerfile ou logs. Usar variaveis de ambiente no provedor, secrets no desenvolvimento e rotacao se vazadas. Configurar TLS/SSL e restricoes de rede. Nao sugerir `0.0.0.0/0` como configuracao definitiva do Atlas; se IPs de saida dinamicos impedirem allowlist restrita, explicar risco e exigir aprovacao explicita e controles compensatorios.
5. Conservar rotas REST, Agent Card, envelope/protocolo A2A, formato de resposta e IDs de correlacao. Nao converter A2A em um POST JSON proprietario. Testar descoberta, POST A2A e respostas estruturadas em enderecos publicos.
6. CORS e politica de navegador; **nao** substitui autorizacao entre backends da SOFIA e agentes. Proteger endpoints REST/A2A com credenciais, rate limiting, HTTPS e permissao minima.
7. Nao executar deploy real, criar contas, cobrar, solicitar senhas no chat ou modificar recursos remotos sem autorizacao e credenciais seguras. Gerar arquivos e comandos reproduziveis para o usuario executar.
8. Qualificar sempre os termos `gratuito`: limites, suspensao, condicoes e alteracoes de preco variam. Vercel Hobby e voltado a uso pessoal/nao comercial; confirmar se uso corporativo INFIS e permitido. Usar plano adequado para projeto institucional/produçao. Custos OpenAI/LLM, dominios, egress e terceiros nao estao incluidos.

## Sequencia de execucao (Spec Kit / projeto existente)

1. Mapear runtime, versoes, projetos, dependencias e requisitos. Gerar relatorio de compatibilidade: `[OK / Adaptacao / Bloqueio]`. Se API requer processamento longo, conexoes persistentes, WebSockets/streaming ou background workers, avisar e propor Render Free via Docker para demonstracao (suspende apos inatividade) ou servico sempre ativo adequado, sem ocultar limitacoes.
2. Criar/atualizar feature `specs/NNN-publicacao-cloud/`: `spec.md`, `plan.md`, `tasks.md`, `contracts/` com criterios de aceite; respeitar `constitution.md` e Skills A2A/contrato/CORS existentes. Nao alterar codigo fora do escopo sem justificativa.
3. Gerar `Dockerfile.vercel` a partir de `assets/Dockerfile.vercel.template`, preencher `<CAMINHO_CSPROJ>` e `<NOME_DLL>`. Ajustar `.dockerignore`. Manter `Dockerfile` tradicional se ja existir.
4. Testar `dotnet restore`, `dotnet test`, `dotnet publish`; compilar imagem localmente, passar `PORT=8080`, chamar `/health` (se ja existe) e Agent Card. Introduzir health check simples somente se aprovado.
5. Provisionar **Atlas Free** com usuario de BD minimo, network access, `mongodb+srv://...` e TLS; executar teste de leitura/escrita e configurar limpeza/retencao de dados. Ver `references/deploy-guide.md`.
6. Provisionar **Neon Free**, copiar a connection string com `sslmode=require`, executar `CREATE EXTENSION IF NOT EXISTS vector;`, validar dimensao dos embeddings e migrar esquema/dados sem perda. Nao assumir que o SQL local com roles/extensoes/DDL funcionara sem revisao.
7. Configurar Vercel Hobby: importar repositorio, escolher container via `Dockerfile.vercel`, definir variaveis de ambiente, verificar `PORT` e URL publica. Sempre validar `vercel.json` existente antes de editar. Nao declarar o deploy completo sem testes e logs comprovatorios.
8. Testar E2E de SOFIA -> agentes A2A, historico no Atlas, ingesao/consulta RAG no Neon, retries/timeout/idempotencia e CORS somente para clientes browser. Documentar rollback e backup; acompanhar consumo e planos.

## Variaveis de ambiente tipicas

- `ASPNETCORE_ENVIRONMENT=Production`
- `PORT` definido pela plataforma (nao fixar em 8080 na nuvem)
- `ConnectionStrings__Postgres=<STRING_NEON_COM_SSL>`
- `ConnectionStrings__MongoDb=<STRING_ATLAS_SRV>` **ou** `MongoDb__ConnectionString=...`, conforme bindings reais da aplicacao
- `OpenAI__ApiKey=<SECRET>` (se utilizado)
- `App__Agents__<Nome>__Url=<URL_HTTPS_AGENT>` conforme configuracao real

Nunca introduzir nomes de variaveis no escuro: identificar como `IConfiguration`/`IOptions` le do `appsettings.json` e adequar. Para Mongo, preservar o nome do banco definido pela app e confirmar `authSource` se necessario. Para Neon, usar pooling somente se adequado a workloads e migrations.

## Entregaveis de uma execucao

- Matriz de compatibilidade de deployment.
- Arquivos propostos com alteracoes minimas: `Dockerfile.vercel`, `.dockerignore`, `.env.example` sem segredos, documentos Spec Kit, opcional workflow GitHub Actions.
- Instrucoes com cliques de dashboard e comandos CLI, mas sem afirmar que conta/deploy foi criado.
- Checklist de validacao e riscos/limites gratuitos, com evidencias de teste e plano de rollback.

## Referencias

- Ler `references/deploy-guide.md` para passos dos tres provedores, alternativas e verificacoes.
- Ler `references/spec-kit.md` para prompts e tarefas SDD.
- Adaptar `assets/Dockerfile.vercel.template` ao caminho real do projeto.
- URLs oficiais a conferir no dia do deploy: https://vercel.com/docs/functions/runtimes , https://vercel.com/kb/guide/does-vercel-support-docker-deployments , https://vercel.com/docs/plans/hobby , https://www.mongodb.com/docs/atlas/tutorial/deploy-free-tier-cluster/ , https://www.mongodb.com/docs/atlas/reference/free-shared-limitations/ , https://neon.com/pricing , https://neon.com/docs/extensions/pgvector .
