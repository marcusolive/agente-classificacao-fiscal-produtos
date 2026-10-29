---
name: memoria-mongodb
description: Implementar memória conversacional no MongoDB para Api.Agente em .NET 10, com isolamento por usuário, conversa e organização, retenção, recuperação limitada e proteção de dados. Usar ao persistir ou recuperar histórico de sessões do agente.
---

# Memória conversacional MongoDB

## Regras transversais
- Ler `.specify/memory/constitution.md` e os artefatos da feature em `specs/` antes de modificar arquivos; se inexistentes, informar e não inventar requisitos.
- Trabalhar somente no escopo das tarefas aprovadas do Spec Kit; relacionar implementação, requisito, teste e evidência.
- Respeitar .NET 10, ASP.NET Core e a separação `Api.Agente`/`Api.RAG`; não criar bibliotecas desnecessárias.
- Nunca inserir segredos no código, logs, testes ou Git; configurar via variáveis de ambiente ou secret manager.
- Não inventar schemas fiscais, regras de negócio, documentos oficiais, versões de manuais nem conclusões de conformidade.
- Validar `dotnet build` e testes relevantes; informar explicitamente quando não for possível executar.
- Registrar decisões arquiteturais significativas em ADR, sem alterar a Constitution silenciosamente.

## Fluxo de execução
1. Confirmar ADR que escolhe MongoDB no lugar de Redis e os critérios de retenção, consentimento, exclusão e residência de dados aplicáveis.
2. Modelar mensagens com `conversation_id`, `user_id`, `tenant_id` quando houver, `role`, timestamp, correlation_id e informações de classificação de dados; nunca usar o texto do usuário como chave de isolamento.
3. Criar índices compostos para consulta por tenant/usuário/conversa e ordenação; considerar TTL somente quando adequado à política aprovada.
4. Implementar recuperação paginada ou limitada pelo orçamento de contexto; preservar ordem cronológica ao enviar mensagens ao modelo.
5. Persistir mensagens e resultados sem guardar indevidamente segredos, tokens, arquivos fiscais completos ou saídas de ferramentas sensíveis.
6. Garantir segregação por autorização em cada consulta; nunca aceitar `user_id` fornecido pelo cliente sem vinculação à identidade autenticada.
7. Implementar política de expiração/exclusão e documentar implicações de backup; distinguir memória de conversa da fonte oficial de conhecimento RAG.
8. Tratar falhas do MongoDB sem inventar histórico; definir comportamento degradado permitido pela especificação.
9. Testar isolamento cruzado, concorrência, ordenação, retenção, exclusão e indisponibilidade.

## Critérios de entrega
- Configuração por variável de ambiente e exemplo `.env.example` sem valores reais.
- Índices, modelos, repositórios, testes e instruções de execução documentados.
