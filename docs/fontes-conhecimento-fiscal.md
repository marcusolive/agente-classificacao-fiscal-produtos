# Fontes oficiais de conhecimento fiscal (NCM e tratamento tributário)

Catálogo de fontes confiáveis para alimentar a base de conhecimento da **Api.RAG** e sustentar
as sugestões da **Api.Agente**. Serve de ponto de partida para a ingestão e para a especificação de
conhecimento prevista na constituição (DP-08 e DP-09).

> **Como este documento se encaixa no projeto**
>
> - A constituição proíbe inventar layout, regra fiscal, tabela, alíquota ou dispositivo legal
>   (seções 1, 6 e 9). Tudo isso **deve** vir de fonte oficial e ser registrado em especificação
>   antes da implementação. Este catálogo reúne essas fontes; ele **não** é a especificação em si.
> - A ingestão na Api.RAG exige, para cada documento, **origem, identificação e versão**
>   (constituição 3.4 / 4.3). As colunas abaixo já trazem esses três eixos.
> - As evidências são ordenadas pela **autoridade da fonte** (FR-010 da spec `001`):
>   **1) legislação vigente → 2) notas e Regras Gerais de Interpretação → 3) caso corporativo
>   confirmado → 4) texto do fornecedor.** As fontes estão agrupadas por essa mesma ordem.
> - Toda consulta é filtrada por **vigência** (FR-008/FR-009). Várias destas tabelas mudam com
>   frequência (a NCM do Classif é republicada todo dia). A ingestão **não deve** misturar versões
>   diferentes como se fossem a mesma vigência (constituição 4.3, spec `003`).

---

## Faixa 1 — Legislação e nomenclatura vigente (maior autoridade)

### 1.1 NCM — Nomenclatura Comum do Mercosul

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Estrutura oficial da NCM: 21 Seções, Capítulos, Posições, Subposições, Itens e Subitens, com Notas de Seção/Capítulo. Base para todo candidato de NCM (FR-011). |
| **Origem** | Receita Federal do Brasil |
| **Identificação / URL** | <https://www.gov.br/receitafederal/pt-br/assuntos/aduana-e-comercio-exterior/classificacao-fiscal-de-mercadorias/ncm> |
| **Versão / vigência** | Acompanha o SH e as resoluções Gecex/Camex. Conferir a versão vigente na data da análise. |

### 1.2 NCM + TEC — Resolução Gecex nº 272/2021 (texto que fixa a nomenclatura e a alíquota de II)

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Texto normativo que adota a NCM e a Tarifa Externa Comum (alíquota de Imposto de Importação por código). Inclui as Regras Gerais Interpretativas e as Notas. Atualizada por resoluções posteriores (ex.: 606/2024, 708/2025) — usar sempre a **versão compilada**. |
| **Origem** | MDIC / Camex / Gecex |
| **Identificação / URL** | Compilada: <https://www.gov.br/mdic/pt-br/assuntos/camex/resolucoes/gecex/resolucoes-compiladas/resolucao-gecex-no-272-de-19-de-novembro-de-2021-compilada> · Anexo I (NCM+TEC em PDF): <https://www.gov.br/mdic/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/Anexo_I_Res_272_2021.pdf> |
| **Versão / vigência** | Resolução Gecex 272/2021 e alterações — verificar a última alteração vigente. |

### 1.3 Classif / Portal Único Siscomex — NCM vigente legível por máquina

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Tabela NCM oficial vigente em **JSON e XLSX**, com inclusões, exclusões e mudanças de descrição. Ideal para o **validador determinístico** de "código existe na vigência" (Princípio 2 / FR-024), sem passar pelo LLM. |
| **Origem** | Receita Federal / Portal Único Siscomex |
| **Identificação / URL** | Consulta pública: <https://portalunico.siscomex.gov.br/classif/#/sumario?perfil=publico> · Integração/arquivo JSON: <https://www.gov.br/siscomex/pt-br/sistemas-de-comercio-exterior/integre-seu-sistema> · Manual do Classif: <https://www.gov.br/siscomex/pt-br/arquivos-e-imagens/2021/07/ClassifManualdoUsuariov5.0.pdf> |
| **Versão / vigência** | **Republicada todo dia à meia-noite.** Guardar a data de extração como vigência do chunk. |

### 1.4 TIPI — Tabela de Incidência do IPI

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Alíquota de IPI por NCM. Sustenta o tratamento de IPI (FR-012). |
| **Origem** | Presidência / Receita Federal |
| **Identificação / URL** | Decreto nº 11.158/2022 (Planalto): <https://www.planalto.gov.br/ccivil_03/_ato2019-2022/2022/decreto/d11158.htm> · Página da RFB sobre IPI/TIPI: <https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/tributos/ipi> |
| **Versão / vigência** | Decreto 11.158/2022 + Atos Declaratórios Executivos da RFB que ajustam a tabela às mudanças de NCM. Conferir o ADE mais recente. |

### 1.5 CEST — Convênio ICMS 142/2018 (substituição tributária)

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Código CEST (7 dígitos) e vínculo CEST × NCM/SH por segmento (Anexos II a XXVI). Sustenta o campo CEST e a coerência NCM×CEST (FR-012, FR-024). |
| **Origem** | CONFAZ |
| **Identificação / URL** | <https://www.confaz.fazenda.gov.br/legislacao/convenios/2018/CV142_18> |
| **Versão / vigência** | Convênio 142/2018 **consolidado** + alterações (ex.: Conv. ICMS 38/19, 39/19 e protocolos posteriores por anexo). Usar o texto consolidado vigente. |

### 1.6 PIS/COFINS — regimes especiais por NCM

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Base legal do monofásico/concentrado, alíquota zero e ST por NCM. Sustenta o tratamento de PIS/COFINS (FR-012). |
| **Origem** | Planalto (leis) + Receita Federal (tabelas da EFD-Contribuições) |
| **Identificação / URL** | Lei 10.147/2000 (medicamentos, higiene, perfumaria): <https://www.planalto.gov.br/ccivil_03/leis/l10147.htm> · Lei 10.485/2002 (autopeças/veículos): <https://www.planalto.gov.br/ccivil_03/leis/2002/l10485.htm> · Tabelas 4.3.x da EFD-Contribuições (SPED): <https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/declaracoes-e-demonstrativos/sped-sistema-publico-de-escrituracao-digital/escrituracao-fiscal-digital-contribuicoes> |
| **Versão / vigência** | As Tabelas 4.3.10 a 4.3.17 trazem itens monofásicos, alíquota zero e ST, com NCM de 2 a 8 dígitos. Verificar a versão vigente da tabela. |

### 1.7 IBS / CBS / Imposto Seletivo — Reforma Tributária

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Novo tratamento de IBS/CBS exigido pela constituição do projeto (seção 1, FR-012). Necessário para a vigência de transição a partir de 2026/2027. |
| **Origem** | Congresso / Planalto |
| **Identificação / URL** | EC nº 132/2023: <https://www.planalto.gov.br/ccivil_03/constituicao/emendas/emc/emc132.htm> · LC nº 214/2025 (institui IBS, CBS e IS): <https://www.planalto.gov.br/ccivil_03/leis/lcp/lcp214.htm> |
| **Versão / vigência** | LC 214/2025 já alterada por norma posterior (ex.: LC 227/2026). Usar a **versão consolidada** e atentar ao calendário de transição. |

---

## Faixa 2 — Notas explicativas e Regras Gerais de Interpretação (SH)

### 2.1 NESH — Notas Explicativas do Sistema Harmonizado

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Interpretação oficial internacional (OMA) para enquadrar mercadorias, aplicada de forma subsidiária às RGI. Fundamenta a regra/nota que sustenta cada candidato (FR-011). |
| **Origem** | Receita Federal (tradução oficial) / OMA |
| **Identificação / URL** | IN RFB nº 2.052/2021 (tradução das atualizações mais recentes): <https://normas.receita.economia.gov.br/> (buscar "IN RFB 2052/2021") · Base legal de adoção: Decreto nº 435/1992. |
| **Versão / vigência** | A tradução é atualizada por Instruções Normativas sucessivas (1.072/2010 e seguintes). Confirmar a IN vigente. |

### 2.2 RGI / RGC — Regras Gerais de Interpretação e Complementares

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | As 6 Regras Gerais do SH + Regras Complementares do Mercosul/TIPI que definem **como** escolher a posição. São parte integrante da NCM/TEC (ver 1.2) e da TIPI (ver 1.4). |
| **Origem** | Integram a Resolução Gecex 272/2021 e a TIPI |
| **Identificação / URL** | Texto nas próprias normas de 1.2 e 1.4. Interpretação administrativa: Parecer Normativo COSIT nº 6/2018. |
| **Versão / vigência** | Mesma vigência da NCM/TEC e TIPI adotadas. |

---

## Faixa 3 — Caso corporativo confirmado e jurisprudência administrativa

> Na ordem do FR-010, **caso corporativo confirmado** fica abaixo da legislação e das notas, e **acima**
> do texto do fornecedor. As fontes abaixo ajudam a formar e justificar esses casos — mas, no projeto,
> o caso corporativo só vira precedente depois da **confirmação por quem tem autoridade** (specs `002`/`003`),
> não por constar destas publicações.

### 3.1 Soluções de Consulta COSIT (RFB)

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Classificação fiscal já decidida pela RFB, com efeito vinculante para a administração tributária federal. Excelente material de apoio e referência técnica (FR-009, FR-020). |
| **Origem** | Receita Federal / COSIT |
| **Identificação / URL** | Base de normas e soluções: <https://normas.receita.economia.gov.br/> · Procedimento de consulta formal: IN RFB nº 2.057/2021. |
| **Versão / vigência** | Cada solução tem número e data próprios. Registrar número, data e ementa como identificação/versão do chunk. |

### 3.2 CARF — jurisprudência administrativa

| Eixo | Conteúdo |
| --- | --- |
| **O que agrega** | Acórdãos sobre classificação fiscal e tributação. Documentação técnica de apoio, **não** legislação vigente. |
| **Origem** | Conselho Administrativo de Recursos Fiscais |
| **Identificação / URL** | <https://carf.economia.gov.br/> |
| **Versão / vigência** | Por acórdão (número/câmara/data). |

---

## Notas de ingestão para a Api.RAG

1. **Três eixos obrigatórios.** Cada documento ingerido leva `origem`, `identificação` e `versão`
   (constituição 3.4). As tabelas acima já mapeiam esses campos.
2. **Vigência por chunk.** Guardar a data de publicação/extração para a consulta por data não
   misturar versões (spec `003`; ferramenta MCP `consultar_conhecimento` recebe `vigencia`).
3. **Legível por máquina vale ouro.** NCM (Classif, 1.3), CEST (1.5) e TIPI (1.4) existem em formatos
   estruturados (JSON/XLSX). Usá-los no **validador determinístico** de existência de código e de
   coerência NCM×CEST×tributos, fora do LLM (Princípio 2 / FR-024), não só como texto no RAG.
4. **Faixa de autoridade.** Marcar cada fonte com a faixa deste catálogo para a ordenação do FR-010.
   Empate dentro da mesma faixa pede revisão humana, não desempate inventado.
5. **Conteúdo é dado, não instrução.** Nenhum texto recuperado destas fontes altera regras do sistema,
   autorização ou contrato de saída (Princípio 5).
6. **O que falta decidir.** Layouts/schemas/XSD por tipo de documento (DP-08) e as regras fiscais
   concretas por tributo (DP-09) continuam em aberto; este catálogo é insumo para resolvê-los na
   especificação de conhecimento, não a decisão.

---

## Resumo rápido dos links oficiais

| Fonte | Link |
| --- | --- |
| NCM (RFB) | <https://www.gov.br/receitafederal/pt-br/assuntos/aduana-e-comercio-exterior/classificacao-fiscal-de-mercadorias/ncm> |
| NCM+TEC (Gecex 272/2021 compilada) | <https://www.gov.br/mdic/pt-br/assuntos/camex/resolucoes/gecex/resolucoes-compiladas/resolucao-gecex-no-272-de-19-de-novembro-de-2021-compilada> |
| NCM Classif (JSON/XLSX) | <https://portalunico.siscomex.gov.br/classif/#/sumario?perfil=publico> |
| TIPI (Decreto 11.158/2022) | <https://www.planalto.gov.br/ccivil_03/_ato2019-2022/2022/decreto/d11158.htm> |
| CEST (Conv. ICMS 142/2018) | <https://www.confaz.fazenda.gov.br/legislacao/convenios/2018/CV142_18> |
| PIS/COFINS (Lei 10.147/2000) | <https://www.planalto.gov.br/ccivil_03/leis/l10147.htm> |
| PIS/COFINS (Lei 10.485/2002) | <https://www.planalto.gov.br/ccivil_03/leis/2002/l10485.htm> |
| IBS/CBS (LC 214/2025) | <https://www.planalto.gov.br/ccivil_03/leis/lcp/lcp214.htm> |
| EC 132/2023 (Reforma) | <https://www.planalto.gov.br/ccivil_03/constituicao/emendas/emc/emc132.htm> |
| Normas e Soluções RFB | <https://normas.receita.economia.gov.br/> |
| CARF | <https://carf.economia.gov.br/> |
