# Modelo de dados: Base de conhecimento da Api.RAG

Nomes da base da Api.RAG. Não descrevem layout de NF-e nem schema oficial de nota.

## Documento

Identificador gerado pelo sistema. Origem, identificação e versão formam a identidade de negócio. Também guarda faixa, vigência inicial, vigência final quando existir, formato, hash SHA-256, estado e a fonte `origem/identificacao/versao`.

Estado: `gravado`, `ignorado`, `recusado`, `falhaTecnica`. Ignorado é a repetição do mesmo hash. Recusado não grava linha nova. Falha técnica não deixa a versão consultável.

A versão só fica consultável quando cada chunk dela tem embedding e os parâmetros de busca estão configurados.

Faixas: `legislacaoVigente`, `notasRgi`, `casoCorporativoConfirmado`, `textoFornecedor`.

Formatos: `texto`, `pdf`, `xmlNotas`.

## Chunk

Pertence a um documento. Guarda o texto, a posição na sequência, a localização quando existir e o hash do documento. A localização de PDF é a página, quando a extração a informar. A coluna de embedding fica fora da primeira migração.

Dois chunks de versões diferentes não compartilham identidade. A consulta por uma data seleciona o documento cujo intervalo contém a data e devolve cada chunk com a fonte da própria versão.

## Caso confirmado

É um documento da faixa `casoCorporativoConfirmado`. Além dos campos do documento, guarda decisão, papel da autoridade, data da confirmação, empresa e execução de origem. Sem qualquer um desses, a ingestão é recusada e a linha não nasce.

A ferramenta de consulta exclui essa faixa. A administração devolve o caso com esses vínculos.

## Evidência devolvida

Não é tabela. É o JSON da ferramenta: fonte versionada, faixa, trecho, localização, a data pedida na vigência, NCM e tributos somente se tiverem sido informados na ingestão. NCM e tributo não são lidos do texto por regra da Api.RAG.

## Fora deste modelo

Dimensão do vetor, métrica, índice, topK e limiar. Eles não têm coluna nem parâmetro padrão até a avaliação da DP-12.
