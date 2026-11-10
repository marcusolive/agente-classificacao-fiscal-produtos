---
name: testes-agentes-ia
description: Projetar e executar testes automatizados do agente tributário .NET 10 com xUnit, integração REST/MCP, PgVector, MongoDB, Semantic Kernel e contratos estruturados. Usar ao criar critérios de aceite, evidências de teste e quality gates em SDD.
---

# Testes de Agentes de IA

## Regras transversais
- Ler `.specify/memory/constitution.md` e os artefatos da feature em `specs/` antes de modificar arquivos; se inexistentes, informar e não inventar requisitos.
- Trabalhar somente no escopo das tarefas aprovadas do Spec Kit; relacionar implementação, requisito, teste e evidência.
- Respeitar .NET 10, ASP.NET Core e a separação `Api.Agente`/`Api.RAG`; não criar bibliotecas desnecessárias.
- Nunca inserir segredos no código, logs, testes ou Git; configurar via variáveis de ambiente ou secret manager.
- Não inventar schemas fiscais, regras de negócio, documentos oficiais, versões de manuais nem conclusões de conformidade.
- Validar `dotnet build` e testes relevantes; informar explicitamente quando não for possível executar.
- Registrar decisões arquiteturais significativas em ADR, sem alterar a Constitution silenciosamente.

## Fluxo de execução
1. Mapear cada requisito e critério de aceite de `specs/` para pelo menos um cenário verificável; identificar requisitos sem teste.
2. Priorizar testes determinísticos: validação de arquivo, schema XSD somente quando aplicável, regras, DTOs, autorização, limites, erros e parse/validação do JSON.
3. Usar xUnit e dublês para OpenAI, MCP e tools em testes unitários; jamais depender de resposta LLM variável para assert determinístico.
4. Criar testes de integração REST e MCP; usar Testcontainers ou infraestrutura isolada para PostgreSQL/PgVector e MongoDB quando autorizado/disponível.
5. Montar fixtures anonimizadas de documentos válidos, inválidos, versão incorreta, manual ausente, falha técnica e prompt injection; não inventar obrigação oficial nem afirmar conformidade de fixture sintética.
6. Validar rastreabilidade das evidências RAG, isolamento por tenant/conversa, bloqueio de ferramentas sem autorização e transparência quando uma validação não foi concluída.
7. Criar testes de contrato para saída estruturada, limites/retries e tratamento de tokens ou custos não informados.
8. Caso sejam definidos testes de qualidade de respostas, usar dataset aprovado, avaliação humana ou rubrica documentada; reportar variabilidade e não confundir com certificação fiscal.
9. Rodar `dotnet test`, coletar TRX/cobertura quando configurado e apresentar testes passados, falhos, ignorados e bloqueios, sem falsificar execução.

## Critérios de entrega
- Matriz requisito -> testes -> evidências; comandos reproduzíveis para Windows PowerShell.
- Nenhuma aprovação de implantação com teste obrigatório falho ou não executado sem ressalva documentada.
