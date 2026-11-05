---
name: sofia-cors-rest-a2a
description: Padronizar a configuracao segura de CORS, conectividade HTTP e autorizacao para agentes de alunos do projeto SOFIA/INFIS em ASP.NET Core .NET 10. Usar no Spec Kit quando um agente deve aceitar chamadas REST ou A2A vindas da SOFIA, expor Agent Card, habilitar clientes web autorizados, configurar Docker ou testar preflight OPTIONS, autenticacao e integracao ponta a ponta.
---

# CORS e integracao REST/A2A - SOFIA / INFIS

## Objetivo
Adaptar projetos existentes com alteracoes minimas para que a SOFIA consiga invocar os agentes especializados via A2A e API REST, respeitando o contrato definido em `sofia-a2a-contrato` e o fluxo definido em `sofia-a2a-implementacao`.

## Regra arquitetural obrigatoria
- Diferenciar **CORS** (politica que navegadores aplicam a requisicoes entre origens) de **acesso backend-backend** (HTTP/A2A entre SOFIA e agente, sem verificacao CORS pelo navegador). Nao afirmar que configurar CORS, isoladamente, permite conexoes A2A.
- Garantir rede, DNS, porta, bind em `0.0.0.0`, TLS quando aplicavel, autenticacao e autorizacao do chamador SOFIA.
- Quando houver frontend da SOFIA executado no navegador, permitir explicitamente **a origem desse frontend**, nao o endpoint backend da SOFIA por engano. Origem = esquema + host + porta, sem path e sem barra final.
- Aplicar CORS somente aos endpoints que precisem de clientes browser. Preferir politica nomeada e escopo restrito; nenhuma necessidade de habilitar CORS em chamadas exclusivamente servidor-servidor.
- Nunca usar `AllowAnyOrigin()` com `AllowCredentials()` ou `SetIsOriginAllowed(_ => true)` com credenciais. Nao usar wildcard aberto em producao. CORS nunca substitui autenticacao.

## Procedimento Spec Kit
1. Inspecionar `Program.cs`, `appsettings*.json`, rotas de controllers/minimal APIs, docker-compose e autenticacao existentes; preservar convenios e recursos implementados pelos alunos.
2. Registrar requisitos em `spec.md`: REST e A2A funcionam a partir da SOFIA; chamadas do navegador so funcionam das origens autorizadas; origem desconhecida nao recebe permissao CORS; endpoints protegidos exigem token.
3. Planejar em `plan.md`: politica `SofiaBrowser`, configuracao `Cors:AllowedOrigins` por ambiente, posicionamento de middleware, endpoints REST/A2A reais e descoberta Agent Card.
4. Dividir tarefas em `tasks.md`: configuracao; testes OPTIONS; testes HTTP sem cabecalho Origin; autenticacao; container DNS/portas; verificacao end-to-end.
5. Implementar a configuracao conforme [references/dotnet.md](references/dotnet.md). Integrar com a autenticacao ja existente sem desabilitar protecoes.
6. Verificar resultados com [references/testes.md](references/testes.md). Reportar evidencias, endpoints testados, problemas remanescentes e riscos.

## Contrato funcional que deve permanecer intacto
- Receber solicitacoes da SOFIA com nome do usuario, pergunta e documentos ja convertidos em texto, conforme Skill de contrato A2A da turma.
- Responder a SOFIA com analise gerada e identificadores de correlacao (`idExecucao`, `idSolicitacao`).
- Nao inventar rotas padronizadas que nao constem do projeto. Mapear rotas existentes e alinhar com a Skill de contrato, incluindo `/.well-known/agent-card.json` e endpoint A2A real.
- CORS nao transforma uma rota REST comum em servidor A2A: usar envelope, metodos e transporte definidos na implementacao A2A.

## Criterios de aceite
- Com browser de origem permitida, preflight `OPTIONS` recebe `Access-Control-Allow-Origin` correspondente, metodos e headers adequados.
- Com origem nao permitida, preflight nao inclui permissao CORS para essa origem.
- Requisicoes backend-backend sem `Origin` passam pelo pipeline normal de rede e autenticacao, sem dependencia de headers CORS.
- GET do Agent Card e POST A2A/REST trafegam atraves da rede usada pela SOFIA, com resposta conforme contrato (ou erro de autenticacao previsto).
- Sem credencial valida, endpoint protegido retorna 401/403 conforme a situacao.
- Nao expor documentos fiscais e nomes em logs indiscriminados; usar correlacao e telemetria sem dados sensiveis.

## Entregaveis ao usar esta Skill
Fornecer os trechos alterados (`Program.cs`, `appsettings`, Compose, testes), explicacao breve das escolhas, comandos de verificacao e checklist dos criterios de aceite. Evitar reescrever integralmente o projeto quando uma configuracao pontual e suficiente.
