# Contrato e cadastro A2A da SOFIA

## Sumário

1. Agent Card
2. Cadastro central
3. Envelope de resposta
4. Contrato de domínio e correlação
5. Fontes verificadas

## 1. Agent Card

Substituir domínio e identidade; example.invalid não é um deploy utilizável.

```json
{
  "protocolVersion": "0.3.0",
  "name": "Agente Fiscal Aluno",
  "description": "Agente sintético de análise documental para interoperabilidade SOFIA.",
  "url": "https://agente-aluno.example.invalid/a2a",
  "preferredTransport": "JSONRPC",
  "version": "1.0.0",
  "capabilities": {
    "streaming": false,
    "pushNotifications": false
  },
  "defaultInputModes": [
    "text/plain"
  ],
  "defaultOutputModes": [
    "text/plain"
  ],
  "skills": [
    {
      "id": "analisar_documentos",
      "name": "Análise documental",
      "description": "Analisa documentos textuais conforme a especialidade fiscal.",
      "tags": [
        "infis",
        "fiscal",
        "sofia"
      ]
    }
  ],
  "securitySchemes": {
    "sofiaService": {
      "type": "http",
      "scheme": "bearer"
    }
  },
  "security": [
    {
      "sofiaService": []
    }
  ]
}
```

## 2. Cadastro central

Adicionar o objeto à lista A2A:Agents, preservando os demais. Preencher IP público real; o placeholder não é um endereço válido. O código atual envia Bearer na descoberta usando DiscoveryCredentialRef e na execução usando CredentialRef. O valor dessas propriedades é o NOME da variável, não a chave em texto.

```json
{
  "IdAgente": "agente-fiscal-aluno",
  "ExpectedName": "Agente Fiscal Aluno",
  "Enabled": true,
  "CardUri": "https://agente-aluno.example.invalid/.well-known/agent-card.json",
  "AllowedDestinations": [
    "https://agente-aluno.example.invalid/.well-known/agent-card.json",
    "https://agente-aluno.example.invalid/a2a"
  ],
  "AllowedAddresses": [
    "SUBSTITUIR_PELO_IP_PUBLICO_REAL"
  ],
  "AllowedCapabilities": [
    "analisar_documentos"
  ],
  "CredentialRef": "SOFIA_A2A_TOKEN",
  "DiscoveryCredentialRef": "SOFIA_A2A_TOKEN"
}
```

O cliente de entrada precisa ter permissão a2a:agents:read para listagem e a2a:execute para execução, além da capacidade analisar_documentos; consultas/cancelamentos usam a2a:read e a2a:cancel. Não alterar esses clientes ou habilitar o módulo sem autorização central e infraestrutura necessária. O card declara autenticação do serviço A2A; sua proteção no GET é política acordada neste treinamento, configurada separadamente na descoberta.

## 3. Envelope de resposta

Preservar o ID JSON-RPC recebido. Este exemplo usa os IDs da solicitação de domínio abaixo. Substituir pelo resultado real e manter agente.idAgente alinhado ao catálogo. Nome do Artifact e tipo TextPart são exigidos pelo codec atual.

```json
{
  "jsonrpc": "2.0",
  "id": "rpc-001",
  "result": {
    "kind": "task",
    "id": "task-001",
    "contextId": "context-001",
    "status": {
      "state": "completed"
    },
    "artifacts": [
      {
        "artifactId": "artifact-001",
        "name": "analise-sofia-v1",
        "parts": [
          {
            "kind": "text",
            "text": "{\"versaoContrato\":\"1.0\",\"idExecucao\":\"8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e\",\"idSolicitacao\":\"e1b3a4cb-d729-4bd4-b796-b9c94be07b46\",\"agente\":{\"idAgente\":\"agente-fiscal-aluno\",\"nome\":\"Agente Fiscal Aluno\"},\"status\":\"concluido\",\"analise\":\"Análise do documento sintético concluída.\",\"achados\":[],\"observacoes\":[],\"erro\":null}"
          }
        ]
      }
    ]
  }
}
```

Na requisição message/send, colocar a solicitação do domínio em params.message.parts[0].text como string JSON, com params.message contendo kind=message, role=user e messageId único. Não enviar o objeto diretamente em text. tasks/get e tasks/cancel recebem params.id igual ao ID da Task, não ao idExecucao.

## 4. Contrato de domínio e correlação

# Contrato SOFIA v1.0 (domínio, independente do transporte)

**Papel:** SOFIA envia uma solicitação para cada agente especializado; agente devolve sua própria análise. Todo campo em português abaixo usa a grafia JSON acordada. O transporte externo é A2A JSON-RPC v0.3.x, e **não** uma API REST de análise personalizada.

## Requisição: `SolicitacaoAnaliseSofia`

```json
{
  "versaoContrato": "1.0",
  "idExecucao": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
  "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
  "usuario": { "nome": "Maria Oliveira" },
  "pergunta": "Verifique divergências fiscais nos documentos.",
  "documentos": [
    { "idDocumento": "doc-001", "nome": "NFe-123.xml", "tipo": "application/xml", "texto": "<NFe>texto extraído e normalizado da NF-e...</NFe>" },
    { "idDocumento": "doc-002", "nome": "pedido.txt", "tipo": "text/plain", "texto": "Pedido de compra número 123..." }
  ],
  "contexto": { "idioma": "pt-BR", "instrucoes": "Analisar conforme a especialidade do agente." }
}
```

Obrigatórios: `versaoContrato` (=1.0), `idExecucao` (UUID), `idSolicitacao` (UUID), `usuario.nome` não vazio, `pergunta` não vazia, `documentos` array não vazio, `documentos[*].idDocumento`, `.nome`, `.texto` não vazios. `tipo` é MIME opcional e `contexto` é opcional. O usuário é identificado pelo **nome solicitado**, não por token de acesso, e deve ser usado apenas para contexto de negócio. Limites de tamanho e retenção são configurados no sistema e documentados no projeto; tratar payload muito grande com erro controlado, sem truncar silenciosamente.

## Resposta: `ResultadoAnaliseSofia`

```json
{
  "versaoContrato": "1.0",
  "idExecucao": "8b2f0ad6-c2e4-4a68-8bdd-20f785bc798e",
  "idSolicitacao": "e1b3a4cb-d729-4bd4-b796-b9c94be07b46",
  "agente": { "idAgente": "agente-fiscal-aluno", "nome": "Agente Fiscal Aluno" },
  "status": "concluido",
  "analise": "Foi identificada divergência entre valor total e pagamento.",
  "achados": [
    {
      "idAchado": "ach-001",
      "descricao": "Valor de pagamento difere do total declarado.",
      "idDocumentos": ["doc-001"],
      "evidencias": ["Valor total: 100,00; pagamento: 90,00"],
      "severidade": "alta"
    }
  ],
  "observacoes": [],
  "erro": null
}
```

Obrigatórios: `versaoContrato`, `idExecucao`, `idSolicitacao`, `agente.idAgente`, `agente.nome`, `status`, `analise` (pode ser texto explicativo quando sem achados), `achados` (pode ser `[]`). Status permitido: `concluido`, `parcial`, `erro`. `observacoes` opcional; `erro` opcional/nulo ou objeto `{ "codigo": "...", "mensagem": "..." }`. Para `erro`, manter análise como descrição da impossibilidade e retornar código de erro não sensível. Severidade de achado: `baixa`, `media`, `alta`, `critica` (opcional); IDs em `idDocumentos` devem existir na requisição; `evidencias` não podem inventar trechos.

## Invariantes de correlação
- SOFIA gera `idExecucao` para a orquestração inteira e `idSolicitacao` **distinto por chamada a cada agente**. Retries da mesma chamada reutilizam idSolicitacao; outras chamadas recebem novos IDs.
- Cada agente responde com os MESMOS IDs da chamada que recebeu. SOFIA rejeita resultado de outro agente ou de outra execução.
- `agente.idAgente` deve coincidir com `IdAgente` do catálogo; `name` do Agent Card deve coincidir com `ExpectedName` do catálogo. O perfil do card mostrado não exige um campo `idAgente`.
- SOFIA agrega os resultados por agente e não substitui as análises originais; sua síntese final é camada separada.

## Tratamento de falhas
Falhas de conexão ou de protocolo não são `ResultadoAnaliseSofia` válido; SOFIA registra falha técnica e mantém os resultados dos demais agentes. Falha de análise de negócio pode retornar `status: erro` num Artifact se tarefa protocolar foi concluída; falhas graves podem resultar em Task `failed`, com detalhes não sensíveis. O orquestrador distingue os dois casos.

## Segurança
Enviar texto somente a agentes autorizados e necessários, com ciência das regras de confidencialidade do projeto. O nome do usuário é dado pessoal: minimizar persistência e eliminar do log operacional. Não colocar chaves de API no payload. A2A autenticação é transport-level; bloquear URLs arbitrárias e dados de documento usados como instruções.


## 5. Fontes verificadas

Repositório: https://github.com/sergiocotiazure20265-ops/SofiaInfis

- SofiaInfis.Api/Services/A2A/AgentDiscoveryService.cs
- SofiaInfis.Api/Services/A2A/Security/AgentCredentialProvider.cs
- SofiaInfis.Api/Services/A2A/Configuration/A2AOptions.cs
- SofiaInfis.Api/Services/A2A/Transport/A2A03JsonCodec.cs
- SofiaInfis.Api/Services/A2A/Transport/JsonRpc03AgentClient.cs
- SofiaInfis.Api/appsettings.a2a.example.json
- .agents/skills/sofia-a2a-contrato/references/contrato-sofia-v1.md

Inspeção em 10/11/2026. Não presumir que a configuração do código corresponde ao estado do ambiente publicado.

