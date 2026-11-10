# Carregador da NCM → Api.RAG

Script operacional que ingere a tabela NCM do Classif/Siscomex na base de
conhecimento da **Api.RAG**, um documento por código, pela rota pública
`POST /v1/documentos`.

Não é projeto da solução (constituição §3.1 / DP-16). Alimenta o RAG **só** pela
API REST de administração, respeitando a fronteira do Princípio 8: nada toca o
PostgreSQL da Api.RAG diretamente.

## O que ele monta

Cada registro da NCM vira um documento da faixa de maior autoridade (`legislacaoVigente`, FR-010):

| Campo do RAG | Valor |
| --- | --- |
| `origem` | `Classif/Siscomex` (parâmetro `-Origem`) |
| `identificacao` | o código (ex.: `0101.21.00`) |
| `versao` | o ato do registro (ex.: `Res Gecex 272/2021`) |
| `faixa` | `legislacaoVigente` |
| `vigenciaInicial` | `Data_Inicio` do registro |
| `vigenciaFinal` | `Data_Fim`, ou vazio quando `31/12/9999` (intervalo aberto) |
| `formato` | `texto` |
| `conteudo` | `"<Codigo> — <Descricao>"` |
| `ncm` | o código |

## Pré-requisitos

1. **Api.RAG no ar** (local ou nuvem). Local: `dotnet run` (perfil Development →
   `http://localhost:5120`) ou o docker do quickstart (`http://localhost:8081`).
2. **Chave de ingestão configurada** na Api.RAG (`Ingestao:Chave`) e passada ao
   script em `-Chave` ou `$env:INGESTAO_CHAVE`. Chave vazia → a API recusa tudo.
3. Para o documento ficar **consultável** (não só gravado), a Api.RAG precisa de
   `OpenAI:ApiKey` + `Embedding:Model/Dimensao/TopK/Limiar`. Sem isso, o documento
   grava mas volta `consultavel=false`. **Gerar embedding tem custo na OpenAI.**

## Uso

```powershell
# 1) Teste sem enviar nada (monta e mostra 3 payloads):
./Invoke-CargaNcm.ps1 -DryRun -SomenteOitoDigitos -Limite 3

# 2) Carga pequena de validação (10 códigos) contra o RAG local:
$env:INGESTAO_CHAVE = 'ingestao-local'
./Invoke-CargaNcm.ps1 -BaseUrl http://localhost:5120 -SomenteOitoDigitos -Limite 10

# 3) Carga completa dos 8 dígitos:
./Invoke-CargaNcm.ps1 -BaseUrl http://localhost:5120 -Chave 'ingestao-local' -SomenteOitoDigitos
```

### Parâmetros

| Parâmetro | Padrão | Função |
| --- | --- | --- |
| `-Arquivo` | `data/ncm/ncm-classif-2026-11-10.json` | JSON da NCM |
| `-BaseUrl` | `http://localhost:5120` | URL da Api.RAG |
| `-Chave` | `$env:INGESTAO_CHAVE` | cabeçalho `X-Chave-Ingestao` |
| `-Origem` | `Classif/Siscomex` | origem gravada |
| `-SomenteVigentes` | ligado | só `Data_Fim = 31/12/9999` |
| `-SomenteOitoDigitos` | desligado | só NCM completa (8 dígitos) |
| `-Limite` | `0` (todos) | máximo de registros |
| `-PausaMs` | `0` | pausa entre requisições (vazão/custo) |
| `-DryRun` | — | monta e mostra, não envia |

## Escala e custo

- O download diário do Classif já traz **apenas códigos vigentes**: nesta versão são
  **15.157** registros em todos os níveis e **10.516** com 8 dígitos. Por isso
  `-SomenteVigentes` não corta nada neste arquivo.
- Carga completa = um `POST` por registro. Com a busca configurada, é **um embedding
  por documento** — 10.516 chamadas à OpenAI para os 8 dígitos. Comece com `-Limite`
  pequeno e `-DryRun` antes da carga cheia.

## Idempotência e reexecução

- Reenviar o mesmo conteúdo responde `ignorado` e **não** cria chunk novo (FR-007).
  Rodar o script de novo é seguro.
- Quando a NCM mudar na fonte, baixe o novo JSON, aponte `-Arquivo` para ele e rode
  de novo: códigos iguais ficam `ignorado`; descrições alteradas sob a **mesma**
  `versao` são **recusadas** (exigem versão nova) — confira o resumo ao final.

## Decisões em aberto que afetam a qualidade

- `Embedding:TamanhoChunk`/`Sobreposicao` vazios ⇒ cada documento é **1 chunk**. Aqui
  cada código é curto, então 1 chunk por código é adequado. Isso **não** resolve a
  DP-12 para documentos longos (NESH, leis) — esses ainda precisam de estratégia de
  chunk avaliada com exemplos.
- `Dimensao/TopK/Limiar` são escolha provisória (DP-12). Reavaliar com exemplos reais
  de classificação antes de tratar como padrão de produção.
