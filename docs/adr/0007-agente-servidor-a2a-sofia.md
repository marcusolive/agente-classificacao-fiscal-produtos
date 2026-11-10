# ADR 0007 — Api.Agente como servidor A2A da SOFIA

- Status: aceita
- Data: 2026-11-05
- Specification: `specs/004-integracao-sofia-a2a/spec.md`

## Decisão

A Api.Agente publica `/.well-known/agent-card.json` e recebe `message/send` em `/a2a`, no perfil A2A JSON-RPC 0.3. O texto da mensagem é o contrato SOFIA 1.0. A resposta concluída traz `ResultadoAnaliseSofia` em um TextPart. A chave Bearer fica em `Sofia:Chave`. A política CORS `SofiaBrowser` só é registrada quando `Cors:AllowedOrigins` tem origens.

## Contexto

A SOFIA é o orquestrador e este repositório é o agente especializado. A DP-16 não cria um terceiro projeto para o cliente A2A. A DP-02 não autoriza gravar nome de usuário nem texto de documento com política de retenção implícita. A DP-09 não autoriza transformar o texto recebido em achado fiscal.

## Consequências

A análise REST `/v1/analises` permanece. Uma retentativa com o mesmo `idSolicitacao` reutiliza a tarefa enquanto o processo estiver no ar, só em memória. Sem chave configurada, `/a2a` responde 401. O texto do documento não vira instrução nem evidência inventada.
