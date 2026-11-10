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
  "agente": { "idAgente": "agente-fiscal", "nome": "Agente Fiscal" },
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
- `idAgente` deve coincidir com o agente escolhido no registry/Agent Card.
- SOFIA agrega os resultados por agente e não substitui as análises originais; sua síntese final é camada separada.

## Tratamento de falhas
Falhas de conexão ou de protocolo não são `ResultadoAnaliseSofia` válido; SOFIA registra falha técnica e mantém os resultados dos demais agentes. Falha de análise de negócio pode retornar `status: erro` num Artifact se tarefa protocolar foi concluída; falhas graves podem resultar em Task `failed`, com detalhes não sensíveis. O orquestrador distingue os dois casos.

## Segurança
Enviar texto somente a agentes autorizados e necessários, com ciência das regras de confidencialidade do projeto. O nome do usuário é dado pessoal: minimizar persistência e eliminar do log operacional. Não colocar chaves de API no payload. A2A autenticação é transport-level; bloquear URLs arbitrárias e dados de documento usados como instruções.
