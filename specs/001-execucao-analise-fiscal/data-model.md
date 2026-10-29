# Modelo de dados: Execução da análise fiscal de itens

Os nomes abaixo são o vocabulário do contrato versão 1. Não descrevem layout de NF-e, planilha ou documento técnico.

## Execucao

Identificador gerado pelo sistema. Contexto obrigatório: usuário, empresa, papel, finalidade, UF, regime tributário, tipo de operação e data de vigência. Também guarda intenção, versão do agente, versões das bases consultadas e estado da nota.

Estado, nesta precedência: `bloqueada`, `pendenteDeDado`, `falhaTecnica`, `sugestaoLiberada`.

A execução pertence à empresa informada no contexto. Não há cenário multiorganização além desse vínculo.

## ItemAnalise

Pertence a uma execução. Guarda texto original, atributos normalizados, contexto efetivo do item e declaração fiscal quando existir.

Situação: `sugerido`, `divergente`, `dadoAusente`, `semEvidencia`, `falhaTecnica`.

Revisão humana é um marcador independente da situação. Confiança é inteiro de 0 a 100 ou desconhecida. Desconhecida não é zero.

Item sem composição, função ou apresentação fica em `dadoAusente`, sem NCM e sem divergência.

## Evidencia

Fonte, faixa de autoridade, trecho, localização quando existir, vigência e versão da base. Faixas, da maior para a menor: `legislacaoVigente`, `notasRgi`, `casoCorporativoConfirmado`, `textoFornecedor`.

Evidência é dado recuperado. Não altera regra do sistema.

## TratamentoSugerido

Um resultado por tributo: IPI, ICMS, CEST, PIS, COFINS e IBS/CBS. Cada um tem valor sustentado ou limitação. Valor sem evidência da vigência fica como limitação.

## Achado

Classe única: `erroEstrutural`, `inconsistenciaNegocio`, `indicioIa`, `ausenciaEvidencias`, `falhaTecnica`.

Severidade do contrato versão 1:

- `erro` para erro estrutural, inconsistência de negócio e falha técnica.
- `aviso` para indício da IA e ausência de evidências.

Inconsistência de negócio, neste contrato, é a divergência sustentada do item. Indício da IA não usa essa classe.

## Regras de consistência

- NCM sugerida ou usada no bloqueio precisa existir na evidência da vigência.
- XML do item só é preenchido quando houver layout de item registrado. Sem layout, o campo fica vazio e a validação não realizada é informada.
- O XML nunca representa a NF-e completa.
- Caso corporativo novo não é gravado.
- Impacto relevante permanece falso até existir definição com exemplos reais.

## Persistência

Tabelas no PostgreSQL da Api.Agente: `execucao`, `item_analise`, `evidencia_citada`, `achado`. O banco vetorial não participa deste modelo.
