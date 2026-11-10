# Testes recomendados - origem web, REST e A2A

Substituir host, porta, origem, caminho A2A e token pelos efetivamente configurados.

## 1. Preflight de origem autorizada (simula browser)

```bash
curl -i -X OPTIONS http://localhost:8085/api/analises \
  -H 'Origin: http://localhost:4200' \
  -H 'Access-Control-Request-Method: POST' \
  -H 'Access-Control-Request-Headers: content-type,authorization'
```

Esperar `Access-Control-Allow-Origin: http://localhost:4200`, `Access-Control-Allow-Methods` contendo POST e headers permitidos. HTTP 204 e comum, mas verificar o comportamento do servidor real.

## 2. Origem bloqueada

```bash
curl -i -X OPTIONS http://localhost:8085/api/analises \
  -H 'Origin: https://site-nao-autorizado.example' \
  -H 'Access-Control-Request-Method: POST'
```

Esperar ausencia de `Access-Control-Allow-Origin` para esta origem; nao interpretar o codigo HTTP isoladamente como evidencia de bloqueio no navegador. CORS nao e firewall e curl nao aplica CORS.

## 3. Backend para backend (sem Origin)

```bash
curl -i http://localhost:8085/.well-known/agent-card.json
curl -i -X POST http://localhost:8085/api/analises \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer TOKEN_VALIDO' \
  --data '{"idExecucao":"teste","idSolicitacao":"teste-1","usuario":{"nome":"Usuario Teste"},"pergunta":"Analisar documento","documentos":[{"nome":"teste.txt","texto":"Documento de teste para analise."}]}'
```

O JSON acima e apenas demonstrativo; usar os campos/versoes exigidos pela Skill de contrato. A requisicao REST so vale se esta rota existir. Para A2A, testar especificamente o endpoint e o envelope JSON-RPC/Task do protocolo, nao reutilizar cegamente o corpo REST.

## 4. Testes extras obrigatorios
- Verificar 401 (nao autenticado) e 403 (sem permissao) em endpoint protegido.
- Verificar header `Vary: Origin` quando aplicavel e comportamento do proxy reverso.
- Verificar `curl` de dentro do container SOFIA para `http://agente-aluno:8080` (DNS, conexao, Agent Card, POST autenticado).
- Confirmar que endpoint REST, Agent Card e endpoint A2A possuem caminhos e contratos corretos.
- Validar limite de tamanho dos documentos, HTTPS/TLS conforme ambiente, timeout e correlacao de logs.
