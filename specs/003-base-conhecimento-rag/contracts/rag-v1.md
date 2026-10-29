# Contrato v1 da Api.RAG

Contrato de comportamento. O YAML OpenAPI será a serialização de `/openapi/v1.json` quando o projeto existir. A versão do documento começa em 1.0.0. Este contrato não altera o documento da Api.Agente.

## Administração

`POST /v1/documentos` exige o cabeçalho `X-Chave-Ingestao` igual a `Ingestao:Chave`. Chave vazia na configuração ou cabeçalho diferente recusa antes de gravar.

Corpo mínimo: origem, identificação, versão, faixa e vigência inicial. Vigência final é opcional. Texto puro vai no campo de conteúdo. PDF e XML de notas vão como arquivo. NCM e tributos são opcionais e só são copiados para a evidência quando vierem neste corpo.

Caso corporativo usa a faixa `casoCorporativoConfirmado` e ainda exige decisão, papel, data da confirmação, empresa e execução de origem.

Resposta: identificador, fonte, estado e se a versão está consultável. Repetição do mesmo hash responde `ignorado` e não cria chunk. Conteúdo diferente na mesma versão é recusa e a versão anterior permanece.

`GET /v1/documentos/{documentoId}` devolve o documento e os chunks, com texto, localização e fonte. Inclui o caso confirmado.

`GET /v1/documentos` filtra por origem e identificação e lista as versões, sem misturar o texto delas numa só vigência.

A administração não exige os parâmetros de embedding.

## Ferramenta MCP

Nome: `consultar_conhecimento`.

Endereço local: `/mcp`.

Argumentos: `texto` e `vigencia`.

Sucesso: lista JSON. Cada item tem `fonte`, `faixaAutoridade`, `trecho`, `localizacao`, `vigencia`, `ncm` e `tributos`. `vigencia` é a data pedida. `fonte` é `origem/identificacao/versao`. A faixa de caso corporativo não entra.

Lista vazia: a busca terminou e nenhum chunk daquela data serviu ao texto.

Erro da ferramenta: parâmetro de busca ausente, provedor de embedding indisponível ou banco indisponível. Esse erro não vem acompanhado de lista parcial.

## O que este contrato não faz

Não cria XSD. Não devolve custo. Não autentica por papel. Não oferece ferramenta de gravação ao agente.
