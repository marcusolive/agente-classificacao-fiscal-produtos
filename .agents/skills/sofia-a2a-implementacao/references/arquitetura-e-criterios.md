# Arquitetura SOFIA e critérios

```text
Usuário -> SOFIA (API .NET / orquestrador)
             | extração de texto e classificação
             | descoberta via Agent Card
             +--> A2A Cliente -> Agente Fiscal (A2A Server)
             +--> A2A Cliente -> Agente Contábil (A2A Server)
             +--> A2A Cliente -> Agente Tributário (A2A Server)
             | <- Task.completed + Artifact(JSON ResultadoAnaliseSofia)
             v
       agregação + síntese + rastreabilidade -> resposta
```

## Requisitos verificáveis
- RF01: Publicar Agent Card válido com identidade, descrição, capabilities e skill `analisar_documentos`, bem como endpoint remoto A2A acessível da rede Docker.
- RF02: Receber pelo método `message/send` solicitação com usuário, pergunta e N documentos com conteúdo textual; rejeitar documento sem `texto` ou sem identificador.
- RF03: Retornar `ResultadoAnaliseSofia` ao fim da tarefa com status, análise, achados/evidências e referências aos documentos enviados.
- RF04: Preservar `idExecucao` e `idSolicitacao`; distinguir falha técnica de ausência de achados.
- RF05: Agente não pode chamar outros agentes sem permissão explícita; a SOFIA centraliza roteamento e síntese.
- RNF01: HTTPS fora do Docker isolado; token não incluído no JSON de negócio; logs sem documentos completos.
- RNF02: Timeout configurável, cancelamento, retry somente quando seguro, deduplicação por idSolicitacao/idAgente.
- RNF03: Contratos versionados (`versaoContrato=1.0`) e validação de schemas de entrada e saída.

## Critérios Given/When/Then
1. Given card e endpoint disponíveis; When SOFIA solicita análise; Then agente retorna Task completed com Artifact JSON válido e IDs corretos.
2. Given dois documentos; When análise é enviada; Then ambos chegam integralmente e são rastreáveis pelo idDocumento.
3. Given indisponibilidade de um agente; When SOFIA chama três agentes; Then os dois sucessos são mantidos e a falha é relatada sem derrubar toda a execução.
4. Given conteúdo adversarial em documento; When agente processa; Then não executa instruções embutidas no documento.
5. Given timeouts ou resposta schema inválido; When SOFIA agrega; Then marca agente como falho e registra trace sem dados sensíveis.

## Etapas sugeridas de tasks.md
T01 Definir versão A2A e contrato v1; T02 Publicar card; T03 Criar modelos e validators; T04 Handler de análise; T05 Adapter A2A servidor; T06 Cliente A2A SOFIA; T07 Registry; T08 Dispatch paralelo controlado; T09 Agregação; T10 Segurança e observabilidade; T11 Testes unitários/contrato/E2E; T12 documentação Docker/README.

## Integração em Docker
Usar DNS de serviço (`http://agente-fiscal:8080`), nunca `localhost` de outro container. Fixar URLs em configuração e whitelist. A origem `http://` é aceitável apenas em rede de desenvolvimento isolada; preferir TLS em ambientes compartilhados ou produção.
