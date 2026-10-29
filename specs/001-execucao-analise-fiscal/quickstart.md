# Quickstart: Execução da análise fiscal de itens

Este guia vale depois que as tarefas forem implementadas. Ainda não há código de produção.

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL da Api.Agente
- Variável `OpenAI:Model` apontando para o modelo disponível no ambiente
- `Analise:TamanhoMaximoBytes` somente quando o limite de arquivo tiver sido decidido

## Verificação sem base fiscal real

Os testes usam evidências fixas no dublê de conhecimento. Essa base não é legislação de produção.

Cenários mínimos do teste de integração:

1. Descrição com dado essencial e legislação no dublê devolve NCM dessa legislação, confiança 90 e XML vazio enquanto não houver layout registrado.
2. NCM declarada diferente da legislação bloqueia a nota e cita a fonte.
3. Item sem composição pede o dado e não bloqueia os demais.
4. Duas legislações da mesma vigência em conflito pedem revisão e não escolhem vencedor.
5. Caso corporativo como melhor fonte devolve sugestão, confiança 65 e revisão humana.
6. Arquivo enviado sem limite configurado é recusado sem estado de bloqueio fiscal.

## Consulta do resultado

`POST /analises` devolve o contrato `1.0.0`. O mesmo corpo é obtido em `GET /analises/{execucaoId}`.
