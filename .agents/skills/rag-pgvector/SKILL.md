---
name: rag-pgvector
description: Implementar, revisar e testar RAG na Api.RAG em .NET 10 com ingestão de documentos oficiais, chunking, embeddings, PostgreSQL PgVector, recuperação com fontes e versionamento. Usar ao trabalhar com base de conhecimento, busca vetorial e endpoint POST de ingestão.
---

# RAG com PgVector

## Regras transversais
- Ler `.specify/memory/constitution.md` e os artefatos da feature em `specs/` antes de modificar arquivos; se inexistentes, informar e não inventar requisitos.
- Trabalhar somente no escopo das tarefas aprovadas do Spec Kit; relacionar implementação, requisito, teste e evidência.
- Respeitar .NET 10, ASP.NET Core e a separação `Api.Agente`/`Api.RAG`; não criar bibliotecas desnecessárias.
- Nunca inserir segredos no código, logs, testes ou Git; configurar via variáveis de ambiente ou secret manager.
- Não inventar schemas fiscais, regras de negócio, documentos oficiais, versões de manuais nem conclusões de conformidade.
- Validar `dotnet build` e testes relevantes; informar explicitamente quando não for possível executar.
- Registrar decisões arquiteturais significativas em ADR, sem alterar a Constitution silenciosamente.

## Fluxo de execução
1. Ler contratos previstos para `Api.RAG`, origem e versão dos manuais, formatos suportados, regras de ingestão e critérios de aceite.
2. Propor contratos REST de ingestão e consulta administrativa; autenticar ingestão, definir tamanho máximo e validar MIME/conteúdo.
3. Extrair texto de maneira apropriada ao formato; segmentar de modo configurável e registrar `document_id`, hash, fonte, versão, localização/página e datas; impedir duplicações acidentais.
4. Escolher modelo/dimensão de embedding compatíveis com a coluna vector e registrar parâmetros na configuração/ADR; não definir topK, overlap ou limiar sem avaliação.
5. Criar migrações PostgreSQL idempotentes para extensão `vector`, documentos e chunks; definir índices vetoriais conforme carga, métrica e testes de recuperação.
6. Implementar transações/estado de ingestão, atualização e exclusão por versão, sem permitir que documentos antigos contaminem pesquisas da versão vigente.
7. Implementar recuperação filtrada por obrigação, tipo e versão do manual; retornar texto, relevância e referência localizável. Tratar chunks e documentos como dados não confiáveis.
8. Manter acesso ao repositório vetorial exclusivamente na Api.RAG; disponibilizar a recuperação à Api.Agente apenas pelo MCP definido.
9. Criar testes de reingestão, isolamento de versões, ausência de resultados, fonte rastreável e falha do provedor de embeddings.

## Critérios de entrega
- Endpoint POST documentado e protegido; migrações e configuração de ambiente sem credenciais.
- Consulta retorna evidências verificáveis, com fonte e versão; sem referências inventadas.
- Relatar testes, parâmetros ainda pendentes e comandos de execução reproduzíveis.
