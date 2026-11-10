# Quickstart: Execução da análise fiscal de itens

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL da Api.Agente, quando a execução for persistida fora dos testes
- Variável `OpenAI:Model` apontando para o modelo disponível no ambiente
- `Analise:TamanhoMaximoBytes` somente quando o limite de arquivo tiver sido decidido

## Verificação sem base fiscal real

Os testes usam evidências fixas no dublê de conhecimento. Essa base não é legislação de produção.

Resultado em 2026-10-29: os cenários abaixo passam em `dotnet test tests/Api.Agente.Tests/Api.Agente.Tests.csproj`.

1. `SugestaoTextoTests.Legislacao_vence_o_fornecedor_e_deixa_o_xml_vazio` — NCM da legislação, confiança 90 e XML vazio.
2. `BloqueioNotaTests.Ncm_declarada_diferente_bloqueia_a_nota` — NCM divergente bloqueia a nota.
3. `ItemIncompletoTests.Item_completo_segue_quando_o_vizinho_esta_incompleto` — item sem composição pede o dado e não bloqueia o vizinho.
4. `RevisaoHumanaTests.Empate_na_mesma_faixa_nao_escolhe_vencedor_nem_bloqueia` — duas legislações empatadas pedem revisão.
5. `RevisaoHumanaTests.Caso_confirmado_devolve_sugestao_65_e_revisao` — caso confirmado devolve sugestão, confiança 65 e revisão.
6. `DocumentoSemLayoutTests.Arquivo_sem_limite_configurado_e_recusado_sem_bloqueio` — arquivo sem limite configurado é recusado sem bloqueio fiscal.

## Consulta e documentação

`POST /v1/analises` devolve o contrato `1.0.0`. O mesmo corpo é obtido em `GET /v1/analises/{execucaoId}`. A interface fica em `/swagger` e o documento em `GET /openapi/v1.json`. O arquivo `contracts/analise-v1.openapi.yaml` é gerado desse documento. `ContratoGeradoTests` recusa divergência entre os dois.
