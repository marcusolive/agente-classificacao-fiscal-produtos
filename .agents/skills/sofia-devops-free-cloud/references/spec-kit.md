# Spec Kit - feature de deploy da SOFIA existente

## Escopo da especificacao

Criar feature `publicacao-cloud-gratuita` **sem reestruturar** controllers e contratos existentes. Entregaveis: diagnostico, Dockerfile.vercel, .dockerignore, example env, configuracao do Atlas e Neon, testes de conectividade, scripts de migracao revisados, roteiros de deploy e rollback.

## Prompt para speckit-specify

> Especifique a publicacao experimental de uma API ASP.NET Core .NET 10 existente da SOFIA em Vercel Hobby usando Container Runtime; MongoDB Atlas Free para historico/memoria; Neon Free com extensao vector para RAG. Preservar arquitetura, contratos A2A e REST, rotas, schema de banco, secrets e disponibilidade de dados. Definir limites de serverless e um plano alternativo se o workload for incompativel. Nao executar deploy sem aprovacao.

## Prompt para speckit-plan

> Inspecione o repo, identifique host .csproj e DLL, workloads longos, services em background, dependencias persistentes, credenciais e interfaces A2A. Proponha alteracoes minimas, matriz de compatibilidade, estrategias de migracao reversiveis e teste E2E. Leia a skill sofia-devops-free-cloud e as Skills de contrato A2A. Nao altere contratos existentes.

## Prompt para speckit-tasks

> Gerar tarefas pequenas com criterios de aceite: backup/branch; testes baseline; Dockerfile.vercel; envs; criacao Atlas/Neon guiada; habilitar vector; migrar schema/dados; validar HTTPS A2A, REST, banco, CORS; logs; custos; rollback. Separar tarefas manuais de automaveis e nao registrar secrets.

## Regras de execucao

- Evitar executar `specify init` novamente em repositorio ja configurado; seguir o fluxo do Spec Kit existente.
- Nao apagar infraestrutura Docker Compose local.
- Nao manipular ambiente de producao da INFIS sem permissao.
- Exigir aprovacao para mudancas destrutivas e exposicao publica de endpoints/documentos.
