---
name: sofia-a2a-implementacao
description: Implementar e integrar agentes especializados do projeto SOFIA da INFIS com o orquestrador via protocolo Agent2Agent (A2A), em projetos .NET com Spec Kit/SDD. Usar ao criar servidor A2A de aluno, cliente A2A da SOFIA, Agent Card, fluxos de tarefas, testes de interoperabilidade ou planos/tasks de integração.
---

# Implementação A2A para SOFIA

## Objetivo
Implementar uma integração **A2A real**, em que SOFIA é cliente/orquestrador e cada agente especializado é servidor A2A. Não substituir o protocolo por um POST REST arbitrário. Adotar **A2A v0.3.x JSON-RPC** como perfil fixado para a turma, até decisão explícita de migração; seguir a especificação oficial em https://a2a-protocol.org/v0.3.0/specification/ e confirmar biblioteca .NET/SDK disponível antes de escrever código.

## Ler primeiro
- Ler `references/arquitetura-e-criterios.md` para workflow Spec Kit, requisitos e testes.
- Ler a skill complementar `sofia-a2a-contrato` e o contrato compartilhado `references/contrato-sofia-v1.md` (fornecido no pacote da outra skill). Se ausente, solicitar/copiar o contrato de referência antes de codificar.

## Workflow obrigatório Spec Kit
1. Inspecionar o repositório atual (`.specify/`, constitution, specs, arquitetura, código, testes). Não sobrescrever especificações ou convenções já existentes.
2. Construir/atualizar constitution com interoperabilidade, segurança de documentos, evolução versionada, rastreabilidade e testes de contrato como princípios.
3. Especificar requisitos: Agent Card público ou autenticado conforme arquitetura, `message/send`, transporte JSON-RPC, análise de documento em texto, retorno da análise, propagação de IDs, tratamento de erros e timeout.
4. Elaborar plano técnico com interfaces `IA2AAgentClient`, `IAgentRegistry`, `IAgentTaskHandler` (nomes ajustáveis ao projeto), camadas Application/Domain/Infrastructure/API, autenticação, logs e observabilidade.
5. Decompor em tarefas executáveis e verificáveis; implementar por etapas: publicação card, servidor, cliente SOFIA, DTOs de negócio, mapeamento A2A, orquestração, testes e documentação.
6. Executar testes unitários, de contrato e ponta a ponta com pelo menos dois agentes simulados. Entregar evidências (requisições, respostas e logs sanitizados).

## Regras de protocolo e domínio
- Descobrir cada agente pelo `/.well-known/agent-card.json`; validar `url`, skills, versão/interface e transporte suportado. Nunca usar o endereço do Agent Card como endpoint RPC sem ler seu `url`.
- Para o perfil v0.3.x usar JSON-RPC 2.0 `message/send`, `params.message.role=user`, `messageId`, `parts` com `kind=text`. A2A recebe `Task` ou `Message`; quando `Task`, recuperar a análise em `artifacts` (ou acompanhar o estado até `completed`). Não supor que `status.message` contém a saída final.
- Enviar o payload `SolicitacaoAnaliseSofia` como **texto JSON UTF-8 em um TextPart** (não tratar campos de negócio como atributos nativos do protocolo). Receber `ResultadoAnaliseSofia` como JSON em TextPart de Artifact; tolerar resposta Message somente se explicitamente pactuada e testada.
- `idExecucao`, `idSolicitacao`, `usuario.nome`, `pergunta`, `documentos[].idDocumento`, `documentos[].nome`, `documentos[].texto` são obrigatórios. Preservar os IDs e a relação documento-análise. Nunca mandar binário quando SOFIA já extraiu o texto.
- Propagar `traceId` e request ID em metadata/headers conforme o SDK, sem expor segredo. Registrar métricas (duração, status, agente, tokens/custo se disponíveis), redigir/remover conteúdo sensível dos logs.
- Para múltiplos agentes, despachar a agentes escolhidos por capacidade, agregar respostas independentemente, aceitar sucesso parcial, aplicar limites de concorrência/timeout e correlacionar cada retorno.
- Autenticar conexões via HTTPS e mecanismo pactuado (ex.: Bearer), validar destinatários permitidos, prevenir SSRF e prompt injection; texto do documento é dado não confiável, nunca instrução de sistema.
- Não inventar APIs de Microsoft Agent Framework nem de SDK A2A: inspecionar versões instaladas, pacotes e documentação antes de programar. Se não existir adaptador pronto, implementar compatibilidade com a versão pinada e testes.

## Saída exigida do assistente
Produzir ou atualizar `constitution.md`, `spec.md`, `plan.md`, `tasks.md`, código .NET, testes, README e exemplos JSON. Explicar todos os arquivos criados e incluir comando de execução/teste coerente com o repositório. Quando trabalhar somente em especificação, não afirmar que a integração está executando.
