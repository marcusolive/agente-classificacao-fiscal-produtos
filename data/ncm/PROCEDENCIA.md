# Procedência — Tabela NCM (Classif / Portal Único Siscomex)

Registro de origem, identificação e versão do arquivo, conforme exigido pela constituição
(seções 3.4 e 4.3) e pela spec `003-base-conhecimento-rag` para ingestão na Api.RAG.

| Eixo | Valor |
| --- | --- |
| **Arquivo** | `ncm-classif-2026-11-10.json` |
| **Origem** | Receita Federal / Portal Único Siscomex — módulo Classif (acesso público) |
| **Endpoint** | `https://portalunico.siscomex.gov.br/classif/api/publico/nomenclatura/download/json` |
| **Baixado em** | 2026-11-10 |
| **Vigência declarada no arquivo** (`Data_Ultima_Atualizacao_NCM`) | `Vigente em 10/11/2026` |
| **Ato normativo** (`Ato`) | Resolução Gecex nº 926/2026 |
| **Formato** | JSON (texto plano), UTF-8, ~3,1 MB |
| **Atualização na fonte** | Republicado todo dia à meia-noite (inclusões, exclusões e mudanças de descrição) |

## Estrutura

Objeto raiz com três chaves:

- `Data_Ultima_Atualizacao_NCM` — string com a vigência da tabela.
- `Ato` — ato normativo que sustenta a versão.
- `Nomenclaturas` — lista com **15.157 registros** (todos os níveis hierárquicos).

Cada registro de `Nomenclaturas`:

| Campo | Significado |
| --- | --- |
| `Codigo` | Código com pontuação (ex.: `0101.21.00`). Níveis de 2 a 8 dígitos. |
| `Descricao` | Texto oficial da posição/subposição/item. |
| `Data_Inicio` | Início da vigência do código (`dd/MM/yyyy`). |
| `Data_Fim` | Fim da vigência; `31/12/9999` = vigente. |
| `Tipo_Ato_Ini` / `Numero_Ato_Ini` / `Ano_Ato_Ini` | Ato que instituiu o código. |

## Contagem por nível (nesta versão)

| Dígitos | Registros |
| --- | --- |
| 2 (Capítulo) | 96 |
| 4 (Posição) | 957 |
| 5 | 1.024 |
| 6 (Subposição) | 1.661 |
| 7 | 903 |
| **8 (NCM completa)** | **10.516** (todos vigentes) |

## Uso no projeto

- **Validador determinístico** (Princípio 2 / FR-024): verificar se o código sugerido existe e está
  vigente na data da análise comparando `Data_Inicio ≤ vigência ≤ Data_Fim`. Isso fica **fora do LLM**.
- **RAG** (spec `003`): cada registro vira evidência com origem = "Classif/Siscomex",
  identificação = `Codigo`, versão/vigência = o par `Data_Inicio`/`Data_Fim` + o `Ato`. Guardar a
  data de extração (2026-11-10) para a consulta por data não misturar versões.
- **Reingestão**: baixar de novo do endpoint e comparar o hash; o mesmo conteúdo é ignorado e
  conteúdo diferente na mesma vigência é recusado (regra da spec `003`).

> Dado oficial vigente — **não** é base simulada. Pode ser usado em desenvolvimento e produção
> (constituição 5). Rebaixar quando a `Data_Ultima_Atualizacao_NCM` mudar.
