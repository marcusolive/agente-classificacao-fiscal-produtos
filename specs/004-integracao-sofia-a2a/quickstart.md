# Quickstart: A2A da SOFIA

Na raiz do repositório:

```powershell
dotnet test tests/Api.Agente.Tests/Api.Agente.Tests.csproj
dotnet run --project src/Api.Agente --launch-profile Development
```

Development escuta `https://localhost:7216`. O Agent Card fica em `/.well-known/agent-card.json`. O JSON-RPC fica em `/a2a`. A chave da chamada é `Sofia__Chave`. Sem essa chave, `/a2a` responde 401.

A análise de itens continua em `POST /v1/analises`. CORS só passa a valer se `Cors:AllowedOrigins` tiver a origem do navegador.
