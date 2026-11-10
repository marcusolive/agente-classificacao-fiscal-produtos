# Cadastro do agente na SOFIA (A2A)

Entregável da skill `sofia-a2a-autenticacao`. Dados para o responsável central
(professor) registrar o agente no catálogo `A2A:Agents` da SOFIA. **A chave autentica,
mas não registra o agente automaticamente** — o cadastro central fica pendente.

## Objeto de cadastro (A2A:Agents)

```json
{
  "IdAgente": "agente-fiscal",
  "ExpectedName": "Agente Fiscal INFIS",
  "Enabled": true,
  "CardUri": "https://agente-classificacao-fiscal.vercel.app/.well-known/agent-card.json",
  "AllowedDestinations": [
    "https://agente-classificacao-fiscal.vercel.app/.well-known/agent-card.json",
    "https://agente-classificacao-fiscal.vercel.app/a2a"
  ],
  "AllowedAddresses": [
    "PENDENTE_COORDENAR_COM_SOFIA"
  ],
  "AllowedCapabilities": [
    "analisar_documentos"
  ],
  "CredentialRef": "SOFIA_A2A_TOKEN",
  "DiscoveryCredentialRef": "SOFIA_A2A_TOKEN"
}
```

- `IdAgente` e `ExpectedName` casam com o Agent Card publicado (`name` = "Agente Fiscal INFIS").
- `CardUri`/`AllowedDestinations` usam o domínio real de produção na Vercel.
- **`AllowedAddresses`**: a Vercel (serverless) usa **IPs de saída dinâmicos** no plano Hobby —
  não há IP público fixo. Coordenar com o responsável da SOFIA uma política por DNS/domínio,
  sem desativar as proteções (conforme a skill). Por isso está pendente, não com placeholder inválido.

## Configuração do servidor (já aplicada na Vercel, produção)

| Variável | Valor | Observação |
| --- | --- | --- |
| `SOFIA_A2A_TOKEN` | `<chave compartilhada do treinamento>` | Token Bearer de entrada (o valor está na skill `sofia-a2a-autenticacao`, não aqui). Prevalece sobre `Sofia:Chave`. |
| `Sofia__Url` | `https://agente-classificacao-fiscal.vercel.app/a2a` | Corrigido para apontar ao endpoint RPC, não à base. |

O segredo **não** está no código nem em arquivo versionado — apenas no ambiente da Vercel e
como placeholder em `.env.example`. Rotacionável por ambiente.

## O que foi implementado

A skill foi aplicada por completo, por decisão do mantenedor, com a governança atualizada
primeiro (constituição 1.0.8 e specification `004`), depois código e testes.

- **`SOFIA_A2A_TOKEN`** como fonte do segredo de entrada A2A (`Program.cs`), prevalecendo sobre `Sofia:Chave`.
- **Comparação em tempo constante** do Bearer (`CryptographicOperations.FixedTimeEquals`), sem normalizar caixa.
- **Agent Card e `/a2a` protegidos**: sem Bearer, respondem 401 com `WWW-Authenticate: Bearer`. Fail-closed quando o segredo está vazio.
- **`tasks/get`** recupera a tarefa concluída por id (`-32001` se não existir).
- **`tasks/cancel`** responde `-32002` para tarefa concluída (processamento síncrono), `-32001` se não existir.
- Testes: suíte do Api.Agente **48/48 verde**, incluindo card protegido, `tasks/get`/`tasks/cancel` e `SOFIA_A2A_TOKEN`. OpenAPI (`analise-v1.openapi.yaml`) regenerado.

## Governança atualizada (rito da seção 8)

- **Constituição 1.0.8** (DP-15 ampliada): card protegido e os métodos `message/send`, `tasks/get` e `tasks/cancel`.
- **Specification `004`**: FR-002 (três métodos) e FR-007 (card protegido + `SOFIA_A2A_TOKEN`) atualizados.

## Pendências de publicação

- As mudanças de código acima **ainda não estão em produção**: exigem novo deploy (merge em `main`).
- Cadastro central na SOFIA: pendente com o responsável (objeto acima).
- Teste ponta a ponta com a SOFIA real: só após o cadastro e a liberação da descoberta.
