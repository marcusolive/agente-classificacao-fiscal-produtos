# Research: Confirmação do caso corporativo

## Decisão: não gravar nesta fatia

**Decisão**: A confirmação valida e devolve o contrato 1.1.0. `gravado` permanece falso. A frase "gravação na base de conhecimento não realizada" entra em `validacoesNaoRealizadas`.

**Rationale**: A autoridade e o limite da correção já estão decididos. O lugar do precedente ainda é a Api.RAG, que não existe. Gravar na Api.Agente faria a análise seguinte poder tratar esse registro como evidência antes da base existir.

**Alternatives considered**: Persistir no PostgreSQL da Api.Agente foi rejeitado para esta specification. Publicar direto na Api.RAG foi rejeitado porque esse serviço não faz parte desta fatia.
