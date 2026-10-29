---
name: controle-custos-llm
description: Implementar mensuração e auditoria do consumo de tokens e custos de OpenAI e embeddings no PostgreSQL da Api.Agente, abrangendo chamadas e retentativas. Usar ao registrar métricas operacionais, estimativas e relatórios de custo.
---

# Controle de Custos LLM

## Regras transversais
- Ler `.specify/memory/constitution.md` e os artefatos da feature em `specs/` antes de modificar arquivos; se inexistentes, informar e não inventar requisitos.
- Trabalhar somente no escopo das tarefas aprovadas do Spec Kit; relacionar implementação, requisito, teste e evidência.
- Respeitar .NET 10, ASP.NET Core e a separação `Api.Agente`/`Api.RAG`; não criar bibliotecas desnecessárias.
- Nunca inserir segredos no código, logs, testes ou Git; configurar via variáveis de ambiente ou secret manager.
- Não inventar schemas fiscais, regras de negócio, documentos oficiais, versões de manuais nem conclusões de conformidade.
- Validar `dotnet build` e testes relevantes; informar explicitamente quando não for possível executar.
- Registrar decisões arquiteturais significativas em ADR, sem alterar a Constitution silenciosamente.

## Fluxo de execução
1. Confirmar esquema, moeda, precificação por modelo, origem da tabela de preços e data de vigência; nunca inventar valores atuais.
2. Modelar uma linha por tentativa de operação com `execution_id`, `correlation_id`, serviço, provedor, modelo, operação, timestamps, duração, status e tokens de entrada/saída/embedding quando fornecidos.
3. Representar métricas desconhecidas como NULL ou estado explícito de indisponibilidade, nunca como zero.
4. Distinguir custo estimado do provedor, custo calculado e faturamento efetivo; registrar referência e versão dos preços utilizados.
5. Centralizar instrumentação em serviço/decorator/middleware testável, incluindo retentativas, chamadas de ferramentas que geram uso de LLM e embeddings.
6. Controlar falhas de persistência com estratégia deliberada (fila/outbox ou best effort documentado), sem apagar o resultado operacional da chamada.
7. Expor consultas agregadas restritas por tenant/usuário autorizado; não incluir prompts completos, credenciais ou dados fiscais nos eventos.
8. Criar migrações PostgreSQL, testes para concorrência, cálculos com decimal, preço ausente, tokens ausentes e várias tentativas.

## Critérios de entrega
- Esquema auditável e relatório por execução/modelo/período.
- Demonstração de uma chamada com medição e caso com custo indeterminado.
