# Quickstart: A2A da SOFIA

Na raiz do repositório:

```powershell
dotnet test tests/Api.Agente.Tests/Api.Agente.Tests.csproj
dotnet run --project src/Api.Agente --launch-profile Development
```

Development escuta `https://localhost:7216`. O Agent Card fica em `/.well-known/agent-card.json`. O JSON-RPC fica em `/a2a`. A chave da chamada é `Sofia__Chave`. Sem essa chave, `/a2a` responde 401.

A análise de itens continua em `POST /v1/analises`. O `texto` de cada documento da SOFIA é o JSON desse pedido. O `contexto` precisa trazer empresa, UF, regime, vigência, papel, finalidade e tipo de operação. Se faltar algum, a tarefa responde erro e não classifica. A intenção executada é `sugerirNcm`. CORS só passa a valer se `Cors:AllowedOrigins` tiver a origem do navegador.
