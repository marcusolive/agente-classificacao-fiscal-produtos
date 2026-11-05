# Exemplos de envelope A2A v0.3.x (JSON-RPC)

Os blocos ilustram **uma** mensagem para o agente e uma resposta de tarefa concluída. Em integração real, substituir os textos placeholder pelos objetos completos do `contrato-sofia-v1.md` devidamente serializados como strings JSON. Não enviar comentários ou `...` literais em produção.

## Chamada SOFIA → agente (POST na URL `AgentCard.url`)

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "method": "message/send",
  "params": {
    "message": {
      "role": "user",
      "messageId": "5f92c2ba-6870-4862-87ba-1ae8669a77f4",
      "parts": [
        {
          "kind": "text",
          "text": "{\"versaoContrato\": \"1.0\", \"idExecucao\": \"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\", \"idSolicitacao\": \"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\", \"usuario\": {\"nome\": \"Maria Oliveira\"}, \"pergunta\": \"Analise o documento\", \"documentos\": [{\"idDocumento\": \"doc-001\", \"nome\": \"NFe-123.xml\", \"texto\": \"Texto extraído da nota\"}]}"
        }
      ]
    }
  }
}
```

## Resposta agente → SOFIA (Task completed com Artifact)

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "result": {
    "id": "task-001",
    "contextId": "context-001",
    "status": {
      "state": "completed"
    },
    "artifacts": [
      {
        "artifactId": "art-001",
        "name": "analise-sofia-v1",
        "parts": [
          {
            "kind": "text",
            "text": "{\"versaoContrato\": \"1.0\", \"idExecucao\": \"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\", \"idSolicitacao\": \"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\", \"agente\": {\"idAgente\": \"agente-fiscal\", \"nome\": \"Agente Fiscal\"}, \"status\": \"concluido\", \"analise\": \"Nenhuma divergência encontrada.\", \"achados\": []}"
          }
        ]
      }
    ]
  }
}
```

## Agent Card (exemplo conceitual do perfil 0.3)

```json
{
  "name": "Agente Fiscal INFIS",
  "description": "Analisa documentos fiscais enviados pela SOFIA",
  "url": "http://agente-fiscal:8080/a2a",
  "version": "1.0.0",
  "capabilities": { "streaming": false, "pushNotifications": false },
  "defaultInputModes": ["text"],
  "defaultOutputModes": ["text"],
  "skills": [
    { "id": "analisar_documentos", "name": "Análise documental", "description": "Analisa textos enviados pela SOFIA", "tags": ["infis", "fiscal", "sofia"] }
  ]
}
```

Publicar card no `/.well-known/agent-card.json`. Endpoint A2A em `url`. Validar compatibilidade real com a versão/SDK selecionado; não transportar versões diferentes usando o mesmo envelope sem negociação.
