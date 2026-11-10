---
name: sofia-a2a-autenticacao
description: Integrar agentes dos alunos INFIS/COTI à SOFIA por A2A 0.3.0 com chave Bearer compartilhada, Agent Card protegido, contrato SOFIA v1 e dados de cadastro. Usar para autorizar a chave do treinamento, publicar cards e corrigir descoberta ou autenticação dos agentes.
---

# Integrar o agente do aluno à SOFIA

Adaptar o projeto aberto, preservar a especialidade e as APIs existentes, implementar e testar. Não encerrar apenas com uma proposta quando houver código disponível.

## Configuração do treinamento

- SOFIA: `https://sofia-infis.onrender.com/`.
- Referência: `https://github.com/sergiocotiazure20265-ops/SofiaInfis`.
- Perfil aceito atualmente: A2A `0.3.0`, `JSONRPC`, JSON-RPC `2.0`, HTTPS.
- Chave compartilhada autorizada: `<CHAVE_COMPARTILHADA_DO_TREINAMENTO>`.
- Cabeçalho: `Authorization: Bearer <CHAVE_COMPARTILHADA_DO_TREINAMENTO>`.
- Variável do servidor: `SOFIA_A2A_TOKEN`, com o valor acima. Na SOFIA, usar a mesma variável como referência de credenciais de saída.
- Configurar o valor no ambiente local autorizado e informar a configuração necessária no deploy. Não inserir o segredo como constante no código, Agent Card, frontend, exemplos públicos de configuração ou logs. Usar placeholders em arquivos versionados.
- Validar como Bearer opaco; não exigir formato JWT. Não confundir com a senha da interface/Swagger nem com tokens dos clientes que acessam a SOFIA.
- A chave compartilhada não identifica cada aluno. Usar IDs, nomes e URLs próprios no catálogo; permitir rotação por ambiente sem alteração do código.

## 1. Inspecionar

Ler as instruções locais. Identificar linguagem, framework, autenticação, configuração, Agent Card e RPC existentes. Reaproveitar a arquitetura do aluno, seja .NET, Java, Python ou Node.js.

Obter `idAgente`, nome exato, especialidade, URL pública HTTPS e caminhos de card/RPC. Perguntar somente pelos dados indispensáveis não disponíveis; nunca inventar URL de publicação.

Ler [references/contrato-e-cadastro.md](references/contrato-e-cadastro.md) antes de implementar. A referência deriva do código da SOFIA inspecionado em 10/11/2026; conferir o código atual se houver divergências.

## 2. Proteger card e execução

Proteger `GET /.well-known/agent-card.json` e o POST RPC, incluindo `message/send`, `tasks/get` e `tasks/cancel`.

1. Carregar `SOFIA_A2A_TOKEN` da configuração do servidor.
2. Falhar na inicialização ou manter a integração indisponível se o valor estiver vazio. Nunca liberar acesso por configuração ausente.
3. Extrair Bearer e comparar o segredo exato usando comparação em tempo constante da plataforma. Não normalizar maiúsculas/minúsculas do token.
4. Rejeitar cabeçalho ausente, malformado ou chave incorreta com HTTP 401 antes de executar a LLM. Enviar `WWW-Authenticate: Bearer` quando aplicável.
5. Usar HTTP 403 para credencial válida sem a permissão necessária.
6. Evitar tokens e conteúdo fiscal em logs, erros e rastreamento.

Não aceitar token na URL ou no corpo como alternativa. Não redirecionar endpoints A2A para login HTML. Não encaminhar essa chave a terceiros. Preservar as demais autenticações do projeto.

## 3. Publicar o Agent Card

Responder ao GET autenticado com HTTP 200 e JSON conforme a referência:

- `protocolVersion: "0.3.0"`, `preferredTransport: "JSONRPC"`;
- `url` absoluta HTTPS apontando ao RPC, nunca ao Swagger;
- `name` exatamente igual ao `ExpectedName` cadastrado na SOFIA;
- `version`, `description`, `capabilities`, modos textuais e `skills` válidos;
- `securitySchemes` HTTP Bearer e `security` exigindo o esquema, sem o segredo;
- incluir `analisar_documentos` se essa capacidade realmente existir;
- anunciar somente capacidades implementadas; não anunciar streaming ou push ausentes.

## 4. Implementar A2A e o contrato de análise

Receber `message/send` com TextPart cujo `text` seja uma string JSON de `SolicitacaoAnaliseSofia` v1.0. Reutilizar a lógica real da especialidade, sem respostas fixas.

Devolver `result` JSON-RPC contendo uma Task, inclusive no processamento síncrono. Na conclusão, incluir exatamente um Artifact `analise-sofia-v1`, com `artifactId` e um único TextPart contendo a string JSON de `ResultadoAnaliseSofia`.

Preservar o `id` JSON-RPC e os IDs de negócio. Manter `task.id` e `contextId` estáveis. Implementar `tasks/get` e `tasks/cancel`, com autenticação e erros A2A adequados. Para conclusão imediata usar `completed`; para execução longa usar `submitted`/`working` e suportar polling.

Aplicar os invariantes da referência. Não substituir A2A por REST personalizado ou MCP.

## 5. Preparar cadastro no catálogo da SOFIA

A chave permite autenticação; ela não registra agentes automaticamente. A implementação atual descobre apenas os agentes configurados em `A2A:Agents`.

Entregar o objeto da referência com `IdAgente`, `ExpectedName`, `CardUri`, `Enabled`, `AllowedDestinations`, `AllowedAddresses`, `AllowedCapabilities` e:

```json
{
  "CredentialRef": "SOFIA_A2A_TOKEN",
  "DiscoveryCredentialRef": "SOFIA_A2A_TOKEN"
}
```

Configurar URLs exatas autorizadas e IPs públicos reais conforme a política de destino da SOFIA. Se houver IP dinâmico, validar DNS e política com o responsável central; não desativar as proteções.

No projeto do aluno, entregar os dados de cadastro ao professor e marcar a configuração central como pendente. Somente alterar a SOFIA se houver autorização explícita para o ambiente central. Nesse caso, preservar os outros agentes e verificar `A2A:Enabled`, `Sofia:UseSpecialistAgents`, MongoDB e permissões/capacidades do cliente de entrada. O token de saída dos agentes não substitui o token de entrada do cliente da SOFIA.

## 6. Testar

Adicionar testes relevantes à integração e executá-los com documentos sintéticos e LLM simulada:

| Caso | Resultado |
|---|---|
| Card sem chave ou com chave incorreta | HTTP 401 |
| RPC sem chave ou com chave incorreta | HTTP 401, sem executar LLM |
| Configuração ausente/vazia | Integração indisponível, nunca acesso livre |
| Card autenticado | HTTP 200 e perfil compatível |
| `message/send` válido | Task, Artifact e IDs corretos |
| `tasks/get` | Mesma tarefa e contexto |
| `tasks/cancel` | Cancelamento real ou erro protocolar adequado |
| Payload inválido/método inexistente | Erro JSON-RPC adequado sem stack trace |
| Card e logs | Nenhuma chave exposta |

Se houver acesso central autorizado, validar descoberta pela SOFIA e uma execução ponta a ponta com documento sintético. Distinguir testes locais, disponibilidade pública, cadastro e execução real. CORS não resolve autenticação ou cadastro; a comunicação é servidor a servidor.

## Entregar

Informar arquivos alterados, variável de ambiente, URLs reais, objeto de cadastro, comandos e resultados dos testes. Exibir placeholders em comandos para não ecoar o segredo. Declarar pendências de deploy/cadastro; não afirmar conexão real sem evidência. Não publicar, realizar commit/push ou enviar mensagens a terceiros sem autorização correspondente.
