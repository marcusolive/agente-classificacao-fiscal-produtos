# Pesquisa: Base de conhecimento da Api.RAG

## 1. Onde o serviço mora

**Decisão**: Novo projeto web `src/Api.RAG`, na mesma solução, sem referência de projeto com a Api.Agente.

**Racional**: A constituição já nomeia Api.RAG como dona do vetor e do servidor MCP. Uma biblioteca compartilhada faria o agente enxergar o tipo interno da base.

**Alternativas rejeitadas**: Colocar a ingestão dentro da Api.Agente. Guardar o vetor no PostgreSQL da execução.

## 2. Pacotes conferidos em 2026-10-29

**Decisão**: Npgsql 10.0.3, já fixado. Pgvector 0.3.2, compatível com Npgsql 8 ou superior. ModelContextProtocol.AspNetCore 2.3.0, a mesma versão do cliente. PdfPig 0.1.16, última estável; a linha 0.1.17 ainda era pré-lançamento nesta data. XML usa `System.Xml`, sem pacote.

**Racional**: O cliente MCP da Api.Agente já chama HTTP e lê JSON de evidência. O servidor precisa falar o mesmo transporte. Entity Framework não entra: a Api.Agente já usa SQL direto.

**Alternativas rejeitadas**: Fixar a pré-versão do PdfPig. Escolher um modelo de embedding nesta pesquisa.

## 3. Transporte MCP

**Decisão**: `MapMcp("/mcp")` com transporte HTTP sem sessão. A ferramenta continua `consultar_conhecimento`, argumentos `texto` e `vigencia` em `yyyy-MM-dd`. O corpo de sucesso é uma lista JSON com fonte, faixa em camelCase, trecho, localização, vigência, NCM e tributos. Tributos usam as chaves que o agente já lê: `ipi`, `icms`, `cest`, `pis`, `cofins`, `ibsCbs`.

**Racional**: O desserializador da Api.Agente compara a vigência com a data pedida e ignora o que for diferente. A fonte precisa carregar a versão, porque o agente registra a base por esse nome.

**Alternativas rejeitadas**: Criar outra ferramenta. Incluir empresa no argumento. Devolver caso corporativo.

## 4. Busca ainda sem número

**Decisão**: Sem modelo, dimensão, topK ou limiar, a ferramenta responde erro. Lista vazia só aparece depois de uma busca que terminou e não achou chunk. A primeira migração não cria coluna `vector(N)` nem índice. Hash é SHA-256 do conteúdo. Sem tamanho de chunk configurado, o texto extraído inteiro é um chunk.

**Racional**: Lista vazia na ferramenta vira ausência de evidência no agente. Erro vira falha técnica, que é o comportamento já especificado para provedor indisponível. Dimensão inventada prenderia a migração num número não avaliado.

**Alternativas rejeitadas**: Busca por texto livre como substituto silencioso do vetor. Escolher HNSW ou IVFFlat agora.

## 5. Retentativa

**Decisão**: A chamada de embedding usa 2 tentativas adicionais e 60 segundos, em configuração.

**Racional**: A constituição exige teto antes da implementação. Esses dois números já foram aceitos para chamada externa de modelo. Não são parâmetro de qualidade da busca.

**Alternativas rejeitadas**: Retentativa ilimitada. Inventar outro teto sem dado novo.
