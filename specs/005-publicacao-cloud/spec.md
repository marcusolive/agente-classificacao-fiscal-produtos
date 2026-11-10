# Specification: Publicação experimental na nuvem gratuita

**Feature Branch**: `005-publicacao-cloud`  
**Created**: 2026-11-10  
**Status**: Especificada  
**Input**: Skill `sofia-devops-free-cloud`.

## Escopo

A publicação experimental usa o que já existe. A Api.Agente e a Api.RAG continuam separadas. Os contratos REST e A2A permanecem. O Compose local permanece. Esta feature não cria conta, não faz deploy e não grava segredo.

## Compatibilidade

| Item | Resultado | Motivo |
|---|---|---|
| Api.Agente .NET 10, DLL `Api.Agente.dll` | Adaptação | A imagem da Vercel escuta `PORT`. O Dockerfile local continua na porta 8080. |
| Api.RAG .NET 10, DLL `Api.RAG.dll` | Adaptação | Segunda imagem. A Vercel detecta só `Dockerfile.vercel` na raiz. |
| Postgres da Api.Agente e da Api.RAG | Adaptação | Neon Free, duas bases, `SSL Mode=Require`. A extensão `vector` só na base da RAG. |
| Busca exata por cosseno | OK | Sem índice HNSW ou IVFFlat. |
| MongoDB Atlas | Bloqueio | A DP-01 não escolheu o armazenamento de memória. A aplicação não lê connection string de Mongo. |
| Estado em memória da `idSolicitacao` | Adaptação | A Function desliga sem tráfego. A repetição não sobrevive a esse desligamento. A execução gravada no Postgres permanece. |
| Swagger aberto | Risco | Produção ainda publica `/swagger` sem autenticação. Fora desta feature. |
| Hobby da Vercel | Risco | O plano é pessoal e não comercial. Uso institucional pode exigir outro plano. |

## Requisitos

- **FR-001**: `Dockerfile.vercel` publica a Api.Agente e escuta `0.0.0.0` na variável `PORT`, com 8080 só como reserva local.
- **FR-002**: `Dockerfile.rag.vercel` publica a Api.RAG do mesmo modo. O Dockerfile de cada projeto, já existente, não muda.
- **FR-003**: `.env.example` lista as chaves que o `IConfiguration` já lê, sem valores. `ConnectionStrings__Agente` e `ConnectionStrings__Rag` são os nomes reais.
- **FR-004**: Segredo não entra em Git, Dockerfile, appsettings versionado nem log.
- **FR-005**: A extensão `vector` continua a ser criada pela subida da Api.RAG. Esta feature não cria tabela de demonstração nem índice HNSW.
- **FR-006**: Nenhum deploy é declarado concluído sem URL pública, log e teste feito pelo operador.

## Fora deste escopo

Conta na Vercel, projeto no Neon, chave da OpenAI, ingestão de documento oficial, saúde `/health`, desligar o Swagger e memória no Atlas.
