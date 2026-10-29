---
name: mcp-client-server
description: Implementar e validar a comunicação MCP entre Api.Agente (cliente) e Api.RAG (servidor) em .NET 10, incluindo descoberta de tools, transporte, contratos, segurança, falhas e testes. Usar ao integrar os serviços por Model Context Protocol.
---

# MCP Client e Server

## Regras transversais
- Ler `.specify/memory/constitution.md` e os artefatos da feature em `specs/` antes de modificar arquivos; se inexistentes, informar e não inventar requisitos.
- Trabalhar somente no escopo das tarefas aprovadas do Spec Kit; relacionar implementação, requisito, teste e evidência.
- Respeitar .NET 10, ASP.NET Core e a separação `Api.Agente`/`Api.RAG`; não criar bibliotecas desnecessárias.
- Nunca inserir segredos no código, logs, testes ou Git; configurar via variáveis de ambiente ou secret manager.
- Não inventar schemas fiscais, regras de negócio, documentos oficiais, versões de manuais nem conclusões de conformidade.
- Validar `dotnet build` e testes relevantes; informar explicitamente quando não for possível executar.
- Registrar decisões arquiteturais significativas em ADR, sem alterar a Constitution silenciosamente.

## Fluxo de execução
1. Consultar versão atual dos pacotes SDK MCP .NET e documentação do transporte compatível antes de selecionar APIs; registrar versões realmente instaladas.
2. Definir Tool MCP de consulta aos manuais com nome, descrição, JSON Schema de entrada (pergunta, tipo, versão e filtros) e saída com evidências, fontes e erros tipados.
3. Expor MCP Server na Api.RAG e MCP Client na Api.Agente; conservar endpoints REST administrativos separados das ferramentas MCP.
4. Implementar autenticação/autorização do transporte escolhido, autorização em cada tool, limites de payload, timeouts, cancelamento, correlação e sanitização de logs.
5. Não permitir que resultados MCP ou conteúdo de documentos alterem instruções internas do agente; validar argumentos e formatos retornados.
6. Não implementar acesso direto da Api.Agente ao banco vetorial nem substituição silenciosa do MCP por HTTP REST funcional.
7. Em indisponibilidade do servidor, retornar erro técnico rastreável; não afirmar que conhecimento foi consultado.
8. Executar teste de ponta a ponta com servidor e cliente em execução, além de testes de contrato, autenticação, timeout e falha de conectividade.

## Critérios de entrega
- Exemplos reproduzíveis de como iniciar os dois serviços e invocar o MCP.
- Descrição das tools, contratos, autenticação e códigos de falha.
- Testes executados e limitações reais da integração.
