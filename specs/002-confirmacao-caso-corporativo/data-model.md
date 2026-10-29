# Data Model: Confirmação do caso corporativo

Nenhuma entidade é persistida. Os objetos abaixo existem só no contrato da resposta.

## PedidoConfirmacao

- `papelAutoridade`: texto obrigatório. Não há catálogo.
- `itens`: ao menos um item da execução.

## ItemPedidoConfirmacao

- `itemId`: identificador do item na execução.
- `decisao`: `aceitar` ou `corrigir`.
- `ncm`: obrigatória apenas em `corrigir`.
- `tributos`: opcional. Quando informado, cada valor precisa existir nas evidências do item. Quando omitido, a confirmação mantém os tratamentos já sustentados.

## ResultadoConfirmacao

- `contratoVersao`: `1.1.0`.
- `execucaoId`
- `empresa`: copiada da execução.
- `papelAutoridade`
- `gravado`: sempre falso nesta specification.
- `validacoesNaoRealizadas`: inclui "gravação na base de conhecimento não realizada".
- `itens`: NCM confirmada e tratamentos aceitos.

## ItemConfirmado

- `itemId`
- `decisao`
- `ncm`
- `tributos`
